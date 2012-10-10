@ECHO OFF
CLS

SETLOCAL
SET DXROOT=%CD%\..\Sandcastle\


"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "HtmlToMaml\HtmlToMaml.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "WebCodeProviders\WebCodeProviders.sln" /t:Clean;Build "/p:Configuration=Release;Platform=Any CPU"
"%WINDIR%\Microsoft.Net\Framework\v4.0.30319\msbuild.exe" "SandcastleInstaller\SandcastleInstaller.csproj" /t:Clean;Build /p:Configuration=Release;Platform=x86

REM Don't use OutDir above or it will overwrite the actual installation configuration file with the test version
IF EXIST SandcastleInstaller\bin\Release\SandcastleInstaller.exe COPY /Y SandcastleInstaller\bin\Release\SandcastleInstaller.exe ..\Deployment\

ENDLOCAL
