
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " ReportAndComparisonTests "

''' <summary>
''' Tests the export formats (Markdown, HTML, JSON, XML, CSV), the assembly comparison
''' and the COM registry key location.
''' </summary>
<TestClass>
Public NotInheritable Class ReportAndComparisonTests

#Region " Helpers "

    ''' <summary>Property groups with values that need escaping in every format.</summary>
    Private Shared Function CreateTrickyGroups() As PropertyGroup()
        Return {
            New PropertyGroup("Product", {New PropertyRow("File Name", "a|b_c*d.dll"), New PropertyRow("Copyright", "© <Contoso> & ""Partners"", Inc.")}),
            New PropertyGroup("Empty", Enumerable.Empty(Of PropertyRow)()),
            New PropertyGroup("Build", {New PropertyRow("Path", $"C:\Folder\Sub{ControlChars.Tab}Tab{ControlChars.Lf}Line")})
        }
    End Function

#End Region

#Region " Formats "

    <TestMethod>
    Public Sub Build_Json_IsValidAndRoundTrips()
        Dim json As String = AssemblyInfoReport.Build("C:\File ""1"".dll", ReportAndComparisonTests.CreateTrickyGroups(), ReportFormat.Json)

        Dim root As XElement = JsonDocumentReader.Parse(json)
        Assert.AreEqual("C:\File ""1"".dll", JsonDocumentReader.GetMemberValue(root, "file"))

        Dim groups As List(Of XElement) = JsonDocumentReader.GetMember(root, "groups").Elements().ToList()
        Assert.AreEqual(2, groups.Count) ' The empty group is skipped.
        Assert.AreEqual("Build", JsonDocumentReader.GetMemberValue(groups(1), "title"))

        Dim firstRows As List(Of XElement) = JsonDocumentReader.GetMember(groups(0), "rows").Elements().ToList()
        Assert.AreEqual("© <Contoso> & ""Partners"", Inc.", JsonDocumentReader.GetMemberValue(firstRows(1), "value"))
        Dim pathRow As XElement = JsonDocumentReader.GetMember(groups(1), "rows").Elements().Single()
        Assert.AreEqual($"C:\Folder\Sub{ControlChars.Tab}Tab{ControlChars.Lf}Line", JsonDocumentReader.GetMemberValue(pathRow, "value"))
    End Sub

    <TestMethod>
    Public Sub Build_Xml_IsValidAndRoundTrips()
        Dim xml As String = AssemblyInfoReport.Build("C:\File.dll", ReportAndComparisonTests.CreateTrickyGroups(), ReportFormat.Xml)

        StringAssert.StartsWith(xml, "<?xml version=""1.0"" encoding=""utf-8""?>")
        Dim document As XDocument = XDocument.Parse(xml)
        Assert.AreEqual("C:\File.dll", document.Root.Attribute("file").Value)
        Assert.AreEqual(2, document.Root.Elements("group").Count())
        Dim copyright As XElement = document.Root.Descendants("property").Single(Function(e As XElement) e.Attribute("name").Value = "Copyright")
        Assert.AreEqual("© <Contoso> & ""Partners"", Inc.", copyright.Value)
    End Sub

    <TestMethod>
    Public Sub Build_Markdown_EscapesSpecialCharacters()
        Dim filePath As String = IO.Path.Combine(IO.Path.GetTempPath(), "File.dll")
        Dim markdown As String = AssemblyInfoReport.Build(filePath, ReportAndComparisonTests.CreateTrickyGroups(), ReportFormat.Markdown)

        StringAssert.StartsWith(markdown, "# File.dll")
        StringAssert.Contains(markdown, "## Product")
        StringAssert.Contains(markdown, "| Property | Value |")
        StringAssert.Contains(markdown, "| File Name | a\|b\_c\*d.dll |")
        StringAssert.Contains(markdown, "\<Contoso\>")
        StringAssert.Contains(markdown, "Tab<br>Line")
        Assert.IsFalse(markdown.Contains("## Empty"))
    End Sub

    <TestMethod>
    Public Sub Build_Html_EncodesValues()
        Dim html As String = AssemblyInfoReport.Build("C:\File.dll", ReportAndComparisonTests.CreateTrickyGroups(), ReportFormat.Html)

        StringAssert.StartsWith(html, "<!DOCTYPE html>")
        StringAssert.Contains(html, "<meta charset=""utf-8"">")
        StringAssert.Contains(html, "&lt;Contoso&gt; &amp; &quot;Partners&quot;")
        StringAssert.Contains(html, "<h2>Build</h2>")
        Assert.IsFalse(html.Contains("<Contoso>"))
    End Sub

    <TestMethod>
    Public Sub Build_Csv_QuotesFieldsPerRfc4180()
        Dim csv As String = AssemblyInfoReport.Build("C:\File.dll", ReportAndComparisonTests.CreateTrickyGroups(), ReportFormat.Csv)
        Dim lines As String() = csv.Split({ControlChars.CrLf}, StringSplitOptions.None)

        Assert.AreEqual("Group,Property,Value", lines(0))
        Assert.AreEqual("Product,File Name,a|b_c*d.dll", lines(1))
        Assert.AreEqual("Product,Copyright,""© <Contoso> & """"Partners"""", Inc.""", lines(2))
        StringAssert.StartsWith(lines(3), "Build,Path,""C:\Folder\Sub")
    End Sub

    <TestMethod>
    Public Sub Build_TextFormat_MatchesTheDefaultOverload()
        Dim groups As PropertyGroup() = ReportAndComparisonTests.CreateTrickyGroups()

        Assert.AreEqual(AssemblyInfoReport.Build("C:\File.dll", groups), AssemblyInfoReport.Build("C:\File.dll", groups, ReportFormat.Text))
    End Sub

    <TestMethod>
    Public Sub Build_InvalidFormat_ThrowsInvalidEnumArgumentException()
        Assert.ThrowsExactly(Of System.ComponentModel.InvalidEnumArgumentException)(
            Sub() AssemblyInfoReport.Build("C:\File.dll", ReportAndComparisonTests.CreateTrickyGroups(), CType(99, ReportFormat)))
    End Sub

    <TestMethod>
    <DataRow("report.txt", CInt(ReportFormat.Text))>
    <DataRow("report.MD", CInt(ReportFormat.Markdown))>
    <DataRow("report.htm", CInt(ReportFormat.Html))>
    <DataRow("report.html", CInt(ReportFormat.Html))>
    <DataRow("report.json", CInt(ReportFormat.Json))>
    <DataRow("report.xml", CInt(ReportFormat.Xml))>
    <DataRow("report.csv", CInt(ReportFormat.Csv))>
    <DataRow("report.dll.unknown", CInt(ReportFormat.Csv))>
    Public Sub GetFormatFromExtension_MapsKnownExtensions(fileName As String, expected As Integer)
        Assert.AreEqual(CType(expected, ReportFormat), AssemblyInfoReport.GetFormatFromExtension(fileName, ReportFormat.Csv))
    End Sub

    <TestMethod>
    Public Sub SaveFileDialogFilter_HasOneEntryPerFormatInEnumOrder()
        Dim parts As String() = AssemblyInfoReport.SaveFileDialogFilter.Split("|"c)
        Dim formats As Array = [Enum].GetValues(GetType(ReportFormat))

        Assert.AreEqual(formats.Length * 2, parts.Length)
        For i As Integer = 0 To formats.Length - 1
            Dim extension As String = parts((i * 2) + 1).TrimStart("*"c)
            Assert.AreEqual(CType(formats.GetValue(i), ReportFormat), AssemblyInfoReport.GetFormatFromExtension($"x{extension}", CType(99, ReportFormat)))
        Next i
    End Sub

    <TestMethod>
    Public Sub Build_EveryFormat_FromRealAssembly_ContainsEveryValue()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)
        Dim groups As ReadOnlyCollection(Of PropertyGroup) = AssemblyInfoPresenter.CreateGroups(info, TimeZoneInfo.Utc)

        For Each format As ReportFormat In [Enum].GetValues(GetType(ReportFormat)).Cast(Of ReportFormat)()
            Dim report As String = AssemblyInfoReport.Build(info.FilePath, groups, format)
            StringAssert.Contains(report, info.SHA256Hash, $"Format: {format}")
            StringAssert.Contains(report, info.ModuleVersionId.ToString("D").ToUpperInvariant(), $"Format: {format}")
        Next format
    End Sub

#End Region

#Region " Comparison "

    <TestMethod>
    Public Sub Compare_MatchesRowsByNameAndOccurrence()
        Dim left As PropertyGroup() = {
            New PropertyGroup("Identity", {New PropertyRow("Version", "1.0"), New PropertyRow("Name", "A")}),
            New PropertyGroup("Attributes", {New PropertyRow("Friend", "X"), New PropertyRow("Friend", "Y"), New PropertyRow("Friend", "Z")})
        }
        Dim right As PropertyGroup() = {
            New PropertyGroup("Identity", {New PropertyRow("Name", "A"), New PropertyRow("Version", "2.0"), New PropertyRow("Culture", "neutral")}),
            New PropertyGroup("Attributes", {New PropertyRow("Friend", "X"), New PropertyRow("Friend", "W")}),
            New PropertyGroup("Runtime", {New PropertyRow("Bundle", "No")})
        }

        Dim result As ReadOnlyCollection(Of PropertyComparison) = AssemblyInfoComparer.Compare(left, right)

        Dim summary As String() = result.Select(Function(c As PropertyComparison) $"{c.GroupTitle}/{c.Name}={c.Result}").ToArray()
        CollectionAssert.AreEqual({
            "Identity/Version=Different", "Identity/Name=Equal", "Identity/Culture=OnlyInRight",
            "Attributes/Friend=Equal", "Attributes/Friend=Different", "Attributes/Friend=OnlyInLeft",
            "Runtime/Bundle=OnlyInRight"
        }, summary)

        Assert.AreEqual("1.0", result(0).LeftValue)
        Assert.AreEqual("2.0", result(0).RightValue)
        Assert.IsNull(result(2).LeftValue)
        Assert.IsNull(result(5).RightValue)
    End Sub

    <TestMethod>
    Public Sub Compare_SameAssembly_IsAllEqual()
        Dim groups As ReadOnlyCollection(Of PropertyGroup) = AssemblyInfoPresenter.CreateGroups(New AssemblyInfo(TestHelpers.TestAssemblyPath), TimeZoneInfo.Utc)

        Dim result As ReadOnlyCollection(Of PropertyComparison) = AssemblyInfoComparer.Compare(groups, groups)

        Assert.AreEqual(groups.Sum(Function(g As PropertyGroup) g.Rows.Count), result.Count)
        Assert.IsTrue(result.All(Function(c As PropertyComparison) c.Result = ComparisonResult.Equal))
    End Sub

    <TestMethod>
    Public Sub Compare_NullArguments_ThrowArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoComparer.Compare(Nothing, Enumerable.Empty(Of PropertyGroup)()))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoComparer.Compare(Enumerable.Empty(Of PropertyGroup)(), Nothing))
    End Sub

#End Region

#Region " COM Registry "

    <TestMethod>
    Public Sub GetRegisteredKeys_OnlyReturnsExistingKeys_TypeLibraryFirst()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As Fixtures.FixtureOptions)
                o.Attributes.Add(Fixtures.FixtureAttribute.Guid("b9adb13a-bc56-4615-8213-9a055d0b1f0f"))
                o.TypeGuids.Add("Contoso.Registered", "8a4d2f1e-3c5b-4a69-9e7d-1f2a3b4c5d6e")
                o.TypeGuids.Add("Contoso.NotRegistered", "11111111-2222-3333-4444-555555555555")
            End Sub)

        Dim existing As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            "TypeLib\{B9ADB13A-BC56-4615-8213-9A055D0B1F0F}",
            "CLSID\{8A4D2F1E-3C5B-4A69-9E7D-1F2A3B4C5D6E}"
        }

        Dim keys As ReadOnlyCollection(Of ComRegistryKey) = ComRegistryLocator.GetRegisteredKeys(info, AddressOf existing.Contains)

        Assert.AreEqual(2, keys.Count)
        Assert.AreEqual("HKEY_CLASSES_ROOT\TypeLib\{B9ADB13A-BC56-4615-8213-9A055D0B1F0F}", keys(0).KeyPath)
        Assert.AreEqual("HKEY_CLASSES_ROOT\CLSID\{8A4D2F1E-3C5B-4A69-9E7D-1F2A3B4C5D6E}", keys(1).KeyPath)
        StringAssert.Contains(keys(1).Description, "Contoso.Registered")
    End Sub

    <TestMethod>
    Public Sub GetRegisteredKeys_NotRegistered_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As Fixtures.FixtureOptions) o.Attributes.Add(Fixtures.FixtureAttribute.Guid("b9adb13a-bc56-4615-8213-9a055d0b1f0f")))

        Assert.AreEqual(0, ComRegistryLocator.GetRegisteredKeys(info, Function(subKey As String) False).Count)
    End Sub

    <TestMethod>
    <DataRow(Nothing, "Computer\HKEY_CLASSES_ROOT\CLSID\{X}")>
    <DataRow("", "Computer\HKEY_CLASSES_ROOT\CLSID\{X}")>
    <DataRow("Equipo\HKEY_LOCAL_MACHINE\SOFTWARE", "Equipo\HKEY_CLASSES_ROOT\CLSID\{X}")>
    <DataRow("Ordinateur", "Ordinateur\HKEY_CLASSES_ROOT\CLSID\{X}")>
    <DataRow("HKEY_CURRENT_USER\Software", "HKEY_CLASSES_ROOT\CLSID\{X}")>
    Public Sub BuildRegeditLastKey_KeepsTheLocalizedRootName(currentLastKey As String, expected As String)
        Assert.AreEqual(expected, ComRegistryLocator.BuildRegeditLastKey(currentLastKey, "HKEY_CLASSES_ROOT\CLSID\{X}"))
    End Sub

#End Region

End Class

#End Region
