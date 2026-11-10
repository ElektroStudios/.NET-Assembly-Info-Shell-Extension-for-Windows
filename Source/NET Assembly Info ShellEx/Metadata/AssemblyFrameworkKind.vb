
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " AssemblyFrameworkKind "

''' <summary>
''' Specifies the .NET platform family that an assembly was built for.
''' </summary>
Public Enum AssemblyFrameworkKind As Integer ' NOSONAR

    ''' <summary>
    ''' The .NET platform family could not be determined.
    ''' </summary>
    Unknown = 0

    ''' <summary>
    ''' .NET Framework (1.0 to 4.8.1).
    ''' </summary>
    NetFramework = 1

    ''' <summary>
    ''' .NET Core 1.0 to 3.1, and .NET 5 or newer.
    ''' </summary>
    NetCore = 2

    ''' <summary>
    ''' .NET Standard (or legacy portable class libraries).
    ''' </summary>
    NetStandard = 3

End Enum

#End Region
