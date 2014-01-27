@ECHO OFF

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

SET SHFBROOT=%CD%\SHFB\Deploy\

CD SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleTools.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\..\Documentation

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\..

:End
ENDLOCAL
