
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms

Imports AssemblyInspection

#End Region

#Region " CompareForm "

''' <summary>
''' Shows the row-by-row comparison of two assemblies:
''' green rows are equal, red rows are different, and yellow rows exist only in one of them.
''' </summary>
Friend NotInheritable Class CompareForm : Inherits Form

#Region " Constants "

    ''' <summary>The text shown for a property that the assembly does not have.</summary>
    Private Const MissingText As String = "(missing)"

#End Region

#Region " Private Fields "

    ''' <summary>The background color of equal rows.</summary>
    Friend Shared ReadOnly EqualColor As Color = Color.FromArgb(230, 244, 234)

    ''' <summary>The background color of different rows.</summary>
    Friend Shared ReadOnly DifferentColor As Color = Color.FromArgb(253, 226, 228)

    ''' <summary>The background color of the rows that only exist in one assembly.</summary>
    Friend Shared ReadOnly MissingColor As Color = Color.FromArgb(255, 243, 205)

    ''' <summary>The comparisons.</summary>
    Private ReadOnly comparisons As ReadOnlyCollection(Of PropertyComparison)

    ''' <summary>The list view that shows the comparisons.</summary>
    Private ReadOnly listViewComparison As ListView

    ''' <summary>The check box that hides the equal rows.</summary>
    Private ReadOnly checkBoxDifferencesOnly As CheckBox

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="CompareForm"/> class.
    ''' </summary>
    ''' <param name="leftPath">
    ''' The path of the inspected assembly.
    ''' </param>
    '''
    ''' <param name="rightPath">
    ''' The path of the compared assembly.
    ''' </param>
    '''
    ''' <param name="comparisons">
    ''' The comparisons.
    ''' </param>
    Friend Sub New(leftPath As String, rightPath As String, comparisons As ReadOnlyCollection(Of PropertyComparison))

        If comparisons Is Nothing Then
            Throw New ArgumentNullException(NameOf(comparisons))
        End If
        Me.comparisons = comparisons

        Me.SuspendLayout()
        Me.Text = $"Compare: {Path.GetFileName(leftPath)} vs. {Path.GetFileName(rightPath)}"
        Me.Icon = My.Resources.VisualStudio
        Me.StartPosition = FormStartPosition.CenterParent
        Me.ShowInTaskbar = False
        Me.MinimizeBox = False
        Me.Size = New Size(900, 640)
        Me.MinimumSize = New Size(500, 300)

        Dim labelFiles As New Label() With {
            .Dock = DockStyle.Top,
            .AutoSize = False,
            .Height = 40,
            .Padding = New Padding(6, 4, 6, 0),
            .AutoEllipsis = True,
            .Text = $"This assembly: {leftPath}{Environment.NewLine}Other assembly: {rightPath}"
        }

        Me.listViewComparison = New ListView() With {
            .Dock = DockStyle.Fill,
            .View = View.Details,
            .FullRowSelect = True,
            .HeaderStyle = ColumnHeaderStyle.Nonclickable,
            .ShowGroups = True,
            .ShowItemToolTips = True,
            .HideSelection = False
        }
        Me.listViewComparison.Columns.Add("Property", 220)
        Me.listViewComparison.Columns.Add(Path.GetFileName(leftPath), 310)
        Me.listViewComparison.Columns.Add(Path.GetFileName(rightPath), 310)

        Dim equalCount As Integer = comparisons.Where(Function(c As PropertyComparison) c.Result = ComparisonResult.Equal).Count()
        Dim differentCount As Integer = comparisons.Where(Function(c As PropertyComparison) c.Result = ComparisonResult.Different).Count()
        Dim missingCount As Integer = comparisons.Count - equalCount - differentCount

        Me.checkBoxDifferencesOnly = New CheckBox() With {
            .Text = "Show differences only",
            .AutoSize = True,
            .Margin = New Padding(6, 9, 6, 3)
        }
        AddHandler Me.checkBoxDifferencesOnly.CheckedChanged, AddressOf Me.CheckBoxDifferencesOnly_CheckedChanged

        Dim labelSummary As New Label() With {
            .AutoSize = True,
            .Margin = New Padding(12, 10, 6, 3),
            .Text = $"{equalCount} equal (green), {differentCount} different (red), {missingCount} missing in one side (yellow)"
        }

        Dim buttonClose As New Button() With {.Text = "Close", .DialogResult = DialogResult.Cancel, .AutoSize = True, .Anchor = AnchorStyles.Right}

        Dim panelBottom As New TableLayoutPanel() With {.Dock = DockStyle.Bottom, .AutoSize = True, .ColumnCount = 3, .Padding = New Padding(3)}
        panelBottom.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        panelBottom.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        panelBottom.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        panelBottom.Controls.Add(Me.checkBoxDifferencesOnly, 0, 0)
        panelBottom.Controls.Add(labelSummary, 1, 0)
        panelBottom.Controls.Add(buttonClose, 2, 0)

        Me.Controls.Add(Me.listViewComparison)
        Me.Controls.Add(labelFiles)
        Me.Controls.Add(panelBottom)
        Me.CancelButton = buttonClose
        Me.ResumeLayout(performLayout:=True)

        Me.LoadItems()

    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>Reloads the items when the "differences only" option changes.</summary>
    Private Sub CheckBoxDifferencesOnly_CheckedChanged(sender As Object, e As EventArgs)
        Me.LoadItems()
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>Loads the comparison rows, grouped by property group.</summary>
    Private Sub LoadItems()

        Me.listViewComparison.BeginUpdate()
        Me.listViewComparison.Items.Clear()
        Me.listViewComparison.Groups.Clear()

        Dim groups As New Dictionary(Of String, ListViewGroup)(StringComparer.Ordinal)
        For Each comparison As PropertyComparison In Me.comparisons
            If Me.checkBoxDifferencesOnly.Checked AndAlso (comparison.Result = ComparisonResult.Equal) Then
                Continue For
            End If

            Dim group As ListViewGroup = Nothing
            If Not groups.TryGetValue(comparison.GroupTitle, group) Then
                group = New ListViewGroup(comparison.GroupTitle, HorizontalAlignment.Left)
                groups.Add(comparison.GroupTitle, group)
                Me.listViewComparison.Groups.Add(group)
            End If

            Dim item As New ListViewItem({comparison.Name,
                                          If(comparison.LeftValue, CompareForm.MissingText),
                                          If(comparison.RightValue, CompareForm.MissingText)}, group) With {
                .BackColor = CompareForm.GetResultColor(comparison.Result)
            }
            Me.listViewComparison.Items.Add(item)
        Next comparison

        Me.listViewComparison.EndUpdate()

    End Sub

    ''' <summary>Gets the background color of a comparison result.</summary>
    Friend Shared Function GetResultColor(result As ComparisonResult) As Color

        Select Case result
            Case ComparisonResult.Equal
                Return CompareForm.EqualColor
            Case ComparisonResult.Different
                Return CompareForm.DifferentColor
            Case ComparisonResult.OnlyInLeft, ComparisonResult.OnlyInRight
                Return CompareForm.MissingColor
            Case Else
                Throw New System.ComponentModel.InvalidEnumArgumentException(NameOf(result), CInt(result), GetType(ComparisonResult))
        End Select
    End Function

#End Region

End Class

#End Region
