; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects.
;
; See install-shared.nsi for a more robust way of checking for administrator rights.
; See install-per-user.nsi for a file association example.

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
;Interface Settings

	!define MUI_ABORTWARNING
  
;--------------------------------
;Pages

	!insertmacro MUI_PAGE_LICENSE "License.txt"
	!insertmacro MUI_PAGE_COMPONENTS
	!insertmacro MUI_PAGE_DIRECTORY
	!insertmacro MUI_PAGE_INSTFILES

	!insertmacro MUI_UNPAGE_CONFIRM
	!insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------

Section "Installer Section"

SectionEnd

; The stuff to install
Section "Mix It Up"

	; Set output path to the installation directory.
	SetOutPath "$INSTDIR"
  
	; Put file there
	File /r *.*
  
	; Write the installation path into the registry
	WriteRegStr HKLM SOFTWARE\MixItUp "Install_Dir" "$INSTDIR"
  
	; Write the uninstall keys for Windows
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "DisplayName" "Mix It Up"
	WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "UninstallString" '"$INSTDIR\Uninstall.exe"'
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "NoModify" 1
	WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp" "NoRepair" 1
	WriteUninstaller "$INSTDIR\Uninstall.exe"
  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

	CreateDirectory "$SMPROGRAMS\MixItUp"
	CreateShortcut "$SMPROGRAMS\MixItUp\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
	CreateShortcut "$SMPROGRAMS\MixItUp\MixItUp.lnk" "$INSTDIR\MixItUp.exe"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"
  
	; Remove registry keys
	DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MixItUp"
	DeleteRegKey /ifempty HKLM SOFTWARE\MixItUp

	; Remove files and uninstaller
	Delete $INSTDIR

	; Remove shortcuts, if any
	Delete "$SMPROGRAMS\MixItUp\*.lnk"

	; Remove directories
	RMDir "$SMPROGRAMS\MixItUp"
	RMDir "$INSTDIR"

SectionEnd
