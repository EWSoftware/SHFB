Sandcastle Help File Builder
----------------------------
This package allows you to deploy the Sandcastle Help File Builder tools inside of a project to build help files
without installing the tools manually such as on a build server.  The following limitations apply:

- Only the website, MS Help Viewer, and Open XML help file formats are supported as there are no external tool
  dependencies for them.

- To build Help 1 or MS Help 2 output, the required help compiler tools and components must be installed separately.
  Help file builds using those formats will fail if those components are not installed.

- Any third-party build components, plug-ins, presentation styles, etc. must be included within your project and
  must be referenced using the Component Path project property.

- The SHFBROOT environment variable does not need to be defined.  However, the help file project must contain a
  conditional property that defines the relative path to the SHFB tools folder within the main PropertyGroup element.

  Example:

    <PropertyGroup>
      <!-- NOTE: Update the version number in the path to match the package version -->
      <SHFBROOT Condition=" '$(SHFBROOT)' == '' ">$(MSBuildThisFileDirectory)..\packages\EWSoftware.SHFB.2014.11.22.0\Tools\</SHFBROOT>

      ... other SHFB project properties ...

    </PropertyGroup>

- No standalone GUI or Visual Studio support is provided for managing the help project and none of the non-essential
  tools or help files are included in this package in order to keep its size down to the bare minimum.  If wanted,
  those items must be downloaded and installed from the CodePlex project site using the guided installer:
  http://SHFB.CodePlex.com

See this help topic for more details on XCOPY/NuGet Build Server Deployment:
    http://www.ewoodruff.us/shfbdocs/html/50ad2c8c-5004-4b4c-a77f-97b8c403c9f2.htm
