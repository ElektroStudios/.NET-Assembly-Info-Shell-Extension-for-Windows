

#define Version "1.0"
#define AppName ".NET Assembly Info"


[Setup]
AppName={#AppName}
AppID={#AppName}
AppVerName={#AppName} {#Version}
SetupMutex={#AppName} {#Version}
AppVersion={#Version}
VersionInfoVersion={#Version}
AppCopyright=© ElektroStudios 2018
AppPublisher=ElektroStudios
DefaultDirName={pf}\ElektroStudios\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayIcon={app}\uninstall.ico
OutputBaseFilename=NET Assembly Info
Compression=lzma/ultra64
InternalCompressLevel=ultra64
SolidCompression=true
AlwaysShowComponentsList=False
DisableWelcomePage=False
DisableDirPage=True
DisableProgramGroupPage=True
DisableReadyPage=True
DisableStartupPrompt=True
FlatComponentsList=False
LanguageDetectionMethod=None
RestartIfNeededByRun=False
ShowLanguageDialog=NO
ShowTasksTreeLines=True
SetupIconFile=Icon.ico
WizardImageFile=embedded\WizardImage.bmp
WizardSmallImageFile=embedded\WizardSmallImage.bmp
InfoBeforeFile=embedded\InfoBefore.rtf
Uninstallable=True
ArchitecturesAllowed=x86 x64
ArchitecturesInstallIn64BitMode=x64

[Files]
; VCL Styles
Source: {cf}\Inno Setup\*; DestDir: {cf}\Inno Setup; Attribs: readonly system; Flags: uninsneveruninstall overwritereadonly onlyifdoesntexist

; Temp files
Source: {tmp}\*; DestDir: {tmp}; Flags: recursesubdirs createallsubdirs ignoreversion

; Program
Source: {app}\*; DestDir: {app}; Flags: recursesubdirs createallsubdirs ignoreversion

[Run]
Filename: {app}\ServerRegistrationManager.exe; Parameters: "uninstall ""{app}\AssemblyInfo.dll"""; WorkingDir: {app}; StatusMsg: Unregistering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated
Filename: {app}\ServerRegistrationManager.exe; Parameters: "install ""{app}\AssemblyInfo.dll"" -codebase"; WorkingDir: {app}; StatusMsg: Registering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated

[UninstallRun]
Filename: {app}\ServerRegistrationManager.exe; Parameters: "uninstall ""{app}\AssemblyInfo.dll"""; WorkingDir: {app}; StatusMsg: Unregistering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated

[Code]

// ************************
// Uninstallation Variables
// ************************

Var
  UninstallSuccess   : Boolean; // Determines whether the uninstallation succeeded.


// *******
// Imports
// *******

// Load VCL Style
procedure LoadVCLStyle(VClStyleFile: String); external 'LoadVCLStyleA@files:VclStylesinno.dll stdcall setuponly';
procedure LoadVCLStyle_UnInstall(VClStyleFile: String); external 'LoadVCLStyleA@{app}\Uninstall.dll stdcall uninstallonly delayload';

// Unload VCL Style
procedure UnLoadVCLStyles; external 'UnLoadVCLStyles@files:VclStylesinno.dll stdcall setuponly';
procedure UnLoadVCLStyles_UnInstall; external 'UnLoadVCLStyles@{app}\Uninstall.dll stdcall uninstallonly delayload';

// Create Symbolic Link
function CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName: string; dwFlags: Integer): Boolean; external 'CreateSymbolicLinkA@kernel32.dll stdcall setuponly';


// *******
// Methods
// *******

// Deletes the VCL style files from {app} after uninstall.
procedure DeleteVclFiles();
begin

  If FileExists(ExpandConstant('{app}\uninstall.vsf')) then begin
    DeleteFile(ExpandConstant('{app}\uninstall.vsf'));
  end;

  If FileExists(ExpandConstant('{app}\uninstall.dll')) then begin
    DeleteFile(ExpandConstant('{app}\uninstall.dll'));
  end;

  // Safe remove {app} dir. It does not remove the folder if it has files or subdirs.
  If DirExists(ExpandConstant('{app}')) then begin
    DelTree(ExpandConstant('{app}\'), true, false, false);
  end;

end;

// ****************
// Installer Events
// ****************

// E: Occurs when the installer initializes.
// -----------------------------------------
function InitializeSetup(): Boolean;
begin

    // Initialize the VCL skin style.
    ExtractTemporaryFile('Carbon.vsf');
    LoadVCLStyle(ExpandConstant('{tmp}\Carbon.vsf'));

    Result := True;

end;

// E: Occurs when the installer deinitializes.
// -------------------------------------------
procedure DeinitializeSetup();
begin

  // Deinitialize the VCL skin style.
  UnLoadVCLStyles;

end;

// E: Occurs when the installer page is at PostInstall.
// ----------------------------------------------------
procedure CurStepChanged(CurStep: TSetupStep);
begin

  if CurStep = ssPostInstall then begin
    CreateSymbolicLink(ExpandConstant('{app}\Uninstall.vsf'), ExpandConstant('{cf}\Inno Setup\Carbon.vsf'), 0)
    CreateSymbolicLink(ExpandConstant('{app}\Uninstall.dll'), ExpandConstant('{cf}\Inno Setup\VclStylesinno.dll'), 0)
  end;

end;

// ******************
// Uninstaller Events
// ******************

// E: Occurs when the uninstaller current page changes.
// ----------------------------------------------------
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin

  if CurUninstallStep = usDone then begin
    UninstallSuccess:= True;
  end;

end;


// E: Occurs when the uninstaller initializes.
// -------------------------------------------
function InitializeUninstall: Boolean;
begin

  Result := True;

  // Initialize the VCL skin style.
  If FileExists(ExpandConstant('{app}\Uninstall.vsf')) then begin
    LoadVCLStyle_UnInstall(ExpandConstant('{app}\Uninstall.vsf'));
  end;

end;

// E: Occurs when the uninstaller deinitializes.
// ---------------------------------------------
procedure DeinitializeUninstall();
begin

  // Deinitialize the VCL skin style.
  If FileExists(ExpandConstant('{app}\uninstall.dll')) then begin
    UnLoadVCLStyles_UnInstall;
    UnloadDll(ExpandConstant('{app}\uninstall.dll'));
  end;

  if UninstallSuccess then begin
    DeleteVclFiles;
  end;

end;
