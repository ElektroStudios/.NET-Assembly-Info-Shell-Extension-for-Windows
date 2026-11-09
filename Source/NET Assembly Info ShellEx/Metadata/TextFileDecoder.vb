
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Text
Imports System.Xml

#End Region

#Region " TextFileDecoder "

Namespace AssemblyInspection

    ''' <summary>
    ''' Provides helper methods to decode and parse the text files read from an assembly or from its sidecar files
    ''' (Win32 manifests, app.config, runtimeconfig.json and deps.json files).
    ''' </summary>
    Friend Module TextFileDecoder

#Region " Public Methods "

        ''' <summary>
        ''' Decodes the specified bytes as text, honoring any byte order mark (UTF-8 is assumed otherwise),
        ''' and removes the trailing null characters used as padding in Win32 resources.
        ''' </summary>
        ''' <param name="bytes">
        ''' The bytes to decode.
        ''' </param>
        ''' <returns>
        ''' The decoded text, without byte order mark.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="bytes"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Function Decode(bytes As Byte()) As String

            If bytes Is Nothing Then
                Throw New ArgumentNullException(NameOf(bytes))
            End If

            Using stream As New MemoryStream(bytes, writable:=False),
                  reader As New StreamReader(stream, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False), detectEncodingFromByteOrderMarks:=True)
                Return reader.ReadToEnd().TrimEnd(ControlChars.NullChar)
            End Using
        End Function

        ''' <summary>
        ''' Parses an XML document securely (DTD processing prohibited and no external resolution).
        ''' </summary>
        ''' <param name="xml">
        ''' The XML text.
        ''' </param>
        ''' <returns>
        ''' The parsed document.
        ''' </returns>
        ''' <exception cref="XmlException">
        ''' The text is not a well-formed XML document.
        ''' </exception>
        Friend Function ParseXml(xml As String) As XDocument

            If xml Is Nothing Then
                Throw New ArgumentNullException(NameOf(xml))
            End If

            Dim settings As New XmlReaderSettings() With {
                .DtdProcessing = DtdProcessing.Prohibit,
                .XmlResolver = Nothing,
                .IgnoreComments = True
            }

            Using stringReader As New StringReader(xml),
                  xmlReader As XmlReader = XmlReader.Create(stringReader, settings)
                Return XDocument.Load(xmlReader)
            End Using
        End Function

#End Region

    End Module

End Namespace

#End Region
