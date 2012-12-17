@ECHO OFF
CLS

REM This assumes that DXROOT and SHFBROOT are pointing at the development folders so that all help files
REM are built using the latest versions of the tools.

CD Sandcastle\Source

CALL BuildAll.bat

CD ..\..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilder.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleBuilderPackage.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..\Extras

CALL BuildAll.bat

CD ..\..\Documentation

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "AllDocumentation.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\Sandcastle\Source\SandcastleSetup

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=x86"

CD ..\..\..\SHFB\Source

"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SHFBSetup.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"

CD ..\..
