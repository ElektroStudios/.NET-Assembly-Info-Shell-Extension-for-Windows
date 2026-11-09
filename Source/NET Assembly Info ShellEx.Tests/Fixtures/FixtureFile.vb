
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO

#End Region

#Region " FixtureFile "

Namespace Fixtures

    ''' <summary>
    ''' Represents a temporary fixture file that is deleted when disposed.
    ''' </summary>
    Friend NotInheritable Class FixtureFile : Implements IDisposable

#Region " Properties "

        ''' <summary>
        ''' Gets the full path of the temporary file.
        ''' </summary>
        Friend ReadOnly Property Path As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="FixtureFile"/> class, writing the specified content to a new temporary file.
        ''' </summary>
        ''' <param name="content">
        ''' The file content.
        ''' </param>
        '''
        ''' <param name="extension">
        ''' The file extension, including the dot.
        ''' </param>
        Friend Sub New(content As Byte(), extension As String)
            If content Is Nothing Then
                Throw New ArgumentNullException(NameOf(content))
            End If

            Dim directory As String = IO.Path.Combine(IO.Path.GetTempPath(), "AssemblyInfo.Tests")
            IO.Directory.CreateDirectory(directory)

            Me.Path = IO.Path.Combine(directory, $"{Guid.NewGuid():N}{extension}")
            File.WriteAllBytes(Me.Path, content)
        End Sub

#End Region

#Region " IDisposable Implementation "

        ''' <summary>
        ''' Deletes the temporary file.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If File.Exists(Me.Path) Then
                File.Delete(Me.Path)
            End If
        End Sub

#End Region

    End Class

End Namespace

#End Region
