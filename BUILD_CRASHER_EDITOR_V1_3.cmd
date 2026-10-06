@echo off
setlocal
cd /d "%~dp0"

title Crasher Editor V1.3 - Build

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"

if not exist "%CSC%" (
  echo.
  echo ERROR: Windows .NET Framework C# compiler was not found.
  echo Enable/install .NET Framework 4.x, then run this file again.
  echo.
  if defined CI exit /b 1
  pause
  exit /b 1
)

echo.
echo Building Crasher Editor V1.3...
echo.

"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ /debug- ^
  /out:"%~dp0Crasher Editor V1.3.exe" ^
  /reference:System.dll ^
  /reference:System.Core.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Windows.Forms.dll ^
  "%~dp0src\CCSaveCrypto.Part01.cs" ^
  "%~dp0src\CCSaveCrypto.Part02.cs" ^
  "%~dp0src\Support.cs" ^
  "%~dp0src\MainForm.Part01.cs" ^
  "%~dp0src\MainForm.Part02.cs" ^
  "%~dp0src\MainForm.Part03.cs" ^
  "%~dp0src\MainForm.Part04.cs" ^
  "%~dp0src\MainForm.Part05.cs" ^
  "%~dp0src\MainForm.Part06.cs" ^
  "%~dp0src\MainForm.Part07.cs" ^
  "%~dp0src\MainForm.Part08.cs" ^
  "%~dp0src\MainForm.Part09.cs" ^
  "%~dp0src\MainForm.Part10.cs" ^
  "%~dp0src\MainForm.Part11.cs" ^
  "%~dp0src\MainForm.Part12.cs" ^
  "%~dp0src\MainForm.Part13.cs" ^
  "%~dp0src\MainForm.Part14.cs" ^
  "%~dp0src\MainForm.Part15.cs" ^
  "%~dp0src\Program.cs"

if errorlevel 1 (
  echo.
  echo BUILD FAILED.
  if defined CI exit /b 1
  pause
  exit /b 1
)

echo.
echo ==========================================
echo  Crasher Editor V1.3 build complete
echo ==========================================
echo.
echo Output:
echo "%~dp0Crasher Editor V1.3.exe"
echo.

if defined CI exit /b 0
pause
