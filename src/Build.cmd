@echo off

IF NOT "%VS140COMNTOOLS%" == "" (call "%VS140COMNTOOLS%vsvars32.bat")

@echo on

.paket\paket.bootstrapper.exe
.paket\paket.exe restore

msbuild.exe /ToolsVersion:14.0 "SimpleMusicPlayer.sln" /p:configuration=Debug /p:platform=x86 /m /t:Clean,Rebuild
msbuild.exe /ToolsVersion:14.0 "SimpleMusicPlayer.sln" /p:configuration=Release /p:platform=x86 /m /t:Clean,Rebuild
msbuild.exe /ToolsVersion:14.0 "SimpleMusicPlayer.sln" /p:configuration=Release /p:platform="Any CPU" /m /t:Clean,Rebuild
