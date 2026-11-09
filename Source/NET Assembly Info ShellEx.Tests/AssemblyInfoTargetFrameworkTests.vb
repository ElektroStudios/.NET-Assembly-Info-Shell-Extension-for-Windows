
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoTargetFrameworkTests "

''' <summary>
''' Tests the target framework and target OS platform detection, and the framework kind inference.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoTargetFrameworkTests

#Region " TargetFrameworkAttribute "

    <TestMethod>
    <DataRow(".NETFramework,Version=v4.8", ".NETFramework", 4, 8, ".NET Framework 4.8", CInt(AssemblyFrameworkKind.NetFramework))>
    <DataRow(".NETFramework,Version=v4.8.1", ".NETFramework", 4, 8, ".NET Framework 4.8.1", CInt(AssemblyFrameworkKind.NetFramework))>
    <DataRow(".NETCoreApp,Version=v3.1", ".NETCoreApp", 3, 1, ".NET Core 3.1", CInt(AssemblyFrameworkKind.NetCore))>
    <DataRow(".NETCoreApp,Version=v8.0", ".NETCoreApp", 8, 0, ".NET 8.0", CInt(AssemblyFrameworkKind.NetCore))>
    <DataRow(".NETStandard,Version=v2.0", ".NETStandard", 2, 0, ".NET Standard 2.0", CInt(AssemblyFrameworkKind.NetStandard))>
    <DataRow(".NETPortable,Version=v4.5,Profile=Profile259", ".NETPortable", 4, 5, ".NET Portable 4.5 (Profile259)", CInt(AssemblyFrameworkKind.NetStandard))>
    Public Sub TargetFramework_FromAttribute_IsParsed(frameworkName As String, expectedIdentifier As String, major As Integer, minor As Integer,
                                                      expectedDisplayName As String, expectedKind As Integer)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.TargetFramework(frameworkName)))

        Assert.AreEqual(frameworkName, info.TargetFrameworkName)
        Assert.AreEqual(expectedIdentifier, info.TargetFrameworkIdentifier)
        Assert.AreEqual(major, info.TargetFrameworkVersion.Major)
        Assert.AreEqual(minor, info.TargetFrameworkVersion.Minor)
        Assert.AreEqual(expectedDisplayName, info.TargetFrameworkDisplayName)
        Assert.AreEqual(CType(expectedKind, AssemblyFrameworkKind), info.FrameworkKind)
        Assert.IsFalse(info.IsFrameworkKindInferred)
    End Sub

    <TestMethod>
    Public Sub TargetFramework_WithFrameworkDisplayName_PrefersTheCompilerDisplayName()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.TargetFramework(".NETFramework,Version=v4.8", ".NET Framework 4.8 (custom)"))
            End Sub)

        Assert.AreEqual(".NET Framework 4.8 (custom)", info.TargetFrameworkDisplayName)
    End Sub

    <TestMethod>
    Public Sub TargetFramework_WithEmptyFrameworkDisplayName_ComposesTheDisplayName()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.SystemRuntime8
                o.Attributes.Add(FixtureAttribute.TargetFramework(".NETCoreApp,Version=v8.0", String.Empty))
            End Sub)

        Assert.AreEqual(".NET 8.0", info.TargetFrameworkDisplayName)
        Assert.IsTrue(info.IsNetCore)
    End Sub

#End Region

#Region " Inference "

    <TestMethod>
    <DataRow(CInt(FixtureCoreLibrary.Mscorlib), CInt(AssemblyFrameworkKind.NetFramework), ".NET Framework 4.x (inferred)")>
    <DataRow(CInt(FixtureCoreLibrary.SystemRuntime8), CInt(AssemblyFrameworkKind.NetCore), ".NET Core / .NET (inferred)")>
    <DataRow(CInt(FixtureCoreLibrary.Netstandard20), CInt(AssemblyFrameworkKind.NetStandard), ".NET Standard (inferred)")>
    <DataRow(CInt(FixtureCoreLibrary.SystemRuntimeLegacy), CInt(AssemblyFrameworkKind.NetStandard), ".NET Standard (inferred)")>
    <DataRow(CInt(FixtureCoreLibrary.None), CInt(AssemblyFrameworkKind.Unknown), "")>
    Public Sub FrameworkKind_WithoutAttribute_IsInferredFromReferences(coreLibrary As Integer, expectedKind As Integer, expectedDisplayName As String)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.CoreLibrary = CType(coreLibrary, FixtureCoreLibrary))

        Assert.AreEqual(CType(expectedKind, AssemblyFrameworkKind), info.FrameworkKind)
        Assert.AreEqual(expectedDisplayName, info.TargetFrameworkDisplayName)
        Assert.IsTrue(info.IsFrameworkKindInferred)
        Assert.AreEqual(String.Empty, info.TargetFrameworkName)
        Assert.IsNull(info.TargetFrameworkVersion)
    End Sub

    <TestMethod>
    Public Sub FrameworkKind_ReferencesSystemPrivateCoreLib_IsNetCore()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.None
                o.AssemblyReferences.Add(Tuple.Create("System.Private.CoreLib", New Version(8, 0, 0, 0), CType(Nothing, Byte())))
            End Sub)

        Assert.AreEqual(AssemblyFrameworkKind.NetCore, info.FrameworkKind)
    End Sub

    <TestMethod>
    Public Sub FrameworkKind_NetCoreAndNetstandardReferences_PrefersNetCore()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.Netstandard20
                o.AssemblyReferences.Add(Tuple.Create("System.Runtime", New Version(6, 0, 0, 0), CType(Nothing, Byte())))
            End Sub)

        Assert.AreEqual(AssemblyFrameworkKind.NetCore, info.FrameworkKind)
    End Sub

    <TestMethod>
    <DataRow("mscorlib", CInt(AssemblyFrameworkKind.NetFramework))>
    <DataRow("System.Private.CoreLib", CInt(AssemblyFrameworkKind.NetCore))>
    <DataRow("netstandard", CInt(AssemblyFrameworkKind.NetStandard))>
    Public Sub FrameworkKind_CoreLibraryItself_IsInferredFromItsOwnName(assemblyName As String, expectedKind As Integer)
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.AssemblyName = assemblyName
                o.CoreLibrary = FixtureCoreLibrary.None
            End Sub)

        Assert.AreEqual(CType(expectedKind, AssemblyFrameworkKind), info.FrameworkKind)
    End Sub

    <TestMethod>
    <DataRow("v1.1.4322", ".NET Framework 1.x (inferred)")>
    <DataRow("v2.0.50727", ".NET Framework 2.0 - 3.5 (inferred)")>
    <DataRow("v4.0.30319", ".NET Framework 4.x (inferred)")>
    Public Sub TargetFrameworkDisplayName_LegacyNetFramework_IsInferredFromMetadataVersion(metadataVersion As String, expected As String)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.MetadataVersion = metadataVersion)

        Assert.AreEqual(expected, info.TargetFrameworkDisplayName)
        Assert.AreEqual(metadataVersion.TrimStart("v"c), info.ClrVersion)
    End Sub

