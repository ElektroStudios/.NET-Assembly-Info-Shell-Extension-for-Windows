
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Reflection.Metadata.Ecma335
Imports System.Reflection.PortableExecutable
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports System.Text

Imports AssemblyInspection

Imports ElektroKit.Interop.Unmanaged.Win32
Imports ElektroKit.Interop.Unmanaged.Win32.Enums
Imports ElektroKit.Interop.Unmanaged.Win32.Interfaces
Imports ElektroKit.Interop.Unmanaged.Win32.Structures

#End Region

#Region " AssemblyInfo "

''' <summary>
''' Provides information about a .NET assembly file.
''' </summary>
''' <remarks>
''' All the information is read from the PE headers and the ECMA-335 metadata of the file
''' through <see cref="System.Reflection.Metadata"/>. The assembly is never loaded into the current process,
''' so assemblies of any architecture (x86, x64, ARM64), mixed-mode (C++/CLI) assemblies
''' and .NET Core / .NET Standard assemblies can be inspected in the same way.
''' </remarks>
Public NotInheritable Class AssemblyInfo

#Region " Constants "

    ''' <summary>
    ''' The value of <see cref="CodeViewDebugDirectoryData"/> minor version that identifies a portable PDB.
    ''' </summary>
    Private Const PortableCodeViewVersionMagic As UShort = &H504D

    ''' <summary>
    ''' The value of <c>DebuggingModes.DisableOptimizations</c>.
    ''' </summary>
    Private Const DebuggingModesDisableOptimizations As Integer = 1 << 8

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the assembly file path.
    ''' </summary>
    Public ReadOnly Property FilePath As String

    ''' <summary>
    ''' Gets the .NET platform family that the assembly was built for.
    ''' </summary>
    Public ReadOnly Property FrameworkKind As AssemblyFrameworkKind

    ''' <summary>
    ''' Gets a value that determines whether the assembly is a .NET Core / .NET 5+ assembly.
    ''' </summary>
    Public ReadOnly Property IsNetCore As Boolean
        Get
            Return Me.FrameworkKind = AssemblyFrameworkKind.NetCore
        End Get
    End Property

    ''' <summary>
    ''' Gets a value that determines whether <see cref="AssemblyInfo.FrameworkKind"/> was inferred from
    ''' the assembly references, because the assembly has no <c>TargetFrameworkAttribute</c>.
    ''' </summary>
    Public ReadOnly Property IsFrameworkKindInferred As Boolean

    ''' <summary>
    ''' Gets the <see cref="System.Reflection.AssemblyName"/> of the assembly.
    ''' </summary>
    Public ReadOnly Property AssemblyName As AssemblyName

    ''' <summary>
    ''' Gets the simple name of the assembly.
    ''' </summary>
    Public ReadOnly Property Name As String

    ''' <summary>
    ''' Gets the assembly version.
    ''' </summary>
    Public ReadOnly Property Version As Version

    ''' <summary>
    ''' Gets the culture name of the assembly, or "neutral".
    ''' </summary>
    Public ReadOnly Property Culture As String

    ''' <summary>
    ''' Gets the display name of the assembly
    ''' (e.g. <c>Name, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null</c>).
    ''' </summary>
    Public ReadOnly Property FullName As String

    ''' <summary>
    ''' Gets the <see cref="System.Diagnostics.FileVersionInfo"/> of the assembly file.
    ''' </summary>
    Public ReadOnly Property FileVersionInfo As FileVersionInfo

    ''' <summary>
    ''' Gets the portable executable (PE) headers of the assembly file.
    ''' </summary>
    Public ReadOnly Property PeHeader As PEHeaders

    ''' <summary>
    ''' Gets the <see cref="System.Reflection.ProcessorArchitecture"/> of the assembly.
    ''' </summary>
    Public ReadOnly Property ProcessorArchitecture As ProcessorArchitecture

    ''' <summary>
    ''' Gets a friendly name of the target platform of the assembly
    ''' (e.g. "Any CPU", "Any CPU (prefer 32-bit)", "x86 (32-bit)", "x64 (64-bit)", "ARM64 (64-bit)").
    ''' </summary>
    Public ReadOnly Property Platform As String

    ''' <summary>
    ''' Gets the output type of the assembly, using the Visual Studio project terminology
    ''' (e.g. "Class Library (DLL)", "Console Application (EXE)", "Windows Application (EXE)").
    ''' </summary>
    Public ReadOnly Property OutputType As String

    ''' <summary>
    ''' Gets the CLR (metadata) version the assembly was compiled against (e.g. "4.0.30319").
    ''' </summary>
    Public ReadOnly Property ClrVersion As String

    ''' <summary>
    ''' Gets the raw value of the <c>TargetFrameworkAttribute</c> (e.g. ".NETFramework,Version=v4.8"),
    ''' or <see cref="String.Empty"/> if the attribute is not present.
    ''' </summary>
    Public ReadOnly Property TargetFrameworkName As String

    ''' <summary>
    ''' Gets the target framework identifier (e.g. ".NETFramework", ".NETCoreApp", ".NETStandard"),
    ''' or <see cref="String.Empty"/> if unknown.
    ''' </summary>
    Public ReadOnly Property TargetFrameworkIdentifier As String

    ''' <summary>
    ''' Gets the target framework version, or <see langword="Nothing"/> if unknown.
    ''' </summary>
    Public ReadOnly Property TargetFrameworkVersion As Version

    ''' <summary>
    ''' Gets a friendly name of the target framework (e.g. ".NET Framework 4.8", ".NET 8.0", ".NET Standard 2.0"),
    ''' or <see cref="String.Empty"/> if unknown.
    ''' </summary>
    Public ReadOnly Property TargetFrameworkDisplayName As String

    ''' <summary>
    ''' Gets the target operating system platform of the assembly (e.g. "Windows 7.0" for net8.0-windows),
    ''' or <see cref="String.Empty"/> if the assembly is not platform-specific.
    ''' </summary>
    Public ReadOnly Property TargetPlatform As String

    ''' <summary>
    ''' Gets the culture name declared by the <c>NeutralResourcesLanguageAttribute</c>,
    ''' or <see cref="String.Empty"/> if the attribute is not present.
    ''' </summary>
    Public ReadOnly Property NeutralResourcesLanguage As String

    ''' <summary>
    ''' Gets the <see cref="CultureInfo"/> declared by the <c>NeutralResourcesLanguageAttribute</c>,
    ''' or <see langword="Nothing"/> if the attribute is not present or its culture name is not valid.
    ''' </summary>
    Public ReadOnly Property DefaultCulture As CultureInfo

    ''' <summary>
    ''' Gets the relative virtual address (RVA) of the native entry-point stub declared in the PE optional header.
    ''' <para></para>
    ''' For IL-only images this is the <c>_CorExeMain</c>/<c>_CorDllMain</c> import stub, and it is zero for 64-bit images.
    ''' </summary>
    Public ReadOnly Property EntryPointAddress As Integer

    ''' <summary>
    ''' Gets the full name of the managed entry-point method (e.g. "MyApp.Program.Main"),
    ''' "(native code)" for mixed-mode images with a native entry point,
    ''' or <see cref="String.Empty"/> if the assembly has no entry point.
    ''' </summary>
    Public ReadOnly Property EntryPointMethod As String

    ''' <summary>
    ''' Gets the strong-name signature state of the assembly.
    ''' </summary>
    Public ReadOnly Property StrongNameSignature As StrongNameSignatureKind

    ''' <summary>
    ''' Gets a value that determines whether the assembly is strong-name signed.
    ''' </summary>
    Public ReadOnly Property IsStrongNameSigned As Boolean
        Get
            Return Me.StrongNameSignature = StrongNameSignatureKind.Signed
        End Get
    End Property

    ''' <summary>
    ''' Gets the public key token (lowercase hexadecimal) of the assembly,
    ''' or <see cref="String.Empty"/> if the assembly has no public key.
    ''' </summary>
    Public ReadOnly Property PublicKeyToken As String

    ''' <summary>
    ''' Gets the detected root namespace of the assembly, or <see cref="String.Empty"/> if it could not be detected.
    ''' </summary>
    ''' <remarks>
    ''' An assembly does not necessarily define a root namespace, so this value is a best-effort heuristic.
    ''' </remarks>
    Public ReadOnly Property RootNamespace As String

    ''' <summary>
    ''' Gets the value of the assembly <see cref="GuidAttribute"/> (the type library ID used when exposed to COM),
    ''' or <see langword="Nothing"/> if the attribute is not present or not valid.
    ''' </summary>
    Public ReadOnly Property Guid As Guid?

    ''' <summary>
    ''' Gets the module version identifier (MVID), a GUID that is unique for every compilation.
    ''' </summary>
    Public ReadOnly Property ModuleVersionId As Guid

    ''' <summary>
    ''' Gets the value of the assembly <see cref="ComVisibleAttribute"/>,
    ''' or <see langword="Nothing"/> if the attribute is not present (in which case public types are COM-visible).
    ''' </summary>
    Public ReadOnly Property IsComVisible As Boolean?

    ''' <summary>
    ''' Gets the value of the assembly <see cref="CLSCompliantAttribute"/>,
    ''' or <see langword="Nothing"/> if the attribute is not present.
    ''' </summary>
    Public ReadOnly Property IsClsCompliant As Boolean?

    ''' <summary>
    ''' Gets a description of the assembly-level declarative security permission requests
    ''' (e.g. "RequestMinimum: SecurityPermission (UnmanagedCode)"),
    ''' or <see cref="String.Empty"/> if there are none.
    ''' </summary>
    Public ReadOnly Property SecurityPermission As String

    ''' <summary>
    ''' Gets a value that determines whether the assembly
    ''' has disabled dots per inch (DPI) awareness for all WPF user interface elements.
    ''' </summary>
    Public ReadOnly Property IsDpiAwarenessDisabled As Boolean

    ''' <summary>
    ''' Gets the value of the assembly <see cref="DefaultDllImportSearchPathsAttribute"/>,
    ''' or <see langword="Nothing"/> if the attribute is not present.
    ''' </summary>
    Public ReadOnly Property DefaultDllImportSearchPaths As DllImportSearchPath?

    ''' <summary>
    ''' Gets a value that determines whether the assembly was produced by a deterministic build.
    ''' <para></para>
    ''' In that case the COFF header time stamp is a content hash, not a date.
    ''' </summary>
    Public ReadOnly Property IsDeterministic As Boolean

    ''' <summary>
    ''' Gets the compilation time stamp (UTC) of the assembly,
    ''' or <see langword="Nothing"/> if the assembly was produced by a deterministic build.
    ''' </summary>
    Public ReadOnly Property TimeStamp As DateTimeOffset?

    ''' <summary>
    ''' Gets the value of the <c>AssemblyConfigurationAttribute</c> (e.g. "Release"),
    ''' or <see cref="String.Empty"/> if the attribute is not present.
    ''' </summary>
    Public ReadOnly Property AssemblyConfiguration As String

    ''' <summary>
    ''' Gets a value that determines whether JIT optimizations are enabled for the assembly,
    ''' according to its <see cref="DebuggableAttribute"/>.
    ''' </summary>
    Public ReadOnly Property IsOptimized As Boolean

    ''' <summary>
    ''' Gets the build configuration of the assembly: the <c>AssemblyConfigurationAttribute</c> value if present;
    ''' otherwise "Debug" or "Release", inferred from <see cref="AssemblyInfo.IsOptimized"/>.
    ''' </summary>
    Public ReadOnly Property BuildConfiguration As String
        Get
            If Not String.IsNullOrWhiteSpace(Me.AssemblyConfiguration) Then
                Return Me.AssemblyConfiguration
            End If
            Return If(Me.IsOptimized, "Release", "Debug")
        End Get
    End Property

    ''' <summary>
    ''' Gets a value that determines whether the assembly is a mixed-mode assembly
    ''' (contains native code, e.g. compiled with C++/CLI).
    ''' </summary>
    Public ReadOnly Property IsMixedMode As Boolean

    ''' <summary>
    ''' Gets a value that determines whether the assembly contains ReadyToRun (precompiled native) code,
    ''' according to its managed native header or its <c>ILLibrary</c> CLI flag.
    ''' </summary>
    Public ReadOnly Property IsReadyToRun As Boolean

    ''' <summary>
    ''' Gets a value that determines whether the assembly is a reference assembly (metadata only, not runnable).
    ''' </summary>
    Public ReadOnly Property IsReferenceAssembly As Boolean

    ''' <summary>
    ''' Gets the PDB file path recorded in the CodeView debug directory entry,
    ''' or <see cref="String.Empty"/> if there is none.
    ''' </summary>
    Public ReadOnly Property PdbPath As String

    ''' <summary>
    ''' Gets a value that determines whether the PDB referenced by <see cref="AssemblyInfo.PdbPath"/> is a portable PDB.
    ''' </summary>
    Public ReadOnly Property IsPortablePdb As Boolean

    ''' <summary>
    ''' Gets a value that determines whether a portable PDB is embedded in the assembly file.
    ''' </summary>
    Public ReadOnly Property HasEmbeddedPdb As Boolean

    ''' <summary>
    ''' Gets the assemblies referenced by the assembly.
    ''' </summary>
    Public ReadOnly Property ReferencedAssemblies As ReadOnlyCollection(Of AssemblyName)

    ''' <summary>
    ''' Gets the names of the native modules (DLLs) referenced by P/Invoke declarations of the assembly.
    ''' </summary>
    Public ReadOnly Property NativeModuleReferences As ReadOnlyCollection(Of String)

    ''' <summary>
    ''' Gets the MD5 hash (uppercase hexadecimal) of the assembly file.
    ''' </summary>
    Public ReadOnly Property MD5Hash As String

    ''' <summary>
    ''' Gets the SHA-1 hash (uppercase hexadecimal) of the assembly file.
    ''' </summary>
    Public ReadOnly Property SHA1Hash As String

    ''' <summary>
    ''' Gets the SHA-256 hash (uppercase hexadecimal) of the assembly file.
    ''' </summary>
    Public ReadOnly Property SHA256Hash As String

    ''' <summary>
    ''' Gets a value that determines whether the assembly is installed in the Global Assembly Cache (GAC).
    ''' <para></para>
    ''' Only strong-named .NET Framework assemblies can be installed in the GAC.
    ''' </summary>
    Public ReadOnly Property IsInstalledInGAC As Boolean
        Get
            Me.EnsureGacQueried()
            Return Me.gacPathB IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the assembly display name used to query the Global Assembly Cache (GAC),
    ''' or <see langword="Nothing"/> if the assembly is not installed in the GAC.
    ''' </summary>
    Public ReadOnly Property GacFullName As String
        Get
            Return If(Me.IsInstalledInGAC, Me.GetGacQueryName(), Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Gets the path of the assembly file inside the Global Assembly Cache (GAC) directory,
    ''' or <see langword="Nothing"/> if the assembly is not installed in the GAC.
    ''' </summary>
    Public ReadOnly Property GacPath As String
        Get
            Me.EnsureGacQueried()
            Return Me.gacPathB
        End Get
    End Property

    ''' <summary>
    ''' Gets the single-file bundle, when the file is a .NET single-file bundle (the assembly information
    ''' is then read from the main assembly embedded in it); otherwise, <see langword="Nothing"/>.
    ''' </summary>
    Public ReadOnly Property Bundle As SingleFileBundle

    ''' <summary>
    ''' Gets the bundle entry of the main assembly, when the file is a single-file bundle; otherwise, <see langword="Nothing"/>.
    ''' </summary>
    Public ReadOnly Property BundleMainAssembly As BundleEntry

    ''' <summary>
    ''' Gets the platform of the native host executable, when the file is a single-file bundle;
    ''' otherwise, <see cref="String.Empty"/>.
    ''' </summary>
    Public ReadOnly Property BundleHostPlatform As String

    ''' <summary>
    ''' Gets the Win32 application manifest embedded in the file, or <see langword="Nothing"/> if there is none.
    ''' </summary>
    Public ReadOnly Property Win32Manifest As Win32ManifestInfo

    ''' <summary>
    ''' Gets the configuration files of the assembly: its sidecar files (app.config, runtimeconfig.json, deps.json)
    ''' or the ones embedded in its single-file bundle.
    ''' </summary>
    Public ReadOnly Property ConfigurationFiles As ReadOnlyCollection(Of ConfigurationFile)

    ''' <summary>
    ''' Gets the settings of the .NET Framework application configuration file, or <see langword="Nothing"/> if there is none.
    ''' </summary>
    Public ReadOnly Property AppConfig As AppConfigInfo

    ''' <summary>
    ''' Gets the settings of the runtimeconfig.json file, or <see langword="Nothing"/> if there is none.
    ''' </summary>
    Public ReadOnly Property RuntimeConfig As RuntimeConfigInfo

    ''' <summary>
    ''' Gets the key/value pairs of the <c>AssemblyMetadataAttribute</c> instances (e.g. RepositoryUrl), in declaration order.
    ''' </summary>
    Public ReadOnly Property AssemblyMetadata As ReadOnlyCollection(Of KeyValuePair(Of String, String))

    ''' <summary>
    ''' Gets the friend assemblies declared with <c>InternalsVisibleToAttribute</c>.
    ''' </summary>
    Public ReadOnly Property InternalsVisibleTo As ReadOnlyCollection(Of String)

    ''' <summary>
    ''' Gets the value of the <c>AssemblyKeyFileAttribute</c>, or <see cref="String.Empty"/>.
    ''' </summary>
    Public ReadOnly Property KeyFile As String

    ''' <summary>
    ''' Gets the value of the <c>AssemblyKeyNameAttribute</c> (key container), or <see cref="String.Empty"/>.
    ''' </summary>
    Public ReadOnly Property KeyName As String

    ''' <summary>
    ''' Gets the value of the <c>AssemblyDelaySignAttribute</c>, or <see langword="Nothing"/> if it is not present.
    ''' </summary>
    Public ReadOnly Property DelaySign As Boolean?

    ''' <summary>
    ''' Gets the value of the <c>AssemblyMetadata("IsTrimmable", ...)</c> attribute written by the .NET SDK
    ''' for trimming-compatible libraries, or <see langword="Nothing"/> if it is not present.
    ''' </summary>
    Public ReadOnly Property IsTrimmable As Boolean?

    ''' <summary>
    ''' Gets the classes with an explicit <see cref="GuidAttribute"/> (candidate COM classes): full type name and CLSID.
    ''' </summary>
    Public ReadOnly Property ComClasses As ReadOnlyCollection(Of KeyValuePair(Of String, Guid))


#End Region

#Region " Private Fields "

    ''' <summary>
    ''' A value that determines whether the Global Assembly Cache (GAC) was already queried.
    ''' </summary>
    Private isGacQueried As Boolean

    ''' <summary>
    ''' ( Backing Field )
    ''' <para></para>
    ''' The path of the assembly inside the GAC, or <see langword="Nothing"/> if it is not installed in the GAC.
    ''' </summary>
    Private gacPathB As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AssemblyInfo"/> class.
    ''' </summary>
    ''' <param name="filepath">
    ''' The assembly file path.
    ''' </param>
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="filepath"/> is <see langword="Nothing"/> or empty.
    ''' </exception>
    '''
    ''' <exception cref="FileNotFoundException">
    ''' The file does not exist.
    ''' </exception>
    '''
    ''' <exception cref="BadImageFormatException">
    ''' The file is not a valid .NET assembly.
    ''' </exception>
    Public Sub New(filepath As String)

        If String.IsNullOrEmpty(filepath) Then
            Throw New ArgumentNullException(NameOf(filepath))
        End If
        If Not File.Exists(filepath) Then
            Throw New FileNotFoundException("The assembly file was not found.", filepath)
        End If

        Me.FilePath = filepath
        Dim rawFile As Byte() = File.ReadAllBytes(filepath)
        Dim assemblyImage As Byte() = rawFile

        ' The file itself: Win32 manifest, single-file bundle detection and configuration files.
        Using fileStream As New MemoryStream(rawFile, writable:=False)
            Dim fileHasMetadata As Boolean
            Using fileReader As New PEReader(fileStream, PEStreamOptions.LeaveOpen Or PEStreamOptions.PrefetchEntireImage)
                ' Throws BadImageFormatException if the file is not a PE image.
                Dim fileHeaders As PEHeaders = fileReader.PEHeaders
                fileHasMetadata = fileReader.HasMetadata
                Me.Win32Manifest = AssemblyInfo.ReadWin32Manifest(fileReader)
                Me.BundleHostPlatform = If(fileHasMetadata, String.Empty, PeImageInspector.GetPlatformName(fileHeaders.CoffHeader.Machine, CType(0, CorFlags)))
            End Using

            If Not fileHasMetadata Then
                Me.Bundle = SingleFileBundle.TryRead(fileStream)
                If Me.Bundle Is Nothing Then
                    Throw New BadImageFormatException($"The file is not a .NET assembly (it has no CLI metadata): {filepath}", filepath)
                End If
                Me.BundleMainAssembly = Me.Bundle.ResolveMainAssembly(fileStream, Path.GetFileName(filepath))
                assemblyImage = Me.Bundle.ReadEntry(fileStream, Me.BundleMainAssembly)
            End If

            Me.ConfigurationFiles = AssemblyInfo.ReadConfigurationFiles(filepath, Me.Bundle, fileStream)
        End Using

        ' The assembly: the file itself, or the main assembly embedded in the single-file bundle.
        Using stream As New MemoryStream(assemblyImage, writable:=False),
              peReader As New PEReader(stream, PEStreamOptions.PrefetchEntireImage)

            Me.PeHeader = peReader.PEHeaders
            If Not peReader.HasMetadata Then
                Throw New BadImageFormatException($"The file is not a .NET assembly (it has no CLI metadata): {filepath}", filepath)
            End If

            Dim reader As MetadataReader = peReader.GetMetadataReader()
            If Not reader.IsAssembly Then
                Throw New BadImageFormatException($"The file is a .NET module, not an assembly (it has no assembly manifest): {filepath}", filepath)
            End If

            Dim allAttributes As Dictionary(Of String, List(Of CustomAttribute)) = MetadataReaderHelper.GetAllAssemblyAttributes(reader)
            Dim attributes As Dictionary(Of String, CustomAttribute) = allAttributes.ToDictionary(
                Function(pair As KeyValuePair(Of String, List(Of CustomAttribute))) pair.Key,
                Function(pair As KeyValuePair(Of String, List(Of CustomAttribute))) pair.Value(0),
                StringComparer.Ordinal)
            Dim corHeader As CorHeader = Me.PeHeader.CorHeader
            Dim machine As Machine = Me.PeHeader.CoffHeader.Machine

            ' Identity
            Dim definition As AssemblyDefinition = reader.GetAssemblyDefinition()
            Dim cultureName As String = reader.GetString(definition.Culture)
            Me.Name = reader.GetString(definition.Name)
            Me.Version = definition.Version
            Me.Culture = If(String.IsNullOrEmpty(cultureName), "neutral", cultureName)
            Me.PublicKeyToken = AssemblyInfo.ComputePublicKeyToken(reader.GetBlobBytes(definition.PublicKey))
            Me.FullName = $"{Me.Name}, Version={Me.Version}, Culture={Me.Culture}, PublicKeyToken={If(Me.PublicKeyToken.Length = 0, "null", Me.PublicKeyToken)}"
            Me.AssemblyName = New AssemblyName(Me.FullName)
            Me.ModuleVersionId = reader.GetGuid(reader.GetModuleDefinition().Mvid)
            Me.StrongNameSignature = AssemblyInfo.GetStrongNameSignature(reader, corHeader.Flags)
            Me.Guid = AssemblyInfo.ParseGuid(AssemblyInfo.GetStringArgument(reader, attributes, "System.Runtime.InteropServices.GuidAttribute"))

            ' PE / CLI headers
            Me.ProcessorArchitecture = PeImageInspector.GetProcessorArchitecture(machine, corHeader.Flags)
            Me.Platform = PeImageInspector.GetPlatformName(machine, corHeader.Flags)
            Me.OutputType = PeImageInspector.GetOutputType(Me.PeHeader)
            Me.ClrVersion = reader.MetadataVersion.TrimStart("v"c)
            Me.EntryPointAddress = Me.PeHeader.PEHeader.AddressOfEntryPoint
            Me.EntryPointMethod = AssemblyInfo.GetEntryPointMethod(reader, corHeader)
            ' ReadyToRun (crossgen) images clear the ILOnly flag and set ILLibrary, but they are not mixed-mode (C++/CLI).
            Me.IsReadyToRun = (corHeader.ManagedNativeHeaderDirectory.Size > 0) OrElse
                              ((corHeader.Flags And CorFlags.ILLibrary) = CorFlags.ILLibrary)
            Me.IsMixedMode = ((corHeader.Flags And CorFlags.ILOnly) <> CorFlags.ILOnly) AndAlso Not Me.IsReadyToRun
            Dim debugData As DebugDirectoryData = AssemblyInfo.ReadDebugDirectory(peReader)
            Me.IsDeterministic = debugData.IsDeterministic
            Me.HasEmbeddedPdb = debugData.HasEmbeddedPdb
            Me.PdbPath = debugData.PdbPath
            Me.IsPortablePdb = debugData.IsPortablePdb
            Me.TimeStamp = AssemblyInfo.GetTimeStamp(Me.PeHeader.CoffHeader.TimeDateStamp, Me.IsDeterministic)

            ' Target framework
            Dim framework As TargetFrameworkData = AssemblyInfo.ReadTargetFramework(reader, attributes)
            Me.TargetFrameworkName = framework.Name
            Me.TargetFrameworkIdentifier = framework.Identifier
            Me.TargetFrameworkVersion = framework.Version
            Me.TargetFrameworkDisplayName = framework.DisplayName
            Me.FrameworkKind = framework.Kind
            Me.IsFrameworkKindInferred = framework.IsKindInferred
            Me.TargetPlatform = AssemblyInfo.FormatPlatformName(
                If(AssemblyInfo.GetStringArgument(reader, attributes, "System.Runtime.Versioning.TargetPlatformAttribute"),
                   AssemblyInfo.GetStringArgument(reader, attributes, "System.Runtime.Versioning.SupportedOSPlatformAttribute")))

            ' Assembly attributes
            Me.NeutralResourcesLanguage = If(AssemblyInfo.GetStringArgument(reader, attributes, "System.Resources.NeutralResourcesLanguageAttribute"), String.Empty)
            Me.DefaultCulture = AssemblyInfo.TryCreateCulture(Me.NeutralResourcesLanguage)
            Me.IsComVisible = AssemblyInfo.GetBooleanArgument(reader, attributes, "System.Runtime.InteropServices.ComVisibleAttribute")
            Me.IsClsCompliant = AssemblyInfo.GetBooleanArgument(reader, attributes, "System.CLSCompliantAttribute")
            Me.IsDpiAwarenessDisabled = attributes.ContainsKey("System.Windows.Media.DisableDpiAwarenessAttribute")
            Me.IsReferenceAssembly = attributes.ContainsKey("System.Runtime.CompilerServices.ReferenceAssemblyAttribute")
            Me.DefaultDllImportSearchPaths = AssemblyInfo.GetDllImportSearchPaths(reader, attributes)
            Me.AssemblyConfiguration = If(AssemblyInfo.GetStringArgument(reader, attributes, "System.Reflection.AssemblyConfigurationAttribute"), String.Empty)
            Me.IsOptimized = AssemblyInfo.GetIsOptimized(reader, attributes)
            Me.SecurityPermission = DeclarativeSecurityDecoder.Describe(reader)
            Me.RootNamespace = AssemblyInfo.DetectRootNamespace(reader)
            Me.KeyFile = If(AssemblyInfo.GetStringArgument(reader, attributes, "System.Reflection.AssemblyKeyFileAttribute"), String.Empty)
            Me.KeyName = If(AssemblyInfo.GetStringArgument(reader, attributes, "System.Reflection.AssemblyKeyNameAttribute"), String.Empty)
            Me.DelaySign = AssemblyInfo.GetBooleanArgument(reader, attributes, "System.Reflection.AssemblyDelaySignAttribute")
            Me.AssemblyMetadata = AssemblyInfo.ReadAssemblyMetadata(allAttributes)
            Me.InternalsVisibleTo = AssemblyInfo.ReadInternalsVisibleTo(allAttributes)
            Me.IsTrimmable = AssemblyInfo.GetMetadataBoolean(Me.AssemblyMetadata, "IsTrimmable")
            Me.ComClasses = AssemblyInfo.ReadComClasses(reader)

            ' References
            Me.ReferencedAssemblies = AssemblyInfo.ReadAssemblyReferences(reader)
            Me.NativeModuleReferences = AssemblyInfo.ReadNativeModuleReferences(reader)
        End Using

        ' Configuration files
        Dim appConfigFile As ConfigurationFile = Me.GetConfigurationFile(ConfigurationFileKind.AppConfig)
        Dim runtimeConfigFile As ConfigurationFile = Me.GetConfigurationFile(ConfigurationFileKind.RuntimeConfig)
        Me.AppConfig = If(appConfigFile Is Nothing, Nothing, New AppConfigInfo(appConfigFile.Content))
        Me.RuntimeConfig = If(runtimeConfigFile Is Nothing, Nothing, New RuntimeConfigInfo(runtimeConfigFile.Content))

        ' Hashes of the file on disk (for bundles, the whole bundle file).
        ' S4790: MD5 and SHA-1 are not used here for any security purpose (integrity, authentication or signing);
        ' they are informational checksums to identify and compare the file against third-party sources.
        ' SHA-256 is also provided for anyone that needs a collision-resistant hash.
