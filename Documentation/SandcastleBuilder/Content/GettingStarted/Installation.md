---
uid: 8c0c97d0-c968-4c15-9fe9-e8f3a443c50a
alt-uid: Installation
title: Installation Instructions
keywords: "Getting started, installation", installation, prerequisites, requirements
---
This topic describes the additional tools that are required to use the Sandcastle Help File Builder along with
some general configuration information.

<autoOutline lead="none" />

## Guided Installation
Due to the number of products that need to be installed to get a working build environment, a guided installation
package has been created to simplify the process.  The guided installer contains most of the necessary parts and
various optional components.  For those parts that cannot be distributed such as the help compilers, it contains
instructions on where to get them and how to install them if you need them.

[Download the latest Sandcastle Help File Builder Guided Installer](https://GitHub.com/EWSoftware/SHFB/releases)

When using the guided installation, you can skip the **Requirements** section below as it takes care of all of
the required steps.  See the **Recommended Additions** sections for additional tools not included in the guided
installation that you may find useful.

The help file builder tools and the Visual Studio package can be installed manually by running the MSI and VSIX
installers found in the *.\InstallResources* folder extracted from the guided installer download.  If installing
the tools manually, the optional components such as the snippets will not be installed.  You will also need to
ensure the requirements noted below are also present if needed.

## Requirements
The latest version of the Sandcastle Help File Builder and Tools can always be found at the <token>SHFB</token>
project site (<token>SandcastleVersion</token> when this help file was produced).

- [.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework) or later is required to run
  the desktop tools and build projects with MSBuild.
- [.NET 9.0](https://dotnet.microsoft.com/download) or later is required to build projects with *dotnet.exe*.
- If you are using the standalone GUI to build help file projects, you will need to install the
  [Microsoft Build Tools for Visual Studio 2022](https://visualstudio.microsoft.com/downloads/).  If you are
  using <token>VisualStudioMinVersion</token> or later to build help file projects, there is no need to install
  the build tools separately.  The version of MSBuild deployed with Visual Studio will work as expected from the
  command line or within Visual Studio.

> [!NOTE]
> The underlying Sandcastle tools were originally created by Microsoft.  The help file builder uses them to
> produce the help file topics.  Microsoft officially discontinued development in October 2012.  The Sandcastle
> tools have been merged with the <token>SHFB</token> project and are developed and supported there now as part
> of that project.

In order to use the help file builder, the following additional tools are required based on the types of help
files that you want to produce.  Each must be installed prior to building a help file of that particular type:

- The [HTML Help Workshop](https://docs.microsoft.com/previous-versions/windows/desktop/htmlhelp/microsoft-html-help-downloads)
  for building HTML Help 1 (.chm) help files.  The Microsoft download link appears to have been discontinued.  If
  you need a copy to install, it can be downloaded from the
  [Sandcastle Help File Builder project website](https://github.com/EWSoftware/SHFB/raw/master/ThirdPartyTools/htmlhelp.exe "HTML Help Workshop download")
- The Microsoft Help Library Viewer for installing and viewing Microsoft Help Viewer (.mshc) help files.  These
  are installed as part of Visual Studio.

The additional tools can be installed in any order.

> [!NOTE]
> You may need to reboot the system in order for any environment variable changes to take effect.

## Recommended Additions
The following tools are optional but you may find them useful:

- If you need a spell checker for Visual Studio, the **Spell Check My Code** extension can be downloaded and
  installed from the Visual Studio Gallery.  It is an editor extension that checks the spelling of comments,
  strings, and plain text as you type or interactively with a tool window.  Support is included for spell
  checking source code, XML files, and MAML topic files.  Versions are available for 
  [Visual Studio 2013 and 2015](https://marketplace.visualstudio.com/items?itemName=EWoodruff.VisualStudioSpellChecker "Spell Check My Code (VS2013/2015)"), 
  [Visual Studio 2017 and 2019](https://marketplace.visualstudio.com/items?itemName=EWoodruff.VisualStudioSpellCheckerVS2017andLater "Spell Check My Code (VS2017/2019)") and
  [Visual Studio 2022 and Later](https://marketplace.visualstudio.com/items?itemName=EWoodruff.VisualStudioSpellCheckerVS2022andLater "Spell Check My Code (VS2022+)").
- For extended XML comments completion support in Visual Studio 2019 and later, install the **Extended XML
  Documentation Comments Completion Provider** extension which can be downloaded and installed from the Visual
  Studio Gallery.  Versions are available for 
  [Visual Studio 2019](https://marketplace.visualstudio.com/items?itemName=EWoodruff.ExtendedDocCommentsProvider "Extended XML Documentation Comments Completion Provider (VS2019)") and 
  [Visual Studio 2022 and later](https://marketplace.visualstudio.com/items?itemName=EWoodruff.ExtendedDocCommentsProvider2022 "Extended XML Documentation Comments Completion Provider (VS2022+)").
- For a Visual Studio add-in used to create XML comments automatically, check out SubMain's
  [GhostDoc](https://submain.com/products/ghostdoc.aspx).

## See Also
**Other Resources**  
[](@b772e00e-1705-4062-adb6-774826ce6700)  
[](@c18d6cb6-e4e4-4944-84ee-f867aa6cfb0d)  
[](@8e3f8757-0ef3-4772-bb2f-5d7ae57e50da)  
