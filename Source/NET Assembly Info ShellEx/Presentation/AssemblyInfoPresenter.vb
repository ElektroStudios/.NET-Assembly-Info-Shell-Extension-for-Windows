
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Globalization
Imports System.IO
Imports System.Reflection

#End Region

#Region " AssemblyInfoPresenter "

Namespace AssemblyInspection

    ''' <summary>
    ''' Converts an <see cref="AssemblyInfo"/> into the titled groups of formatted rows shown in the property page.
    ''' </summary>
    Friend NotInheritable Class AssemblyInfoPresenter

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AssemblyInfoPresenter"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates the property groups (Product, Build, Identity, Attributes, References and Runtime) of the specified assembly.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        '''
        ''' <param name="timeZone">
        ''' The time zone used to display the compilation time stamp.
        ''' </param>
        ''' <returns>
        ''' The property groups, in tab order.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="info"/> or <paramref name="timeZone"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Shared Function CreateGroups(info As AssemblyInfo, timeZone As TimeZoneInfo) As ReadOnlyCollection(Of PropertyGroup)

            If info Is Nothing Then
                Throw New ArgumentNullException(NameOf(info))
            End If
            If timeZone Is Nothing Then
                Throw New ArgumentNullException(NameOf(timeZone))
            End If

            Dim groups As PropertyGroup() = {
                AssemblyInfoPresenter.CreateProductGroup(info),
                AssemblyInfoPresenter.CreateBuildGroup(info, timeZone),
                AssemblyInfoPresenter.CreateIdentityGroup(info),
                AssemblyInfoPresenter.CreateAttributesGroup(info),
                AssemblyInfoPresenter.CreateReferencesGroup(info),
                AssemblyInfoPresenter.CreateRuntimeGroup(info)
            }
            Return Array.AsReadOnly(groups)
        End Function

        ''' <summary>
        ''' Creates the property groups that report an error that occurred while reading an assembly file.
        ''' </summary>
        ''' <param name="filePath">
        ''' The assembly file path.
        ''' </param>
        '''
        ''' <param name="exception">
        ''' The exception that was thrown while reading the file.
        ''' </param>
        ''' <returns>
        ''' The property groups, in tab order. Only the first group contains rows.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' <paramref name="filePath"/> or <paramref name="exception"/> is <see langword="Nothing"/>.
        ''' </exception>
        Friend Shared Function CreateErrorGroups(filePath As String, exception As Exception) As ReadOnlyCollection(Of PropertyGroup)

            If filePath Is Nothing Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If
            If exception Is Nothing Then
                Throw New ArgumentNullException(NameOf(exception))
            End If

            Dim errorRows As PropertyRow() = {
                New PropertyRow(PropertyNames.FileName, Path.GetFileName(filePath)),
                New PropertyRow(PropertyNames.ErrorRow, $"{exception.GetType().Name}: {exception.Message}")
            }

            Dim groups As PropertyGroup() = {
                New PropertyGroup(PropertyNames.GroupProduct, errorRows),
                New PropertyGroup(PropertyNames.GroupBuild, Enumerable.Empty(Of PropertyRow)()),
                New PropertyGroup(PropertyNames.GroupIdentity, Enumerable.Empty(Of PropertyRow)()),
                New PropertyGroup(PropertyNames.GroupAttributes, Enumerable.Empty(Of PropertyRow)()),
                New PropertyGroup(PropertyNames.GroupReferences, Enumerable.Empty(Of PropertyRow)()),
                New PropertyGroup(PropertyNames.GroupRuntime, Enumerable.Empty(Of PropertyRow)())
            }
            Return Array.AsReadOnly(groups)
        End Function

#End Region