#End Region

#Region " TryParseFrameworkName / GetFrameworkDisplayName "

    <TestMethod>
    Public Sub TryParseFrameworkName_WithProfile_ReturnsAllParts()
        Dim identifier As String = Nothing
        Dim version As Version = Nothing
        Dim profile As String = Nothing

        Assert.IsTrue(AssemblyInfo.TryParseFrameworkName(".NETFramework, Version=v4.0, Profile=Client", identifier, version, profile))
        Assert.AreEqual(".NETFramework", identifier)
        Assert.AreEqual(New Version(4, 0), version)
        Assert.AreEqual("Client", profile)
    End Sub

    <TestMethod>
    Public Sub TryParseFrameworkName_Null_ReturnsFalse()
        Dim identifier As String = Nothing
        Dim version As Version = Nothing
        Dim profile As String = Nothing

        Assert.IsFalse(AssemblyInfo.TryParseFrameworkName(Nothing, identifier, version, profile))
        Assert.AreEqual(String.Empty, identifier)
    End Sub

    <TestMethod>
    <DataRow("")>
    <DataRow("   ")>
    Public Sub TryParseFrameworkName_EmptyOrWhiteSpace_ReturnsFalse(value As String)
        Dim identifier As String = Nothing
        Dim version As Version = Nothing
        Dim profile As String = Nothing

        Assert.IsFalse(AssemblyInfo.TryParseFrameworkName(value, identifier, version, profile))
        Assert.AreEqual(String.Empty, identifier)
        Assert.IsNull(version)
        Assert.AreEqual(String.Empty, profile)
    End Sub

    <TestMethod>
    Public Sub TryParseFrameworkName_MalformedVersion_KeepsIdentifierAndNullVersion()
        Dim identifier As String = Nothing
        Dim version As Version = Nothing
        Dim profile As String = Nothing

        Assert.IsTrue(AssemblyInfo.TryParseFrameworkName(".NETCoreApp,Version=vNext,Garbage", identifier, version, profile))
        Assert.AreEqual(".NETCoreApp", identifier)
        Assert.IsNull(version)
    End Sub

    <TestMethod>
    Public Sub GetFrameworkDisplayName_UnknownIdentifierWithoutVersion_ReturnsIdentifier()
        Assert.AreEqual("Xamarin.iOS", AssemblyInfo.GetFrameworkDisplayName("Xamarin.iOS", Nothing, Nothing))
    End Sub

    <TestMethod>
    Public Sub GetFrameworkDisplayName_ClientProfile_AppendsProfile()
        Assert.AreEqual(".NET Framework 4.0 (Client Profile)", AssemblyInfo.GetFrameworkDisplayName(".NETFramework", New Version(4, 0), "Client"))
    End Sub

#End Region

#Region " Target OS Platform "

    <TestMethod>
    Public Sub TargetPlatform_WithoutAttributes_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(String.Empty, info.TargetPlatform)
    End Sub

    <TestMethod>
    Public Sub TargetPlatform_FromTargetPlatformAttribute_IsFormatted()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.TargetPlatform("Windows7.0")))

        Assert.AreEqual("Windows 7.0", info.TargetPlatform)
    End Sub

    <TestMethod>
    Public Sub TargetPlatform_FromSupportedOSPlatformAttribute_IsFormattedAndCapitalized()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.SupportedOSPlatform("windows10.0.19041")))

        Assert.AreEqual("Windows 10.0.19041", info.TargetPlatform)
    End Sub

    <TestMethod>
    Public Sub TargetPlatform_BothAttributes_PrefersTargetPlatformAttribute()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.SupportedOSPlatform("windows10.0"))
                o.Attributes.Add(FixtureAttribute.TargetPlatform("Windows7.0"))
            End Sub)

        Assert.AreEqual("Windows 7.0", info.TargetPlatform)
    End Sub

    <TestMethod>
    Public Sub TargetPlatform_WithoutVersion_IsKeptAsIs()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.TargetPlatform("Browser")))

        Assert.AreEqual("Browser", info.TargetPlatform)
    End Sub

#End Region

End Class

#End Region
