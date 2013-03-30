@ECHO OFF
CLS

REM Point DXROOT and SHFBROOT at the development folders so that all help files are built using the latest
REM versions of the tools.
SETLOCAL

SET DXROOT=%CD%\Sandcastle\
SET SHFBROOT=%CD%\SHFB\Source\SandcastleBuilderGUI\bin\Release\

CD Sandcastle\Source

CALL BuildAll.bat

CD ..\..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..

CD .\Extras

CALL BuildAll.bat

CD ..\Documentation

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\Sandcastle\Source\SandcastleSetup

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=x86"

CD ..\..\..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..

ENDLOCAL
