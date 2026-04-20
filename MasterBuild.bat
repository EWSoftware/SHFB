@ECHO OFF

IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\18\Community\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Community\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\18\Professional\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Professional\MSBuild\Current\bin\MSBuild.exe"
IF EXIST "%ProgramFiles%\Microsoft Visual Studio\18\Enterprise\MSBuild\Current" SET "MSBUILD=%ProgramFiles%\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\bin\MSBuild.exe"

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

SET SHFBROOT=%CD%\SHFB\Deploy\
SET BuildConfig=%1

IF '%BuildConfig%'=='' SET BuildConfig=Release

CD SHFB\Source

ECHO *
ECHO * Core tools
ECHO *

"%MSBUILD%" /nr:false /r /nologo /v:m /m "SandcastleTools.slnx" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
IF ERRORLEVEL 1 GOTO End

ECHO *
ECHO * VSIX package
ECHO *

"%MSBUILD%" /nr:false /r /nologo /v:m /m "SandcastleBuilderPackage.slnx" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
IF ERRORLEVEL 1 GOTO End

:BuildDocs

REM Copy or build the reflection data sets if necessary
IF EXIST %SHFBROOT%\Data\.NETFramework\*.xml GOTO ReflectionDataExists
IF NOT EXIST "%ProgramFiles(x86)%\EWSoftware\Sandcastle Help File Builder\Data" GOTO BuildReflectionData

ECHO Copying reflection data from "%ProgramFiles(x86)%\EWSoftware\Sandcastle Help File Builder\Data"
XCOPY /S /Q /Y "%ProgramFiles(x86)%\EWSoftware\Sandcastle Help File Builder\Data\*" "%SHFBROOT%Data\"
GOTO ReflectionDataExists

:BuildReflectionData

ECHO *
ECHO *
ECHO * Reflection data does not exist for the frameworks.  Building default
ECHO * set for the latest version of the .NET and .NETFramework platforms on
ECHO * this system.  See the System Requirements and Building the Code wiki
ECHO * topic for more information:
ECHO *
ECHO * https://github.com/EWSoftware/SHFB/wiki/System-Requirements-and-Building-the-Code
ECHO *
ECHO *

%SHFBROOT%Tools\ReflectionDataManager /platform:.NET
%SHFBROOT%Tools\ReflectionDataManager /platform:.NETFramework

IF ERRORLEVEL 1 GOTO End

:ReflectionDataExists

CD ..\..\Documentation
IF EXIST .\WebHelp\*.* RD /S /Q .\WebHelp

ECHO *
ECHO * Documentation
ECHO *

"%MSBUILD%" /nr:false /nologo /v:m /m "AllDocumentation.slnx" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

CD ..\SHFB\Source

"%MSBUILD%" /nr:false /nologo /v:m /m "SHFBSetup.slnx" /t:Clean;Restore;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
"%MSBUILD%" /nr:false /nologo /v:m /m "WixSetup.slnx" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=x86"

IF ERRORLEVEL 1 GOTO End

ECHO *
ECHO * All builds completed successfully.
ECHO *

CD ..\..\NuGet

BuildNuGet.bat

CD ..

:End

ENDLOCAL