#Region " Friend Methods (Formatting) "

        ''' <summary>
        ''' Formats a <see cref="Boolean"/> value as "Yes" or "No".
        ''' </summary>
        ''' <param name="value">
        ''' The value.
        ''' </param>
        ''' <returns>
        ''' "Yes" or "No".
        ''' </returns>
        Friend Shared Function FormatBoolean(value As Boolean) As String

            Return If(value, PropertyNames.YesValue, PropertyNames.NoValue)
        End Function

        ''' <summary>
        ''' Formats a text value, replacing null, empty or white-space values with "(none)".
        ''' </summary>
        ''' <param name="value">
        ''' The value. It can be <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The trimmed value, or "(none)".
        ''' </returns>
        Friend Shared Function FormatText(value As String) As String

            Return If(String.IsNullOrWhiteSpace(value), PropertyNames.NoneValue, value.Trim())
        End Function

        ''' <summary>
        ''' Formats the compilation time stamp in the specified time zone
        ''' (e.g. "05/November/2018 00:00:31 UTC+01:00").
        ''' </summary>
        ''' <param name="timeStamp">
        ''' The time stamp. It can be <see langword="Nothing"/>.
        ''' </param>
        '''
        ''' <param name="isDeterministic">
        ''' A value that determines whether the assembly was produced by a deterministic build.
        ''' </param>
        '''
        ''' <param name="timeZone">
        ''' The time zone.
        ''' </param>
        ''' <returns>
        ''' The formatted time stamp, "(not available: deterministic build)", or "(none)".
        ''' </returns>
        Friend Shared Function FormatTimeStamp(timeStamp As DateTimeOffset?, isDeterministic As Boolean, timeZone As TimeZoneInfo) As String

            If timeZone Is Nothing Then
                Throw New ArgumentNullException(NameOf(timeZone))
            End If

            If isDeterministic Then
                Return "(not available: deterministic build)"
            End If
            If Not timeStamp.HasValue Then
                Return PropertyNames.NoneValue
            End If

            Dim localTime As DateTimeOffset = TimeZoneInfo.ConvertTime(timeStamp.Value, timeZone)
            Return localTime.ToString("dd/MMMM/yyyy HH:mm:ss 'UTC'zzz", CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Formats the debug symbols (PDB) information.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The formatted debug symbols information, or "(none)".
        ''' </returns>
        Friend Shared Function FormatDebugSymbols(info As AssemblyInfo) As String

            Dim parts As New List(Of String)
            If info.HasEmbeddedPdb Then
                parts.Add("Embedded portable PDB")
            End If
            If Not String.IsNullOrEmpty(info.PdbPath) Then
                parts.Add($"{info.PdbPath} ({If(info.IsPortablePdb, "portable", "Windows PDB")})")
            End If

            Return If(parts.Count = 0, PropertyNames.NoneValue, String.Join("; ", parts))
        End Function

        ''' <summary>
        ''' Formats a culture as "English name (name)" (e.g. "English (United States) (en-US)").
        ''' </summary>
        ''' <param name="cultureName">
        ''' The raw culture name declared in the assembly.
        ''' </param>
        '''
        ''' <param name="culture">
        ''' The resolved culture, or <see langword="Nothing"/> if the name is not a known culture.
        ''' </param>
        ''' <returns>
        ''' The formatted culture, the raw name followed by "(unknown culture)", or "(none)".
        ''' </returns>
        Friend Shared Function FormatCulture(cultureName As String, culture As CultureInfo) As String

            If String.IsNullOrWhiteSpace(cultureName) Then
                Return PropertyNames.NoneValue
            End If
            If culture Is Nothing Then
                Return $"{cultureName} (unknown culture)"
            End If
            Return $"{culture.EnglishName} ({culture.Name})"
        End Function

        ''' <summary>
        ''' Formats the strong-name signature state.
        ''' </summary>
        ''' <param name="kind">
        ''' The strong-name signature state.
        ''' </param>
        ''' <returns>
        ''' "Signed", "Delay-signed (public key only)" or "Not signed".
        ''' </returns>
        Friend Shared Function FormatStrongNameSignature(kind As StrongNameSignatureKind) As String

            Select Case kind
                Case StrongNameSignatureKind.Signed
                    Return "Signed"
                Case StrongNameSignatureKind.DelaySigned
                    Return "Delay-signed (public key only)"
                Case StrongNameSignatureKind.NotSigned
                    Return "Not signed"
                Case Else
                    Throw New InvalidEnumArgumentException(NameOf(kind), CInt(kind), GetType(StrongNameSignatureKind))
            End Select
        End Function

        ''' <summary>
        ''' Formats a referenced assembly (e.g. "4.0.0.0", "4.0.0.0, es-ES", "0.0.0.0 (any version)" or "2.0.5.0, retargetable").
        ''' </summary>
        ''' <param name="reference">
        ''' The referenced assembly name.
        ''' </param>
        ''' <returns>
        ''' The formatted reference.
        ''' </returns>
        Friend Shared Function FormatReference(reference As AssemblyName) As String

            Dim parts As New List(Of String)
            Dim version As Version = If(reference.Version, New Version(0, 0, 0, 0))
            ' Version 0.0.0.0 is stored literally by some facades/shims: it matches any version at bind time.
            parts.Add(If(version = New Version(0, 0, 0, 0), $"{version}{PropertyNames.AnyVersionSuffix}", version.ToString()))
            If Not String.IsNullOrEmpty(reference.CultureName) Then
                parts.Add(reference.CultureName)
            End If
            If (reference.Flags And AssemblyNameFlags.Retargetable) = AssemblyNameFlags.Retargetable Then
                parts.Add("retargetable")
            End If
            Return String.Join(", ", parts)
        End Function

        ''' <summary>
        ''' Formats an optional <see cref="Boolean"/> value as "Yes", "No" or "(not specified)".
        ''' </summary>
        ''' <param name="value">
        ''' The value, or <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The formatted value.
        ''' </returns>
        Friend Shared Function FormatOptionalBoolean(value As Boolean?) As String

            Return If(value.HasValue, AssemblyInfoPresenter.FormatBoolean(value.Value), PropertyNames.NotSpecifiedValue)
        End Function

        ''' <summary>
        ''' Formats an optional setting, replacing null, empty or white-space values with "(not specified)".
        ''' </summary>
        ''' <param name="value">
        ''' The value. It can be <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The trimmed value, or "(not specified)".
        ''' </returns>
        Friend Shared Function FormatOptionalText(value As String) As String

            Return If(String.IsNullOrWhiteSpace(value), PropertyNames.NotSpecifiedValue, value.Trim())
        End Function

        ''' <summary>
        ''' Formats the single-file bundle information (e.g. "Yes (format v6.0, 171 files, compressed)").
        ''' </summary>
        ''' <param name="bundle">
        ''' The bundle, or <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The formatted bundle information, or "No".
        ''' </returns>
        Friend Shared Function FormatBundle(bundle As SingleFileBundle) As String

            If bundle Is Nothing Then
                Return PropertyNames.NoValue
            End If
            Dim compression As String = If(bundle.IsCompressed, ", compressed", String.Empty)
            Return $"{PropertyNames.YesValue} (format v{bundle.MajorVersion}.{bundle.MinorVersion}, {bundle.Entries.Count} files{compression})"
        End Function

        ''' <summary>
        ''' Formats the deployment model of the assembly.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' "Self-contained", "Framework-dependent" (with ", single-file" for bundles),
        ''' "Framework-dependent (.NET Framework)", or "(unknown)" when there is no runtimeconfig.json to tell.
        ''' </returns>
        Friend Shared Function FormatDeployment(info As AssemblyInfo) As String

            Dim singleFile As String = If(info.Bundle Is Nothing, String.Empty, ", single-file")
            If (info.RuntimeConfig IsNot Nothing) AndAlso info.RuntimeConfig.IsValidJson Then
                Return $"{If(info.RuntimeConfig.IsSelfContained, "Self-contained", "Framework-dependent")}{singleFile}"
            End If
            If info.FrameworkKind = AssemblyFrameworkKind.NetFramework Then
                Return "Framework-dependent (.NET Framework)"
            End If
            Return If(info.Bundle Is Nothing, PropertyNames.UnknownValue, "Single-file")
        End Function

        ''' <summary>
        ''' Formats a configuration file (e.g. "MyApp.runtimeconfig.json (sidecar file)").
        ''' </summary>
        ''' <param name="file">
        ''' The configuration file, or <see langword="Nothing"/>.
        ''' </param>
        '''
        ''' <param name="isValid">
        ''' A value indicating whether the file content could be parsed.
        ''' </param>
        ''' <returns>
        ''' The formatted configuration file, or "(none)".
        ''' </returns>
        Friend Shared Function FormatConfigurationFile(file As ConfigurationFile, isValid As Boolean) As String

            If file Is Nothing Then
                Return PropertyNames.NoneValue
            End If
            Dim location As String = If(file.IsEmbedded, "embedded in bundle", "sidecar file")
            Return If(isValid, $"{file.FileName} ({location})", $"{file.FileName} ({location}, invalid content)")
        End Function

        ''' <summary>
        ''' Formats the Win32 manifest presence (e.g. "Yes (resource ID 1)").
        ''' </summary>
        ''' <param name="manifest">
        ''' The manifest, or <see langword="Nothing"/>.
        ''' </param>
        ''' <returns>
        ''' The formatted manifest presence, or "(none)".
        ''' </returns>
        Friend Shared Function FormatManifest(manifest As Win32ManifestInfo) As String

            If manifest Is Nothing Then
                Return PropertyNames.NoneValue
            End If
            Dim validity As String = If(manifest.IsValidXml, String.Empty, ", invalid XML")
            Return $"{PropertyNames.YesValue} (resource ID {manifest.ResourceId}{validity})"
        End Function

        ''' <summary>
        ''' Formats the DPI awareness settings of a Win32 manifest (dpiAwareness takes precedence over dpiAware on Windows 10+).
        ''' </summary>
        ''' <param name="manifest">
        ''' The manifest.
        ''' </param>
        ''' <returns>
        ''' The formatted settings (e.g. "PerMonitorV2 (dpiAware: true/pm)"), or "(not specified)".
        ''' </returns>
        Friend Shared Function FormatDpiAwareness(manifest As Win32ManifestInfo) As String

            If (manifest.DpiAwareness.Length <> 0) AndAlso (manifest.DpiAware.Length <> 0) Then
                Return $"{manifest.DpiAwareness} (dpiAware: {manifest.DpiAware})"
            ElseIf manifest.DpiAwareness.Length <> 0 Then
                Return manifest.DpiAwareness
            ElseIf manifest.DpiAware.Length <> 0 Then
                Return $"dpiAware: {manifest.DpiAware}"
            End If
            Return PropertyNames.NotSpecifiedValue
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates the "Product" group, from the Win32 version resource of the file.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateProductGroup(info As AssemblyInfo) As PropertyGroup

            Dim version As FileVersionInfo = info.FileVersionInfo
            Dim rows As PropertyRow() = {
                New PropertyRow(PropertyNames.FileName, Path.GetFileName(info.FilePath)),
                New PropertyRow(PropertyNames.OriginalFileName, AssemblyInfoPresenter.FormatText(version.OriginalFilename)),
                New PropertyRow(PropertyNames.ProductName, AssemblyInfoPresenter.FormatText(version.ProductName)),
                New PropertyRow(PropertyNames.FileDescription, AssemblyInfoPresenter.FormatText(version.FileDescription)),
                New PropertyRow(PropertyNames.Comments, AssemblyInfoPresenter.FormatText(version.Comments)),
                New PropertyRow(PropertyNames.Company, AssemblyInfoPresenter.FormatText(version.CompanyName)),
                New PropertyRow(PropertyNames.Copyright, AssemblyInfoPresenter.FormatText(version.LegalCopyright)),
                New PropertyRow(PropertyNames.Trademarks, AssemblyInfoPresenter.FormatText(version.LegalTrademarks)),
                New PropertyRow(PropertyNames.ProductVersion, AssemblyInfoPresenter.FormatText(version.ProductVersion)),
                New PropertyRow(PropertyNames.FileVersion, AssemblyInfoPresenter.FormatText(version.FileVersion))
            }
            Return New PropertyGroup(PropertyNames.GroupProduct, rows)
        End Function

        ''' <summary>
        ''' Creates the "Build" group.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        '''
        ''' <param name="timeZone">
        ''' The time zone used to display the compilation time stamp.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateBuildGroup(info As AssemblyInfo, timeZone As TimeZoneInfo) As PropertyGroup

            Dim rows As PropertyRow() = {
                New PropertyRow(PropertyNames.OutputType, info.OutputType),
                New PropertyRow(PropertyNames.Platform, info.Platform),
                New PropertyRow(PropertyNames.TargetFramework, If(String.IsNullOrWhiteSpace(info.TargetFrameworkDisplayName), PropertyNames.UnknownValue, info.TargetFrameworkDisplayName)),
                New PropertyRow(PropertyNames.TargetOsPlatform, AssemblyInfoPresenter.FormatText(info.TargetPlatform)),
                New PropertyRow(PropertyNames.ClrVersion, AssemblyInfoPresenter.FormatText(info.ClrVersion)),
                New PropertyRow(PropertyNames.BuildConfiguration, info.BuildConfiguration),
                New PropertyRow(PropertyNames.CodeOptimizations, If(info.IsOptimized, "Enabled", "Disabled")),
                New PropertyRow(PropertyNames.DeterministicBuild, AssemblyInfoPresenter.FormatBoolean(info.IsDeterministic)),
                New PropertyRow(PropertyNames.CompilationTimeStamp, AssemblyInfoPresenter.FormatTimeStamp(info.TimeStamp, info.IsDeterministic, timeZone)),
                New PropertyRow(PropertyNames.DebugSymbols, AssemblyInfoPresenter.FormatDebugSymbols(info)),
                New PropertyRow(PropertyNames.EntryPoint, AssemblyInfoPresenter.FormatText(info.EntryPointMethod)),
                New PropertyRow(PropertyNames.MixedMode, AssemblyInfoPresenter.FormatBoolean(info.IsMixedMode)),
                New PropertyRow(PropertyNames.ReadyToRun, AssemblyInfoPresenter.FormatBoolean(info.IsReadyToRun)),
                New PropertyRow(PropertyNames.ReferenceAssembly, AssemblyInfoPresenter.FormatBoolean(info.IsReferenceAssembly)),
                New PropertyRow(PropertyNames.RootNamespace, AssemblyInfoPresenter.FormatText(info.RootNamespace))
            }
            Return New PropertyGroup(PropertyNames.GroupBuild, rows)
        End Function

        ''' <summary>
        ''' Creates the "Attributes" group: the assembly-level attributes, including the variable-length lists
        ''' (friend assemblies, classes with explicit GUID and AssemblyMetadataAttribute pairs).
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateAttributesGroup(info As AssemblyInfo) As PropertyGroup

            Dim comVisible As String = If(info.IsComVisible.HasValue,
                                          AssemblyInfoPresenter.FormatBoolean(info.IsComVisible.Value),
                                          $"{PropertyNames.YesValue} (default)")

            Dim dllImportSearchPaths As String = If(info.DefaultDllImportSearchPaths.HasValue,
                                                    info.DefaultDllImportSearchPaths.Value.ToString(),
                                                    "(default)")

            Dim rows As New List(Of PropertyRow) From {
                New PropertyRow(PropertyNames.NeutralResourcesLanguage, AssemblyInfoPresenter.FormatCulture(info.NeutralResourcesLanguage, info.DefaultCulture)),
                New PropertyRow(PropertyNames.ClsCompliant, AssemblyInfoPresenter.FormatOptionalBoolean(info.IsClsCompliant)),
                New PropertyRow(PropertyNames.ComVisible, comVisible),
                New PropertyRow(PropertyNames.DpiAwarenessDisabled, AssemblyInfoPresenter.FormatBoolean(info.IsDpiAwarenessDisabled)),
                New PropertyRow(PropertyNames.DllImportSearchPaths, dllImportSearchPaths),
                New PropertyRow(PropertyNames.SecurityPermissions, AssemblyInfoPresenter.FormatText(info.SecurityPermission)),
                New PropertyRow(PropertyNames.KeyFile, AssemblyInfoPresenter.FormatText(info.KeyFile)),
                New PropertyRow(PropertyNames.KeyContainer, AssemblyInfoPresenter.FormatText(info.KeyName)),
                New PropertyRow(PropertyNames.DelaySign, AssemblyInfoPresenter.FormatOptionalBoolean(info.DelaySign))
            }

            If info.InternalsVisibleTo.Count = 0 Then
                rows.Add(New PropertyRow(PropertyNames.InternalsVisibleTo, PropertyNames.NoneValue))
            Else
                For Each friendAssembly As String In info.InternalsVisibleTo
                    rows.Add(New PropertyRow(PropertyNames.InternalsVisibleTo, AssemblyInfoPresenter.FormatText(friendAssembly)))
                Next friendAssembly
            End If

            For Each comClass As KeyValuePair(Of String, Guid) In info.ComClasses
                rows.Add(New PropertyRow($"{PropertyNames.ComClassPrefix}{comClass.Key}", comClass.Value.ToString("B").ToUpperInvariant()))
            Next comClass

            For Each pair As KeyValuePair(Of String, String) In info.AssemblyMetadata
                rows.Add(New PropertyRow($"{PropertyNames.MetadataPrefix}{pair.Key}", AssemblyInfoPresenter.FormatText(pair.Value)))
            Next pair

            Return New PropertyGroup(PropertyNames.GroupAttributes, rows)
        End Function

        ''' <summary>
        ''' Creates the "Runtime" group: single-file bundle, deployment model, configuration files and Win32 manifest.
        ''' <para></para>
        ''' The detail rows of each configuration file and of the manifest are only included when it exists and is valid.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateRuntimeGroup(info As AssemblyInfo) As PropertyGroup

            Dim rows As New List(Of PropertyRow) From {
                New PropertyRow(PropertyNames.SingleFileBundle, AssemblyInfoPresenter.FormatBundle(info.Bundle))
            }
            If info.Bundle IsNot Nothing Then
                rows.Add(New PropertyRow(PropertyNames.BundledMainAssembly, info.BundleMainAssembly.RelativePath))
                rows.Add(New PropertyRow(PropertyNames.BundleHostPlatform, info.BundleHostPlatform))
            End If

            rows.Add(New PropertyRow(PropertyNames.Deployment, AssemblyInfoPresenter.FormatDeployment(info)))
            rows.Add(New PropertyRow(PropertyNames.Trimmable, AssemblyInfoPresenter.FormatOptionalBoolean(info.IsTrimmable)))

            ' runtimeconfig.json
            Dim runtimeConfigFile As ConfigurationFile = info.GetConfigurationFile(ConfigurationFileKind.RuntimeConfig)
            rows.Add(New PropertyRow(PropertyNames.RuntimeConfig, AssemblyInfoPresenter.FormatConfigurationFile(runtimeConfigFile, info.RuntimeConfig IsNot Nothing AndAlso info.RuntimeConfig.IsValidJson)))
            If (info.RuntimeConfig IsNot Nothing) AndAlso info.RuntimeConfig.IsValidJson Then
                Dim config As RuntimeConfigInfo = info.RuntimeConfig
                rows.Add(New PropertyRow(PropertyNames.RuntimeConfigTfm, AssemblyInfoPresenter.FormatText(config.TargetFrameworkMoniker)))
                rows.Add(New PropertyRow(PropertyNames.Frameworks, If(config.Frameworks.Count = 0, PropertyNames.NoneValue, String.Join("; ", config.Frameworks))))
                rows.Add(New PropertyRow(PropertyNames.RollForward, If(config.RollForward.Length = 0, "(default)", config.RollForward)))
                For Each pair As KeyValuePair(Of String, String) In config.ConfigProperties
                    rows.Add(New PropertyRow($"{PropertyNames.ConfigPropertyPrefix}{pair.Key}", pair.Value))
                Next pair
            End If

            ' app.config
            Dim appConfigFile As ConfigurationFile = info.GetConfigurationFile(ConfigurationFileKind.AppConfig)
            rows.Add(New PropertyRow(PropertyNames.AppConfig, AssemblyInfoPresenter.FormatConfigurationFile(appConfigFile, info.AppConfig IsNot Nothing AndAlso info.AppConfig.IsValidXml)))
            If (info.AppConfig IsNot Nothing) AndAlso info.AppConfig.IsValidXml Then
                rows.Add(New PropertyRow(PropertyNames.SupportedRuntimes, If(info.AppConfig.SupportedRuntimes.Count = 0, PropertyNames.NotSpecifiedValue, String.Join("; ", info.AppConfig.SupportedRuntimes))))
                For Each redirect As BindingRedirectInfo In info.AppConfig.BindingRedirects
                    rows.Add(New PropertyRow($"{PropertyNames.BindingRedirectPrefix}{redirect.AssemblyName}", $"{redirect.OldVersion} -> {redirect.NewVersion}"))
                Next redirect
            End If

            ' Win32 manifest
            Dim manifest As Win32ManifestInfo = info.Win32Manifest
            rows.Add(New PropertyRow(PropertyNames.Win32Manifest, AssemblyInfoPresenter.FormatManifest(manifest)))
            If (manifest IsNot Nothing) AndAlso manifest.IsValidXml Then
                rows.Add(New PropertyRow(PropertyNames.ExecutionLevel, AssemblyInfoPresenter.FormatOptionalText(manifest.RequestedExecutionLevel)))
                rows.Add(New PropertyRow(PropertyNames.UIAccess, AssemblyInfoPresenter.FormatOptionalText(manifest.UIAccess)))
                rows.Add(New PropertyRow(PropertyNames.DpiAwareness, AssemblyInfoPresenter.FormatDpiAwareness(manifest)))
                rows.Add(New PropertyRow(PropertyNames.SupportedOS, If(manifest.SupportedOperatingSystems.Count = 0, PropertyNames.NotSpecifiedValue, String.Join(", ", manifest.SupportedOperatingSystems))))
                rows.Add(New PropertyRow(PropertyNames.CommonControlsV6, AssemblyInfoPresenter.FormatBoolean(manifest.UsesCommonControlsV6)))
                rows.Add(New PropertyRow(PropertyNames.LongPathAware, AssemblyInfoPresenter.FormatOptionalText(manifest.LongPathAware)))
                rows.Add(New PropertyRow(PropertyNames.ActiveCodePage, AssemblyInfoPresenter.FormatOptionalText(manifest.ActiveCodePage)))
            End If

            Return New PropertyGroup(PropertyNames.GroupRuntime, rows)
        End Function

        ''' <summary>
        ''' Creates the "Identity" group.
        ''' <para></para>
        ''' The GAC rows are only included for .NET Framework assemblies, the only ones that can be installed in the GAC.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateIdentityGroup(info As AssemblyInfo) As PropertyGroup

            Dim rows As New List(Of PropertyRow) From {
                New PropertyRow(PropertyNames.AssemblyName, info.Name),
                New PropertyRow(PropertyNames.AssemblyVersion, info.Version.ToString()),
                New PropertyRow(PropertyNames.Culture, info.Culture),
                New PropertyRow(PropertyNames.FullName, info.FullName),
                New PropertyRow(PropertyNames.StrongNameSignature, AssemblyInfoPresenter.FormatStrongNameSignature(info.StrongNameSignature)),
                New PropertyRow(PropertyNames.PublicKeyToken, AssemblyInfoPresenter.FormatText(info.PublicKeyToken.ToUpperInvariant())),
                New PropertyRow(PropertyNames.AssemblyGuid, If(info.Guid.HasValue, info.Guid.Value.ToString("D").ToUpperInvariant(), PropertyNames.NoneValue)),
                New PropertyRow(PropertyNames.ModuleVersionId, info.ModuleVersionId.ToString("D").ToUpperInvariant())
            }

            If info.FrameworkKind = AssemblyFrameworkKind.NetFramework Then
                rows.Add(New PropertyRow(PropertyNames.InstalledInGac, AssemblyInfoPresenter.FormatBoolean(info.IsInstalledInGAC)))
                rows.Add(New PropertyRow(PropertyNames.GacName, AssemblyInfoPresenter.FormatText(info.GacFullName)))
                rows.Add(New PropertyRow(PropertyNames.GacPath, AssemblyInfoPresenter.FormatText(info.GacPath)))
            End If

            rows.Add(New PropertyRow(PropertyNames.MD5Hash, info.MD5Hash))
            rows.Add(New PropertyRow(PropertyNames.SHA1Hash, info.SHA1Hash))
            rows.Add(New PropertyRow(PropertyNames.SHA256Hash, info.SHA256Hash))

            Return New PropertyGroup(PropertyNames.GroupIdentity, rows)
        End Function

        ''' <summary>
        ''' Creates the "References" group: the referenced assemblies, followed by the native modules referenced by P/Invoke.
        ''' </summary>
        ''' <param name="info">
        ''' The assembly information.
        ''' </param>
        ''' <returns>
        ''' The property group.
        ''' </returns>
        Private Shared Function CreateReferencesGroup(info As AssemblyInfo) As PropertyGroup

            Dim rows As New List(Of PropertyRow)
            For Each reference As AssemblyName In info.ReferencedAssemblies
                rows.Add(New PropertyRow(reference.Name, AssemblyInfoPresenter.FormatReference(reference)))
            Next reference

            For Each moduleName As String In info.NativeModuleReferences
                rows.Add(New PropertyRow(moduleName, PropertyNames.NativeLibraryValue))
            Next moduleName

            If rows.Count = 0 Then
                rows.Add(New PropertyRow(PropertyNames.NoReferences, String.Empty))
            End If

            Return New PropertyGroup(PropertyNames.GroupReferences, rows)
        End Function

#End Region

    End Class

End Namespace

#End Region
