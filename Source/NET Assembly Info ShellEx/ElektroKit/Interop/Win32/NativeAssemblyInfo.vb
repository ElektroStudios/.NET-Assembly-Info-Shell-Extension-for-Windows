#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

Imports ElektroKit.Interop.Unmanaged.Win32.Enums
Imports ElektroKit.Interop.Unmanaged.Win32.Interfaces

#End Region

#Region " Assembly Info "

Namespace ElektroKit.Interop.Unmanaged.Win32.Structures

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Contains information about an assembly in the side-by-side assembly store. 
    ''' The information is used by the <see cref="IAssemblyCache.QueryAssemblyInfo"/> method.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/aa374213%28v=vs.85%29.aspx"/>
    ''' </remarks>
    ''' ----------------------------------------------------------------------------------------------------
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure NativeAssemblyInfo

        ''' <summary>
        ''' The size of the structure in bytes.
        ''' </summary>
        Public Size As Integer

        ''' <summary>
        ''' The size of the structure in bytes.
        ''' </summary>
        Public AssemblyFlags As AssemblyInfoFlags

        ''' <summary>
        ''' The size of the files that comprise the assembly in kilobytes (KB).
        ''' </summary>
        Public AssemblySizeInKB As Long

        ''' <summary>
        ''' A pointer to a null-terminated string that contains the path to the manifest file.
        ''' </summary>
        <MarshalAs(UnmanagedType.LPWStr)>
        Public CurrentAssemblyPath As String

        ''' <summary>
        ''' The number of characters, including the null terminator, 
        ''' in the string specified by <see cref="AssemblyInfo.currentAssemblyPath"/>.
        ''' </summary>
        Public CharBuffer As Integer

    End Structure

End Namespace

#End Region