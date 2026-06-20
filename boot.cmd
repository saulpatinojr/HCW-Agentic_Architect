@echo off
TITLE Agentic AI Workspace Bootloader
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "scripts\manage-workspace.ps1"
pause