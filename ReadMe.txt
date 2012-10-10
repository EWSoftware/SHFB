System Requirements
-------------------
In order to use Sandcastle and the Sandcastle Help File Builder (SHFB), you will need the following tools:

    Required:
        - .NET Framework 4.0 (Needed to run the Sandcastle and SHFB tools)

    Optional:
        - HTML Help Workshop (Used for building Help 1 CHM files)
        - HTML Help 2 Compiler from the Visual Studio 2005 or 2008 SDK (Used for building Help 2 HxS files)

In order to build the source code, you will need the following tools:

    Required:
        - Visual Studio 2010 SP1 (Used to build the C# projects for the tools)

    Optional:
        - Visual Studio 2010 SDK SP1 (Required for VSPackage development)
        - VSPackage Builder Extension (Required by the VSPackage project)
        - Wix 3.x Toolset (Used to create the MSI installers).


Folder Layout
-------------
Deployment - This folder contains the deployment resources (the installer and all related files).  These are
used when creating the distribution package for SHFB.

Documentation - This folder contains documentation projects for Sandcastle and SHFB as well as related documentation
projects for such things as the MAML Guide.

Extras - This folder contains optional projects that are not part of Sandcastle or SHFB.

Extras\HtmlToMaml - This folder contains a utility that can be used to convert HTML files to MAML topics.

Extras\SandcastleInstaller - This folder contains the source code for the guided installer application.

Extras\WebCodeProviders - This folder contains a code provider that can be used in website projects to produce XML
comments files for use by Sandcastle.

Sandcastle - The main Sandcastle folder.  The layout in here mimics the installed folder layout.  This makes it easy
to test and debug using the source code version of the Sandcastle tools and presentation styles (see below).

Sandcastle\Data - This folder contains the reflection data files for the core .NET Framework.  These are used during
the BuildAssembler step to insert information about the base .NET Framework class members.  A script is included
to rebuild this information as it is not included with the source code.  It is currently set to build using the
.NET Framework 4.0 assemblies.

Sandcastle\Examples - This folder contains various examples of using the Sandcastle tools by themselves.  Future
releases of Sandcastle will hopefully improve upon these examples.

Sandcastle\Presentation - This folder contains the presentation style files used during the BuildAssembler step to
apply formatting to the API and conceptual topics.

Sandcastle\ProductionTools - This folder contains the Sandcastle tools and their configuration files.

Sandcastle\ProductionTransforms - This folder contains various XSL transformations used during the build to alter the
reflection file data, produce build manifests, table of contents files, etc.

Sandcastle\Schemas - This folder contains the XML schemas for the reflection data file and the MAML topic files. Note
that Sandcastle does not strictly conform to the MAML schema.  Where necessary, it has been updated to reflect
these differences and to extend it to provide additional features that make using MAML with Sandcastle better.
The MAML schema files can be copied to the Visual Studio XML schema folder to provide IntelliSense within Visual
Studio for MAML topic files.

Sandcastle\Source - This folder contains the source code for the core set of Sandcastle tools.  The BuildAll.bat
script can be used to build all tools and place the output in the .\ProductionTools folder.  The individual
projects can be built and debugged here too.  They place their build output in the .\ProductionTools folder as
well and can be set up to run from there to make testing and debugging easier.

SHFB\Source - This folder contains the source code for the core set of SHFB tools.

TestCaseProject - This folder contains a test project with various test cases and test documentation projects.


Running Sandcastle with the Source Code Version
-----------------------------------------------
When you install Sandcastle, it creates a system environment variable called DXROOT that points to the release
version (typically C:\Program Files (x86)\Sandcastle).  To use the source code version of Sandcastle, you must
edit your system environment variables to point DXROOT at the .\Sandcastle folder of the source code
installation. For example, set it to C:\CP\TFS01\SHFB\Sandcastle\.  Your location may differ based on where
you extracted the code. This will allow you to run the source code version of the tools while leaving the
release build in place in the standard location.  You can freely make changes to the tool source code and
presentation style files and test them as needed.  Pointing DXROOT back at the standard location for the release
version of Sandcastle will let you run it to compare the results against your changed version.

Before using the source code version, you will need to build the tools and the reflection data files.  To do
this, open a command prompt, change into the .\Sandcastle\Source folder and run the BuildAll.bat script.  This will
build the tools and place them in the .\ProductionTools folder ready for use.  Once that completes, change into
the .\Sandcastle\Data folder, and execute the BuildReflectionData.bat script. This will generate the reflection data
files for the .NET Framework into a temporary folder.  Rename the folder it creates to "Reflection".


Running SHFB with the Source Code Version
-----------------------------------------
When you install SHFB, it creates a system environment variable called SHFBROOT that points to the release
version (typically C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder).  To use the source code
version of SHFB, you must edit your system environment variables to point SHFBROOT at the
.\SHFB\Source\SandcastleBuilderGUI\bin\Debug folder of the source code installation.  For example, set it to
C:\CP\TFS01\SHFB\SHFB\Source\SandcastleBuilderGUI\bin\Debug\.  Your location may differ based on where you
extracted the code.  The layout of this folder mimics the release version and will allow you to run the source
code version of the tools while leaving the release build in place in the standard location.  You can freely
make changes to the tool source code and test them as needed.  Pointing SHFBROOT back at the standard location
for the release version of SHFB will let you run it to compare the results against your changed version.

Before using the source code version, you will need to build the tools.  To do this, open the
.\SHFB\Source\SandcastleBuilder.sln solution file and build it.  This will build the standalone GUI, the
build engine, and related files.  The .\SHFB\Source\SandcastleBuilderPackage.sln solution file can be used to
build the Visual Studio integration package.
