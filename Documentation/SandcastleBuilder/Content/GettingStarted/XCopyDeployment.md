---
uid: 50ad2c8c-5004-4b4c-a77f-97b8c403c9f2
alt-uid: XCopyDeployment
title: XCOPY/NuGet Build Server Deployment
keywords: "deployment, xcopy", "deployment, build server", "deployment, NuGet"
---
For use in a build server environment, the Sandcastle Help File Builder and Tools support simple XCOPY
deployment.  As long as the `SHFBROOT` system environment variable points to the location of the help file
builder assemblies, it will be able to find everything it needs at build time.  In such situations, custom build
components, plug-ins, syntax generators, and presentation styles may also be copied to the help file builder
folder or a sub-folder beneath it.

A NuGet Package is also available (**EWSoftware.SHFB**) that allows you to deploy the Sandcastle Help File
Builder tools inside of a project to build help files without installing the tools manually and without defining
the `SHFBROOT` environment variable.  Certain limitations apply in this case and are detailed in the package's
Read Me file which is displayed when the package is installed.

For NuGet installations, you must also install one or more of the Reflection Data Set packages based on which
platform types you need.  The reflection data set package names start with **EWSoftware.SHFB** and end with a
framework suffix: .NET, .NETFramework, .NETMicroFramework, .NETCore, .NETPortable, .Silverlight, .WindowsPhone,
and .WindowsPhoneApp.  If multiple versions are available for any given reflection data set package, install the
latest version as it will cover all prior versions as well.  For example, the latest
**EWSoftware.SHFB.NETFramework** package covers all .NET Framework versions from 1.0 to 4.8.  Use the
**EWSoftware.SHFB.NET** package for .NET Core, .NET Standard, and .NET 5.0 and later.

To install the packages, right click on the project's [](@273dcaf1-1e51-475d-a622-732dd3afc212) node and select
the **Manage NuGet Packages** option.  If NuGet packages are available for build components used by the project,
they can also be added as package references using this option.  Some required import elements must be present in
the help file build project for package restores to work properly.  Those elements will be added by the package
manager form when the first package is added to the project.

When searching for custom components, the following search order is used:

- If NuGet package references are present in the project, the package tool paths in each are searched.  Help file
  builder packages define a build properties file containing a `SHFBComponentPath` item that equates to the
  package's *.\tools* folder.
- If a `ComponentPath` project property value is set, the folder it refers to is searched first and then the
  actual project folder is searched.  If the property contains no value, only the project folder is searched.
- Next, the common application data folders noted in the [](@a24489fb-45d6-46f4-9eaa-9a9c4e0919b2) topic are
  searched.
- Finally, the help file builder installation folder (`SHFBROOT`) is searched.

If any duplicate components are encountered, the first one loaded based on the above search order will be used.

> [!IMPORTANT]
> The Help 1 file format is unsupported in the XCOPY/NuGet deployment scenario as it requires additional tools
> and components to be installed.  If these tools and components are missing on the build server, the help file
> build will fail.  Only the MS Help Viewer, Open XML, website, and markdown help file formats are guaranteed to
> work as they have no external tool dependencies.
> 
> It is possible to install the tools and the Help 1 compiler in Docker.  See the following online topics for
> more information:
> [Installation in Docker Image](https://github.com/EWSoftware/SHFB/issues/1017#issuecomment-2106312821 "Installation in Docker Image")
> and [Install current HTML Help 1 Compiler for Client, Server or Docker image](https://github.com/EWSoftware/SHFB/issues/1060 "Install current HTML Help 1 Compiler for Client, Server or Docker image").

## See Also
**Other Resources**  
[](@GettingStarted)  
[](@a24489fb-45d6-46f4-9eaa-9a9c4e0919b2)  
[](@GitHubWorkflow)
