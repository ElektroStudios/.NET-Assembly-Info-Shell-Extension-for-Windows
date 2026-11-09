
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

#Region " PropertyGroup "

Namespace AssemblyInspection

    ''' <summary>
    ''' Represents a titled group of property rows, as shown in a property page tab.
    ''' </summary>
    <DebuggerDisplay("{Title,nq} ({Rows.Count} rows)")>
    Friend NotInheritable Class PropertyGroup

#Region " Properties "

        ''' <summary>
        ''' Gets the group title.
        ''' </summary>
        Friend ReadOnly Property Title As String

        ''' <summary>
        ''' Gets the property rows, in display order.
        ''' </summary>
        Friend ReadOnly Property Rows As ReadOnlyCollection(Of PropertyRow)

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="PropertyGroup"/> class.
        ''' </summary>
        ''' <param name="title">
        ''' The group title.
        ''' </param>
        '''
        ''' <param name="rows">
        ''' The property rows, in display order.
        ''' </param>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="title"/> is <see langword="Nothing"/> or empty, or <paramref name="rows"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Sub New(title As String, rows As IEnumerable(Of PropertyRow))

            If String.IsNullOrEmpty(title) Then
                Throw New ArgumentNullException(NameOf(title))
            End If
            If rows Is Nothing Then
                Throw New ArgumentNullException(NameOf(rows))
            End If

            Me.Title = title
            Me.Rows = rows.ToList().AsReadOnly()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Gets the value of the row with the specified name.
        ''' </summary>
        ''' <param name="name">
        ''' The row name.
        ''' </param>
        ''' <returns>
        ''' The row value.
        ''' </returns>
        ''' <exception cref="KeyNotFoundException">
        ''' The group has no row with the specified name.
        ''' </exception>
        Friend Function GetValue(name As String) As String

            For Each row As PropertyRow In Me.Rows
                If String.Equals(row.Name, name, StringComparison.Ordinal) Then
                    Return row.Value
                End If
            Next row

            Throw New KeyNotFoundException($"The group '{Me.Title}' has no row named '{name}'.")
        End Function

        ''' <summary>
        ''' Determines whether the group has a row with the specified name.
        ''' </summary>
        ''' <param name="name">
        ''' The row name.
        ''' </param>
        ''' <returns>
        ''' <see langword="True"/> if the group has a row with the specified name; otherwise, <see langword="False"/>.
        ''' </returns>
        Friend Function ContainsRow(name As String) As Boolean

            Return Me.Rows.Any(Function(row As PropertyRow) String.Equals(row.Name, name, StringComparison.Ordinal))
        End Function

#End Region

    End Class

End Namespace

#End Region
