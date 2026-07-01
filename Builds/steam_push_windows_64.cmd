@echo off
setlocal

REM ========================
REM CONFIG
REM ========================

set STEAMCMD_DIR=%~dp0steamcmd
set SCRIPTS_DIR=%~dp0scripts

REM Optional: remove - rememberlogin if you want fresh login each time
set STEAMCMD_EXE=%STEAMCMD_DIR%\steamcmd.exe

REM ========================
REM RUN
REM ========================

echo =====================================
echo Uploading Steam build...
echo =====================================

"%STEAMCMD_EXE%" ^
  +login zerobyterdev ^
  +run_app_build "%SCRIPTS_DIR%\app_build.vdf" ^
  +quit

echo.
echo =====================================
echo Base build upload complete.
echo =====================================

start "" "https://partner.steamgames.com/apps/builds/913600"

pause