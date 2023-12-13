@echo off


setlocal EnableDelayedExpansion
:: Project post-build event for Windows


set PROJECT_DIR=%~1
set TARGET_DIR=%~2
set TARGET_FILENAME=%~3

set ILMERGE="%PROJECT_DIR%\Tools\ILMerge.exe" 

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

for %%i in ("%TARGET_FILENAME%") do (
    set "TARGET_FILENAME_DLL=%%~nxi"
    set "TARGET_FILENAME_NO_EXT=%%~ni"
)

set "DLL_LIST=%TARGET_FILENAME_DLL%"

for %%A in ("%TARGET_DIR%\*.dll") do (
    if "%%~nxA" neq "%TARGET_FILENAME_DLL%" (
	    set "DLL_LIST=!DLL_LIST! %%~nxA"
    )
)

echo Merging DLLs into one: %DLL_LIST%
cd %TARGET_DIR% 
%ILMERGE% /lib:"%ProgramFiles(x86)%"\Loupedeck\Loupedeck2\ /out:..\%TARGET_FILENAME_DLL%  %DLL_LIST%
if errorlevel 1 goto :error

echo Deleting merged DLLs
del /Q *.dll *.pdb

echo Moving merged DLL to plugin output folder
move ..\"%TARGET_FILENAME_NO_EXT%.*" .\

echo Copying "%METADATA_DIR%" to "%TARGET_DIR%..\metadata\"
xcopy /s /y "%METADATA_DIR%" "%TARGET_DIR%..\metadata\"

goto :end
:error
echo %~0: Error: Failed to merge DLLs

:end
endlocal
