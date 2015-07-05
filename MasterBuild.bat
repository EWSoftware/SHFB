@ECHO OFF

REM Point SHFBROOT at the development folder so that all help files are built using the latest version of the tools.
SETLOCAL

SET MSBUILD=%ProgramFiles(x86)%\MSBuild\12.0\bin\MSBuild.exe
SET NUGET=%CD%\SHFB\Source\.nuget\NuGet.exe
SET SHFBROOT=%CD%\SHFB\Deploy\
SET BuildConfig=%1

IF '%BuildConfig%'=='' SET BuildConfig=Release

REM Use Visual Studio 2015 references if Visual Studio 2013 is not present
IF NOT EXIST "%VS120COMNTOOLS%..\IDE\devenv.exe" SET VisualStudioVersion=14.0

CD SHFB\Source

"%NUGET%" restore "SandcastleTools.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleTools.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

"%NUGET%" restore "SandcastleBuilder.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"

IF ERRORLEVEL 1 GOTO End

REM Enforce use of VS 2010 SDK if present.  If not it tries to use the VS 2011 SDK even if its not there.
REM IF EXIST "%ProgramFiles(x86)%\MSBuild\Microsoft\VisualStudio\v10.0\VSSDK\Microsoft.VsSDK.targets" SET VisualStudioVersion=10.0

"%NUGET%" restore "SandcastleBuilderPackage.sln"
"%MSBUILD%" /nologo /v:m /m "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=%BuildConfig%;Platform=Any CPU"
IF ERRORLEVEL 1 GOTO End
GOTO BuildDocs

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
