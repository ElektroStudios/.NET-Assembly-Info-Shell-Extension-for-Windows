
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Windows.Forms

#End Region

#Region " TextViewerForm "

''' <summary>
''' A read-only viewer for text documents (Win32 manifests, app.config, runtimeconfig.json, deps.json),
''' with copy and save options.
''' </summary>
Friend NotInheritable Class TextViewerForm : Inherits Form ' NOSONAR

#Region " Private Fields "

    ''' <summary>The text box that shows the document.</summary>
    Private ReadOnly textBoxContent As TextBox

    ''' <summary>The suggested file name for "Save As...".</summary>
    Private ReadOnly suggestedFileName As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="TextViewerForm"/> class.
    ''' </summary>
    ''' <param name="title">
    ''' The window title.
    ''' </param>
    '''
    ''' <param name="suggestedFileName">
    ''' The suggested file name for "Save As...".
    ''' </param>
    '''
    ''' <param name="content">
    ''' The text to show.
    ''' </param>
    Friend Sub New(title As String, suggestedFileName As String, content As String)

        If content Is Nothing Then
            Throw New ArgumentNullException(NameOf(content))
        End If
        Me.suggestedFileName = suggestedFileName

        Me.SuspendLayout()
        Me.Text = title
        Me.Icon = My.Resources.VisualStudio
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ShowInTaskbar = False
        Me.MinimizeBox = False
        Me.Size = New Size(760, 540)
        Me.MinimumSize = New Size(400, 250)

        ' Normalize the line endings, because the TextBox control only breaks lines on CRLF.
        Dim normalized As String = content.Replace(ControlChars.CrLf, ControlChars.Lf).Replace(ControlChars.Cr, ControlChars.Lf).Replace(ControlChars.Lf, ControlChars.CrLf)

        Me.textBoxContent = New TextBox() With {
            .Multiline = True,
            .ReadOnly = True,
            .WordWrap = False,
            .ScrollBars = ScrollBars.Both,
            .Dock = DockStyle.Fill,
            .Font = New Font("Consolas", 9.75F),
            .BackColor = SystemColors.Window,
            .Text = normalized
        }

        Dim buttonClose As New Button() With {.Text = "Close", .DialogResult = DialogResult.Cancel, .AutoSize = True}
        Dim buttonSave As New Button() With {.Text = "Save As...", .Image = My.Resources.Save, .TextImageRelation = TextImageRelation.ImageBeforeText, .AutoSize = True}
        Dim buttonCopy As New Button() With {.Text = "Copy", .Image = My.Resources.Copy, .TextImageRelation = TextImageRelation.ImageBeforeText, .AutoSize = True}
        AddHandler buttonSave.Click, AddressOf Me.ButtonSave_Click
        AddHandler buttonCopy.Click, AddressOf Me.ButtonCopy_Click

        Dim panelButtons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Bottom,
            .FlowDirection = FlowDirection.RightToLeft,
            .AutoSize = True,
            .Padding = New Padding(6)
        }
        panelButtons.Controls.AddRange({buttonClose, buttonSave, buttonCopy})

        Me.Controls.Add(Me.textBoxContent)
        Me.Controls.Add(panelButtons)
        Me.CancelButton = buttonClose
        Me.ResumeLayout(performLayout:=True)

    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>Copies the whole document to the clipboard.</summary>
    Private Sub ButtonCopy_Click(sender As Object, e As EventArgs)

        If Me.textBoxContent.TextLength = 0 Then
            Clipboard.Clear()
        Else
            Clipboard.SetText(Me.textBoxContent.Text)
        End If

    End Sub

    ''' <summary>Saves the document to a file (UTF-8).</summary>
    Private Sub ButtonSave_Click(sender As Object, e As EventArgs)

        Using sfd As New SaveFileDialog() With {
            .FileName = Me.suggestedFileName,
            .Filter = "All files (*.*)|*.*",
            .OverwritePrompt = True,
            .RestoreDirectory = True,
            .Title = "Save As"
        }
            If sfd.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            Try
                File.WriteAllText(sfd.FileName, Me.textBoxContent.Text, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True))

            Catch ex As Exception When (TypeOf ex Is IOException) OrElse (TypeOf ex Is UnauthorizedAccessException)
                MessageBox.Show(Me, ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)

            End Try
        End Using

    End Sub

#End Region

End Class

#End Region
