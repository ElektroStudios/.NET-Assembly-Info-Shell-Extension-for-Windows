
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

Imports ShellExTests.Fixtures

#End Region

#Region " TestHelpers "

''' <summary>
''' Provides helper methods shared by the test classes.
''' </summary>
Friend Module TestHelpers

    ''' <summary>
    ''' Gets the file path of this test assembly.
    ''' </summary>
    Friend ReadOnly Property TestAssemblyPath As String
        Get
            Return GetType(TestHelpers).Assembly.Location
        End Get
    End Property

    ''' <summary>
    ''' Gets a value indicating whether the tests are running on .NET Framework.
    ''' </summary>
    Friend ReadOnly Property IsRunningOnNetFramework As Boolean
        Get
            Return RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.Ordinal)
        End Get
    End Property

    ''' <summary>
    ''' Emits a fixture assembly with the default options modified by <paramref name="configure"/>,
    ''' reads it with <see cref="AssemblyInfo"/>, and deletes the fixture file.
    ''' </summary>
    ''' <param name="configure">
    ''' An action that customizes the fixture options. It can be <see langword="Nothing"/>.
    ''' </param>
    ''' <returns>
    ''' The <see cref="AssemblyInfo"/> of the fixture.
    ''' </returns>
    Friend Function Inspect(configure As Action(Of FixtureOptions)) As AssemblyInfo

        Dim options As New FixtureOptions()
        configure?.Invoke(options)

        ' AssemblyInfo reads everything in its constructor, so the file can be deleted right away.
        Using fixture As FixtureFile = FixtureAssemblyBuilder.Create(options)
            Return New AssemblyInfo(fixture.Path)
        End Using
    End Function

End Module

#End Region
