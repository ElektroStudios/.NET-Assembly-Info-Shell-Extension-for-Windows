
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoMetadataTests "

''' <summary>
''' Tests the debug directory (deterministic builds, time stamp, PDB),
''' the root namespace heuristics and the references.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoMetadataTests

#Region " Deterministic Build / Time Stamp "

    <TestMethod>
    Public Sub TimeStamp_NonDeterministicBuild_IsCoffTimeStamp()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.TimeDateStamp = 1541372431UI)

        Assert.IsFalse(info.IsDeterministic)
        Assert.AreEqual(New DateTimeOffset(2018, 11, 4, 23, 0, 31, TimeSpan.Zero), info.TimeStamp.Value)
        Assert.AreEqual(TimeSpan.Zero, info.TimeStamp.Value.Offset)
    End Sub

    <TestMethod>
    Public Sub TimeStamp_DeterministicBuild_IsNothing()
        ' Regression: in deterministic builds the COFF time stamp is a content hash, so it was shown as a random date.
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Deterministic = True)

        Assert.IsTrue(info.IsDeterministic)
        Assert.IsFalse(info.TimeStamp.HasValue)
    End Sub

    <TestMethod>
    Public Sub GetTimeStamp_ValueAbove2038_IsTreatedAsUnsigned()
        ' 0x80000000 seconds = 2038-01-19T03:14:08Z; as a signed Int32 it would be negative (1901).
        Dim timeStamp As DateTimeOffset? = AssemblyInfo.GetTimeStamp(Integer.MinValue, isDeterministic:=False)

        Assert.AreEqual(New DateTimeOffset(2038, 1, 19, 3, 14, 8, TimeSpan.Zero), timeStamp.Value)
    End Sub

    <TestMethod>
    Public Sub GetTimeStamp_Deterministic_IsNothing()
        Assert.IsFalse(AssemblyInfo.GetTimeStamp(12345, isDeterministic:=True).HasValue)
    End Sub

#End Region

#Region " Debug Symbols "

    <TestMethod>
    Public Sub DebugSymbols_NoDebugDirectory_AreEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(String.Empty, info.PdbPath)
        Assert.IsFalse(info.HasEmbeddedPdb)
    End Sub

    <TestMethod>
    <DataRow(True)>
    <DataRow(False)>
    Public Sub DebugSymbols_CodeViewEntry_ExposesPathAndFormat(portable As Boolean)
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PdbPath = "C:\Build\obj\Release\Contoso.pdb"
                o.PortablePdb = portable
            End Sub)

        Assert.AreEqual("C:\Build\obj\Release\Contoso.pdb", info.PdbPath)
        Assert.AreEqual(portable, info.IsPortablePdb)
    End Sub

    <TestMethod>
    Public Sub DebugSymbols_EmbeddedPortablePdb_IsDetected()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PdbPath = "Contoso.pdb"
                o.EmbeddedPdb = True
            End Sub)

        Assert.IsTrue(info.HasEmbeddedPdb)
    End Sub

#End Region

#Region " Root Namespace "

    <TestMethod>
    Public Sub RootNamespace_VisualBasicMyTypes_StripsMyNamespace()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Contoso.App.MainForm")
                o.TypeNames.Add("Contoso.App.My.MyApplication")
                o.TypeNames.Add("Contoso.Other.Helper")
            End Sub)

        Assert.AreEqual("Contoso.App", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_VisualBasicMyResources_StripsMyResourcesNamespace()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.TypeNames.Add("WinForms_Test_Assembly.My.Resources.Resources"))

        Assert.AreEqual("WinForms_Test_Assembly", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_VisualBasicEmptyRootNamespace_IsEmpty()
        ' This is the case of the shell extension itself (<RootNamespace></RootNamespace>).
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("My.MyProject")
                o.TypeNames.Add("Contoso.Widgets.Widget")
            End Sub)

        Assert.AreEqual(String.Empty, info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_CSharpPropertiesTypes_StripsPropertiesNamespace()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Contoso.Lib.Widget")
                o.TypeNames.Add("Contoso.Lib.Properties.Resources")
            End Sub)

        Assert.AreEqual("Contoso.Lib", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_NoWellKnownTypes_IsLongestCommonNamespace()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Contoso.Core.Alpha")
                o.TypeNames.Add("Contoso.Core.Data.Beta")
                o.TypeNames.Add("Contoso.Core.Gamma+Nested")
            End Sub)

        Assert.AreEqual("Contoso.Core", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_CommonPrefixMatchesWholeSegmentsOnly()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Contoso.Core.Alpha")
                o.TypeNames.Add("Contoso.CoreX.Beta")
            End Sub)

        Assert.AreEqual("Contoso", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_UnrelatedNamespaces_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Alpha.One")
                o.TypeNames.Add("Beta.Two")
            End Sub)

        Assert.AreEqual(String.Empty, info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_CompilerEmbeddedAttributes_AreIgnored()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.TypeNames.Add("Microsoft.CodeAnalysis.EmbeddedAttribute")
                o.TypeNames.Add("System.Runtime.CompilerServices.NullableAttribute")
                o.TypeNames.Add("Contoso.Tool.Program")
            End Sub)

        Assert.AreEqual("Contoso.Tool", info.RootNamespace)
    End Sub

    <TestMethod>
    Public Sub RootNamespace_NoTypes_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(String.Empty, info.RootNamespace)
    End Sub

#End Region

#Region " References "

    <TestMethod>
    Public Sub ReferencedAssemblies_AreSortedByNameWithVersionAndToken()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.CoreLibrary = FixtureCoreLibrary.Mscorlib
                o.AssemblyReferences.Add(Tuple.Create("System.Xml", New Version(4, 0, 0, 0), FixtureAssemblyBuilder.HexToBytes("b77a5c561934e089")))
                o.AssemblyReferences.Add(Tuple.Create("Contoso.Unsigned", New Version(1, 0, 0, 0), CType(Nothing, Byte())))
            End Sub)

        Dim names As String() = info.ReferencedAssemblies.Select(Function(x As AssemblyName) x.Name).ToArray()
        CollectionAssert.AreEqual({"Contoso.Unsigned", "mscorlib", "System.Xml"}, names)

        Dim mscorlib As AssemblyName = info.ReferencedAssemblies(1)
        Assert.AreEqual(New Version(4, 0, 0, 0), mscorlib.Version)
        CollectionAssert.AreEqual(FixtureAssemblyBuilder.HexToBytes("b77a5c561934e089"), mscorlib.GetPublicKeyToken())
    End Sub

    <TestMethod>
    Public Sub ReferencedAssemblies_NoReferences_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.CoreLibrary = FixtureCoreLibrary.None)

        Assert.AreEqual(0, info.ReferencedAssemblies.Count)
    End Sub

    <TestMethod>
    Public Sub NativeModuleReferences_AreDistinctCaseInsensitiveAndSorted()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.NativeModules.Add("user32.dll")
                o.NativeModules.Add("Kernel32.dll")
                o.NativeModules.Add("USER32.dll")
            End Sub)

        CollectionAssert.AreEqual({"Kernel32.dll", "user32.dll"}, info.NativeModuleReferences.ToArray())
    End Sub

    <TestMethod>
    Public Sub NativeModuleReferences_NameWithoutExtension_GetsImplicitDllExtension()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.NativeModules.Add("kernel32")
                o.NativeModules.Add("kernel32.dll")
                o.NativeModules.Add("libc.so.6")
            End Sub)

        CollectionAssert.AreEqual({"kernel32.dll", "libc.so.6"}, info.NativeModuleReferences.ToArray())
    End Sub

#End Region

End Class

#End Region
