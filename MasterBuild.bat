@ECHO OFF
CLS

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

SET SHFBROOT=%CD%\SHFB\Deploy\

CD SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleTools.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..\Documentation

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..

ENDLOCAL
