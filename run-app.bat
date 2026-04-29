@echo off
REM Stop any previous TrackCell run, then start API + UI in two console windows.
taskkill /F /IM dotnet.exe >nul 2>&1
timeout /t 2 /nobreak >nul
start "TrackCell.Api" cmd /k "cd /d %~dp0TrackCell.Api && dotnet run"
timeout /t 4 /nobreak >nul
start "TrackCell.UI" cmd /k "cd /d %~dp0TrackCell.UI && dotnet run"
exit
