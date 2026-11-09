
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Text

#End Region

#Region " AssemblyInfoReport "

Namespace AssemblyInspection

    ''' <summary>
    ''' Builds the plain-text report exported by the property page.
    ''' </summary>
    Friend NotInheritable Class AssemblyInfoReport

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AssemblyInfoReport"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Builds the plain-text report of the specified property groups.
        ''' </summary>
        ''' <param name="filePath">
        ''' The assembly file path, written as the report header.
        ''' </param>
        '''
        ''' <param name="groups">
        ''' The property groups.
        ''' </param>
        ''' <returns>
        ''' The report text. Each group is written as an underlined title followed by its rows,
        ''' with the row names padded to the longest row name of the group.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="filePath"/> or <paramref name="groups"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Shared Function Build(filePath As String, groups As IEnumerable(Of PropertyGroup)) As String

            If filePath Is Nothing Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If
            If groups Is Nothing Then
                Throw New ArgumentNullException(NameOf(groups))
            End If

            Dim sb As New StringBuilder()
            sb.AppendLine(filePath)
            sb.AppendLine()

            For Each group As PropertyGroup In groups
                If group.Rows.Count = 0 Then
                    Continue For
                End If

                sb.AppendLine(group.Title)
                sb.AppendLine(New String("-"c, group.Title.Length))

                Dim nameWidth As Integer = group.Rows.Max(Function(row As PropertyRow) row.Name.Length)
                For Each row As PropertyRow In group.Rows
                    sb.AppendLine($"{row.Name.PadRight(nameWidth)}: {row.Value}".TrimEnd())
                Next row

                sb.AppendLine()
            Next group

            Return sb.ToString()
        End Function

#End Region

    End Class

End Namespace

#End Region
