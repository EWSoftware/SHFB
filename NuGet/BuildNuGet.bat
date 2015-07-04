@ECHO OFF

..\SHFB\Source\.nuget\NuGet Pack SHFB.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NETCore.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NETFramework.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NETMicroFramework.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.NETPortable.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.Silverlight.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.WindowsPhone.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
..\SHFB\Source\.nuget\NuGet Pack SHFB.WindowsPhoneApp.nuspec -NoDefaultExcludes -NoPackageAnalysis -OutputDirectory ..\Deployment\NuGet
