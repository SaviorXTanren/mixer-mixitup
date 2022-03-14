;--------------------------------
;Include Modern UI

!include "MUI2.nsh"

;--------------------------------

; The name of the installer
Name "Mix It Up"

; The file to write
OutFile "MixItUp-Installer.exe"

; Request application privileges for Windows Vista and higher
RequestExecutionLevel admin

; Build Unicode installer
Unicode True

; The default installation directory
InstallDir $PROGRAMFILES\MixItUp

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\MixItUp" "Install_Dir"

;--------------------------------

; Pages

!insertmacro MUI_PAGE_LICENSE "License.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------

; The stuff to install
Section "Mix It Up (required)"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File /r *.*
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\MixItUp "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "DisplayName" "Mix It Up"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "DisplayIcon" '"$INSTDIR\MixItUp.exe,0"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "NoRepair" 1
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\MixItUp"
  CreateShortcut "$SMPROGRAMS\MixItUp\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  CreateShortcut "$SMPROGRAMS\MixItUp\Mix It Up.lnk" "$INSTDIR\MixItUp.exe"

SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp"
  DeleteRegKey HKLM SOFTWARE\MixItUp

  ; Remove files and uninstaller
  Delete $INSTDIR\*.*

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\MixItUp\*.lnk"

  ; Remove directories
  RMDir "$SMPROGRAMS\MixItUp"
  RMDir "$INSTDIR"

SectionEnd