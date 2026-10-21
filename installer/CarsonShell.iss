#define MyAppName "CarsonShell"
#ifndef AppVersion
  #define AppVersion "0.1.0-dev"
#endif
#define MyAppPublisher "carjam120443-netizen"
#define MyAppExeName "CarsonShell.exe"

[Setup]
AppId={{7C3DF1C2-2A4E-4D9D-8D17-9B2DBBCDF117}
AppName={#MyAppName}
AppVersion={#AppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\CarsonShell
DefaultGroupName=CarsonShell
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=..\publish\installer
OutputBaseFilename=CarsonShell-Setup-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
CloseApplications= no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\CarsonShell.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\CarsonShell"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\CarsonShell"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch CarsonShell"; Flags: postinstall nowait skipifsilent
