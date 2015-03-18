@ECHO OFF

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

SET NUGET=%CD%\SHFB\Source\.nuget\NuGet.exe
SET SHFBROOT=%CD%\SHFB\Deploy\
SET BuildConfig=%1

IF '%BuildConfig%'=='' SET BuildConfig=Release

CD SHFB\Source

"%NUGET%" restore "SandcastleTools.sln"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleTools.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

"%NUGET%" restore "SandcastleBuilder.sln"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

"%NUGET%" restore "SandcastleBuilderPackage.sln"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m /m "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
IF ERRORLEVEL 1 GOTO End
GOTO BuildDocs

:BuildDocs

REM Skip help file and setup build if there is no reflection data yet
IF NOT EXIST %SHFBROOT%\Data\.NETFramework\*.xml GOTO MissingReflectionData

CD ..\..\Documentation
IF EXIST .\WebHelp\*.* RD /S /Q .\WebHelp

"%NUGET%" restore "AllDocumentation.sln"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\SHFB\Source

"%NUGET%" restore "SHFBSetup.sln"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" /nologo /v:m "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\..\NuGet

BuildNuGet.bat

CD ..

GOTO End

:MissingReflectionData
ECHO *
ECHO *
ECHO * Reflection data has not been built yet.  Help file and setup file generation skipped.
ECHO * See ReadMe.txt for more information on running BuildReflectionData.bat.
ECHO *
ECHO *

:End

ECHO *
ECHO * The tools have been built successfully.
ECHO *

ENDLOCAL