#Disable Warning S4790 ' Using weak hashing algorithms is security-sensitive
        Me.MD5Hash = AssemblyInfo.ComputeHash(MD5.Create(), rawFile)
        Me.SHA1Hash = AssemblyInfo.ComputeHash(SHA1.Create(), rawFile)
#Enable Warning S4790
        Me.SHA256Hash = AssemblyInfo.ComputeHash(SHA256.Create(), rawFile)

        Me.FileVersionInfo = FileVersionInfo.GetVersionInfo(filepath)
    End Sub

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Determines whether the specified file is a .NET assembly (or a .NET single-file bundle),
    ''' by reading only its PE/CLI headers (and, for native images, the bundle manifest).
    ''' </summary>
    ''' <param name="filepath">
    ''' The file path.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if the file is a .NET assembly or a single-file bundle that contains assemblies;
    ''' <see langword="False"/> if it is not a PE image, has no CLI metadata, or is a module without manifest.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="filepath"/> is <see langword="Nothing"/> or empty.
    ''' </exception>
    '''
    ''' <exception cref="IOException">
    ''' The file cannot be read.
    ''' </exception>
    Public Shared Function IsAssemblyFile(filepath As String) As Boolean

        If String.IsNullOrEmpty(filepath) Then
            Throw New ArgumentNullException(NameOf(filepath))
        End If

        Using stream As New FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite Or FileShare.Delete)
            Try
                Using peReader As New PEReader(stream, PEStreamOptions.LeaveOpen)
                    If peReader.HasMetadata Then
                        Return peReader.GetMetadataReader().IsAssembly
                    End If
                End Using

                ' A native image: it can still be a .NET single-file bundle.
                Dim bundle As SingleFileBundle = SingleFileBundle.TryRead(stream)
                Return (bundle IsNot Nothing) AndAlso bundle.Entries.Any(Function(entry As BundleEntry) entry.Type = BundleFileType.Assembly)

            Catch ex As BadImageFormatException
                ' Not a PE image, or corrupted CLI metadata or bundle manifest: by definition, not a (readable) .NET assembly.
                Return False

            End Try
        End Using
    End Function

    ''' <summary>
    ''' Gets the configuration file of the specified kind.
    ''' </summary>
    ''' <param name="kind">
    ''' The kind of configuration file.
    ''' </param>
    ''' <returns>
    ''' The configuration file, or <see langword="Nothing"/> if there is none.
    ''' </returns>
    Public Function GetConfigurationFile(kind As ConfigurationFileKind) As ConfigurationFile

        Return Me.ConfigurationFiles.FirstOrDefault(Function(file As ConfigurationFile) file.Kind = kind)
    End Function

#End Region

#Region " Friend Methods "

    ''' <summary>
    ''' Parses a target framework name (e.g. ".NETFramework,Version=v4.8,Profile=Client").
    ''' </summary>
    ''' <param name="frameworkName">
    ''' The target framework name.
    ''' </param>
    '''
    ''' <param name="identifier">
    ''' When this method returns, contains the framework identifier.
    ''' </param>
    '''
    ''' <param name="version">
    ''' When this method returns, contains the framework version, or <see langword="Nothing"/> if missing or invalid.
    ''' </param>
    '''
    ''' <param name="profile">
    ''' When this method returns, contains the framework profile, or <see cref="String.Empty"/>.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if the framework name has at least an identifier; otherwise, <see langword="False"/>.
    ''' </returns>
    Friend Shared Function TryParseFrameworkName(frameworkName As String, ByRef identifier As String, ByRef version As Version, ByRef profile As String) As Boolean

        identifier = String.Empty
        version = Nothing
        profile = String.Empty

        If String.IsNullOrWhiteSpace(frameworkName) Then
            Return False
        End If

        Dim parts As String() = frameworkName.Split(","c)
        identifier = parts(0).Trim()

        For i As Integer = 1 To parts.Length - 1
            Dim separatorIndex As Integer = parts(i).IndexOf("="c)
            If separatorIndex = -1 Then
                Continue For
            End If

            Dim key As String = parts(i).Substring(0, separatorIndex).Trim()
            Dim value As String = parts(i).Substring(separatorIndex + 1).Trim()

            If key.Equals("Version", StringComparison.OrdinalIgnoreCase) Then
                Dim parsedVersion As Version = Nothing
                If System.Version.TryParse(value.TrimStart("v"c, "V"c), parsedVersion) Then
                    version = parsedVersion
                End If

            ElseIf key.Equals("Profile", StringComparison.OrdinalIgnoreCase) Then
                profile = value

            End If
        Next i

        Return identifier.Length <> 0
    End Function

    ''' <summary>
    ''' Gets a friendly target framework name (e.g. ".NET Framework 4.8", ".NET Core 3.1", ".NET 8.0", ".NET Standard 2.0").
    ''' </summary>
    ''' <param name="identifier">
    ''' The framework identifier.
    ''' </param>
    '''
    ''' <param name="version">
    ''' The framework version. It can be <see langword="Nothing"/>.
    ''' </param>
    '''
    ''' <param name="profile">
    ''' The framework profile. It can be <see langword="Nothing"/> or empty.
    ''' </param>
    ''' <returns>
    ''' The friendly target framework name.
    ''' </returns>
    Friend Shared Function GetFrameworkDisplayName(identifier As String, version As Version, profile As String) As String

        Dim versionText As String = AssemblyInfo.FormatFrameworkVersion(version)
        Dim name As String

        Select Case identifier
            Case ".NETFramework"
                name = $".NET Framework {versionText}"

            Case ".NETCoreApp"
                name = If((version IsNot Nothing) AndAlso (version.Major >= 5), $".NET {versionText}", $".NET Core {versionText}")

            Case ".NETStandard"
                name = $".NET Standard {versionText}"

            Case ".NETPortable"
                name = $".NET Portable {versionText}"

            Case Else
                name = $"{identifier} {versionText}"
        End Select

        name = name.Trim()
        If Not String.IsNullOrEmpty(profile) Then
            ' Portable profiles are already named "ProfileNNN"; framework profiles are not (e.g. "Client").
            Dim profileText As String = If(profile.StartsWith("Profile", StringComparison.OrdinalIgnoreCase), profile, $"{profile} Profile")
            name = $"{name} ({profileText})"
        End If
        Return name
    End Function

