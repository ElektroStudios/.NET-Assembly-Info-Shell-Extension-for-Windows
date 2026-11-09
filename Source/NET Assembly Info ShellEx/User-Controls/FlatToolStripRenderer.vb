
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Drawing
Imports System.Windows.Forms

#End Region

#Region " FlatToolStripRenderer "

Namespace Usercontrols

    ''' <summary>
    ''' A <see cref="ToolStripProfessionalRenderer"/> that paints a flat, solid background with the
    ''' <see cref="ToolStrip.BackColor"/> of the tool strip, and no border.
    ''' </summary>
    ''' <remarks>
    ''' Inside a shell property sheet page, the system renderer (<see cref="ToolStripRenderMode.System"/>)
    ''' can leave the tool strip background unpainted, which is shown as a black area.
    ''' This renderer always fills the background, so the tool strip looks like the rest of the page.
    ''' </remarks>
    Friend NotInheritable Class FlatToolStripRenderer : Inherits ToolStripProfessionalRenderer

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FlatToolStripRenderer"/> class.
        ''' </summary>
        Friend Sub New()
            MyBase.New(New ProfessionalColorTable() With {.UseSystemColors = True})
            Me.RoundedEdges = False
        End Sub

        ''' <summary>
        ''' Paints the background of the tool strip with its <see cref="ToolStrip.BackColor"/>.
        ''' <para></para>
        ''' Drop-down menus keep the default professional rendering.
        ''' </summary>
        ''' <param name="e">
        ''' The event data.
        ''' </param>
        Protected Overrides Sub OnRenderToolStripBackground(e As ToolStripRenderEventArgs)

            If TypeOf e.ToolStrip Is ToolStripDropDown Then
                MyBase.OnRenderToolStripBackground(e)
                Return
            End If

            Using brush As New SolidBrush(e.ToolStrip.BackColor)
                e.Graphics.FillRectangle(brush, e.AffectedBounds)
            End Using
        End Sub

        ''' <summary>
        ''' Paints no border for the tool strip itself; drop-down menus keep their default border.
        ''' </summary>
        ''' <param name="e">
        ''' The event data.
        ''' </param>
        Protected Overrides Sub OnRenderToolStripBorder(e As ToolStripRenderEventArgs)

            If TypeOf e.ToolStrip Is ToolStripDropDown Then
                MyBase.OnRenderToolStripBorder(e)
            End If
        End Sub

    End Class

End Namespace

#End Region
