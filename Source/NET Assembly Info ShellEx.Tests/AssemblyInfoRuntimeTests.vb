
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Text

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoRuntimeTests "

''' <summary>
''' Tests the Win32 manifest resource, the sidecar configuration files, the new assembly attributes
''' (AssemblyMetadata, InternalsVisibleTo, key file/name, delay sign), the COM classes and their presentation.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoRuntimeTests

#Region " Helpers "

    ''' <summary>Gets the group with the specified title.</summary>
    Private Shared Function GetGroup(info As AssemblyInfo, title As String) As PropertyGroup
        Return AssemblyInfoPresenter.CreateGroups(info, TimeZoneInfo.Utc).Single(Function(g As PropertyGroup) g.Title = title)
    End Function

#End Region

#Region " Win32 Manifest "

    <TestMethod>
    Public Sub Win32Manifest_NoResources_IsNothing()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.IsNull(info.Win32Manifest)
        Assert.AreEqual(PropertyNames.NoneValue, AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime).GetValue(PropertyNames.Win32Manifest))
    End Sub

    <TestMethod>
    <DataRow(1)>
    <DataRow(2)>
    Public Sub Win32Manifest_EmbeddedResource_IsReadAndParsed(resourceId As Integer)
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Win32Manifest = Encoding.UTF8.GetBytes(ConfigurationParsingTests.SampleManifest)
                o.Win32ManifestId = resourceId
            End Sub)

        Assert.IsNotNull(info.Win32Manifest)
        Assert.AreEqual(resourceId, info.Win32Manifest.ResourceId)
        Assert.AreEqual("requireAdministrator", info.Win32Manifest.RequestedExecutionLevel)
        StringAssert.Contains(info.Win32Manifest.Xml, "Microsoft.Windows.Common-Controls")
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_RuntimeGroup_ShowsTheManifestSettings()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Win32Manifest = Encoding.UTF8.GetBytes(ConfigurationParsingTests.SampleManifest))
        Dim runtime As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime)

        Assert.AreEqual("Yes (resource ID 1)", runtime.GetValue(PropertyNames.Win32Manifest))
        Assert.AreEqual("requireAdministrator", runtime.GetValue(PropertyNames.ExecutionLevel))
        Assert.AreEqual("false", runtime.GetValue(PropertyNames.UIAccess))
        Assert.AreEqual("PerMonitorV2, PerMonitor (dpiAware: true/pm)", runtime.GetValue(PropertyNames.DpiAwareness))
        Assert.AreEqual("Windows 7, Windows 10/11, {00000000-1111-2222-3333-444444444444}", runtime.GetValue(PropertyNames.SupportedOS))
        Assert.AreEqual(PropertyNames.YesValue, runtime.GetValue(PropertyNames.CommonControlsV6))
        Assert.AreEqual("true", runtime.GetValue(PropertyNames.LongPathAware))
        Assert.AreEqual("UTF-8", runtime.GetValue(PropertyNames.ActiveCodePage))
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_InvalidXml_HasNoDetailRows()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Win32Manifest = Encoding.UTF8.GetBytes("<assembly>"))
        Dim runtime As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime)

        Assert.AreEqual("Yes (resource ID 1, invalid XML)", runtime.GetValue(PropertyNames.Win32Manifest))
        Assert.IsFalse(runtime.ContainsRow(PropertyNames.ExecutionLevel))
    End Sub

#End Region

