
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Globalization
Imports System.IO
Imports System.Text
Imports System.Windows.Forms

Imports Usercontrols
Imports SharpShell.SharpPropertySheet

#End Region

#Region " PropertyPage "

''' ----------------------------------------------------------------------------------------------------
''' <summary>
''' The .NET Assembly Info property page.
''' </summary>
''' ----------------------------------------------------------------------------------------------------
Partial Public Class PropertyPage : Inherits SharpPropertyPage

#Region " Private Fields "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' The source assembly file path.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private filePath As String

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' The source AssemblyInfo.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private asmInfo As AssemblyInfo

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Keep track of the current shown ListView to perform copy operations from the context menu.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private currentListView As ListView

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' A dictionary that holds all the list-view items to be added into the <see cref="PropertyPage.ListViewProduct"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private ReadOnly lvItemsProductDict As New Dictionary(Of String, ListViewItem)(9, StringComparer.OrdinalIgnoreCase) From {
            {"File Name", New ListViewItem("File Name")},
            {"Original Name", New ListViewItem("Original Name")},
            {"Product Name", New ListViewItem("Product Name")},
            {"Product Description", New ListViewItem("Product Description")},
            {"Company Name", New ListViewItem("Company Name")},
            {"Copyright", New ListViewItem("Copyright")},
            {"File Version", New ListViewItem("File Version")},
            {"Product Version", New ListViewItem("Product Version")},
            {"Language", New ListViewItem("Language")}
        }

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' A dictionary that holds all the list-view items to be added into the <see cref="PropertyPage.ListViewBuild"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private ReadOnly lvItemsBuildDict As New Dictionary(Of String, ListViewItem)(18, StringComparer.OrdinalIgnoreCase) From {
            {"PE File Kind", New ListViewItem("PE File Kind")},
            {"Platform", New ListViewItem("Platform")},
            {"Build Configuration", New ListViewItem("Build Configuration")},
            {"CLR Version", New ListViewItem("CLR Version")},
            {"Target .NET Framework", New ListViewItem("Target .NET Framework")},
            {"Entry-Point Address", New ListViewItem("Entry-Point Address")},
            {"Root Namespace", New ListViewItem("Root Namespace")},
            {"Is Strong-Name Signed", New ListViewItem("Is Strong-Name Signed")},
            {"Public Key Token", New ListViewItem("Public Key Token")},
            {"Security Permission", New ListViewItem("Security Permission")},
            {"Dll Import Search Paths", New ListViewItem("Dll Import Search Paths")},
            {"Is CLS Compliant", New ListViewItem("Is CLS Compliant")},
            {"Is COM Visible", New ListViewItem("Is COM Visible")},
            {"Is DPI Awareness Disabled", New ListViewItem("Is DPI Awareness Disabled")},
            {"Is Installed in GAC", New ListViewItem("Is Installed in GAC")},
            {"GAC Name", New ListViewItem("GAC Name")},
            {"GAC Path", New ListViewItem("GAC Path")}
        }

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' A dictionary that holds all the list-view items to be added into the <see cref="PropertyPage.ListViewIdentity"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private ReadOnly lvItemsIdentityDict As New Dictionary(Of String, ListViewItem)(4, StringComparer.OrdinalIgnoreCase) From {
            {"Assembly GUID", New ListViewItem("Assembly GUID")},
            {"Compilation TimeStamp", New ListViewItem("Compilation TimeStamp")},
            {"MD5 Hash", New ListViewItem("MD5 Hash")},
            {"SHA-1 Hash", New ListViewItem("SHA-1 Hash")},
            {"SHA-256 Hash", New ListViewItem("SHA-256 Hash")}
        }

#End Region

#Region " Constructors "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Initializes a new instance of the <see cref="PropertyPage"/> class.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Public Sub New()
        MyClass.InitializeComponent()

        Me.Name = My.Application.Info.AssemblyName
        Me.PageTitle = Me.Name
        Me.PageIcon = My.Resources.VisualStudio

        Me.currentListView = Me.TabControlListViews.TabPages(0).Controls.OfType(Of ListView).Single()

        Me.ListViewProduct.Text = "Product"
        Me.ListViewProduct.Tag = Me.lvItemsProductDict

        Me.ListViewBuild.Text = "Build"
        Me.ListViewBuild.Tag = Me.lvItemsBuildDict

        Me.ListViewIdentity.Text = "Identity"
        Me.ListViewIdentity.Tag = Me.lvItemsIdentityDict

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

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Called when the property page is initialised.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="e">
    ''' The parent property sheet.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Protected Overrides Sub OnPropertyPageInitialised(e As SharpPropertySheet)
        Me.filePath = e.SelectedItemPaths.Single()

        Me.LoadAssemblyInfo()
        Me.LoadListViewItems(Me.currentListView)

        MyBase.OnPropertyPageInitialised(e)
    End Sub

#End Region

