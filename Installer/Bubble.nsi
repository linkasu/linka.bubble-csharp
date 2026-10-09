Unicode true
!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "x64.nsh"
!cd ".."

Name "Линка.Пузырик"
OutFile "Linka.Bubble.Setup.exe"
InstallDir "$LOCALAPPDATA\Programs\LINKa\Bubble"
RequestExecutionLevel user
SetCompressor /SOLID lzma
VIProductVersion "0.1.0.0"
VIAddVersionKey "ProductName" "Линка.Пузырик"
VIAddVersionKey "FileDescription" "Установщик Линка.Пузырик"
VIAddVersionKey "FileVersion" "0.1.0.0"
VIAddVersionKey "LegalCopyright" "LINKa"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Russian"

Function .onInit
    ${IfNot} ${RunningX64}
        MessageBox MB_ICONSTOP "Для работы необходима 64-разрядная Windows 10 или 11."
        Abort
    ${EndIf}
    SetRegView 64
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
    IntCmp $0 528040 frameworkMissing frameworkReady frameworkReady
    frameworkMissing:
        MessageBox MB_ICONSTOP "Для работы необходим .NET Framework 4.8 или новее. Установите его и повторите запуск."
        Abort
    frameworkReady:
FunctionEnd

Section "Линка.Пузырик" Install
    SetOutPath "$INSTDIR"
    File "Bubble/bin/x64/Release/net48/Linka.Bubble.exe"
    File "Bubble/bin/x64/Release/net48/Linka.Bubble.exe.config"
    File "Bubble/bin/x64/Release/net48/Tobii.EyeX.Client.dll"
    File "Bubble/bin/x64/Release/net48/Tobii.Interaction.Model.dll"
    File "Bubble/bin/x64/Release/net48/Tobii.Interaction.Net.dll"
    File "README.md"
    File "TESTING.md"
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    CreateDirectory "$SMPROGRAMS\LINKa"
    CreateShortcut "$SMPROGRAMS\LINKa\Линка.Пузырик.lnk" "$INSTDIR\Linka.Bubble.exe"
    CreateShortcut "$SMPROGRAMS\LINKa\Удалить Линка.Пузырик.lnk" "$INSTDIR\Uninstall.exe"

    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "DisplayName" "Линка.Пузырик"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "InstallLocation" "$INSTDIR"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "DisplayVersion" "0.1.0"
    WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "NoModify" 1
    WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble" "NoRepair" 1
SectionEnd

Section "Uninstall"
    Delete "$SMPROGRAMS\LINKa\Линка.Пузырик.lnk"
    Delete "$SMPROGRAMS\LINKa\Удалить Линка.Пузырик.lnk"
    RMDir "$SMPROGRAMS\LINKa"

    Delete "$INSTDIR\Linka.Bubble.exe"
    Delete "$INSTDIR\Linka.Bubble.exe.config"
    Delete "$INSTDIR\Tobii.EyeX.Client.dll"
    Delete "$INSTDIR\Tobii.Interaction.Model.dll"
    Delete "$INSTDIR\Tobii.Interaction.Net.dll"
    Delete "$INSTDIR\README.md"
    Delete "$INSTDIR\TESTING.md"
    Delete "$INSTDIR\Uninstall.exe"
    RMDir "$INSTDIR"
    DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\LINKa.Bubble"
    ; Keep settings.json in the user's local application data when uninstalling.
SectionEnd
