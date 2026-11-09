
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Net
Imports System.Text

#End Region

#Region " ReportFormat "

Namespace AssemblyInspection

    ''' <summary>
    ''' Specifies the format of an exported report.
    ''' <para></para>
    ''' The order of the values matches the order of <see cref="AssemblyInfoReport.SaveFileDialogFilter"/>.
    ''' </summary>
    Friend Enum ReportFormat As Integer

        ''' <summary>Plain text, with aligned "Name: Value" rows.</summary>
        Text = 0

        ''' <summary>Markdown, with one table per group (GitHub flavored).</summary>
        Markdown = 1

        ''' <summary>A standalone HTML document, with one table per group.</summary>
        Html = 2

        ''' <summary>JSON: <c>{ "file": ..., "groups": [ { "title": ..., "rows": [ { "name": ..., "value": ... } ] } ] }</c>.</summary>
        Json = 3

        ''' <summary>XML: <c>&lt;assemblyInfo file="..."&gt;&lt;group title="..."&gt;&lt;property name="..."&gt;value&lt;/property&gt;</c>.</summary>
        Xml = 4

        ''' <summary>CSV (RFC 4180): <c>Group,Property,Value</c>.</summary>
        Csv = 5

    End Enum

#End Region

#Region " AssemblyInfoReport "

    ''' <summary>
    ''' Builds the reports exported or copied by the property page.
    ''' </summary>
    Friend NotInheritable Class AssemblyInfoReport

