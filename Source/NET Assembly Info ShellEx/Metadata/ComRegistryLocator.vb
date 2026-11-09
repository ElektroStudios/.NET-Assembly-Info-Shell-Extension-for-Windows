
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel

#End Region

#Region " ComRegistryKey "

Namespace AssemblyInspection

    ''' <summary>
    ''' Represents a registry key where a COM type library or class of an assembly is registered.
    ''' </summary>
    <DebuggerDisplay("{Description,nq}: {KeyPath,nq}")>
    Friend NotInheritable Class ComRegistryKey

        ''' <summary>Gets a description of the registered item (e.g. "CLSID of Namespace.MyClass").</summary>
        Friend ReadOnly Property Description As String

        ''' <summary>Gets the full key path (e.g. "HKEY_CLASSES_ROOT\CLSID\{...}").</summary>
        Friend ReadOnly Property KeyPath As String

        ''' <summary>Initializes a new instance of the <see cref="ComRegistryKey"/> class.</summary>
        Friend Sub New(description As String, keyPath As String)
            Me.Description = description
            Me.KeyPath = keyPath
        End Sub

    End Class

    ''' <summary>
    ''' Locates the registry keys where the COM type library (assembly GUID) and the COM classes
    ''' (types with an explicit GUID) of an assembly are registered.
    ''' </summary>
    Friend Module ComRegistryLocator

        ''' <summary>The prefix of the HKEY_CLASSES_ROOT key paths.</summary>
        Friend Const ClassesRoot As String = "HKEY_CLASSES_ROOT"

        ''' <summary>
        ''' Gets the registry keys of the COM type library and classes of the assembly that are registered.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        '''
        ''' <param name="classesRootKeyExists">
        ''' A function that determines whether a subkey of HKEY_CLASSES_ROOT exists (e.g. "CLSID\{...}").
        ''' </param>
        ''' <returns>
        ''' The registered keys: the type library first, then the classes in metadata order.
        ''' </returns>
        Friend Function GetRegisteredKeys(info As AssemblyInfo, classesRootKeyExists As Func(Of String, Boolean)) As ReadOnlyCollection(Of ComRegistryKey)

            If info Is Nothing Then
                Throw New ArgumentNullException(NameOf(info))
            End If
            If classesRootKeyExists Is Nothing Then
                Throw New ArgumentNullException(NameOf(classesRootKeyExists))
            End If

            Dim keys As New List(Of ComRegistryKey)

            If info.Guid.HasValue Then
                Dim subKey As String = $"TypeLib\{info.Guid.Value.ToString("B").ToUpperInvariant()}"
                If classesRootKeyExists(subKey) Then
                    keys.Add(New ComRegistryKey("Type library (assembly GUID)", $"{ComRegistryLocator.ClassesRoot}\{subKey}"))
                End If
            End If

            For Each comClass As KeyValuePair(Of String, Guid) In info.ComClasses
                Dim subKey As String = $"CLSID\{comClass.Value.ToString("B").ToUpperInvariant()}"
                If classesRootKeyExists(subKey) Then
                    keys.Add(New ComRegistryKey($"CLSID of {comClass.Key}", $"{ComRegistryLocator.ClassesRoot}\{subKey}"))
                End If
            Next comClass

            Return keys.AsReadOnly()
        End Function

        ''' <summary>
        ''' Builds the value of the Registry Editor "LastKey" setting that makes it open at the specified key.
        ''' </summary>
        ''' <param name="currentLastKey">
        ''' The current "LastKey" value, or <see langword="Nothing"/>. Its root node name is reused,
        ''' because that name is localized (e.g. "Computer", "Equipo", "Ordinateur").
        ''' </param>
        '''
        ''' <param name="keyPath">
        ''' The full key path (e.g. "HKEY_CLASSES_ROOT\CLSID\{...}").
        ''' </param>
        ''' <returns>
        ''' The new "LastKey" value.
        ''' </returns>
        Friend Function BuildRegeditLastKey(currentLastKey As String, keyPath As String) As String

            If String.IsNullOrEmpty(keyPath) Then
                Throw New ArgumentNullException(NameOf(keyPath))
            End If

            Const DefaultRootName As String = "Computer"
            If String.IsNullOrEmpty(currentLastKey) Then
                Return $"{DefaultRootName}\{keyPath}"
            End If

            If currentLastKey.StartsWith("HKEY_", StringComparison.OrdinalIgnoreCase) Then
                Return keyPath ' Older Regedit versions store the path without root node.
            End If

            Dim hiveIndex As Integer = currentLastKey.IndexOf("\HKEY_", StringComparison.OrdinalIgnoreCase)
            Dim rootName As String = If(hiveIndex > 0, currentLastKey.Substring(0, hiveIndex), currentLastKey.TrimEnd("\"c))
            Return $"{rootName}\{keyPath}"
        End Function

    End Module

End Namespace

#End Region
