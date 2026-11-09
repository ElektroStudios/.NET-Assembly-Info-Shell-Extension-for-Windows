
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

            Me.Path = IO.Path.Combine(FixtureFile.GetTempDirectory(), $"{Guid.NewGuid():N}{extension}")
            File.WriteAllBytes(Me.Path, content)
        End Sub

        ''' <summary>Initializes a new instance of the <see cref="FixtureFile"/> class, writing the content to the exact specified path.</summary>
        Private Sub New(path As String, content As Byte())
            Me.Path = path
            File.WriteAllBytes(Me.Path, content)
        End Sub

        ''' <summary>
        ''' Creates a temporary file with a specific file name, inside a new unique temporary directory
        ''' (so the name cannot collide with other tests).
        ''' </summary>
        ''' <param name="content">The file content.</param>
        ''' <param name="fileNameWithoutExtension">The file name, without extension.</param>
        ''' <param name="extension">The file extension, including the dot.</param>
        ''' <returns>
        ''' The temporary file.
        ''' </returns>
        Friend Shared Function CreateNamed(content As Byte(), fileNameWithoutExtension As String, extension As String) As FixtureFile
            Dim directory As String = IO.Path.Combine(FixtureFile.GetTempDirectory(), Guid.NewGuid().ToString("N"))
            IO.Directory.CreateDirectory(directory)
            Return New FixtureFile(IO.Path.Combine(directory, $"{fileNameWithoutExtension}{extension}"), content)
        End Function

        ''' <summary>
        ''' Creates a sidecar text file (UTF-8) at the exact specified path.
        ''' </summary>
        ''' <param name="path">The file path.</param>
        ''' <param name="text">The file content.</param>
        ''' <returns>
        ''' The temporary file.
        ''' </returns>
        Friend Shared Function CreateSidecar(path As String, text As String) As FixtureFile
            Return New FixtureFile(path, System.Text.Encoding.UTF8.GetBytes(text))
        End Function

        ''' <summary>Gets (and creates) the root temporary directory of the fixtures.</summary>
        Private Shared Function GetTempDirectory() As String
            Dim directory As String = IO.Path.Combine(IO.Path.GetTempPath(), "AssemblyInfo.Tests")
            IO.Directory.CreateDirectory(directory)
            Return directory
        End Function

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
