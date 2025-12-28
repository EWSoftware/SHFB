---
uid: GitHubWorkflow
title: Building with a GitHub Workflow
---
<!-- Ignore Spelling: Hidem -->

This topic provides information on building a help website and deploying it to GitHub Pages using a workflow.
Thanks to Stephen Hidem for making the changes and doing the testing necessary to get this working.

## The NuGet Packages
You need to add the tools package and a reflection data set package to your Sandcastle project.  The packages are
located at **NuGet.org**.  *EWSoftware.SHFB*, version 2025.12.18 or later, is the Sandcastle tools package.

You must also install one or more of the reflection data set packages based on which platform types you need.
The reflection data set package names start with **EWSoftware.SHFB** and end with a framework suffix: .NET,
.NETFramework, .NETMicroFramework, .NETCore, .NETPortable, .Silverlight, .WindowsPhone, and .WindowsPhoneApp.  If
multiple versions are available for any given reflection data set package, install the latest version as it will
cover all prior versions as well.  For example, the latest **EWSoftware.SHFB.NETFramework** package covers all
.NET Framework versions from 1.0 to 4.8.  Use the **EWSoftware.SHFB.NET** package for .NET Core, .NET Standard,
and .NET 5.0 and later.

You can add the NuGet packages to your Sandcastle project by right clicking on the **Component Packages** node
and selecting **Manage NuGet Packages**.  Alternatively, it is a simple matter to directly edit the *.shfbproj*
file and add the package references.

``` xml{title=" "}
<ItemGroup>
  <!-- Add the tools package -->
  <PackageReference Include="EWSoftware.SHFB">
    <Version>2025.12.18</Version>
  </PackageReference>
  <!-- Add a reflection data set package based on your documentation source -->
  <PackageReference Include="EWSoftware.SHFB.NET">
    <Version>10.0.0</Version>
  </PackageReference>
</ItemGroup>
```

## Example GitHub Workflow
Here is an example workflow used to build, test, and deploy NuGet packages and a help website built with
Sandcastle.

``` yml{title=" "}
# This workflow will build a .NET project
# For more information see: https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net

name: Deploy Release to NuGet and Website

on:
  release:
    types:
      - published
  workflow_dispatch:
    
jobs:
  build:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      actions: read

    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 10.0.x

    # Restore Dependencies
    - name: Restore Dependencies
      run: |
        dotnet restore AntPlus
        dotnet restore AntRadioInterface
        dotnet restore AntPlus.Extensions.Hosting
        dotnet restore AntPlus.UnitTests
        dotnet restore Examples/AntUsbStick
        dotnet restore Extensions/Hosting/Hosting.UnitTests
        dotnet restore Documentation/Documentation.shfbproj

    # Build GitHub Solution Configuration
    - name: Build NuGet Packages
      run: dotnet build --no-restore --configuration GitHub

    # Run Unit Tests
    - name: Run Unit Tests
      run: dotnet test --no-build --verbosity minimal --configuration GitHub

    # Publish NuGet Packages
    - name: Publish NuGet Packages
      run: dotnet nuget push **/*.nupkg --source https://api.nuget.org/v3/index.json --api-key ${{ secrets.NUGET_API_KEY }} --skip-duplicate

    # Build Documentation and Upload Artifacts
    - name: Build Documentation
      run: dotnet build Documentation/Documentation.shfbproj --no-restore --configuration Release
    - name: Setup Pages
      uses: actions/configure-pages@v5
    - name: Upload artifact
      uses: actions/upload-pages-artifact@v3
      with:
        path: './Documentation/Help'

  deploy:
    needs: build

    # Grant permissions to deploy to GitHub Pages
    permissions:
      pages: write
      id-token: write

    # Deploy to GitHub Pages environment
    environment:
        name: github-pages
        url: ${{ steps.deployment.outputs.page_url }}

    runs-on: ubuntu-latest
    steps:
    # Deploy Documentation to GitHub Pages
    - name: Deploy to GitHub Pages
      id: deployment
      uses: actions/deploy-pages@v4
```

Some things to keep in mind:

- The example above uses the GitHub hosted runner **ubuntu-latest**.  You could use the **windows-latest**
  runner, but you pay a 2x multiplier on the time used.
- You should restore the dependencies and then build the projects you are documenting with the
  **--no-restore** option.
- You can retrieve the *LastBuild.log* file by downloading the artifacts file generated and extracting the
  compressed files.

All of the potential scenarios have not been tested but the example above should be feature complete.  If you
encounter any issue, please open a work item at the [project website](https://github.com/EWSoftware/SHFB/issues)
and provide the necessary details and examples needed to diagnose the issue.

## See Also
**Other Resources**  
[](@GettingStarted)  
[](@XCopyDeployment)  
[](@ConsoleBuild)  
[](@ec822059-b179-4add-984d-485580050ffb)