#Region " Sidecar Configuration Files "

    <TestMethod>
    Public Sub ConfigurationFiles_NoSidecars_AreEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(0, info.ConfigurationFiles.Count)
        Assert.IsNull(info.AppConfig)
        Assert.IsNull(info.RuntimeConfig)
        Assert.IsNull(info.GetConfigurationFile(ConfigurationFileKind.AppConfig))
    End Sub

    <TestMethod>
    Public Sub ConfigurationFiles_AppConfigSidecar_IsReadAndPresented()
        Using fixture As FixtureFile = FixtureFile.CreateNamed(FixtureAssemblyBuilder.Build(New FixtureOptions() With {.ImageKind = FixtureImageKind.WindowsExe}), "Contoso.App", ".exe"),
              config As FixtureFile = FixtureFile.CreateSidecar($"{fixture.Path}.config", ConfigurationParsingTests.SampleAppConfig)

            Dim info As New AssemblyInfo(fixture.Path)

            Dim file As ConfigurationFile = info.GetConfigurationFile(ConfigurationFileKind.AppConfig)
            Assert.AreEqual("Contoso.App.exe.config", file.FileName)
            Assert.IsFalse(file.IsEmbedded)
            Assert.AreEqual(2, info.AppConfig.BindingRedirects.Count)

            Dim runtime As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime)
            Assert.AreEqual("Contoso.App.exe.config (sidecar file)", runtime.GetValue(PropertyNames.AppConfig))
            Assert.AreEqual("v4.0 (.NETFramework,Version=v4.8)", runtime.GetValue(PropertyNames.SupportedRuntimes))
            Assert.AreEqual("0.0.0.0-6.0.3.0 -> 6.0.3.0", runtime.GetValue($"{PropertyNames.BindingRedirectPrefix}System.Runtime.CompilerServices.Unsafe"))
        End Using
    End Sub

    <TestMethod>
    Public Sub ConfigurationFiles_RuntimeConfigAndDepsJsonSidecars_AreReadAndPresented()
        Dim options As New FixtureOptions() With {.CoreLibrary = FixtureCoreLibrary.SystemRuntime8}
        options.Attributes.Add(FixtureAttribute.TargetFramework(".NETCoreApp,Version=v8.0"))

        Using fixture As FixtureFile = FixtureFile.CreateNamed(FixtureAssemblyBuilder.Build(options), "Contoso.App", ".dll"),
              runtimeConfig As FixtureFile = FixtureFile.CreateSidecar(Path.ChangeExtension(fixture.Path, ".runtimeconfig.json"), ConfigurationParsingTests.SampleRuntimeConfigFrameworkDependent),
              depsJson As FixtureFile = FixtureFile.CreateSidecar(Path.ChangeExtension(fixture.Path, ".deps.json"), ConfigurationParsingTests.SampleDepsJson)

            Dim info As New AssemblyInfo(fixture.Path)

            CollectionAssert.AreEqual({"Contoso.App.runtimeconfig.json", "Contoso.App.deps.json"},
                                      info.ConfigurationFiles.Select(Function(f As ConfigurationFile) f.FileName).ToArray())
            Assert.IsFalse(info.RuntimeConfig.IsSelfContained)

            Dim runtime As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime)
            Assert.AreEqual(PropertyNames.NoValue, runtime.GetValue(PropertyNames.SingleFileBundle))
            Assert.AreEqual("Framework-dependent", runtime.GetValue(PropertyNames.Deployment))
            Assert.AreEqual("Contoso.App.runtimeconfig.json (sidecar file)", runtime.GetValue(PropertyNames.RuntimeConfig))
            Assert.AreEqual("net8.0", runtime.GetValue(PropertyNames.RuntimeConfigTfm))
            Assert.AreEqual("Microsoft.NETCore.App 8.0.0; Microsoft.WindowsDesktop.App 8.0.0", runtime.GetValue(PropertyNames.Frameworks))
            Assert.AreEqual("LatestMinor", runtime.GetValue(PropertyNames.RollForward))
            Assert.AreEqual("true", runtime.GetValue($"{PropertyNames.ConfigPropertyPrefix}System.GC.Server"))
            Assert.AreEqual(PropertyNames.NoneValue, runtime.GetValue(PropertyNames.AppConfig))
        End Using
    End Sub

    <TestMethod>
    Public Sub ConfigurationFiles_InvalidRuntimeConfig_IsFlaggedWithoutDetailRows()
        Using fixture As FixtureFile = FixtureFile.CreateNamed(FixtureAssemblyBuilder.Build(New FixtureOptions()), "Broken", ".dll"),
              runtimeConfig As FixtureFile = FixtureFile.CreateSidecar(Path.ChangeExtension(fixture.Path, ".runtimeconfig.json"), "{ invalid")

            Dim runtime As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(New AssemblyInfo(fixture.Path), PropertyNames.GroupRuntime)

            Assert.AreEqual("Broken.runtimeconfig.json (sidecar file, invalid content)", runtime.GetValue(PropertyNames.RuntimeConfig))
            Assert.IsFalse(runtime.ContainsRow(PropertyNames.RuntimeConfigTfm))
        End Using
    End Sub

    <TestMethod>
    <DataRow(CInt(FixtureCoreLibrary.Mscorlib), "Framework-dependent (.NET Framework)")>
    <DataRow(CInt(FixtureCoreLibrary.SystemRuntime8), "(unknown)")>
    Public Sub Deployment_WithoutRuntimeConfig_DependsOnTheFramework(coreLibrary As Integer, expected As String)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.CoreLibrary = CType(coreLibrary, FixtureCoreLibrary))

        Assert.AreEqual(expected, AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupRuntime).GetValue(PropertyNames.Deployment))
    End Sub

