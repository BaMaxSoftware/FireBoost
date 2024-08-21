@echo off

(
    cd %~dp0
    start call obfuscateFireBoost.bat
) | set /P "="

echo "obfuscation done"