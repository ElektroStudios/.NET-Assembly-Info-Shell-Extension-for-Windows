

#define Version "1.1"
#define AppName ".NET Assembly Info"

#define AuthorWebsite "https://github.com/ElektroStudios/.NET-Assembly-Info-ShellEx"

[Languages]
Name: en; MessagesFile: compiler:Default.isl
Name: es; MessagesFile: compiler:Languages\Spanish.isl

[LangOptions]
DialogFontName=Segoe UI
DialogFontSize=10
WelcomeFontName=Verdana
WelcomeFontSize=16
TitleFontName=Arial
TitleFontSize=29
CopyrightFontName=Arial
CopyrightFontSize=10

[Setup]
AppName={#AppName}
AppID={#AppName}
AppVerName={#AppName} {#Version}
SetupMutex={#AppName} {#Version}
AppVersion={#Version}
VersionInfoVersion={#Version}
AppCopyright=© ElektroStudios 2023
VersionInfoCopyright=© ElektroStudios 2023
AppPublisher=ElektroStudios
DefaultDirName={autopf}\ElektroStudios\{#AppName}
DefaultGroupName=
UninstallDisplayIcon={app}\uninstall.ico
OutputBaseFilename=NET Assembly Info
Compression=lzma/max
InternalCompressLevel=max
SolidCompression=false
AlwaysShowComponentsList=False
DisableWelcomePage=False
UserInfoPage=False
DisableDirPage=False
AllowRootDirectory=False
AllowNetworkDrive=False
AllowUNCPath=False
AppendDefaultDirName=True
AppendDefaultGroupName=False
UsePreviousAppDir=False
UsePreviousGroup=False
WizardImageStretch=True
TerminalServicesAware=True
MinVersion=0,6.1sp1
Password=
DisableProgramGroupPage=false
AllowNoIcons=True
DirExistsWarning=True
DisableReadyPage=False
AlwaysShowDirOnReadyPage=True
AlwaysShowGroupOnReadyPage=True
WizardStyle=Classic
SetupLogging=True
UninstallLogMode=New
ASLRCompatible=True
DEPCompatible=True
LZMAUseSeparateProcess=True
MissingMessagesWarning=True
MissingRunOnceIdsWarning=True
NotRecognizedMessagesWarning=True
UsedUserAreasWarning=True
CompressionThreads=Auto
CloseApplications=True
CloseApplicationsFilter=*.*
DefaultDialogFontName=Tahoma
DisableStartupPrompt=True
FlatComponentsList=False
LanguageDetectionMethod=UILanguage
PrivilegesRequired=PowerUser
RestartIfNeededByRun=True
ShowLanguageDialog=Auto
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
Source: {commoncf}\Inno Setup\*; DestDir: {commoncf}\Inno Setup; Attribs: readonly system; Flags: uninsneveruninstall overwritereadonly onlyifdoesntexist

; Temp files
Source: {tmp}\*; DestDir: {tmp}; Flags: recursesubdirs createallsubdirs ignoreversion deleteafterinstall

; Program
Source: {app}\*; DestDir: {app}; Flags: recursesubdirs createallsubdirs ignoreversion

[Run]
Filename: {app}\ServerRegistrationManager.exe; Parameters: "uninstall ""{app}\AssemblyInfo.dll"""; WorkingDir: {app}; StatusMsg: Unregistering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated
Filename: {app}\ServerRegistrationManager.exe; Parameters: "install ""{app}\AssemblyInfo.dll"" -codebase"; WorkingDir: {app}; StatusMsg: Registering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated

[UninstallRun]
Filename: {app}\ServerRegistrationManager.exe; Parameters: "uninstall ""{app}\AssemblyInfo.dll"""; WorkingDir: {app}; StatusMsg: Unregistering the shell-extension...; Flags: runhidden runascurrentuser waituntilterminated


[Code]

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Variables ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

Var
  // Custom Pages
  CustomSelectDirPage: TWizardPage;

  // Custom Controls
  DesktopShortcutCheckBox: TNewCheckBox;

  // Custom Vars
  UninstallSuccess : Boolean; // Determines whether the uninstallation succeeded.

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Function Imports ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// LoadVCLStyle for Unicode Inno Setup versions
procedure LoadVCLStyle(VClStyleFile: String)          ; external 'LoadVCLStyleW@files:VclStylesinno.dll stdcall setuponly';
procedure LoadVCLStyle_UnInstall(VClStyleFile: String); external 'LoadVCLStyleW@{app}\Uninstall.dll stdcall uninstallonly delayload';

// LoadVCLStyle for ANSI Inno Setup versions
// procedure LoadVCLStyle(VClStyleFile: String)          ; external 'LoadVCLStyleA@files:VclStylesinno.dll stdcall setuponly';
// procedure LoadVCLStyle_UnInstall(VClStyleFile: String); external 'LoadVCLStyleA@{app}\Uninstall.dll stdcall uninstallonly delayload';

// Unload VCL Style
procedure UnLoadVCLStyles          ; external 'UnLoadVCLStyles@files:VclStylesinno.dll stdcall setuponly';
procedure UnLoadVCLStyles_UnInstall; external 'UnLoadVCLStyles@{app}\Uninstall.dll stdcall uninstallonly delayload';

// CreateSymbolicLink for Unicode Inno Setup versions
function CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName: string; dwFlags: Integer): Boolean; external 'CreateSymbolicLinkW@kernel32.dll stdcall setuponly';

// CreateSymbolicLink for ANSI Inno Setup versions
//function CreateSymbolicLink(lpSymlinkFileName, lpTargetFileName: string; dwFlags: Integer): Boolean; external 'CreateSymbolicLinkA@kernel32.dll stdcall setuponly';

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Utility Functions ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determine whether the operating system is using light theme (instead of dark theme) for applications  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function AppsUseLightTheme(): Boolean;
var
  regValue: Cardinal;
begin
  RegQueryDWordValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize', 'AppsUseLightTheme', regValue);
  Result := (regValue <> 0);
end;


// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determine whether the operating system is using dark theme (instead of light theme) for applications  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function AppsUseDarkTheme(): Boolean;
begin
  Result := not AppsUseLightTheme();
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Adds a directory path to PATH environment variable  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure PathEnvAddDir(dirPath: String; forAllUsers: Boolean);
var
    PathValue: string;
    RootKey: Integer;
begin
    if forAllUsers then RootKey := HKEY_LOCAL_MACHINE else RootKey := HKEY_CURRENT_USER;

    // Retrieve current PATH value (use empty string if entry not exists).
    if not RegQueryStringValue(RootKey, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', PathValue) then
        PathValue := '';

    if PathValue = '' then begin
        PathValue := dirPath + ';';
    end else begin
        // Skip if string already found in path.
        if Pos(';' + Uppercase(dirPath) + ';',  ';' + Uppercase(PathValue) + ';') > 0 then exit;
        if Pos(';' + Uppercase(dirPath) + '\;', ';' + Uppercase(PathValue) + ';') > 0 then exit;

        // Append directory path to the end of the PATH environment variable.
        Log(Format('Right(PathValue, 1): [%s]', [PathValue[length(PathValue)]]));
        if PathValue[length(PathValue)] = ';' then begin
          // Don't double up ';' in env(PATH).
          PathValue := PathValue + dirPath + ';';
        end else begin
          PathValue := PathValue + ';' + dirPath + ';';
        end;
    end;

    // Overwrite (or create if missing) rhe PATH environment variable.
    if RegWriteStringValue(RootKey, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', PathValue) then begin
      Log(Format('The directory [%s] was added to PATH environment variable: [%s]', [dirPath, PathValue]));
    end else begin
      Log(Format('Error while adding the directory [%s] to PATH environment variable: [%s]', [dirPath, PathValue]));
    end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Removes a directory path from PATH environment variable //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure PathEnvDelDir(dirPath: String; forAllUsers: Boolean);
var
    PathValue: string;
    RootKey: Integer;
    P: Integer;
    Offset: Integer;
    DelimLen: Integer;
begin
    if forAllUsers then RootKey := HKEY_LOCAL_MACHINE else RootKey := HKEY_CURRENT_USER;

    // Skip if registry entry not exists.
    if not RegQueryStringValue(RootKey, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', PathValue) then exit;

    // Skip if string not found in path.
    DelimLen := 1; // Length(';')
    P := Pos(';' + Uppercase(dirPath) + ';', ';' + Uppercase(PathValue) + ';');
    if P = 0 then begin
        // perhaps dirPath lives in PathValue, but terminated by '\;'.
        DelimLen := 2; // Length('\;')
        P := Pos(';' + Uppercase(dirPath) + '\;', ';' + Uppercase(PathValue) + ';');
        if P = 0 then exit;
    end;

    // Decide where to start string subset in Delete() operation.
    if P = 1 then begin
        Offset := 0;
    end else begin
        Offset := 1;
    end;

    // Update PATH environment variable.
    Delete(PathValue, P - Offset, Length(dirPath) + DelimLen);

    // Overwrite (or create if missing) rhe PATH environment variable.
    if RegWriteStringValue(RootKey, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment', 'Path', PathValue) then begin
      Log(Format('The directory [%s] was removed from PATH environment variable: [%s]', [dirPath, PathValue]));
    end else begin
      Log(Format('Error while removing the directory [%s] from PATH environment variable: [%s]', [dirPath, PathValue]));
    end;
end;

// - - - - - - - - - - - - - - - - - - - - //
// Determines whether a directory is empty //
// - - - - - - - - - - - - - - - - - - - - //
function IsDirEmpty(dirName: String): Boolean;
var
  FindRec: TFindRec;
  FileCount: Integer;
begin
  Result := Not DirExists(dirName);
  if FindFirst(dirName+'\*', FindRec) then begin
    try
      repeat
        if (FindRec.Name <> '.') and (FindRec.Name <> '..') then begin
          FileCount := 1;
          break;
        end;
      until not FindNext(FindRec);
    except
      ShowExceptionMessage();
    finally
      FindClose(FindRec);
      if FileCount = 0 then Result := True;
    end;
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determine whether the source directory path is a root directory (eg. 'C:\') //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
Function IsRootDir(Path: String): Boolean;
var
  PathLength: LongInt;
begin
  StringChangeEx(Path, ':', '', True);
  Path := RemoveBackslash(Path);
  PathLength := Length(Path);
  Result := (PathLength <= 1)
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determine whether the source string ends with the specified characters      //
//                                                                             //
//https://stackoverflow.com/a/61522649                                         //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function EndsWith(SubText, Text: string): Boolean;
var
  EndStr: string;
begin
  EndStr := Copy(Text, Length(Text) - Length(SubText) + 1, Length(SubText));
  // Use SameStr function if you need a case-sensitive comparison.
  Result := SameText(SubText, EndStr);
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Splits a string by the specified delimiter character  //
//                                                       //
// https://stackoverflow.com/a/37916394/1248295          //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function StrSplit(Text: String; Separator: String): TArrayOfString;
var
  i, p: Integer;
  Dest: TArrayOfString;
begin
  i := 0;
  repeat
    SetArrayLength(Dest, i+1);
    p := Pos(Separator,Text);
    if p > 0 then begin
      Dest[i] := Copy(Text, 1, p-1);
      Text := Copy(Text, p + Length(Separator), Length(Text));
      i := i + 1;
    end else begin
      Dest[i] := Text;
      Text := '';
    end;
  until Length(Text)=0;
  Result := Dest
end;

// - - - - - - - - - - - - - - - - - - - - - -  //
// Converts a Boolean to String                 //
//                                              //
// https://stackoverflow.com/a/35044243/1248295 //
// - - - - - - - - - - - - - - - - - - - - - -  //
function BoolToStr(Value: Boolean): String;
begin
  if Value then
    Result := 'True'
  else
    Result := 'False';
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - //
// Converts a Yes/No or True/False string to Boolean //
// - - - - - - - - - - - - - - - - - - - - - - - - - //
function YesNoOrTrueFalseToBool(Value: String): Boolean;
begin
  if (LowerCase(Value) = 'true') or (LowerCase(Value) = 'yes') or (LowerCase(Value) = 'auto') then begin
    Result := True;

  end else if (LowerCase(Value) = 'false') or (LowerCase(Value) = 'no') then begin
    Result := False;

  end else begin
    RaiseException('Invalid string format for parameter Value ("' + Value + '") in "YesNoOrTrueFalseToBool" function.');
  end;
end;

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Custom Event-Handlers ------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Occurs when the 'AuthorWebsiteLabel' and 'AuthorWebsiteBitmap' controls are clicked //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure AuthorWebsiteControlClick(Sender: TObject);
var
  ErrorCode: Integer;
begin
  // This will open the specified url in the default web-browser.
  // https://stackoverflow.com/a/38934870/1248295
  ShellExec('open', '{#AuthorWebsite}', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -- //
// Occurs when the 'Next' button in the 'CustomSelectDirPage' page is pressed  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function CustomSelectDirPageNextButtonClick(Sender: TWizardPage): Boolean;
begin
  Result := True;

  // Ignore validations if installing in a temporary directory or in Windows directory.
  if (Pos('{tmp}', LowerCase(WizardDirValue)) <> 0) or (Pos('.tmp', LowerCase(WizardDirValue)) <> 0) then Exit;
  if (Pos('{win}', LowerCase(WizardDirValue)) <> 0) or (Pos(LowerCase(ExpandConstant('{win}')), LowerCase(WizardDirValue)) <> 0) then Exit;

  // Prevents the user from installing the program in a root directory if 'AllowRootDirectory' directive is set to 'False'.
  If not YesNoOrTrueFalseToBool('{#SetupSetting("AllowRootDirectory")}') and IsRootDir(WizardDirValue) then begin
    MsgBox('Setup does not allow {#AppName} to be installed on a root directory ("' + WizardDirValue + '").', mbCriticalError, MB_OK);
    Result := False;
    Exit;
  end;

  // Optionally, forces the user to install the program on a empty directory.
  // Result := IsDirEmpty(ExpandConstant(WizardDirValue));
  // If not Result then begin
  //   MsgBox('{#AppName} can only be installed to an empty directory.', mbCriticalError, MB_OK);
  //   Exit;
  // end;

  // Prevents the user from installing the program in a empty directory if 'DirExistsWarning' directive is set to 'True'.
  if (YesNoOrTrueFalseToBool('{#SetupSetting("DirExistsWarning")}')) and (DirExists(WizardDirValue))  then begin
    if MsgBox(FmtMessage(SetupMessage(msgDirExists), [WizardDirValue]), mbConfirmation, MB_YESNO) = IDNO then begin
      Result := false;
    end;
  end;

end;

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Custom UI Methods ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Deletes the VCL style files from {app} after uninstall  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure DeleteVclFiles();
begin
  try
    If FileExists(ExpandConstant('{app}\uninstall.vsf')) then begin
      DeleteFile(ExpandConstant('{app}\uninstall.vsf'));
    end;

    If FileExists(ExpandConstant('{app}\uninstall.dll')) then begin
      DeleteFile(ExpandConstant('{app}\uninstall.dll'));
    end;

    // Safe remove {app} directory. It does not remove the directory if it contains files or sub-dirrectories.
    If DirExists(ExpandConstant('{app}')) then begin
      DelTree(ExpandConstant('{app}\'), true, false, false);
    end;
  except
    Log(Format('Error calling DeleteVclFiles function: [%s]', [GetExceptionMessage()]));
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - //
// Creates the author website related controls //
// - - - - - - - - - - - - - - - - - - - - - - //
procedure CreateAuthorControls(AuthorWebsiteUrl: String);
var
  InstallerAuthorLabel: TNewStaticText;
  AuthorWebsiteLabel  : TNewStaticText;
  AuthorWebsiteBitmap : TBitmapImage;
  UseDarkTheme        : Boolean;

begin
  UseDarkTheme := AppsUseDarkTheme();

  // Set AuthorWebsiteBitmap control properties...
  AuthorWebsiteBitmap          := TBitmapImage.Create(WizardForm);
  AuthorWebsiteBitmap.Parent   := WizardForm.WelcomePage;
  AuthorWebsiteBitmap.AutoSize := True;
  AuthorWebsiteBitmap.Left     := (WizardForm.WizardBitmapImage.Left + WizardForm.WizardBitmapImage.Width) + ScaleX(10);
  AuthorWebsiteBitmap.Top      := (WizardForm.WelcomeLabel2.Top + WizardForm.WelcomeLabel2.Height) - (AuthorWebsiteBitmap.Height div 2) - ScaleX(16);
  AuthorWebsiteBitmap.Cursor   := crHand
  AuthorWebsiteBitmap.OnClick  := @AuthorWebsiteControlClick;
  AuthorWebsiteBitmap.Anchors  := [akLeft, akBottom];
  AuthorWebsiteBitmap.Visible  := (AuthorWebsiteUrl <> '');

  if FileExists(ExpandConstant('{tmp}\carbon.vsf')) then begin
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\AuthorWebsiteCarbon.bmp');
      AuthorWebsiteBitmap.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\AuthorWebsiteCarbon.bmp'));
  end else begin
    if UseDarkTheme then begin
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\AuthorWebsiteCarbon.bmp');
      AuthorWebsiteBitmap.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\AuthorWebsiteCarbon.bmp'));
    end else begin
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\AuthorWebsiteWhite.bmp');
      AuthorWebsiteBitmap.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\AuthorWebsiteWhite.bmp'));
    end;
  end;

  // Resize WelcomeLabel2 height to be able see AuthorWebsiteBitmap control.
  WizardForm.WelcomeLabel2.Height := WizardForm.WelcomeLabel2.Height - (AuthorWebsiteBitmap.Height + ScaleY(8));
  // This will not work...
  // AuthorWebsiteBitmap.BringToFront();

  // Set AuthorWebsiteLabel control properties...
  AuthorWebsiteLabel         := TNewStaticText.Create(WizardForm);
  AuthorWebsiteLabel.Parent  := WizardForm.WelcomePage;
  AuthorWebsiteLabel.Left    := AuthorWebsiteBitmap.Left;
  AuthorWebsiteLabel.Top     := AuthorWebsiteBitmap.Top - ScaleY(18);
  AuthorWebsiteLabel.Cursor  := crHand;
  AuthorWebsiteLabel.OnClick := @AuthorWebsiteControlClick;
  AuthorWebsiteLabel.Anchors := [akLeft, akBottom];
  AuthorWebsiteLabel.Visible := (AuthorWebsiteUrl <> '');

  // Set InstallerAuthorLabel control properties...
  InstallerAuthorLabel         := TNewStaticText.Create(WizardForm);
  InstallerAuthorLabel.Parent  := WizardForm;
  InstallerAuthorLabel.Left    := ScaleX(2);
  InstallerAuthorLabel.Top     := WizardForm.NextButton.Top + WizardForm.NextButton.Height div 2 + ScaleY(10) - ScaleY(2);
  InstallerAuthorLabel.Anchors := [akLeft, akBottom];

  if ActiveLanguage = 'es' then begin
    InstallerAuthorLabel.Caption := 'Instalador creado por ElektroStudios';
    AuthorWebsiteLabel.Caption := 'Haga clic aquí para abrir el sitio web del autor del programa.';
  end else begin
    InstallerAuthorLabel.Caption := 'Installer made by ElektroStudios';
    AuthorWebsiteLabel.Caption := 'Click here to open the website of the program author.';
  end;

end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Creates a control that serves as a password hint for the password textbox.  //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure CreatePasswordHintControl();
var
  PasswordHintLabel  : TNewStaticText;

begin
  // Exit if no password has been specified.
  if ('{SetupSetting("Password"))' = '') then exit;

  // Set PasswordHintLabel control properties...
  PasswordHintLabel         := TNewStaticText.Create(WizardForm);
  PasswordHintLabel.Parent  := WizardForm.PasswordPage;
  PasswordHintLabel.Left    := WizardForm.PasswordEdit.Left;
  PasswordHintLabel.Top     := WizardForm.PasswordEdit.Top + WizardForm.PasswordEdit.Height + ScaleY(10);
  PasswordHintLabel.Anchors := [akLeft, akTop];

  if ActiveLanguage = 'es' then begin
    PasswordHintLabel.Caption := 'Pista: La contraseña es el nombre, en minúsculas, de mi primera mascota felina.';
  end else begin
    PasswordHintLabel.Caption := 'Hint: The password is the name, in lower case, of my first feline pet.';
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Creates a custom wizard page to select program install folder, and start menu group folder. //
//                                                                                             //
// This page is a full replacement for both 'wpSelectDir' and 'wpSelectProgramGroup' pages.    //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure CreateCustomSelectDirPage(CrateShortCutCheckBox: Boolean; DisableDirControls: Boolean; DisableGroupControls: Boolean);
var
  WizardStyle: String; // Determines the wizard style, 'modern' or 'classic'.

  // Pages
  ThisPage: TWizardPage;
  SelectDirPage: TWizardPage;
  SelectProgramGroupPage: TWizardPage;

begin

  WizardStyle := LowerCase('{#SetupSetting("WizardStyle")}');

  // Pages
  SelectDirPage          := PageFromID(wpSelectDir);
  SelectProgramGroupPage := PageFromID(wpSelectProgramGroup);
  ThisPage               := CreateCustomPage(wpInfoBefore, SelectDirPage.Caption + ' | ' + SelectProgramGroupPage.Caption,
                                                           SelectDirPage.Description + #13#10 + SelectProgramGroupPage.Description);
  CustomSelectDirPage    := ThisPage;

  // wpSelectDir Control Parenting
  WizardForm.SelectDirBitmapImage.Parent := ThisPage.Surface;
  WizardForm.SelectDirLabel.Parent       := ThisPage.Surface;
  WizardForm.SelectDirBrowseLabel.Parent := ThisPage.Surface;
  WizardForm.DirEdit.Parent              := ThisPage.Surface;
  WizardForm.DirBrowseButton.Parent      := ThisPage.Surface;
  WizardForm.DiskSpaceLabel.Parent       := ThisPage.Surface;

  // wpSelectProgramGroup Control Parenting
  WizardForm.SelectGroupBitmapImage.Parent           := ThisPage.Surface;
  WizardForm.SelectStartMenuFolderLabel.Parent       := ThisPage.Surface;
  WizardForm.SelectStartMenuFolderBrowseLabel.Parent := ThisPage.Surface;
  WizardForm.GroupEdit.Parent                        := ThisPage.Surface;
  WizardForm.GroupBrowseButton.Parent                := ThisPage.Surface;
  WizardForm.NoIconsCheck.Parent                     := ThisPage.Surface;

  // Control Positioning
  if WizardStyle = 'modern' then begin
      WizardForm.SelectGroupBitmapImage.Top           := WizardForm.DirEdit.Top + ScaleX(70);
      WizardForm.SelectStartMenuFolderLabel.Top       := WizardForm.SelectGroupBitmapImage.Top + ScaleX(6);
      WizardForm.SelectStartMenuFolderBrowseLabel.Top := WizardForm.SelectStartMenuFolderLabel.Top + ScaleX(35);
      WizardForm.GroupEdit.Top                        := WizardForm.SelectStartMenuFolderBrowseLabel.Top + ScaleX(20);
      WizardForm.GroupBrowseButton.Top                := WizardForm.GroupEdit.Top;
      WizardForm.NoIconsCheck.Top                     := WizardForm.GroupEdit.Top - ScaleX(42);
  end else begin
      WizardForm.SelectDirBrowseLabel.Visible             := False;
      WizardForm.SelectStartMenuFolderBrowseLabel.Visible := False;
      WizardForm.DirEdit.Top                    := WizardForm.SelectDirLabel.Top + ScaleX(30);
      WizardForm.DirBrowseButton.Top            := WizardForm.DirEdit.Top;
      WizardForm.SelectGroupBitmapImage.Top     := WizardForm.DirEdit.Top + ScaleX(70);
      WizardForm.SelectStartMenuFolderLabel.Top := WizardForm.SelectGroupBitmapImage.Top + ScaleX(6);
      WizardForm.GroupEdit.Top                  := WizardForm.SelectStartMenuFolderLabel.Top + ScaleX(30);
      WizardForm.GroupBrowseButton.Top          := WizardForm.GroupEdit.Top;
      WizardForm.NoIconsCheck.Top               := WizardForm.GroupEdit.Top + ScaleX(28);
  end;

  // Load custom bitmap images
  if FileExists(ExpandConstant('{tmp}\carbon.vsf')) then begin
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\FolderCarbon.bmp');
      WizardForm.SelectDirBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\FolderCarbon.bmp'));
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\StartMenuCarbon.bmp');
      WizardForm.SelectGroupBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\StartMenuCarbon.bmp'));
  end else begin
    if AppsUseDarkTheme() then begin
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\FolderCarbon.bmp');
      WizardForm.SelectDirBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\FolderCarbon.bmp'));
      ExtractTemporaryFiles('{tmp}\WizardBitmaps\StartMenuCarbon.bmp');
      WizardForm.SelectGroupBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\StartMenuCarbon.bmp'));
    end else begin
      if WizardStyle = 'modern' then begin
        ExtractTemporaryFiles('{tmp}\WizardBitmaps\FolderWhiteModern.bmp');
        WizardForm.SelectDirBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\FolderWhiteModern.bmp'));
        ExtractTemporaryFiles('{tmp}\WizardBitmaps\StartMenuWhiteModern.bmp');
        WizardForm.SelectGroupBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\StartMenuWhiteModern.bmp'));
      end else begin
        ExtractTemporaryFiles('{tmp}\WizardBitmaps\FolderWhiteClassic.bmp');
        WizardForm.SelectDirBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\FolderWhiteClassic.bmp'));
        ExtractTemporaryFiles('{tmp}\WizardBitmaps\StartMenuWhiteClassic.bmp');
        WizardForm.SelectGroupBitmapImage.Bitmap.LoadFromFile(ExpandConstant('{tmp}\WizardBitmaps\StartMenuWhiteClassic.bmp'));
      end;
    end;
  end;

  // Disable 'DirEdit' and 'DirBrowseButton' controls if 'WizardGroupValue' contains a temp directory or Windows directory.
  WizardForm.DirEdit.Enabled := (Pos('{tmp}', LowerCase(WizardDirValue)) = 0) and (Pos('.tmp', LowerCase(WizardDirValue)) = 0) and (Pos('{win}', LowerCase(WizardDirValue)) = 0) and (Pos('\windows\', LowerCase(WizardDirValue)) = 0);
  WizardForm.DirBrowseButton.Enabled := (Pos('{tmp}', LowerCase(WizardDirValue)) = 0) and (Pos('.tmp', LowerCase(WizardDirValue)) = 0) and (Pos('{win}', LowerCase(WizardDirValue)) = 0) and (Pos('\windows\', LowerCase(WizardDirValue)) = 0);

  // Disable 'DirEdit' and 'DirBrowseButton' controls if specified in the function parameter.
  if DisableDirControls then begin
    WizardForm.DirEdit.Enabled := False;
    WizardForm.DirBrowseButton.Enabled := False;
  end;

  // Disable 'GroupEdit', 'GroupBrowseButton' and 'NoIconsCheck' controls if 'WizardGroupValue' is the default / not set.
  WizardForm.GroupEdit.Enabled := (Pos('(default)', LowerCase(WizardGroupValue)) = 0);
  WizardForm.GroupBrowseButton.Enabled := (Pos('(default)', LowerCase(WizardGroupValue)) = 0);
  WizardForm.NoIconsCheck.Enabled := (Pos('(default)', LowerCase(WizardGroupValue)) = 0);

  // Disable 'GroupEdit', 'GroupBrowseButton' and 'NoIconsCheck' controls if specified in the function parameter.
  if DisableGroupControls then begin
    WizardForm.GroupEdit.Enabled := False;
    WizardForm.GroupBrowseButton.Enabled := False;
    WizardForm.NoIconsCheck.Enabled := False;
  end;

  // 'Create a desktop shortcut' Control
  DesktopShortcutCheckBox         := TNewCheckBox.Create(ThisPage);
  if CrateShortCutCheckBox then begin
    DesktopShortcutCheckBox.Parent  := ThisPage.Surface;
    DesktopShortcutCheckBox.Top     := WizardForm.DirEdit.Top + ScaleX(30);
    DesktopShortcutCheckBox.Caption := CustomMessage('CreateDesktopIcon');
    DesktopShortcutCheckBox.Checked := True;
    DesktopShortcutCheckBox.Width   := WizardForm.NoIconsCheck.Width;
  end;

  // Set tab Order
  WizardForm.DirEdit.TabOrder           := 900;
  WizardForm.DirBrowseButton.TabOrder   := 901;
  DesktopShortcutCheckBox.TabOrder      := 902;
  WizardForm.GroupEdit.TabOrder         := 903;
  WizardForm.GroupBrowseButton.TabOrder := 904;
  WizardForm.NoIconsCheck.TabOrder      := 905;
  WizardForm.DiskSpaceLabel.TabOrder    := 906;

  // Set page event-handlers
  ThisPage.OnNextButtonClick := @CustomSelectDirPageNextButtonClick;

end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determines whether the user has choosed to create a desktop shortcut of the program executable. //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function IsCreateDesktopShortcutChecked(): Boolean;
begin
  Result := not (CustomSelectDirPage = nil) and DesktopShortcutCheckBox.Checked;
end;

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Installer Events ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - //
// Occurs when the installer initializes //
// - - - - - - - - - - - - - - - - - - - //
function InitializeSetup(): Boolean;
begin
  ExtractTemporaryFile('Carbon.vsf');

  if AppsUseDarkTheme then begin
    // Initialize the VCL skin style.
    LoadVCLStyle(ExpandConstant('{tmp}\Carbon.vsf'));
  end;

  Result := True;
end;

// - - - - - - - - - - - - - - - - - - - - //
// Occurs when the installer deinitializes //
// - - - - - - - - - - - - - - - - - - - - //
procedure DeinitializeSetup();
begin
  if AppsUseDarkTheme then begin
    // Deinitialize the VCL skin style.
    UnLoadVCLStyles();
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - //
// Occurs when the installer page is at PostInstall  //
// - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then begin
    if WizardIsTaskSelected('envPath') then PathEnvAddDir(ExpandConstant('{app}'), True);

    if YesNoOrTrueFalseToBool('{#SetupSetting("Uninstallable")}') then begin
      try
        CreateSymbolicLink(ExpandConstant('{app}\Uninstall.vsf'), ExpandConstant('{commoncf}\Inno Setup\Carbon.vsf'), 0)
        CreateSymbolicLink(ExpandConstant('{app}\Uninstall.dll'), ExpandConstant('{commoncf}\Inno Setup\VclStylesinno.dll'), 0)
      except
        Log(Format('Error calling CreateSymbolicLink in CurStepChanged function: [%s]', [GetExceptionMessage()]));
      end;
    end;
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Determine whether a wizard page should be skipped / not shown //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function ShouldSkipPage(PageID: Integer): Boolean;
begin
  // Force to skip the 'wpSelectDir' and 'wpSelectProgramGroup' pages if 'CustomSelectDirPage' is not nul.
  If ((PageID = wpSelectDir) or (PageID = wpSelectProgramGroup)) and (CustomSelectDirPage <> nil) then begin
    Result := True;
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// It is called automatically when the Ready to Install wizard page becomes the active page.           //
//                                                                                                     //
// It should return the text to be displayed in the settings memo on the Ready to Install wizard page. //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
function UpdateReadyMemo(
  Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  additionalText: string;
begin
  if Length(MemoUserInfoInfo) > 0 then begin
    Result := Result + MemoUserInfoInfo + NewLine + NewLine;
  end;

  // Force to skip the the MemoDirInfo if 'DefaultDirName' contains a temp directory.
  if (Length(MemoDirInfo) > 0) and (Pos('.tmp', LowerCase(WizardDirValue)) = 0) and (Pos('{tmp}', LowerCase(WizardDirValue)) = 0) then begin
    Result := Result + MemoDirInfo + NewLine + NewLine;
  end;

  if Length(MemoTypeInfo) > 0 then begin
    Result := Result + MemoTypeInfo + NewLine + NewLine;
  end;

  if Length(MemoComponentsInfo) > 0 then begin
    Result := Result + MemoComponentsInfo + NewLine + NewLine;
  end;

  // This is necessary when 'CreateCustomSelectDirPage' method is called.
  if not (WizardNoIcons) and (CustomSelectDirPage <> nil) and (Pos('(default)', LowerCase(WizardGroupValue)) = 0) then begin
    additionalText := WizardGroupValue;
    StringChange(additionalText, '&', '');
    if (Length(MemoGroupInfo) = 0) then begin
      Result := Result + SetupMessage(msgReadyMemoGroup) + NewLine + Space + additionalText + NewLine + NewLine;
    end else begin
      Result := Result + MemoGroupInfo + NewLine + NewLine;
    end;
  end else if (Length(MemoGroupInfo) > 0) and (Pos('(default)', LowerCase(WizardGroupValue)) = 0) then begin
      Result := Result + MemoGroupInfo + NewLine + NewLine;
  end;

  // This is necessary when 'CreateCustomSelectDirPage' method is called.
  if not (DesktopShortcutCheckBox = nil) and (DesktopShortcutCheckBox.Checked) then begin
    additionalText := DesktopShortcutCheckBox.Caption;
    StringChange(additionalText, '&', '');
    if (Length(MemoTasksInfo) = 0) then begin
      Result := Result + SetupMessage(msgReadyMemoTasks) + NewLine + Space + additionalText + NewLine + NewLine;
    end else begin
      Result := Result + MemoTasksInfo + NewLine + Space + additionalText + NewLine + NewLine;
    end;
  end else if (Length(MemoTasksInfo) > 0) then begin
      Result := Result + MemoTasksInfo + NewLine + NewLine;
  end;

end;

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// Uninstaller Events ---------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - - //
// Occurs when the uninstaller initializes //
// - - - - - - - - - - - - - - - - - - - - //
function InitializeUninstall: Boolean;
begin
  // Initialize the VCL skin style.
  if AppsUseDarkTheme then begin
    If FileExists(ExpandConstant('{app}\Uninstall.vsf')) then begin
      try
        LoadVCLStyle_UnInstall(ExpandConstant('{app}\Uninstall.vsf'));
      except
        Log(Format('Error calling LoadVCLStyle_UnInstall: [%s]', [GetExceptionMessage()]));
      end;
    end;
  end;
  Result := True;
end;

// - - - - - - - - - - - - - - - - - - - - - //
// Occurs when the uninstaller deinitializes //
// - - - - - - - - - - - - - - - - - - - - - //
procedure DeinitializeUninstall();
begin
  // Deinitialize the VCL skin style.
  if AppsUseDarkTheme then begin
    If FileExists(ExpandConstant('{app}\uninstall.dll')) then begin
      UnLoadVCLStyles_UnInstall;
      UnloadDll(ExpandConstant('{app}\uninstall.dll'));
    end;
  end;

  if UninstallSuccess then begin
    DeleteVclFiles();
  end;
end;

// - - - - - - - - - - - - - - - - - - - - - - - - - //
// Occurs when the uninstaller current page changes  //
// - - - - - - - - - - - - - - - - - - - - - - - - - //
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DeleteCustomFoldersEnabled: Boolean;
  FolderPath: String;
  FolderDescriptionEN: String;
  FolderDescriptionES: String;
  FoldersToDelete: array of string;
  Tokens: array of string;
  Index: Integer;
  DeleteFolderString: String;
  SelectedTasksHKLM: String;
  SelectedTasksHKCU: String;
begin
  case CurUninstallStep of
    usPostUninstall: begin
    end;

    usDone: begin
      UninstallSuccess:= True;
    end;

    usUninstall: begin
      // Remove the application directory from PATH environment variable.
      RegQueryStringValue(HKLM, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' + '{#SetupSetting("AppId")}' + '_is1', 'Inno Setup: Selected Tasks', SelectedTasksHKLM)
      RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' + '{#SetupSetting("AppId")}' + '_is1', 'Inno Setup: Selected Tasks', SelectedTasksHKCU)
      if (Pos('envpath', LowerCase(SelectedTasksHKLM)) <> 0) or (Pos('envpath', LowerCase(SelectedTasksHKCU)) <> 0) then PathEnvDelDir(ExpandConstant('{app}'), True);

      if not DeleteCustomFoldersEnabled then Exit; // Exits from this block if DeleteCustomFoldersEnabled = False

      // Set custom folders to delete (Path|Description)
      SetArrayLength(FoldersToDelete, 10);
      FoldersToDelete[0] := ExpandConstant('{userappdata}\MyProgramFolder') + '|current user settings|la configuración de usuario actual';
      FoldersToDelete[1] := ExpandConstant('{app}\tmp')                     + '|program temp files|los archivos temporales del programa';

      // Delete custom folders
      for Index := 0 to Length(foldersToDelete) - 1 do begin
        if foldersToDelete[Index] = '' then Continue; // Ignore empty array items.
        Tokens := StrSplit(FoldersToDelete[Index], '|');
        FolderPath := Tokens[0];
        FolderDescriptionEN := Tokens[1];
        FolderDescriptionES := Tokens[2];

        if DirExists(FolderPath) then begin
          if ActiveLanguage = 'es' then begin
            DeleteFolderString := '¿Quieres también borrar ' + FolderDescriptionES + '?';
          end else begin
            DeleteFolderString := 'Do you want also to delete ' + FolderDescriptionEN + '?';
          end;

          if MsgBox(DeleteFolderString, mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then begin
            DelTree(FolderPath, True, True, True);
          end;
        end; // DirExists
      end; // for
    end; // usUninstall
  end; // case CurUninstallStep
end;

// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// InitializeWizard Event ------------------------------------------------------------------------------------------------------------------------------------------------------------------ //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //
// ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
// Occurs when the wizard initializes                                                //
// Use this event function to make changes to the wizard or wizard pages at startup. //
// At the time this event is triggered, the wizard form does not yet exist.          //
// https://jrsoftware.org/ishelp/index.php?topic=scriptevents                        //
// - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - //
<event('InitializeWizard')>
procedure InitializeWizard1();
begin
  // No need to call CreateAuthorControls(), CreatePasswordHintControl() and CreateCustomSelectDirPage() if wizard has no GUI or it is the uninstaller.
  if WizardSilent or IsUninstaller then Exit;
  CreateAuthorControls(ExpandConstant('{#AuthorWebsite}'));
  CreatePasswordHintControl();
  CreateCustomSelectDirPage({CrateShortCutCheckBox} False, {DisableDirControls} False, {DisableGroupControls} False);
end;
