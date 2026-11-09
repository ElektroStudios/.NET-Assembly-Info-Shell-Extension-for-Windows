
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection.PortableExecutable

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoBundleTests "

''' <summary>
''' Tests the support of .NET single-file bundles (synthetic bundles with the real format:
''' apphost placeholder in a writable data section, bundle header and file entries).
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoBundleTests

#Region " Helpers "

    ''' <summary>Builds the bytes of an executable fixture assembly with the specified name.</summary>
    Private Shared Function BuildApp(assemblyName As String) As Byte()
        Return FixtureAssemblyBuilder.Build(New FixtureOptions() With {
            .AssemblyName = assemblyName,
            .ImageKind = FixtureImageKind.ConsoleExe,
            .EntryPointTypeName = $"{assemblyName}.Program",
            .CoreLibrary = FixtureCoreLibrary.SystemRuntime8
        })
    End Function

    ''' <summary>Builds the bytes of a library fixture assembly with the specified name.</summary>
    Private Shared Function BuildLibrary(assemblyName As String) As Byte()
        Return FixtureAssemblyBuilder.Build(New FixtureOptions() With {.AssemblyName = assemblyName, .CoreLibrary = FixtureCoreLibrary.SystemRuntime8})
    End Function

#End Region

#Region " SingleFileBundle "

    <TestMethod>
    <DataRow(1)>
    <DataRow(2)>
    <DataRow(6)>
    Public Sub TryRead_EveryFormatVersion_ReadsAllEntries(majorVersion As Integer)
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("Contoso.App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("Contoso.App")),
            FixtureBundleEntry.FromText("Contoso.App.runtimeconfig.json", BundleFileType.RuntimeConfigJson, "{}"),
            FixtureBundleEntry.FromText("sub/dir/readme.txt", BundleFileType.Unknown, "hello")
        }

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries, majorVersion))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)

            Assert.IsNotNull(bundle)
            Assert.AreEqual(majorVersion, bundle.MajorVersion)
            Assert.AreEqual(3, bundle.Entries.Count)
            Assert.AreEqual("sub/dir/readme.txt", bundle.Entries(2).RelativePath)
            Assert.AreEqual(BundleFileType.RuntimeConfigJson, bundle.Entries(1).Type)
            Assert.AreEqual("hello", TextFileDecoder.Decode(bundle.ReadEntry(stream, bundle.Entries(2))))
            StringAssert.StartsWith(bundle.BundleId, "BundleId_")
        End Using
    End Sub

    <TestMethod>
    Public Sub ReadEntry_CompressedEntry_IsDecompressed()
        Dim content As Byte() = AssemblyInfoBundleTests.BuildApp("Contoso.App")
        Dim entries As FixtureBundleEntry() = {New FixtureBundleEntry("Contoso.App.dll", BundleFileType.Assembly, content, compress:=True)}

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)

            Assert.IsTrue(bundle.IsCompressed)
            Assert.IsTrue(bundle.Entries(0).IsCompressed)
            CollectionAssert.AreEqual(content, bundle.ReadEntry(stream, bundle.Entries(0)))
        End Using
    End Sub

    <TestMethod>
    Public Sub TryRead_PlainApphostWithoutBundle_ReturnsNothing()
        Using stream As New MemoryStream(FixtureBundleBuilder.Build(Nothing))
            Assert.IsNull(SingleFileBundle.TryRead(stream))
        End Using
    End Sub

    <TestMethod>
    Public Sub TryRead_ManagedAssembly_ReturnsNothing()
        Using stream As New MemoryStream(AssemblyInfoBundleTests.BuildLibrary("Contoso.Lib"))
            Assert.IsNull(SingleFileBundle.TryRead(stream))
        End Using
    End Sub

    <TestMethod>
    Public Sub TryRead_HeaderOffsetOutOfRange_ThrowsBadImageFormatException()
        Dim bundle As Byte() = FixtureBundleBuilder.Build({New FixtureBundleEntry("A.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("A"))})
        FixtureBundleBuilder.PatchHeaderOffset(bundle, bundle.Length + 100L)

        Using stream As New MemoryStream(bundle)
            Assert.ThrowsExactly(Of BadImageFormatException)(Sub() SingleFileBundle.TryRead(stream))
        End Using
    End Sub

    <TestMethod>
    Public Sub TryRead_TruncatedManifest_ThrowsBadImageFormatException()
        Dim bundle As Byte() = FixtureBundleBuilder.Build({New FixtureBundleEntry("A.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("A"))})
        FixtureBundleBuilder.PatchHeaderOffset(bundle, bundle.Length - 4L) ' Header starts 4 bytes before the end of the file.

        Using stream As New MemoryStream(bundle)
            Assert.ThrowsExactly(Of BadImageFormatException)(Sub() SingleFileBundle.TryRead(stream))
        End Using
    End Sub

    <TestMethod>
    Public Sub ResolveMainAssembly_ByBundleName_IsPreferred()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("Other.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("Other")),
            New FixtureBundleEntry("MyApp.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("MyApp"))
        }

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)
            Assert.AreEqual("MyApp.dll", bundle.ResolveMainAssembly(stream, "MyApp.exe").RelativePath)
        End Using
    End Sub

    <TestMethod>
    Public Sub ResolveMainAssembly_RenamedBundle_UsesDepsJsonProject()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("Helper.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("Helper")),
            New FixtureBundleEntry("Contoso.App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("Contoso.App")),
            FixtureBundleEntry.FromText("Contoso.App.deps.json", BundleFileType.DepsJson, ConfigurationParsingTests.SampleDepsJson)
        }

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)
            Assert.AreEqual("Contoso.App.dll", bundle.ResolveMainAssembly(stream, "Renamed.exe").RelativePath)
        End Using
    End Sub

    <TestMethod>
    Public Sub ResolveMainAssembly_RenamedBundleWithoutDepsJson_UsesTheOnlyExecutable()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("Helper.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("Helper")),
            New FixtureBundleEntry("Contoso.App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("Contoso.App"), compress:=True)
        }

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)
            Assert.AreEqual("Contoso.App.dll", bundle.ResolveMainAssembly(stream, "Renamed.exe").RelativePath)
        End Using
    End Sub

    <TestMethod>
    Public Sub ResolveMainAssembly_Ambiguous_ThrowsBadImageFormatException()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("A.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("A")),
            New FixtureBundleEntry("B.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("B"))
        }

        Using stream As New MemoryStream(FixtureBundleBuilder.Build(entries))
            Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)
            Assert.ThrowsExactly(Of BadImageFormatException)(Sub() bundle.ResolveMainAssembly(stream, "Renamed.exe"))
        End Using
    End Sub

#End Region

#Region " AssemblyInfo on Bundles "

    <TestMethod>
    Public Sub IsAssemblyFile_SingleFileBundle_ReturnsTrue()
        Using fixture As FixtureFile = FixtureBundleBuilder.Create({New FixtureBundleEntry("App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("App"))})
            Assert.IsTrue(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_PlainApphost_ReturnsFalse()
        Using fixture As New FixtureFile(FixtureBundleBuilder.Build(Nothing), ".exe")
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_BundleWithoutAssemblies_ReturnsFalse()
        Using fixture As FixtureFile = FixtureBundleBuilder.Create({FixtureBundleEntry.FromText("a.txt", BundleFileType.Unknown, "x")})
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_SingleFileBundle_ReadsTheEmbeddedMainAssembly()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("Contoso.App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("Contoso.App"), compress:=True),
            New FixtureBundleEntry("Helper.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildLibrary("Helper")),
            FixtureBundleEntry.FromText("Contoso.App.runtimeconfig.json", BundleFileType.RuntimeConfigJson, ConfigurationParsingTests.SampleRuntimeConfigSelfContained),
            FixtureBundleEntry.FromText("Contoso.App.deps.json", BundleFileType.DepsJson, ConfigurationParsingTests.SampleDepsJson)
        }

        Using fixture As FixtureFile = FixtureBundleBuilder.Create(entries, fileName:="Contoso.App")
            Dim info As New AssemblyInfo(fixture.Path)

            Assert.AreEqual("Contoso.App", info.Name)
            Assert.AreEqual("Contoso.App.Program.Main", info.EntryPointMethod)
            Assert.IsNotNull(info.Bundle)
            Assert.AreEqual("Contoso.App.dll", info.BundleMainAssembly.RelativePath)
            Assert.AreEqual("x64 (64-bit)", info.BundleHostPlatform)
            Assert.AreEqual(fixture.Path, info.FilePath)

            ' Configuration files embedded in the bundle.
            Assert.AreEqual(2, info.ConfigurationFiles.Count)
            Assert.IsTrue(info.ConfigurationFiles.All(Function(file As ConfigurationFile) file.IsEmbedded))
            Assert.IsTrue(info.RuntimeConfig.IsSelfContained)

            ' The hashes are those of the whole bundle file.
            Using sha As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
                Dim expected As String = String.Concat(sha.ComputeHash(File.ReadAllBytes(fixture.Path)).Select(Function(b As Byte) b.ToString("X2")))
                Assert.AreEqual(expected, info.SHA256Hash)
            End Using
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_SingleFileBundle_RuntimeGroupDescribesTheBundle()
        Dim entries As FixtureBundleEntry() = {
            New FixtureBundleEntry("App.dll", BundleFileType.Assembly, AssemblyInfoBundleTests.BuildApp("App"), compress:=True),
            FixtureBundleEntry.FromText("App.runtimeconfig.json", BundleFileType.RuntimeConfigJson, ConfigurationParsingTests.SampleRuntimeConfigSelfContained)
        }

        Using fixture As FixtureFile = FixtureBundleBuilder.Create(entries, fileName:="App")
            Dim runtime As PropertyGroup = AssemblyInfoPresenter.CreateGroups(New AssemblyInfo(fixture.Path), TimeZoneInfo.Utc).
                                           Single(Function(g As PropertyGroup) g.Title = PropertyNames.GroupRuntime)

            Assert.AreEqual("Yes (format v6.0, 2 files, compressed)", runtime.GetValue(PropertyNames.SingleFileBundle))
            Assert.AreEqual("App.dll", runtime.GetValue(PropertyNames.BundledMainAssembly))
            Assert.AreEqual("x64 (64-bit)", runtime.GetValue(PropertyNames.BundleHostPlatform))
            Assert.AreEqual("Self-contained, single-file", runtime.GetValue(PropertyNames.Deployment))
            Assert.AreEqual("App.runtimeconfig.json (embedded in bundle)", runtime.GetValue(PropertyNames.RuntimeConfig))
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_PlainApphost_ThrowsBadImageFormatException()
        Using fixture As New FixtureFile(FixtureBundleBuilder.Build(Nothing), ".exe")
            Assert.ThrowsExactly(Of BadImageFormatException)(
                Sub()
                    Dim unused As New AssemblyInfo(fixture.Path)
                End Sub)
        End Using
    End Sub

#End Region

End Class

#End Region
