
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " FixtureArgument "

Namespace Fixtures

    ''' <summary>
    ''' Specifies the type of a custom attribute argument emitted into a fixture assembly.
    ''' </summary>
    Friend Enum FixtureArgumentKind As Integer

        ''' <summary>A <see cref="String"/> argument.</summary>
        [String] = 0

        ''' <summary>A <see cref="Boolean"/> argument.</summary>
        [Boolean] = 1

        ''' <summary>An <see cref="Integer"/> argument.</summary>
        Int32 = 2

        ''' <summary>An Int32-based enum argument.</summary>
        [Enum] = 3

    End Enum

    ''' <summary>
    ''' Represents a custom attribute argument (fixed or named) emitted into a fixture assembly.
    ''' </summary>
    Friend NotInheritable Class FixtureArgument

#Region " Properties "

        ''' <summary>
        ''' Gets the argument type.
        ''' </summary>
        Friend ReadOnly Property Kind As FixtureArgumentKind

        ''' <summary>
        ''' Gets the argument value.
        ''' </summary>
        Friend ReadOnly Property Value As Object

        ''' <summary>
        ''' Gets the full name of the enum type (nested types use '+'), when <see cref="Kind"/> is <see cref="FixtureArgumentKind.Enum"/>.
        ''' </summary>
        Friend ReadOnly Property EnumTypeFullName As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FixtureArgument"/> class.
        ''' </summary>
        ''' <param name="kind">
        ''' The argument type.
        ''' </param>
        '''
        ''' <param name="value">
        ''' The argument value.
        ''' </param>
        '''
        ''' <param name="enumTypeFullName">
        ''' The full name of the enum type, for enum arguments.
        ''' </param>
        Private Sub New(kind As FixtureArgumentKind, value As Object, enumTypeFullName As String)
            Me.Kind = kind
            Me.Value = value
            Me.EnumTypeFullName = enumTypeFullName
        End Sub

#End Region

#Region " Factory Methods "

        ''' <summary>
        ''' Creates a <see cref="String"/> argument.
        ''' </summary>
        ''' <param name="value">
        ''' The value. It can be <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The argument.
        ''' </returns>
        Friend Shared Function FromString(value As String) As FixtureArgument
            Return New FixtureArgument(FixtureArgumentKind.String, value, Nothing)
        End Function

        ''' <summary>
        ''' Creates a <see cref="Boolean"/> argument.
        ''' </summary>
        ''' <param name="value">
        ''' The value.
        ''' </param>
        ''' <returns>
        ''' The argument.
        ''' </returns>
        Friend Shared Function FromBoolean(value As Boolean) As FixtureArgument
            Return New FixtureArgument(FixtureArgumentKind.Boolean, value, Nothing)
        End Function

        ''' <summary>
        ''' Creates an <see cref="Integer"/> argument.
        ''' </summary>
        ''' <param name="value">
        ''' The value.
        ''' </param>
        ''' <returns>
        ''' The argument.
        ''' </returns>
        Friend Shared Function FromInt32(value As Integer) As FixtureArgument
            Return New FixtureArgument(FixtureArgumentKind.Int32, value, Nothing)
        End Function

        ''' <summary>
        ''' Creates an Int32-based enum argument.
        ''' </summary>
        ''' <param name="enumTypeFullName">
        ''' The full name of the enum type (nested types use '+').
        ''' </param>
        '''
        ''' <param name="value">
        ''' The underlying enum value.
        ''' </param>
        ''' <returns>
        ''' The argument.
        ''' </returns>
        Friend Shared Function FromEnum(enumTypeFullName As String, value As Integer) As FixtureArgument
            If String.IsNullOrEmpty(enumTypeFullName) Then
                Throw New ArgumentNullException(NameOf(enumTypeFullName))
            End If
            Return New FixtureArgument(FixtureArgumentKind.Enum, value, enumTypeFullName)
        End Function

#End Region

    End Class

End Namespace

#End Region
