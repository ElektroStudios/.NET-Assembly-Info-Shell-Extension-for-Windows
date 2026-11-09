
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Globalization
Imports System.Reflection.Metadata

#End Region

#Region " DeclarativeSecurityDecoder "

Namespace AssemblyInspection

    ''' <summary>
    ''' Decodes the assembly-level declarative security (CAS) permission requests
    ''' (e.g. <c>&lt;Assembly: SecurityPermission(SecurityAction.RequestMinimum, ...)&gt;</c>).
    ''' </summary>
    ''' <remarks>
    ''' Declarative security attributes are not stored as custom attributes,
    ''' but as permission set blobs in the <c>DeclSecurity</c> metadata table (ECMA-335 §II.23.1.3 and §II.22.11).
    ''' </remarks>
    Friend Module DeclarativeSecurityDecoder

#Region " Constants "

        ''' <summary>
        ''' The prolog byte ('.') of a binary-encoded permission set blob.
        ''' </summary>
        Private Const BinaryPermissionSetProlog As Byte = &H2E

        ''' <summary>
        ''' The full name of the <c>SecurityPermissionAttribute</c> type.
        ''' </summary>
        Private Const SecurityPermissionAttributeName As String = "System.Security.Permissions.SecurityPermissionAttribute"

#End Region

