
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

#Region " FixtureAttribute "

Namespace Fixtures

    ''' <summary>
    ''' Represents an assembly-level custom attribute emitted into a fixture assembly.
    ''' </summary>
    Friend NotInheritable Class FixtureAttribute

#Region " Properties "

        ''' <summary>
        ''' Gets the full name of the attribute type.
        ''' </summary>
        Friend ReadOnly Property TypeFullName As String

        ''' <summary>
        ''' Gets the fixed (constructor) arguments.
        ''' </summary>
        Friend ReadOnly Property FixedArguments As ReadOnlyCollection(Of FixtureArgument)

        ''' <summary>
        ''' Gets the named (property) arguments.
        ''' </summary>
        Friend ReadOnly Property NamedArguments As ReadOnlyCollection(Of KeyValuePair(Of String, FixtureArgument))

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FixtureAttribute"/> class.
        ''' </summary>
        ''' <param name="typeFullName">
        ''' The full name of the attribute type.
        ''' </param>
        '''
        ''' <param name="fixedArguments">
        ''' The fixed (constructor) arguments.
        ''' </param>
        Friend Sub New(typeFullName As String, ParamArray fixedArguments As FixtureArgument())
            Me.New(typeFullName, fixedArguments, Array.Empty(Of KeyValuePair(Of String, FixtureArgument))())
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FixtureAttribute"/> class.
        ''' </summary>
        ''' <param name="typeFullName">
        ''' The full name of the attribute type.
        ''' </param>
        '''
        ''' <param name="fixedArguments">
        ''' The fixed (constructor) arguments.
        ''' </param>
        '''
        ''' <param name="namedArguments">
        ''' The named (property) arguments.
        ''' </param>
        Friend Sub New(typeFullName As String, fixedArguments As IEnumerable(Of FixtureArgument), namedArguments As IEnumerable(Of KeyValuePair(Of String, FixtureArgument)))
            If String.IsNullOrEmpty(typeFullName) Then
                Throw New ArgumentNullException(NameOf(typeFullName))
            End If
            Me.TypeFullName = typeFullName
            Me.FixedArguments = fixedArguments.ToList().AsReadOnly()
            Me.NamedArguments = namedArguments.ToList().AsReadOnly()
        End Sub

#End Region

#Region " Factory Methods (Well-Known Attributes) "

        ''' <summary>Creates a <c>GuidAttribute</c>.</summary>
        Friend Shared Function Guid(value As String) As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.InteropServices.GuidAttribute", FixtureArgument.FromString(value))
        End Function

        ''' <summary>Creates a <c>ComVisibleAttribute</c>.</summary>
        Friend Shared Function ComVisible(value As Boolean) As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.InteropServices.ComVisibleAttribute", FixtureArgument.FromBoolean(value))
        End Function

        ''' <summary>Creates a <c>CLSCompliantAttribute</c>.</summary>
        Friend Shared Function ClsCompliant(value As Boolean) As FixtureAttribute
            Return New FixtureAttribute("System.CLSCompliantAttribute", FixtureArgument.FromBoolean(value))
        End Function

        ''' <summary>Creates a <c>NeutralResourcesLanguageAttribute</c>.</summary>
        Friend Shared Function NeutralResourcesLanguage(cultureName As String) As FixtureAttribute
            Return New FixtureAttribute("System.Resources.NeutralResourcesLanguageAttribute", FixtureArgument.FromString(cultureName))
        End Function

        ''' <summary>Creates a <c>TargetFrameworkAttribute</c>, optionally with a <c>FrameworkDisplayName</c>.</summary>
        Friend Shared Function TargetFramework(frameworkName As String, Optional displayName As String = Nothing) As FixtureAttribute
            Dim named As New List(Of KeyValuePair(Of String, FixtureArgument))
            If displayName IsNot Nothing Then
                named.Add(New KeyValuePair(Of String, FixtureArgument)("FrameworkDisplayName", FixtureArgument.FromString(displayName)))
            End If
            Return New FixtureAttribute("System.Runtime.Versioning.TargetFrameworkAttribute", {FixtureArgument.FromString(frameworkName)}, named)
        End Function

        ''' <summary>Creates a <c>TargetPlatformAttribute</c>.</summary>
        Friend Shared Function TargetPlatform(platformName As String) As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.Versioning.TargetPlatformAttribute", FixtureArgument.FromString(platformName))
        End Function

        ''' <summary>Creates a <c>SupportedOSPlatformAttribute</c>.</summary>
        Friend Shared Function SupportedOSPlatform(platformName As String) As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.Versioning.SupportedOSPlatformAttribute", FixtureArgument.FromString(platformName))
        End Function

        ''' <summary>Creates a <c>DefaultDllImportSearchPathsAttribute</c>.</summary>
        Friend Shared Function DefaultDllImportSearchPaths(paths As Integer) As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.InteropServices.DefaultDllImportSearchPathsAttribute",
                                        FixtureArgument.FromEnum("System.Runtime.InteropServices.DllImportSearchPath", paths))
        End Function

        ''' <summary>Creates a <c>DisableDpiAwarenessAttribute</c>.</summary>
        Friend Shared Function DisableDpiAwareness() As FixtureAttribute
            Return New FixtureAttribute("System.Windows.Media.DisableDpiAwarenessAttribute")
        End Function

        ''' <summary>Creates a <c>ReferenceAssemblyAttribute</c>.</summary>
        Friend Shared Function ReferenceAssembly() As FixtureAttribute
            Return New FixtureAttribute("System.Runtime.CompilerServices.ReferenceAssemblyAttribute")
        End Function

        ''' <summary>Creates an <c>AssemblyConfigurationAttribute</c>.</summary>
        Friend Shared Function AssemblyConfiguration(configuration As String) As FixtureAttribute
            Return New FixtureAttribute("System.Reflection.AssemblyConfigurationAttribute", FixtureArgument.FromString(configuration))
        End Function

        ''' <summary>Creates a <c>DebuggableAttribute(DebuggingModes)</c>.</summary>
        Friend Shared Function Debuggable(modes As Integer) As FixtureAttribute
            Return New FixtureAttribute("System.Diagnostics.DebuggableAttribute",
                                        FixtureArgument.FromEnum("System.Diagnostics.DebuggableAttribute+DebuggingModes", modes))
        End Function

        ''' <summary>Creates a <c>DebuggableAttribute(Boolean, Boolean)</c>.</summary>
        Friend Shared Function Debuggable(isJitTrackingEnabled As Boolean, isJitOptimizerDisabled As Boolean) As FixtureAttribute
            Return New FixtureAttribute("System.Diagnostics.DebuggableAttribute",
                                        FixtureArgument.FromBoolean(isJitTrackingEnabled),
                                        FixtureArgument.FromBoolean(isJitOptimizerDisabled))
        End Function

#End Region

    End Class

End Namespace

#End Region
