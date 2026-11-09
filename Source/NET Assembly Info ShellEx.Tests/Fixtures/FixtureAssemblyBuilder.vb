
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Reflection.Metadata.Ecma335
Imports System.Reflection.PortableExecutable

#End Region

#Region " FixtureAssemblyBuilder "

Namespace Fixtures

    ''' <summary>
    ''' Emits small, valid, synthetic PE/ECMA-335 assemblies with precisely controlled headers,
    ''' flags, attributes and debug entries, so the tests do not depend on binary files in the repository.
    ''' </summary>
    Friend NotInheritable Class FixtureAssemblyBuilder

#Region " Constants "

        ''' <summary>The serialized type name of <c>SecurityPermissionAttribute</c>, as written by the .NET Framework compilers.</summary>
        Friend Const SecurityPermissionAttributeSerializedName As String =
            "System.Security.Permissions.SecurityPermissionAttribute, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"

        ''' <summary>The serialized type name of <c>SecurityPermissionFlag</c>.</summary>
        Friend Const SecurityPermissionFlagSerializedName As String =
            "System.Security.Permissions.SecurityPermissionFlag, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"

        ''' <summary>The offset of the <c>ManagedNativeHeader</c> data directory inside the CLI (COR20) header.</summary>
        Private Const ManagedNativeHeaderOffset As Integer = 64

        ''' <summary>The index of the CLI header in the PE optional header data directories.</summary>
        Private Const CliHeaderDataDirectoryIndex As Integer = 14

#End Region

