
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection.Metadata

#End Region

#Region " MetadataReaderHelper "

Namespace AssemblyInspection

    ''' <summary>
    ''' Provides helper methods to resolve names from a <see cref="MetadataReader"/>.
    ''' </summary>
    Friend Module MetadataReaderHelper

#Region " Public Methods "

        ''' <summary>
        ''' Gets the full name of a type definition, using the reflection notation for nested types
        ''' (e.g. <c>Namespace.Outer+Inner</c>).
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        '''
        ''' <param name="handle">
        ''' The type definition handle.
        ''' </param>
        ''' <returns>
        ''' The full name of the type.
        ''' </returns>
        <DebuggerStepThrough>
        Friend Function GetTypeFullName(reader As MetadataReader, handle As TypeDefinitionHandle) As String

            MetadataReaderHelper.ThrowIfNull(reader, NameOf(reader))
            If handle.IsNil Then
                Throw New ArgumentException("The type definition handle is nil.", NameOf(handle))
            End If

            Dim names As New List(Of String)
            Dim namespaceName As String
            Dim current As TypeDefinitionHandle = handle

            ' Walk up the declaring types iteratively (no recursion).
            Do
                Dim definition As TypeDefinition = reader.GetTypeDefinition(current)
                names.Insert(0, reader.GetString(definition.Name))

                Dim declaringType As TypeDefinitionHandle = definition.GetDeclaringType()
                If declaringType.IsNil Then
                    namespaceName = reader.GetString(definition.Namespace)
                    Exit Do
                End If
                current = declaringType
            Loop

            Return MetadataReaderHelper.CombineTypeName(namespaceName, names)
        End Function

        ''' <summary>
        ''' Gets the full name of a type reference, using the reflection notation for nested types
        ''' (e.g. <c>Namespace.Outer+Inner</c>).
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        '''
        ''' <param name="handle">
        ''' The type reference handle.
        ''' </param>
        ''' <returns>
        ''' The full name of the referenced type.
        ''' </returns>
        <DebuggerStepThrough>
        Friend Function GetTypeReferenceFullName(reader As MetadataReader, handle As TypeReferenceHandle) As String

            MetadataReaderHelper.ThrowIfNull(reader, NameOf(reader))
            If handle.IsNil Then
                Throw New ArgumentException("The type reference handle is nil.", NameOf(handle))
            End If

            Dim names As New List(Of String)
            Dim namespaceName As String
            Dim current As TypeReferenceHandle = handle

            ' A nested type reference has another type reference as its resolution scope.
            Do
                Dim reference As TypeReference = reader.GetTypeReference(current)
                names.Insert(0, reader.GetString(reference.Name))

                If reference.ResolutionScope.Kind <> HandleKind.TypeReference Then
                    namespaceName = reader.GetString(reference.Namespace)
                    Exit Do
                End If
                current = CType(reference.ResolutionScope, TypeReferenceHandle)
            Loop

            Return MetadataReaderHelper.CombineTypeName(namespaceName, names)
        End Function

        ''' <summary>
        ''' Gets the full name of the attribute type of the specified custom attribute.
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        '''
        ''' <param name="attribute">
        ''' The custom attribute.
        ''' </param>
        ''' <returns>
        ''' The full name of the attribute type,
        ''' or <see cref="String.Empty"/> if the attribute type is a generic instantiation (type specification).
        ''' </returns>
        ''' <exception cref="BadImageFormatException">
        ''' The attribute constructor is neither a method definition nor a member reference.
        ''' </exception>
        <DebuggerStepThrough>
        Friend Function GetAttributeTypeFullName(reader As MetadataReader, attribute As CustomAttribute) As String

            MetadataReaderHelper.ThrowIfNull(reader, NameOf(reader))

            Select Case attribute.Constructor.Kind

                Case HandleKind.MethodDefinition
                    Dim method As MethodDefinition = reader.GetMethodDefinition(CType(attribute.Constructor, MethodDefinitionHandle))
                    Return MetadataReaderHelper.GetTypeFullName(reader, method.GetDeclaringType())

                Case HandleKind.MemberReference
                    Dim member As MemberReference = reader.GetMemberReference(CType(attribute.Constructor, MemberReferenceHandle))
                    Select Case member.Parent.Kind
                        Case HandleKind.TypeReference
                            Return MetadataReaderHelper.GetTypeReferenceFullName(reader, CType(member.Parent, TypeReferenceHandle))

                        Case HandleKind.TypeDefinition
                            Return MetadataReaderHelper.GetTypeFullName(reader, CType(member.Parent, TypeDefinitionHandle))

                        Case Else
                            ' Generic attribute instantiations (TypeSpec) are not well-known attributes.
                            Return String.Empty
                    End Select

                Case Else
                    Throw New BadImageFormatException($"Invalid custom attribute constructor kind: {attribute.Constructor.Kind}.")

            End Select
        End Function

        ''' <summary>
        ''' Gets the custom attributes applied to the assembly definition, keyed by attribute type full name.
        ''' <para></para>
        ''' When an attribute type is applied more than once, only the first occurrence is kept.
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        ''' <returns>
        ''' A dictionary of attribute type full name and custom attribute.
        ''' </returns>
        <DebuggerStepThrough>
        Friend Function GetAssemblyAttributes(reader As MetadataReader) As Dictionary(Of String, CustomAttribute)

            MetadataReaderHelper.ThrowIfNull(reader, NameOf(reader))

            Dim attributes As New Dictionary(Of String, CustomAttribute)(StringComparer.Ordinal)
            For Each handle As CustomAttributeHandle In reader.GetAssemblyDefinition().GetCustomAttributes()
                Dim attribute As CustomAttribute = reader.GetCustomAttribute(handle)
                Dim typeName As String = MetadataReaderHelper.GetAttributeTypeFullName(reader, attribute)
                If (typeName.Length <> 0) AndAlso Not attributes.ContainsKey(typeName) Then
                    attributes.Add(typeName, attribute)
                End If
            Next handle

            Return attributes
        End Function

        ''' <summary>
        ''' Gets the full name of a method definition, in the form <c>Namespace.Type.Method</c>.
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        '''
        ''' <param name="handle">
        ''' The method definition handle.
        ''' </param>
        ''' <returns>
        ''' The full name of the method.
        ''' </returns>
        <DebuggerStepThrough>
        Friend Function GetMethodFullName(reader As MetadataReader, handle As MethodDefinitionHandle) As String

            MetadataReaderHelper.ThrowIfNull(reader, NameOf(reader))
            If handle.IsNil Then
                Throw New ArgumentException("The method definition handle is nil.", NameOf(handle))
            End If

            Dim method As MethodDefinition = reader.GetMethodDefinition(handle)
            Dim typeName As String = MetadataReaderHelper.GetTypeFullName(reader, method.GetDeclaringType())
            Return $"{typeName}.{reader.GetString(method.Name)}"
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Combines a namespace and a chain of (nested) type names.
        ''' </summary>
        ''' <param name="namespaceName">
        ''' The namespace name (it can be empty).
        ''' </param>
        '''
        ''' <param name="names">
        ''' The type names, from the outermost to the innermost type.
        ''' </param>
        ''' <returns>
        ''' The combined full type name.
        ''' </returns>
        Private Function CombineTypeName(namespaceName As String, names As List(Of String)) As String

            Dim typeName As String = String.Join("+", names)
            Return If(String.IsNullOrEmpty(namespaceName), typeName, $"{namespaceName}.{typeName}")
        End Function

        ''' <summary>
        ''' Throws an <see cref="ArgumentNullException"/> if the specified reader is <see langword="Nothing"/>.
        ''' </summary>
        ''' <param name="reader">
        ''' The metadata reader.
        ''' </param>
        '''
        ''' <param name="paramName">
        ''' The parameter name.
        ''' </param>
        Private Sub ThrowIfNull(reader As MetadataReader, paramName As String)

            If reader Is Nothing Then
                Throw New ArgumentNullException(paramName)
            End If
        End Sub

#End Region

    End Module

End Namespace

#End Region
