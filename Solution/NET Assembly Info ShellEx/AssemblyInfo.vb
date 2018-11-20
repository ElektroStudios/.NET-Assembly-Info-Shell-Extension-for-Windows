
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Specialized
Imports System.Globalization
Imports System.IO
Imports System.Reflection
Imports System.Reflection.PortableExecutable
Imports System.Resources
Imports System.Runtime.InteropServices
Imports System.Runtime.Versioning
Imports System.Security.Cryptography
Imports System.Security.Permissions
Imports System.Text
Imports System.Web
Imports System.Windows.Media

Imports ElektroKit.Interop.Unmanaged.Win32
Imports ElektroKit.Interop.Unmanaged.Win32.Enums
Imports ElektroKit.Interop.Unmanaged.Win32.Interfaces
Imports ElektroKit.Interop.Unmanaged.Win32.Structures

#End Region

#Region " AssemblyInfo "

''' ----------------------------------------------------------------------------------------------------
''' <summary>
''' Provides information about an <see cref="System.Reflection.Assembly"/>.
''' </summary>
''' ----------------------------------------------------------------------------------------------------
Public NotInheritable Class AssemblyInfo

#Region " Private Fields "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' The assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private ReadOnly assembly As Assembly

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' The assembly file path.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private ReadOnly filepath As String

#End Region

#Region " Properties "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the <see cref="System.Reflection.AssemblyName"/> of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property AssemblyName As AssemblyName

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the <see cref="System.Diagnostics.FileVersionInfo"/> of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property FileVersionInfo As FileVersionInfo

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the platform of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property Platform As String
        Get
            Dim peKind As PortableExecutableKinds
            Me.assembly.ManifestModule.GetPEKind(peKind, Nothing)

            If peKind.HasFlag(PortableExecutableKinds.PE32Plus) Then
                Return "64-bit"

            ElseIf peKind.HasFlag(PortableExecutableKinds.Required32Bit) OrElse
                   peKind.HasFlag(PortableExecutableKinds.Unmanaged32Bit) Then
                Return "32-bit"

            ElseIf peKind.HasFlag(PortableExecutableKinds.Preferred32Bit) Then
                Return "Any CPU ( Prefer 32-bit )"

            Else
                Return "Any CPU"

            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the CLR runtime version of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property ClrVersion As String
        Get
            Return Me.assembly.ImageRuntimeVersion.TrimStart("v"c)
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the target .NET Framework name of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property TargetNetFrameworkName As String
        Get
            Dim attrib As TargetFrameworkAttribute = Me.assembly.GetCustomAttribute(Of TargetFrameworkAttribute)
            If (attrib Is Nothing) Then
                Return String.Empty
            Else
                Return attrib.FrameworkDisplayName
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the target .NET Framework version of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property TargetNetFrameworkVersion As Version
        Get
            Dim attrib As TargetFrameworkAttribute = Me.assembly.GetCustomAttribute(Of TargetFrameworkAttribute)
            If (attrib IsNot Nothing) Then
                Dim nameValues As NameValueCollection = HttpUtility.ParseQueryString(attrib.FrameworkName.Replace(","c, "&"c))
                Return New Version(nameValues("Version").TrimStart("v"c))
            Else
                Return Nothing
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the default <see cref="CultureInfo"/> of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property DefaultCulture As CultureInfo
        Get
            Dim attrib As NeutralResourcesLanguageAttribute = Me.assembly.GetCustomAttribute(Of NeutralResourcesLanguageAttribute)
            If (attrib Is Nothing) Then
                Return Nothing
            Else
                Return New CultureInfo(attrib.CultureName)
            End If

        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the entry-point name (if any) of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property EntryPointAddress As Integer
        Get
            Return Me.PeHeader.PEHeader.AddressOfEntryPoint
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a value that determine whether the loaded assembly is installed in Global Assembly Cache (GAC).
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property IsInstalledInGAC As Boolean
        Get
            If (Me.isInstalledInGacB = TriState.UseDefault) Then
                If (Me.IsStrongNameSigned AndAlso Me.GetIsAssemblyInGAC()) Then
                    Me.isInstalledInGacB = TriState.True
                Else
                    Me.isInstalledInGacB = TriState.False
                End If
            End If
            Return CBool(Me.isInstalledInGacB)
        End Get
    End Property
    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' ( Backing Field )
    ''' <para></para>
    ''' A value that determine whether the loaded assembly is installed in Global Assembly Cache (GAC).
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private isInstalledInGacB As TriState = TriState.UseDefault

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a suitable assembly name for Global Assembly Cache (GAC).
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property GacFullName As String
        Get
            If Not (Me.IsInstalledInGAC) Then
                Return Nothing
            End If
            Return String.Format("{0}, ProcessorArchitecture={1}", Me.AssemblyName.FullName, Me.AssemblyName.ProcessorArchitecture.ToString().ToUpper())
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the assembly path from Global Assembly Cache (GAC) directory.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property GacPath As String
        Get
            If Not (Me.IsInstalledInGAC) Then
                Return Nothing
            End If
            Return Me.assembly.Location
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the portable executable (PE) header of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property PeHeader As PEHeaders
        Get
            If (Me.peHeaderB Is Nothing) Then
                Me.peHeaderB = Me.GetPEHeader()
            End If
            Return Me.peHeaderB
        End Get
    End Property
    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' ( Backing Field)
    ''' <para></para>
    ''' The portable executable (PE) header of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private peHeaderB As PEHeaders

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a value that determine whether the loaded assembly is strong-name signed.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property IsStrongNameSigned As Boolean
        Get
            Return Me.PeHeader.CorHeader.Flags.HasFlag(CorFlags.StrongNameSigned)
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the public key token (if any) used to sign the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property PublicKeyToken As String
        Get
            Return Me.GetPublicKeyToken()
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the portable executable (PE) file kind of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property PeFileKind As String ' PEFileKinds
        Get
            ' NOTE: the header values cause 'PEHeaders.IsConsoleApplication' to return true for dlls, 
            ' so we need to check 'PEHeaders.IsDll' first.
            If (Me.PeHeader.IsDll) Then
                Return "Dynamic-Link Library (dll)"
                ' Return PEFileKinds.Dll

            ElseIf (Me.PeHeader.IsConsoleApplication) Then
                Return "Console application (CLI)"
                ' Return PEFileKinds.ConsoleApplication
            Else
                Return "Window application (GUI)"
                ' Return PEFileKinds.WindowApplication
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the root namespace (if any) of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property RootNamespace As XNamespace
        Get
            Return Me.GetRootNamespace()
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the assembly GUID (if any) of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property Guid As Guid
        Get
            Dim attrib As GuidAttribute = Me.assembly.GetCustomAttribute(Of GuidAttribute)
            If (attrib Is Nothing) Then
                Return Nothing
            Else
                Return New Guid(attrib.Value)
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a value that determine whether the loaded assembly is COM visible.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property IsCOMVisible As Boolean
        Get
            Dim attrib As ComVisibleAttribute = Me.assembly.GetCustomAttribute(Of ComVisibleAttribute)
            If (attrib Is Nothing) Then
                Return False
            Else
                Return attrib.Value
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a value that determine whether the loaded assembly is CLS compliant.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property IsClsCompliant As Boolean
        Get
            Dim attrib As CLSCompliantAttribute = Me.assembly.GetCustomAttribute(Of CLSCompliantAttribute)
            If (attrib Is Nothing) Then
                Return False
            Else
                Return attrib.IsCompliant
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the security permission flags (if any) of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property SecurityPermission As SecurityPermissionAttribute
        Get
            Return Me.assembly.GetCustomAttribute(Of SecurityPermissionAttribute)
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets a value that determine whether the loaded assembly 
    ''' has disabled dots per inch (DPI) awareness for all user interface elements.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property IsDpiAwarenessDisabled As Boolean
        Get
            Dim attrib As DisableDpiAwarenessAttribute = Me.assembly.GetCustomAttribute(Of DisableDpiAwarenessAttribute)
            Return (attrib IsNot Nothing)
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the default <see cref="DllImportSearchPath"/> flags (if any) of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property DefaultDllImportSearchPaths As DllImportSearchPath
        Get
            Dim attrib As DefaultDllImportSearchPathsAttribute = Me.assembly.GetCustomAttribute(Of DefaultDllImportSearchPathsAttribute)
            If (attrib IsNot Nothing) Then
                Return attrib.Paths
            Else
                Return Nothing
            End If
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the compilation timestamp of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property TimeStamp As DateTimeOffset
        Get
            Dim secondsSince1970 As Integer = Me.PeHeader.CoffHeader.TimeDateStamp
            Dim dt As DateTimeOffset = (New DateTimeOffset(1970, 1, 1, 0, 0, 0, DateTimeOffset.Now.Offset) + DateTimeOffset.Now.Offset)
            dt = dt.AddSeconds(secondsSince1970)
            Return dt
        End Get
    End Property

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the MD5 hash of the loaded assembly file.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property MD5Hash As String

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the SHA-1 hash of the loaded assembly file.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property SHA1Hash As String

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the SHA-256 hash of the loaded assembly file.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public ReadOnly Property SHA256Hash As String

