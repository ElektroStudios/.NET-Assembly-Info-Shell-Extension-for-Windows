
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
Imports ElektroKit.Interop.Unmanaged.Win32
Imports Microsoft.Win32
Imports SharpShell.SharpPropertySheet
Imports Usercontrols

#End Region

#Region " PropertyPage "

''' <summary>
''' The .NET Assembly Info property page.
''' </summary>
Partial Public Class PropertyPage : Inherits SharpPropertyPage

#Region " Constants "

    ''' <summary>The minimum width, in pixels, of the list view columns after auto-sizing them.</summary>
    Private Const MinimumColumnWidth As Integer = 120

    ''' <summary>The <c>EM_SETCUEBANNER</c> message, that sets the placeholder text of an edit control.</summary>
    Private Const EmSetCueBanner As Integer = &H1501

    ''' <summary>The Win32 error code returned when the user cancels an UAC elevation prompt.</summary>
    Private Const ErrorCancelled As Integer = 1223

    ''' <summary>The registry key where Registry Editor keeps its last opened key.</summary>
    Private Const RegeditAppletKey As String = "Software\Microsoft\Windows\CurrentVersion\Applets\Regedit"

#End Region

#Region " Private Fields "

    ''' <summary>The source assembly file path.</summary>
    Private filePath As String

    ''' <summary>The assembly information, or <see langword="Nothing"/> if the file could not be read.</summary>
    Private info As AssemblyInfo

    ''' <summary>The property groups shown in the list views, in tab order.</summary>
    Private groups As ReadOnlyCollection(Of PropertyGroup)

    ''' <summary>The registry keys where the COM type library and classes of the assembly are registered.</summary>
    Private comRegistryKeys As ReadOnlyCollection(Of ComRegistryKey)

    ''' <summary>The list views, in tab order (Product, Build, Identity, Attributes, References, Runtime).</summary>
    Private ReadOnly listViews As ListView()

    ''' <summary>Keep track of the current shown ListView to perform copy operations from the context menu.</summary>
    Private currentListView As ListView

    ''' <summary>The last filter index chosen in the export dialog (1-based, <see cref="ReportFormat"/> order).</summary>
    Private Shared lastExportFilterIndex As Integer = 1

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

        ' Flat, solid background (same color as the page): the system renderer can leave it unpainted (black) in the property sheet.
        Me.ToolStripMain.Renderer = New FlatToolStripRenderer()

        Me.listViews = {Me.ListViewProduct, Me.ListViewBuild, Me.ListViewIdentity, Me.ListViewAttributes, Me.ListViewReferences, Me.ListViewRuntime}
        Me.currentListView = Me.TabControlListViews.TabPages(0).Controls.OfType(Of ListView).Single()

        For Each lv As ListView In Me.listViews
            AddHandler lv.SelectedIndexChanged, AddressOf Me.ListView_SelectedIndexChanged
        Next lv

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

#Region " Friend Methods "

    ''' <summary>
    ''' Loads and shows the information of the specified file, outside of a property sheet.
    ''' <para></para>
    ''' This is used by the AssemblyInfo.Preview project, to design and debug the property page in a regular window.
    ''' </summary>
    ''' <param name="path">
    ''' The full path of the file to inspect.
    ''' </param>
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="path"/> is <see langword="Nothing"/> or empty.
    ''' </exception>
    ''' <exception cref="FileNotFoundException">
    ''' Thrown when the file does not exist.
    ''' </exception>
    Friend Sub LoadFile(path As String)

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentNullException(NameOf(path))
        End If
        If Not File.Exists(path) Then
            Throw New FileNotFoundException("The file does not exist.", path)
        End If

        Me.filePath = path
        Me.LoadAssemblyInfo()
        Me.LoadListViewItems(Me.currentListView)

    End Sub

#End Region

