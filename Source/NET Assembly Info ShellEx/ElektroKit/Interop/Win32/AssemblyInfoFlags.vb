#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports ElektroKit.Interop.Unmanaged.Win32.Interfaces

#End Region

#Region " AssemblyInfo Flags "

Namespace ElektroKit.Interop.Unmanaged.Win32.Enums

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Flags combination for <c>flags</c> parameter of 
    ''' <see cref="IAssemblyCache.QueryAssemblyInfo"/> function.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/ms230114(v=vs.110).aspx"/>
    ''' </remarks>
    ''' ----------------------------------------------------------------------------------------------------
    <Flags>
    Public Enum AssemblyInfoFlags As UInteger

        ''' <summary>
        ''' Indicates that the assembly is installed. 
        ''' </summary>
        AssemblyinfoFlagInstalled = &H1

        ''' <summary>
        ''' Indicates that the assembly is a payload resident.
        ''' </summary>
        AssemblyinfoFlagPayloadResident = &H2

    End Enum

End Namespace

#End Region
