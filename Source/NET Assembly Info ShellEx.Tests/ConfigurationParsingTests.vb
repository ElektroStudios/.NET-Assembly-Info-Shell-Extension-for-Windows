
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " ConfigurationParsingTests "

''' <summary>
''' Tests the parsing of Win32 manifests, app.config, runtimeconfig.json and deps.json documents.
''' </summary>
<TestClass>
Public NotInheritable Class ConfigurationParsingTests

#Region " Sample Documents "

    ''' <summary>A typical application manifest.</summary>
    Friend Const SampleManifest As String =
"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0"">
  <assemblyIdentity version=""1.0.0.0"" name=""MyApplication.app""/>
  <trustInfo xmlns=""urn:schemas-microsoft-com:asm.v2"">
    <security>
      <requestedPrivileges xmlns=""urn:schemas-microsoft-com:asm.v3"">
        <requestedExecutionLevel level=""requireAdministrator"" uiAccess=""false"" />
      </requestedPrivileges>
    </security>
  </trustInfo>
  <compatibility xmlns=""urn:schemas-microsoft-com:compatibility.v1"">
    <application>
      <supportedOS Id=""{35138b9a-5d96-4fbd-8e2d-a2440225f93a}"" />
      <supportedOS Id=""{8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a}"" />
      <supportedOS Id=""{00000000-1111-2222-3333-444444444444}"" />
    </application>
  </compatibility>
  <application xmlns=""urn:schemas-microsoft-com:asm.v3"">
    <windowsSettings>
      <dpiAware xmlns=""http://schemas.microsoft.com/SMI/2005/WindowsSettings"">true/pm</dpiAware>
      <dpiAwareness xmlns=""http://schemas.microsoft.com/SMI/2016/WindowsSettings"">PerMonitorV2, PerMonitor</dpiAwareness>
      <longPathAware xmlns=""http://schemas.microsoft.com/SMI/2016/WindowsSettings"">true</longPathAware>
      <activeCodePage xmlns=""http://schemas.microsoft.com/SMI/2019/WindowsSettings"">UTF-8</activeCodePage>
    </windowsSettings>
  </application>
  <dependency>
    <dependentAssembly>
      <assemblyIdentity type=""win32"" name=""Microsoft.Windows.Common-Controls"" version=""6.0.0.0"" processorArchitecture=""*"" publicKeyToken=""6595b64144ccf1df"" language=""*"" />
    </dependentAssembly>
  </dependency>
</assembly>"

    ''' <summary>A typical .NET Framework application configuration file.</summary>
    Friend Const SampleAppConfig As String =
"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <startup>
    <supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.8"" />
  </startup>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""System.Memory"" publicKeyToken=""cc7b13ffcd2ddd51"" culture=""neutral"" />
        <bindingRedirect oldVersion=""0.0.0.0-4.0.5.0"" newVersion=""4.0.5.0"" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name=""System.Runtime.CompilerServices.Unsafe"" publicKeyToken=""b03f5f7f11d50a3a"" culture=""neutral"" />
        <bindingRedirect oldVersion=""0.0.0.0-6.0.3.0"" newVersion=""6.0.3.0"" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>"

    ''' <summary>A framework-dependent runtimeconfig.json (.NET 5+ format).</summary>
    Friend Const SampleRuntimeConfigFrameworkDependent As String =
"{
  ""runtimeOptions"": {
    ""tfm"": ""net8.0"",
    ""rollForward"": ""LatestMinor"",
    ""frameworks"": [
      { ""name"": ""Microsoft.NETCore.App"", ""version"": ""8.0.0"" },
      { ""name"": ""Microsoft.WindowsDesktop.App"", ""version"": ""8.0.0"" }
    ],
    ""configProperties"": {
      ""System.GC.Server"": true,
      ""System.Globalization.Invariant"": false,
      ""Custom/Key With Spaces"": ""value""
    }
  }
}"

    ''' <summary>A self-contained runtimeconfig.json, as written by "dotnet publish --self-contained".</summary>
    Friend Const SampleRuntimeConfigSelfContained As String =
"{
  ""runtimeOptions"": {
    ""tfm"": ""net8.0"",
    ""includedFrameworks"": [
      { ""name"": ""Microsoft.NETCore.App"", ""version"": ""8.0.33"" }
    ]
  }
}"

    ''' <summary>A deps.json with the application project and one package.</summary>
    Friend Const SampleDepsJson As String =
