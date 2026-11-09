#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

#End Region

#Region " PreviewForm "

''' <summary>
''' A window that hosts the <see cref="Global.PropertyPage"/> of the shell-extension,
''' with the same size as in the Windows Explorer property sheet.
''' <para></para>
''' Files can be opened from the toolbar, or by dragging and dropping them on the window.
''' </summary>
Friend NotInheritable Class PreviewForm : Inherits Form

#Region " Constants "

    ''' <summary>The client size of the property page in the Windows Explorer property sheet (at 96 DPI).</summary>
    Private Shared ReadOnly PropertySheetPageSize As New Size(339, 422)

    ''' <summary>The filter of the open file dialog.</summary>
    Private Const OpenFileDialogFilter As String = ".NET Assemblies (*.dll;*.exe)|*.dll;*.exe|All Files (*.*)|*.*"

#End Region

#Region " Private Fields "

    ''' <summary>The hosted property page.</summary>
    Private ReadOnly propertyPage As Global.PropertyPage

    ''' <summary>The tool strip at the top of the window.</summary>
    Private ReadOnly toolStrip As ToolStrip

    ''' <summary>The "Reload" button, enabled when a file is opened.</summary>
    Private ReadOnly reloadButton As ToolStripButton

    ''' <summary>The full path of the opened file, or <see langword="Nothing"/> if no file was opened.</summary>
    Private filePath As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="PreviewForm"/> class.
    ''' </summary>
    Friend Sub New()

        Me.SuspendLayout()

        Me.propertyPage = New Global.PropertyPage() With {.Dock = DockStyle.Fill}

        Dim openButton As New ToolStripButton("Open...", Nothing, AddressOf Me.OpenButton_Click) With {
            .ToolTipText = "Open a file (you can also drag and drop files on this window)."
        }
        Me.reloadButton = New ToolStripButton("Reload", Nothing, AddressOf Me.ReloadButton_Click) With {
            .ToolTipText = "Read the opened file again.",
            .Enabled = False
        }
        Me.toolStrip = New ToolStrip({openButton, Me.reloadButton}) With {
            .Dock = DockStyle.Top,
            .GripStyle = ToolStripGripStyle.Hidden
        }

        Me.Controls.Add(Me.propertyPage)
        Me.Controls.Add(Me.toolStrip)

        Me.AutoScaleDimensions = New SizeF(6.0F, 13.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(PreviewForm.PropertySheetPageSize.Width, PreviewForm.PropertySheetPageSize.Height + Me.toolStrip.Height)
        Me.MinimumSize = Me.Size
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.ShowIcon = False
        Me.AllowDrop = True
        Me.Text = "AssemblyInfo Preview"

        Me.ResumeLayout(performLayout:=True)

    End Sub

#End Region

#Region " Friend Methods "

    ''' <summary>
    ''' Shows the information of the specified file in the property page.
    ''' <para></para>
    ''' If the file cannot be opened, an error message is shown and the previous file (if any) remains shown.
    ''' </summary>
    ''' <param name="path">
    ''' The full path of the file.
    ''' </param>
    Friend Sub OpenFile(path As String)

        Try
            Me.propertyPage.LoadFile(path)

        Catch ex As Exception When (TypeOf ex Is IOException) OrElse
                                   (TypeOf ex Is UnauthorizedAccessException) OrElse
                                   (TypeOf ex Is ArgumentException)
            MessageBox.Show(Me, $"The file cannot be opened:{Environment.NewLine}{path}{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                            Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return

        End Try

        Me.filePath = path
        Me.reloadButton.Enabled = True
        Me.Text = $"AssemblyInfo Preview - {IO.Path.GetFileName(path)}"

    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>Shows the open file dialog, and opens the chosen file.</summary>
    Private Sub OpenButton_Click(sender As Object, e As EventArgs)

        Using dialog As New OpenFileDialog() With {
            .Filter = PreviewForm.OpenFileDialogFilter,
            .CheckFileExists = True,
            .Multiselect = False
        }
            If Not String.IsNullOrEmpty(Me.filePath) Then
                dialog.InitialDirectory = Path.GetDirectoryName(Me.filePath)
            End If

            If dialog.ShowDialog(Me) = DialogResult.OK Then
                Me.OpenFile(dialog.FileName)
            End If
        End Using

    End Sub

    ''' <summary>Reads the opened file again (e.g. after rebuilding it).</summary>
    Private Sub ReloadButton_Click(sender As Object, e As EventArgs)

        If Not String.IsNullOrEmpty(Me.filePath) Then
            Me.OpenFile(Me.filePath)
        End If

    End Sub

#End Region

#Region " Event Invocators "

    ''' <summary>
    ''' Raises the <see cref="Control.DragEnter"/> event.
    ''' <para></para>
    ''' Accepts a single dropped file.
    ''' </summary>
    ''' <param name="e">
    ''' The event data.
    ''' </param>
    Protected Overrides Sub OnDragEnter(e As DragEventArgs)

        Dim files As String() = PreviewForm.GetDroppedFiles(e)
        e.Effect = If((files IsNot Nothing) AndAlso (files.Length = 1) AndAlso File.Exists(files(0)), DragDropEffects.Copy, DragDropEffects.None)

        MyBase.OnDragEnter(e)

    End Sub

    ''' <summary>
    ''' Raises the <see cref="Control.DragDrop"/> event.
    ''' <para></para>
    ''' Opens the dropped file.
    ''' </summary>
    ''' <param name="e">
    ''' The event data.
    ''' </param>
    Protected Overrides Sub OnDragDrop(e As DragEventArgs)

        Dim files As String() = PreviewForm.GetDroppedFiles(e)
        If (files IsNot Nothing) AndAlso (files.Length = 1) Then
            Me.OpenFile(files(0))
        End If

        MyBase.OnDragDrop(e)

    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Gets the file paths of a drag and drop operation.
    ''' </summary>
    ''' <param name="e">
    ''' The drag and drop event data.
    ''' </param>
    ''' <returns>
    ''' The file paths, or <see langword="Nothing"/> if the dragged data are not files.
    ''' </returns>
    Private Shared Function GetDroppedFiles(e As DragEventArgs) As String()

        If (e.Data Is Nothing) OrElse Not e.Data.GetDataPresent(DataFormats.FileDrop) Then
            Return Nothing
        End If

        Return TryCast(e.Data.GetData(DataFormats.FileDrop), String())

    End Function

#End Region

End Class

#End Region
