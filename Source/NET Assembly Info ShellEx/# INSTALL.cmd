@Echo Off
Title Shell Extension Deployment - Installer
Color 07

Echo ==========================================
Echo   Shell Extension Deployment - Installer
Echo ==========================================
Echo.

:: 1. Check for Administrator privileges
net session >nul 2>&1
If %ERRORLEVEL% NEQ 0 (
    Color 0C
    Echo [ERROR] Access Denied.
    Echo Please right-click this script and select "Run as administrator".
    Echo.
    Pause
    Exit /B 1
)

:: 2. Verify dependencies exist in the current directory
If Not Exist "%~dp0ServerRegistrationManager.exe" (
    Color 0C
    Echo [ERROR] Missing dependency: ServerRegistrationManager.exe
    Echo It must be located in the same directory as this script.
    Echo.
    Pause
    Exit /B 1
)

If Not Exist "%~dp0AssemblyInfo.dll" (
    Color 0C
    Echo [ERROR] Missing file: AssemblyInfo.dll
    Echo Cannot proceed without the target assembly.
    Echo.
    Pause
    Exit /B 1
)

:: 3. Execution
Echo [INFO] Registering assembly with codebase...
"%~dp0ServerRegistrationManager.exe" install "%~dp0AssemblyInfo.dll" -codebase

:: 4. Status Evaluation
If %ERRORLEVEL% NEQ 0 (
    Color 0C
    Echo.
    Echo [FAIL] Registration process failed with exit code: %ERRORLEVEL%
) Else (
    Color 0A
    Echo.
    Echo [OK] Shell extension successfully installed!
)

Echo.
Pause
Exit /B 0