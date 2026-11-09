
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection
Imports System.Reflection.Metadata
Imports System.Reflection.PortableExecutable

#End Region

#Region " FixtureOptions "

Namespace Fixtures

    ''' <summary>
    ''' Specifies the kind of image of a fixture assembly.
    ''' </summary>
    Friend Enum FixtureImageKind As Integer

        ''' <summary>A class library (DLL).</summary>
        Dll = 0

        ''' <summary>A console application (EXE, Windows CUI subsystem).</summary>
        ConsoleExe = 1

        ''' <summary>A windows application (EXE, Windows GUI subsystem).</summary>
        WindowsExe = 2

    End Enum

    ''' <summary>
    ''' Specifies the core library referenced by a fixture assembly.
    ''' </summary>
    Friend Enum FixtureCoreLibrary As Integer

        ''' <summary>No core library reference at all.</summary>
        None = 0

        ''' <summary><c>mscorlib, Version=4.0.0.0</c> (.NET Framework).</summary>
        Mscorlib = 1

        ''' <summary><c>System.Runtime, Version=8.0.0.0</c> (.NET 8).</summary>
        SystemRuntime8 = 2

        ''' <summary><c>netstandard, Version=2.0.0.0</c> (.NET Standard 2.0).</summary>
        Netstandard20 = 3

        ''' <summary><c>System.Runtime, Version=4.0.10.0</c> (.NET Standard 1.x / portable class library).</summary>
        SystemRuntimeLegacy = 4

    End Enum

    ''' <summary>
    ''' The options used by <see cref="FixtureAssemblyBuilder"/> to emit a synthetic assembly.
    ''' </summary>
    Friend NotInheritable Class FixtureOptions

        ''' <summary>The ECMA standard public key, whose public key token is <c>b77a5c561934e089</c>.</summary>
        Friend Shared ReadOnly EcmaStandardPublicKey As Byte() = {0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0}

        ''' <summary>Gets or sets the assembly simple name.</summary>
        Friend Property AssemblyName As String = $"Fixture_{System.Guid.NewGuid():N}"

        ''' <summary>Gets or sets the assembly version.</summary>
        Friend Property Version As New Version(1, 2, 3, 4)

        ''' <summary>Gets or sets the assembly culture name (empty for neutral).</summary>
        Friend Property CultureName As String = String.Empty

        ''' <summary>Gets or sets the assembly public key, or <see langword="Nothing"/> for no public key.</summary>
        Friend Property PublicKey As Byte()

        ''' <summary>Gets or sets a value indicating whether the assembly manifest is omitted (emits a .netmodule).</summary>
        Friend Property OmitAssemblyManifest As Boolean

        ''' <summary>Gets or sets the module version identifier.</summary>
        Friend Property Mvid As Guid = System.Guid.NewGuid()

        ''' <summary>Gets or sets the COFF machine type.</summary>
        Friend Property Machine As Machine = Machine.I386

        ''' <summary>Gets or sets the CLI header flags.</summary>
        Friend Property CorFlags As CorFlags = CorFlags.ILOnly

        ''' <summary>Gets or sets the image kind.</summary>
        Friend Property ImageKind As FixtureImageKind = FixtureImageKind.Dll

        ''' <summary>Gets or sets the metadata version string.</summary>
        Friend Property MetadataVersion As String = "v4.0.30319"

        ''' <summary>Gets or sets the referenced core library.</summary>
        Friend Property CoreLibrary As FixtureCoreLibrary = FixtureCoreLibrary.Mscorlib

        ''' <summary>Gets the assembly-level custom attributes.</summary>
        Friend ReadOnly Property Attributes As New List(Of FixtureAttribute)

        ''' <summary>Gets the full names of the types to define (nested types use '+'; declaring types are defined automatically).</summary>
        Friend ReadOnly Property TypeNames As New List(Of String)

        ''' <summary>Gets the additional referenced assemblies (name, version, public key token or <see langword="Nothing"/>).</summary>
        Friend ReadOnly Property AssemblyReferences As New List(Of Tuple(Of String, Version, Byte()))

        ''' <summary>Gets the native module (DLL) names referenced by P/Invoke.</summary>
        Friend ReadOnly Property NativeModules As New List(Of String)

        ''' <summary>Gets the assembly-level declarative security entries (action and permission set blob).</summary>
        Friend ReadOnly Property DeclarativeSecurity As New List(Of Tuple(Of DeclarativeSecurityAction, Byte()))

        ''' <summary>Gets or sets the full name of the type that declares a static <c>Main</c> entry-point method, or <see langword="Nothing"/> for no entry point.</summary>
        Friend Property EntryPointTypeName As String

        ''' <summary>Gets or sets the COFF header time stamp.</summary>
        Friend Property TimeDateStamp As UInteger = 1541372431UI ' 2018-11-04T23:00:31Z

        ''' <summary>Gets or sets a value indicating whether a Reproducible (deterministic build) debug entry is emitted.</summary>
        Friend Property Deterministic As Boolean

        ''' <summary>Gets or sets the PDB path of the CodeView debug entry, or <see langword="Nothing"/> for no CodeView entry.</summary>
        Friend Property PdbPath As String

        ''' <summary>Gets or sets a value indicating whether the CodeView entry refers to a portable PDB.</summary>
        Friend Property PortablePdb As Boolean = True

        ''' <summary>Gets or sets a value indicating whether an embedded portable PDB debug entry is emitted.</summary>
        Friend Property EmbeddedPdb As Boolean

        ''' <summary>Gets or sets a value indicating whether a ReadyToRun managed native header is patched into the CLI header.</summary>
        Friend Property ReadyToRun As Boolean

        ''' <summary>Gets or sets a value indicating whether the CLI header data directory is removed (emits a native-only PE).</summary>
        Friend Property RemoveCliHeader As Boolean

        ''' <summary>Gets or sets the Win32 manifest resource data (RT_MANIFEST), or <see langword="Nothing"/> for no resources.</summary>
        Friend Property Win32Manifest As Byte()

        ''' <summary>Gets or sets the integer ID of the Win32 manifest resource.</summary>
        Friend Property Win32ManifestId As Integer = 1

        ''' <summary>Gets the GUIDs to apply with a type-level <c>GuidAttribute</c>, keyed by type full name (the types are defined automatically).</summary>
        Friend ReadOnly Property TypeGuids As New Dictionary(Of String, String)(StringComparer.Ordinal)

        ''' <summary>Gets the full names of the types (of <see cref="TypeNames"/> or <see cref="TypeGuids"/>) that are defined as interfaces.</summary>
        Friend ReadOnly Property InterfaceTypeNames As New List(Of String)

        ''' <summary>Gets the flags of the additional assembly references, keyed by reference name.</summary>
        Friend ReadOnly Property AssemblyReferenceFlags As New Dictionary(Of String, AssemblyFlags)(StringComparer.Ordinal)

    End Class

End Namespace

#End Region
