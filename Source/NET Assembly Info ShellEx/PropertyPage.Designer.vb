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
        Me.TableLayoutPanelMain = New System.Windows.Forms.TableLayoutPanel()
        Me.PanelBottom = New System.Windows.Forms.Panel()
        Me.ButtonSave = New System.Windows.Forms.Button()
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
        Me.ContextMenuStripListView.SuspendLayout()
        Me.TableLayoutPanelMain.SuspendLayout()
        Me.PanelBottom.SuspendLayout()
        Me.TabControlListViews.SuspendLayout()
        Me.TabPageProduct.SuspendLayout()
        Me.TabPageBuild.SuspendLayout()
        Me.TabPageIdentity.SuspendLayout()
        Me.SuspendLayout()
        '
        'ContextMenuStripListView
        '
        Me.ContextMenuStripListView.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.CopyToolStripMenuItem})
        Me.ContextMenuStripListView.Name = "ContextMenuStrip1"
        Me.ContextMenuStripListView.Size = New System.Drawing.Size(102, 26)
        '
        'CopyToolStripMenuItem
        '
        Me.CopyToolStripMenuItem.Image = Global.My.Resources.Resources.Copy
        Me.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem"
        Me.CopyToolStripMenuItem.Size = New System.Drawing.Size(101, 22)
        Me.CopyToolStripMenuItem.Text = "Copy"
        '
        'TableLayoutPanelMain
        '
        Me.TableLayoutPanelMain.ColumnCount = 1
        Me.TableLayoutPanelMain.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.TableLayoutPanelMain.Controls.Add(Me.PanelBottom, 0, 1)
        Me.TableLayoutPanelMain.Controls.Add(Me.TabControlListViews, 0, 0)
        Me.TableLayoutPanelMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TableLayoutPanelMain.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanelMain.Name = "TableLayoutPanelMain"
        Me.TableLayoutPanelMain.RowCount = 2
        Me.TableLayoutPanelMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.69212!))
        Me.TableLayoutPanelMain.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.307876!))
        Me.TableLayoutPanelMain.Size = New System.Drawing.Size(339, 422)
        Me.TableLayoutPanelMain.TabIndex = 37
        '
        'PanelBottom
        '
        Me.PanelBottom.Controls.Add(Me.ButtonSave)
        Me.PanelBottom.Location = New System.Drawing.Point(3, 385)
        Me.PanelBottom.Name = "PanelBottom"
        Me.PanelBottom.Size = New System.Drawing.Size(333, 34)
        Me.PanelBottom.TabIndex = 38
        '
        'ButtonSave
        '
        Me.ButtonSave.Image = Global.My.Resources.Resources.Save
        Me.ButtonSave.ImageAlign = System.Drawing.ContentAlignment.TopLeft
        Me.ButtonSave.Location = New System.Drawing.Point(4, 6)
        Me.ButtonSave.Name = "ButtonSave"
        Me.ButtonSave.Size = New System.Drawing.Size(82, 23)
        Me.ButtonSave.TabIndex = 0
        Me.ButtonSave.Text = "Export..."
        Me.ButtonSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText
        Me.ButtonSave.UseVisualStyleBackColor = True
        '
        'TabControlListViews
        '
        Me.TabControlListViews.Controls.Add(Me.TabPageProduct)
        Me.TabControlListViews.Controls.Add(Me.TabPageBuild)
        Me.TabControlListViews.Controls.Add(Me.TabPageIdentity)
        Me.TabControlListViews.Dock = System.Windows.Forms.DockStyle.Fill
        Me.TabControlListViews.Location = New System.Drawing.Point(3, 3)
        Me.TabControlListViews.Name = "TabControlListViews"
        Me.TabControlListViews.SelectedIndex = 0
        Me.TabControlListViews.Size = New System.Drawing.Size(333, 376)
        Me.TabControlListViews.TabIndex = 38
        '
        'TabPageProduct
        '
        Me.TabPageProduct.Controls.Add(Me.ListViewProduct)
        Me.TabPageProduct.Location = New System.Drawing.Point(4, 22)
        Me.TabPageProduct.Name = "TabPageProduct"
        Me.TabPageProduct.Size = New System.Drawing.Size(325, 350)
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
        Me.ListViewProduct.Location = New System.Drawing.Point(0, 0)
        Me.ListViewProduct.Name = "ListViewProduct"
        Me.ListViewProduct.Size = New System.Drawing.Size(325, 350)
        Me.ListViewProduct.TabIndex = 34
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
        Me.TabPageBuild.Size = New System.Drawing.Size(325, 350)
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
        Me.ListViewBuild.Location = New System.Drawing.Point(0, 0)
        Me.ListViewBuild.Name = "ListViewBuild"
        Me.ListViewBuild.Size = New System.Drawing.Size(325, 350)
        Me.ListViewBuild.TabIndex = 40
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
        Me.TabPageIdentity.Size = New System.Drawing.Size(325, 350)
        Me.TabPageIdentity.TabIndex = 3
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
        Me.ListViewIdentity.Location = New System.Drawing.Point(0, 0)
        Me.ListViewIdentity.Name = "ListViewIdentity"
        Me.ListViewIdentity.Size = New System.Drawing.Size(325, 350)
        Me.ListViewIdentity.TabIndex = 39
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
        Me.PanelBottom.ResumeLayout(False)
        Me.TabControlListViews.ResumeLayout(False)
        Me.TabPageProduct.ResumeLayout(False)
        Me.TabPageBuild.ResumeLayout(False)
        Me.TabPageIdentity.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents ListViewProduct As Windows.Forms.ListView
    Friend WithEvents ColumnHeaderProperty As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeaderValue As Windows.Forms.ColumnHeader
    Friend WithEvents ContextMenuStripListView As Windows.Forms.ContextMenuStrip
    Friend WithEvents CopyToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    Friend WithEvents TableLayoutPanelMain As Windows.Forms.TableLayoutPanel
    Friend WithEvents PanelBottom As Windows.Forms.Panel
    Friend WithEvents ButtonSave As Windows.Forms.Button
    Friend WithEvents TabControlListViews As Usercontrols.TabControlFix
    Friend WithEvents TabPageProduct As Windows.Forms.TabPage
    Friend WithEvents TabPageBuild As Windows.Forms.TabPage
    Friend WithEvents ListViewBuild As Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As Windows.Forms.ColumnHeader
    Friend WithEvents TabPageIdentity As Windows.Forms.TabPage
    Friend WithEvents ListViewIdentity As Windows.Forms.ListView
    Friend WithEvents ColumnHeader5 As Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader6 As Windows.Forms.ColumnHeader
End Class

