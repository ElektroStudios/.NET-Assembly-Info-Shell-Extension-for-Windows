#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Windows.Forms

#End Region

#Region " TabControlFix "

Namespace Usercontrols

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' A custom tabcontrol that fixes a Explorer.exe crash that occurs when using SharpShell v2.7.0 (and previous versions).
    ''' <para></para>
    ''' See the issue for more details: https://github.com/dwmkerr/sharpshell/issues/233
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public NotInheritable Class TabControlFix : Inherits TabControl

#Region " Constructors "

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Initializes a new instance of the <see cref="TabControlFix"/> class.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        Public Sub New()
            ' This is the fix.
            Me.SetStyle(ControlStyles.ContainerControl, value:=True)
        End Sub

#End Region

    End Class

End Namespace

#End Region
