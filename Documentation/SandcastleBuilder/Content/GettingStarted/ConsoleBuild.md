---
uid: 8ffc0d37-0215-4609-b6f8-dba53a6c5063
alt-uid: ConsoleBuild
title: Building Projects Outside the GUI
keywords: "console mode, build tool", "console mode, command line options", "console mode, project option overrides", "console mode, response file", "console mode, post-build event", "console mode, third-party build tools"
---
Starting with version 1.8.0.0 of the help file builder, the project file uses a standard MSBuild file format.
Projects can be built from the command line using the *MSBuild* executable.  Starting with version 2021.9.9.0,
the *dotnet build* tool is also supported.

<autoOutline />

## Building From the Command Line
Use *MSBuild.exe* or *dotnet.exe* with the **build** command line option to build help file builder projects from
the command line.  All of the usual MSBuild command line options can be used.  Below are some common command line
option and property overrides not directly represented by a matching help file builder project property.  For a
full list of MSBuild command line options, see the
[MSBuild Command Line Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference).

<table>
  <thead>
    <tr>
      <th>Option</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>/verbosity:[level] or /v:[level]</td>
      <td>

Set the overall verbosity level of the build.  This controls which messages MSBuild will display.  The default is
`normal` which shows informational messages, errors, and warnings.

</td>
    </tr>
    <tr>
      <td>/p:Verbose=[True|False]</td>
      <td>This property value can be specified to control the output from the build process.  The
default is false to only output build step names as the build is performed.  Setting this property value to true
will display all output from all build steps.  This is equivalent to the information written to the log file.</td>
    </tr>
    <tr>
      <td>/p:DumpLogOnFailure=[True|False]</td>
      <td>This property value can be specified to control whether or not the log file is dumped to the
MSBuild log if the project fails to build.  The default is false to not dump the log.  Setting this property
value to true will dump the log content if the build fails.</td>
    </tr>
    <tr>
      <td>/p:Configuration=[configName]</td>
      <td>

This can be used to specify the `Configuration` value to use for any solution or project file documentation
sources.  If not set, it defaults to `Debug`.

</td>
    </tr>
    <tr>
      <td>/p:Platform=[configName]</td>
      <td>

This can be used to specify the `Platform` value to use for any solution or project file documentation sources.
If not set, it defaults to `AnyCPU`.

</td>
    </tr>
    <tr>
      <td>/p:CustomBeforeSHFBTargets=[projectFile]</td>
      <td>This can be used to specify a custom project file containing property or target overrides
that should be evaluated prior to the help file builder's targets.  Specify the full path to the project file.
Do not enclose the project filename value in single quotes or it will be ignored.  If necessary, enclose the
entire command line parameter in double quotes.</td>
    </tr>
    <tr>
      <td>/p:CustomAfterSHFBTargets=[projectFile]</td>
      <td>This can be used to specify a custom project file containing property or target overrides
that should be evaluated after the help file builder's targets.  Specify the full path to the project file.  Do
not enclose the project filename value in single quotes or it will be ignored.  If necessary, enclose the entire
command line parameter in double quotes.</td>
    </tr>
  </tbody>
</table>

> [!NOTE]
> Regardless of any verbosity level setting, full output from the build is always saved to the log file.  As with
> the standalone GUI, the help file and the log file can be found in the folder specified in the project's
> `OutputPath` property after a build.

Prefix options with '-' or '/'.  Property values should be enclosed in double quotes if they contain spaces,
commas, or other special characters. All relative paths specified on the command line and those in a response
file (see below) are relative to the project's path as usual.

> [!IMPORTANT]
> Property names specified on the command line or in response files are case-insensitive as are simple property
> values.  However, XML-based property values (the collection-based ones) are case-sensitive with regard to the
> element names.  The values of the elements themselves are case-insensitive.
> 
> If a file path is specified on the command line that ends in a backslash, escape it with an additional
> backslash or the command interpreter will likely include it as an escape for the following character such as
> the closing quote that surrounds the path.  This will likely cause the path to be interpreted incorrectly and
> the build will fail.

## Response Files
In addition to specifying project option overrides on the command line, MSBuild supports response (*.rsp*) files
that contain a complex list of command line options used to build help file projects.  Each option can be on a
separate line or all options can be on one line.  Comment lines are prefaced with a `#` symbol.  The `@` switch
is used to pass another response file to *MSBuild.exe*.

``` bat{title=" "}
%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe @ResponseFile.rsp
```

