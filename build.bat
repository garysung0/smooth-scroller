@echo off
set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist %CSC% (
    echo Error: csc.exe not found at %CSC%
    exit /b 1
)

echo Compiling ReadFlow.exe with embedded app.ico...
%CSC% /nologo /target:winexe /optimize+ /platform:anycpu /win32icon:"app.ico" /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll /out:"ReadFlow.exe" "SmoothScroller.cs"

if %ERRORLEVEL% equ 0 (
    copy /y "ReadFlow.exe" "SmoothScroller.exe" >nul
    echo [SUCCESS] Built ReadFlow.exe and SmoothScroller.exe successfully!
) else (
    echo [ERROR] Compilation failed!
    exit /b %ERRORLEVEL%
)
