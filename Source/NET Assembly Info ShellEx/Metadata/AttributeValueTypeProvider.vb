
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection.Metadata

#End Region

#Region " AttributeValueTypeProvider "

Namespace AssemblyInspection

    ''' <summary>
    ''' A minimal <see cref="ICustomAttributeTypeProvider(Of TType)"/> that represents types by their full name,
    ''' used to decode the values of well-known custom attributes without loading any assembly.
    ''' </summary>
    ''' <remarks>
    ''' Only the enum types listed in <see cref="AttributeValueTypeProvider.KnownEnumTypes"/> can be decoded.
    ''' Any other enum type causes a <see cref="NotSupportedException"/>, because guessing its underlying type
    ''' would silently produce corrupted values.
    ''' </remarks>
    Friend NotInheritable Class AttributeValueTypeProvider : Implements ICustomAttributeTypeProvider(Of String)

#Region " Constants "

        ''' <summary>
        ''' The full name of the <see cref="System.Type"/> class.
        ''' </summary>
        Friend Const SystemTypeName As String = "System.Type"

#End Region

#Region " Private Fields "

        ''' <summary>
        ''' The enum types that can appear in the constructor of the decoded attributes,
        ''' and their underlying primitive type.
        ''' </summary>
        Private Shared ReadOnly KnownEnumTypes As New Dictionary(Of String, PrimitiveTypeCode)(StringComparer.Ordinal) From {
            {"System.Runtime.InteropServices.DllImportSearchPath", PrimitiveTypeCode.Int32},
            {"System.Diagnostics.DebuggableAttribute+DebuggingModes", PrimitiveTypeCode.Int32},
            {"System.Security.Permissions.SecurityAction", PrimitiveTypeCode.Int32}
        }

#End Region

#Region " ICustomAttributeTypeProvider Implementation "

        ''' <inheritdoc/>
        Public Function GetPrimitiveType(typeCode As PrimitiveTypeCode) As String Implements ISimpleTypeProvider(Of String).GetPrimitiveType
            Return typeCode.ToString()
        End Function

        ''' <inheritdoc/>
        Public Function GetTypeFromDefinition(reader As MetadataReader, handle As TypeDefinitionHandle, rawTypeKind As Byte) As String Implements ISimpleTypeProvider(Of String).GetTypeFromDefinition
            Return MetadataReaderHelper.GetTypeFullName(reader, handle)
        End Function

        ''' <inheritdoc/>
        Public Function GetTypeFromReference(reader As MetadataReader, handle As TypeReferenceHandle, rawTypeKind As Byte) As String Implements ISimpleTypeProvider(Of String).GetTypeFromReference
            Return MetadataReaderHelper.GetTypeReferenceFullName(reader, handle)
        End Function

        ''' <inheritdoc/>
        Public Function GetSZArrayType(elementType As String) As String Implements ISZArrayTypeProvider(Of String).GetSZArrayType
            Return $"{elementType}[]"
        End Function

        ''' <inheritdoc/>
        Public Function GetSystemType() As String Implements ICustomAttributeTypeProvider(Of String).GetSystemType
            Return AttributeValueTypeProvider.SystemTypeName
        End Function

        ''' <inheritdoc/>
        Public Function IsSystemType(type As String) As Boolean Implements ICustomAttributeTypeProvider(Of String).IsSystemType
            Return String.Equals(type, AttributeValueTypeProvider.SystemTypeName, StringComparison.Ordinal)
        End Function

        ''' <inheritdoc/>
        Public Function GetTypeFromSerializedName(name As String) As String Implements ICustomAttributeTypeProvider(Of String).GetTypeFromSerializedName
            Return name
        End Function

        ''' <inheritdoc/>
        ''' <exception cref="NotSupportedException">
        ''' The enum type is not one of the known enum types.
        ''' </exception>
        Public Function GetUnderlyingEnumType(type As String) As PrimitiveTypeCode Implements ICustomAttributeTypeProvider(Of String).GetUnderlyingEnumType

            ' Serialized names can be assembly-qualified ("Namespace.Type, AssemblyName, Version=...").
            Dim commaIndex As Integer = type.IndexOf(","c)
            Dim typeName As String = If(commaIndex = -1, type, type.Substring(0, commaIndex)).Trim()

            Dim typeCode As PrimitiveTypeCode
            If AttributeValueTypeProvider.KnownEnumTypes.TryGetValue(typeName, typeCode) Then
                Return typeCode
            End If

            Throw New NotSupportedException($"The underlying type of enum '{typeName}' is unknown.")
        End Function

#End Region

    End Class

End Namespace

#End Region
