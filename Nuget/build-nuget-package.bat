@echo off
setlocal EnableDelayedExpansion

rem ============================================================
rem  build-nuget-package.bat
rem
rem  Builds two NuGet packages that bundle the six first-party
rem  Rhino.Inside.AutoCAD assemblies:
rem      Rhino.Inside.AutoCAD.NET48  (lib\net48)
rem      Rhino.Inside.AutoCAD.NET8   (lib\net8.0-windows)
rem
rem  Third-party libraries are declared as <dependencies> in the
rem  .nuspec files (NOT bundled). Host APIs (RhinoCommon,
rem  Grasshopper, AutoCAD.NET, Civil3D.NET) are provided by the
rem  host at runtime and are neither bundled nor referenced.
rem
rem  Output lands in Nuget\Output\, fully isolated from the
rem  existing Deployment\ / MSI flow.
rem ============================================================

set "SCRIPT_DIR=%~dp0"

rem Repo root is the folder one level above this Nuget folder.
pushd "%SCRIPT_DIR%.."
set "REPO_ROOT=%CD%"
popd

set "PROPS=%REPO_ROOT%\Directory.Build.props"
set "STAGING=%SCRIPT_DIR%staging"
set "OUTPUT=%SCRIPT_DIR%Output"

rem --- the six first-party projects (output DLL name == project name) ---
set "PROJECTS=Rhino.Inside.AutoCAD.Core Rhino.Inside.AutoCAD.Applications Rhino.Inside.AutoCAD.Interop Rhino.Inside.AutoCAD.Services Rhino.Inside.AutoCAD.Civil.Interop Rhino.Inside.AutoCAD.UI.Resources"

