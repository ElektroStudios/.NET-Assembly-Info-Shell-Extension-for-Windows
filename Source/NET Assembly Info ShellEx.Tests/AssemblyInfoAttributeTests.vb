
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection
Imports System.Runtime.InteropServices

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports ShellExTests.Fixtures

#End Region

#Region " AssemblyInfoAttributeTests "

''' <summary>
''' Tests the decoding of the well-known assembly attributes and of the declarative security permissions.
''' </summary>
<TestClass>
Public NotInheritable Class AssemblyInfoAttributeTests

#Region " Constants "

    ''' <summary>The value of <c>DebuggingModes.Default</c>.</summary>
    Private Const DebuggingModesDefault As Integer = 1 << 0

    ''' <summary>The value of <c>DebuggingModes.IgnoreSymbolStoreSequencePoints</c>.</summary>
    Private Const DebuggingModesIgnoreSymbolStoreSequencePoints As Integer = 1 << 1

    ''' <summary>The value of <c>DebuggingModes.EnableEditAndContinue</c>.</summary>
    Private Const DebuggingModesEnableEditAndContinue As Integer = 1 << 2

    ''' <summary>The value of <c>DebuggingModes.DisableOptimizations</c>.</summary>
    Private Const DebuggingModesDisableOptimizations As Integer = 1 << 8

#End Region

#Region " Absent Attributes "

    <TestMethod>
    Public Sub Attributes_NonePresent_ReturnDefaults()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Nothing)

        Assert.IsFalse(info.Guid.HasValue)
        Assert.IsFalse(info.IsComVisible.HasValue)
        Assert.IsFalse(info.IsClsCompliant.HasValue)
        Assert.IsFalse(info.DefaultDllImportSearchPaths.HasValue)
        Assert.IsFalse(info.IsDpiAwarenessDisabled)
        Assert.IsFalse(info.IsReferenceAssembly)
        Assert.AreEqual(String.Empty, info.NeutralResourcesLanguage)
        Assert.IsNull(info.DefaultCulture)
        Assert.AreEqual(String.Empty, info.AssemblyConfiguration)
        Assert.IsTrue(info.IsOptimized)
        Assert.AreEqual("Release", info.BuildConfiguration)
        Assert.AreEqual(String.Empty, info.SecurityPermission)
    End Sub

#End Region

#Region " GuidAttribute "

    <TestMethod>
    Public Sub Guid_ValidValue_IsParsed()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.Guid("B9ADB13A-BC56-4615-8213-9A055D0B1F0F")))

        Assert.AreEqual(New Guid("B9ADB13A-BC56-4615-8213-9A055D0B1F0F"), info.Guid.Value)
    End Sub

    <TestMethod>
    Public Sub Guid_InvalidValue_IsNothing()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.Guid("not-a-guid")))

        Assert.IsFalse(info.Guid.HasValue)
    End Sub

#End Region

#Region " ComVisible / CLSCompliant "

    <TestMethod>
    <DataRow(True)>
    <DataRow(False)>
    Public Sub ComVisible_Present_IsDecoded(value As Boolean)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.ComVisible(value)))

        Assert.AreEqual(value, info.IsComVisible.Value)
    End Sub

    <TestMethod>
    <DataRow(True)>
    <DataRow(False)>
    Public Sub ClsCompliant_Present_IsDecoded(value As Boolean)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.ClsCompliant(value)))

        Assert.AreEqual(value, info.IsClsCompliant.Value)
    End Sub

#End Region

#Region " NeutralResourcesLanguage "

    <TestMethod>
    Public Sub NeutralResourcesLanguage_KnownCulture_IsResolved()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.NeutralResourcesLanguage("es-ES")))

        Assert.AreEqual("es-ES", info.NeutralResourcesLanguage)
        Assert.IsNotNull(info.DefaultCulture)
        Assert.AreEqual("es-ES", info.DefaultCulture.Name)
    End Sub

    <TestMethod>
    Public Sub NeutralResourcesLanguage_InvalidCulture_KeepsRawNameWithoutCulture()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.NeutralResourcesLanguage("$$-not-a-culture")))

        Assert.AreEqual("$$-not-a-culture", info.NeutralResourcesLanguage)
        Assert.IsNull(info.DefaultCulture)
    End Sub

#End Region

#Region " Other Attributes "

    <TestMethod>
    Public Sub DefaultDllImportSearchPaths_Present_IsDecodedAsFlags()
        Dim paths As DllImportSearchPath = DllImportSearchPath.ApplicationDirectory Or DllImportSearchPath.System32
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.DefaultDllImportSearchPaths(CInt(paths))))

        Assert.AreEqual(paths, info.DefaultDllImportSearchPaths.Value)
    End Sub

    <TestMethod>
    Public Sub DisableDpiAwareness_Present_IsTrue()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.DisableDpiAwareness()))

        Assert.IsTrue(info.IsDpiAwarenessDisabled)
    End Sub

    <TestMethod>
    Public Sub ReferenceAssembly_Present_IsTrue()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.ReferenceAssembly()))

        Assert.IsTrue(info.IsReferenceAssembly)
    End Sub

#End Region

#Region " Build Configuration / Optimizations "

    <TestMethod>
    Public Sub BuildConfiguration_AssemblyConfigurationAttribute_TakesPrecedence()
        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.Attributes.Add(FixtureAttribute.AssemblyConfiguration("Staging"))
                o.Attributes.Add(FixtureAttribute.Debuggable(DebuggingModesDefault Or DebuggingModesDisableOptimizations))
            End Sub)

        Assert.AreEqual("Staging", info.AssemblyConfiguration)
        Assert.AreEqual("Staging", info.BuildConfiguration)
        Assert.IsFalse(info.IsOptimized)
    End Sub

    <TestMethod>
    Public Sub BuildConfiguration_DebugCompilerModes_IsDebug()
        ' Regression: FileVersionInfo.IsDebug is never set by the .NET compilers, so every assembly was shown as "Release".
        Dim modes As Integer = DebuggingModesDefault Or DebuggingModesDisableOptimizations Or
                               DebuggingModesIgnoreSymbolStoreSequencePoints Or DebuggingModesEnableEditAndContinue
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.Debuggable(modes)))

        Assert.IsFalse(info.IsOptimized)
        Assert.AreEqual("Debug", info.BuildConfiguration)
    End Sub

    <TestMethod>
    Public Sub BuildConfiguration_ReleaseCompilerModes_IsRelease()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.Debuggable(DebuggingModesIgnoreSymbolStoreSequencePoints)))

        Assert.IsTrue(info.IsOptimized)
        Assert.AreEqual("Release", info.BuildConfiguration)
    End Sub

    <TestMethod>
    <DataRow(True, True, False)>
    <DataRow(True, False, True)>
    <DataRow(False, False, True)>
    Public Sub IsOptimized_LegacyBooleanConstructor_UsesOptimizerDisabledArgument(tracking As Boolean, optimizerDisabled As Boolean, expected As Boolean)
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.Debuggable(tracking, optimizerDisabled)))

        Assert.AreEqual(expected, info.IsOptimized)
    End Sub

    <TestMethod>
    Public Sub BuildConfiguration_WhiteSpaceConfigurationAttribute_FallsBackToOptimizations()
        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.Attributes.Add(FixtureAttribute.AssemblyConfiguration("   ")))

        Assert.AreEqual("Release", info.BuildConfiguration)
    End Sub

#End Region

#Region " Declarative Security "

    <TestMethod>
    Public Sub SecurityPermission_FlagsProperty_IsDecodedToFlagNames()
        Dim blob As Byte() = FixtureAssemblyBuilder.CreateBinaryPermissionSet(
            FixtureAssemblyBuilder.SecurityPermissionAttributeSerializedName,
            New KeyValuePair(Of String, FixtureArgument)("Flags", FixtureArgument.FromEnum(FixtureAssemblyBuilder.SecurityPermissionFlagSerializedName, (1 << 1) Or (1 << 3))))

        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestMinimum, blob)))

        Assert.AreEqual("RequestMinimum: SecurityPermission (UnmanagedCode, Execution)", info.SecurityPermission)
    End Sub

    <TestMethod>
    Public Sub SecurityPermission_BooleanProperty_IsShownAsNameValue()
        Dim blob As Byte() = FixtureAssemblyBuilder.CreateBinaryPermissionSet(
            FixtureAssemblyBuilder.SecurityPermissionAttributeSerializedName,
            New KeyValuePair(Of String, FixtureArgument)("Unrestricted", FixtureArgument.FromBoolean(True)))

        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestOptional, blob)))

        Assert.AreEqual("RequestOptional: SecurityPermission (Unrestricted=True)", info.SecurityPermission)
    End Sub

    <TestMethod>
    Public Sub SecurityPermission_OtherPermissionWithoutArguments_ShowsShortName()
        Dim blob As Byte() = FixtureAssemblyBuilder.CreateBinaryPermissionSet(
            "System.Security.Permissions.UIPermissionAttribute, mscorlib, Version=4.0.0.0")

        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestRefuse, blob)))

        Assert.AreEqual("RequestRefuse: UIPermission", info.SecurityPermission)
    End Sub

    <TestMethod>
    Public Sub SecurityPermission_XmlPermissionSet_IsIdentified()
        Dim blob As Byte() = Text.Encoding.Unicode.GetBytes("<PermissionSet class=""System.Security.PermissionSet"" version=""1""/>")

        Dim info As AssemblyInfo = TestHelpers.Inspect(Sub(o As FixtureOptions) o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestMinimum, blob)))

        Assert.AreEqual("RequestMinimum: (XML permission set)", info.SecurityPermission)
    End Sub

    <TestMethod>
    Public Sub SecurityPermission_MultipleRequests_AreSeparatedBySemicolon()
        Dim minimum As Byte() = FixtureAssemblyBuilder.CreateBinaryPermissionSet(
            FixtureAssemblyBuilder.SecurityPermissionAttributeSerializedName,
            New KeyValuePair(Of String, FixtureArgument)("Flags", FixtureArgument.FromEnum(FixtureAssemblyBuilder.SecurityPermissionFlagSerializedName, 1 << 3)))
        Dim refuse As Byte() = FixtureAssemblyBuilder.CreateBinaryPermissionSet(
            FixtureAssemblyBuilder.SecurityPermissionAttributeSerializedName,
            New KeyValuePair(Of String, FixtureArgument)("Flags", FixtureArgument.FromEnum(FixtureAssemblyBuilder.SecurityPermissionFlagSerializedName, 1 << 1)))

        Dim info As AssemblyInfo = TestHelpers.Inspect(
            Sub(o As FixtureOptions)
                o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestMinimum, minimum))
                o.DeclarativeSecurity.Add(Tuple.Create(DeclarativeSecurityAction.RequestRefuse, refuse))
            End Sub)

        Assert.AreEqual("RequestMinimum: SecurityPermission (Execution); RequestRefuse: SecurityPermission (UnmanagedCode)", info.SecurityPermission)
    End Sub

#End Region

End Class

#End Region
