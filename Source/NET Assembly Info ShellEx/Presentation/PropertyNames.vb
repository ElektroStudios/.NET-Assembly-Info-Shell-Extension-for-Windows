
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " PropertyNames "

Namespace AssemblyInspection

    ''' <summary>
    ''' Defines the group titles, row names and common values shown in the property page.
    ''' </summary>
    Friend NotInheritable Class PropertyNames

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="PropertyNames"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Group Titles "

        ''' <summary>The title of the group with the Win32 version resource information.</summary>
        Friend Const GroupProduct As String = "Product"

        ''' <summary>The title of the group with the build, platform and assembly attribute information.</summary>
        Friend Const GroupBuild As String = "Build"

        ''' <summary>The title of the group with the assembly identity, GAC and file hash information.</summary>
        Friend Const GroupIdentity As String = "Identity"

        ''' <summary>The title of the group with the referenced assemblies and native modules.</summary>
        Friend Const GroupReferences As String = "References"

        ''' <summary>The title of the group with the assembly-level attributes.</summary>
        Friend Const GroupAttributes As String = "Attributes"

        ''' <summary>The title of the group with the deployment, configuration files and Win32 manifest information.</summary>
        Friend Const GroupRuntime As String = "Runtime"

#End Region

#Region " Common Values "

        ''' <summary>The value shown when a property has no value.</summary>
        Friend Const NoneValue As String = "(none)"

        ''' <summary>The value shown when a property value cannot be determined.</summary>
        Friend Const UnknownValue As String = "(unknown)"

        ''' <summary>The value shown for <see langword="True"/>.</summary>
        Friend Const YesValue As String = "Yes"

        ''' <summary>The value shown for <see langword="False"/>.</summary>
        Friend Const NoValue As String = "No"

        ''' <summary>The value shown for an attribute that is not applied to the assembly.</summary>
        Friend Const NotSpecifiedValue As String = "(not specified)"

#End Region

#Region " Product Rows "

        ''' <summary>The file name.</summary>
        Friend Const FileName As String = "File Name"

        ''' <summary>The original file name (version resource).</summary>
        Friend Const OriginalFileName As String = "Original File Name"

        ''' <summary>The product name (version resource).</summary>
        Friend Const ProductName As String = "Product Name"

        ''' <summary>The file description (version resource; <c>AssemblyTitle</c> in .NET).</summary>
        Friend Const FileDescription As String = "File Description"

        ''' <summary>The comments (version resource; <c>AssemblyDescription</c> in .NET).</summary>
        Friend Const Comments As String = "Comments"

        ''' <summary>The company name (version resource).</summary>
        Friend Const Company As String = "Company"

        ''' <summary>The copyright (version resource).</summary>
        Friend Const Copyright As String = "Copyright"

        ''' <summary>The trademarks (version resource).</summary>
        Friend Const Trademarks As String = "Trademarks"

        ''' <summary>The product version (version resource; <c>AssemblyInformationalVersion</c> in .NET).</summary>
        Friend Const ProductVersion As String = "Product Version"

        ''' <summary>The file version (version resource; <c>AssemblyFileVersion</c> in .NET).</summary>
        Friend Const FileVersion As String = "File Version"

#End Region

