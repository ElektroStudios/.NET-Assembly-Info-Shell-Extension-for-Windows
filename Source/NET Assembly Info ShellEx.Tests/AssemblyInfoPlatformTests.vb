
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection
Imports System.Reflection.PortableExecutable

Imports AssemblyInspection

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoPlatformTests "

''' <summary>
''' Tests the platform, processor architecture, output type, mixed-mode and ReadyToRun detection.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoPlatformTests

#Region " Constants "

    Private Const ILOnly As Integer = CInt(CorFlags.ILOnly)
    Private Const ILOnly32BitRequired As Integer = CInt(CorFlags.ILOnly Or CorFlags.Requires32Bit)
    Private Const ILOnly32BitPreferred As Integer = CInt(CorFlags.ILOnly Or CorFlags.Requires32Bit Or CorFlags.Prefers32Bit)
    Private Const NotILOnly As Integer = 0

    Private Const MachineI386 As Integer = CInt(Machine.I386)
    Private Const MachineAmd64 As Integer = CInt(Machine.Amd64)
    Private Const MachineArm64 As Integer = CInt(Machine.Arm64)
    Private Const MachineArmThumb2 As Integer = CInt(Machine.ArmThumb2)
    Private Const MachineIA64 As Integer = CInt(Machine.IA64)

#End Region

#Region " Platform / ProcessorArchitecture "

    <TestMethod>
    <DataRow(MachineI386, ILOnly, "Any CPU", ProcessorArchitecture.MSIL)>
    <DataRow(MachineI386, ILOnly32BitPreferred, "Any CPU (prefer 32-bit)", ProcessorArchitecture.MSIL)>
    <DataRow(MachineI386, ILOnly32BitRequired, "x86 (32-bit)", ProcessorArchitecture.X86)>
    <DataRow(MachineI386, NotILOnly, "x86 (32-bit)", ProcessorArchitecture.X86)>
    <DataRow(MachineAmd64, ILOnly, "x64 (64-bit)", ProcessorArchitecture.Amd64)>
    <DataRow(MachineArm64, ILOnly, "ARM64 (64-bit)", ProcessorArchitecture.None)>
    <DataRow(MachineArmThumb2, ILOnly, "ARM (32-bit)", ProcessorArchitecture.Arm)>
    <DataRow(MachineIA64, ILOnly, "Itanium (64-bit)", ProcessorArchitecture.IA64)>
    Public Sub Platform_FromFixture_MatchesHeaders(machine As Integer, flags As Integer, expectedPlatform As String, expectedArchitecture As ProcessorArchitecture)
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Machine = CType(machine, Machine)
                o.CorFlags = CType(flags, CorFlags)
            End Sub)

        Assert.AreEqual(expectedPlatform, info.Platform)
        Assert.AreEqual(expectedArchitecture, info.ProcessorArchitecture)
    End Sub

    <TestMethod>
    Public Sub Platform_X86OnlyAssembly_IsReadableFromAnyProcess()
        ' Regression: Assembly.Load() of an x86-only image throws BadImageFormatException in a 64-bit Explorer.
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Machine = Machine.I386
                o.CorFlags = CorFlags.ILOnly Or CorFlags.Requires32Bit
            End Sub)

        Assert.AreEqual(ProcessorArchitecture.X86, info.ProcessorArchitecture)
    End Sub

    <TestMethod>
    <DataRow(&H7B79, "Linux")>
    <DataRow(&H4644, "macOS")>
    <DataRow(&HADC4, "FreeBSD")>
    <DataRow(&H1993, "NetBSD")>
    Public Sub GetPlatformName_ReadyToRunOsOverride_IsDecoded(osMagic As Integer, expectedOs As String)
        Dim rawMachine As Machine = CType(CInt(Machine.Arm64) Xor osMagic, Machine)

        Assert.AreEqual($"ARM64 (64-bit) - {expectedOs}", PeImageInspector.GetPlatformName(rawMachine, CorFlags.ILOnly))
    End Sub

    <TestMethod>
    Public Sub ResolveMachine_KnownMachine_IsReturnedUnchanged()
        Dim targetOs As String = Nothing

        Assert.AreEqual(Machine.Amd64, PeImageInspector.ResolveMachine(Machine.Amd64, targetOs))
        Assert.AreEqual(String.Empty, targetOs)
    End Sub

    <TestMethod>
    Public Sub ResolveMachine_LinuxOverride_ReturnsRealMachineAndOs()
        Dim targetOs As String = Nothing

        Assert.AreEqual(Machine.Amd64, PeImageInspector.ResolveMachine(CType(CInt(Machine.Amd64) Xor &H7B79, Machine), targetOs))
        Assert.AreEqual("Linux", targetOs)
    End Sub

    <TestMethod>
    Public Sub GetPlatformName_UnknownMachine_ShowsHexValue()
        Assert.AreEqual("Unknown (0x1234)", PeImageInspector.GetPlatformName(CType(&H1234, Machine), CorFlags.ILOnly))
    End Sub

    <TestMethod>
    Public Sub GetProcessorArchitecture_LinuxReadyToRunX64_IsAmd64()
        Dim rawMachine As Machine = CType(CInt(Machine.Amd64) Xor &H7B79, Machine)

        Assert.AreEqual(ProcessorArchitecture.Amd64, PeImageInspector.GetProcessorArchitecture(rawMachine, CorFlags.ILOnly))
    End Sub