#End Region

#Region " Additional Attributes "

    <TestMethod>
    Public Sub Attributes_MultiInstance_KeepEveryInstanceInOrder()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.AssemblyMetadata("RepositoryUrl", "https://github.com/ElektroStudios/Example"))
                o.Attributes.Add(FixtureAttribute.InternalsVisibleTo("Contoso.Tests"))
                o.Attributes.Add(FixtureAttribute.AssemblyMetadata("IsTrimmable", "True"))
                o.Attributes.Add(FixtureAttribute.InternalsVisibleTo("DynamicProxyGenAssembly2"))
            End Sub)

        Assert.AreEqual(2, info.AssemblyMetadata.Count)
        Assert.AreEqual("RepositoryUrl", info.AssemblyMetadata(0).Key)
        Assert.AreEqual("https://github.com/ElektroStudios/Example", info.AssemblyMetadata(0).Value)
        CollectionAssert.AreEqual({"Contoso.Tests", "DynamicProxyGenAssembly2"}, info.InternalsVisibleTo.ToArray())
        Assert.IsTrue(info.IsTrimmable.Value)
    End Sub

    <TestMethod>
    Public Sub Attributes_KeyFileKeyNameAndDelaySign_AreRead()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.KeyFile("..\..\Key.snk"))
                o.Attributes.Add(FixtureAttribute.KeyName("ContosoKeyContainer"))
                o.Attributes.Add(FixtureAttribute.DelaySign(True))
            End Sub)

        Assert.AreEqual("..\..\Key.snk", info.KeyFile)
        Assert.AreEqual("ContosoKeyContainer", info.KeyName)
        Assert.IsTrue(info.DelaySign.Value)
    End Sub

    <TestMethod>
    Public Sub Attributes_None_AreEmptyOrNothing()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(0, info.AssemblyMetadata.Count)
        Assert.AreEqual(0, info.InternalsVisibleTo.Count)
        Assert.AreEqual(String.Empty, info.KeyFile)
        Assert.AreEqual(String.Empty, info.KeyName)
        Assert.IsFalse(info.DelaySign.HasValue)
        Assert.IsFalse(info.IsTrimmable.HasValue)
        Assert.AreEqual(0, info.ComClasses.Count)
    End Sub

    <TestMethod>
    Public Sub ComClasses_ClassesWithGuid_AreListedAndInterfacesSkipped()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeGuids.Add("Contoso.Shell.PropertySheet", "8a4d2f1e-3c5b-4a69-9e7d-1f2a3b4c5d6e")
                o.TypeGuids.Add("Contoso.Shell.IContract", "11111111-2222-3333-4444-555555555555")
                o.InterfaceTypeNames.Add("Contoso.Shell.IContract")
                o.TypeNames.Add("Contoso.Shell.NoGuid")
            End Sub)

        Assert.AreEqual(1, info.ComClasses.Count)
        Assert.AreEqual("Contoso.Shell.PropertySheet", info.ComClasses(0).Key)
        Assert.AreEqual(New Guid("8a4d2f1e-3c5b-4a69-9e7d-1f2a3b4c5d6e"), info.ComClasses(0).Value)
    End Sub

    <TestMethod>
    Public Sub AttributesGroup_ListsFriendsComClassesAndMetadata()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.InternalsVisibleTo("A.Tests"))
                o.Attributes.Add(FixtureAttribute.InternalsVisibleTo("B.Tests"))
                o.Attributes.Add(FixtureAttribute.AssemblyMetadata("RepositoryCommit", "0123abcd"))
                o.Attributes.Add(FixtureAttribute.DelaySign(False))
                o.TypeGuids.Add("Contoso.Server", "8a4d2f1e-3c5b-4a69-9e7d-1f2a3b4c5d6e")
            End Sub)

        Dim attributes As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupAttributes)

        CollectionAssert.AreEqual({"A.Tests", "B.Tests"},
                                  attributes.Rows.Where(Function(r As PropertyRow) r.Name = PropertyNames.InternalsVisibleTo).Select(Function(r As PropertyRow) r.Value).ToArray())
        Assert.AreEqual("{8A4D2F1E-3C5B-4A69-9E7D-1F2A3B4C5D6E}", attributes.GetValue($"{PropertyNames.ComClassPrefix}Contoso.Server"))
        Assert.AreEqual("0123abcd", attributes.GetValue($"{PropertyNames.MetadataPrefix}RepositoryCommit"))
        Assert.AreEqual(PropertyNames.NoValue, attributes.GetValue(PropertyNames.DelaySign))
        Assert.AreEqual(PropertyNames.NoneValue, attributes.GetValue(PropertyNames.KeyFile))
    End Sub

    <TestMethod>
    Public Sub AttributesGroup_NoFriends_HasSingleNoneRow()
        Dim attributes As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(TestHelpers.Inspect(Nothing), PropertyNames.GroupAttributes)

        Assert.AreEqual(PropertyNames.NoneValue, attributes.GetValue(PropertyNames.InternalsVisibleTo))
        Assert.AreEqual(1, attributes.Rows.Where(Function(r As PropertyRow) r.Name = PropertyNames.InternalsVisibleTo).Count())
    End Sub

