Imports SharpShell.SharpPropertySheet

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PropertyPage : Inherits SharpPropertyPage

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If (disposing) AndAlso (components IsNot Nothing) Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Me.ContextMenuStripListView = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.CopyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CopyValueToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparatorContext = New System.Windows.Forms.ToolStripSeparator()
        Me.CopySelectedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportSelectedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.TableLayoutPanelMain = New System.Windows.Forms.TableLayoutPanel()
        Me.TextBoxFilter = New System.Windows.Forms.TextBox()
        Me.TabControlListViews = New Usercontrols.TabControlFix()
        Me.TabPageProduct = New System.Windows.Forms.TabPage()
        Me.ListViewProduct = New System.Windows.Forms.ListView()
        Me.ColumnHeaderProperty = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeaderValue = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPageBuild = New System.Windows.Forms.TabPage()
        Me.ListViewBuild = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPageIdentity = New System.Windows.Forms.TabPage()
        Me.ListViewIdentity = New System.Windows.Forms.ListView()
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPageAttributes = New System.Windows.Forms.TabPage()
        Me.ListViewAttributes = New System.Windows.Forms.ListView()
        Me.ColumnHeader7 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader8 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPageReferences = New System.Windows.Forms.TabPage()
        Me.ListViewReferences = New System.Windows.Forms.ListView()
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TabPageRuntime = New System.Windows.Forms.TabPage()
        Me.ListViewRuntime = New System.Windows.Forms.ListView()
        Me.ColumnHeader9 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader10 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ToolStripMain = New System.Windows.Forms.ToolStrip()
        Me.ToolStripDropDownButtonExport = New System.Windows.Forms.ToolStripDropDownButton()
        Me.ExportAllToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportTabToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ExportSelectionToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSplitButtonCopyAll = New System.Windows.Forms.ToolStripSplitButton()
        Me.CopyAllTextToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.CopyAllMarkdownToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButtonCompare = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripButtonManifest = New System.Windows.Forms.ToolStripButton()
        Me.ToolStripDropDownButtonConfig = New System.Windows.Forms.ToolStripDropDownButton()
        Me.ToolStripSeparator3 = New System.Windows.Forms.ToolStripSeparator()
        Me.ToolStripSplitButtonRegEdit = New System.Windows.Forms.ToolStripSplitButton()
        Me.ToolStripButtonGac = New System.Windows.Forms.ToolStripButton()
        Me.ContextMenuStripListView.SuspendLayout()
        Me.TableLayoutPanelMain.SuspendLayout()
        Me.TabControlListViews.SuspendLayout()
        Me.TabPageProduct.SuspendLayout()
        Me.TabPageBuild.SuspendLayout()
        Me.TabPageIdentity.SuspendLayout()
        Me.TabPageAttributes.SuspendLayout()
        Me.TabPageReferences.SuspendLayout()
        Me.TabPageRuntime.SuspendLayout()
        Me.ToolStripMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContextMenuStripListView
        '
        Me.ContextMenuStripListView.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CopyToolStripMenuItem, Me.CopyValueToolStripMenuItem, Me.ToolStripSeparatorContext, Me.CopySelectedToolStripMenuItem, Me.ExportSelectedToolStripMenuItem})
        Me.ContextMenuStripListView.Name = "ContextMenuStripListView"
        Me.ContextMenuStripListView.Size = New System.Drawing.Size(196, 98)
        '
        'CopyToolStripMenuItem
        '
        Me.CopyToolStripMenuItem.Image = Global.My.Resources.Resources.Copy
        Me.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem"
        Me.CopyToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.CopyToolStripMenuItem.Text = "Copy"
        '
        'CopyValueToolStripMenuItem
        '
        Me.CopyValueToolStripMenuItem.Image = Global.My.Resources.Resources.Copy
        Me.CopyValueToolStripMenuItem.Name = "CopyValueToolStripMenuItem"
        Me.CopyValueToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.CopyValueToolStripMenuItem.Text = "Copy Value"
        '
        'CopySelectedToolStripMenuItem
        '
        Me.CopySelectedToolStripMenuItem.Image = Global.My.Resources.Resources.CopyAll
        Me.CopySelectedToolStripMenuItem.Name = "CopySelectedToolStripMenuItem"
        Me.CopySelectedToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.CopySelectedToolStripMenuItem.Text = "Copy Selected Rows"
        '
        'ExportSelectedToolStripMenuItem
        '
        Me.ExportSelectedToolStripMenuItem.Image = Global.My.Resources.Resources.ExportSelection
        Me.ExportSelectedToolStripMenuItem.Name = "ExportSelectedToolStripMenuItem"
        Me.ExportSelectedToolStripMenuItem.Size = New System.Drawing.Size(195, 22)
        Me.ExportSelectedToolStripMenuItem.Text = "Export Selected Rows..."
        '
        'ToolStripSeparatorContext
        '
        Me.ToolStripSeparatorContext.Name = "ToolStripSeparatorContext"
        Me.ToolStripSeparatorContext.Size = New System.Drawing.Size(192, 6)
        '
        'TableLayoutPanelMain
        '
        Me.TableLayoutPanelMain.ColumnCount = 1
        Me.TableLayoutPanelMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanelMain.Controls.Add(Me.TextBoxFilter, 0, 0)
        Me.TableLayoutPanelMain.Controls.Add(Me.TabControlListViews, 0, 1)
        Me.TableLayoutPanelMain.Controls.Add(Me.ToolStripMain, 0, 2)
        Me.TableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanelMain.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanelMain.Name = "TableLayoutPanelMain"
        Me.TableLayoutPanelMain.RowCount = 3
        Me.TableLayoutPanelMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 26.0!))
        Me.TableLayoutPanelMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanelMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28.0!))
        Me.TableLayoutPanelMain.Size = New System.Drawing.Size(339, 422)
        Me.TableLayoutPanelMain.TabIndex = 0
        '
        'TextBoxFilter
        '
        Me.TextBoxFilter.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TextBoxFilter.Location = New System.Drawing.Point(3, 3)
        Me.TextBoxFilter.Name = "TextBoxFilter"
        Me.TextBoxFilter.Size = New System.Drawing.Size(333, 20)
        Me.TextBoxFilter.TabIndex = 0
        '
        'TabControlListViews
        '
        Me.TabControlListViews.Controls.Add(Me.TabPageProduct)
        Me.TabControlListViews.Controls.Add(Me.TabPageBuild)
        Me.TabControlListViews.Controls.Add(Me.TabPageIdentity)
        Me.TabControlListViews.Controls.Add(Me.TabPageAttributes)
        Me.TabControlListViews.Controls.Add(Me.TabPageReferences)
        Me.TabControlListViews.Controls.Add(Me.TabPageRuntime)
        Me.TabControlListViews.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControlListViews.Location = New System.Drawing.Point(3, 29)
        Me.TabControlListViews.Name = "TabControlListViews"
        Me.TabControlListViews.Padding = New System.Drawing.Point(4, 3)
        Me.TabControlListViews.SelectedIndex = 0
        Me.TabControlListViews.Size = New System.Drawing.Size(333, 362)
        Me.TabControlListViews.TabIndex = 1
        '
        'TabPageProduct
        '
        Me.TabPageProduct.Controls.Add(Me.ListViewProduct)
        Me.TabPageProduct.Location = New System.Drawing.Point(4, 22)
        Me.TabPageProduct.Name = "TabPageProduct"
        Me.TabPageProduct.Size = New System.Drawing.Size(325, 336)
        Me.TabPageProduct.TabIndex = 0
        Me.TabPageProduct.Text = "Product"
        Me.TabPageProduct.UseVisualStyleBackColor = True
        '
        'ListViewProduct
        '
        Me.ListViewProduct.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeaderProperty, Me.ColumnHeaderValue})
        Me.ListViewProduct.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewProduct.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewProduct.FullRowSelect = True
        Me.ListViewProduct.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewProduct.HideSelection = False
        Me.ListViewProduct.Location = New System.Drawing.Point(0, 0)
        Me.ListViewProduct.Name = "ListViewProduct"
        Me.ListViewProduct.ShowItemToolTips = True
        Me.ListViewProduct.Size = New System.Drawing.Size(325, 336)
        Me.ListViewProduct.TabIndex = 0
        Me.ListViewProduct.UseCompatibleStateImageBehavior = False
        Me.ListViewProduct.View = System.Windows.Forms.View.Details
        '
        'ColumnHeaderProperty
        '
        Me.ColumnHeaderProperty.Text = "Property"
        Me.ColumnHeaderProperty.Width = 130
        '
        'ColumnHeaderValue
        '
        Me.ColumnHeaderValue.Text = "Value"
        Me.ColumnHeaderValue.Width = 180
        '
        'TabPageBuild
        '
        Me.TabPageBuild.Controls.Add(Me.ListViewBuild)
        Me.TabPageBuild.Location = New System.Drawing.Point(4, 22)
        Me.TabPageBuild.Name = "TabPageBuild"
        Me.TabPageBuild.Size = New System.Drawing.Size(325, 336)
        Me.TabPageBuild.TabIndex = 1
        Me.TabPageBuild.Text = "Build"
        Me.TabPageBuild.UseVisualStyleBackColor = True
        '
        'ListViewBuild
        '
        Me.ListViewBuild.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2})
        Me.ListViewBuild.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewBuild.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewBuild.FullRowSelect = True
        Me.ListViewBuild.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewBuild.HideSelection = False
        Me.ListViewBuild.Location = New System.Drawing.Point(0, 0)
        Me.ListViewBuild.Name = "ListViewBuild"
        Me.ListViewBuild.ShowItemToolTips = True
        Me.ListViewBuild.Size = New System.Drawing.Size(325, 336)
        Me.ListViewBuild.TabIndex = 0
        Me.ListViewBuild.UseCompatibleStateImageBehavior = False
        Me.ListViewBuild.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        Me.ColumnHeader1.Text = "Property"
        Me.ColumnHeader1.Width = 130
        '
        'ColumnHeader2
        '
        Me.ColumnHeader2.Text = "Value"
        Me.ColumnHeader2.Width = 180
        '
        'TabPageIdentity
        '
        Me.TabPageIdentity.Controls.Add(Me.ListViewIdentity)
        Me.TabPageIdentity.Location = New System.Drawing.Point(4, 22)
        Me.TabPageIdentity.Name = "TabPageIdentity"
        Me.TabPageIdentity.Size = New System.Drawing.Size(325, 336)
        Me.TabPageIdentity.TabIndex = 2
        Me.TabPageIdentity.Text = "Identity"
        Me.TabPageIdentity.UseVisualStyleBackColor = True
        '
        'ListViewIdentity
        '
        Me.ListViewIdentity.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader5, Me.ColumnHeader6})
        Me.ListViewIdentity.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewIdentity.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewIdentity.FullRowSelect = True
        Me.ListViewIdentity.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewIdentity.HideSelection = False
        Me.ListViewIdentity.Location = New System.Drawing.Point(0, 0)
        Me.ListViewIdentity.Name = "ListViewIdentity"
        Me.ListViewIdentity.ShowItemToolTips = True
        Me.ListViewIdentity.Size = New System.Drawing.Size(325, 336)
        Me.ListViewIdentity.TabIndex = 0
        Me.ListViewIdentity.UseCompatibleStateImageBehavior = False
        Me.ListViewIdentity.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader5
        '
        Me.ColumnHeader5.Text = "Property"
        Me.ColumnHeader5.Width = 130
        '
        'ColumnHeader6
        '
        Me.ColumnHeader6.Text = "Value"
        Me.ColumnHeader6.Width = 180
        '
        'TabPageAttributes
        '
        Me.TabPageAttributes.Controls.Add(Me.ListViewAttributes)
        Me.TabPageAttributes.Location = New System.Drawing.Point(4, 22)
        Me.TabPageAttributes.Name = "TabPageAttributes"
        Me.TabPageAttributes.Size = New System.Drawing.Size(325, 336)
        Me.TabPageAttributes.TabIndex = 3
        Me.TabPageAttributes.Text = "Attributes"
        Me.TabPageAttributes.UseVisualStyleBackColor = True
        '
        'ListViewAttributes
        '
        Me.ListViewAttributes.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader7, Me.ColumnHeader8})
        Me.ListViewAttributes.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewAttributes.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewAttributes.FullRowSelect = True
        Me.ListViewAttributes.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewAttributes.HideSelection = False
        Me.ListViewAttributes.Location = New System.Drawing.Point(0, 0)
        Me.ListViewAttributes.Name = "ListViewAttributes"
        Me.ListViewAttributes.ShowItemToolTips = True
        Me.ListViewAttributes.Size = New System.Drawing.Size(325, 336)
        Me.ListViewAttributes.TabIndex = 0
        Me.ListViewAttributes.UseCompatibleStateImageBehavior = False
        Me.ListViewAttributes.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader7
        '
        Me.ColumnHeader7.Text = "Property"
        Me.ColumnHeader7.Width = 130
        '
        'ColumnHeader8
        '
        Me.ColumnHeader8.Text = "Value"
        Me.ColumnHeader8.Width = 180
        '
        'TabPageReferences
        '
        Me.TabPageReferences.Controls.Add(Me.ListViewReferences)
        Me.TabPageReferences.Location = New System.Drawing.Point(4, 22)
        Me.TabPageReferences.Name = "TabPageReferences"
        Me.TabPageReferences.Size = New System.Drawing.Size(325, 336)
        Me.TabPageReferences.TabIndex = 4
        Me.TabPageReferences.Text = "References"
        Me.TabPageReferences.UseVisualStyleBackColor = True
        '
        'ListViewReferences
        '
        Me.ListViewReferences.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader3, Me.ColumnHeader4})
        Me.ListViewReferences.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewReferences.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewReferences.FullRowSelect = True
        Me.ListViewReferences.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewReferences.HideSelection = False
        Me.ListViewReferences.Location = New System.Drawing.Point(0, 0)
        Me.ListViewReferences.Name = "ListViewReferences"
        Me.ListViewReferences.ShowItemToolTips = True
        Me.ListViewReferences.Size = New System.Drawing.Size(325, 336)
        Me.ListViewReferences.TabIndex = 0
        Me.ListViewReferences.UseCompatibleStateImageBehavior = False
        Me.ListViewReferences.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader3
        '
        Me.ColumnHeader3.Text = "Name"
        Me.ColumnHeader3.Width = 130
        '
        'ColumnHeader4
        '
        Me.ColumnHeader4.Text = "Version"
        Me.ColumnHeader4.Width = 180
        '
        'TabPageRuntime
        '
        Me.TabPageRuntime.Controls.Add(Me.ListViewRuntime)
        Me.TabPageRuntime.Location = New System.Drawing.Point(4, 22)
        Me.TabPageRuntime.Name = "TabPageRuntime"
        Me.TabPageRuntime.Size = New System.Drawing.Size(325, 336)
        Me.TabPageRuntime.TabIndex = 5
        Me.TabPageRuntime.Text = "Runtime"
        Me.TabPageRuntime.UseVisualStyleBackColor = True
        '
        'ListViewRuntime
        '
        Me.ListViewRuntime.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader9, Me.ColumnHeader10})
        Me.ListViewRuntime.ContextMenuStrip = Me.ContextMenuStripListView
        Me.ListViewRuntime.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ListViewRuntime.FullRowSelect = True
        Me.ListViewRuntime.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable
        Me.ListViewRuntime.HideSelection = False
        Me.ListViewRuntime.Location = New System.Drawing.Point(0, 0)
        Me.ListViewRuntime.Name = "ListViewRuntime"
        Me.ListViewRuntime.ShowItemToolTips = True
        Me.ListViewRuntime.Size = New System.Drawing.Size(325, 336)
        Me.ListViewRuntime.TabIndex = 0
        Me.ListViewRuntime.UseCompatibleStateImageBehavior = False
        Me.ListViewRuntime.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader9
        '
        Me.ColumnHeader9.Text = "Property"
        Me.ColumnHeader9.Width = 130
        '
        'ColumnHeader10
        '
        Me.ColumnHeader10.Text = "Value"
        Me.ColumnHeader10.Width = 180
        '
        'ToolStripMain
        '
        Me.ToolStripMain.BackColor = System.Drawing.SystemColors.Window
        Me.ToolStripMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.ToolStripMain.ForeColor = System.Drawing.SystemColors.ControlText
        Me.ToolStripMain.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden
        Me.ToolStripMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripDropDownButtonExport, Me.ToolStripSplitButtonCopyAll, Me.ToolStripSeparator1, Me.ToolStripButtonCompare, Me.ToolStripSeparator2, Me.ToolStripButtonManifest, Me.ToolStripDropDownButtonConfig, Me.ToolStripSeparator3, Me.ToolStripSplitButtonRegEdit, Me.ToolStripButtonGac})
        Me.ToolStripMain.Location = New System.Drawing.Point(0, 394)
        Me.ToolStripMain.Name = "ToolStripMain"
        Me.ToolStripMain.Size = New System.Drawing.Size(339, 28)
        Me.ToolStripMain.TabIndex = 2
        '
        'ToolStripDropDownButtonExport
        '
        Me.ToolStripDropDownButtonExport.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExportAllToolStripMenuItem, Me.ExportTabToolStripMenuItem, Me.ExportSelectionToolStripMenuItem})
        Me.ToolStripDropDownButtonExport.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText
        Me.ToolStripDropDownButtonExport.Image = Global.My.Resources.Resources.Save
        Me.ToolStripDropDownButtonExport.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripDropDownButtonExport.Name = "ToolStripDropDownButtonExport"
        Me.ToolStripDropDownButtonExport.Text = "Export"
        Me.ToolStripDropDownButtonExport.ToolTipText = "Export the assembly information to a file (TXT, MD, HTML, JSON, XML, CSV)"
        '
        'ExportAllToolStripMenuItem
        '
        Me.ExportAllToolStripMenuItem.Image = Global.My.Resources.Resources.Save
        Me.ExportAllToolStripMenuItem.Name = "ExportAllToolStripMenuItem"
        Me.ExportAllToolStripMenuItem.Size = New System.Drawing.Size(196, 22)
        Me.ExportAllToolStripMenuItem.Text = "Export All Tabs..."
        '
        'ExportTabToolStripMenuItem
        '
        Me.ExportTabToolStripMenuItem.Image = Global.My.Resources.Resources.ExportTab
        Me.ExportTabToolStripMenuItem.Name = "ExportTabToolStripMenuItem"
        Me.ExportTabToolStripMenuItem.Size = New System.Drawing.Size(196, 22)
        Me.ExportTabToolStripMenuItem.Text = "Export Current Tab..."
        '
        'ExportSelectionToolStripMenuItem
        '
        Me.ExportSelectionToolStripMenuItem.Image = Global.My.Resources.Resources.ExportSelection
        Me.ExportSelectionToolStripMenuItem.Name = "ExportSelectionToolStripMenuItem"
        Me.ExportSelectionToolStripMenuItem.Size = New System.Drawing.Size(196, 22)
        Me.ExportSelectionToolStripMenuItem.Text = "Export Selected Rows..."
        '
        'ToolStripSplitButtonCopyAll
        '
        Me.ToolStripSplitButtonCopyAll.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CopyAllTextToolStripMenuItem, Me.CopyAllMarkdownToolStripMenuItem})
        Me.ToolStripSplitButtonCopyAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripSplitButtonCopyAll.Image = Global.My.Resources.Resources.CopyAll
        Me.ToolStripSplitButtonCopyAll.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripSplitButtonCopyAll.Name = "ToolStripSplitButtonCopyAll"
        Me.ToolStripSplitButtonCopyAll.Text = "Copy All"
        Me.ToolStripSplitButtonCopyAll.ToolTipText = "Copy all the tabs to the clipboard"
        '
        'CopyAllTextToolStripMenuItem
        '
        Me.CopyAllTextToolStripMenuItem.Image = Global.My.Resources.Resources.CopyAll
        Me.CopyAllTextToolStripMenuItem.Name = "CopyAllTextToolStripMenuItem"
        Me.CopyAllTextToolStripMenuItem.Size = New System.Drawing.Size(196, 22)
        Me.CopyAllTextToolStripMenuItem.Text = "Copy All as Text"
        '
        'CopyAllMarkdownToolStripMenuItem
        '
        Me.CopyAllMarkdownToolStripMenuItem.Image = Global.My.Resources.Resources.CopyAll
        Me.CopyAllMarkdownToolStripMenuItem.Name = "CopyAllMarkdownToolStripMenuItem"
        Me.CopyAllMarkdownToolStripMenuItem.Size = New System.Drawing.Size(196, 22)
        Me.CopyAllMarkdownToolStripMenuItem.Text = "Copy All as Markdown"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(6, 28)
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(6, 28)
        '
        'ToolStripSeparator3
        '
        Me.ToolStripSeparator3.Name = "ToolStripSeparator3"
        Me.ToolStripSeparator3.Size = New System.Drawing.Size(6, 28)
        '
        'ToolStripButtonCompare
        '
        Me.ToolStripButtonCompare.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonCompare.Image = Global.My.Resources.Resources.Compare
        Me.ToolStripButtonCompare.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonCompare.Name = "ToolStripButtonCompare"
        Me.ToolStripButtonCompare.Text = "Compare..."
        Me.ToolStripButtonCompare.ToolTipText = "Compare with another assembly..."
        '
        'ToolStripButtonManifest
        '
        Me.ToolStripButtonManifest.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonManifest.Image = Global.My.Resources.Resources.Manifest
        Me.ToolStripButtonManifest.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonManifest.Name = "ToolStripButtonManifest"
        Me.ToolStripButtonManifest.Text = "Win32 Manifest"
        Me.ToolStripButtonManifest.ToolTipText = "View the Win32 manifest"
        '
        'ToolStripDropDownButtonConfig
        '
        Me.ToolStripDropDownButtonConfig.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripDropDownButtonConfig.Image = Global.My.Resources.Resources.Config
        Me.ToolStripDropDownButtonConfig.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripDropDownButtonConfig.Name = "ToolStripDropDownButtonConfig"
        Me.ToolStripDropDownButtonConfig.Text = "Configuration Files"
        Me.ToolStripDropDownButtonConfig.ToolTipText = "View the configuration files (app.config, runtimeconfig.json, deps.json)"
        '
        'ToolStripSplitButtonRegEdit
        '
        Me.ToolStripSplitButtonRegEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripSplitButtonRegEdit.Image = Global.My.Resources.Resources.RegEdit
        Me.ToolStripSplitButtonRegEdit.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripSplitButtonRegEdit.Name = "ToolStripSplitButtonRegEdit"
        Me.ToolStripSplitButtonRegEdit.Text = "Open in Registry Editor"
        Me.ToolStripSplitButtonRegEdit.ToolTipText = "Open the COM registration in Registry Editor"
        '
        'ToolStripButtonGac
        '
        Me.ToolStripButtonGac.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image
        Me.ToolStripButtonGac.Image = Global.My.Resources.Resources.Gac
        Me.ToolStripButtonGac.ImageTransparentColor = System.Drawing.Color.Magenta
        Me.ToolStripButtonGac.Name = "ToolStripButtonGac"
        Me.ToolStripButtonGac.Text = "Show in GAC"
        Me.ToolStripButtonGac.ToolTipText = "Show the assembly in the Global Assembly Cache folder"
        '
        'PropertyPage
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Window
        Me.Controls.Add(Me.TableLayoutPanelMain)
        Me.Name = "PropertyPage"
        Me.Size = New System.Drawing.Size(339, 422)
        Me.ContextMenuStripListView.ResumeLayout(False)
        Me.TableLayoutPanelMain.ResumeLayout(False)
        Me.TableLayoutPanelMain.PerformLayout()
        Me.TabControlListViews.ResumeLayout(False)
        Me.TabPageProduct.ResumeLayout(False)
        Me.TabPageBuild.ResumeLayout(False)
        Me.TabPageIdentity.ResumeLayout(False)
        Me.TabPageAttributes.ResumeLayout(False)
        Me.TabPageReferences.ResumeLayout(False)
        Me.TabPageRuntime.ResumeLayout(False)
        Me.ToolStripMain.ResumeLayout(False)
        Me.ToolStripMain.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ContextMenuStripListView As Windows.Forms.ContextMenuStrip
    Friend WithEvents CopyToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopyValueToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparatorContext As Windows.Forms.ToolStripSeparator
    Friend WithEvents CopySelectedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportSelectedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents TableLayoutPanelMain As Windows.Forms.TableLayoutPanel
    Friend WithEvents TextBoxFilter As Windows.Forms.TextBox
    Friend WithEvents TabControlListViews As Usercontrols.TabControlFix
    Friend WithEvents TabPageProduct As Windows.Forms.TabPage
    Friend WithEvents ListViewProduct As Windows.Forms.ListView
    Friend WithEvents ColumnHeaderProperty As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeaderValue As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageBuild As Windows.Forms.TabPage
    Friend WithEvents ListViewBuild As Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageIdentity As Windows.Forms.TabPage
    Friend WithEvents ListViewIdentity As Windows.Forms.ListView
    Friend WithEvents ColumnHeader5 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageAttributes As Windows.Forms.TabPage
    Friend WithEvents ListViewAttributes As Windows.Forms.ListView
    Friend WithEvents ColumnHeader7 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader8 As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageReferences As Windows.Forms.TabPage
    Friend WithEvents ListViewReferences As Windows.Forms.ListView
    Friend WithEvents ColumnHeader3 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageRuntime As Windows.Forms.TabPage
    Friend WithEvents ListViewRuntime As Windows.Forms.ListView
    Friend WithEvents ColumnHeader9 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader10 As Windows.Forms.ColumnHeader
    Friend WithEvents ToolStripMain As Windows.Forms.ToolStrip
    Friend WithEvents ToolStripDropDownButtonExport As Windows.Forms.ToolStripDropDownButton
    Friend WithEvents ExportAllToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportTabToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExportSelectionToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSplitButtonCopyAll As Windows.Forms.ToolStripSplitButton
    Friend WithEvents CopyAllTextToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopyAllMarkdownToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButtonCompare As Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripSeparator2 As Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripButtonManifest As Windows.Forms.ToolStripButton
    Friend WithEvents ToolStripDropDownButtonConfig As Windows.Forms.ToolStripDropDownButton
    Friend WithEvents ToolStripSeparator3 As Windows.Forms.ToolStripSeparator
    Friend WithEvents ToolStripSplitButtonRegEdit As Windows.Forms.ToolStripSplitButton
    Friend WithEvents ToolStripButtonGac As Windows.Forms.ToolStripButton
End Class
