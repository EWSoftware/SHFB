System Requirements
-------------------
In order to use the Sandcastle Help File Builder (SHFB), you will need the following tools:

    Required:
        - .NET Framework 4.0 (Needed to run the SHFB tools)
        - Microsoft Sandcastle Documentation Compiler Tools (Latest release, required by the build engine)

    Optional:
        - HTML Help Workshop (Used for building Help 1 CHM files)
        - HTML Help 2 Compiler from the Visual Studio 2005 or 2008 SDK (Used for building Help 2 HxS files)

In order to build the source code, you will need the following tools:

    Required:
        - Visual Studio 2010 SP1 (Used to build the C# projects for the tools)

    Optional:
        - Visual Studio 2010 SDK SP1 (Required for VSPackage development)
        - VSPackage Builder Extension (Required by the VSPackage project)
        - Wix 3.x Toolset (Used to create the SHFB MSI installer).


Folder Layout
-------------
Deployment - This folder contains the deployment resources (the installer and all related files).  These are
used when creating the distribution package for SHFB.

Main\Source - This folder contains the source code for the core set of SHFB tools.  the .\Doc subfolder contains
the documentation projects.  The .\TestCaseProject subfolder contains a test project with various test cases and
test documentation projects.  The other folders are related to the various SHFB components and tools.


Running SHFB with the Source Code Version
-----------------------------------------
When you install SHFB, it creates a system environment variable called SHFBROOT that points to the release
version (typically C:\Program Files (x86)\EWSoftware\Sandcastle Help File Builder).  To use the source code
version of SHFB, you must edit your system environment variables to point SHFBROOT at the
.\Main\Source\SandcastleBuilderGUI\bin\Debug folder of the source code installation.  For example, set it to
C:\CP\TFS01\SHFB\Main\Source\SandcastleBuilderGUI\bin\Debug.  Your location may differ based on where you
extracted the code.  The layout of this folder mimics the release version and will allow you to run the source
code version of the tools while leaving the release build in place in the standard location.  You can freely
make changes to the tool source code and test them as needed.  Pointing SHFBROOT back at the standard location
for the release version of SHFB will let you run it to compare the results against your changed version.

Before using the source code version, you will need to build the tools.  To do this, open the
.\Main\Source\SandcastleBuilder.sln solution file and build it.  This will build the standalone GUI, the
build engine, and related files.  The .\Main\Source\SandcastleBuilderPackage.sln solution file can be used to
build the Visual Studio integration package.