#Region " Event Handlers (List Views and Filter) "

    ''' <summary>Loads the rows of the selected tab.</summary>
    Private Sub TabControlListViews_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControlListViews.SelectedIndexChanged

        Me.currentListView = DirectCast(sender, TabControlFix).SelectedTab.Controls.OfType(Of ListView).Single()
        Me.LoadListViewItems(Me.currentListView)

    End Sub

    ''' <summary>Updates the commands that depend on the selected rows.</summary>
    Private Sub ListView_SelectedIndexChanged(sender As Object, e As EventArgs)

        Me.UpdateSelectionCommands()

    End Sub

    ''' <summary>Sets the placeholder text of the filter box once its handle exists.</summary>
    Private Sub TextBoxFilter_HandleCreated(sender As Object, e As EventArgs) Handles TextBoxFilter.HandleCreated

        NativeMethods.SendMessage(Me.TextBoxFilter.Handle, PropertyPage.EmSetCueBanner, New IntPtr(1), "Filter rows by name or value...")

    End Sub

    ''' <summary>Filters the rows of the current tab.</summary>
    Private Sub TextBoxFilter_TextChanged(sender As Object, e As EventArgs) Handles TextBoxFilter.TextChanged

        Me.LoadListViewItems(Me.currentListView)

    End Sub

    ''' <summary>Clears the filter when ESC is pressed.</summary>
    Private Sub TextBoxFilter_KeyDown(sender As Object, e As KeyEventArgs) Handles TextBoxFilter.KeyDown

        If (e.KeyCode = Keys.Escape) AndAlso (Me.TextBoxFilter.TextLength <> 0) Then
            Me.TextBoxFilter.Clear()
            e.Handled = True
            e.SuppressKeyPress = True
        End If

    End Sub

#End Region

#Region " Event Handlers (Context Menu) "

    ''' <summary>Cancels the context menu when there is nothing selected to copy.</summary>
    Private Sub ContextMenuStripListView_Opening(sender As Object, e As CancelEventArgs) Handles ContextMenuStripListView.Opening

        e.Cancel = (Me.currentListView Is Nothing) OrElse (Me.currentListView.SelectedItems.Count = 0)

    End Sub

    ''' <summary>Copies "Name: Value" of the focused row.</summary>
    Private Sub CopyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyToolStripMenuItem.Click

        Dim row As PropertyRow = Me.GetSelectedRows().First()
        PropertyPage.SetClipboardText($"{row.Name}: {row.Value}")

    End Sub

    ''' <summary>Copies only the value of the focused row.</summary>
    Private Sub CopyValueToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyValueToolStripMenuItem.Click

        PropertyPage.SetClipboardText(Me.GetSelectedRows().First().Value)

    End Sub

    ''' <summary>Copies "Name: Value" of every selected row, one per line.</summary>
    Private Sub CopySelectedToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopySelectedToolStripMenuItem.Click

        Dim lines As IEnumerable(Of String) = Me.GetSelectedRows().Select(Function(row As PropertyRow) $"{row.Name}: {row.Value}")
        PropertyPage.SetClipboardText(String.Join(Environment.NewLine, lines))

    End Sub

    ''' <summary>Exports the selected rows.</summary>
    Private Sub ExportSelected_Click(sender As Object, e As EventArgs) Handles ExportSelectedToolStripMenuItem.Click, ExportSelectionToolStripMenuItem.Click

        Dim selection As New PropertyGroup(Me.GetCurrentGroup().Title, Me.GetSelectedRows())
        Me.ExportGroups({selection}, "selected rows")

    End Sub

#End Region

