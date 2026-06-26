#define MyAppName "SignalDeck"
#ifndef MyAppVersion
  #define MyAppVersion "0.2.0"
#endif
#ifndef PublishDir
  #error "PublishDir must be provided to the script."
#endif
#ifndef OutputDir
  #error "OutputDir must be provided to the script."
#endif
#ifndef IconPath
  #error "IconPath must be provided to the script."
#endif

[Setup]
AppId={{E3F1AAAF-7CF3-41DF-97D0-909E71990243}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=Personal Use
DefaultDirName=C:\Program Files\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=SignalDeckSetup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
SetupIconFile={#IconPath}
UninstallDisplayIcon={app}\SignalDeck.exe
UsePreviousAppDir=yes
UsePreviousTasks=yes
UsePreviousLanguage=yes
CloseApplications=yes
CloseApplicationsFilter=SignalDeck.exe
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
Name: "startup"; Description: "Launch SignalDeck automatically when I sign in (recommended)"; GroupDescription: "Startup options:"; Flags: checkedonce

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\SignalDeck.exe"; IconFilename: "{app}\SignalDeck.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\SignalDeck.exe"; Tasks: desktopicon; IconFilename: "{app}\SignalDeck.ico"

[Run]
Filename: "{app}\SignalDeck.exe"; Description: "Launch SignalDeck now"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "SignalDeck"; ValueData: """{app}\SignalDeck.exe"""; Tasks: startup; Flags: uninsdeletevalue