## Target Override Files
As noted above, the `CustomBeforeSHFBTargets` and `CustomAfterSHFBTargets` properties can be set to the names of
MSBuild project files containing property and/or target definition overrides.  The `CustomBeforeSHFBTargets`
property is useful for specifying overrides for complex properties such as those for build component
configurations, plug-in configurations, and API filter settings as you can specify them using normal XML that
spans multiple lines.  An example override file is shown below:

> [!IMPORTANT]
> The paths to the target override files should be fully qualified as relative paths on them are evaluated by
> MSBuild based on the help file builder's target file location.  Do not enclose the project filename in single
> quotes or it will be ignored.  If necessary, enclose the entire command line parameter in double quotes.

``` xml{title="Example Override Project File"}
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <!-- Example documentation source overrides
  <PropertyGroup>
    <DocumentationSources>
      <DocumentationSource sourceFile="..\..\OtherProject\bin\Release\OtherAssembly.xml" xmlns="" />
      <DocumentationSource sourceFile="..\..\OtherProject\bin\Release\OtherAssembly.dll" xmlns="" />
    </DocumentationSources>
  </PropertyGroup>
  -->

  <PropertyGroup>
    <HtmlHelpName>TestDocOverride</HtmlHelpName>
    <RootNamespaceTitle>Root Namespace Override</RootNamespaceTitle>
    <HelpTitle>Test Help File Override</HelpTitle>
  </PropertyGroup>

  <PropertyGroup>
    <!-- Add the overridden properties as the value of the SubstitutionTags property -->
    <SubstitutionTags>$(SubstitutionTags);HtmlHelpName=$(HtmlHelpName);
      RootNamespaceTitle=$(RootNamespaceTitle);HelpTitle=$(HelpTitle)
    </SubstitutionTags>
  </PropertyGroup>

  <!-- Example target override -->
  <Target Name="BeforeBuildHelp">
    <Message Text="BeforeBuildHelp override executed" />
  </Target>

</Project>
```

The help file builder target definition file contains several targets that can be overridden to define custom
behavior that occurs before and after the help file builder build targets.  These can be defined in a custom
project file specified with the `CustomAfterSHFBTargets` property.

> [!NOTE]
> These targets are not executed when built from within the standalone help file builder GUI.  These are only
> executed when built using MSBuild.

| Target | Purpose |
| --- | --- |
| BeforeBuildHelp   | This target executes before the help file is built. |
| AfterBuildHelp    | This target executes after the help file is built. |
| BeforeCleanHelp   | This target executes before the prior build's output is cleared. |
| AfterCleanHelp    | This target executes after the prior build's output is cleared. |
| BeforeRebuildHelp | This target executes before the help file is rebuilt. |
| AfterRebuildHelp  | This target executes after the help file is rebuilt. |

## Specifying a Post-Build Event to Build a Help File
Help file builder projects are supported in Visual Studio if you install the Visual Studio extension package.  In
addition, a post-build event in a Visual Studio project can be used to build them.  To do so, right click on a
project name in the Solution Explorer, select **Properties**, and select the **Build Events** sub-item.  Click in
the **Post-build Event Command Line** option to enter the command line to run.  You can click the **Edit
Post-build** button to open a dialog with a larger editor and a list of available macros.  Below is an example of
a common command line script that can be used (lines wrapped for display purposes). Replace the path to the tool
with the path where you installed it on your PC.  The `IF` statement prevents building the help file in debug
builds where it may not be needed.

``` bat{title="Note: Lines wrapped for display purposes"}
IF "$(ConfigurationName)"=="Debug" Goto Exit

ECHO Building SHFB help file via MSBuild
%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe"
    /p:CleanIntermediates=True /p:Configuration=Release
    "$(SolutionDir)Doc\SandcastleBuilder.shfbproj"

:Exit
```

In a solution with multiple projects that are documented by the same help file builder project, the post-build
event should be defined on the last project built by Visual Studio.  If the projects are documented individually,
you can place a post-build event on each one.

## Other Build Tools
Other automated build tools also exist.  As long as the build tool supports execution of command line based
executables with parameters such as *MSBuild.exe*, you should be able to integrate it with the build process in a
similar fashion.

## See Also
**Other Resources**  
[](@GettingStarted)  
[](@GitHubWorkFlow)  
[](@ec822059-b179-4add-984d-485580050ffb)  
[](@96956ab6-fd5e-4bc7-a577-a18b0ff258ea)  
[](@76eb8f39-b225-4881-afa2-13cb7829b944)
