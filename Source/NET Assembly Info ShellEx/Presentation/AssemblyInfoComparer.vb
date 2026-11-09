
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

#Region " ComparisonResult "

Namespace AssemblyInspection

    ''' <summary>
    ''' Specifies the result of comparing a property of two assemblies.
    ''' </summary>
    Friend Enum ComparisonResult As Integer

        ''' <summary>Both assemblies have the property, with the same value.</summary>
        Equal = 0

        ''' <summary>Both assemblies have the property, with different values.</summary>
        Different = 1

        ''' <summary>Only the left (inspected) assembly has the property.</summary>
        OnlyInLeft = 2

        ''' <summary>Only the right (compared) assembly has the property.</summary>
        OnlyInRight = 3

    End Enum

#End Region

#Region " PropertyComparison "

    ''' <summary>
    ''' Represents the comparison of a property of two assemblies.
    ''' </summary>
    <DebuggerDisplay("{GroupTitle,nq}/{Name,nq}: {Result}")>
    Friend NotInheritable Class PropertyComparison

        ''' <summary>Gets the title of the group of the property.</summary>
        Friend ReadOnly Property GroupTitle As String

        ''' <summary>Gets the property name.</summary>
        Friend ReadOnly Property Name As String

        ''' <summary>Gets the value in the left assembly, or <see langword="Nothing"/> if it does not have the property.</summary>
        Friend ReadOnly Property LeftValue As String

        ''' <summary>Gets the value in the right assembly, or <see langword="Nothing"/> if it does not have the property.</summary>
        Friend ReadOnly Property RightValue As String

        ''' <summary>Gets the comparison result.</summary>
        Friend ReadOnly Property Result As ComparisonResult

        ''' <summary>Initializes a new instance of the <see cref="PropertyComparison"/> class.</summary>
        Friend Sub New(groupTitle As String, name As String, leftValue As String, rightValue As String)
            Me.GroupTitle = groupTitle
            Me.Name = name
            Me.LeftValue = leftValue
            Me.RightValue = rightValue

            If leftValue Is Nothing Then
                Me.Result = ComparisonResult.OnlyInRight
            ElseIf rightValue Is Nothing Then
                Me.Result = ComparisonResult.OnlyInLeft
            ElseIf String.Equals(leftValue, rightValue, StringComparison.Ordinal) Then
                Me.Result = ComparisonResult.Equal
            Else
                Me.Result = ComparisonResult.Different
            End If
        End Sub

    End Class

#End Region

#Region " AssemblyInfoComparer "

    ''' <summary>
    ''' Compares the property groups of two assemblies, row by row.
    ''' </summary>
    Friend NotInheritable Class AssemblyInfoComparer

        ''' <summary>Prevents a default instance of the <see cref="AssemblyInfoComparer"/> class from being created.</summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Compares the property groups of two assemblies.
        ''' <para></para>
        ''' Groups are matched by title, and rows by name and occurrence (rows such as "Internals Visible To"
        ''' can appear more than once). The left order is kept, followed by what only exists in the right side.
        ''' </summary>
        ''' <param name="left">The property groups of the inspected assembly.</param>
        ''' <param name="right">The property groups of the compared assembly.</param>
        ''' <returns>
        ''' The comparisons, in display order.
        ''' </returns>
        Friend Shared Function Compare(left As IEnumerable(Of PropertyGroup), right As IEnumerable(Of PropertyGroup)) As ReadOnlyCollection(Of PropertyComparison)

            If left Is Nothing Then
                Throw New ArgumentNullException(NameOf(left))
            End If
            If right Is Nothing Then
                Throw New ArgumentNullException(NameOf(right))
            End If

            Dim leftGroups As List(Of PropertyGroup) = left.ToList()
            Dim rightGroups As List(Of PropertyGroup) = right.ToList()
            Dim titles As List(Of String) = leftGroups.Select(Function(g As PropertyGroup) g.Title).
                                            Concat(rightGroups.Select(Function(g As PropertyGroup) g.Title)).
                                            Distinct(StringComparer.Ordinal).ToList()

            Dim comparisons As New List(Of PropertyComparison)
            For Each title As String In titles
                Dim leftRows As List(Of KeyValuePair(Of String, PropertyRow)) = AssemblyInfoComparer.GetKeyedRows(leftGroups, title)
                Dim rightRows As List(Of KeyValuePair(Of String, PropertyRow)) = AssemblyInfoComparer.GetKeyedRows(rightGroups, title)
                Dim rightLookup As Dictionary(Of String, PropertyRow) = rightRows.ToDictionary(
                    Function(pair As KeyValuePair(Of String, PropertyRow)) pair.Key,
                    Function(pair As KeyValuePair(Of String, PropertyRow)) pair.Value, StringComparer.Ordinal)

                Dim leftKeys As New HashSet(Of String)(StringComparer.Ordinal)
                For Each pair As KeyValuePair(Of String, PropertyRow) In leftRows
                    leftKeys.Add(pair.Key)
                    Dim rightRow As PropertyRow = Nothing
                    rightLookup.TryGetValue(pair.Key, rightRow)
                    comparisons.Add(New PropertyComparison(title, pair.Value.Name, pair.Value.Value, If(rightRow Is Nothing, Nothing, rightRow.Value)))
                Next pair

                For Each pair As KeyValuePair(Of String, PropertyRow) In rightRows
                    If Not leftKeys.Contains(pair.Key) Then
                        comparisons.Add(New PropertyComparison(title, pair.Value.Name, Nothing, pair.Value.Value))
                    End If
                Next pair
            Next title

            Return comparisons.AsReadOnly()
        End Function

        ''' <summary>
        ''' Gets the rows of the group with the specified title, keyed by name and occurrence index.
        ''' </summary>
        ''' <param name="groups">The property groups.</param>
        ''' <param name="title">The group title.</param>
        ''' <returns>
        ''' The keyed rows (empty if there is no such group), in display order.
        ''' </returns>
        Private Shared Function GetKeyedRows(groups As List(Of PropertyGroup), title As String) As List(Of KeyValuePair(Of String, PropertyRow))

            Dim keyed As New List(Of KeyValuePair(Of String, PropertyRow))
            Dim group As PropertyGroup = groups.FirstOrDefault(Function(g As PropertyGroup) String.Equals(g.Title, title, StringComparison.Ordinal))
            If group Is Nothing Then
                Return keyed
            End If

            Dim occurrences As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For Each row As PropertyRow In group.Rows
                Dim count As Integer = 0
                occurrences.TryGetValue(row.Name, count)
                occurrences(row.Name) = count + 1
                keyed.Add(New KeyValuePair(Of String, PropertyRow)($"{row.Name}#{count}", row))
            Next row
            Return keyed
        End Function

    End Class

End Namespace

#End Region
