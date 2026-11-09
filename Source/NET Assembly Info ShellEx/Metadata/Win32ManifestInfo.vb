
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Xml

Imports AssemblyInspection

#End Region

#Region " Win32ManifestInfo "

''' <summary>
''' Represents the Win32 application manifest (<c>RT_MANIFEST</c> resource) embedded in a PE image,
''' and the most relevant settings declared in it.
''' </summary>
Public NotInheritable Class Win32ManifestInfo

#Region " Fields "

    ''' <summary>The <c>supportedOS</c> GUIDs and the Windows versions they identify.</summary>
    Private Shared ReadOnly SupportedOsNames As New Dictionary(Of Guid, String) From {
        {New Guid("e2011457-1546-43c5-a5fe-008deee3d3f0"), "Windows Vista"},
        {New Guid("35138b9a-5d96-4fbd-8e2d-a2440225f93a"), "Windows 7"},
        {New Guid("4a2f28e3-53b9-4441-ba9c-d69d4a4a6e38"), "Windows 8"},
        {New Guid("1f676c76-80e1-4239-95bb-83d0f6d0da78"), "Windows 8.1"},
        {New Guid("8e0f7a12-bfb3-4fe8-b9a5-48fd50a15a9a"), "Windows 10/11"}
    }

#End Region

#Region " Properties "

    ''' <summary>Gets the integer ID of the manifest resource (1 for executables, 2 for DLLs, typically).</summary>
    Public ReadOnly Property ResourceId As Integer

    ''' <summary>Gets the manifest XML text.</summary>
    Public ReadOnly Property Xml As String

    ''' <summary>Gets a value indicating whether the manifest is a well-formed XML document.</summary>
    Public ReadOnly Property IsValidXml As Boolean

    ''' <summary>Gets the requested execution level (asInvoker, highestAvailable, requireAdministrator), or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property RequestedExecutionLevel As String

    ''' <summary>Gets the <c>uiAccess</c> value of the requested execution level, or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property UIAccess As String

    ''' <summary>Gets the <c>dpiAware</c> setting (e.g. "true", "true/pm"), or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property DpiAware As String

    ''' <summary>Gets the <c>dpiAwareness</c> setting (e.g. "PerMonitorV2, PerMonitor"), or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property DpiAwareness As String

    ''' <summary>Gets the Windows versions declared as supported (<c>supportedOS</c>); unknown GUIDs are kept as-is.</summary>
    Public ReadOnly Property SupportedOperatingSystems As ReadOnlyCollection(Of String)

    ''' <summary>Gets a value indicating whether the manifest depends on the Common Controls v6 (visual styles).</summary>
    Public ReadOnly Property UsesCommonControlsV6 As Boolean

    ''' <summary>Gets the <c>longPathAware</c> setting, or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property LongPathAware As String

    ''' <summary>Gets the <c>activeCodePage</c> setting (e.g. "UTF-8"), or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property ActiveCodePage As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Initializes a new instance of the <see cref="Win32ManifestInfo"/> class.
    ''' </summary>
    ''' <param name="resourceData">
    ''' The raw data of the manifest resource.
    ''' </param>
    '''
    ''' <param name="resourceId">
    ''' The integer ID of the manifest resource.
    ''' </param>
    Public Sub New(resourceData As Byte(), resourceId As Integer)

        If resourceData Is Nothing Then
            Throw New ArgumentNullException(NameOf(resourceData))
        End If

        Me.ResourceId = resourceId
        Me.Xml = TextFileDecoder.Decode(resourceData)
        Me.RequestedExecutionLevel = String.Empty
        Me.UIAccess = String.Empty
        Me.DpiAware = String.Empty
        Me.DpiAwareness = String.Empty
        Me.LongPathAware = String.Empty
        Me.ActiveCodePage = String.Empty
        Me.SupportedOperatingSystems = New List(Of String)().AsReadOnly()

        Dim document As XDocument
        Try
            document = TextFileDecoder.ParseXml(Me.Xml.Trim())

        Catch ex As XmlException
            Me.IsValidXml = False ' The raw text is still exposed through 'Xml', and the invalid state is shown in the UI.
            Return

        End Try
        Me.IsValidXml = True

        ' Element names are compared without namespace: manifests mix asm.v1, asm.v2, asm.v3 and the windowsSettings namespaces.
        Dim elements As List(Of XElement) = document.Descendants().ToList()

        Dim executionLevel As XElement = Win32ManifestInfo.FindElement(elements, "requestedExecutionLevel")
        If executionLevel IsNot Nothing Then
            Me.RequestedExecutionLevel = Win32ManifestInfo.GetAttributeValue(executionLevel, "level")
            Me.UIAccess = Win32ManifestInfo.GetAttributeValue(executionLevel, "uiAccess")
        End If

        Me.DpiAware = Win32ManifestInfo.GetElementValue(elements, "dpiAware")
        Me.DpiAwareness = Win32ManifestInfo.GetElementValue(elements, "dpiAwareness")
        Me.LongPathAware = Win32ManifestInfo.GetElementValue(elements, "longPathAware")
        Me.ActiveCodePage = Win32ManifestInfo.GetElementValue(elements, "activeCodePage")

        Dim systems As New List(Of String)
        For Each element As XElement In elements.Where(Function(e As XElement) e.Name.LocalName = "supportedOS")
            Dim id As String = Win32ManifestInfo.GetAttributeValue(element, "Id")
            Dim osGuid As Guid
            Dim osName As String = Nothing
            If Guid.TryParse(id, osGuid) AndAlso Win32ManifestInfo.SupportedOsNames.TryGetValue(osGuid, osName) Then
                systems.Add(osName)
            ElseIf id.Length <> 0 Then
                systems.Add(id)
            End If
        Next element
        Me.SupportedOperatingSystems = systems.AsReadOnly()

        Me.UsesCommonControlsV6 = elements.Any(
            Function(e As XElement) (e.Name.LocalName = "assemblyIdentity") AndAlso
                                    (e.Parent IsNot Nothing) AndAlso (e.Parent.Name.LocalName = "dependentAssembly") AndAlso
                                    String.Equals(Win32ManifestInfo.GetAttributeValue(e, "name"), "Microsoft.Windows.Common-Controls", StringComparison.OrdinalIgnoreCase) AndAlso
                                    Win32ManifestInfo.GetAttributeValue(e, "version").StartsWith("6.", StringComparison.Ordinal))
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>Finds the first element with the specified local name.</summary>
    Private Shared Function FindElement(elements As List(Of XElement), localName As String) As XElement
        Return elements.FirstOrDefault(Function(e As XElement) e.Name.LocalName = localName)
    End Function

    ''' <summary>Gets the trimmed value of the first element with the specified local name, or <see cref="String.Empty"/>.</summary>
    Private Shared Function GetElementValue(elements As List(Of XElement), localName As String) As String
        Dim element As XElement = Win32ManifestInfo.FindElement(elements, localName)
        Return If(element Is Nothing, String.Empty, element.Value.Trim())
    End Function

    ''' <summary>Gets the trimmed value of the attribute with the specified local name, or <see cref="String.Empty"/>.</summary>
    Private Shared Function GetAttributeValue(element As XElement, localName As String) As String
        Dim attribute As XAttribute = element.Attributes().FirstOrDefault(Function(a As XAttribute) a.Name.LocalName = localName)
        Return If(attribute Is Nothing, String.Empty, attribute.Value.Trim())
    End Function

#End Region

End Class

#End Region
