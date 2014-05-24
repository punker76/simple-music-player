@echo on
call "%VS120COMNTOOLS%vsvars32.bat"

msbuild.exe /ToolsVersion:4.0 "SimpleMusicPlayer.sln" /p:StrongName=True /p:configuration=Release /p:platform=x86
msbuild.exe /ToolsVersion:4.0 "SimpleMusicPlayer.sln" /p:StrongName=True /p:configuration=Release /p:platform="Any CPU"
