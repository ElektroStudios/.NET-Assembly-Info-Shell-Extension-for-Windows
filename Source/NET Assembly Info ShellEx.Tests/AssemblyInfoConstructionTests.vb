
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Text

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoConstructionTests "

''' <summary>
''' Tests the argument validation of <see cref="AssemblyInfo"/> and <see cref="AssemblyInfo.IsAssemblyFile"/>,
''' and the rejection of files that are not .NET assemblies.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoConstructionTests

#Region " Constructor "

    <TestMethod>
    Public Sub Constructor_NullPath_ThrowsArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New AssemblyInfo(Nothing)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Constructor_EmptyPath_ThrowsArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(
            Sub()
                Dim unused As New AssemblyInfo(String.Empty)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Constructor_MissingFile_ThrowsFileNotFoundException()
        Dim missingPath As String = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dll")

        Dim ex As FileNotFoundException = Assert.ThrowsExactly(Of FileNotFoundException)(
            Sub()
                Dim unused As New AssemblyInfo(missingPath)
            End Sub)

        Assert.AreEqual(missingPath, ex.FileName)
    End Sub

    <TestMethod>
    Public Sub Constructor_TextFile_ThrowsBadImageFormatException()
        Using fixture As New FixtureFile(Encoding.UTF8.GetBytes("This is not a PE image."), ".dll")
            Assert.ThrowsExactly(Of BadImageFormatException)(
                Sub()
                    Dim unused As New AssemblyInfo(fixture.Path)
                End Sub)
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_EmptyFile_ThrowsBadImageFormatException()
        Using fixture As New FixtureFile(Array.Empty(Of Byte)(), ".dll")
            Assert.ThrowsExactly(Of BadImageFormatException)(
                Sub()
                    Dim unused As New AssemblyInfo(fixture.Path)
                End Sub)
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_NativeImageWithoutCliHeader_ThrowsBadImageFormatException()
        Dim options As New FixtureOptions() With {.RemoveCliHeader = True}
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(options)
            Dim ex As BadImageFormatException = Assert.ThrowsExactly(Of BadImageFormatException)(
                Sub()
                    Dim unused As New AssemblyInfo(fixture.Path)
                End Sub)

            StringAssert.Contains(ex.Message, "no CLI metadata")
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_NetModuleWithoutManifest_ThrowsBadImageFormatException()
        Dim options As New FixtureOptions() With {.OmitAssemblyManifest = True}
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(options)
            Dim ex As BadImageFormatException = Assert.ThrowsExactly(Of BadImageFormatException)(
                Sub()
                    Dim unused As New AssemblyInfo(fixture.Path)
                End Sub)

            StringAssert.Contains(ex.Message, "module")
        End Using
    End Sub

    <TestMethod>
    Public Sub Constructor_ValidFixture_ExposesFilePath()
        Dim options As New FixtureOptions()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(options)
            Dim info As New AssemblyInfo(fixture.Path)
            Assert.AreEqual(fixture.Path, info.FilePath)
        End Using
    End Sub

#End Region

#Region " IsAssemblyFile "

    <TestMethod>
    Public Sub IsAssemblyFile_NullPath_ThrowsArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AssemblyInfo.IsAssemblyFile(Nothing))
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_MissingFile_ThrowsIOException()
        Dim missingPath As String = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.dll")
        Assert.ThrowsExactly(Of FileNotFoundException)(Sub() AssemblyInfo.IsAssemblyFile(missingPath))
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_ManagedAssembly_ReturnsTrue()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions())
            Assert.IsTrue(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_TestAssembly_ReturnsTrue()
        Assert.IsTrue(AssemblyInfo.IsAssemblyFile(TestHelpers.TestAssemblyPath))
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_TextFile_ReturnsFalse()
        Using fixture As New FixtureFile(Encoding.UTF8.GetBytes("MZ but not really a PE image"), ".exe")
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_EmptyFile_ReturnsFalse()
        Using fixture As New FixtureFile(Array.Empty(Of Byte)(), ".dll")
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_NativeImage_ReturnsFalse()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions() With {.RemoveCliHeader = True})
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_NetModule_ReturnsFalse()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions() With {.OmitAssemblyManifest = True})
            Assert.IsFalse(AssemblyInfo.IsAssemblyFile(fixture.Path))
        End Using
    End Sub

    <TestMethod>
    Public Sub IsAssemblyFile_DoesNotLockTheFile()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions())
            AssemblyInfo.IsAssemblyFile(fixture.Path)

            ' Opening for exclusive write access fails if the file handle was leaked.
            Using stream As New FileStream(fixture.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)
                Assert.IsTrue(stream.CanWrite)
            End Using
        End Using
    End Sub

#End Region

End Class

#End Region