#Region " Build Rows "

        ''' <summary>The output type (class library, console or windows application).</summary>
        Friend Const OutputType As String = "Output Type"

        ''' <summary>The target platform (Any CPU, x86, x64, ARM64...).</summary>
        Friend Const Platform As String = "Platform"

        ''' <summary>The target framework.</summary>
        Friend Const TargetFramework As String = "Target Framework"

        ''' <summary>The target operating system platform.</summary>
        Friend Const TargetOsPlatform As String = "Target OS Platform"

        ''' <summary>The CLR (metadata) version.</summary>
        Friend Const ClrVersion As String = "CLR Version"

        ''' <summary>The build configuration.</summary>
        Friend Const BuildConfiguration As String = "Build Configuration"

        ''' <summary>The JIT code optimizations state.</summary>
        Friend Const CodeOptimizations As String = "Code Optimizations"

        ''' <summary>Whether the build is deterministic.</summary>
        Friend Const DeterministicBuild As String = "Deterministic Build"

        ''' <summary>The compilation time stamp.</summary>
        Friend Const CompilationTimeStamp As String = "Compilation Timestamp"

        ''' <summary>The debug symbols (PDB) information.</summary>
        Friend Const DebugSymbols As String = "Debug Symbols (PDB)"

        ''' <summary>The managed entry-point method.</summary>
        Friend Const EntryPoint As String = "Entry Point"

        ''' <summary>Whether the assembly is mixed-mode (C++/CLI).</summary>
        Friend Const MixedMode As String = "Mixed-Mode (Native Code)"

        ''' <summary>Whether the assembly contains ReadyToRun code.</summary>
        Friend Const ReadyToRun As String = "ReadyToRun (Precompiled)"

        ''' <summary>Whether the assembly is a reference assembly.</summary>
        Friend Const ReferenceAssembly As String = "Reference Assembly"

        ''' <summary>The detected root namespace.</summary>
        Friend Const RootNamespace As String = "Root Namespace"

        ''' <summary>The neutral resources language.</summary>
        Friend Const NeutralResourcesLanguage As String = "Neutral Resources Language"

        ''' <summary>The CLS compliance.</summary>
        Friend Const ClsCompliant As String = "CLS Compliant"

        ''' <summary>The COM visibility.</summary>
        Friend Const ComVisible As String = "COM Visible"

        ''' <summary>Whether WPF DPI awareness is disabled.</summary>
        Friend Const DpiAwarenessDisabled As String = "DPI Awareness Disabled"

        ''' <summary>The default DLL import search paths.</summary>
        Friend Const DllImportSearchPaths As String = "DLL Import Search Paths"

        ''' <summary>The declarative security permission requests.</summary>
        Friend Const SecurityPermissions As String = "Security Permissions"

#End Region

#Region " Identity Rows "

        ''' <summary>The assembly simple name.</summary>
        Friend Const AssemblyName As String = "Assembly Name"

        ''' <summary>The assembly version.</summary>
        Friend Const AssemblyVersion As String = "Assembly Version"

        ''' <summary>The assembly culture.</summary>
        Friend Const Culture As String = "Culture"

        ''' <summary>The assembly display name.</summary>
        Friend Const FullName As String = "Full Name"

        ''' <summary>The strong-name signature state.</summary>
        Friend Const StrongNameSignature As String = "Strong-Name Signature"

        ''' <summary>The public key token.</summary>
        Friend Const PublicKeyToken As String = "Public Key Token"

        ''' <summary>The assembly GUID (type library ID).</summary>
        Friend Const AssemblyGuid As String = "Assembly GUID"

        ''' <summary>The module version identifier.</summary>
        Friend Const ModuleVersionId As String = "Module Version ID (MVID)"

        ''' <summary>Whether the assembly is installed in the GAC.</summary>
        Friend Const InstalledInGac As String = "Installed in GAC"

        ''' <summary>The GAC display name.</summary>
        Friend Const GacName As String = "GAC Name"

        ''' <summary>The GAC path.</summary>
        Friend Const GacPath As String = "GAC Path"

        ''' <summary>The MD5 hash.</summary>
        Friend Const MD5Hash As String = "MD5 Hash"

        ''' <summary>The SHA-1 hash.</summary>
        Friend Const SHA1Hash As String = "SHA-1 Hash"

        ''' <summary>The SHA-256 hash.</summary>
        Friend Const SHA256Hash As String = "SHA-256 Hash"

#End Region

#Region " Attributes Rows "

        ''' <summary>The key file (AssemblyKeyFileAttribute).</summary>
        Friend Const KeyFile As String = "Key File"

        ''' <summary>The key container (AssemblyKeyNameAttribute).</summary>
        Friend Const KeyContainer As String = "Key Container"

        ''' <summary>The delay-sign flag (AssemblyDelaySignAttribute).</summary>
        Friend Const DelaySign As String = "Delay Sign"

        ''' <summary>A friend assembly (InternalsVisibleToAttribute); one row per friend.</summary>
        Friend Const InternalsVisibleTo As String = "Internals Visible To"

        ''' <summary>The prefix of the rows of the classes with an explicit GUID (candidate COM classes).</summary>
        Friend Const ComClassPrefix As String = "COM Class: "

        ''' <summary>The prefix of the AssemblyMetadataAttribute rows.</summary>
        Friend Const MetadataPrefix As String = "Metadata: "

#End Region

#Region " Runtime Rows "

        ''' <summary>Whether the file is a .NET single-file bundle.</summary>
        Friend Const SingleFileBundle As String = "Single-File Bundle"

        ''' <summary>The main assembly embedded in the single-file bundle.</summary>
        Friend Const BundledMainAssembly As String = "Bundled Main Assembly"

        ''' <summary>The platform of the native host of the single-file bundle.</summary>
        Friend Const BundleHostPlatform As String = "Bundle Host Platform"

        ''' <summary>The deployment model (framework-dependent or self-contained).</summary>
        Friend Const Deployment As String = "Deployment"

        ''' <summary>Whether the library is marked as trimmable.</summary>
        Friend Const Trimmable As String = "Trimmable"

        ''' <summary>The runtimeconfig.json file.</summary>
        Friend Const RuntimeConfig As String = "Runtime Config"

        ''' <summary>The target framework moniker declared in runtimeconfig.json.</summary>
        Friend Const RuntimeConfigTfm As String = "Runtime Config TFM"

        ''' <summary>The shared or included frameworks declared in runtimeconfig.json.</summary>
        Friend Const Frameworks As String = "Frameworks"

        ''' <summary>The roll-forward policy declared in runtimeconfig.json.</summary>
        Friend Const RollForward As String = "Roll Forward"

        ''' <summary>The prefix of the runtimeconfig.json configuration property rows.</summary>
        Friend Const ConfigPropertyPrefix As String = "Config: "

        ''' <summary>The application configuration file (app.config).</summary>
        Friend Const AppConfig As String = "App Config"

        ''' <summary>The supported runtimes declared in app.config.</summary>
        Friend Const SupportedRuntimes As String = "Supported Runtimes"

        ''' <summary>The prefix of the app.config binding redirect rows.</summary>
        Friend Const BindingRedirectPrefix As String = "Binding Redirect: "

        ''' <summary>The Win32 application manifest.</summary>
        Friend Const Win32Manifest As String = "Win32 Manifest"

        ''' <summary>The UAC requested execution level.</summary>
        Friend Const ExecutionLevel As String = "Requested Execution Level"

        ''' <summary>The UAC uiAccess setting.</summary>
        Friend Const UIAccess As String = "UI Access"

        ''' <summary>The Win32 DPI awareness settings.</summary>
        Friend Const DpiAwareness As String = "DPI Awareness"

        ''' <summary>The Windows versions declared as supported.</summary>
        Friend Const SupportedOS As String = "Supported OS"

        ''' <summary>Whether the manifest enables the Common Controls v6 (visual styles).</summary>
        Friend Const CommonControlsV6 As String = "Common Controls v6"

        ''' <summary>The longPathAware setting.</summary>
        Friend Const LongPathAware As String = "Long Path Aware"

        ''' <summary>The activeCodePage setting.</summary>
        Friend Const ActiveCodePage As String = "Active Code Page"

#End Region

#Region " References Rows "

        ''' <summary>The row name shown when there are no references.</summary>
        Friend Const NoReferences As String = "(no references)"

        ''' <summary>The value shown for a native module referenced by P/Invoke.</summary>
        Friend Const NativeLibraryValue As String = "Native library (P/Invoke)"

        ''' <summary>The suffix appended to version 0.0.0.0 references, which match any version.</summary>
        Friend Const AnyVersionSuffix As String = " (any version)"

        ''' <summary>The row name used to report a loading error.</summary>
        Friend Const ErrorRow As String = "Error"

#End Region

    End Class

End Namespace

#End Region
