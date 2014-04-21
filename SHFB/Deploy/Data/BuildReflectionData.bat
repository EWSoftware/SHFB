@ECHO OFF
SETLOCAL

REM Comment this out to use the current environment variable value
SET SHFBROOT=%CD%\..\

ECHO *
ECHO * Building .NET Framework reflection data files using tools in %SHFBROOT%
ECHO *

CD %SHFBROOT%\Data

REM Use the command line overrides if specified
IF NOT '%1'=='' SET FrameworkPlatform=%1
IF NOT '%2'=='' SET FrameworkVersion=%2
IF '%1_%2'=='.NETPortable_4.6' GOTO Unsupported

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "BuildReflectionData.proj"
IF ERRORLEVEL 1 GOTO BuildFailed

ECHO *
ECHO * The reflection data has been built successfully.
ECHO * Rename the "%1_%2" folder to "Reflection" to make it usable.
ECHO *

GOTO Done

:Unsupported

REM Parsing the .NETPortable 4.6 assemblies causes a stack overflow due to type forwarding which MRefBuilder
REM doesn't appear to handle in this case.
ECHO *** The .NETPortable 4.6 Framework assemblies cannot be parsed.  Use another framework type.

:BuildFailed

ECHO *
ECHO * Build failed!
ECHO *

:Done
ENDLOCAL
