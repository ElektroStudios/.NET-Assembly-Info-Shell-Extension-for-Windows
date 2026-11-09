
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Reflection.PortableExecutable

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoPresenterTests "

''' <summary>
''' Tests the conversion of <see cref="AssemblyInfo"/> into the property groups shown in the property page.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoPresenterTests

#Region " Private Fields "

    ''' <summary>A fixed UTC+02:00 time zone, so the tests do not depend on the machine time zone.</summary>
    Private Shared ReadOnly UtcPlus2 As TimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone("Test+02", TimeSpan.FromHours(2), "Test+02", "Test+02")

#End Region

#Region " CreateGroups "

    <TestMethod>
    Public Sub CreateGroups_NullArguments_ThrowArgumentNullException()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoPresenter.CreateGroups(Nothing, TimeZoneInfo.Utc))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoPresenter.CreateGroups(info, Nothing))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ReturnsTheFourGroupsInTabOrder()
        Dim groups As ReadOnlyCollection(Of PropertyGroup) = AssemblyInfoPresenter.CreateGroups(TestHelpers.Inspect(Nothing), TimeZoneInfo.Utc)

        CollectionAssert.AreEqual(
            {PropertyNames.GroupProduct, PropertyNames.GroupBuild, PropertyNames.GroupIdentity, PropertyNames.GroupReferences},
            groups.Select(Function(g As PropertyGroup) g.Title).ToArray())
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ProductGroup_HasRowsInDisplayOrder()
        Dim product As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(TestHelpers.Inspect(Nothing), PropertyNames.GroupProduct)

        CollectionAssert.AreEqual(
            {PropertyNames.FileName, PropertyNames.OriginalFileName, PropertyNames.ProductName, PropertyNames.FileDescription,
             PropertyNames.Comments, PropertyNames.Company, PropertyNames.Copyright, PropertyNames.Trademarks,
             PropertyNames.ProductVersion, PropertyNames.FileVersion},
            product.Rows.Select(Function(r As PropertyRow) r.Name).ToArray())
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ProductGroup_UsesTheVersionResource()
        ' Regression: the "File Version" row used to show the assembly version instead of the file version.
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)
        Dim product As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupProduct)
        Dim version As FileVersionInfo = FileVersionInfo.GetVersionInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual(Path.GetFileName(TestHelpers.TestAssemblyPath), product.GetValue(PropertyNames.FileName))
        Assert.AreEqual(AssemblyInfoPresenter.FormatText(version.FileVersion), product.GetValue(PropertyNames.FileVersion))
        Assert.AreEqual(AssemblyInfoPresenter.FormatText(version.ProductVersion), product.GetValue(PropertyNames.ProductVersion))
        Assert.AreEqual(AssemblyInfoPresenter.FormatText(version.CompanyName), product.GetValue(PropertyNames.Company))
        Assert.AreEqual(AssemblyInfoPresenter.FormatText(version.LegalTrademarks), product.GetValue(PropertyNames.Trademarks))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_BuildGroup_HasRowsInDisplayOrder()
        Dim build As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(TestHelpers.Inspect(Nothing), PropertyNames.GroupBuild)

        CollectionAssert.AreEqual(
            {PropertyNames.OutputType, PropertyNames.Platform, PropertyNames.TargetFramework, PropertyNames.TargetOsPlatform,
             PropertyNames.ClrVersion, PropertyNames.BuildConfiguration, PropertyNames.CodeOptimizations, PropertyNames.DeterministicBuild,
             PropertyNames.CompilationTimeStamp, PropertyNames.DebugSymbols, PropertyNames.EntryPoint, PropertyNames.MixedMode,
             PropertyNames.ReadyToRun, PropertyNames.ReferenceAssembly, PropertyNames.RootNamespace, PropertyNames.NeutralResourcesLanguage,
             PropertyNames.ClsCompliant, PropertyNames.ComVisible, PropertyNames.DpiAwarenessDisabled, PropertyNames.DllImportSearchPaths,
             PropertyNames.SecurityPermissions},
            build.Rows.Select(Function(r As PropertyRow) r.Name).ToArray())
    End Sub

    <TestMethod>
    Public Sub CreateGroups_BuildGroup_FormatsValues()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Machine = Machine.Amd64
                o.ImageKind = FixtureImageKind.WindowsExe
                o.EntryPointTypeName = "Contoso.Program"
                o.Attributes.Add(FixtureAttribute.TargetFramework(".NETFramework,Version=v4.8"))
                o.Attributes.Add(FixtureAttribute.NeutralResourcesLanguage("en-US"))
                o.Attributes.Add(FixtureAttribute.ComVisible(False))
            End Sub)

        Dim build As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupBuild, AssemblyInfoPresenterTests.UtcPlus2)

        Assert.AreEqual("Windows Application (EXE)", build.GetValue(PropertyNames.OutputType))
        Assert.AreEqual("x64 (64-bit)", build.GetValue(PropertyNames.Platform))
        Assert.AreEqual(".NET Framework 4.8", build.GetValue(PropertyNames.TargetFramework))
        Assert.AreEqual(PropertyNames.NoneValue, build.GetValue(PropertyNames.TargetOsPlatform))
        Assert.AreEqual("4.0.30319", build.GetValue(PropertyNames.ClrVersion))
        Assert.AreEqual("Release", build.GetValue(PropertyNames.BuildConfiguration))
        Assert.AreEqual("Enabled", build.GetValue(PropertyNames.CodeOptimizations))
        Assert.AreEqual(PropertyNames.NoValue, build.GetValue(PropertyNames.DeterministicBuild))
        Assert.AreEqual("05/November/2018 01:00:31 UTC+02:00", build.GetValue(PropertyNames.CompilationTimeStamp))
        Assert.AreEqual(PropertyNames.NoneValue, build.GetValue(PropertyNames.DebugSymbols))
        Assert.AreEqual("Contoso.Program.Main", build.GetValue(PropertyNames.EntryPoint))
        Assert.AreEqual(PropertyNames.NoValue, build.GetValue(PropertyNames.MixedMode))
        Assert.AreEqual("Contoso", build.GetValue(PropertyNames.RootNamespace))
        Assert.AreEqual($"{CultureInfo.GetCultureInfo("en-US").EnglishName} (en-US)", build.GetValue(PropertyNames.NeutralResourcesLanguage))
        Assert.AreEqual(PropertyNames.NotSpecifiedValue, build.GetValue(PropertyNames.ClsCompliant))
        Assert.AreEqual(PropertyNames.NoValue, build.GetValue(PropertyNames.ComVisible))
        Assert.AreEqual("(default)", build.GetValue(PropertyNames.DllImportSearchPaths))
        Assert.AreEqual(PropertyNames.NoneValue, build.GetValue(PropertyNames.SecurityPermissions))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ComVisibleNotSpecified_IsYesByDefault()
        ' Regression: a missing ComVisibleAttribute was shown as "No", but public types are COM-visible by default.
        Dim build As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(TestHelpers.Inspect(Nothing), PropertyNames.GroupBuild)

        Assert.AreEqual("Yes (default)", build.GetValue(PropertyNames.ComVisible))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_IdentityGroup_NetFramework_IncludesGacRows()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.TargetFramework(".NETFramework,Version=v4.8")))
        Dim identity As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupIdentity)

        Assert.AreEqual(PropertyNames.NoValue, identity.GetValue(PropertyNames.InstalledInGac))
        Assert.AreEqual(PropertyNames.NoneValue, identity.GetValue(PropertyNames.GacName))
        Assert.AreEqual(PropertyNames.NoneValue, identity.GetValue(PropertyNames.GacPath))
    End Sub

    <TestMethod>
    <DataRow(".NETCoreApp,Version=v8.0")>
    <DataRow(".NETStandard,Version=v2.0")>
    Public Sub CreateGroups_IdentityGroup_NotNetFramework_ExcludesGacRows(frameworkName As String)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.TargetFramework(frameworkName)))
        Dim identity As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupIdentity)

        Assert.IsFalse(identity.ContainsRow(PropertyNames.InstalledInGac))
        Assert.IsFalse(identity.ContainsRow(PropertyNames.GacName))
        Assert.IsFalse(identity.ContainsRow(PropertyNames.GacPath))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_IdentityGroup_FormatsValues()
        Dim mvid As New Guid("0f8fad5b-d9cb-469f-a165-70867728950e")
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.AssemblyName = "Contoso.Signed"
                o.Version = New Version(2, 0, 0, 0)
                o.PublicKey = FixtureOptions.EcmaStandardPublicKey
                o.CorFlags = CorFlags.ILOnly Or CorFlags.StrongNameSigned
                o.Mvid = mvid
                o.Attributes.Add(FixtureAttribute.Guid("b9adb13a-bc56-4615-8213-9a055d0b1f0f"))
            End Sub)

        Dim identity As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupIdentity)

        Assert.AreEqual("Contoso.Signed", identity.GetValue(PropertyNames.AssemblyName))
        Assert.AreEqual("2.0.0.0", identity.GetValue(PropertyNames.AssemblyVersion))
        Assert.AreEqual("neutral", identity.GetValue(PropertyNames.Culture))
        Assert.AreEqual("Contoso.Signed, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", identity.GetValue(PropertyNames.FullName))
        Assert.AreEqual("Signed", identity.GetValue(PropertyNames.StrongNameSignature))
        Assert.AreEqual("B77A5C561934E089", identity.GetValue(PropertyNames.PublicKeyToken))
        Assert.AreEqual("B9ADB13A-BC56-4615-8213-9A055D0B1F0F", identity.GetValue(PropertyNames.AssemblyGuid))
        Assert.AreEqual("0F8FAD5B-D9CB-469F-A165-70867728950E", identity.GetValue(PropertyNames.ModuleVersionId))
        Assert.AreEqual(info.SHA256Hash, identity.GetValue(PropertyNames.SHA256Hash))
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ReferencesGroup_ListsAssembliesThenNativeModules()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.Mscorlib
                o.NativeModules.Add("kernel32.dll")
            End Sub)

        Dim references As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupReferences)

        Assert.AreEqual(2, references.Rows.Count)
        Assert.AreEqual("mscorlib", references.Rows(0).Name)
        Assert.AreEqual("4.0.0.0", references.Rows(0).Value)
        Assert.AreEqual("kernel32.dll", references.Rows(1).Name)
        Assert.AreEqual(PropertyNames.NativeLibraryValue, references.Rows(1).Value)
    End Sub

    <TestMethod>
    Public Sub CreateGroups_ReferencesGroup_NoReferences_HasPlaceholderRow()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.CoreLibrary = FixtureCoreLibrary.None)
        Dim references As PropertyGroup = AssemblyInfoPresenterTests.GetGroup(info, PropertyNames.GroupReferences)

        Assert.AreEqual(1, references.Rows.Count)
        Assert.AreEqual(PropertyNames.NoReferences, references.Rows(0).Name)
        Assert.AreEqual(String.Empty, references.Rows(0).Value)
    End Sub

#End Region

#Region " CreateErrorGroups "

    <TestMethod>
    Public Sub CreateErrorGroups_ShowsTheErrorInTheFirstGroup()
        Dim filePath As String = Path.Combine(Path.GetTempPath(), "Folder", "Broken.dll")
        Dim groups As ReadOnlyCollection(Of PropertyGroup) = AssemblyInfoPresenter.CreateErrorGroups(filePath, New BadImageFormatException("Bad image."))

        Assert.AreEqual(4, groups.Count)
        Assert.AreEqual("Broken.dll", groups(0).GetValue(PropertyNames.FileName))
        Assert.AreEqual("BadImageFormatException: Bad image.", groups(0).GetValue(PropertyNames.ErrorRow))
        Assert.IsTrue(groups.Skip(1).All(Function(g As PropertyGroup) g.Rows.Count = 0))
    End Sub

    <TestMethod>
    Public Sub CreateErrorGroups_NullArguments_ThrowArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoPresenter.CreateErrorGroups(Nothing, New IOException()))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoPresenter.CreateErrorGroups("C:\File.dll", Nothing))
    End Sub

#End Region

#Region " Formatting "

    <TestMethod>
    Public Sub FormatBoolean_ReturnsYesOrNo()
        Assert.AreEqual("Yes", AssemblyInfoPresenter.FormatBoolean(True))
        Assert.AreEqual("No", AssemblyInfoPresenter.FormatBoolean(False))
    End Sub

    <TestMethod>
    <DataRow("", "(none)")>
    <DataRow("   ", "(none)")>
    <DataRow("  value  ", "value")>
    Public Sub FormatText_ReplacesEmptyValues(value As String, expected As String)
        Assert.AreEqual(expected, AssemblyInfoPresenter.FormatText(value))
    End Sub

    <TestMethod>
    Public Sub FormatText_Nothing_IsNone()
        Assert.AreEqual(PropertyNames.NoneValue, AssemblyInfoPresenter.FormatText(Nothing))
    End Sub

    <TestMethod>
    Public Sub FormatTimeStamp_ConvertsToTheSpecifiedTimeZone()
        Dim timeStamp As New DateTimeOffset(2018, 11, 4, 23, 0, 31, TimeSpan.Zero)

        Assert.AreEqual("05/November/2018 01:00:31 UTC+02:00", AssemblyInfoPresenter.FormatTimeStamp(timeStamp, False, AssemblyInfoPresenterTests.UtcPlus2))
        Assert.AreEqual("04/November/2018 23:00:31 UTC+00:00", AssemblyInfoPresenter.FormatTimeStamp(timeStamp, False, TimeZoneInfo.Utc))
    End Sub

    <TestMethod>
    Public Sub FormatTimeStamp_Deterministic_IsNotAvailable()
        Assert.AreEqual("(not available: deterministic build)", AssemblyInfoPresenter.FormatTimeStamp(Nothing, True, TimeZoneInfo.Utc))
    End Sub

    <TestMethod>
    Public Sub FormatTimeStamp_NoValue_IsNone()
        Assert.AreEqual(PropertyNames.NoneValue, AssemblyInfoPresenter.FormatTimeStamp(Nothing, False, TimeZoneInfo.Utc))
    End Sub

    <TestMethod>
    Public Sub FormatTimeStamp_NullTimeZone_ThrowsArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfoPresenter.FormatTimeStamp(DateTimeOffset.UtcNow, False, Nothing))
    End Sub

    <TestMethod>
    Public Sub FormatDebugSymbols_CombinesEmbeddedAndCodeView()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PdbPath = "Contoso.pdb"
                o.PortablePdb = True
                o.EmbeddedPdb = True
            End Sub)

        Assert.AreEqual("Embedded portable PDB; Contoso.pdb (portable)", AssemblyInfoPresenter.FormatDebugSymbols(info))
    End Sub

    <TestMethod>
    Public Sub FormatDebugSymbols_WindowsPdb_IsLabeled()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PdbPath = "C:\obj\Contoso.pdb"
                o.PortablePdb = False
            End Sub)

        Assert.AreEqual("C:\obj\Contoso.pdb (Windows PDB)", AssemblyInfoPresenter.FormatDebugSymbols(info))
    End Sub

    <TestMethod>
    Public Sub FormatCulture_FormatsKnownUnknownAndMissingCultures()
        Dim culture As CultureInfo = CultureInfo.GetCultureInfo("es-ES")

        Assert.AreEqual($"{culture.EnglishName} (es-ES)", AssemblyInfoPresenter.FormatCulture("es-ES", culture))
        Assert.AreEqual("zz-ZZ (unknown culture)", AssemblyInfoPresenter.FormatCulture("zz-ZZ", Nothing))
        Assert.AreEqual(PropertyNames.NoneValue, AssemblyInfoPresenter.FormatCulture(String.Empty, Nothing))
    End Sub

    <TestMethod>
    Public Sub FormatStrongNameSignature_FormatsEveryState()
        Assert.AreEqual("Signed", AssemblyInfoPresenter.FormatStrongNameSignature(StrongNameSignatureKind.Signed))
        Assert.AreEqual("Delay-signed (public key only)", AssemblyInfoPresenter.FormatStrongNameSignature(StrongNameSignatureKind.DelaySigned))
        Assert.AreEqual("Not signed", AssemblyInfoPresenter.FormatStrongNameSignature(StrongNameSignatureKind.NotSigned))
        Assert.ThrowsExactly(Of InvalidEnumArgumentException)(Sub() AssemblyInfoPresenter.FormatStrongNameSignature(CType(99, StrongNameSignatureKind)))
    End Sub

    <TestMethod>
    Public Sub FormatReference_IncludesCultureWhenPresent()
        Dim neutral As New AssemblyName("Contoso, Version=1.2.3.4")
        Dim satellite As New AssemblyName("Contoso.resources, Version=1.2.3.4, Culture=es-ES")

        Assert.AreEqual("1.2.3.4", AssemblyInfoPresenter.FormatReference(neutral))
        Assert.AreEqual("1.2.3.4, es-ES", AssemblyInfoPresenter.FormatReference(satellite))
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>Gets the group with the specified title, using UTC for time stamps.</summary>
    Private Shared Function GetGroup(info As AssemblyInfo, title As String) As PropertyGroup
        Return AssemblyInfoPresenterTests.GetGroup(info, title, TimeZoneInfo.Utc)
    End Function

    ''' <summary>Gets the group with the specified title.</summary>
    Private Shared Function GetGroup(info As AssemblyInfo, title As String, timeZone As TimeZoneInfo) As PropertyGroup
        Return AssemblyInfoPresenter.CreateGroups(info, timeZone).Single(Function(g As PropertyGroup) g.Title = title)
    End Function

#End Region

End Class

#End Region