#End Region

#Region " Output Type "

    <TestMethod>
    <DataRow(CInt(FixtureImageKind.Dll), "Class Library (DLL)")>
    <DataRow(CInt(FixtureImageKind.ConsoleExe), "Console Application (EXE)")>
    <DataRow(CInt(FixtureImageKind.WindowsExe), "Windows Application (EXE)")>
    Public Sub OutputType_FromFixture_MatchesImageKind(kind As Integer, expected As String)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.ImageKind = CType(kind, FixtureImageKind))

        Assert.AreEqual(expected, info.OutputType)
    End Sub

    <TestMethod>
    Public Sub GetOutputType_NullHeaders_ThrowsArgumentNullException()
        Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PeImageInspector.GetOutputType(Nothing))
    End Sub

#End Region

#Region " Mixed-Mode / ReadyToRun "

    <TestMethod>
    Public Sub IsMixedMode_ILOnlyImage_IsFalse()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.IsFalse(info.IsMixedMode)
    End Sub

    <TestMethod>
    Public Sub IsMixedMode_ImageWithoutILOnlyFlag_IsTrue()
        ' Regression: mixed-mode (C++/CLI) images cannot be loaded with Assembly.Load(Byte()).
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Machine = Machine.Amd64
                o.CorFlags = CType(0, CorFlags)
            End Sub)

        Assert.IsTrue(info.IsMixedMode)
        Assert.AreEqual("x64 (64-bit)", info.Platform)
    End Sub

    <TestMethod>
    Public Sub IsReadyToRun_WithoutManagedNativeHeader_IsFalse()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.IsFalse(info.IsReadyToRun)
    End Sub

    <TestMethod>
    Public Sub IsReadyToRun_CrossgenFlags_IsReadyToRunButNotMixedMode()
        ' Crossgen clears ILOnly and sets ILLibrary; such images must not be reported as mixed-mode (C++/CLI).
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Machine = Machine.Amd64
                o.CorFlags = CorFlags.ILLibrary
                o.ReadyToRun = True
            End Sub)

        Assert.IsTrue(info.IsReadyToRun)
        Assert.IsFalse(info.IsMixedMode)
        Assert.AreEqual("x64 (64-bit)", info.Platform)
    End Sub

    <TestMethod>
    Public Sub IsReadyToRun_ILLibraryFlagOnly_IsTrue()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.CorFlags = CorFlags.ILOnly Or CorFlags.ILLibrary)

        Assert.IsTrue(info.IsReadyToRun)
    End Sub

    <TestMethod>
    Public Sub IsReadyToRun_WithManagedNativeHeader_IsTrue()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.ReadyToRun = True)

        Assert.IsTrue(info.IsReadyToRun)
    End Sub

#End Region

#Region " Entry Point "

    <TestMethod>
    Public Sub EntryPoint_LibraryWithoutEntryPoint_IsEmpty()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.AreEqual(String.Empty, info.EntryPointMethod)
    End Sub

    <TestMethod>
    Public Sub EntryPoint_ExecutableWithMain_IsMethodFullName()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.ImageKind = FixtureImageKind.ConsoleExe
                o.TypeNames.Add("Contoso.Helpers")
                o.EntryPointTypeName = "Contoso.App.Program"
                o.TypeNames.Add("Contoso.Zeta")
            End Sub)

        Assert.AreEqual("Contoso.App.Program.Main", info.EntryPointMethod)
    End Sub

    <TestMethod>
    Public Sub EntryPoint_NestedDeclaringType_UsesReflectionNotation()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.ImageKind = FixtureImageKind.ConsoleExe
                o.EntryPointTypeName = "Contoso.Outer+Middle+Program"
            End Sub)

        Assert.AreEqual("Contoso.Outer+Middle+Program.Main", info.EntryPointMethod)
    End Sub

    <TestMethod>
    Public Sub EntryPoint_GlobalNamespaceType_HasNoNamespacePrefix()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.ImageKind = FixtureImageKind.ConsoleExe
                o.EntryPointTypeName = "Program"
            End Sub)

        Assert.AreEqual("Program.Main", info.EntryPointMethod)
    End Sub

    <TestMethod>
    Public Sub EntryPoint_NativeEntryPointFlag_IsReportedAsNativeCode()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.ImageKind = FixtureImageKind.ConsoleExe
                o.EntryPointTypeName = "Program"
                o.CorFlags = CorFlags.NativeEntryPoint
            End Sub)

        Assert.AreEqual("(native code)", info.EntryPointMethod)
    End Sub

#End Region

End Class

#End Region
