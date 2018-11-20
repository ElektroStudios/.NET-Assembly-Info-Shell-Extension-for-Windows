#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.ComponentModel
Imports System.Runtime.InteropServices

Imports ElektroKit.Interop.Unmanaged.Win32.Enums
Imports ElektroKit.Interop.Unmanaged.Win32.Structures

#End Region

#Region " IAssemblyCache "

Namespace ElektroKit.Interop.Unmanaged.Win32.Interfaces

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Represents the global assembly cache for use by the fusion technology.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/ms231448(v=vs.110).aspx"/>
    ''' </remarks>
    ''' ----------------------------------------------------------------------------------------------------
    <ComImport>
    <InterfaceType(ComInterfaceType.InterfaceIsIUnknown)>
    <Guid("E707DCDE-D1CD-11D2-BAB9-00C04F8ECEAE")>
    Public Interface IAssemblyCache

        ' *****************************************************************************
        '                            WARNING!, NEED TO KNOW...
        '
        '  THIS INTERFACE IS PARTIALLY DEFINED TO MEET THE PURPOSES OF THIS API
        ' *****************************************************************************

        <EditorBrowsable(EditorBrowsableState.Never)>
        <PreserveSig()>
        Function NotImplemented1() As Integer

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Gets the requested data about the specified assembly.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <param name="flags">
        ''' Flags defined in <c>Fusion.idl</c>. The following values are supported:
        ''' <para></para> QUERYASMINFO_FLAG_VALIDATE (0x00000001)
        ''' <para></para> QUERYASMINFO_FLAG_GETSIZE (0x00000002)
        ''' </param>
        ''' 
        ''' <param name="assemblyName">
        ''' The name of the assembly for which data will be retrieved.
        ''' </param>
        ''' 
        ''' <param name="refAssemblyInfo">
        ''' An <see cref="AssemblyInfo"/> structure that contains data about the assembly.
        ''' </param>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' An <see cref="IntPtr"/> that points to an HRESULT.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        <PreserveSig>
        Function QueryAssemblyInfo(ByVal flags As QueryAssemblyInfoFlags,
 <MarshalAs(UnmanagedType.LPWStr)> ByVal assemblyName As String,
                                   ByRef refAssemblyInfo As NativeAssemblyInfo
        ) As Integer

        <EditorBrowsable(EditorBrowsableState.Never)>
        <PreserveSig()>
        Function NotImplemented2() As Integer

        <EditorBrowsable(EditorBrowsableState.Never)>
        <PreserveSig()>
        Function NotImplemented3() As Integer

        <EditorBrowsable(EditorBrowsableState.Never)>
        <PreserveSig()>
        Function NotImplemented4() As Integer

    End Interface

End Namespace

#End Region
