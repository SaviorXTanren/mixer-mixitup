@echo off

:: Project post-build event for Windows

set PROJECT_DIR=%~1
set TARGET_DIR=%~2

if "%PROJECT_DIR%" == "" (
    echo %~0: Error: Project directory was not given
    exit /b 1
)

if "%TARGET_DIR%" == "" (
    echo %~0: Error: Target directory was not given
    exit /b 1
)

if not exist "%TARGET_DIR%" (
    echo %~0: Error: Target directory does not exist: '%TARGET_DIR%'"
    exit /b 1
)

set METADATA_DIR=%PROJECT_DIR%metadata\

if not exist "%METADATA_DIR%" (
    echo %~0: "Error: metadata directory does not exist: '%METADATA_DIR%'"
    exit /b 1
)

echo Copying "%METADATA_DIR%" to "%TARGET_DIR%..\metadata\"
xcopy /s /y "%METADATA_DIR%" "%TARGET_DIR%..\metadata\"
