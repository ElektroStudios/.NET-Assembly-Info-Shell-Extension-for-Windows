Option Strict On
Option Explicit On
Option Infer Off

Imports System.IO
Imports System.Runtime.InteropServices
Imports AssemblyInspection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports ShellExTests.Fixtures

''' <summary>Verifies the additional metadata and host layout displayed by the property page.</summary>
<TestClass>
Public NotInheritable Class ExtendedInspectionTests

    ''' <summary>Provides a declaration with a managed alias without calling native code.</summary>
    <DllImport("fixture-native.dll", EntryPoint:="inspect_record", SetLastError:=True, CallingConvention:=CallingConvention.Cdecl)>
    Private Shared Function InspectRecord() As Integer
    End Function

    ''' <summary>Verifies the entry point and error flag are read from the declared import.</summary>
    <TestMethod>
    Public Sub NativeImportPreservesAlias()
        Dim info As New AssemblyInfo(GetType(ExtendedInspectionTests).Assembly.Location)
        Dim row As PropertyRow = info.NativeImports.Single(Function(r As PropertyRow) r.Name.EndsWith(".InspectRecord", StringComparison.Ordinal))
        StringAssert.Contains(row.Value, "fixture-native.dll!inspect_record")
        StringAssert.Contains(row.Value, "SetLastError: True")
    End Sub

    ''' <summary>Verifies that optional dependency manifests only supply explicitly published RIDs.</summary>
    <TestMethod>
    Public Sub ReadsDeclaredRuntimeIdentifier()
        Assert.AreEqual("win-arm64", AssemblyInfo.ReadPublishedRid("{""runtimeTarget"":{""name"":"".NETCoreApp,Version=v8.0/win-arm64""}}"))
        For Each json As String In {"{}", "{", "{""runtimeTarget"":{""name"":""net8.0""}}"}
            Assert.AreEqual(String.Empty, AssemblyInfo.ReadPublishedRid(json))
        Next
    End Sub

    ''' <summary>Verifies external resources and forwarded types without resolving either dependency.</summary>
    <TestMethod>
    Public Sub ManifestRecordsRetainDestination()
        Using file As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions With {.IncludeManifestDetails = True})
            Dim info As New AssemblyInfo(file.Path)
            Assert.AreEqual("File: Linked.dat", info.ManagedResources.Single().Value)
            Assert.AreEqual("Example.Widget", info.ForwardedTypes(0).Name)
            Assert.AreEqual("Example.Widget+Child", info.ForwardedTypes(1).Name)
            Assert.AreEqual("NewLibrary, Version=2.0.0.0", info.ForwardedTypes(1).Value)
        End Using
    End Sub

    ''' <summary>Checks a bound host before, during and after deployment of its companion DLL.</summary>
    <TestMethod>
    Public Sub OrdinaryHostHandlesMissingOrInvalidCompanion()
        Using host As FixtureFile = FixtureFile.CreateNamed(FixtureBundleBuilder.Build(Nothing, boundName:="Example.dll"), "Example", ".exe")
            Dim companion As String = Path.ChangeExtension(host.Path, ".dll")
            Assert.IsTrue(AssemblyInfo.IsAssemblyFile(host.Path))
            Assert.ThrowsExactly(Of FileNotFoundException)(Sub() GC.KeepAlive(New AssemblyInfo(host.Path)))
            Try
                File.WriteAllBytes(companion, FixtureAssemblyBuilder.Build(New FixtureOptions()))
                Dim info As New AssemblyInfo(host.Path)
                Assert.AreEqual(companion, info.AppHostAssembly)
                Assert.IsNull(info.Bundle)
                File.WriteAllBytes(companion, {CByte(0)})
                Assert.ThrowsExactly(Of BadImageFormatException)(Sub() GC.KeepAlive(New AssemblyInfo(host.Path)))
            Finally
                File.Delete(companion)
            End Try
        End Using
    End Sub

    ''' <summary>A same-name DLL beside an unrelated host cannot establish an apphost association.</summary>
    <TestMethod>
    Public Sub UnrelatedSiblingIsIgnored()
        Using host As FixtureFile = FixtureFile.CreateNamed(FixtureBundleBuilder.Build(Nothing, boundName:="OtherExample.dll"), "Example", ".exe")
            Using sibling As FixtureFile = FixtureFile.CreateSidecar(Path.ChangeExtension(host.Path, ".dll"), "unrelated")
                Assert.IsFalse(AssemblyInfo.IsAssemblyFile(host.Path))
            End Using
        End Using
    End Sub

    ''' <summary>Verifies displayed stored and original sizes for a compressed entry.</summary>
    <TestMethod>
    Public Sub BundleShowsCompressedEntrySizes()
        Dim entry As New FixtureBundleEntry("Example.dll", BundleFileType.Assembly, FixtureAssemblyBuilder.Build(New FixtureOptions()), True)
        Using host As FixtureFile = FixtureBundleBuilder.Create({entry}, fileName:="Example")
            Dim info As New AssemblyInfo(host.Path)
            Dim runtime As PropertyGroup = AssemblyInfoPresenter.CreateGroups(info, TimeZoneInfo.Utc).Single(Function(g As PropertyGroup) g.Title = "Runtime")
            Dim row As PropertyRow = runtime.Rows.Single(Function(r As PropertyRow) r.Name = "Bundle File: Example.dll")
            Assert.AreEqual($"Assembly, original: {entry.Content.Length:N0} bytes, stored: {info.Bundle.Entries(0).CompressedSize:N0} bytes", row.Value)
        End Using
    End Sub

    ''' <summary>Checks report alignment across aliases of different lengths.</summary>
    <TestMethod>
    Public Sub ImportViewerAlignsAllColumns()
        Dim first As String() = {"A.Short", "user32.dll", "RegisterHotKey", "WinApi", "None", "True"}
        Dim second As String() = {"A.MuchLongerManagedMethod", "dwmapi.dll", "DwmSetWindowAttribute", "WinApi", "Unicode", "False"}
        Dim rows As PropertyRow() = {New PropertyRow(first(0), first(2), first), New PropertyRow(second(0), second(2), second)}
        Dim lines As String() = AssemblyInfoReport.FormatImports(rows).Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)
        Assert.AreEqual(3, lines.Length)
        For index As Integer = 0 To first.Length - 1
            Dim column As Integer = lines(0).IndexOf(AssemblyInfoReport.ImportHeaders()(index), StringComparison.Ordinal)
            Assert.AreEqual(column, lines(1).IndexOf(first(index), StringComparison.Ordinal))
            Assert.AreEqual(column, lines(2).IndexOf(second(index), StringComparison.Ordinal))
        Next
    End Sub

    ''' <summary>Checks optional tabs, including a file with resources and no native imports.</summary>
    <TestMethod>
    Public Sub OptionalGroupsOnlyAppearWhenPopulated()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions With {.IncludeManifestDetails = True})
            Dim groups As IEnumerable(Of PropertyGroup) = AssemblyInfoPresenter.CreateGroups(New AssemblyInfo(fixture.Path), TimeZoneInfo.Utc)
            Assert.IsFalse(groups.Any(Function(g As PropertyGroup) g.Title = PropertyNames.GroupImports))
            Assert.AreEqual("Data", groups.Single(Function(g As PropertyGroup) g.Title = PropertyNames.GroupResources).Rows.Single().Name)
            Assert.IsFalse(groups.Single(Function(g As PropertyGroup) g.Title = PropertyNames.GroupAttributes).Rows.Any(Function(r As PropertyRow) r.Name = "Data"))
        End Using
        Dim own As New AssemblyInfo(GetType(ExtendedInspectionTests).Assembly.Location)
        Dim importGroup As PropertyGroup = AssemblyInfoPresenter.CreateGroups(own, TimeZoneInfo.Utc).Single(Function(g As PropertyGroup) g.Title = PropertyNames.GroupImports)
        Assert.IsTrue(importGroup.Rows.Any(Function(r As PropertyRow) r.Name.EndsWith(".InspectRecord", StringComparison.Ordinal)))
    End Sub

    ''' <summary>Checks Windows save suggestions for tabs, embedded paths and reserved names.</summary>
    <TestMethod>
    Public Sub SaveSuggestionsAreValidWindowsFileNames()
        Assert.AreEqual("MouseLock.exe.PInvokeImports.txt", AssemblyInfoReport.SafeFileName("MouseLock.exe.PInvokeImports.txt"))
        Assert.AreEqual("MouseLock.exe (P_Invoke).txt", AssemblyInfoReport.SafeFileName("MouseLock.exe (P/Invoke).txt"))
        For Each invalid As Char In "<>:""/\|?*" & ChrW(0) & ChrW(31)
            Assert.AreEqual("a_b.txt", AssemblyInfoReport.SafeFileName("a" & invalid & "b.txt"))
        Next
        Assert.AreEqual("_COM1.txt", AssemblyInfoReport.SafeFileName("COM1.txt. "))
        Assert.AreEqual("Report.txt", AssemblyInfoReport.SafeFileName("..."))
        Assert.AreEqual(255, AssemblyInfoReport.SafeFileName(New String("a"c, 300) & ".xml").Length)
    End Sub

End Class
