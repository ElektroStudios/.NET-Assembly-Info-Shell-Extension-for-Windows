
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection
Imports System.Reflection.PortableExecutable
Imports System.Security.Cryptography
Imports System.Text

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoIdentityTests "

''' <summary>
''' Tests the assembly identity: name, version, culture, display name, public key token,
''' strong-name signature, MVID and file hashes.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoIdentityTests

    <TestMethod>
    Public Sub Identity_NeutralUnsignedAssembly_IsReadFromManifest()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.AssemblyName = "Contoso.Widgets"
                o.Version = New Version(5, 6, 7, 8)
            End Sub)

        Assert.AreEqual("Contoso.Widgets", info.Name)
        Assert.AreEqual(New Version(5, 6, 7, 8), info.Version)
        Assert.AreEqual("neutral", info.Culture)
        Assert.AreEqual(String.Empty, info.PublicKeyToken)
        Assert.AreEqual("Contoso.Widgets, Version=5.6.7.8, Culture=neutral, PublicKeyToken=null", info.FullName)
    End Sub

    <TestMethod>
    Public Sub Identity_SatelliteCulture_IsReadFromManifest()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.AssemblyName = "Contoso.Widgets.resources"
                o.CultureName = "es-ES"
            End Sub)

        Assert.AreEqual("es-ES", info.Culture)
        StringAssert.Contains(info.FullName, "Culture=es-ES")
    End Sub

    <TestMethod>
    Public Sub Identity_AssemblyName_MatchesFullName()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.AssemblyName = "Contoso.Signed"
                o.PublicKey = FixtureOptions.EcmaStandardPublicKey
            End Sub)

        Assert.AreEqual("Contoso.Signed", info.AssemblyName.Name)
        Assert.AreEqual(info.Version, info.AssemblyName.Version)
        Assert.AreEqual(info.FullName, info.AssemblyName.FullName)
    End Sub

    <TestMethod>
    Public Sub PublicKeyToken_EcmaStandardKey_IsWellKnownToken()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.PublicKey = FixtureOptions.EcmaStandardPublicKey)

        Assert.AreEqual("b77a5c561934e089", info.PublicKeyToken)
        StringAssert.EndsWith(info.FullName, "PublicKeyToken=b77a5c561934e089")
    End Sub

    <TestMethod>
    Public Sub ComputePublicKeyToken_NullOrEmptyKey_ReturnsEmpty()
        Assert.AreEqual(String.Empty, AssemblyInfo.ComputePublicKeyToken(Nothing))
        Assert.AreEqual(String.Empty, AssemblyInfo.ComputePublicKeyToken(Array.Empty(Of Byte)()))
    End Sub

    <TestMethod>
    Public Sub StrongNameSignature_NoPublicKey_IsNotSigned()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(StrongNameSignatureKind.NotSigned, info.StrongNameSignature)
        Assert.IsFalse(info.IsStrongNameSigned)
    End Sub

    <TestMethod>
    Public Sub StrongNameSignature_PublicKeyWithoutSignedFlag_IsDelaySigned()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.PublicKey = FixtureOptions.EcmaStandardPublicKey)

        Assert.AreEqual(StrongNameSignatureKind.DelaySigned, info.StrongNameSignature)
        Assert.IsFalse(info.IsStrongNameSigned)
    End Sub

    <TestMethod>
    Public Sub StrongNameSignature_PublicKeyWithSignedFlag_IsSigned()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.PublicKey = FixtureOptions.EcmaStandardPublicKey
                o.CorFlags = CorFlags.ILOnly Or CorFlags.StrongNameSigned
            End Sub)

        Assert.AreEqual(StrongNameSignatureKind.Signed, info.StrongNameSignature)
        Assert.IsTrue(info.IsStrongNameSigned)
    End Sub

    <TestMethod>
    Public Sub ModuleVersionId_IsReadFromModuleTable()
        Dim mvid As Guid = Guid.NewGuid()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Mvid = mvid)

        Assert.AreEqual(mvid, info.ModuleVersionId)
    End Sub

    <TestMethod>
    Public Sub Hashes_MatchHashesOfTheFileContent()
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(New FixtureOptions())
            Dim content As Byte() = File.ReadAllBytes(fixture.Path)
            Dim info As New AssemblyInfo(fixture.Path)

            Using md5Algorithm As MD5 = MD5.Create(),
                  sha1Algorithm As SHA1 = SHA1.Create(),
                  sha256Algorithm As SHA256 = SHA256.Create()

                Assert.AreEqual(AssemblyInfoIdentityTests.ToHex(md5Algorithm.ComputeHash(content)), info.MD5Hash)
                Assert.AreEqual(AssemblyInfoIdentityTests.ToHex(sha1Algorithm.ComputeHash(content)), info.SHA1Hash)
                Assert.AreEqual(AssemblyInfoIdentityTests.ToHex(sha256Algorithm.ComputeHash(content)), info.SHA256Hash)
            End Using
        End Using
    End Sub

    <TestMethod>
    Public Sub Hashes_AreUppercaseHexadecimalOfTheExpectedLength()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(32, info.MD5Hash.Length)
        Assert.AreEqual(40, info.SHA1Hash.Length)
        Assert.AreEqual(64, info.SHA256Hash.Length)
        Assert.AreEqual(info.SHA256Hash.ToUpperInvariant(), info.SHA256Hash)
    End Sub

    ''' <summary>Converts bytes to an uppercase hexadecimal string.</summary>
    Private Shared Function ToHex(bytes As Byte()) As String
        Dim sb As New StringBuilder(bytes.Length * 2)
        For Each b As Byte In bytes
            sb.Append(b.ToString("X2"))
        Next b
        Return sb.ToString()
    End Function

End Class

#End Region
