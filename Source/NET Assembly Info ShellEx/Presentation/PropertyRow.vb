
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " PropertyRow "

Namespace AssemblyInspection

    ''' <summary>
    ''' Represents a property name and its formatted value, as shown in a property page list view row.
    ''' </summary>
    <DebuggerDisplay("{Name,nq}: {Value,nq}")>
    Friend NotInheritable Class PropertyRow

#Region " Properties "

        ''' <summary>
        ''' Gets the property name.
        ''' </summary>
        Friend ReadOnly Property Name As String

        ''' <summary>
        ''' Gets the formatted property value.
        ''' </summary>
        Friend ReadOnly Property Value As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="PropertyRow"/> class.
        ''' </summary>
        ''' <param name="name">
        ''' The property name.
        ''' </param>
        '''
        ''' <param name="value">
        ''' The formatted property value.
        ''' </param>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="name"/> is <see langword="Nothing"/> or empty, or <paramref name="value"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Sub New(name As String, value As String)

            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentNullException(NameOf(name))
            End If
            If value Is Nothing Then
                Throw New ArgumentNullException(NameOf(value), $"The value of property '{name}' is null.")
            End If

            Me.Name = name
            Me.Value = value
        End Sub

#End Region

    End Class

End Namespace

#End Region