"{
  ""runtimeTarget"": { ""name"": "".NETCoreApp,Version=v8.0/win-x64"" },
  ""targets"": { },
  ""libraries"": {
    ""Newtonsoft.Json/13.0.3"": { ""type"": ""package"", ""serviceable"": true },
    ""Contoso.App/1.2.3"": { ""type"": ""project"", ""serviceable"": false, ""sha512"": """" }
  }
}"

#End Region

#Region " Win32ManifestInfo "

    <TestMethod>
    Public Sub Win32Manifest_TypicalManifest_ExposesTheRelevantSettings()
        Dim manifest As New Win32ManifestInfo(Encoding.UTF8.GetBytes(ConfigurationParsingTests.SampleManifest), 1)

        Assert.IsTrue(manifest.IsValidXml)
        Assert.AreEqual(1, manifest.ResourceId)
        Assert.AreEqual("requireAdministrator", manifest.RequestedExecutionLevel)
        Assert.AreEqual("false", manifest.UIAccess)
        Assert.AreEqual("true/pm", manifest.DpiAware)
        Assert.AreEqual("PerMonitorV2, PerMonitor", manifest.DpiAwareness)
        Assert.AreEqual("true", manifest.LongPathAware)
        Assert.AreEqual("UTF-8", manifest.ActiveCodePage)
        Assert.IsTrue(manifest.UsesCommonControlsV6)
        CollectionAssert.AreEqual({"Windows 7", "Windows 10/11", "{00000000-1111-2222-3333-444444444444}"}, manifest.SupportedOperatingSystems.ToArray())
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_MinimalManifest_HasEmptySettings()
        Dim xml As String = "<assembly xmlns=""urn:schemas-microsoft-com:asm.v1"" manifestVersion=""1.0""><assemblyIdentity version=""1.0.0.0"" name=""App""/></assembly>"
        Dim manifest As New Win32ManifestInfo(Encoding.UTF8.GetBytes(xml), 2)

        Assert.IsTrue(manifest.IsValidXml)
        Assert.AreEqual(String.Empty, manifest.RequestedExecutionLevel)
        Assert.AreEqual(String.Empty, manifest.DpiAwareness)
        Assert.IsFalse(manifest.UsesCommonControlsV6)
        Assert.AreEqual(0, manifest.SupportedOperatingSystems.Count)
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_CommonControlsV5_IsNotV6()
        Dim xml As String = "<assembly><dependency><dependentAssembly><assemblyIdentity name=""Microsoft.Windows.Common-Controls"" version=""5.82.0.0""/></dependentAssembly></dependency></assembly>"

        Assert.IsFalse(New Win32ManifestInfo(Encoding.UTF8.GetBytes(xml), 1).UsesCommonControlsV6)
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_Utf16WithBomAndNullPadding_IsDecoded()
        Dim xml As String = "<assembly><trustInfo><security><requestedPrivileges><requestedExecutionLevel level=""asInvoker""/></requestedPrivileges></security></trustInfo></assembly>"
        Dim bytes As Byte() = Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(xml)).Concat(New Byte() {0, 0, 0, 0}).ToArray()

        Dim manifest As New Win32ManifestInfo(bytes, 1)

        Assert.IsTrue(manifest.IsValidXml)
        Assert.AreEqual("asInvoker", manifest.RequestedExecutionLevel)
        Assert.IsFalse(manifest.Xml.EndsWith(ControlChars.NullChar, StringComparison.Ordinal))
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_InvalidXml_KeepsRawText()
        Dim manifest As New Win32ManifestInfo(Encoding.UTF8.GetBytes("<assembly><unclosed></assembly>"), 1)

        Assert.IsFalse(manifest.IsValidXml)
        Assert.AreEqual("<assembly><unclosed></assembly>", manifest.Xml)
    End Sub

    <TestMethod>
    Public Sub Win32Manifest_DtdIsNotProcessed()
        Dim xml As String = "<!DOCTYPE assembly [<!ENTITY xxe SYSTEM ""file:///c:/windows/win.ini"">]><assembly>&xxe;</assembly>"

        Assert.IsFalse(New Win32ManifestInfo(Encoding.UTF8.GetBytes(xml), 1).IsValidXml)
    End Sub

#End Region

#Region " AppConfigInfo "

    <TestMethod>
    Public Sub AppConfig_TypicalFile_ExposesRuntimesAndRedirects()
        Dim config As New AppConfigInfo(ConfigurationParsingTests.SampleAppConfig)

        Assert.IsTrue(config.IsValidXml)
        CollectionAssert.AreEqual({"v4.0 (.NETFramework,Version=v4.8)"}, config.SupportedRuntimes.ToArray())
        Assert.AreEqual(2, config.BindingRedirects.Count)
        Assert.AreEqual("System.Runtime.CompilerServices.Unsafe", config.BindingRedirects(1).AssemblyName)
        Assert.AreEqual("b03f5f7f11d50a3a", config.BindingRedirects(1).PublicKeyToken)
        Assert.AreEqual("0.0.0.0-6.0.3.0", config.BindingRedirects(1).OldVersion)
        Assert.AreEqual("6.0.3.0", config.BindingRedirects(1).NewVersion)
    End Sub

    <TestMethod>
    Public Sub AppConfig_SupportedRuntimeWithoutSku_ShowsOnlyVersion()
        Dim config As New AppConfigInfo("<configuration><startup><supportedRuntime version=""v2.0.50727""/></startup></configuration>")

        CollectionAssert.AreEqual({"v2.0.50727"}, config.SupportedRuntimes.ToArray())
    End Sub

    <TestMethod>
    Public Sub AppConfig_InvalidXml_IsFlagged()
        Dim config As New AppConfigInfo("<configuration>")

        Assert.IsFalse(config.IsValidXml)
        Assert.AreEqual(0, config.BindingRedirects.Count)
    End Sub

#End Region

#Region " RuntimeConfigInfo "

    <TestMethod>
    Public Sub RuntimeConfig_FrameworkDependent_ExposesAllSettings()
        Dim config As New RuntimeConfigInfo(ConfigurationParsingTests.SampleRuntimeConfigFrameworkDependent)

        Assert.IsTrue(config.IsValidJson)
        Assert.IsFalse(config.IsSelfContained)
        Assert.AreEqual("net8.0", config.TargetFrameworkMoniker)
        Assert.AreEqual("LatestMinor", config.RollForward)
        CollectionAssert.AreEqual({"Microsoft.NETCore.App 8.0.0", "Microsoft.WindowsDesktop.App 8.0.0"}, config.Frameworks.ToArray())

        Assert.AreEqual(3, config.ConfigProperties.Count)
        Assert.AreEqual("System.GC.Server", config.ConfigProperties(0).Key)
        Assert.AreEqual("true", config.ConfigProperties(0).Value)
        Assert.AreEqual("Custom/Key With Spaces", config.ConfigProperties(2).Key) ' Not a valid XML name: mapped through <a:item item="...">.
        Assert.AreEqual("value", config.ConfigProperties(2).Value)
    End Sub

    <TestMethod>
    Public Sub RuntimeConfig_SelfContained_IsDetected()
        Dim config As New RuntimeConfigInfo(ConfigurationParsingTests.SampleRuntimeConfigSelfContained)

        Assert.IsTrue(config.IsSelfContained)
        CollectionAssert.AreEqual({"Microsoft.NETCore.App 8.0.33"}, config.Frameworks.ToArray())
        Assert.AreEqual(String.Empty, config.RollForward)
    End Sub

    <TestMethod>
    Public Sub RuntimeConfig_LegacySingleFramework_IsRead()
        Dim config As New RuntimeConfigInfo("{ ""runtimeOptions"": { ""tfm"": ""netcoreapp3.1"", ""framework"": { ""name"": ""Microsoft.NETCore.App"", ""version"": ""3.1.0"" } } }")

        CollectionAssert.AreEqual({"Microsoft.NETCore.App 3.1.0"}, config.Frameworks.ToArray())
        Assert.AreEqual("netcoreapp3.1", config.TargetFrameworkMoniker)
    End Sub

    <TestMethod>
    Public Sub RuntimeConfig_InvalidJson_IsFlagged()
        Dim config As New RuntimeConfigInfo("{ ""runtimeOptions"": ")

        Assert.IsFalse(config.IsValidJson)
        Assert.AreEqual(0, config.Frameworks.Count)
    End Sub

    <TestMethod>
    Public Sub RuntimeConfig_WithoutRuntimeOptions_IsEmpty()
        Dim config As New RuntimeConfigInfo("{ }")

        Assert.IsTrue(config.IsValidJson)
        Assert.AreEqual(String.Empty, config.TargetFrameworkMoniker)
        Assert.IsFalse(config.IsSelfContained)
    End Sub

#End Region

#Region " deps.json / JSON / Text "

    <TestMethod>
    Public Sub DepsJson_ProjectLibrary_IsFound()
        Assert.AreEqual("Contoso.App", SingleFileBundle.GetDepsJsonProjectName(ConfigurationParsingTests.SampleDepsJson))
    End Sub

    <TestMethod>
    <DataRow("{ ""libraries"": { ""Pkg/1.0.0"": { ""type"": ""package"" } } }")>
    <DataRow("{ }")>
    <DataRow("not json")>
    Public Sub DepsJson_WithoutProjectLibrary_ReturnsEmpty(json As String)
        Assert.AreEqual(String.Empty, SingleFileBundle.GetDepsJsonProjectName(json))
    End Sub

    <TestMethod>
    Public Sub JsonDocumentReader_MemberNames_AreMappedBack()
        Dim root As XElement = JsonDocumentReader.Parse("{ ""simple"": 1, ""with/slash"": ""x"", ""System.GC.Server"": true }")

        Assert.AreEqual("1", JsonDocumentReader.GetMemberValue(root, "simple"))
        Assert.AreEqual("x", JsonDocumentReader.GetMemberValue(root, "with/slash"))
        Assert.AreEqual("true", JsonDocumentReader.GetMemberValue(root, "System.GC.Server"))
        Assert.AreEqual(String.Empty, JsonDocumentReader.GetMemberValue(root, "missing"))
        Assert.IsNull(JsonDocumentReader.GetMember(root, "missing"))
    End Sub

    <TestMethod>
    Public Sub TextFileDecoder_HonorsByteOrderMarks()
        Assert.AreEqual("ñ", TextFileDecoder.Decode(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("ñ")).ToArray()))
        Assert.AreEqual("ñ", TextFileDecoder.Decode(Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes("ñ")).ToArray()))
        Assert.AreEqual("ñ", TextFileDecoder.Decode(Encoding.UTF8.GetBytes("ñ")))
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() TextFileDecoder.Decode(Nothing))
    End Sub

#End Region

End Class

#End Region
