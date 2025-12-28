---
uid: ec822059-b179-4add-984d-485580050ffb
alt-uid: TeamBuild
title: Generating Documentation with Team Build
keywords: Team Build, "console mode, Team Build"
---
<!-- Ignore Spelling: Dallmann Jeroen Vos Proc src tfsserver -->

This topic provides instructions for using the help file builder in conjunction with Team Build to generate
documentation for your projects.  This information was originally supplied by Tim Dallmann in the MSDN Developer
Documentation and Help System forum.  It was revised by Jeroen Vos to provide up to date information on using the
Sandcastle Help File Builder v1.8.0.0 or later with Visual Studio Team Foundation Server 2008 SP1.

> [!NOTE]
> Since I do not have a copy of Team Build to test against, the information in this topic may not be completely
> accurate for all users.  Also note that current versions of the help file builder provide a Visual Studio
> Package that allows integration of help file builder projects into Visual Studio solutions.  As such, this
> information is subject to change.

> [!NOTE]
> On newer version of the build server it may be necessary to disable the parallel build setting in the build
> definition in order for the build to work properly.  For the default build template, you can find the setting
> under **Process** | **3. Advanced** | **MSBuild Multi-Proc**.  Set that option to false.

## The Process
The Sandcastle Help File Builder includes a GUI for editing documentation generation properties on the client and
also provides a VSPackage for full integration with Visual Studio.  The project file is a standard MSBuild
project file and can be built from the command line using the MSBuild executable.  The output from Sandcastle can
be copied with the build script to any location you like.  For example, it can be copied to Team Foundation
Server and a link to it can be placed on the team's project site.

<list class="ordered">
<listItem>

Download and [install](@8c0c97d0-c968-4c15-9fe9-e8f3a443c50a) Sandcastle, the HTML Help Workshop, and the
Sandcastle Help File Builder on your development machine and your build server.

</listItem>

<listItem>

Run the Sandcastle Help File builder and create a new project.  Save it in the root folder of your solution if
using the standalone GUI to manage the project.

- Add the solution or the project files as documentation sources. That way, the help file builder will include
  all projects and project outputs in the build. If you do not add Visual Studio Solution or project files, make
  sure the assemblies you want to document are built to the same location relative to the help file builder's
  project file on your development machine as they are on your build machine.  In addition, you can specify the
  `Configuration` and `Platform` project properties in the build script to select the `Debug` or `Release`
  version rather than hard coding the paths to them in the project file.
- If you used solution or project files as the documentation sources you do not need to worry about dependencies
  as they are taken care of automatically.  However, if you add assemblies or XML comments files as documentation
  sources you may need to add dependencies manually too.  In that case, make sure that the dependencies are
  located in the same location relative to the help file builder project file on both the development machine and
  the build machine.
- You may find that the build fails on the build machine because it cannot find one or more of the tools (the
  Sandcastle tools such as **MRefBuilder** or the help compiler).  If this occurs, specify the paths to the tools
  in the help file project's **Paths** category properties.
- Set all other project properties as needed.

</listItem>

<listItem>

If using the standalone GUI to manage the project, add the generated *.shfbproj* project file to your Visual
Studio solution as a Solution Item and check it in.

</listItem>

<listItem>

Modify your build file.  This example uses a daily build to generate documentation:

<list class="bullet">
<listItem>

Add a `GenerateDocumentation` target to your build script.  This target was chosen because the TFS team meant it
to be used specifically for this purpose.  This will call the Sandcastle Help File Builder project to create the
help files.

> [!IMPORTANT]
> Because Team Build overrides the `OutDir` property for all built solutions/projects to point to one folder that
> will store all build output, you should provide this property to the help file builder project file if Visual
> Studio solution or project files were specified as the documentation sources.  If you do not, the help file
> builder will look in the wrong place for the assemblies to document.

``` xml{title=" "}
<Target Name="GenerateDocumentation">
	<!-- Build source code docs -->
	<MSBuild Projects="$(SolutionRoot)\src\MyProjectHelp.shfbproj"
		Properties="Configuration=Release;Platform=AnyCPU;OutDir=$(OutDir)" />
</Target>
```

> [!TIP]
> The `Configuration` and `Platform` properties can be obtained from predefined Team Build properties as well.
> However, be aware that the help file builder as well as most Visual Studio projects default to a value of
> "AnyCPU" for the `Platform` property whereas Team Build will use "Any CPU" (with as space) as the default.

See the [](@ConsoleBuild) help topic for details on building a help file builder project manually from the
command line.

</listItem>
</list>

</listItem>

<listItem>

The following steps are optional and can be used to deploy the help file.  This example assumes you are deploying
website output to a web server.

<list class="bullet">
<listItem>

Add a new web share to your destination server (i.e. the Team Foundation Server).  The following example assumes
it is called *Code Documentation*.

</listItem>

<listItem>

Make sure the folder properties grant full access to the process user running the build (**TFSBuild** for example).

</listItem>

<listItem>

Add *Index.aspx* and/or *Index.html* to the default documents for the site.

</listItem>

<listItem>

Place a subfolder within this folder for the project documentation.

</listItem>

<listItem>

Add an `AfterDropBuild` target to your build script to copy all the new help files to the web server:

``` xml{title=" "}
<Target Name="AfterDropBuild">
  <!-- Delete old source code docs -->
  <CreateItem Include="\\tfsserver\Code Documentation\MyProject\**\*.*">
    <Output TaskParameter="Include" ItemName="DocDeployFiles" />
  </CreateItem>
  <Delete Files="@(DocDeployFiles)" />

  <!-- Copy new source code docs to team system -->
  <CreateItem Include="$(SolutionRoot)\src\Help\**\*.*"
      Exclude="$(SolutionRoot)\src\Help\Working\**\*.*">
    <Output TaskParameter="Include" ItemName="NewDocFiles" />
  </CreateItem>
  <Copy SourceFiles="@(NewDocFiles)"
    DestinationFolder="\\tfsserver\Code Documentation\MyProject\%(NewDocFiles.RecursiveDir)" />
</Target>
```

> [!NOTE]
> You do not need the `Exclude` attribute for the `NewDocFiles` property if you set the help file project's
> `CleanIntermediates` property to true. 	

</listItem>
</list>

</listItem>

<listItem>

Check in your build script and run it.

</listItem>
</list>

You should now have fresh documentation available every day.  You can also set up a separate build using a
different name that allows you to create new documentation during the day without running all the other items in
the daily build.  This can be useful if someone adds a lot of new documentation into the code that you want to
make available immediately.

## See Also
**Other Resources**  
[](@GitHubWorkflow)  
[](@ConsoleBuild)  
[](@GettingStarted)  
[](@Installation)
