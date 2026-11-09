#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

Imports System.Resources
Imports System
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle(".NET Assembly Info Shell-Extension")>
<Assembly: AssemblyDescription("A shell-extension that shows info about .NET assemblies on the file properties")>
<Assembly: AssemblyCompany("ElektroStudios")>
<Assembly: AssemblyProduct(".NET Assembly Info Shell-Extension")>
<Assembly: AssemblyCopyright("Copyright © ElektroStudios 2018-2026")>
<Assembly: AssemblyTrademark("")>

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("68951AE5-7484-4EC1-120D-7DD1D4AA8832")>

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")>

<Assembly: AssemblyVersion("1.2.0.0")>
<Assembly: AssemblyFileVersion("1.2.0.0")>
<Assembly: NeutralResourcesLanguage("en-US")>

' Allows the unit test project to access the Friend (internal) members.
<Assembly: InternalsVisibleTo("AssemblyInfo.Tests")>
<Assembly: InternalsVisibleTo("AssemblyInfo.Preview")>
