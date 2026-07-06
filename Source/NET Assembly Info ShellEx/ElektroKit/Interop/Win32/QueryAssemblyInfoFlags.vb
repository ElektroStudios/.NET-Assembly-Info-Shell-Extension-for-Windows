#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " QueryAssemblyInfo Flags "

Namespace ElektroKit.Interop.Unmanaged.Win32.Enums

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Flags combination for <c>flags</c> parameter of 
    ''' <see cref="Interfaces.IAssemblyCache.QueryAssemblyInfo"/> function.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/ms230114(v=vs.110).aspx"/>
    ''' </remarks>
    ''' ----------------------------------------------------------------------------------------------------
    <Flags>
    Public Enum QueryAssemblyInfoFlags As Integer

        ''' <summary>
        ''' This value is no documented.
        ''' </summary>
        QueryAsmInfoValidate = &H1

        ''' <summary>
        ''' This value is no documented.
        ''' </summary>
        QueryAsmInfoGetsize = &H2

    End Enum

End Namespace

#End Region
