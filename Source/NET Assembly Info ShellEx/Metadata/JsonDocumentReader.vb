
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.Serialization.Json
Imports System.Text
Imports System.Xml

#End Region

#Region " JsonDocumentReader "

Namespace AssemblyInspection

    ''' <summary>
    ''' Reads JSON documents through the built-in JSON-to-XML mapping of <see cref="JsonReaderWriterFactory"/>,
    ''' which is available in .NET Framework and in modern .NET without any additional package.
    ''' </summary>
    ''' <remarks>
    ''' In that mapping every JSON member is an element named after the member, except when the member name
    ''' is not a valid XML name: then the element is <c>&lt;a:item xmlns:a="item" item="member name"&gt;</c>.
    ''' Array items are <c>&lt;item&gt;</c> elements.
    ''' </remarks>
    Friend Module JsonDocumentReader

#Region " Constants "

        ''' <summary>The namespace used by the JSON-to-XML mapping for member names that are not valid XML names.</summary>
        Private Const ItemNamespace As String = "item"

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Parses a JSON document.
        ''' </summary>
        ''' <param name="json">
        ''' The JSON text.
        ''' </param>
        ''' <returns>
        ''' The root element of the JSON-to-XML mapping.
        ''' </returns>
        ''' <exception cref="XmlException">
        ''' The text is not valid JSON.
        ''' </exception>
        Friend Function Parse(json As String) As XElement

            If json Is Nothing Then
                Throw New ArgumentNullException(NameOf(json))
            End If

            Dim bytes As Byte() = Encoding.UTF8.GetBytes(json)
            Using reader As XmlDictionaryReader = JsonReaderWriterFactory.CreateJsonReader(bytes, XmlDictionaryReaderQuotas.Max)
                Return XElement.Load(reader)
            End Using
        End Function

        ''' <summary>
        ''' Gets the JSON member name represented by an element.
        ''' </summary>
        ''' <param name="element">
        ''' The element.
        ''' </param>
        ''' <returns>
        ''' The JSON member name.
        ''' </returns>
        Friend Function GetMemberName(element As XElement) As String

            If element Is Nothing Then
                Throw New ArgumentNullException(NameOf(element))
            End If

            If String.Equals(element.Name.NamespaceName, JsonDocumentReader.ItemNamespace, StringComparison.Ordinal) Then
                Dim nameAttribute As XAttribute = element.Attribute("item")
                If nameAttribute IsNot Nothing Then
                    Return nameAttribute.Value
                End If
            End If
            Return element.Name.LocalName
        End Function

        ''' <summary>
        ''' Gets the child element that represents the specified JSON member.
        ''' </summary>
        ''' <param name="element">
        ''' The parent element (a JSON object).
        ''' </param>
        '''
        ''' <param name="memberName">
        ''' The JSON member name.
        ''' </param>
        ''' <returns>
        ''' The child element, or <see langword="Nothing"/> if the object has no such member.
        ''' </returns>
        Friend Function GetMember(element As XElement, memberName As String) As XElement

            If element Is Nothing Then
                Throw New ArgumentNullException(NameOf(element))
            End If

            Return element.Elements().FirstOrDefault(
                Function(child As XElement) String.Equals(JsonDocumentReader.GetMemberName(child), memberName, StringComparison.Ordinal))
        End Function

        ''' <summary>
        ''' Gets the string value of the specified JSON member.
        ''' </summary>
        ''' <param name="element">
        ''' The parent element (a JSON object).
        ''' </param>
        '''
        ''' <param name="memberName">
        ''' The JSON member name.
        ''' </param>
        ''' <returns>
        ''' The value, or <see cref="String.Empty"/> if the object has no such member.
        ''' </returns>
        Friend Function GetMemberValue(element As XElement, memberName As String) As String

            Dim member As XElement = JsonDocumentReader.GetMember(element, memberName)
            Return If(member Is Nothing, String.Empty, member.Value)
        End Function

#End Region

    End Module

End Namespace

#End Region