#End Region

#Region " Private Types "

    ''' <summary>
    ''' The target framework information of an assembly.
    ''' </summary>
    Private Structure TargetFrameworkData

        ''' <summary>The raw <c>TargetFrameworkAttribute</c> value, or <see cref="String.Empty"/>.</summary>
        Friend Name As String

        ''' <summary>The framework identifier, or <see cref="String.Empty"/>.</summary>
        Friend Identifier As String

        ''' <summary>The framework version, or <see langword="Nothing"/>.</summary>
        Friend Version As Version

        ''' <summary>The friendly framework name, or <see cref="String.Empty"/>.</summary>
        Friend DisplayName As String

        ''' <summary>The framework kind.</summary>
        Friend Kind As AssemblyFrameworkKind

        ''' <summary>A value that determines whether <see cref="TargetFrameworkData.Kind"/> was inferred.</summary>
        Friend IsKindInferred As Boolean

    End Structure

    ''' <summary>
    ''' The relevant information of the debug directory of an assembly.
    ''' </summary>
    Private Structure DebugDirectoryData

        ''' <summary>A value that determines whether there is a Reproducible (deterministic build) entry.</summary>
        Friend IsDeterministic As Boolean

        ''' <summary>A value that determines whether there is an embedded portable PDB entry.</summary>
        Friend HasEmbeddedPdb As Boolean

        ''' <summary>The PDB path of the first CodeView entry, or <see cref="String.Empty"/>.</summary>
        Friend PdbPath As String

        ''' <summary>A value that determines whether the CodeView entry refers to a portable PDB.</summary>
        Friend IsPortablePdb As Boolean

    End Structure