#Region " Event Handlers (Tool Strip) "

    ''' <summary>Exports all the tabs.</summary>
    Private Sub ExportAll_Click(sender As Object, e As EventArgs) Handles ExportAllToolStripMenuItem.Click

        Me.ExportGroups(Me.groups, String.Empty)

    End Sub

    ''' <summary>Exports the current tab.</summary>
    Private Sub ExportTab_Click(sender As Object, e As EventArgs) Handles ExportTabToolStripMenuItem.Click

        Dim group As PropertyGroup = Me.GetCurrentGroup()
        Me.ExportGroups({group}, group.Title)

    End Sub

    ''' <summary>Copies all the tabs as plain text (button click).</summary>
    Private Sub ToolStripSplitButtonCopyAll_ButtonClick(sender As Object, e As EventArgs) Handles ToolStripSplitButtonCopyAll.ButtonClick, CopyAllTextToolStripMenuItem.Click

        PropertyPage.SetClipboardText(AssemblyInfoReport.Build(Me.filePath, Me.groups, ReportFormat.Text))

    End Sub

    ''' <summary>Copies all the tabs as Markdown.</summary>
    Private Sub CopyAllMarkdownToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyAllMarkdownToolStripMenuItem.Click

        PropertyPage.SetClipboardText(AssemblyInfoReport.Build(Me.filePath, Me.groups, ReportFormat.Markdown))

    End Sub

    ''' <summary>Compares the assembly with another one, chosen by the user.</summary>
    Private Sub ToolStripButtonCompare_Click(sender As Object, e As EventArgs) Handles ToolStripButtonCompare.Click

        Dim otherPath As String
        Using ofd As New OpenFileDialog() With {
            .Filter = ".NET assemblies (*.dll;*.exe)|*.dll;*.exe|All files (*.*)|*.*",
            .InitialDirectory = Path.GetDirectoryName(Me.filePath),
            .RestoreDirectory = True,
            .Title = $"Compare {Path.GetFileName(Me.filePath)} with..."
        }
            If ofd.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If
            otherPath = ofd.FileName
        End Using

        Dim otherGroups As ReadOnlyCollection(Of PropertyGroup)
        Try
            otherGroups = AssemblyInfoPresenter.CreateGroups(New AssemblyInfo(otherPath), TimeZoneInfo.Local)

        Catch ex As Exception When PropertyPage.IsReadError(ex)
            MessageBox.Show(Me, $"{Path.GetFileName(otherPath)}: {ex.Message}", Me.Name, MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return

        End Try

        Using form As New CompareForm(Me.filePath, otherPath, AssemblyInfoComparer.Compare(Me.groups, otherGroups))
            form.ShowDialog(Me)
        End Using

    End Sub

    ''' <summary>Shows the Win32 manifest.</summary>
    Private Sub ToolStripButtonManifest_Click(sender As Object, e As EventArgs) Handles ToolStripButtonManifest.Click

        Dim manifest As Win32ManifestInfo = Me.info.Win32Manifest
        Dim title As String = $"Win32 Manifest (resource ID {manifest.ResourceId}) - {Path.GetFileName(Me.filePath)}"
        Using form As New TextViewerForm(title, $"{Path.GetFileName(Me.filePath)}.manifest", manifest.Xml)
            form.ShowDialog(Me)
        End Using

    End Sub

    ''' <summary>Shows a configuration file.</summary>
    Private Sub ConfigurationFileMenuItem_Click(sender As Object, e As EventArgs)

        Dim file As ConfigurationFile = DirectCast(DirectCast(sender, ToolStripItem).Tag, ConfigurationFile)
        Dim location As String = If(file.IsEmbedded, "embedded in the single-file bundle", "sidecar file")
        Using form As New TextViewerForm($"{file.FileName} ({location})", file.FileName, file.Content)
            form.ShowDialog(Me)
        End Using

    End Sub

    ''' <summary>Opens the first registered COM key in Registry Editor (button click).</summary>
    Private Sub ToolStripSplitButtonRegEdit_ButtonClick(sender As Object, e As EventArgs) Handles ToolStripSplitButtonRegEdit.ButtonClick

        Me.OpenInRegistryEditor(Me.comRegistryKeys(0).KeyPath)

    End Sub

    ''' <summary>Opens a registered COM key in Registry Editor (drop-down item).</summary>
    Private Sub RegistryKeyMenuItem_Click(sender As Object, e As EventArgs)

        Me.OpenInRegistryEditor(DirectCast(DirectCast(sender, ToolStripItem).Tag, ComRegistryKey).KeyPath)

    End Sub

    ''' <summary>Shows the assembly file inside the GAC folder.</summary>
    Private Sub ToolStripButtonGac_Click(sender As Object, e As EventArgs) Handles ToolStripButtonGac.Click

        PropertyPage.StartProcess("explorer.exe", $"/select,""{Me.info.GacPath}""")

    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Reads the assembly information, builds the property groups and updates the commands.
    ''' <para></para>
    ''' If the file cannot be read, the error is shown in the first tab instead of crashing the property sheet.
    ''' </summary>
    Private Sub LoadAssemblyInfo()

        Try
            Me.info = New AssemblyInfo(Me.filePath)
            Me.groups = AssemblyInfoPresenter.CreateGroups(Me.info, TimeZoneInfo.Local)
            Me.comRegistryKeys = ComRegistryLocator.GetRegisteredKeys(Me.info, AddressOf PropertyPage.ClassesRootKeyExists)

        Catch ex As Exception When PropertyPage.IsReadError(ex)
            Me.info = Nothing
            Me.groups = AssemblyInfoPresenter.CreateErrorGroups(Me.filePath, ex)
            Me.comRegistryKeys = New List(Of ComRegistryKey)().AsReadOnly()

        End Try

        If Me.groups.Count <> Me.listViews.Length Then
            Throw New InvalidOperationException($"Expected {Me.listViews.Length} property groups, but got {Me.groups.Count}.")
        End If

        For i As Integer = 0 To Me.listViews.Length - 1
            Me.listViews(i).Tag = Me.groups(i)
        Next i

        Me.UpdateCommands()

    End Sub

    ''' <summary>
    ''' Enables or disables the tool strip commands according to the available information,
    ''' and fills the drop-down lists of configuration files and COM registry keys.
    ''' </summary>
    Private Sub UpdateCommands()

        Dim hasInfo As Boolean = Me.info IsNot Nothing

        Me.ToolStripButtonCompare.Enabled = hasInfo
        Me.ToolStripButtonManifest.Enabled = hasInfo AndAlso (Me.info.Win32Manifest IsNot Nothing)
        Me.ToolStripButtonGac.Enabled = hasInfo AndAlso Me.info.IsInstalledInGAC AndAlso Not String.IsNullOrEmpty(Me.info.GacPath)

        ' Configuration files.
        Me.ToolStripDropDownButtonConfig.DropDownItems.Clear()
        If hasInfo Then
            For Each file As ConfigurationFile In Me.info.ConfigurationFiles
                Dim text As String = If(file.IsEmbedded, $"{file.FileName} (embedded)", file.FileName)
                Dim item As New ToolStripMenuItem(text, My.Resources.Config, AddressOf Me.ConfigurationFileMenuItem_Click) With {.Tag = file}
                Me.ToolStripDropDownButtonConfig.DropDownItems.Add(item)
            Next file
        End If
        Me.ToolStripDropDownButtonConfig.Enabled = Me.ToolStripDropDownButtonConfig.DropDownItems.Count <> 0

        ' COM registration.
        Me.ToolStripSplitButtonRegEdit.DropDownItems.Clear()
        For Each key As ComRegistryKey In Me.comRegistryKeys
            Dim item As New ToolStripMenuItem($"{key.Description}: {key.KeyPath}", My.Resources.RegEdit, AddressOf Me.RegistryKeyMenuItem_Click) With {.Tag = key}
            Me.ToolStripSplitButtonRegEdit.DropDownItems.Add(item)
        Next key
        Me.ToolStripSplitButtonRegEdit.Enabled = Me.comRegistryKeys.Count <> 0

        Me.UpdateSelectionCommands()

    End Sub

    ''' <summary>Enables the "selected rows" commands only when there is a selection.</summary>
    Private Sub UpdateSelectionCommands()

        Dim hasSelection As Boolean = (Me.currentListView IsNot Nothing) AndAlso (Me.currentListView.SelectedItems.Count <> 0)
        Me.ExportSelectionToolStripMenuItem.Enabled = hasSelection

    End Sub

    ''' <summary>
    ''' Loads the rows of the property group associated to the specified list view, applying the filter.
    ''' </summary>
    ''' <param name="lv">
    ''' The list view.
    ''' </param>
    Private Sub LoadListViewItems(lv As ListView)

        Dim group As PropertyGroup = TryCast(lv.Tag, PropertyGroup)
        If group Is Nothing Then
            Throw New InvalidOperationException($"The list view '{lv.Name}' has no property group assigned.")
        End If

        Dim filter As String = Me.TextBoxFilter.Text.Trim()

        lv.BeginUpdate()
        lv.Items.Clear()

        For Each row As PropertyRow In group.Rows
            If (filter.Length = 0) OrElse
               (row.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) <> -1) OrElse
               (row.Value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) <> -1) Then
                lv.Items.Add(New ListViewItem({row.Name, row.Value}) With {.Tag = row})
            End If
        Next row

        ' Fit both columns to their content, so long values (hashes, paths) can be read by scrolling horizontally.
        For Each column As ColumnHeader In lv.Columns
            column.AutoResize(If(lv.Items.Count = 0, ColumnHeaderAutoResizeStyle.HeaderSize, ColumnHeaderAutoResizeStyle.ColumnContent))
            If column.Width < PropertyPage.MinimumColumnWidth Then
                column.Width = PropertyPage.MinimumColumnWidth
            End If
        Next column

        lv.EndUpdate()
        Me.UpdateSelectionCommands()

    End Sub

    ''' <summary>Gets the property group of the current tab.</summary>
    Private Function GetCurrentGroup() As PropertyGroup

        Dim group As PropertyGroup = TryCast(Me.currentListView.Tag, PropertyGroup)
        If group Is Nothing Then
            Throw New InvalidOperationException($"The list view '{Me.currentListView.Name}' has no property group assigned.")
        End If
        Return group

    End Function

    ''' <summary>
    ''' Gets the rows selected in the current list view, in display order.
    ''' </summary>
    ''' <returns>
    ''' The selected rows.
    ''' </returns>
    ''' <exception cref="InvalidOperationException">
    ''' No item is selected.
    ''' </exception>
    Private Function GetSelectedRows() As List(Of PropertyRow)

        If Me.currentListView.SelectedItems.Count = 0 Then
            Throw New InvalidOperationException("No list view item is selected.")
        End If

        Return Me.currentListView.SelectedItems.Cast(Of ListViewItem)().
               OrderBy(Function(item As ListViewItem) item.Index).
               Select(Function(item As ListViewItem) DirectCast(item.Tag, PropertyRow)).ToList()

    End Function

    ''' <summary>
    ''' Exports the specified property groups to a file, in the format chosen in the save dialog.
    ''' </summary>
    ''' <param name="exportGroups">
    ''' The property groups to export.
    ''' </param>
    '''
    ''' <param name="scope">
    ''' A short description of what is exported (e.g. "Build", "selected rows"),
    ''' added to the suggested file name; or <see cref="String.Empty"/> for everything.
    ''' </param>
    Private Sub ExportGroups(exportGroups As IEnumerable(Of PropertyGroup), scope As String)

        Dim baseName As String = If(scope.Length = 0, Path.GetFileName(Me.filePath), $"{Path.GetFileName(Me.filePath)} ({scope})")

        Using sfd As New SaveFileDialog() With {
            .AddExtension = True,
            .CheckPathExists = True,
            .FileName = $"{baseName}.txt",
            .Filter = AssemblyInfoReport.SaveFileDialogFilter,
            .FilterIndex = PropertyPage.lastExportFilterIndex,
            .OverwritePrompt = True,
            .RestoreDirectory = True,
            .SupportMultiDottedExtensions = True,
            .Title = "Export Assembly Info",
            .ValidateNames = True
        }
            If sfd.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If

            PropertyPage.lastExportFilterIndex = sfd.FilterIndex
            Dim selectedFormat As ReportFormat = CType(sfd.FilterIndex - 1, ReportFormat)
            Dim format As ReportFormat = AssemblyInfoReport.GetFormatFromExtension(sfd.FileName, selectedFormat)
            Dim report As String = AssemblyInfoReport.Build(Me.filePath, exportGroups, format)

            ' The UTF-8 BOM helps Notepad and Excel with plain text and CSV; the structured formats are written without it.
            Dim writeBom As Boolean = (format = ReportFormat.Text) OrElse (format = ReportFormat.Csv)

            Try
                File.WriteAllText(sfd.FileName, report, New UTF8Encoding(encoderShouldEmitUTF8Identifier:=writeBom))

            Catch ex As Exception When (TypeOf ex Is IOException) OrElse
                                       (TypeOf ex Is UnauthorizedAccessException) OrElse
                                       (TypeOf ex Is Security.SecurityException)
                MessageBox.Show(Me, ex.Message, Me.Name, MessageBoxButtons.OK, MessageBoxIcon.Error)

            End Try
        End Using

    End Sub

    ''' <summary>
    ''' Opens Registry Editor at the specified key, through its "LastKey" setting.
    ''' </summary>
    ''' <param name="keyPath">
    ''' The full key path (e.g. "HKEY_CLASSES_ROOT\CLSID\{...}").
    ''' </param>
    Private Sub OpenInRegistryEditor(keyPath As String)

        Using appletKey As RegistryKey = Registry.CurrentUser.CreateSubKey(PropertyPage.RegeditAppletKey)
            Dim currentLastKey As String = TryCast(appletKey.GetValue("LastKey"), String)
            appletKey.SetValue("LastKey", ComRegistryLocator.BuildRegeditLastKey(currentLastKey, keyPath), RegistryValueKind.String)
        End Using

        ' "/m" opens a new instance: an already running Registry Editor would ignore the new "LastKey" value.
        PropertyPage.StartProcess("regedit.exe", "/m")

    End Sub

    ''' <summary>
    ''' Starts a process through the shell. Canceling an elevation (UAC) prompt is not an error.
    ''' </summary>
    ''' <param name="fileName">The program to start.</param>
    ''' <param name="arguments">The command-line arguments.</param>
    Private Shared Sub StartProcess(fileName As String, arguments As String)

        Try
            Using Process.Start(New ProcessStartInfo(fileName, arguments) With {.UseShellExecute = True})
            End Using

        Catch ex As Win32Exception When ex.NativeErrorCode = PropertyPage.ErrorCancelled
            ' The user canceled the UAC prompt: nothing to do.

        End Try

    End Sub

    ''' <summary>Determines whether a subkey of HKEY_CLASSES_ROOT exists.</summary>
    Private Shared Function ClassesRootKeyExists(subKey As String) As Boolean

        Using key As RegistryKey = Registry.ClassesRoot.OpenSubKey(subKey, writable:=False)
            Return key IsNot Nothing
        End Using

    End Function

    ''' <summary>Determines whether an exception is an expected error while reading an assembly file.</summary>
    Private Shared Function IsReadError(ex As Exception) As Boolean

        Return (TypeOf ex Is BadImageFormatException) OrElse
               (TypeOf ex Is IOException) OrElse
               (TypeOf ex Is UnauthorizedAccessException) OrElse
               (TypeOf ex Is NotSupportedException)

    End Function

    ''' <summary>Sets the clipboard text (an empty text clears the clipboard, as <see cref="Clipboard.SetText(String)"/> rejects it).</summary>
    Private Shared Sub SetClipboardText(text As String)

        If String.IsNullOrEmpty(text) Then
            Clipboard.Clear()
        Else
            Clipboard.SetText(text)
        End If

    End Sub

#End Region

End Class

#End Region
