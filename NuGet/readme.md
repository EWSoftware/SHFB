# Sandcastle Help File Builder

This package allows you to deploy the Sandcastle Help File Builder tools inside of a project to build help files
without installing the tools manually such as on a build server.  The `SHFBROOT` environment variable does not
need to be defined.  The following limitations apply:

- You must install one or more of the Reflection Data Set packages based on which platform types you need:

  `.NET`, `.NETFramework`, `.NETCore`, `.NETMicroFramework`, `.NETPortable`, `Silverlight`, `WindowsPhone`,
  `WindowsPhoneApp`, and/or `UniversalWindows`.

  If multiple versions are available for any given reflection data set package, install the latest version as
  it will cover all prior versions as well.

- Only the website, MS Help Viewer, Open XML, and markdown help file formats are supported unconditionally as
  there are no external tool dependencies for them.

- To build Help 1 output, the required help compiler tools and components must be installed separately.  Help file
  builds using that formats will fail if those components are not installed.

- Any third-party build components, plug-ins, presentation styles, etc. not deployed as NuGet packages must be
  included within your project and must be placed within the project folder so that they can be found.

- No standalone GUI or Visual Studio support is provided for managing the help project and none of the non-essential
  tools or help files are included in this package in order to keep its size down to the bare minimum.  If wanted,
  those items must be downloaded and installed from the GitHub project site using the guided installer:
  [https://GitHub.com/EWSoftware/SHFB](https://GitHub.com/EWSoftware/SHFB)

See the [XCOPY/NuGet Build Server Deployment](http://EWSoftware.github.io/SHFB/html/50ad2c8c-5004-4b4c-a77f-97b8c403c9f2.htm)
help topic for more details.