echo(
echo === Rhino.Inside.AutoCAD NuGet packaging ===
echo Repo root : %REPO_ROOT%

rem ------------------------------------------------------------
rem 1. Read version from Directory.Build.props (single source).
rem    Robust to any leading indentation.
rem ------------------------------------------------------------
if not exist "%PROPS%" (
  echo ERROR: "%PROPS%" not found.
  goto :fail
)
set "VLINE="
for /f "delims=" %%a in ('findstr /i /c:"<AssemblyVersion>" "%PROPS%"') do set "VLINE=%%a"
if not defined VLINE (
  echo ERROR: Could not find ^<AssemblyVersion^> in "%PROPS%".
  goto :fail
)
rem Strip everything up to and including the opening tag, then everything from the closing tag.
set "VLINE=!VLINE:*<AssemblyVersion>=!"
for /f "delims=<" %%a in ("!VLINE!") do set "VERSION=%%a"
if not defined VERSION (
  echo ERROR: Could not parse the version value from "%PROPS%".
  goto :fail
)
echo Version   : %VERSION%

rem ------------------------------------------------------------
rem 2. Verify required tools are on PATH.
rem ------------------------------------------------------------
where dotnet >nul 2>nul
if errorlevel 1 (
  echo ERROR: dotnet SDK was not found on PATH.
  goto :fail
)

rem Resolve a NuGet CLI: prefer one already on PATH, otherwise use a
rem local copy next to this script, downloading it on first run.
set "NUGET=nuget"
where nuget >nul 2>nul
if not errorlevel 1 goto :nuget_ok

set "NUGET=%SCRIPT_DIR%nuget.exe"
if exist "%NUGET%" goto :nuget_ok

echo nuget.exe not found on PATH - downloading a local copy...
powershell -NoProfile -ExecutionPolicy Bypass -Command "[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%NUGET%'"
if errorlevel 1 (
  echo ERROR: failed to download nuget.exe.
  echo        Download it manually from https://www.nuget.org/downloads and place it next to this script.
  goto :fail
)
if not exist "%NUGET%" (
  echo ERROR: nuget.exe download did not produce a file.
  goto :fail
)
:nuget_ok
echo NuGet     : %NUGET%

rem ------------------------------------------------------------
rem 3. Build both configurations.
rem      Release      -> net48
rem      ReleaseNET8  -> net8.0-windows
rem
rem    Building Applications + Civil.Interop transitively builds
rem    all six target projects via their ProjectReferences. The
rem    full .sln is deliberately NOT built here because it
rem    contains the WiX installer project, which dotnet build
rem    cannot process (the CI pipeline removes it for the same
rem    reason).
rem ------------------------------------------------------------
call :build Release      || goto :fail
call :build ReleaseNET8  || goto :fail

rem ------------------------------------------------------------
rem 4. Stage the six first-party DLLs (only) per framework.
rem ------------------------------------------------------------
if exist "%STAGING%" rd /s /q "%STAGING%"
call :stage Release      net48           net48 || goto :fail
call :stage ReleaseNET8  net8.0-windows  net8  || goto :fail

rem ------------------------------------------------------------
rem 5. Pack.
rem ------------------------------------------------------------
if not exist "%OUTPUT%" mkdir "%OUTPUT%"

echo(
echo --- Packing ---
"%NUGET%" pack "%SCRIPT_DIR%Rhino.Inside.AutoCAD.NET48.nuspec" -BasePath "%STAGING%\net48" -Version %VERSION% -OutputDirectory "%OUTPUT%"
if errorlevel 1 ( echo ERROR: packing NET48 failed. & goto :fail )
"%NUGET%" pack "%SCRIPT_DIR%Rhino.Inside.AutoCAD.NET8.nuspec" -BasePath "%STAGING%\net8" -Version %VERSION% -OutputDirectory "%OUTPUT%"
if errorlevel 1 ( echo ERROR: packing NET8 failed. & goto :fail )

echo(
echo === Done ===
echo   %OUTPUT%\Rhino.Inside.AutoCAD.NET48.%VERSION%.nupkg
echo   %OUTPUT%\Rhino.Inside.AutoCAD.NET8.%VERSION%.nupkg
echo(
pause
endlocal
exit /b 0

rem ============================================================
rem  Failure exit: report, wait for a key, then return non-zero.
rem ============================================================
:fail
echo(
echo *** Packaging FAILED - see the messages above. ***
echo(
pause
endlocal
exit /b 1

rem ============================================================
rem  :build <Configuration>
rem ============================================================
:build
echo(
echo --- Building %~1 ---
dotnet build "%REPO_ROOT%\src\Rhino.Inside.AutoCAD.Applications\Rhino.Inside.AutoCAD.Applications.csproj" -c %~1 -v minimal
if errorlevel 1 ( echo ERROR: build of Applications ^(%~1^) failed. & exit /b 1 )
dotnet build "%REPO_ROOT%\src\Rhino.Inside.AutoCAD.Civil.Interop\Rhino.Inside.AutoCAD.Civil.Interop.csproj" -c %~1 -v minimal
if errorlevel 1 ( echo ERROR: build of Civil.Interop ^(%~1^) failed. & exit /b 1 )
exit /b 0

rem ============================================================
rem  :stage <Configuration> <tfm> <subfolder>
rem  Copies the six first-party DLLs into:
rem      staging\<subfolder>\lib\<tfm>\
rem  and the repo-root LICENSE.txt + README.md into the package
rem  BasePath root (staging\<subfolder>\) so the .nuspec can
rem  reference them by bare filename.
rem ============================================================
:stage
set "CFG=%~1"
set "TFM=%~2"
set "BASE=%STAGING%\%~3"
set "LIBDIR=%BASE%\lib\%TFM%"
mkdir "%LIBDIR%"
echo(
echo --- Staging %CFG% ^(%TFM%^) ---
set "ICON_SRC=%REPO_ROOT%\src\Rhino.Inside.AutoCAD.UI.Resources\Resources\RhinoInsideLogo.png"
if not exist "%REPO_ROOT%\LICENSE.txt" ( echo ERROR: LICENSE.txt not found at repo root. & exit /b 1 )
if not exist "%REPO_ROOT%\README.md" ( echo ERROR: README.md not found at repo root. & exit /b 1 )
if not exist "%ICON_SRC%" ( echo ERROR: icon not found: %ICON_SRC% & exit /b 1 )
copy /y "%REPO_ROOT%\LICENSE.txt" "%BASE%\" >nul
if errorlevel 1 ( echo ERROR: failed to copy LICENSE.txt & exit /b 1 )
copy /y "%REPO_ROOT%\README.md" "%BASE%\" >nul
if errorlevel 1 ( echo ERROR: failed to copy README.md & exit /b 1 )
copy /y "%ICON_SRC%" "%BASE%\" >nul
if errorlevel 1 ( echo ERROR: failed to copy RhinoInsideLogo.png & exit /b 1 )
echo   + LICENSE.txt
echo   + README.md
echo   + RhinoInsideLogo.png
for %%P in (%PROJECTS%) do (
  set "SRC=%REPO_ROOT%\src\%%P\bin\%CFG%\%TFM%\%%P.dll"
  if not exist "!SRC!" (
    echo ERROR: expected DLL not found: !SRC!
    exit /b 1
  )
  copy /y "!SRC!" "%LIBDIR%\" >nul
  if errorlevel 1 ( echo ERROR: failed to copy !SRC! & exit /b 1 )
  echo   + %%P.dll
)
exit /b 0