#Region " Event Handlers "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Handles the <see cref="TabControl.SelectedIndexChanged"/> event of the <see cref="PropertyPage.TabControlListViews"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub TabControlListViews_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControlListViews.SelectedIndexChanged

        Me.currentListView = DirectCast(sender, TabControlFix).SelectedTab.Controls.OfType(Of ListView).Single()
        Me.LoadListViewItems(Me.currentListView)

    End Sub

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Handles the <see cref="ToolStripMenuItem.Click"/> event of the <see cref="PropertyPage.CopyToolStripMenuItem"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub CopyToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CopyToolStripMenuItem.Click

        Dim str As String = String.Format("{0}: {1}", Me.currentListView.SelectedItems(0).SubItems(0).Text,
                                                      Me.currentListView.SelectedItems(0).SubItems(1).Text)

        Clipboard.SetText(str)

    End Sub

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Handles the <see cref="Button.Click"/> event of the <see cref="PropertyPage.ButtonSave"/> control.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="e">
    ''' The <see cref="EventArgs"/> instance containing the event data.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub ButtonSave_Click(sender As Object, e As EventArgs) Handles ButtonSave.Click

        Using sfd As New SaveFileDialog() With {
            .AddExtension = True,
            .CheckPathExists = True,
            .CreatePrompt = False,
            .DefaultExt = "txt",
            .FileName = String.Format("{0}.{1}", Path.GetFileName(Me.filePath), .DefaultExt),
            .Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt",
            .FilterIndex = 2,
            .OverwritePrompt = True,
            .RestoreDirectory = True,
            .ShowHelp = False,
            .SupportMultiDottedExtensions = True,
            .Title = "Save Assembly Info",
            .ValidateNames = True
        }

            If (sfd.ShowDialog = DialogResult.OK) Then
                Me.SaveAsTextFile(sfd.FileName)
            End If
        End Using

    End Sub

#End Region

#Region " Private Methods "

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Loads the assembly information into the listview items.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub LoadAssemblyInfo()

        asmInfo = New AssemblyInfo(Me.filePath)

        ' ListViewProduct Items

        Me.lvItemsProductDict("File Name").SubItems.Add(
            Path.GetFileName(Me.filePath))

        Me.lvItemsProductDict("Original Name").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.FileVersionInfo.OriginalFilename),
                                        asmInfo.FileVersionInfo.OriginalFilename, "(null)"))

        Me.lvItemsProductDict("Product Name").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.FileVersionInfo.ProductName),
                                        asmInfo.FileVersionInfo.ProductName, "(null)"))

        Me.lvItemsProductDict("Product Description").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.FileVersionInfo.FileDescription),
                                        asmInfo.FileVersionInfo.FileDescription, "(null)"))

        Me.lvItemsProductDict("Company Name").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.FileVersionInfo.CompanyName),
                                        asmInfo.FileVersionInfo.CompanyName, "(null)"))

        Me.lvItemsProductDict("Copyright").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.FileVersionInfo.LegalCopyright),
                                        asmInfo.FileVersionInfo.LegalCopyright, "(null)"))

        Me.lvItemsProductDict("File Version").SubItems.Add(
            asmInfo.AssemblyName.Version.ToString())

        Me.lvItemsProductDict("Product Version").SubItems.Add(
            asmInfo.FileVersionInfo.ProductVersion)

        Me.lvItemsProductDict("Language").SubItems.Add(
            If(asmInfo.DefaultCulture IsNot Nothing,
              String.Format("{0} ({1})", asmInfo.DefaultCulture.EnglishName, asmInfo.DefaultCulture.Name), "(null)"))

        ' ListViewBuild Items
        Me.lvItemsBuildDict("PE File Kind").SubItems.Add(
            asmInfo.PeFileKind)

        Me.lvItemsBuildDict("Platform").SubItems.Add(
            asmInfo.Platform)

        Me.lvItemsBuildDict("Build Configuration").SubItems.Add(
            If(asmInfo.FileVersionInfo.IsDebug, "Debug", "Release"))

        Me.lvItemsBuildDict("CLR Version").SubItems.Add(asmInfo.ClrVersion)

        Me.lvItemsBuildDict("Target .NET Framework").SubItems.Add(
            If(asmInfo.TargetNetFrameworkVersion IsNot Nothing,
               asmInfo.TargetNetFrameworkVersion.ToString(), "(null)"))

        Me.lvItemsBuildDict("Entry-Point Address").SubItems.Add(
            String.Format("0x{0}", asmInfo.EntryPointAddress.ToString("X2")))

        Me.lvItemsBuildDict("Root Namespace").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.RootNamespace.NamespaceName),
                                        asmInfo.RootNamespace.NamespaceName, "(null)"))

        Me.lvItemsBuildDict("Is Strong-Name Signed").SubItems.Add(
            If(asmInfo.IsStrongNameSigned, "Yes", "No"))

        Me.lvItemsBuildDict("Public Key Token").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.PublicKeyToken),
                                        asmInfo.PublicKeyToken.ToUpper(), "(null)"))

        Me.lvItemsBuildDict("Is DPI Awareness Disabled").SubItems.Add(
            If(asmInfo.IsDpiAwarenessDisabled, "Yes", "No"))

        Me.lvItemsBuildDict("Dll Import Search Paths").SubItems.Add(
            If(asmInfo.DefaultDllImportSearchPaths <> Nothing,
               asmInfo.DefaultDllImportSearchPaths.ToString(), "(default)"))

        Me.lvItemsBuildDict("Is CLS Compliant").SubItems.Add(
            If(asmInfo.IsClsCompliant, "Yes", "No"))

        Me.lvItemsBuildDict("Is COM Visible").SubItems.Add(
            If(asmInfo.IsCOMVisible, "Yes", "No"))

        Me.lvItemsBuildDict("Security Permission").SubItems.Add(
            If(asmInfo.SecurityPermission IsNot Nothing,
               asmInfo.SecurityPermission.Action.ToString(), "(null)"))

        Me.lvItemsBuildDict("Is Installed in GAC").SubItems.Add(
            If(asmInfo.IsInstalledInGAC, "Yes", "No"))

        Me.lvItemsBuildDict("GAC Name").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.GacFullName),
                                        asmInfo.GacFullName, "(null)"))

        Me.lvItemsBuildDict("GAC Path").SubItems.Add(
            If(Not String.IsNullOrEmpty(asmInfo.GacPath),
                                        asmInfo.GacPath, "(null)"))


        ' ListViewIdentity Items

        Me.lvItemsIdentityDict("Assembly GUID").SubItems.Add(
            If(asmInfo.Guid <> Nothing,
               asmInfo.Guid.ToString().ToUpper(), "(null)"))

        Me.lvItemsIdentityDict("Compilation TimeStamp").SubItems.Add(
            asmInfo.TimeStamp.ToString("dd/MMMM/yyyy HH:mm:ss", CultureInfo.InvariantCulture))

        Me.lvItemsIdentityDict("MD5 Hash").SubItems.Add(
            asmInfo.MD5Hash)

        Me.lvItemsIdentityDict("SHA-1 Hash").SubItems.Add(
            asmInfo.SHA1Hash)

        Me.lvItemsIdentityDict("SHA-256 Hash").SubItems.Add(
            asmInfo.SHA256Hash)

    End Sub

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Loads the listview items.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="addNullValues">
    ''' If <see langword="True"/>, also adds "(null)" values to the List View.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub LoadListViewItems(lv As ListView)

        Dim dict As Dictionary(Of String, ListViewItem) = DirectCast(lv.Tag, Dictionary(Of String, ListViewItem))

        lv.BeginUpdate()
        lv.Items.Clear()

        For Each lvItem As ListViewItem In dict.Values
            If asmInfo.IsNetCore Then
                Select Case lvItem.Text
                    Case Me.lvItemsProductDict("Language").Text,
                         Me.lvItemsIdentityDict("Assembly GUID").Text,
                         Me.lvItemsIdentityDict("Compilation TimeStamp").Text,
                         Me.lvItemsBuildDict("Is DPI Awareness Disabled").Text,
                         Me.lvItemsBuildDict("Dll Import Search Paths").Text,
                         Me.lvItemsBuildDict("Is CLS Compliant").Text,
                         Me.lvItemsBuildDict("Is COM Visible").Text,
                         Me.lvItemsBuildDict("Security Permission").Text,
                         Me.lvItemsBuildDict("Is Installed in GAC").Text,
                         Me.lvItemsBuildDict("GAC Name").Text,
                         Me.lvItemsBuildDict("GAC Path").Text,
                         Me.lvItemsBuildDict("Target .NET Framework").Text

                        ' IGNORE, values can't be retrieved for .NET Core assembly.

                    Case Else
                        lv.Items.Add(lvItem)

                End Select

            Else
                lv.Items.Add(lvItem)

            End If

        Next lvItem

        lv.EndUpdate()

    End Sub

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' Save the assembly info table as a plain text file.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    ''' <param name="filePath">
    ''' The destination txt file path.
    ''' </param>
    ''' ----------------------------------------------------------------------------------------------------
    Private Sub SaveAsTextFile(filePath As String)

        Dim sb As New StringBuilder()

        sb.AppendLine(Me.filePath)
        sb.AppendLine()

        For Each lv As ListView In {Me.ListViewProduct, Me.ListViewBuild, Me.ListViewIdentity}

            Dim dict As Dictionary(Of String, ListViewItem) = DirectCast(lv.Tag, Dictionary(Of String, ListViewItem))
            Dim largestItemName As String = dict.Keys.OrderByDescending(Function(str As String) str.Length).First()

            Dim groupName As String = lv.Text
            sb.AppendLine(groupName)
            sb.AppendLine(New String("-"c, groupName.Length))

            For Each lvItem As ListViewItem In dict.Values
                sb.AppendFormat("{0,-" & largestItemName.Length & "}: {1}", lvItem.SubItems(0).Text, lvItem.SubItems(1).Text)
                sb.AppendLine()
            Next lvItem

            sb.AppendLine()
        Next lv

        Try
            File.WriteAllText(filePath, sb.ToString(), Encoding.Default)

        Catch ex As Exception
            MessageBox.Show(Me, ex.Message, Me.Name, MessageBoxButtons.OK, MessageBoxIcon.Error)

        End Try

    End Sub

#End Region

End Class

#End Region