#End Region

#Region " Private Methods (Reading) "

    ''' <summary>
    ''' Reads the target framework, from the <c>TargetFrameworkAttribute</c> if present,
    ''' or inferred from the assembly references otherwise.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    ''' <returns>
    ''' The target framework information.
    ''' </returns>
    Private Shared Function ReadTargetFramework(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute)) As TargetFrameworkData

        Const AttributeName As String = "System.Runtime.Versioning.TargetFrameworkAttribute"

        Dim data As New TargetFrameworkData With {
            .Name = String.Empty,
            .Identifier = String.Empty,
            .DisplayName = String.Empty
        }

        Dim attribute As CustomAttribute
        If attributes.TryGetValue(AttributeName, attribute) Then
            Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
            data.Name = If(TryCast(value.FixedArguments(0).Value, String), String.Empty)

            Dim identifier As String = Nothing
            Dim version As Version = Nothing
            Dim profile As String = Nothing
            If AssemblyInfo.TryParseFrameworkName(data.Name, identifier, version, profile) Then
                data.Identifier = identifier
                data.Version = version
                data.DisplayName = AssemblyInfo.GetFrameworkDisplayName(identifier, version, profile)
            End If

            ' Prefer the display name chosen by the compiler/SDK, when present.
            For Each namedArgument As CustomAttributeNamedArgument(Of String) In value.NamedArguments
                If String.Equals(namedArgument.Name, "FrameworkDisplayName", StringComparison.Ordinal) Then
                    Dim displayName As String = TryCast(namedArgument.Value, String)
                    If Not String.IsNullOrWhiteSpace(displayName) Then
                        data.DisplayName = displayName
                    End If
                End If
            Next namedArgument
        End If

        Select Case data.Identifier
            Case ".NETFramework"
                data.Kind = AssemblyFrameworkKind.NetFramework

            Case ".NETCoreApp"
                data.Kind = AssemblyFrameworkKind.NetCore

            Case ".NETStandard", ".NETPortable"
                data.Kind = AssemblyFrameworkKind.NetStandard

            Case Else
                data.Kind = AssemblyInfo.InferFrameworkKind(reader)
                data.IsKindInferred = True
                If data.DisplayName.Length = 0 Then
                    data.DisplayName = AssemblyInfo.GetInferredFrameworkDisplayName(data.Kind, reader.MetadataVersion)
                End If
        End Select

        Return data
    End Function

    ''' <summary>
    ''' Reads the debug directory to determine the PDB information and whether the build is deterministic.
    ''' </summary>
    ''' <param name="peReader">
    ''' The PE reader.
    ''' </param>
    ''' <returns>
    ''' The debug directory information.
    ''' </returns>
    Private Shared Function ReadDebugDirectory(peReader As PEReader) As DebugDirectoryData

        Dim data As New DebugDirectoryData With {.PdbPath = String.Empty}

        For Each entry As DebugDirectoryEntry In peReader.ReadDebugDirectory()
            Select Case entry.Type
                Case DebugDirectoryEntryType.Reproducible
                    data.IsDeterministic = True

                Case DebugDirectoryEntryType.EmbeddedPortablePdb
                    data.HasEmbeddedPdb = True

                Case DebugDirectoryEntryType.CodeView
                    If data.PdbPath.Length = 0 Then
                        Dim codeView As CodeViewDebugDirectoryData = peReader.ReadCodeViewDebugDirectoryData(entry)
                        data.PdbPath = If(codeView.Path, String.Empty)
                        data.IsPortablePdb = entry.MinorVersion = AssemblyInfo.PortableCodeViewVersionMagic
                    End If
            End Select
        Next entry

        Return data
    End Function

    ''' <summary>
    ''' Converts the COFF header time stamp to a <see cref="DateTimeOffset"/> (UTC).
    ''' </summary>
    ''' <param name="timeDateStamp">
    ''' The COFF header time stamp: an unsigned 32-bit count of seconds since the Unix epoch.
    ''' </param>
    '''
    ''' <param name="isDeterministic">
    ''' A value that determines whether the image was produced by a deterministic build.
    ''' </param>
    ''' <returns>
    ''' The time stamp (UTC), or <see langword="Nothing"/> for deterministic builds
    ''' (where the value is a content hash, not a date).
    ''' </returns>
    Friend Shared Function GetTimeStamp(timeDateStamp As Integer, isDeterministic As Boolean) As DateTimeOffset?

        If isDeterministic Then
            Return Nothing
        End If

        Dim seconds As Long = CLng(timeDateStamp) And &HFFFFFFFFL
        Return DateTimeOffset.FromUnixTimeSeconds(seconds)
    End Function

    ''' <summary>
    ''' Reads the assemblies referenced by the assembly, sorted by name.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    ''' <returns>
    ''' The referenced assembly names.
    ''' </returns>
    Private Shared Function ReadAssemblyReferences(reader As MetadataReader) As ReadOnlyCollection(Of AssemblyName)

        Dim assemblies As New List(Of AssemblyName)
        For Each handle As AssemblyReferenceHandle In reader.AssemblyReferences
            Dim reference As AssemblyReference = reader.GetAssemblyReference(handle)
            Dim name As New AssemblyName() With {
                .Name = reader.GetString(reference.Name),
                .Version = reference.Version,
                .CultureName = reader.GetString(reference.Culture)
            }
            If (reference.Flags And AssemblyFlags.Retargetable) = AssemblyFlags.Retargetable Then
                name.Flags = name.Flags Or AssemblyNameFlags.Retargetable
            End If

            Dim keyOrToken As Byte() = reader.GetBlobBytes(reference.PublicKeyOrToken)
            If (reference.Flags And AssemblyFlags.PublicKey) = AssemblyFlags.PublicKey Then
                name.SetPublicKey(keyOrToken)
                name.SetPublicKeyToken(AssemblyInfo.HexToBytes(AssemblyInfo.ComputePublicKeyToken(keyOrToken)))
            Else
                name.SetPublicKeyToken(keyOrToken)
            End If
            assemblies.Add(name)
        Next handle

        Return assemblies.OrderBy(Function(x As AssemblyName) x.Name, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly()
    End Function

    ''' <summary>
    ''' Reads the distinct names of the native modules referenced by P/Invoke declarations, sorted by name.
    ''' <para></para>
    ''' Names without extension get the implicit ".dll" extension appended.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    ''' <returns>
    ''' The native module names.
    ''' </returns>
    Private Shared Function ReadNativeModuleReferences(reader As MetadataReader) As ReadOnlyCollection(Of String)

        Dim modules As New List(Of String)
        For i As Integer = 1 To reader.GetTableRowCount(TableIndex.ModuleRef)
            Dim handle As ModuleReferenceHandle = MetadataTokens.ModuleReferenceHandle(i)
            Dim moduleName As String = reader.GetString(reader.GetModuleReference(handle).Name)

            ' LoadLibrary appends ".dll" when the name has no extension, so "kernel32" and "kernel32.dll" are the same module.
            If Not Path.HasExtension(moduleName) Then
                moduleName = $"{moduleName}.dll"
            End If

            If Not modules.Contains(moduleName, StringComparer.OrdinalIgnoreCase) Then
                modules.Add(moduleName)
            End If
        Next i

        Return modules.OrderBy(Function(x As String) x, StringComparer.OrdinalIgnoreCase).ToList().AsReadOnly()
    End Function

#End Region

#Region " Private Methods (Attributes) "

    ''' <summary>
    ''' Gets the first fixed (constructor) argument of an assembly attribute, as <see cref="String"/>.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    '''
    ''' <param name="attributeName">
    ''' The full name of the attribute type.
    ''' </param>
    ''' <returns>
    ''' The argument value, or <see langword="Nothing"/> if the attribute is not present
    ''' or its first argument is not a string.
    ''' </returns>
    Private Shared Function GetStringArgument(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute), attributeName As String) As String

        Dim value As Object = AssemblyInfo.GetFirstArgument(reader, attributes, attributeName)
        Return TryCast(value, String)
    End Function

    ''' <summary>
    ''' Gets the first fixed (constructor) argument of an assembly attribute, as <see cref="Boolean"/>.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    '''
    ''' <param name="attributeName">
    ''' The full name of the attribute type.
    ''' </param>
    ''' <returns>
    ''' The argument value, or <see langword="Nothing"/> if the attribute is not present.
    ''' </returns>
    ''' <exception cref="BadImageFormatException">
    ''' The first argument of the attribute is not a <see cref="Boolean"/>.
    ''' </exception>
    Private Shared Function GetBooleanArgument(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute), attributeName As String) As Boolean?

        If Not attributes.ContainsKey(attributeName) Then
            Return Nothing
        End If

        Dim value As Object = AssemblyInfo.GetFirstArgument(reader, attributes, attributeName)
        If TypeOf value IsNot Boolean Then
            Throw New BadImageFormatException($"The first argument of '{attributeName}' is not a Boolean.")
        End If
        Return DirectCast(value, Boolean)
    End Function

    ''' <summary>
    ''' Gets the first fixed (constructor) argument of an assembly attribute.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    '''
    ''' <param name="attributeName">
    ''' The full name of the attribute type.
    ''' </param>
    ''' <returns>
    ''' The argument value, or <see langword="Nothing"/> if the attribute is not present or has no fixed arguments.
    ''' </returns>
    Private Shared Function GetFirstArgument(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute), attributeName As String) As Object

        Dim attribute As CustomAttribute
        If Not attributes.TryGetValue(attributeName, attribute) Then
            Return Nothing
        End If

        Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
        Return If(value.FixedArguments.Length = 0, Nothing, value.FixedArguments(0).Value)
    End Function

    ''' <summary>
    ''' Gets the value of the assembly <see cref="DefaultDllImportSearchPathsAttribute"/>.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    ''' <returns>
    ''' The search paths, or <see langword="Nothing"/> if the attribute is not present.
    ''' </returns>
    Private Shared Function GetDllImportSearchPaths(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute)) As DllImportSearchPath?

        Dim value As Object = AssemblyInfo.GetFirstArgument(reader, attributes, "System.Runtime.InteropServices.DefaultDllImportSearchPathsAttribute")
        If value Is Nothing Then
            Return Nothing
        End If
        Return CType(Convert.ToInt32(value, CultureInfo.InvariantCulture), DllImportSearchPath)
    End Function

    ''' <summary>
    ''' Determines whether JIT optimizations are enabled, according to the assembly <see cref="DebuggableAttribute"/>.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="attributes">
    ''' The assembly custom attributes.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if JIT optimizations are enabled (also when the attribute is not present);
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    Private Shared Function GetIsOptimized(reader As MetadataReader, attributes As Dictionary(Of String, CustomAttribute)) As Boolean

        Dim attribute As CustomAttribute
        If Not attributes.TryGetValue("System.Diagnostics.DebuggableAttribute", attribute) Then
            Return True
        End If

        Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
        Select Case value.FixedArguments.Length
            Case 1 ' DebuggableAttribute(DebuggingModes modes)
                Dim modes As Integer = Convert.ToInt32(value.FixedArguments(0).Value, CultureInfo.InvariantCulture)
                Return (modes And AssemblyInfo.DebuggingModesDisableOptimizations) = 0

            Case 2 ' DebuggableAttribute(bool isJITTrackingEnabled, bool isJITOptimizerDisabled)
                Return Not CBool(value.FixedArguments(1).Value)

            Case Else
                Throw New BadImageFormatException("Unexpected DebuggableAttribute constructor.")
        End Select
    End Function

