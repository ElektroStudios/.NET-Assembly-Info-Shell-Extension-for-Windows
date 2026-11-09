
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Text
Imports System.Windows.Forms

Imports AssemblyInspection
Imports SharpShell.SharpPropertySheet
Imports Usercontrols

#End Region

#Region " PropertyPage "

''' <summary>
''' The .NET Assembly Info property page.
''' </summary>
Partial Public Class PropertyPage : Inherits SharpPropertyPage

#Region " Constants "

    ''' <summary>
    ''' The minimum width, in pixels, of the list view columns after auto-sizing them.
    ''' </summary>
    Private Const MinimumColumnWidth As Integer = 120

#End Region

#Region " Private Fields "

    ''' <summary>
    ''' The source assembly file path.
    ''' </summary>
    Private filePath As String

    ''' <summary>
    ''' The property groups shown in the list views, in tab order.
    ''' </summary>
    Private groups As ReadOnlyCollection(Of PropertyGroup)

    ''' <summary>
    ''' The list views, in tab order (Product, Build, Identity, References).
    ''' </summary>
    Private ReadOnly listViews As ListView()

    ''' <summary>
    ''' Keep track of the current shown ListView to perform copy operations from the context menu.
    ''' </summary>
    Private currentListView As ListView

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="PropertyPage"/> class.
    ''' </summary>
    Public Sub New()
        MyClass.InitializeComponent()

        Me.Name = My.Application.Info.AssemblyName
        Me.PageTitle = Me.Name
        Me.PageIcon = My.Resources.VisualStudio

        Me.listViews = {Me.ListViewProduct, Me.ListViewBuild, Me.ListViewIdentity, Me.ListViewReferences}
        Me.currentListView = Me.TabControlListViews.TabPages(0).Controls.OfType(Of ListView).Single()

#If DEBUG Then
        Me.filePath = "C:\File.dll"
        If Not File.Exists(Me.filePath) Then
            MessageBox.Show(Nothing, $"[DEBUG] Test dll file not found: {Me.filePath}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Environment.Exit(1)
        End If

        Me.LoadAssemblyInfo()
        Me.LoadListViewItems(Me.currentListView)
#End If

    End Sub

#End Region

#Region " Event Invocators "

    ''' <summary>
    ''' Called when the property page is initialised.
    ''' </summary>
    ''' <param name="e">
    ''' The parent property sheet.
    ''' </param>
    Protected Overrides Sub OnPropertyPageInitialised(e As SharpPropertySheet)
        Me.filePath = e.SelectedItemPaths.Single()

        Me.LoadAssemblyInfo()
        Me.LoadListViewItems(Me.currentListView)

        MyBase.OnPropertyPageInitialised(e)
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the <see cref="TabControl.SelectedIndexChanged"/> event of the <see cref="PropertyPage.TabControlListViews"/> control.
    ''' </summary>
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    Private Sub TabControlListViews_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControlListViews.SelectedIndexChanged

        Me.currentListView = DirectCast(sender, TabControlFix).SelectedTab.Controls.OfType(Of ListView).Single()
        Me.LoadListViewItems(Me.currentListView)

    End Sub

    ''' <summary>
    ''' Handles the <see cref="ContextMenuStrip.Opening"/> event of the <see cref="PropertyPage.ContextMenuStripListView"/> control.
    ''' </summary>
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The <see cref="CancelEventArgs"/> instance containing the event data.
    ''' </param>
    Private Sub ContextMenuStripListView_Opening(sender As Object, e As CancelEventArgs) Handles ContextMenuStripListView.Opening

        ' Nothing to copy when the user right-clicks on an empty area of the list view.
        e.Cancel = (Me.currentListView Is Nothing) OrElse (Me.currentListView.SelectedItems.Count = 0)

    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripMenuItem.Click"/> event of the <see cref="PropertyPage.CopyToolStripMenuItem"/> control.
    ''' </summary>
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    Private Sub CopyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyToolStripMenuItem.Click

        Dim item As ListViewItem = Me.GetSelectedItem()
        Clipboard.SetText($"{item.SubItems(0).Text}: {item.SubItems(1).Text}")

    End Sub

    ''' <summary>
    ''' Handles the <see cref="ToolStripMenuItem.Click"/> event of the <see cref="PropertyPage.CopyValueToolStripMenuItem"/> control.
    ''' </summary>
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    Private Sub CopyValueToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyValueToolStripMenuItem.Click

        Dim item As ListViewItem = Me.GetSelectedItem()
        Dim value As String = item.SubItems(1).Text

        ' Clipboard.SetText throws ArgumentNullException for empty strings.
        If value.Length = 0 Then
            Clipboard.Clear()
        Else
            Clipboard.SetText(value)
        End If

    End Sub

    ''' <summary>
    ''' Handles the <see cref="Button.Click"/> event of the <see cref="PropertyPage.ButtonSave"/> control.
    ''' </summary>
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    '''
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    Private Sub ButtonSave_Click(sender As Object, e As EventArgs) Handles ButtonSave.Click

        Using sfd As New SaveFileDialog() With {
            .AddExtension = True,
            .CheckPathExists = True,
            .CreatePrompt = False,
            .DefaultExt = "txt",
            .FileName = $"{Path.GetFileName(Me.filePath)}.txt",
            .Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt",
            .FilterIndex = 2,
            .OverwritePrompt = True,
            .RestoreDirectory = True,
            .ShowHelp = False,
            .SupportMultiDottedExtensions = True,
            .Title = "Save Assembly Info",
            .ValidateNames = True
        }

            If (sfd.ShowDialog(Me) = DialogResult.OK) Then
                Me.SaveAsTextFile(sfd.FileName)
            End If
        End Using

    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Reads the assembly information and builds the property groups shown in the list views.
    ''' <para></para>
    ''' If the file cannot be read, the error is shown in the first tab instead of crashing the property sheet.
    ''' </summary>
    Private Sub LoadAssemblyInfo()

        Try
            Dim info As New AssemblyInfo(Me.filePath)
            Me.groups = AssemblyInfoPresenter.CreateGroups(info, TimeZoneInfo.Local)

        Catch ex As Exception When (TypeOf ex Is BadImageFormatException) OrElse
                                   (TypeOf ex Is IOException) OrElse
                                   (TypeOf ex Is UnauthorizedAccessException) OrElse
                                   (TypeOf ex Is NotSupportedException)
            Me.groups = AssemblyInfoPresenter.CreateErrorGroups(Me.filePath, ex)

        End Try

        If Me.groups.Count <> Me.listViews.Length Then
            Throw New InvalidOperationException($"Expected {Me.listViews.Length} property groups, but got {Me.groups.Count}.")
        End If

        For i As Integer = 0 To Me.listViews.Length - 1
            Me.listViews(i).Tag = Me.groups(i)
        Next i

    End Sub

    ''' <summary>
    ''' Loads the rows of the property group associated to the specified list view.
    ''' </summary>
    ''' <param name="lv">
    ''' The list view.
    ''' </param>
    Private Sub LoadListViewItems(lv As ListView)

        Dim group As PropertyGroup = TryCast(lv.Tag, PropertyGroup)
        If group Is Nothing Then
            Throw New InvalidOperationException($"The list view '{lv.Name}' has no property group assigned.")
        End If

        lv.BeginUpdate()
        lv.Items.Clear()

        For Each row As PropertyRow In group.Rows
            lv.Items.Add(New ListViewItem({row.Name, row.Value}))
        Next row

        ' Fit both columns to their content, so long values (hashes, paths) can be read by scrolling horizontally.
        For Each column As ColumnHeader In lv.Columns
            column.AutoResize(ColumnHeaderAutoResizeStyle.ColumnContent)
            If column.Width < PropertyPage.MinimumColumnWidth Then
                column.Width = PropertyPage.MinimumColumnWidth
            End If
        Next column

        lv.EndUpdate()

    End Sub

    ''' <summary>
    ''' Gets the selected item of the current list view.
    ''' </summary>
    ''' <returns>
    ''' The selected <see cref="ListViewItem"/>.
    ''' </returns>
    ''' <exception cref="InvalidOperationException">
    ''' No item is selected.
    ''' </exception>
    Private Function GetSelectedItem() As ListViewItem

        If Me.currentListView.SelectedItems.Count = 0 Then
            Throw New InvalidOperationException("No list view item is selected.")
        End If
        Return Me.currentListView.SelectedItems(0)

    End Function

    ''' <summary>
    ''' Save the assembly info as a plain text file (UTF-8).
    ''' </summary>
    ''' <param name="filePath">
    ''' The destination txt file path.
    ''' </param>
    Private Sub SaveAsTextFile(filePath As String)

        Dim report As String = AssemblyInfoReport.Build(Me.filePath, Me.groups)

        Try
            File.WriteAllText(filePath, report, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True))

        Catch ex As Exception When (TypeOf ex Is IOException) OrElse
                                   (TypeOf ex Is UnauthorizedAccessException) OrElse
                                   (TypeOf ex Is Security.SecurityException)
            MessageBox.Show(Me, ex.Message, Me.Name, MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try

    End Sub

#End Region

End Class

#End Region
