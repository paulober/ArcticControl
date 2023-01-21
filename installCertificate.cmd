@echo off
certutil.exe -addstore TrustedPeople %~dp0\"ArcticControl.cer"
echo Certificate installed!
pause