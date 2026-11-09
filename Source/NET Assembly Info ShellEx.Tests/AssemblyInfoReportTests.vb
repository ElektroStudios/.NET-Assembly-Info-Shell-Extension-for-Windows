
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

#Region " AssemblyInfoReportTests "

''' <summary>
''' Tests the plain-text report exported by the property page.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoReportTests

    <TestMethod>
    Public Sub Build_NullArguments_ThrowArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoReport.Build(Nothing, Enumerable.Empty(Of PropertyGroup)()))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoReport.Build("C:\File.dll", Nothing))
    End Sub

    <TestMethod>
    Public Sub Build_WritesHeaderTitlesUnderlinesAndAlignedRows()
        Dim groups As PropertyGroup() = {
            New PropertyGroup("Product", {New PropertyRow("File Name", "File.dll"), New PropertyRow("Company", "ElektroStudios")}),
            New PropertyGroup("Build", {New PropertyRow("Platform", "Any CPU")})
        }

        Dim expected As String = String.Join(Environment.NewLine,
            "C:\File.dll",
            "",
            "Product",
            "-------",
            "File Name: File.dll",
            "Company  : ElektroStudios",
            "",
            "Build",
            "-----",
            "Platform: Any CPU",
            "",
            "")

        Assert.AreEqual(expected, AssemblyInfoReport.Build("C:\File.dll", groups))
    End Sub

    <TestMethod>
    Public Sub Build_SkipsEmptyGroups()
        Dim groups As PropertyGroup() = {
            New PropertyGroup("Product", {New PropertyRow("Error", "Bad image.")}),
            New PropertyGroup("Build", Enumerable.Empty(Of PropertyRow)())
        }

        Dim report As String = AssemblyInfoReport.Build("C:\File.dll", groups)

        Assert.IsFalse(report.Contains("Build"))
        StringAssert.Contains(report, "Error: Bad image.")
    End Sub

    <TestMethod>
    Public Sub Build_EmptyValues_HaveNoTrailingWhiteSpace()
        Dim groups As PropertyGroup() = {
            New PropertyGroup("References", {New PropertyRow(PropertyNames.NoReferences, String.Empty)})
        }

        Dim report As String = AssemblyInfoReport.Build("C:\File.dll", groups)

        StringAssert.Contains(report, $"{PropertyNames.NoReferences}:{Environment.NewLine}")
    End Sub

    <TestMethod>
    Public Sub Build_FromRealAssembly_ContainsEveryRowOfEveryGroup()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)
        Dim groups As ReadOnlyCollection(Of PropertyGroup) = AssemblyInfoPresenter.CreateGroups(info, TimeZoneInfo.Utc)

        Dim report As String = AssemblyInfoReport.Build(info.FilePath, groups)

        StringAssert.StartsWith(report, info.FilePath)
        For Each group As PropertyGroup In groups
            StringAssert.Contains(report, $"{group.Title}{Environment.NewLine}{New String("-"c, group.Title.Length)}")
            For Each row As PropertyRow In group.Rows
                StringAssert.Contains(report, row.Name)
                StringAssert.Contains(report, row.Value)
            Next row
        Next group
    End Sub

End Class

#End Region
