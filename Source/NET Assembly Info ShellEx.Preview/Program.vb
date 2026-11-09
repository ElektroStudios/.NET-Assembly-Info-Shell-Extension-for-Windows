#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Windows.Forms

#End Region

#Region " Program "

''' <summary>
''' The entry point of the preview application.
''' </summary>
Friend NotInheritable Class Program

#Region " Constructors "

    ''' <summary>
    ''' Prevents a default instance of the <see cref="Program"/> class from being created.
    ''' </summary>
    Private Sub New()
    End Sub

#End Region

#Region " Entry Point "

    ''' <summary>
    ''' Shows the preview window.
    ''' <para></para>
    ''' The file to inspect can be specified as the first command-line argument
    ''' (Project Properties -> Debug -> Command line arguments);
    ''' otherwise the shell-extension assembly itself (AssemblyInfo.dll) is shown.
    ''' </summary>
    ''' <param name="args">
    ''' The command-line arguments.
    ''' </param>
    <STAThread>
    Friend Shared Sub Main(args As String())

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Dim filePath As String = If(args.Length <> 0, Path.GetFullPath(args(0)), GetType(Global.PropertyPage).Assembly.Location)

        Using form As New PreviewForm()
            form.OpenFile(filePath)
            Application.Run(form)
        End Using

    End Sub

#End Region

End Class

#End Region
