# .NET Assembly Info Shell-Extension Change Log 📋

## v1.2 *(current)* 🆕
#### 🚀 New Features:
 -  New "References" tab, listing the referenced assemblies (with version and culture) 
      and the native DLLs imported through P/Invoke.
 -  New information fields:
      - Product tab: Comments, Trademarks and the real File Version (from the version resource).
      - Build tab: Target OS Platform (e.g. net8.0-windows), Code Optimizations, Deterministic Build, 
        Debug Symbols (PDB path, portable/Windows PDB, embedded PDB), managed Entry Point method, 
        Mixed-Mode (native code), ReadyToRun (precompiled) and Reference Assembly.
      - Identity tab: Assembly Name, Assembly Version, Culture, Full Name, 
        Strong-Name Signature (signed / delay-signed / not signed) and Module Version ID (MVID).
 -  .NET Core / .NET 5+ and .NET Standard assemblies now show every field, 
      including target framework (e.g. ".NET 8.0", ".NET Standard 2.0"), language, GUID and compilation timestamp.
 -  New "Attributes" tab: AssemblyMetadata key/value pairs (e.g. RepositoryUrl, RepositoryCommit, IsTrimmable),
      InternalsVisibleTo friend assemblies, key file, key container, delay sign, and classes with explicit GUID (COM classes).
      Neutral Resources Language, CLS Compliant, COM Visible, DPI Awareness Disabled, DLL Import Search Paths
      and Security Permissions were moved here from the Build tab.
 -  New "Runtime" tab: single-file bundle details, deployment model (framework-dependent / self-contained), trimmable flag,
      runtimeconfig.json settings (TFM, frameworks, roll forward, configuration properties), app.config settings
      (supported runtimes, binding redirects) and Win32 manifest settings (UAC execution level, UI access, DPI awareness,
      supported OS, Common Controls v6, long path awareness, active code page).
 -  Support for .NET single-file executables (PublishSingleFile, compressed or not): the property sheet is now shown 
      for them, with the information of the main assembly embedded in the bundle.
 -  New toolbar (with icons and tooltips) at the bottom of the property page:
      - Export: all the tabs, the current tab or the selected rows, in text, Markdown, HTML, JSON, XML or CSV format.
      - Copy All: copies all the tabs to the clipboard, as plain text or Markdown.
      - Compare...: compares the assembly with another one, row by row, in a separate window
        (green = equal, red = different, yellow = missing in one side), with a "Show differences only" option.
      - Win32 Manifest: shows the embedded manifest XML (with Copy and Save As options).
      - Configuration Files: shows the app.config, runtimeconfig.json and deps.json files (sidecar or embedded in the bundle).
      - Open in Registry Editor: opens the COM registration keys (type library and classes), only when they are registered.
      - Show in GAC: shows the assembly file in the Global Assembly Cache folder, only when it is installed in the GAC.
 -  New filter box at the top of the property page, that filters the rows of the current tab by name or value (ESC clears it).
 -  New context menu options: "Copy Value", "Copy Selected Rows" and "Export Selected Rows..." (multiple selection).
 -  Long values (hashes, paths) can be read through tooltips and by scrolling horizontally (auto-sized columns).
#### 🌟 Improvements:
 -  Assemblies are now read exclusively through their PE headers and ECMA-335 metadata 
      (System.Reflection.Metadata), instead of being loaded with Assembly.Load() into the Explorer process.
 -  Reorganized fields and clearer names:
      - "PE File Kind" is now "Output Type" (Class Library / Console Application / Windows Application).
      - "Platform" now distinguishes x86, x64, ARM64, ARM and Itanium, and "Any CPU ( Prefer 64-bit )" is now "Any CPU".
      - "Language" is now "Neutral Resources Language" (Attributes tab).
      - "Product Description" is now "File Description"; "Company Name" is now "Company".
      - "Entry-Point Address" (native stub RVA) was replaced by the managed "Entry Point" method name.
      - Strong-name, public key token and assembly GUID were moved to the Identity tab.
      - "Is ..." prefixes were removed (e.g. "Is COM Visible" is now "COM Visible").
      - Empty values are shown as "(none)" instead of "(null)".
 -  The compilation timestamp now includes the UTC offset.
 -  Security Permissions now show the requested permission flags (e.g. "RequestMinimum: SecurityPermission (UnmanagedCode)").
 -  References with version 0.0.0.0 (stored like that by some facades and shims, such as the netstandard.dll shim) 
      are now shown as "0.0.0.0 (any version)", and retargetable references are flagged as "retargetable".
 -  References to native DLLs without extension (e.g. "kernel32") are merged with their ".dll" counterparts.
 -  Files that cannot be read now show the error in the property page, instead of failing silently.
 -  The exported report is now saved as UTF-8 (it was saved with the ANSI code page) and includes the References tab.
 -  The detection of .NET assemblies (to show the property sheet) now reads only the PE/CLI headers.
 -  Added an MSTest unit test project (synthetic PE fixtures, no binary files in the repository).
 -  Added a small WinForms preview project (AssemblyInfo.Preview) that hosts the property page in a regular window,
      to design and debug it without registering the shell-extension (the UserControl Test Container cannot load its dependencies).
#### 🐛 Bug Fixes:
 -  Fixed a memory leak: every inspected assembly stayed loaded in Explorer until it was restarted.
 -  Fixed x86-only (32-bit required) assemblies failing to load in the 64-bit Explorer (BadImageFormatException).
 -  Fixed mixed-mode (C++/CLI) assemblies failing to load.
 -  Fixed ReadyToRun images (e.g. most .NET runtime assemblies) being reported as mixed-mode.
 -  Fixed "File Version" showing the assembly version instead of the file version.
 -  Fixed "GAC Path" always showing "(null)" for assemblies installed in the GAC.
 -  Fixed "Build Configuration" always showing "Release" (it relied on a flag that .NET compilers never set);
      it now uses AssemblyConfigurationAttribute or the DebuggableAttribute optimization flags.
 -  Fixed "COM Visible" showing "No" when the attribute is not specified (public types are COM-visible by default).
 -  Fixed "CLS Compliant" showing "No" when the attribute is not specified; it now shows "(not specified)".
 -  Fixed the compilation timestamp showing a meaningless date for deterministic builds (it is a content hash there).
 -  Fixed timestamps beyond the year 2038 being interpreted as negative values.
 -  Fixed the .NET Core detection heuristic (misspelled assembly names, and .NET Standard 1.x libraries detected as .NET Core).
 -  Fixed the root namespace detection returning only the first segment of dotted root namespaces (e.g. "Company" for "Company.Product").
 -  Fixed a crash when invalid culture names are declared in NeutralResourcesLanguageAttribute.
 -  Fixed a crash when using "Copy" from the context menu without a selected row.
 -  Fixed the COM object of the GAC query (IAssemblyCache) never being released.
 -  Fixed the System.Runtime.CompilerServices.Unsafe binding redirect version in app.config (6.0.3.0).

## v1.1 🔄
#### 🚀 New Features:
 -  Preliminary support for reading .NET Core assemblies.
#### 🌟 Improvements:
 -  Source-code upgraded to .NET Framework 4.8
 -  SharpShell updated to v2.7.2

## v1.0 🔄
Initial Release.
