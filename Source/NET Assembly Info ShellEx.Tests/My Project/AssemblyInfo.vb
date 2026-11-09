
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Reflection
Imports System.Resources
Imports System.Runtime.InteropServices

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

' NOTE: several tests read these attributes from this very assembly through the metadata reader,
'       and compare the results against the values returned by reflection.

<Assembly: AssemblyTitle(".NET Assembly Info Shell-Extension Tests")>
<Assembly: AssemblyDescription("Unit tests for the .NET Assembly Info Shell-Extension")>
<Assembly: AssemblyCompany("ElektroStudios")>
<Assembly: AssemblyProduct(".NET Assembly Info Shell-Extension")>
<Assembly: AssemblyCopyright("Copyright © ElektroStudios 2026")>
<Assembly: AssemblyTrademark("")>
<Assembly: AssemblyConfiguration("UnitTests")>

<Assembly: ComVisible(False)>
<Assembly: CLSCompliant(True)>
<Assembly: Guid("3F2C7D0E-5B7A-4C1E-9A61-2D8E4F1B6C35")>
<Assembly: NeutralResourcesLanguage("es-ES")>
<Assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 Or DllImportSearchPath.AssemblyDirectory)>

<Assembly: AssemblyVersion("1.2.0.0")>
<Assembly: AssemblyFileVersion("1.2.0.0")>

' Every test class works on its own temporary files, so classes can run in parallel.
<Assembly: Parallelize(Workers:=0, Scope:=ExecutionScope.ClassLevel)>