#Region " Constructors "

        ''' <summary>Prevents a default instance of the <see cref="FixtureAssemblyBuilder"/> class from being created.</summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Emits a fixture assembly to a temporary file.
        ''' </summary>
        ''' <param name="options">
        ''' The fixture options.
        ''' </param>
        ''' <returns>
        ''' The temporary fixture file. Dispose it to delete the file.
        ''' </returns>
        Friend Shared Function Create(options As FixtureOptions) As FixtureFile

            Dim extension As String = If(options.ImageKind = FixtureImageKind.Dll, ".dll", ".exe")
            Return New FixtureFile(FixtureAssemblyBuilder.Build(options), extension)
        End Function

        ''' <summary>
        ''' Emits a fixture assembly.
        ''' </summary>
        ''' <param name="options">
        ''' The fixture options.
        ''' </param>
        ''' <returns>
        ''' The PE image bytes.
        ''' </returns>
        Friend Shared Function Build(options As FixtureOptions) As Byte()

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            Dim metadata As New MetadataBuilder()
            Dim ilStream As New BlobBuilder()
            Dim methodBodies As New MethodBodyStreamEncoder(ilStream)

            ' Module and assembly manifest.
            metadata.AddModule(0, metadata.GetOrAddString($"{options.AssemblyName}.dll"), metadata.GetOrAddGuid(options.Mvid), Nothing, Nothing)

            Dim assemblyHandle As AssemblyDefinitionHandle = Nothing
            If Not options.OmitAssemblyManifest Then
                assemblyHandle = metadata.AddAssembly(
                    metadata.GetOrAddString(options.AssemblyName),
                    options.Version,
                    If(String.IsNullOrEmpty(options.CultureName), Nothing, metadata.GetOrAddString(options.CultureName)),
                    If(options.PublicKey Is Nothing, Nothing, metadata.GetOrAddBlob(options.PublicKey)),
                    If(options.PublicKey Is Nothing, CType(0, AssemblyFlags), AssemblyFlags.PublicKey),
                    AssemblyHashAlgorithm.Sha1)
            End If

            ' Assembly references.
            Dim coreLibrary As AssemblyReferenceHandle = FixtureAssemblyBuilder.AddCoreLibraryReference(metadata, options.CoreLibrary)
            For Each reference As Tuple(Of String, Version, Byte()) In options.AssemblyReferences
                Dim flags As AssemblyFlags = Nothing
                options.AssemblyReferenceFlags.TryGetValue(reference.Item1, flags)
                metadata.AddAssemblyReference(
                    metadata.GetOrAddString(reference.Item1), reference.Item2, Nothing,
                    If(reference.Item3 Is Nothing, Nothing, metadata.GetOrAddBlob(reference.Item3)),
                    flags, Nothing)
            Next reference

            Dim typeReferences As New Dictionary(Of String, TypeReferenceHandle)(StringComparer.Ordinal)

            ' Assembly attributes and declarative security.
            If (options.Attributes.Count > 0) OrElse (options.DeclarativeSecurity.Count > 0) Then
                If assemblyHandle.IsNil Then
                    Throw New InvalidOperationException("Assembly attributes require an assembly manifest.")
                End If
            End If

            For Each attribute As FixtureAttribute In options.Attributes
                FixtureAssemblyBuilder.AddCustomAttribute(metadata, assemblyHandle, coreLibrary, typeReferences, attribute)
            Next attribute

            For Each security As Tuple(Of DeclarativeSecurityAction, Byte()) In options.DeclarativeSecurity
                metadata.AddDeclarativeSecurityAttribute(assemblyHandle, security.Item1, metadata.GetOrAddBlob(security.Item2))
            Next security

            ' Native module references (P/Invoke targets).
            For Each moduleName As String In options.NativeModules
                metadata.AddModuleReference(metadata.GetOrAddString(moduleName))
            Next moduleName

            ' Types (and the entry-point method).
            Dim entryPoint As MethodDefinitionHandle = FixtureAssemblyBuilder.AddTypes(metadata, methodBodies, coreLibrary, typeReferences, options)

            ' PE image.
            Dim characteristics As Characteristics = Characteristics.ExecutableImage Or Characteristics.LargeAddressAware
            If options.ImageKind = FixtureImageKind.Dll Then
                characteristics = characteristics Or Characteristics.Dll
            End If

            Dim header As New PEHeaderBuilder(
                machine:=options.Machine,
                imageCharacteristics:=characteristics,
                subsystem:=If(options.ImageKind = FixtureImageKind.WindowsExe, Subsystem.WindowsGui, Subsystem.WindowsCui))

            Dim debugDirectory As New DebugDirectoryBuilder()
            If options.PdbPath IsNot Nothing Then
                debugDirectory.AddCodeViewEntry(options.PdbPath, New BlobContentId(Guid.NewGuid(), 1UI), If(options.PortablePdb, CUShort(&H100), CUShort(0)))
            End If
            If options.Deterministic Then
                debugDirectory.AddReproducibleEntry()
            End If
            If options.EmbeddedPdb Then
                Dim pdbContent As New BlobBuilder()
                pdbContent.WriteBytes(New Byte() {&H42, &H53, &H4A, &H42}) ' "BSJB" (content is irrelevant for these tests)
                debugDirectory.AddEmbeddedPortablePdbEntry(pdbContent, &H100)
            End If

            Dim mvid As Guid = options.Mvid
            Dim timeDateStamp As UInteger = options.TimeDateStamp
            Dim idProvider As Func(Of IEnumerable(Of Blob), BlobContentId) =
                Function(content As IEnumerable(Of Blob)) New BlobContentId(mvid, timeDateStamp)

            Dim peBuilder As New ManagedPEBuilder(
                header:=header,
                metadataRootBuilder:=New MetadataRootBuilder(metadata, options.MetadataVersion),
                ilStream:=ilStream,
                debugDirectoryBuilder:=debugDirectory,
                nativeResources:=If(options.Win32Manifest Is Nothing, Nothing, New ManifestResourceSectionBuilder(24, options.Win32ManifestId, options.Win32Manifest)),
                strongNameSignatureSize:=If(options.PublicKey Is Nothing, 0, 128),
                entryPoint:=entryPoint,
                flags:=options.CorFlags,
                deterministicIdProvider:=idProvider)

            Dim peBlob As New BlobBuilder()
            peBuilder.Serialize(peBlob)
            Dim image As Byte() = peBlob.ToArray()

            FixtureAssemblyBuilder.ApplyPatches(image, options)
            Return image
        End Function

        ''' <summary>
        ''' Creates a binary (.NET 2.0+) permission set blob with a single security attribute.
        ''' </summary>
        ''' <param name="attributeSerializedName">
        ''' The serialized (assembly-qualified) name of the security attribute type.
        ''' </param>
        '''
        ''' <param name="namedArguments">
        ''' The named (property) arguments. Enum arguments are serialized as <c>SecurityPermissionFlag</c>.
        ''' </param>
        ''' <returns>
        ''' The permission set blob.
        ''' </returns>
        Friend Shared Function CreateBinaryPermissionSet(attributeSerializedName As String, ParamArray namedArguments As KeyValuePair(Of String, FixtureArgument)()) As Byte()

            Dim arguments As New BlobBuilder()
            arguments.WriteCompressedInteger(namedArguments.Length)
            For Each pair As KeyValuePair(Of String, FixtureArgument) In namedArguments
                arguments.WriteByte(&H54) ' PROPERTY
                FixtureAssemblyBuilder.WriteNamedArgumentType(arguments, pair.Value)
                arguments.WriteSerializedString(pair.Key)
                FixtureAssemblyBuilder.WriteArgumentValue(arguments, pair.Value)
            Next pair

            Dim blob As New BlobBuilder()
            blob.WriteByte(&H2E) ' '.'
            blob.WriteCompressedInteger(1)
            blob.WriteSerializedString(attributeSerializedName)
            blob.WriteCompressedInteger(arguments.Count)
            blob.LinkSuffix(arguments)
            Return blob.ToArray()
        End Function

        ''' <summary>
        ''' Converts an hexadecimal string to a byte array.
        ''' </summary>
        ''' <param name="hex">
        ''' The hexadecimal string.
        ''' </param>
        ''' <returns>
        ''' The byte array.
        ''' </returns>
        Friend Shared Function HexToBytes(hex As String) As Byte()

            Dim bytes As Byte() = New Byte((hex.Length \ 2) - 1) {}
            For i As Integer = 0 To bytes.Length - 1
                bytes(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
            Next i
            Return bytes
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Adds the reference to the specified core library.
        ''' </summary>
        Private Shared Function AddCoreLibraryReference(metadata As MetadataBuilder, coreLibrary As FixtureCoreLibrary) As AssemblyReferenceHandle

            Dim name As String
            Dim version As Version
            Dim token As String

            Select Case coreLibrary
                Case FixtureCoreLibrary.None
                    Return Nothing
                Case FixtureCoreLibrary.Mscorlib
                    name = "mscorlib" : version = New Version(4, 0, 0, 0) : token = "b77a5c561934e089"
                Case FixtureCoreLibrary.SystemRuntime8
                    name = "System.Runtime" : version = New Version(8, 0, 0, 0) : token = "b03f5f7f11d50a3a"
                Case FixtureCoreLibrary.Netstandard20
                    name = "netstandard" : version = New Version(2, 0, 0, 0) : token = "cc7b13ffcd2ddd51"
                Case FixtureCoreLibrary.SystemRuntimeLegacy
                    name = "System.Runtime" : version = New Version(4, 0, 10, 0) : token = "b03f5f7f11d50a3a"
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(coreLibrary))
            End Select

            Return metadata.AddAssemblyReference(
                metadata.GetOrAddString(name), version, Nothing,
                metadata.GetOrAddBlob(FixtureAssemblyBuilder.HexToBytes(token)), CType(0, AssemblyFlags), Nothing)
        End Function

        ''' <summary>
        ''' Gets or adds a (possibly nested) type reference resolved by the core library.
        ''' </summary>
        Private Shared Function GetOrAddTypeReference(metadata As MetadataBuilder, coreLibrary As AssemblyReferenceHandle,
                                                      cache As Dictionary(Of String, TypeReferenceHandle), fullName As String) As TypeReferenceHandle

            If coreLibrary.IsNil Then
                Throw New InvalidOperationException("Type references require a core library reference.")
            End If

            ' Nested types ("Namespace.Outer+Inner") are resolved iteratively, from the outermost type.
            Dim parts As String() = fullName.Split("+"c)
            Dim currentName As String = String.Empty
            Dim scope As EntityHandle = coreLibrary
            Dim handle As TypeReferenceHandle = Nothing

            For i As Integer = 0 To parts.Length - 1
                currentName = If(i = 0, parts(0), $"{currentName}+{parts(i)}")
                If Not cache.TryGetValue(currentName, handle) Then
                    Dim namespaceName As String = String.Empty
                    Dim typeName As String = parts(i)
                    If i = 0 Then
                        Dim dotIndex As Integer = typeName.LastIndexOf("."c)
                        If dotIndex <> -1 Then
                            namespaceName = typeName.Substring(0, dotIndex)
                            typeName = typeName.Substring(dotIndex + 1)
                        End If
                    End If
                    handle = metadata.AddTypeReference(scope, metadata.GetOrAddString(namespaceName), metadata.GetOrAddString(typeName))
                    cache.Add(currentName, handle)
                End If
                scope = handle
            Next i

            Return handle
        End Function

        ''' <summary>
        ''' Adds an assembly-level custom attribute.
        ''' </summary>
        Private Shared Sub AddCustomAttribute(metadata As MetadataBuilder, parent As EntityHandle, coreLibrary As AssemblyReferenceHandle,
                                              cache As Dictionary(Of String, TypeReferenceHandle), attribute As FixtureAttribute)

            Dim attributeType As TypeReferenceHandle = FixtureAssemblyBuilder.GetOrAddTypeReference(metadata, coreLibrary, cache, attribute.TypeFullName)

            ' Constructor signature.
            Dim parameterTypes As New List(Of EntityHandle)
            For Each argument As FixtureArgument In attribute.FixedArguments
                If argument.Kind = FixtureArgumentKind.Enum Then
                    parameterTypes.Add(FixtureAssemblyBuilder.GetOrAddTypeReference(metadata, coreLibrary, cache, argument.EnumTypeFullName))
                Else
                    parameterTypes.Add(Nothing)
                End If
            Next argument

            Dim signature As New BlobBuilder()
            Dim fixedArguments As IList(Of FixtureArgument) = attribute.FixedArguments
            Dim signatureEncoder As New BlobEncoder(signature)
            signatureEncoder.MethodSignature(isInstanceMethod:=True).Parameters(
                fixedArguments.Count,
                Sub(returnType As ReturnTypeEncoder) returnType.Void(),
                Sub(parameters As ParametersEncoder)
                    For i As Integer = 0 To fixedArguments.Count - 1
                        Dim type As SignatureTypeEncoder = parameters.AddParameter().Type()
                        Select Case fixedArguments(i).Kind
                            Case FixtureArgumentKind.String : type.String()
                            Case FixtureArgumentKind.Boolean : type.Boolean()
                            Case FixtureArgumentKind.Int32 : type.Int32()
                            Case FixtureArgumentKind.Enum : type.Type(parameterTypes(i), isValueType:=True)
                        End Select
                    Next i
                End Sub)

            Dim constructor As MemberReferenceHandle = metadata.AddMemberReference(attributeType, metadata.GetOrAddString(".ctor"), metadata.GetOrAddBlob(signature))

            ' Value blob (ECMA-335 §II.23.3).
            Dim value As New BlobBuilder()
            value.WriteUInt16(1) ' Prolog.
            For Each argument As FixtureArgument In attribute.FixedArguments
                FixtureAssemblyBuilder.WriteArgumentValue(value, argument)
            Next argument

            value.WriteUInt16(CUShort(attribute.NamedArguments.Count))
            For Each pair As KeyValuePair(Of String, FixtureArgument) In attribute.NamedArguments
                value.WriteByte(&H54) ' PROPERTY
                FixtureAssemblyBuilder.WriteNamedArgumentType(value, pair.Value)
                value.WriteSerializedString(pair.Key)
                FixtureAssemblyBuilder.WriteArgumentValue(value, pair.Value)
            Next pair

            metadata.AddCustomAttribute(parent, constructor, metadata.GetOrAddBlob(value))
        End Sub

        ''' <summary>
        ''' Writes the serialization type of a named argument.
        ''' </summary>
        Private Shared Sub WriteNamedArgumentType(blob As BlobBuilder, argument As FixtureArgument)

            Select Case argument.Kind
                Case FixtureArgumentKind.String
                    blob.WriteByte(&HE)
                Case FixtureArgumentKind.Boolean
                    blob.WriteByte(&H2)
                Case FixtureArgumentKind.Int32
                    blob.WriteByte(&H8)
                Case FixtureArgumentKind.Enum
                    blob.WriteByte(&H55)
                    blob.WriteSerializedString(argument.EnumTypeFullName)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(argument))
            End Select
        End Sub

        ''' <summary>
        ''' Writes the serialized value of an argument.
        ''' </summary>
        Private Shared Sub WriteArgumentValue(blob As BlobBuilder, argument As FixtureArgument)

            Select Case argument.Kind
                Case FixtureArgumentKind.String
                    blob.WriteSerializedString(DirectCast(argument.Value, String))
                Case FixtureArgumentKind.Boolean
                    blob.WriteBoolean(DirectCast(argument.Value, Boolean))
                Case FixtureArgumentKind.Int32, FixtureArgumentKind.Enum
                    blob.WriteInt32(DirectCast(argument.Value, Integer))
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(argument))
            End Select
        End Sub

        ''' <summary>
        ''' Adds the &lt;Module&gt; type, the requested types and the entry-point method (if any).
        ''' </summary>
        ''' <returns>
        ''' The entry-point method handle, or a nil handle.
        ''' </returns>
        Private Shared Function AddTypes(metadata As MetadataBuilder, methodBodies As MethodBodyStreamEncoder, coreLibrary As AssemblyReferenceHandle,
                                         cache As Dictionary(Of String, TypeReferenceHandle), options As FixtureOptions) As MethodDefinitionHandle

            ' Expand the type list so every declaring type precedes its nested types.
            Dim requested As New List(Of String)(options.TypeNames)
            For Each typeName As String In options.TypeGuids.Keys
                If Not requested.Contains(typeName) Then
                    requested.Add(typeName)
                End If
            Next typeName
            If (options.EntryPointTypeName IsNot Nothing) AndAlso Not requested.Contains(options.EntryPointTypeName) Then
                requested.Add(options.EntryPointTypeName)
            End If

            Dim ordered As New List(Of String)
            For Each fullName As String In requested
                Dim parts As String() = fullName.Split("+"c)
                Dim current As String = String.Empty
                For i As Integer = 0 To parts.Length - 1
                    current = If(i = 0, parts(0), $"{current}+{parts(i)}")
                    If Not ordered.Contains(current) Then
                        ordered.Add(current)
                    End If
                Next i
            Next fullName

            ' The entry point (if any) is the first and only method; it belongs to its declaring type.
            Dim entryPoint As MethodDefinitionHandle = Nothing
            Dim ownerIndex As Integer = If(options.EntryPointTypeName Is Nothing, -1, ordered.IndexOf(options.EntryPointTypeName))
            If ownerIndex <> -1 Then
                Dim signature As New BlobBuilder()
                Dim signatureEncoder As New BlobEncoder(signature)
                signatureEncoder.MethodSignature().Parameters(
                    0,
                    Sub(returnType As ReturnTypeEncoder) returnType.Void(),
                    Sub(parameters As ParametersEncoder)
                        ' No parameters.
                    End Sub)

                Dim il As New InstructionEncoder(New BlobBuilder())
                il.OpCode(ILOpCode.Ret)
                Dim bodyOffset As Integer = methodBodies.AddMethodBody(il)

                entryPoint = metadata.AddMethodDefinition(
                    MethodAttributes.Public Or MethodAttributes.Static Or MethodAttributes.HideBySig,
                    MethodImplAttributes.IL,
                    metadata.GetOrAddString("Main"),
                    metadata.GetOrAddBlob(signature),
                    bodyOffset,
                    MetadataTokens.ParameterHandle(1))
            End If

            Dim baseType As EntityHandle = Nothing
            If Not coreLibrary.IsNil Then
                baseType = FixtureAssemblyBuilder.GetOrAddTypeReference(metadata, coreLibrary, cache, "System.Object")
            End If

            metadata.AddTypeDefinition(CType(0, TypeAttributes), Nothing, metadata.GetOrAddString("<Module>"), Nothing,
                                       MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1))

            Dim definitions As New Dictionary(Of String, TypeDefinitionHandle)(StringComparer.Ordinal)
            For i As Integer = 0 To ordered.Count - 1
                Dim fullName As String = ordered(i)
                Dim plusIndex As Integer = fullName.LastIndexOf("+"c)
                Dim namespaceName As String = String.Empty
                Dim typeName As String
                Dim attributes As TypeAttributes

                Dim isInterface As Boolean = options.InterfaceTypeNames.Contains(fullName)
                If plusIndex <> -1 Then
                    typeName = fullName.Substring(plusIndex + 1)
                    attributes = TypeAttributes.NestedPublic
                Else
                    Dim dotIndex As Integer = fullName.LastIndexOf("."c)
                    namespaceName = If(dotIndex = -1, String.Empty, fullName.Substring(0, dotIndex))
                    typeName = If(dotIndex = -1, fullName, fullName.Substring(dotIndex + 1))
                    attributes = TypeAttributes.Public
                End If

                If isInterface Then
                    attributes = attributes Or TypeAttributes.Interface Or TypeAttributes.Abstract
                End If

                Dim methodList As Integer = If((ownerIndex <> -1) AndAlso (i > ownerIndex), 2, 1)
                Dim handle As TypeDefinitionHandle = metadata.AddTypeDefinition(
                    attributes,
                    If(namespaceName.Length = 0, Nothing, metadata.GetOrAddString(namespaceName)),
                    metadata.GetOrAddString(typeName),
                    If(isInterface, Nothing, baseType),
                    MetadataTokens.FieldDefinitionHandle(1),
                    MetadataTokens.MethodDefinitionHandle(methodList))

                definitions.Add(fullName, handle)
                If plusIndex <> -1 Then
                    metadata.AddNestedType(handle, definitions(fullName.Substring(0, plusIndex)))
                End If
            Next i

            ' Type-level GuidAttribute (candidate COM classes and interfaces).
            For Each pair As KeyValuePair(Of String, String) In options.TypeGuids
                FixtureAssemblyBuilder.AddCustomAttribute(metadata, definitions(pair.Key), coreLibrary, cache,
                                                          FixtureAttribute.Guid(pair.Value))
            Next pair

            Return entryPoint
        End Function

        ''' <summary>
        ''' Applies the post-serialization patches requested by the options.
        ''' </summary>
        Private Shared Sub ApplyPatches(image As Byte(), options As FixtureOptions)

            If Not options.ReadyToRun AndAlso Not options.RemoveCliHeader Then
                Return
            End If

            Dim headers As PEHeaders
            Using stream As New MemoryStream(image, writable:=False)
                headers = New PEHeaders(stream)
            End Using

            If options.ReadyToRun Then
                ' Any non-empty ManagedNativeHeader directory marks the image as ReadyToRun.
                FixtureAssemblyBuilder.WriteInt32(image, headers.CorHeaderStartOffset + FixtureAssemblyBuilder.ManagedNativeHeaderOffset, &H2000)
                FixtureAssemblyBuilder.WriteInt32(image, headers.CorHeaderStartOffset + FixtureAssemblyBuilder.ManagedNativeHeaderOffset + 4, &H48)
            End If

            If options.RemoveCliHeader Then
                ' Optional header = COFF header + 20. Data directories start at 96 (PE32) or 112 (PE32+).
                Dim optionalHeaderOffset As Integer = headers.CoffHeaderStartOffset + 20
                Dim dataDirectoriesOffset As Integer = If(headers.PEHeader.Magic = PEMagic.PE32Plus, 112, 96)
                Dim cliDirectoryOffset As Integer = optionalHeaderOffset + dataDirectoriesOffset + (FixtureAssemblyBuilder.CliHeaderDataDirectoryIndex * 8)
                FixtureAssemblyBuilder.WriteInt32(image, cliDirectoryOffset, 0)
                FixtureAssemblyBuilder.WriteInt32(image, cliDirectoryOffset + 4, 0)
            End If
        End Sub

        ''' <summary>Writes a little-endian <see cref="Integer"/> into a byte array.</summary>
        Private Shared Sub WriteInt32(buffer As Byte(), offset As Integer, value As Integer)
            Dim bytes As Byte() = BitConverter.GetBytes(value)
            Array.Copy(bytes, 0, buffer, offset, 4)
        End Sub

#End Region

    End Class

End Namespace

#End Region
