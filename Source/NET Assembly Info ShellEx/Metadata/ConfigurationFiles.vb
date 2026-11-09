
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

#Region " ConfigurationFileKind "

''' <summary>
''' Specifies the kind of a configuration file associated to an assembly.
''' </summary>
Public Enum ConfigurationFileKind As Integer

    ''' <summary>A .NET Framework application configuration file (e.g. "MyApp.exe.config").</summary>
    AppConfig = 0

    ''' <summary>A .NET Core / .NET 5+ runtime configuration file (e.g. "MyApp.runtimeconfig.json").</summary>
    RuntimeConfig = 1

    ''' <summary>A .NET Core / .NET 5+ dependencies file (e.g. "MyApp.deps.json").</summary>
    DepsJson = 2

End Enum

#End Region

#Region " ConfigurationFile "

''' <summary>
''' Represents a configuration file associated to an assembly: a sidecar file next to it,
''' or a file embedded in its single-file bundle.
''' </summary>
<DebuggerDisplay("{FileName,nq} ({Kind})")>
Public NotInheritable Class ConfigurationFile

    ''' <summary>Gets the kind of the file.</summary>
    Public ReadOnly Property Kind As ConfigurationFileKind

    ''' <summary>Gets the file name.</summary>
    Public ReadOnly Property FileName As String

    ''' <summary>Gets a value indicating whether the file is embedded in a single-file bundle (otherwise, it is a sidecar file).</summary>
    Public ReadOnly Property IsEmbedded As Boolean

    ''' <summary>Gets the text content of the file.</summary>
    Public ReadOnly Property Content As String

    ''' <summary>
    ''' Initializes a new instance of the <see cref="ConfigurationFile"/> class.
    ''' </summary>
    ''' <param name="kind">The kind of the file.</param>
    ''' <param name="fileName">The file name.</param>
    ''' <param name="isEmbedded">A value indicating whether the file is embedded in a single-file bundle.</param>
    ''' <param name="content">The text content of the file.</param>
    Public Sub New(kind As ConfigurationFileKind, fileName As String, isEmbedded As Boolean, content As String)
        If String.IsNullOrEmpty(fileName) Then
            Throw New ArgumentNullException(NameOf(fileName))
        End If
        If content Is Nothing Then
            Throw New ArgumentNullException(NameOf(content))
        End If
        Me.Kind = kind
        Me.FileName = fileName
        Me.IsEmbedded = isEmbedded
        Me.Content = content
    End Sub

End Class

#End Region

#Region " BindingRedirectInfo "

''' <summary>
''' Represents an assembly binding redirect declared in an application configuration file.
''' </summary>
<DebuggerDisplay("{AssemblyName,nq}: {OldVersion,nq} -> {NewVersion,nq}")>
Public NotInheritable Class BindingRedirectInfo

    ''' <summary>Gets the name of the redirected assembly.</summary>
    Public ReadOnly Property AssemblyName As String

    ''' <summary>Gets the public key token of the redirected assembly, or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property PublicKeyToken As String

    ''' <summary>Gets the redirected version (or version range, e.g. "0.0.0.0-6.0.3.0").</summary>
    Public ReadOnly Property OldVersion As String

    ''' <summary>Gets the target version.</summary>
    Public ReadOnly Property NewVersion As String

    ''' <summary>Initializes a new instance of the <see cref="BindingRedirectInfo"/> class.</summary>
    Public Sub New(assemblyName As String, publicKeyToken As String, oldVersion As String, newVersion As String)
        Me.AssemblyName = assemblyName
        Me.PublicKeyToken = publicKeyToken
        Me.OldVersion = oldVersion
        Me.NewVersion = newVersion
    End Sub

End Class

#End Region

#Region " AppConfigInfo "

''' <summary>
''' Represents the relevant settings of a .NET Framework application configuration file (app.config).
''' </summary>
Public NotInheritable Class AppConfigInfo

    ''' <summary>Gets a value indicating whether the file is a well-formed XML document.</summary>
    Public ReadOnly Property IsValidXml As Boolean

    ''' <summary>Gets the supported runtimes (e.g. "v4.0 (.NETFramework,Version=v4.8)").</summary>
    Public ReadOnly Property SupportedRuntimes As ReadOnlyCollection(Of String)

    ''' <summary>Gets the assembly binding redirects.</summary>
    Public ReadOnly Property BindingRedirects As ReadOnlyCollection(Of BindingRedirectInfo)

    ''' <summary>
    ''' Initializes a new instance of the <see cref="AppConfigInfo"/> class.
    ''' </summary>
    ''' <param name="xml">
    ''' The XML text of the configuration file.
    ''' </param>
    Public Sub New(xml As String)

        If xml Is Nothing Then
            Throw New ArgumentNullException(NameOf(xml))
        End If

        Dim runtimes As New List(Of String)
        Dim redirects As New List(Of BindingRedirectInfo)
        Me.SupportedRuntimes = runtimes.AsReadOnly()
        Me.BindingRedirects = redirects.AsReadOnly()

        Dim document As XDocument
        Try
            document = TextFileDecoder.ParseXml(xml.Trim())

        Catch ex As XmlException
            Me.IsValidXml = False ' The invalid state is shown in the UI; the raw text is available in the viewer.
            Return

        End Try
        Me.IsValidXml = True

        For Each element As XElement In document.Descendants().Where(Function(e As XElement) e.Name.LocalName = "supportedRuntime")
            Dim version As String = AppConfigInfo.GetAttributeValue(element, "version")
            Dim sku As String = AppConfigInfo.GetAttributeValue(element, "sku")
            runtimes.Add(If(sku.Length = 0, version, $"{version} ({sku})"))
        Next element

        For Each dependent As XElement In document.Descendants().Where(Function(e As XElement) e.Name.LocalName = "dependentAssembly")
            Dim identity As XElement = dependent.Elements().FirstOrDefault(Function(e As XElement) e.Name.LocalName = "assemblyIdentity")
            Dim name As String = If(identity Is Nothing, String.Empty, AppConfigInfo.GetAttributeValue(identity, "name"))
            Dim token As String = If(identity Is Nothing, String.Empty, AppConfigInfo.GetAttributeValue(identity, "publicKeyToken"))

            For Each redirect As XElement In dependent.Elements().Where(Function(e As XElement) e.Name.LocalName = "bindingRedirect")
                redirects.Add(New BindingRedirectInfo(name, token,
                                                      AppConfigInfo.GetAttributeValue(redirect, "oldVersion"),
                                                      AppConfigInfo.GetAttributeValue(redirect, "newVersion")))
            Next redirect
        Next dependent
    End Sub

    ''' <summary>Gets the trimmed value of the attribute with the specified local name, or <see cref="String.Empty"/>.</summary>
    Private Shared Function GetAttributeValue(element As XElement, localName As String) As String
        Dim attribute As XAttribute = element.Attributes().FirstOrDefault(Function(a As XAttribute) a.Name.LocalName = localName)
        Return If(attribute Is Nothing, String.Empty, attribute.Value.Trim())
    End Function

