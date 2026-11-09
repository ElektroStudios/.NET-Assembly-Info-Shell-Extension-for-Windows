
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoRealAssemblyTests "

''' <summary>
''' Compares the metadata-based <see cref="AssemblyInfo"/> against the values returned by reflection
''' for real assemblies (this test assembly, the shell extension and the core library),
''' and tests the Global Assembly Cache (GAC) lookup.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoRealAssemblyTests

#Region " Test Assembly vs. Reflection "

    <TestMethod>
    Public Sub TestAssembly_Identity_MatchesReflection()
        Dim expected As AssemblyName = GetType(AssemblyInfoRealAssemblyTests).Assembly.GetName()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual(expected.Name, info.Name)
        Assert.AreEqual(expected.Version, info.Version)
        Assert.AreEqual(expected.FullName, info.FullName)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_Attributes_MatchReflection()
        Dim assembly As Assembly = GetType(AssemblyInfoRealAssemblyTests).Assembly
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual(New Guid(assembly.GetCustomAttribute(Of GuidAttribute)().Value), info.Guid.Value)
        Assert.AreEqual(assembly.GetCustomAttribute(Of ComVisibleAttribute)().Value, info.IsComVisible.Value)
        Assert.AreEqual(assembly.GetCustomAttribute(Of CLSCompliantAttribute)().IsCompliant, info.IsClsCompliant.Value)
        Assert.AreEqual(assembly.GetCustomAttribute(Of NeutralResourcesLanguageAttribute)().CultureName, info.NeutralResourcesLanguage)
        Assert.AreEqual(assembly.GetCustomAttribute(Of DefaultDllImportSearchPathsAttribute)().Paths, info.DefaultDllImportSearchPaths.Value)
        Assert.AreEqual(assembly.GetCustomAttribute(Of AssemblyConfigurationAttribute)().Configuration, info.BuildConfiguration)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_TargetFramework_MatchesReflection()
        Dim attribute As TargetFrameworkAttribute = GetType(AssemblyInfoRealAssemblyTests).Assembly.GetCustomAttribute(Of TargetFrameworkAttribute)()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        If attribute Is Nothing Then
            Assert.IsTrue(info.IsFrameworkKindInferred)
        Else
            Assert.AreEqual(attribute.FrameworkName, info.TargetFrameworkName)
            Assert.IsFalse(info.IsFrameworkKindInferred)
        End If
        Assert.AreEqual(If(TestHelpers.IsRunningOnNetFramework, AssemblyFrameworkKind.NetFramework, AssemblyFrameworkKind.NetCore), info.FrameworkKind)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_Optimizations_MatchDebuggableAttribute()
        Dim attribute As DebuggableAttribute = GetType(AssemblyInfoRealAssemblyTests).Assembly.GetCustomAttribute(Of DebuggableAttribute)()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Dim expected As Boolean = (attribute Is Nothing) OrElse Not attribute.IsJITOptimizerDisabled
        Assert.AreEqual(expected, info.IsOptimized)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_ModuleVersionId_MatchesReflection()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual(GetType(AssemblyInfoRealAssemblyTests).Module.ModuleVersionId, info.ModuleVersionId)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_EntryPoint_MatchesReflection()
        Dim entryPoint As MethodInfo = GetType(AssemblyInfoRealAssemblyTests).Assembly.EntryPoint
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Dim expected As String = If(entryPoint Is Nothing, String.Empty, $"{entryPoint.DeclaringType.FullName}.{entryPoint.Name}")
        Assert.AreEqual(expected, info.EntryPointMethod)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_ReferencedAssemblies_MatchReflection()
        Dim expected As String() = GetType(AssemblyInfoRealAssemblyTests).Assembly.GetReferencedAssemblies().
                                   Select(Function(x As AssemblyName) x.FullName).
                                   OrderBy(Function(x As String) x, StringComparer.OrdinalIgnoreCase).ToArray()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Dim actual As String() = info.ReferencedAssemblies.
                                 Select(Function(x As AssemblyName) x.FullName).
                                 OrderBy(Function(x As String) x, StringComparer.OrdinalIgnoreCase).ToArray()
        CollectionAssert.AreEqual(expected, actual)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_DeterministicBuild_HasNoTimeStamp()
        ' The test project sets <Deterministic>true</Deterministic>.
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.IsTrue(info.IsDeterministic)
        Assert.IsFalse(info.TimeStamp.HasValue)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_PortablePdb_IsReferenced()
        ' The test project sets <DebugType>portable</DebugType>.
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        StringAssert.EndsWith(info.PdbPath, "AssemblyInfo.Tests.pdb")
        Assert.IsTrue(info.IsPortablePdb)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_RootNamespace_IsCommonNamespaceOfTestTypes()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual("ShellExTests", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub TestAssembly_ProcessorArchitecture_MatchesAssemblyName()
        If Not TestHelpers.IsRunningOnNetFramework Then
            Assert.Inconclusive("AssemblyName.ProcessorArchitecture always returns None on .NET 5+.")
        End If

#Disable Warning BC40000 ' Obsolete on .NET 5+ only.
        Dim expected As ProcessorArchitecture = AssemblyName.GetAssemblyName(TestHelpers.TestAssemblyPath).ProcessorArchitecture
#Enable Warning BC40000
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.AreEqual(expected, info.ProcessorArchitecture)
    End Sub

#End Region

#Region " Shell Extension Assembly "

    <TestMethod>
    Public Sub ShellExtensionAssembly_Identity_MatchesReflection()
        Dim assembly As Assembly = GetType(AssemblyInfo).Assembly
        Dim info As New AssemblyInfo(assembly.Location)

        Assert.AreEqual(assembly.GetName().FullName, info.FullName)
        Assert.AreEqual(New Guid(assembly.GetCustomAttribute(Of GuidAttribute)().Value), info.Guid.Value)
        Assert.AreEqual(assembly.GetCustomAttribute(Of NeutralResourcesLanguageAttribute)().CultureName, info.NeutralResourcesLanguage)
        Assert.AreEqual(String.Empty, info.RootNamespace) ' <RootNamespace></RootNamespace> in the .vbproj

        If TestHelpers.IsRunningOnNetFramework Then
            Assert.AreEqual(".NETFramework", info.TargetFrameworkIdentifier)
            Assert.AreEqual(AssemblyFrameworkKind.NetFramework, info.FrameworkKind)
        End If
    End Sub

#End Region

#Region " Core Library vs. Reflection "

    <TestMethod>
    Public Sub CoreLibrary_Identity_MatchesReflection()
        Dim assembly As Assembly = GetType(Object).Assembly
        Dim expected As AssemblyName = assembly.GetName()
        Dim info As New AssemblyInfo(assembly.Location)

        Assert.AreEqual(expected.FullName, info.FullName)
        Assert.AreEqual(AssemblyInfoRealAssemblyTests.ToHex(expected.GetPublicKeyToken()), info.PublicKeyToken)
        Assert.AreEqual(StrongNameSignatureKind.Signed, info.StrongNameSignature)
    End Sub

    <TestMethod>
    Public Sub CoreLibrary_FrameworkKind_MatchesCurrentRuntime()
        Dim info As New AssemblyInfo(GetType(Object).Assembly.Location)

        Assert.AreEqual(If(TestHelpers.IsRunningOnNetFramework, AssemblyFrameworkKind.NetFramework, AssemblyFrameworkKind.NetCore), info.FrameworkKind)
    End Sub

#End Region

#Region " No Assembly Loading "

    <TestMethod>
    Public Sub Constructor_DoesNotLoadTheAssemblyIntoTheCurrentProcess()
        ' Regression: Assembly.Load(Byte()) kept every inspected assembly loaded in Explorer forever.
        Dim assemblyName As String = $"NotLoaded_{Guid.NewGuid():N}"
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.AssemblyName = assemblyName)

        Assert.AreEqual(assemblyName, info.Name)
        Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(Function(x As Assembly) x.GetName().Name = assemblyName))
    End Sub

    <TestMethod>
    Public Sub Constructor_DoesNotLockTheFile()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions())
            Dim info As New AssemblyInfo(fixture.Path)

            Using stream As New FileStream(fixture.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Assert.IsTrue(stream.CanWrite)
            End Using
            Assert.IsNotNull(info)
        End Using
    End Sub

#End Region

#Region " Global Assembly Cache "

    <TestMethod>
    <TestCategory("GAC")>
    Public Sub Gac_NetFrameworkAssemblyInstalledInGac_IsDetectedWithItsPath()
        If Not TestHelpers.IsRunningOnNetFramework OrElse Not RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
            Assert.Inconclusive("The Global Assembly Cache only exists for .NET Framework on Windows.")
        End If

        ' System.dll is always installed in the GAC (GAC_MSIL) and is loaded from there.
        Dim location As String = GetType(Uri).Assembly.Location
        Dim info As New AssemblyInfo(location)

        Assert.IsTrue(info.IsInstalledInGAC)
        Assert.AreEqual(location, info.GacPath, ignoreCase:=True)
        StringAssert.StartsWith(info.GacFullName, info.FullName)
        StringAssert.EndsWith(info.GacFullName, "ProcessorArchitecture=MSIL")
    End Sub

    <TestMethod>
    <TestCategory("GAC")>
    Public Sub Gac_UnsignedAssembly_IsNotInstalled()
        Dim info As New AssemblyInfo(TestHelpers.TestAssemblyPath)

        Assert.IsFalse(info.IsInstalledInGAC)
        Assert.IsNull(info.GacPath)
        Assert.IsNull(info.GacFullName)
    End Sub

    <TestMethod>
    <TestCategory("GAC")>
    Public Sub Gac_NetCoreAssembly_IsNeverQueried()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.SystemRuntime8
                o.PublicKey = FixtureOptions.EcmaStandardPublicKey
                o.Attributes.Add(FixtureAttribute.TargetFramework(".NETCoreApp,Version=v8.0"))
            End Sub)

        Assert.IsFalse(info.IsInstalledInGAC)
        Assert.IsNull(info.GacPath)
    End Sub

    <TestMethod>
    <TestCategory("GAC")>
    Public Sub Gac_SignedFixtureNotInstalled_IsNotInstalled()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PublicKey = FixtureOptions.EcmaStandardPublicKey
                o.Attributes.Add(FixtureAttribute.TargetFramework(".NETFramework,Version=v4.8"))
            End Sub)

        Assert.IsFalse(info.IsInstalledInGAC)
        Assert.IsNull(info.GacPath)
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>Converts bytes to a lowercase hexadecimal string.</summary>
    Private Shared Function ToHex(bytes As Byte()) As String
        Return String.Concat(bytes.Select(Function(b As Byte) b.ToString("x2")))
    End Function

#End Region

End Class

#End Region
