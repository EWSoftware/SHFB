@ECHO OFF
SETLOCAL

REM Comment this out to use the current environment variable value
SET SHFBROOT=%CD%\..\

ECHO *
ECHO * Building framework reflection data files using tools in %SHFBROOT%
ECHO *

CD %SHFBROOT%\Data

REM Use the command line overrides if specified
IF NOT '%1'=='' SET FrameworkPlatform=%1

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "BuildReflectionData.proj"

IF ERRORLEVEL 0 GOTO BuildSuccess

ECHO *
ECHO * Build failed!
ECHO *

GOTO Done

:BuildSuccess

ECHO *
ECHO * The reflection data has been built successfully.  Rename the temporary
ECHO * folder appropriately to make it usable:
ECHO *
ECHO *     .NETFramework_[version]     to .NETFramework
ECHO *     .NETCore_[version]          to .NETCore
ECHO *     .NETPortable_[version]      to .NETPortable
ECHO *     .NETMicroFramewor_[version] to .NETMicroFramework
ECHO *     Silverlight_[version]       to Silverlight
ECHO *     WindowsPhone_[version]      to WindowsPhone
ECHO *     WindowsPhoneApp_[version]   to WindowsPhoneApp
ECHO *

:Done
ENDLOCAL
