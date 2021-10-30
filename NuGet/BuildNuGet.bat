@ECHO OFF

NuGet Pack SHFB.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
NuGet Pack SHFB.NET.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
NuGet Pack SHFB.NETFramework.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