#End Region

#Region " Private Methods (Helpers) "

    ''' <summary>
    ''' Gets the strong-name signature state of the assembly.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="flags">
    ''' The CLI header flags.
    ''' </param>
    ''' <returns>
    ''' The strong-name signature state.
    ''' </returns>
    Private Shared Function GetStrongNameSignature(reader As MetadataReader, flags As CorFlags) As StrongNameSignatureKind

        If reader.GetAssemblyDefinition().PublicKey.IsNil Then
            Return StrongNameSignatureKind.NotSigned
        End If
        Return If((flags And CorFlags.StrongNameSigned) = CorFlags.StrongNameSigned,
                  StrongNameSignatureKind.Signed,
                  StrongNameSignatureKind.DelaySigned)
    End Function

    ''' <summary>
    ''' Gets the full name of the managed entry-point method.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    '''
    ''' <param name="corHeader">
    ''' The CLI header.
    ''' </param>
    ''' <returns>
    ''' The full name of the entry-point method, "(native code)" for a native entry point,
    ''' or <see cref="String.Empty"/> if there is no entry point.
    ''' </returns>
    Private Shared Function GetEntryPointMethod(reader As MetadataReader, corHeader As CorHeader) As String

        Dim tokenOrRva As Integer = corHeader.EntryPointTokenOrRelativeVirtualAddress
        If tokenOrRva = 0 Then
            Return String.Empty
        End If

        If (corHeader.Flags And CorFlags.NativeEntryPoint) = CorFlags.NativeEntryPoint Then
            Return "(native code)"
        End If

        Dim handle As EntityHandle = MetadataTokens.EntityHandle(tokenOrRva)
        If handle.Kind <> HandleKind.MethodDefinition Then
            ' Multi-module assemblies can point to a File entry; there is nothing more to resolve here.
            Return $"(metadata token 0x{tokenOrRva:X8})"
        End If

        Return MetadataReaderHelper.GetMethodFullName(reader, CType(handle, MethodDefinitionHandle))
    End Function

    ''' <summary>
    ''' Infers the .NET platform family from the assembly references
    ''' (or from the assembly itself if it is a core library).
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    ''' <returns>
    ''' The inferred <see cref="AssemblyFrameworkKind"/>.
    ''' </returns>
    Private Shared Function InferFrameworkKind(reader As MetadataReader) As AssemblyFrameworkKind

        Dim ownName As String = reader.GetString(reader.GetAssemblyDefinition().Name)
        Select Case ownName
            Case "System.Private.CoreLib"
                Return AssemblyFrameworkKind.NetCore
            Case "mscorlib"
                Return AssemblyFrameworkKind.NetFramework
            Case "netstandard"
                Return AssemblyFrameworkKind.NetStandard
        End Select

        Dim referencesNetCore As Boolean
        Dim referencesNetStandard As Boolean
        Dim referencesMscorlib As Boolean
        Dim referencesLegacyContracts As Boolean

        For Each handle As AssemblyReferenceHandle In reader.AssemblyReferences
            Dim reference As AssemblyReference = reader.GetAssemblyReference(handle)
            Dim name As String = reader.GetString(reference.Name)
            Dim version As Version = reference.Version

            Select Case name
                Case "System.Private.CoreLib"
                    referencesNetCore = True

                Case "System.Runtime"
                    ' netcoreapp2.0+ references System.Runtime >= 4.2; netstandard1.x/PCL reference 4.0.x - 4.1.x.
                    If (version.Major >= 5) OrElse (version.Major = 4 AndAlso version.Minor >= 2) Then
                        referencesNetCore = True
                    Else
                        referencesLegacyContracts = True
                    End If

                Case "netstandard"
                    referencesNetStandard = True

                Case "mscorlib"
                    referencesMscorlib = True
            End Select
        Next handle

        If referencesNetCore Then
            Return AssemblyFrameworkKind.NetCore
        ElseIf referencesNetStandard Then
            Return AssemblyFrameworkKind.NetStandard
        ElseIf referencesMscorlib Then
            Return AssemblyFrameworkKind.NetFramework
        ElseIf referencesLegacyContracts Then
            Return AssemblyFrameworkKind.NetStandard
        Else
            Return AssemblyFrameworkKind.Unknown
        End If
    End Function

    ''' <summary>
    ''' Gets a friendly name for an inferred target framework.
    ''' </summary>
    ''' <param name="kind">
    ''' The inferred framework kind.
    ''' </param>
    '''
    ''' <param name="metadataVersion">
    ''' The metadata version string (e.g. "v4.0.30319").
    ''' </param>
    ''' <returns>
    ''' The friendly name, or <see cref="String.Empty"/> if the framework is unknown.
    ''' </returns>
    Private Shared Function GetInferredFrameworkDisplayName(kind As AssemblyFrameworkKind, metadataVersion As String) As String

        Select Case kind
            Case AssemblyFrameworkKind.NetFramework
                If metadataVersion.StartsWith("v1.", StringComparison.OrdinalIgnoreCase) Then
                    Return ".NET Framework 1.x (inferred)"
                ElseIf metadataVersion.StartsWith("v2.", StringComparison.OrdinalIgnoreCase) Then
                    Return ".NET Framework 2.0 - 3.5 (inferred)"
                Else
                    Return ".NET Framework 4.x (inferred)"
                End If

            Case AssemblyFrameworkKind.NetCore
                Return ".NET Core / .NET (inferred)"

            Case AssemblyFrameworkKind.NetStandard
                Return ".NET Standard (inferred)"

            Case Else
                Return String.Empty
        End Select
    End Function

    ''' <summary>
    ''' Formats a framework version for display (e.g. 4.8 -> "4.8"; 4.8.1 -> "4.8.1"; 8.0 -> "8.0").
    ''' </summary>
    ''' <param name="version">
    ''' The version. It can be <see langword="Nothing"/>.
    ''' </param>
    ''' <returns>
    ''' The formatted version, or <see cref="String.Empty"/> if <paramref name="version"/> is null.
    ''' </returns>
    Private Shared Function FormatFrameworkVersion(version As Version) As String

        If version Is Nothing Then
            Return String.Empty
        End If
        Return If(version.Build > 0, version.ToString(3), version.ToString(2))
    End Function

    ''' <summary>
    ''' Formats a platform name for display, separating the OS name and its version (e.g. "Windows7.0" -> "Windows 7.0").
    ''' </summary>
    ''' <param name="platformName">
    ''' The platform name. It can be <see langword="Nothing"/>.
    ''' </param>
    ''' <returns>
    ''' The formatted platform name, or <see cref="String.Empty"/>.
    ''' </returns>
    Private Shared Function FormatPlatformName(platformName As String) As String

        If String.IsNullOrWhiteSpace(platformName) Then
            Return String.Empty
        End If

        Dim trimmed As String = platformName.Trim()
        Dim digitIndex As Integer = trimmed.IndexOfAny("0123456789".ToCharArray())
        If digitIndex <= 0 Then
            Return trimmed
        End If

        Dim osName As String = trimmed.Substring(0, digitIndex)
        If Char.IsLower(osName(0)) Then
            osName = $"{Char.ToUpperInvariant(osName(0))}{osName.Substring(1)}"
        End If
        Return $"{osName} {trimmed.Substring(digitIndex)}"
    End Function

    ''' <summary>
    ''' Detects the root namespace of the assembly.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    ''' <returns>
    ''' The detected root namespace, or <see cref="String.Empty"/>.
    ''' </returns>
    ''' <remarks>
    ''' Strategies, by priority:
    ''' <list type="number">
    '''   <item><description>VB.NET "My" types (<c>[Root].My.MyApplication</c>, <c>[Root].My.Resources.Resources</c>, ...).</description></item>
    '''   <item><description>C# "Properties" types (<c>[Root].Properties.Resources</c>, <c>[Root].Properties.Settings</c>).</description></item>
    '''   <item><description>The longest common namespace of all the top-level user types.</description></item>
    ''' </list>
    ''' </remarks>
    Private Shared Function DetectRootNamespace(reader As MetadataReader) As String

        Dim vbMyTypeNames As String() = {"MyApplication", "MyComputer", "MyProject", "MySettings", "MySettingsProperty"}
        Dim csPropertiesTypeNames As String() = {"Resources", "Settings"}
        Dim ignoredNamespacePrefixes As String() = {"Microsoft.CodeAnalysis", "System.Runtime.CompilerServices", "System.Diagnostics.CodeAnalysis"}

        Dim userNamespaces As New List(Of String)

        For Each handle As TypeDefinitionHandle In reader.TypeDefinitions
            Dim definition As TypeDefinition = reader.GetTypeDefinition(handle)
            If Not definition.GetDeclaringType().IsNil Then
                Continue For ' Nested type.
            End If

            Dim typeName As String = reader.GetString(definition.Name)
            Dim namespaceName As String = reader.GetString(definition.Namespace)
            If typeName.StartsWith("<", StringComparison.Ordinal) Then
                Continue For ' <Module>, <PrivateImplementationDetails>, etc.
            End If

            ' VB.NET: [Root].My.MyApplication, [Root].My.Resources.Resources
            If vbMyTypeNames.Contains(typeName) Then
                Dim root As String = Nothing
                If AssemblyInfo.TryStripNamespaceSuffix(namespaceName, "My", root) Then
                    Return root
                End If
            End If
            If typeName = "Resources" Then
                Dim root As String = Nothing
                If AssemblyInfo.TryStripNamespaceSuffix(namespaceName, "My.Resources", root) Then
                    Return root
                End If
            End If

            ' C#: [Root].Properties.Resources, [Root].Properties.Settings
            If csPropertiesTypeNames.Contains(typeName) Then
                Dim root As String = Nothing
                If AssemblyInfo.TryStripNamespaceSuffix(namespaceName, "Properties", root) Then
                    Return root
                End If
            End If

            If namespaceName.Length <> 0 Then
                Dim isIgnored As Boolean = False
                For i As Integer = 0 To ignoredNamespacePrefixes.Length - 1
                    If namespaceName.StartsWith(ignoredNamespacePrefixes(i), StringComparison.Ordinal) Then
                        isIgnored = True
                        Exit For
                    End If
                Next i

                If Not isIgnored Then
                    userNamespaces.Add(namespaceName)
                End If
            End If

        Next handle

        Return AssemblyInfo.GetCommonNamespace(userNamespaces)
    End Function

    ''' <summary>
    ''' Removes a namespace suffix (e.g. "My" from "Root.My").
    ''' </summary>
    ''' <param name="namespaceName">
    ''' The namespace name.
    ''' </param>
    '''
    ''' <param name="suffix">
    ''' The suffix to remove.
    ''' </param>
    '''
    ''' <param name="root">
    ''' When this method returns, contains the namespace without the suffix
    ''' (<see cref="String.Empty"/> if the namespace was exactly the suffix).
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if the namespace ends with the suffix; otherwise, <see langword="False"/>.
    ''' </returns>
    Private Shared Function TryStripNamespaceSuffix(namespaceName As String, suffix As String, ByRef root As String) As Boolean

        If String.Equals(namespaceName, suffix, StringComparison.Ordinal) Then
            root = String.Empty
            Return True
        End If

        If namespaceName.EndsWith($".{suffix}", StringComparison.Ordinal) Then
            root = namespaceName.Substring(0, namespaceName.Length - suffix.Length - 1)
            Return True
        End If

        root = Nothing
        Return False
    End Function

    ''' <summary>
    ''' Gets the longest common namespace (whole segments) of the specified namespaces.
    ''' </summary>
    ''' <param name="namespaces">
    ''' The namespaces.
    ''' </param>
    ''' <returns>
    ''' The longest common namespace, or <see cref="String.Empty"/> if there is none.
    ''' </returns>
    Private Shared Function GetCommonNamespace(namespaces As List(Of String)) As String

        If namespaces.Count = 0 Then
            Return String.Empty
        End If

        Dim common As String() = namespaces(0).Split("."c)
        Dim commonLength As Integer = common.Length

        For Each namespaceName As String In namespaces
            Dim segments As String() = namespaceName.Split("."c)
            Dim length As Integer = Math.Min(commonLength, segments.Length)
            Dim matched As Integer = 0
            While (matched < length) AndAlso String.Equals(common(matched), segments(matched), StringComparison.Ordinal)
                matched += 1
            End While
            commonLength = matched
            If commonLength = 0 Then
                Return String.Empty
            End If
        Next namespaceName

        Return String.Join(".", common, 0, commonLength)
    End Function

    ''' <summary>
    ''' Parses a GUID string.
    ''' </summary>
    ''' <param name="value">
    ''' The GUID string. It can be <see langword="Nothing"/>.
    ''' </param>
    ''' <returns>
    ''' The parsed <see cref="System.Guid"/>, or <see langword="Nothing"/> if the value is null or not a valid GUID.
    ''' </returns>
    Private Shared Function ParseGuid(value As String) As Guid?

        Dim result As Guid
        Return If(System.Guid.TryParse(value, result), result, CType(Nothing, Guid?))
    End Function

    ''' <summary>
    ''' Creates a <see cref="CultureInfo"/> from a culture name declared in the assembly.
    ''' </summary>
    ''' <param name="cultureName">
    ''' The culture name.
    ''' </param>
    ''' <returns>
    ''' The <see cref="CultureInfo"/>, or <see langword="Nothing"/> if the name is empty or not a known culture.
    ''' </returns>
    Private Shared Function TryCreateCulture(cultureName As String) As CultureInfo

        If String.IsNullOrWhiteSpace(cultureName) Then
            Return Nothing
        End If

        Try
            Return CultureInfo.GetCultureInfo(cultureName)

        Catch ex As CultureNotFoundException
            ' The name comes from an arbitrary file; it is still exposed raw through 'NeutralResourcesLanguage'.
            Return Nothing

        End Try
    End Function

    ''' <summary>
    ''' Computes the public key token (last 8 bytes of the SHA-1 hash, reversed) of a public key.
    ''' </summary>
    ''' <param name="publicKey">
    ''' The public key.
    ''' </param>
    ''' <returns>
    ''' The public key token (lowercase hexadecimal), or <see cref="String.Empty"/> if the key is empty.
    ''' </returns>
    Friend Shared Function ComputePublicKeyToken(publicKey As Byte()) As String

        If (publicKey Is Nothing) OrElse (publicKey.Length = 0) Then
            Return String.Empty
        End If

        Dim hash As Byte()
        ' S4790: SHA-1 is mandated by the ECMA-335 specification to derive the public key token;
        ' any other algorithm would produce a token that does not match the one the CLR, the GAC and the references use.