#End Region

#Region " References (any version / retargetable) "

    <TestMethod>
    Public Sub References_VersionZero_IsShownAsAnyVersion()
        ' Some facades and shims (e.g. the netstandard.dll shim) store references with version 0.0.0.0.
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.None
                o.AssemblyReferences.Add(Tuple.Create("System.Data", New Version(0, 0, 0, 0), FixtureAssemblyBuilder.HexToBytes("b77a5c561934e089")))
            End Sub)

        Dim references As PropertyGroup = AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupReferences)

        Assert.AreEqual("0.0.0.0 (any version)", references.GetValue("System.Data"))
    End Sub

    <TestMethod>
    Public Sub References_Retargetable_IsShown()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.None
                o.AssemblyReferences.Add(Tuple.Create("System.Runtime", New Version(2, 0, 5, 0), FixtureAssemblyBuilder.HexToBytes("7cec85d7bea7798e")))
                o.AssemblyReferenceFlags.Add("System.Runtime", AssemblyFlags.Retargetable)
            End Sub)

        Assert.AreEqual(AssemblyNameFlags.Retargetable, info.ReferencedAssemblies(0).Flags And AssemblyNameFlags.Retargetable)
        Assert.AreEqual("2.0.5.0, retargetable", AssemblyInfoRuntimeTests.GetGroup(info, PropertyNames.GroupReferences).GetValue("System.Runtime"))
    End Sub

#End Region

End Class

#End Region
