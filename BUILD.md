# Debugging Overview <!-- omit in toc -->

- [1. Preconditions](#1-preconditions)
- [2. Building the Software](#2-building-the-software)
  - [2.1. Setting the SHFBROOT Directory](#21-setting-the-shfbroot-directory)
  - [2.2. Build Command](#22-build-command)
  - [2.3. Building with Visual Studio 2022](#23-building-with-visual-studio-2022)
  - [2.4. Reference Framework](#24-reference-framework)
    - [2.4.1. Building the Reference Framework Manually](#241-building-the-reference-framework-manually)
  - [2.5. Installer Package](#25-installer-package)
    - [2.5.1. Visual Studio Extension Development](#251-visual-studio-extension-development)
    - [2.5.2. WiX Toolchain](#252-wix-toolchain)
- [3. Debugging](#3-debugging)

## 1. Preconditions

The following development tools are required:

- Visual Studio 2026
- Visual Studio Extension Development (from Other Toolsets)
- [Wix 3.x](https://github.com/wixtoolset/wix3/releases/tag/wix3141rtm)
- [WiX 3.x VS2022
  extension](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2022Extension)
- `NuGet` binary

Optionally, to get prebuilt .NET reflection documentation, install:

- Install [SHFB
  v2026.3.29.0](https://github.com/EWSoftware/SHFB/releases/tag/v2026.3.29.0).

## 2. Building the Software

Main documentation for building is found at the [GitHub SHFB
Wiki](https://github.com/EWSoftware/SHFB/wiki/System-Requirements-and-Building-the-Code).

Building and debugging works for Visual Studio 2026.

### 2.1. Setting the SHFBROOT Directory

Update the `SHFBROOT` to the location of the `Deploy` folder in the repository
downloaded, e.g.

```text
SHFBROOT=D:\repo\SHFB\SHFB\Deploy\
```

All help files are built using the latest version of the tools.

### 2.2. Build Command

Ensure that the SDKs for the frameworks are installed (e.g. .NET Framework 4.8
and .NET Core 10.0).

```sh
D:\repo\SHFB> masterbuild.bat
```

To build in debug mode:

```sh
D:\repo\SHFB> masterbuild.bat debug
```

Note, internally the command sets the `SHFBROOT=%CD%\SHFB\Deploy\`, so the
variable doesn't need to be set for this build sequence.

### 2.3. Building with Visual Studio 2022

Building with Visual Studio 2022 directly does *not* work. Building results in
the errors:

```text
CSC : error CS1617: Invalid option '14.0' for /langversion. Use '/langversion:?' to list supported values. [D:\repo\SHFB\SHFB\Source\CodeColorizer\ColorizerLibrary\ColorizerLibrary.csproj::TargetFramework=net48]
CSC : error CS1617: Invalid option '14.0' for /langversion. Use '/langversion:?' to list supported values. [D:\repo\SHFB\SHFB\Source\HelpLibraryManagerLauncher\HelpLibraryManagerLauncher.csproj]
CSC : error CS1617: Invalid option '14.0' for /langversion. Use '/langversion:?' to list supported values. [D:\repo\SHFB\SHFB\Source\CodeColorizer\ColorizerLibrary\ColorizerLibrary.csproj::TargetFramework=netstandard2.0]
CSC : error CS1617: Invalid option '14.0' for /langversion. Use '/langversion:?' to list supported values. [D:\repo\SHFB\SHFB\Source\BuildAssembler\BuildComponentTargets\BuildComponentTargets.csproj]
CSC : error CS1617: Invalid option '14.0' for /langversion. Use '/langversion:?' to list supported values. [D:\repo\SHFB\SHFB\Source\SandcastleCore\SandcastleCore.csproj]
```

One can work around this by modifying the file
`SHFB\Source\Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <SignAssembly>true</SignAssembly>
    <AssemblyOriginatorKeyFile>$(MSBuildThisFileDirectory)SandcastleTools.snk</AssemblyOriginatorKeyFile>
    <NeutralLanguage>en</NeutralLanguage>
    <Product>Sandcastle Help File Builder and Tools</Product>

    <!-- Using 14.0 for the field keyword.  If you only have VS 2022, setting it to preview should work too. -->
    <LangVersion>14.0</LangVersion>

    <EnableNETAnalyzers>true</EnableNETAnalyzers>
    <AnalysisMode>AllEnabledByDefault</AnalysisMode>
  </PropertyGroup>
</Project>
```

### 2.4. Reference Framework

Running `MasterBuild.bat` generates the .NET Framework and .NET Core reflection
documentation.

1. Check if SHFB is installed, and if so, it copies the documentation to the
   current directory.
2. Build reference documentation using the `ReflectionDataManager.exe` tool.

#### 2.4.1. Building the Reference Framework Manually

One can build documentation for .NET Core by running the tool
`%SHFBROOT%Tools\ReflectionDataManager.exe` directly, loading the file
`Data\.NET\.NETCoreStandardNet50Plus.reflection` and building the documentation
via the GUI.

However, even when building this manually, building existing projects that
depend on .NET Core still fail. The only solution right now is to copy from an
existing installation.

### 2.5. Installer Package

#### 2.5.1. Visual Studio Extension Development

If the following error occurs:

```text
D:\repo\SHFB\SHFB\Source\SandcastleInstaller\VisualStudioInstance.cs(28,17): error CS0234: The type or namespace name 'VisualStudio' does not exist in the namespace 'Microsoft' (are you missing an assembly reference?) [D:\repo\SHFB\SHFB\Source\SHFBProjectLauncher\SHFBProjectLauncher.csproj]
```

Ensure that the Visual Studio Extension Development components are installed. It
is observed that if both VS2022 and VS2026, that both installations needed the
extensions installed.

Rerunning the build new continues beyond the last error.

#### 2.5.2. WiX Toolchain

The project supports WiX 3.x. If building fails with:

```text
D:\repo\SHFB\SHFB\Source\Setup\Setup.wixproj(51,3): error MSB4019: The imported project "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Microsoft\WiX\v3.x\Wix.targets" was not found. Confirm that the expression in the Import declaration "$(WixTargetsPath)", which evaluated to "C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Microsoft\WiX\v3.x\Wix.targets", is correct, and that the file exists on disk.
```

## 3. Debugging

Once the `SHFBROOT` is setup, the deployment folder is built with debugging in
the previous step, it is now possible to debug from the solution level in debug
mode.

1. Clean the repository prior

```sh
D:\repo\SHFB> masterbuild.bat debug
```

Load the project `D:\repo\SHFB\SHFB\Source\SandcastleTools.slnx`.

Run `SandcastleBuilderGUI` where projects can be loaded and built, with single
breakpoints and single stepping. Ensure to clean and build in DEBUG mode prior.

If you get the *Just My Code Warning*, clean and rebuild for Debug mode. You can
still build your own projects in Release Mode.