#Disable Warning S4790 ' Using weak hashing algorithms is security-sensitive
        Using sha As SHA1 = SHA1.Create()
#Enable Warning S4790
            hash = sha.ComputeHash(publicKey)
        End Using

        Dim sb As New StringBuilder(capacity:=16)
        For i As Integer = hash.Length - 1 To hash.Length - 8 Step -1
            sb.Append(hash(i).ToString("x2", CultureInfo.InvariantCulture))
        Next i
        Return sb.ToString()
    End Function

    ''' <summary>
    ''' Converts an hexadecimal string to a byte array.
    ''' </summary>
    ''' <param name="hex">
    ''' The hexadecimal string.
    ''' </param>
    ''' <returns>
    ''' The byte array.
    ''' </returns>
    Private Shared Function HexToBytes(hex As String) As Byte()

        Dim bytes As Byte() = New Byte((hex.Length \ 2) - 1) {}
        For i As Integer = 0 To bytes.Length - 1
            bytes(i) = Convert.ToByte(hex.Substring(i * 2, 2), 16)
        Next i
        Return bytes
    End Function

    ''' <summary>
    ''' Computes a hash for the specified byte array, and disposes the hash algorithm.
    ''' </summary>
    ''' <param name="algorithm">
    ''' The hash algorithm.
    ''' </param>
    '''
    ''' <param name="data">
    ''' The byte array.
    ''' </param>
    ''' <returns>
    ''' An uppercase hexadecimal representation of the resulting hash value.
    ''' </returns>
    <DebuggerStepThrough>
    Private Shared Function ComputeHash(algorithm As HashAlgorithm, data As Byte()) As String

        Using algorithm
            Dim hash As Byte() = algorithm.ComputeHash(data)
            Dim sb As New StringBuilder(capacity:=hash.Length * 2)
            For Each b As Byte In hash
                sb.Append(b.ToString("X2", CultureInfo.InvariantCulture))
            Next b
            Return sb.ToString()
        End Using
    End Function

