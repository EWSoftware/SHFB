@ECHO OFF

..\SHFB\Source\.nuget\NuGet Pack SHFB.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NET.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NETFramework.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
