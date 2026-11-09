
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
        ''' Creates the property groups (Product, Build, Identity and References) of the specified assembly.
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
                AssemblyInfoPresenter.CreateReferencesGroup(info)
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
                New PropertyGroup(PropertyNames.GroupReferences, Enumerable.Empty(Of PropertyRow)())
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
        ''' Formats a referenced assembly (e.g. "4.0.0.0" or "4.0.0.0, es-ES").
        ''' </summary>
        ''' <param name="reference">
        ''' The referenced assembly name.
        ''' </param>
        ''' <returns>
        ''' The formatted reference.
        ''' </returns>
        Friend Shared Function FormatReference(reference As AssemblyName) As String

            Dim versionText As String = If(reference.Version Is Nothing, "0.0.0.0", reference.Version.ToString())
            Return If(String.IsNullOrEmpty(reference.CultureName), versionText, $"{versionText}, {reference.CultureName}")
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

            Dim comVisible As String = If(info.IsComVisible.HasValue,
                                          AssemblyInfoPresenter.FormatBoolean(info.IsComVisible.Value),
                                          $"{PropertyNames.YesValue} (default)")

            Dim clsCompliant As String = If(info.IsClsCompliant.HasValue,
                                            AssemblyInfoPresenter.FormatBoolean(info.IsClsCompliant.Value),
                                            PropertyNames.NotSpecifiedValue)

            Dim dllImportSearchPaths As String = If(info.DefaultDllImportSearchPaths.HasValue,
                                                    info.DefaultDllImportSearchPaths.Value.ToString(),
                                                    "(default)")

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
                New PropertyRow(PropertyNames.RootNamespace, AssemblyInfoPresenter.FormatText(info.RootNamespace)),
                New PropertyRow(PropertyNames.NeutralResourcesLanguage, AssemblyInfoPresenter.FormatCulture(info.NeutralResourcesLanguage, info.DefaultCulture)),
                New PropertyRow(PropertyNames.ClsCompliant, clsCompliant),
                New PropertyRow(PropertyNames.ComVisible, comVisible),
                New PropertyRow(PropertyNames.DpiAwarenessDisabled, AssemblyInfoPresenter.FormatBoolean(info.IsDpiAwarenessDisabled)),
                New PropertyRow(PropertyNames.DllImportSearchPaths, dllImportSearchPaths),
                New PropertyRow(PropertyNames.SecurityPermissions, AssemblyInfoPresenter.FormatText(info.SecurityPermission))
            }
            Return New PropertyGroup(PropertyNames.GroupBuild, rows)
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