#Region " Enumerations "

        ''' <summary>
        ''' Mirrors the <c>System.Security.Permissions.SecurityPermissionFlag</c> enumeration,
        ''' which is not available on every target framework.
        ''' </summary>
        <Flags>
        Private Enum SecurityPermissionFlags As Integer
            NoFlags = 0
            Assertion = 1 << 0
            UnmanagedCode = 1 << 1
            SkipVerification = 1 << 2
            Execution = 1 << 3
            ControlThread = 1 << 4
            ControlEvidence = 1 << 5
            ControlPolicy = 1 << 6
            SerializationFormatter = 1 << 7
            ControlDomainPolicy = 1 << 8
            ControlPrincipal = 1 << 9
            ControlAppDomain = 1 << 10
            RemotingConfiguration = 1 << 11
            Infrastructure = 1 << 12
            BindingRedirects = 1 << 13
            AllFlags = (1 << 14) - 1
        End Enum

        ''' <summary>
        ''' The element types that can appear in a serialized named argument (ECMA-335 §II.23.3).
        ''' </summary>
        Private Enum SerializationType As Byte
            [Boolean] = &H2
            [Char] = &H3
            [SByte] = &H4
            [Byte] = &H5
            Int16 = &H6
            UInt16 = &H7
            Int32 = &H8
            UInt32 = &H9
            Int64 = &HA
            UInt64 = &HB
            [Single] = &HC
            [Double] = &HD
            [String] = &HE
            Type = &H50
            [Enum] = &H55
        End Enum

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets a human-readable description of the assembly-level declarative security permission requests.
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        ''' <returns>
        ''' A description such as <c>RequestMinimum: SecurityPermission (UnmanagedCode, Execution)</c>,
        ''' with multiple requests separated by "; ";
        ''' or <see cref="String.Empty"/> if the assembly has no declarative security.
        ''' </returns>
        Friend Function Describe(reader As MetadataReader) As String

            If reader Is Nothing Then
                Throw New ArgumentNullException(NameOf(reader))
            End If

            Dim requests As New List(Of String)
            For Each handle As DeclarativeSecurityAttributeHandle In reader.GetAssemblyDefinition().GetDeclarativeSecurityAttributes()
                Dim security As DeclarativeSecurityAttribute = reader.GetDeclarativeSecurityAttribute(handle)
                Dim blob As BlobReader = reader.GetBlobReader(security.PermissionSet)
                requests.Add($"{security.Action}: {DeclarativeSecurityDecoder.DescribePermissionSet(blob)}")
            Next handle

            Return String.Join("; ", requests)
        End Function

        ''' <summary>
        ''' Gets a human-readable description of a permission set blob.
        ''' </summary>
        ''' <param name="blob">
        ''' The permission set blob.
        ''' </param>
        ''' <returns>
        ''' The description of the permission set.
        ''' </returns>
        Friend Function DescribePermissionSet(blob As BlobReader) As String

            If blob.Length = 0 Then
                Return "(empty permission set)"
            End If

            ' .NET 1.x compilers stored the permission set as an UTF-16 XML document.
            If blob.ReadByte() <> DeclarativeSecurityDecoder.BinaryPermissionSetProlog Then
                Return "(XML permission set)"
            End If

            Dim attributeCount As Integer = blob.ReadCompressedInteger()
            Dim permissions As New List(Of String)(attributeCount)

            For i As Integer = 0 To attributeCount - 1
                Dim attributeTypeName As String = DeclarativeSecurityDecoder.StripAssemblyQualification(blob.ReadSerializedString())
                Dim argumentsLength As Integer = blob.ReadCompressedInteger()
                Dim argumentsEnd As Integer = blob.Offset + argumentsLength

                Dim arguments As List(Of String) = DeclarativeSecurityDecoder.ReadNamedArguments(blob, attributeTypeName, argumentsEnd)

                ' Always continue from the end of this attribute, even if not every argument could be decoded.
                blob.Offset = argumentsEnd

                Dim permissionName As String = DeclarativeSecurityDecoder.GetPermissionName(attributeTypeName)
                permissions.Add(If(arguments.Count = 0, permissionName, $"{permissionName} ({String.Join(", ", arguments)})"))
            Next i

            Return String.Join(", ", permissions)
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Reads the named arguments (properties and fields) of a security attribute.
        ''' </summary>
        ''' <param name="blob">
        ''' The blob reader, positioned at the named argument count.
        ''' </param>
        '''
        ''' <param name="attributeTypeName">
        ''' The full name of the security attribute type.
        ''' </param>
        '''
        ''' <param name="argumentsEnd">
        ''' The blob offset where the arguments of this attribute end.
        ''' </param>
        ''' <returns>
        ''' The formatted arguments. Decoding stops at the first argument of an unsupported type.
        ''' </returns>
        Private Function ReadNamedArguments(ByRef blob As BlobReader, attributeTypeName As String, argumentsEnd As Integer) As List(Of String)

            Dim arguments As New List(Of String)
            If blob.Offset >= argumentsEnd Then
                Return arguments
            End If

            Dim argumentCount As Integer = blob.ReadCompressedInteger()
            For i As Integer = 0 To argumentCount - 1
                blob.ReadByte() ' FIELD (0x53) or PROPERTY (0x54); irrelevant for display purposes.
                Dim type As SerializationType = CType(blob.ReadByte(), SerializationType)
                If type = SerializationType.Enum Then
                    blob.ReadSerializedString() ' Enum type name. All the CAS enums are Int32-based.
                End If

                Dim name As String = blob.ReadSerializedString()
                Dim value As Object = Nothing
                If Not DeclarativeSecurityDecoder.TryReadValue(blob, type, value) Then
                    Exit For
                End If

                If String.Equals(attributeTypeName, DeclarativeSecurityDecoder.SecurityPermissionAttributeName, StringComparison.Ordinal) AndAlso
                   String.Equals(name, "Flags", StringComparison.Ordinal) Then
                    arguments.Add(CType(CInt(value), SecurityPermissionFlags).ToString())
                Else
                    arguments.Add($"{name}={Convert.ToString(value, CultureInfo.InvariantCulture)}")
                End If
            Next i

            Return arguments
        End Function

        ''' <summary>
        ''' Reads a serialized value of the specified type.
        ''' </summary>
        ''' <param name="blob">
        ''' The blob reader.
        ''' </param>
        '''
        ''' <param name="type">
        ''' The serialization type.
        ''' </param>
        '''
        ''' <param name="value">
        ''' When this method returns, contains the value that was read.
        ''' </param>
        ''' <returns>
        ''' <see langword="True"/> if the type is supported and the value was read; otherwise, <see langword="False"/>.
        ''' </returns>
        Private Function TryReadValue(ByRef blob As BlobReader, type As SerializationType, ByRef value As Object) As Boolean

            Select Case type
                Case SerializationType.Boolean : value = blob.ReadBoolean()
                Case SerializationType.Char : value = blob.ReadChar()
                Case SerializationType.SByte : value = blob.ReadSByte()
                Case SerializationType.Byte : value = blob.ReadByte()
                Case SerializationType.Int16 : value = blob.ReadInt16()
                Case SerializationType.UInt16 : value = blob.ReadUInt16()
                Case SerializationType.Int32, SerializationType.Enum : value = blob.ReadInt32()
                Case SerializationType.UInt32 : value = blob.ReadUInt32()
                Case SerializationType.Int64 : value = blob.ReadInt64()
                Case SerializationType.UInt64 : value = blob.ReadUInt64()
                Case SerializationType.Single : value = blob.ReadSingle()
                Case SerializationType.Double : value = blob.ReadDouble()
                Case SerializationType.String, SerializationType.Type : value = blob.ReadSerializedString()
                Case Else
                    value = Nothing
                    Return False
            End Select

            Return True
        End Function

        ''' <summary>
        ''' Gets a short permission name from a security attribute type name
        ''' (e.g. <c>System.Security.Permissions.SecurityPermissionAttribute</c> -> <c>SecurityPermission</c>).
        ''' </summary>
        ''' <param name="attributeTypeName">
        ''' The full name of the security attribute type.
        ''' </param>
        ''' <returns>
        ''' The short permission name.
        ''' </returns>
        Private Function GetPermissionName(attributeTypeName As String) As String

            If String.IsNullOrEmpty(attributeTypeName) Then
                Return "(unknown permission)"
            End If

            Dim name As String = attributeTypeName.Substring(attributeTypeName.LastIndexOf("."c) + 1)
            If name.EndsWith("Attribute", StringComparison.Ordinal) AndAlso (name.Length > "Attribute".Length) Then
                name = name.Substring(0, name.Length - "Attribute".Length)
            End If
            Return name
        End Function

        ''' <summary>
        ''' Removes the assembly qualification from a serialized type name
        ''' (e.g. <c>Namespace.Type, mscorlib, Version=4.0.0.0</c> -> <c>Namespace.Type</c>).
        ''' </summary>
        ''' <param name="typeName">
        ''' The serialized type name. It can be <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The type name without assembly qualification, or <see cref="String.Empty"/> if <paramref name="typeName"/> is null.
        ''' </returns>
        Private Function StripAssemblyQualification(typeName As String) As String

            If typeName Is Nothing Then
                Return String.Empty
            End If

            Dim commaIndex As Integer = typeName.IndexOf(","c)
            Return If(commaIndex = -1, typeName, typeName.Substring(0, commaIndex)).Trim()
        End Function

#End Region

    End Module

End Namespace

#End Region