#End Region

#Region " Constructors "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Prevents a default instance of the <see cref="AssemblyInfo"/> class from being created.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub New()
    End Sub

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Initializes a new instance of the <see cref="AssemblyInfo"/> class.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="filepath">
    ''' The assembly file path.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Public Sub New(ByVal filepath As String)
        Me.filepath = filepath

        Dim rawAssembly As Byte() = File.ReadAllBytes(filepath)

        Try
            Me.assembly = Assembly.Load(rawAssembly) ' DON'T USE ONLY REFLECTION ( Assembly.ReflectionOnlyLoadFrom(filepath) )

        Catch ex As BadImageFormatException
            ' Add custom handling for BadImageFormatException.
            Throw

        Catch ex As FileLoadException
            ' Add custom handling for FileLoadException.
            Throw

        Catch ex As Exception
            ' Add custom handling for Exception.
            Throw

        End Try

        Me.MD5Hash = Me.ComputeHashOfData(Of MD5CryptoServiceProvider)(rawAssembly)
        Me.SHA1Hash = Me.ComputeHashOfData(Of SHA1CryptoServiceProvider)(rawAssembly)
        Me.SHA256Hash = Me.ComputeHashOfData(Of SHA256CryptoServiceProvider)(rawAssembly)
        rawAssembly = Nothing

        Me.AssemblyName = Me.assembly.GetName()
        Me.FileVersionInfo = FileVersionInfo.GetVersionInfo(filepath)
    End Sub

#End Region

#Region " Private Methods "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Determines whether the loaded assembly is installed in the Global Assembly Cache (GAC).
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <returns>
    ''' <see langword="True"/> if the assembly is installed in GAC; otherwise, <see langword="False"/>.
    ''' </returns>
    ''' ----------------------------------------------------------------------------------------------------
    <DebuggerStepThrough>
    Private Function GetIsAssemblyInGAC() As Boolean

        Dim fullName As String = String.Format("{0}, ProcessorArchitecture={1}", Me.AssemblyName.FullName, Me.AssemblyName.ProcessorArchitecture.ToString().ToUpper())

        Dim hResult As Integer
        Dim assCache As IAssemblyCache = Nothing
        Dim asmInfo As New NativeAssemblyInfo With {
            .CharBuffer = 1024,
            .AssemblyFlags = AssemblyInfoFlags.AssemblyinfoFlagInstalled
        }

        asmInfo.CurrentAssemblyPath = New String(ControlChars.NullChar, asmInfo.CharBuffer)

        hResult = NativeMethods.CreateAssemblyCache(assCache, 0)
        If (hResult = 0) Then
            hResult = assCache.QueryAssemblyInfo(QueryAssemblyInfoFlags.QueryAsmInfoValidate, fullName, asmInfo)
            Dim result As Boolean = (hResult = 0)
            Return result

        Else
            Marshal.ThrowExceptionForHR(hResult)
            Return False

        End If

    End Function

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the portable executable (PE) header of the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <returns>
    ''' The resulting <see cref="PEHeaders"/>.
    ''' </returns>
    ''' ----------------------------------------------------------------------------------------------------
    <DebuggerStepThrough>
    Private Function GetPEHeader() As PEHeaders

        Using fs As FileStream = File.OpenRead(Me.filepath),
            peReader As New PEReader(fs, PEStreamOptions.Default)

            Return peReader.PEHeaders
        End Using

    End Function

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the public key token (if any) used to sign the loaded assembly.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <returns>
    ''' The resulting <see cref="PEHeaders"/>.
    ''' </returns>
    ''' ----------------------------------------------------------------------------------------------------
    Private Function GetPublicKeyToken() As String
        Dim bytes As Byte() = Me.assembly.GetName().GetPublicKeyToken()
        If (bytes Is Nothing) OrElse (bytes.Length = 0) Then
            Return String.Empty
        End If

        Dim publicKeyToken As New StringBuilder()
        For i As Integer = 0 To (bytes.GetLength(0) - 1)
            publicKeyToken.Append(String.Format("{0:x2}", bytes(i)))
        Next i

        Return publicKeyToken.ToString()
    End Function

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Tries to detect the root namespace name of the loaded assembly.
    ''' <para></para>
    ''' Note that an assembly does not necessarily should have defined a root namespace, 
    ''' and also <see cref="AssemblyInfo.FindRootNamespace"/> could return a wrong root namespace name in some circumstances.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <returns>
    ''' The resulting <see cref="XNamespace"/>.
    ''' <para></para>
    ''' If the root namespace is not found, the return value is <see cref="XNamespace.None"/>.
    ''' </returns>
    ''' ----------------------------------------------------------------------------------------------------
    <DebuggerStepThrough>
    Private Function GetRootNamespace() As XNamespace

        Dim resName As String = Me.assembly.GetManifestResourceNames().FirstOrDefault()
        If String.IsNullOrEmpty(resName) Then
            Return XNamespace.None
        End If

        If (resName.IndexOf(".Resources", StringComparison.OrdinalIgnoreCase) <> -1) Then
            resName = resName.Substring(0, resName.IndexOf(".Resources", StringComparison.OrdinalIgnoreCase))
        End If

        If (resName.Equals("Resources", StringComparison.OrdinalIgnoreCase)) Then
            Return XNamespace.None
        End If

        Do While True
            Dim t As Global.System.Type = Me.assembly.GetType(resName, throwOnError:=False, ignoreCase:=False)
            If (t IsNot Nothing) Then
                If (resName.Contains("."c)) Then
                    resName = resName.Substring(0, resName.LastIndexOf("."c))
                Else
                    resName = String.Empty
                    Exit Do
                End If

            Else
                If (resName.Contains("."c)) Then
                    resName = resName.Substring(0, resName.LastIndexOf("."c))
                Else
                    Exit Do
                End If

            End If
        Loop

        Return resName

    End Function

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Computes a hash for the specified byte array using the given hash algorithm.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <typeparam name="T">
    ''' The <see cref="HashAlgorithm"/> provider.
    ''' </typeparam>
    ''' 
    ''' <param name="data">
    ''' The byte array.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <returns>
    ''' An Hexadecimal representation of the resulting hash value.
    ''' </returns>
    ''' ----------------------------------------------------------------------------------------------------
    <DebuggerStepThrough>
    Private Function ComputeHashOfData(Of T As HashAlgorithm)(ByVal data As Byte()) As String

        Using algorithm As HashAlgorithm = DirectCast(Activator.CreateInstance(GetType(T)), HashAlgorithm)

            Dim hash As Byte() = algorithm.ComputeHash(data)
            Dim sb As New StringBuilder(capacity:=hash.Length)

            For Each b As Byte In hash
                sb.Append(b.ToString("X2"))
            Next b

            Return sb.ToString()
        End Using

    End Function

#End Region

End Class

#End Region