#Region " Constants "

        ''' <summary>The filter of the export dialog, in <see cref="ReportFormat"/> order.</summary>
        Friend Const SaveFileDialogFilter As String =
            "Text file (*.txt)|*.txt|Markdown file (*.md)|*.md|HTML document (*.html)|*.html|JSON file (*.json)|*.json|XML file (*.xml)|*.xml|CSV file (*.csv)|*.csv"

#End Region

#Region " Constructors "

        ''' <summary>Prevents a default instance of the <see cref="AssemblyInfoReport"/> class from being created.</summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Builds the plain-text report of the specified property groups.
        ''' </summary>
        ''' <param name="filePath">The assembly file path, written as the report header.</param>
        ''' <param name="groups">The property groups.</param>
        ''' <returns>
        ''' The report text.
        ''' </returns>
        Friend Shared Function Build(filePath As String, groups As IEnumerable(Of PropertyGroup)) As String
            Return AssemblyInfoReport.Build(filePath, groups, ReportFormat.Text)
        End Function

        ''' <summary>
        ''' Builds the report of the specified property groups in the specified format.
        ''' Groups without rows are skipped.
        ''' </summary>
        ''' <param name="filePath">The assembly file path, written as the report header.</param>
        ''' <param name="groups">The property groups.</param>
        ''' <param name="format">The report format.</param>
        ''' <returns>
        ''' The report text.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="filePath"/> or <paramref name="groups"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Shared Function Build(filePath As String, groups As IEnumerable(Of PropertyGroup), format As ReportFormat) As String

            If filePath Is Nothing Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If
            If groups Is Nothing Then
                Throw New ArgumentNullException(NameOf(groups))
            End If

            Dim nonEmpty As List(Of PropertyGroup) = groups.Where(Function(group As PropertyGroup) group.Rows.Count <> 0).ToList()

            Select Case format
                Case ReportFormat.Text : Return AssemblyInfoReport.BuildText(filePath, nonEmpty)
                Case ReportFormat.Markdown : Return AssemblyInfoReport.BuildMarkdown(filePath, nonEmpty)
                Case ReportFormat.Html : Return AssemblyInfoReport.BuildHtml(filePath, nonEmpty)
                Case ReportFormat.Json : Return AssemblyInfoReport.BuildJson(filePath, nonEmpty)
                Case ReportFormat.Xml : Return AssemblyInfoReport.BuildXml(filePath, nonEmpty)
                Case ReportFormat.Csv : Return AssemblyInfoReport.BuildCsv(nonEmpty)
                Case Else
                    Throw New System.ComponentModel.InvalidEnumArgumentException(NameOf(format), CInt(format), GetType(ReportFormat))
            End Select
        End Function

        ''' <summary>
        ''' Gets the report format that corresponds to a file extension.
        ''' </summary>
        ''' <param name="filePath">The destination file path.</param>
        ''' <param name="defaultFormat">The format to use when the extension is not recognized.</param>
        ''' <returns>
        ''' The report format.
        ''' </returns>
        Friend Shared Function GetFormatFromExtension(filePath As String, defaultFormat As ReportFormat) As ReportFormat

            Select Case Path.GetExtension(If(filePath, String.Empty)).ToLowerInvariant()
                Case ".txt", ".text" : Return ReportFormat.Text
                Case ".md", ".markdown" : Return ReportFormat.Markdown
                Case ".htm", ".html" : Return ReportFormat.Html
                Case ".json" : Return ReportFormat.Json
                Case ".xml" : Return ReportFormat.Xml
                Case ".csv" : Return ReportFormat.Csv
                Case Else : Return defaultFormat
            End Select
        End Function

        ''' <summary>
        ''' Escapes a value as a JSON string literal (including the quotes).
        ''' </summary>
        ''' <param name="value">The value.</param>
        ''' <returns>
        ''' The JSON string literal.
        ''' </returns>
        Friend Shared Function ToJsonString(value As String) As String

            Dim sb As New StringBuilder(value.Length + 2)
            sb.Append(""""c)
            For Each c As Char In value
                Select Case c
                    Case """"c : sb.Append("\""")
                    Case "\"c : sb.Append("\\")
                    Case ControlChars.Back : sb.Append("\b")
                    Case ControlChars.FormFeed : sb.Append("\f")
                    Case ControlChars.Lf : sb.Append("\n")
                    Case ControlChars.Cr : sb.Append("\r")
                    Case ControlChars.Tab : sb.Append("\t")
                    Case Else
                        If AscW(c) < &H20 Then
                            sb.Append($"\u{AscW(c):x4}")
                        Else
                            sb.Append(c)
                        End If
                End Select
            Next c
            sb.Append(""""c)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Escapes a value for a Markdown table cell.
        ''' </summary>
        ''' <param name="value">The value.</param>
        ''' <returns>
        ''' The escaped value.
        ''' </returns>
        Friend Shared Function ToMarkdownCell(value As String) As String

            Dim sb As New StringBuilder(value.Length)
            For Each c As Char In value
                Select Case c
                    Case "\"c, "`"c, "*"c, "_"c, "|"c, "<"c, ">"c, "["c, "]"c
                        sb.Append("\"c).Append(c)
                    Case ControlChars.Cr
                        ' Ignored: line breaks are written as <br>.
                    Case ControlChars.Lf
                        sb.Append("<br>")
                    Case Else
                        sb.Append(c)
                End Select
            Next c
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Escapes a value as a CSV field (RFC 4180): quoted when it contains a comma, a quote or a line break.
        ''' </summary>
        ''' <param name="value">The value.</param>
        ''' <returns>
        ''' The CSV field.
        ''' </returns>
        Friend Shared Function ToCsvField(value As String) As String

            If value.IndexOfAny({","c, """"c, ControlChars.Cr, ControlChars.Lf}) = -1 Then
                Return value
            End If
            Return $"""{value.Replace("""", """""")}"""
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>Builds the plain-text report.</summary>
        Private Shared Function BuildText(filePath As String, groups As List(Of PropertyGroup)) As String

            Dim sb As New StringBuilder()
            sb.AppendLine(filePath)
            sb.AppendLine()

            For Each group As PropertyGroup In groups
                sb.AppendLine(group.Title)
                sb.AppendLine(New String("-"c, group.Title.Length))

                Dim nameWidth As Integer = group.Rows.Max(Function(row As PropertyRow) row.Name.Length)
                For Each row As PropertyRow In group.Rows
                    sb.AppendLine($"{row.Name.PadRight(nameWidth)}: {row.Value}".TrimEnd())
                Next row

                sb.AppendLine()
            Next group

            Return sb.ToString()
        End Function

        ''' <summary>Builds the Markdown report.</summary>
        Private Shared Function BuildMarkdown(filePath As String, groups As List(Of PropertyGroup)) As String

            Dim sb As New StringBuilder()
            sb.AppendLine($"# {AssemblyInfoReport.ToMarkdownCell(Path.GetFileName(filePath))}")
            sb.AppendLine()
            sb.AppendLine($"`{filePath.Replace("`", "'")}`")

            For Each group As PropertyGroup In groups
                sb.AppendLine()
                sb.AppendLine($"## {AssemblyInfoReport.ToMarkdownCell(group.Title)}")
                sb.AppendLine()
                sb.AppendLine("| Property | Value |")
                sb.AppendLine("|---|---|")
                For Each row As PropertyRow In group.Rows
                    sb.AppendLine($"| {AssemblyInfoReport.ToMarkdownCell(row.Name)} | {AssemblyInfoReport.ToMarkdownCell(row.Value)} |")
                Next row
            Next group

            Return sb.ToString()
        End Function

        ''' <summary>Builds the HTML report.</summary>
        Private Shared Function BuildHtml(filePath As String, groups As List(Of PropertyGroup)) As String

            Dim sb As New StringBuilder()
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html lang=""en"">")
            sb.AppendLine("<head>")
            sb.AppendLine("<meta charset=""utf-8"">")
            sb.AppendLine($"<title>{WebUtility.HtmlEncode(Path.GetFileName(filePath))} - .NET Assembly Info</title>")
            sb.AppendLine("<style>")
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 14px; margin: 24px; color: #1f2328; }")
            sb.AppendLine("h1 { font-size: 22px; margin-bottom: 4px; } h2 { font-size: 17px; margin-top: 28px; }")
            sb.AppendLine(".path { color: #59636e; font-family: Consolas, monospace; }")
            sb.AppendLine("table { border-collapse: collapse; min-width: 50%; }")
            sb.AppendLine("th, td { border: 1px solid #d1d9e0; padding: 4px 10px; text-align: left; vertical-align: top; }")
            sb.AppendLine("th { background: #f6f8fa; } td:first-child { white-space: nowrap; font-weight: 600; }")
            sb.AppendLine("td:last-child { font-family: Consolas, monospace; word-break: break-all; }")
            sb.AppendLine("</style>")
            sb.AppendLine("</head>")
            sb.AppendLine("<body>")
            sb.AppendLine($"<h1>{WebUtility.HtmlEncode(Path.GetFileName(filePath))}</h1>")
            sb.AppendLine($"<p class=""path"">{WebUtility.HtmlEncode(filePath)}</p>")

            For Each group As PropertyGroup In groups
                sb.AppendLine($"<h2>{WebUtility.HtmlEncode(group.Title)}</h2>")
                sb.AppendLine("<table>")
                sb.AppendLine("<tr><th>Property</th><th>Value</th></tr>")
                For Each row As PropertyRow In group.Rows
                    sb.AppendLine($"<tr><td>{WebUtility.HtmlEncode(row.Name)}</td><td>{WebUtility.HtmlEncode(row.Value)}</td></tr>")
                Next row
                sb.AppendLine("</table>")
            Next group

            sb.AppendLine("</body>")
            sb.AppendLine("</html>")
            Return sb.ToString()
        End Function

        ''' <summary>Builds the JSON report.</summary>
        Private Shared Function BuildJson(filePath As String, groups As List(Of PropertyGroup)) As String

            Dim sb As New StringBuilder()
            sb.AppendLine("{")
            sb.AppendLine($"  ""file"": {AssemblyInfoReport.ToJsonString(filePath)},")
            sb.AppendLine("  ""groups"": [")

            For g As Integer = 0 To groups.Count - 1
                Dim group As PropertyGroup = groups(g)
                sb.AppendLine("    {")
                sb.AppendLine($"      ""title"": {AssemblyInfoReport.ToJsonString(group.Title)},")
                sb.AppendLine("      ""rows"": [")
                For r As Integer = 0 To group.Rows.Count - 1
                    Dim row As PropertyRow = group.Rows(r)
                    Dim rowSeparator As String = If(r = group.Rows.Count - 1, String.Empty, ",")
                    sb.AppendLine($"        {{ ""name"": {AssemblyInfoReport.ToJsonString(row.Name)}, ""value"": {AssemblyInfoReport.ToJsonString(row.Value)} }}{rowSeparator}")
                Next r
                sb.AppendLine("      ]")
                sb.AppendLine($"    }}{If(g = groups.Count - 1, String.Empty, ",")}")
            Next g

            sb.AppendLine("  ]")
            sb.AppendLine("}")
            Return sb.ToString()
        End Function

        ''' <summary>Builds the XML report.</summary>
        Private Shared Function BuildXml(filePath As String, groups As List(Of PropertyGroup)) As String

            Dim root As New XElement("assemblyInfo", New XAttribute("file", filePath))
            For Each group As PropertyGroup In groups
                Dim groupElement As New XElement("group", New XAttribute("title", group.Title))
                For Each row As PropertyRow In group.Rows
                    groupElement.Add(New XElement("property", New XAttribute("name", row.Name), row.Value))
                Next row
                root.Add(groupElement)
            Next group

            Dim document As New XDocument(New XDeclaration("1.0", "utf-8", Nothing), root)
            Return $"{document.Declaration}{Environment.NewLine}{document}{Environment.NewLine}"
        End Function

        ''' <summary>Builds the CSV report (RFC 4180, CRLF line endings).</summary>
        Private Shared Function BuildCsv(groups As List(Of PropertyGroup)) As String

            Dim sb As New StringBuilder()
            sb.Append("Group,Property,Value").Append(ControlChars.CrLf)
            For Each group As PropertyGroup In groups
                For Each row As PropertyRow In group.Rows
                    sb.Append($"{AssemblyInfoReport.ToCsvField(group.Title)},{AssemblyInfoReport.ToCsvField(row.Name)},{AssemblyInfoReport.ToCsvField(row.Value)}")
                    sb.Append(ControlChars.CrLf)
                Next row
            Next group
            Return sb.ToString()
        End Function

#End Region

    End Class

End Namespace

#End Region