End Class

#End Region

#Region " RuntimeConfigInfo "

''' <summary>
''' Represents the relevant settings of a .NET Core / .NET 5+ runtime configuration file (runtimeconfig.json).
''' </summary>
Public NotInheritable Class RuntimeConfigInfo

    ''' <summary>Gets a value indicating whether the file is a valid JSON document.</summary>
    Public ReadOnly Property IsValidJson As Boolean

    ''' <summary>Gets the target framework moniker (e.g. "net8.0"), or <see cref="String.Empty"/>.</summary>
    Public ReadOnly Property TargetFrameworkMoniker As String

    ''' <summary>Gets the frameworks (e.g. "Microsoft.NETCore.App 8.0.0"), shared or included.</summary>
    Public ReadOnly Property Frameworks As ReadOnlyCollection(Of String)

    ''' <summary>Gets a value indicating whether the application is self-contained (it declares <c>includedFrameworks</c>).</summary>
    Public ReadOnly Property IsSelfContained As Boolean

    ''' <summary>Gets the roll-forward policy, or <see cref="String.Empty"/> if not specified.</summary>
    Public ReadOnly Property RollForward As String

    ''' <summary>Gets the runtime configuration properties (e.g. "System.GC.Server" = "true").</summary>
    Public ReadOnly Property ConfigProperties As ReadOnlyCollection(Of KeyValuePair(Of String, String))

    ''' <summary>
    ''' Initializes a new instance of the <see cref="RuntimeConfigInfo"/> class.
    ''' </summary>
    ''' <param name="json">
    ''' The JSON text of the runtime configuration file.
    ''' </param>
    Public Sub New(json As String)

        If json Is Nothing Then
            Throw New ArgumentNullException(NameOf(json))
        End If

        Dim frameworkList As New List(Of String)
        Dim properties As New List(Of KeyValuePair(Of String, String))
        Me.Frameworks = frameworkList.AsReadOnly()
        Me.ConfigProperties = properties.AsReadOnly()
        Me.TargetFrameworkMoniker = String.Empty
        Me.RollForward = String.Empty

        Dim root As XElement
        Try
            root = JsonDocumentReader.Parse(json)

        Catch ex As XmlException
            Me.IsValidJson = False ' The invalid state is shown in the UI; the raw text is available in the viewer.
            Return

        End Try
        Me.IsValidJson = True

        Dim options As XElement = JsonDocumentReader.GetMember(root, "runtimeOptions")
        If options Is Nothing Then
            Return
        End If

        Me.TargetFrameworkMoniker = JsonDocumentReader.GetMemberValue(options, "tfm")
        Me.RollForward = JsonDocumentReader.GetMemberValue(options, "rollForward")

        ' .NET Core 2.x/3.x used a single "framework" object; .NET 5+ uses the "frameworks" array.
        Dim singleFramework As XElement = JsonDocumentReader.GetMember(options, "framework")
        If singleFramework IsNot Nothing Then
            frameworkList.Add(RuntimeConfigInfo.FormatFramework(singleFramework))
        End If
        RuntimeConfigInfo.AddFrameworks(JsonDocumentReader.GetMember(options, "frameworks"), frameworkList)

        Dim included As XElement = JsonDocumentReader.GetMember(options, "includedFrameworks")
        If included IsNot Nothing Then
            Me.IsSelfContained = True
            RuntimeConfigInfo.AddFrameworks(included, frameworkList)
        End If

        Dim configProperties As XElement = JsonDocumentReader.GetMember(options, "configProperties")
        If configProperties IsNot Nothing Then
            For Each element As XElement In configProperties.Elements()
                properties.Add(New KeyValuePair(Of String, String)(JsonDocumentReader.GetMemberName(element), element.Value))
            Next element
        End If
    End Sub

    ''' <summary>Adds the frameworks of a JSON array of framework references.</summary>
    Private Shared Sub AddFrameworks(array As XElement, frameworkList As List(Of String))
        If array Is Nothing Then
            Return
        End If
        For Each item As XElement In array.Elements()
            frameworkList.Add(RuntimeConfigInfo.FormatFramework(item))
        Next item
    End Sub

    ''' <summary>Formats a framework reference as "Name Version".</summary>
    Private Shared Function FormatFramework(framework As XElement) As String
        Return $"{JsonDocumentReader.GetMemberValue(framework, "name")} {JsonDocumentReader.GetMemberValue(framework, "version")}".Trim()
    End Function

End Class

#End Region
