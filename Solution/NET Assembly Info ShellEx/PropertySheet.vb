
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices

Imports SharpShell
Imports SharpShell.Attributes
Imports SharpShell.SharpPropertySheet

#End Region

Namespace FileTimesPropertySheet

    ' <COMServerAssociation(AssociationType.ClassOfExtension, ".exe", ".dll")>
    ' <COMServerAssociation(AssociationType.FileExtension, ".exe", ".dll")>
    ' <COMServerAssociation(AssociationType.AllFiles)>

    ''' ----------------------------------------------------------------------------------------------------
    ''' <summary>
    ''' The .NET Assembly Info property sheet.
    ''' </summary>
    ''' ----------------------------------------------------------------------------------------------------
    <DisplayName(".NET Assembly Info")>
    <COMServerAssociation(AssociationType.ClassOfExtension, ".exe", ".dll")>
    <ComVisible(True)>
    Public NotInheritable Class PropertySheet : Inherits SharpPropertySheet

        ' Private ReadOnly validExtensions As String() = {".exe", ".dll"}

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Determines whether this instance can show a shell property sheet, given the specified selected file list.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' <see langword="True"/> if this instance should show a shell property sheet for the specified file list; 
        ''' otherwise, <see langword="False"/>.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        Protected Overrides Function CanShowSheet() As Boolean

            Dim selectedItemsCount As Integer = Me.SelectedItemPaths.Count()
            If (selectedItemsCount <> 1) Then
                Return False
            End If

            Dim filepath As String = Me.SelectedItemPaths.Single()
            'If Not Path.HasExtension(filepath) OrElse
            '   Not Me.validExtensions.Contains(Path.GetExtension(filepath).ToLower()) Then
            '    Return False
            'End If

            Try
                AssemblyName.GetAssemblyName(filepath)
                Return True

            Catch ex As Exception
                Return False

            End Try

        End Function

        ''' ----------------------------------------------------------------------------------------------------
        ''' <summary>
        ''' Creates the property sheet pages.
        ''' </summary>
        ''' ----------------------------------------------------------------------------------------------------
        ''' <returns>
        ''' The property sheet pages.
        ''' </returns>
        ''' ----------------------------------------------------------------------------------------------------
        Protected Overrides Function CreatePages() As IEnumerable(Of SharpPropertyPage)

            Dim page As New PropertyPage()
            Return {page}

        End Function

    End Class

End Namespace