#End Region

#Region " Private Methods (Files and Attributes) "

    ''' <summary>
    ''' Reads the Win32 application manifest of a PE image.
    ''' </summary>
    ''' <param name="peReader">
    ''' The PE reader.
    ''' </param>
    ''' <returns>
    ''' The manifest, or <see langword="Nothing"/> if the image has none.
    ''' </returns>
    Private Shared Function ReadWin32Manifest(peReader As PEReader) As Win32ManifestInfo

        Dim resourceId As Integer
        Dim data As Byte() = Win32ResourceReader.ReadFirstResource(peReader, Win32ResourceReader.ManifestResourceType, resourceId)
        Return If(data Is Nothing, Nothing, New Win32ManifestInfo(data, resourceId))
    End Function

    ''' <summary>
    ''' Reads the configuration files of an assembly: the app.config sidecar file ("[file].config"),
    ''' and the runtimeconfig.json and deps.json files (embedded in the single-file bundle, or sidecar files).
    ''' </summary>
    ''' <param name="filepath">
    ''' The assembly (or bundle) file path.
    ''' </param>
    '''
    ''' <param name="bundle">
    ''' The single-file bundle, or <see langword="Nothing"/>.
    ''' </param>
    '''
    ''' <param name="bundleStream">
    ''' The stream of the bundle file.
    ''' </param>
    ''' <returns>
    ''' The configuration files that exist.
    ''' </returns>
    Private Shared Function ReadConfigurationFiles(filepath As String, bundle As SingleFileBundle, bundleStream As Stream) As ReadOnlyCollection(Of ConfigurationFile)

        Dim files As New List(Of ConfigurationFile)

        Dim appConfigPath As String = $"{filepath}.config"
        If File.Exists(appConfigPath) Then
            files.Add(New ConfigurationFile(ConfigurationFileKind.AppConfig, Path.GetFileName(appConfigPath), False, TextFileDecoder.Decode(File.ReadAllBytes(appConfigPath))))
        End If

        Dim directory As String = If(Path.GetDirectoryName(filepath), String.Empty)
        Dim baseName As String = Path.GetFileNameWithoutExtension(filepath)
        Dim kinds As New Dictionary(Of ConfigurationFileKind, BundleFileType) From {
            {ConfigurationFileKind.RuntimeConfig, BundleFileType.RuntimeConfigJson},
            {ConfigurationFileKind.DepsJson, BundleFileType.DepsJson}
        }

        For Each pair As KeyValuePair(Of ConfigurationFileKind, BundleFileType) In kinds
            Dim embedded As BundleEntry = If(bundle Is Nothing, Nothing, bundle.Entries.FirstOrDefault(Function(entry As BundleEntry) entry.Type = pair.Value))
            If embedded IsNot Nothing Then
                files.Add(New ConfigurationFile(pair.Key, embedded.RelativePath, True, TextFileDecoder.Decode(bundle.ReadEntry(bundleStream, embedded))))
                Continue For
            End If

            Dim extension As String = If(pair.Key = ConfigurationFileKind.RuntimeConfig, ".runtimeconfig.json", ".deps.json")
            Dim sidecarPath As String = Path.Combine(directory, $"{baseName}{extension}")
            If File.Exists(sidecarPath) Then
                files.Add(New ConfigurationFile(pair.Key, Path.GetFileName(sidecarPath), False, TextFileDecoder.Decode(File.ReadAllBytes(sidecarPath))))
            End If
        Next pair

        Return files.AsReadOnly()
    End Function

    ''' <summary>
    ''' Reads the key/value pairs of every <c>AssemblyMetadataAttribute</c>.
    ''' </summary>
    ''' <param name="allAttributes">
    ''' All the assembly custom attributes, grouped by attribute type.
    ''' </param>
    ''' <returns>
    ''' The key/value pairs, in declaration order.
    ''' </returns>
    Private Shared Function ReadAssemblyMetadata(allAttributes As Dictionary(Of String, List(Of CustomAttribute))) As ReadOnlyCollection(Of KeyValuePair(Of String, String))

        Dim pairs As New List(Of KeyValuePair(Of String, String))
        Dim instances As List(Of CustomAttribute) = Nothing
        If allAttributes.TryGetValue("System.Reflection.AssemblyMetadataAttribute", instances) Then
            For Each attribute As CustomAttribute In instances
                Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
                If value.FixedArguments.Length <> 2 Then
                    Throw New BadImageFormatException("Unexpected AssemblyMetadataAttribute constructor.")
                End If
                pairs.Add(New KeyValuePair(Of String, String)(If(TryCast(value.FixedArguments(0).Value, String), String.Empty),
                                                             If(TryCast(value.FixedArguments(1).Value, String), String.Empty)))
            Next attribute
        End If
        Return pairs.AsReadOnly()
    End Function

    ''' <summary>
    ''' Reads the friend assemblies declared with <c>InternalsVisibleToAttribute</c>.
    ''' </summary>
    ''' <param name="allAttributes">
    ''' All the assembly custom attributes, grouped by attribute type.
    ''' </param>
    ''' <returns>
    ''' The friend assembly names, in declaration order.
    ''' </returns>
    Private Shared Function ReadInternalsVisibleTo(allAttributes As Dictionary(Of String, List(Of CustomAttribute))) As ReadOnlyCollection(Of String)

        Dim friends As New List(Of String)
        Dim instances As List(Of CustomAttribute) = Nothing
        If allAttributes.TryGetValue("System.Runtime.CompilerServices.InternalsVisibleToAttribute", instances) Then
            For Each attribute As CustomAttribute In instances
                Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
                friends.Add(If(TryCast(value.FixedArguments(0).Value, String), String.Empty))
            Next attribute
        End If
        Return friends.AsReadOnly()
    End Function

    ''' <summary>
    ''' Gets the Boolean value of an <c>AssemblyMetadataAttribute</c> key.
    ''' </summary>
    ''' <param name="metadata">
    ''' The assembly metadata key/value pairs.
    ''' </param>
    '''
    ''' <param name="key">
    ''' The key (case-insensitive).
    ''' </param>
    ''' <returns>
    ''' The value, or <see langword="Nothing"/> if the key is not present or its value is not a Boolean.
    ''' </returns>
    Private Shared Function GetMetadataBoolean(metadata As IEnumerable(Of KeyValuePair(Of String, String)), key As String) As Boolean?

        For Each pair As KeyValuePair(Of String, String) In metadata
            Dim result As Boolean
            If String.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase) AndAlso Boolean.TryParse(pair.Value, result) Then
                Return result
            End If
        Next pair
        Return Nothing
    End Function

    ''' <summary>
    ''' Reads the classes (not interfaces) that have an explicit <see cref="GuidAttribute"/>.
    ''' </summary>
    ''' <param name="reader">
    ''' The metadata reader.
    ''' </param>
    ''' <returns>
    ''' The full type names and their GUIDs, in metadata order.
    ''' </returns>
    Private Shared Function ReadComClasses(reader As MetadataReader) As ReadOnlyCollection(Of KeyValuePair(Of String, Guid))

        Const GuidAttributeName As String = "System.Runtime.InteropServices.GuidAttribute"
        Dim classes As New List(Of KeyValuePair(Of String, Guid))

        For Each handle As TypeDefinitionHandle In reader.TypeDefinitions
            Dim definition As TypeDefinition = reader.GetTypeDefinition(handle)
            If (definition.Attributes And TypeAttributes.Interface) = TypeAttributes.Interface Then
                Continue For
            End If

            For Each attributeHandle As CustomAttributeHandle In definition.GetCustomAttributes()
                Dim attribute As CustomAttribute = reader.GetCustomAttribute(attributeHandle)
                If Not String.Equals(MetadataReaderHelper.GetAttributeTypeFullName(reader, attribute), GuidAttributeName, StringComparison.Ordinal) Then
                    Continue For
                End If

                Dim value As CustomAttributeValue(Of String) = attribute.DecodeValue(New AttributeValueTypeProvider())
                Dim classId As Guid
                If (value.FixedArguments.Length = 1) AndAlso System.Guid.TryParse(TryCast(value.FixedArguments(0).Value, String), classId) Then
                    classes.Add(New KeyValuePair(Of String, Guid)(MetadataReaderHelper.GetTypeFullName(reader, handle), classId))
                End If
            Next attributeHandle
        Next handle

        Return classes.AsReadOnly()
    End Function

#End Region

#Region " Private Methods (GAC) "

    ''' <summary>
    ''' Gets the assembly display name used to query the Global Assembly Cache (GAC).
    ''' </summary>
    ''' <returns>
    ''' The assembly display name, including the processor architecture when known.
    ''' </returns>
    Private Function GetGacQueryName() As String

        If Me.ProcessorArchitecture = ProcessorArchitecture.None Then
            Return Me.FullName
        End If
        Return $"{Me.FullName}, ProcessorArchitecture={Me.ProcessorArchitecture.ToString().ToUpperInvariant()}"
    End Function

    ''' <summary>
    ''' Queries the Global Assembly Cache (GAC) once, and caches the result.
    ''' </summary>
    Private Sub EnsureGacQueried()

        If Me.isGacQueried Then
            Return
        End If

        Me.gacPathB = Nothing
        If (Me.FrameworkKind = AssemblyFrameworkKind.NetFramework) AndAlso
           (Me.PublicKeyToken.Length <> 0) AndAlso
           RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then

            Me.gacPathB = AssemblyInfo.QueryGacPath(Me.GetGacQueryName())
        End If

        Me.isGacQueried = True
    End Sub

    ''' <summary>
    ''' Queries the Global Assembly Cache (GAC) for the specified assembly.
    ''' </summary>
    ''' <param name="gacQueryName">
    ''' The assembly display name.
    ''' </param>
    ''' <returns>
    ''' The path of the assembly inside the GAC, or <see langword="Nothing"/> if it is not installed in the GAC.
    ''' </returns>
    <DebuggerStepThrough>
    Private Shared Function QueryGacPath(gacQueryName As String) As String

        Const BufferLength As Integer = 1024

        Dim assemblyCache As IAssemblyCache = Nothing
        Dim hResult As Integer = NativeMethods.CreateAssemblyCache(assemblyCache, 0)
        If hResult <> 0 Then
            Marshal.ThrowExceptionForHR(hResult)
        End If

        Try
            Dim nativeInfo As New NativeAssemblyInfo With {
                .Size = Marshal.SizeOf(GetType(NativeAssemblyInfo)),
                .AssemblyFlags = AssemblyInfoFlags.AssemblyinfoFlagInstalled,
                .CharBuffer = BufferLength,
                .CurrentAssemblyPath = New String(ControlChars.NullChar, BufferLength)
            }

            hResult = assemblyCache.QueryAssemblyInfo(QueryAssemblyInfoFlags.QueryAsmInfoValidate, gacQueryName, nativeInfo)
            If hResult <> 0 Then
                ' Typically HRESULT_FROM_WIN32(ERROR_FILE_NOT_FOUND): the assembly is not installed in the GAC.
                Return Nothing
            End If

            Dim path As String = If(nativeInfo.CurrentAssemblyPath, String.Empty).TrimEnd(ControlChars.NullChar)
            ' Installed, but the path could not be retrieved: still report it as installed.
            Return If(path.Length = 0, String.Empty, path)

        Finally
            Marshal.ReleaseComObject(assemblyCache)

        End Try
    End Function

#End Region

End Class

#End Region
