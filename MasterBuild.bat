@ECHO OFF

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

REM We need to use MSBuild 15.0 in order to support the new VSIX format in VS2017 and later
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current" SET "MSBUILD=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\bin\MSBuild.exe"

SET NUGET=%CD%\SHFB\Source\.nuget\NuGet.exe
SET SHFBROOT=%CD%\SHFB\Deploy\
SET BuildConfig=%1

IF '%BuildConfig%'=='' SET BuildConfig=Release

CD SHFB\Source

ECHO *
ECHO * Core tools
ECHO *

"%NUGET%" restore "SandcastleTools.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleTools.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

ECHO *
ECHO * Standalone GUI
ECHO *

"%NUGET%" restore "SandcastleBuilder.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

ECHO *
ECHO * VS2015 and later package
ECHO *

"%NUGET%" restore "SandcastleBuilderPackage.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
IF ERRORLEVEL 1 GOTO End

:BuildDocs

REM Skip help file and setup build if there is no reflection data yet
IF EXIST %SHFBROOT%\Data\.NETFramework\*.xml GOTO ReflectionDataExists

ECHO *
ECHO *
ECHO * Reflection data does not exist for the frameworks.  Building default
ECHO * set for the latest version of the .NETFramework platform on this
ECHO * system.  See the System Requirements and Building the Code wiki topic
ECHO * for topic for more information:
ECHO *
ECHO * https://github.com/EWSoftware/SHFB/wiki/System-Requirements-and-Building-the-Code
ECHO *
ECHO *

%SHFBROOT%ReflectionDataManager /platform:.NETFramework

IF ERRORLEVEL 1 GOTO End

:ReflectionDataExists

CD ..\..\Documentation
IF EXIST .\WebHelp\*.* RD /S /Q .\WebHelp

ECHO *
ECHO * Documentation
ECHO *

"%MSBUILD%" /nologo /v:m "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\SHFB\Source

"%NUGET%" restore "SHFBSetup.sln"
"%MSBUILD%" /nologo /v:m "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

ECHO *
ECHO * All builds completed successfully.
ECHO *

CD ..\..\NuGet

BuildNuGet.bat

CD ..

:End

ENDLOCAL
