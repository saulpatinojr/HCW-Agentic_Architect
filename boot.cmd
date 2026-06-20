@echo off
TITLE Workspace Manager Bootloader
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\manage-workspace.ps1"
pause
