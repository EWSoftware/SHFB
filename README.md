Welcome to the **Sandcastle Help File Builder** project.

* [Get the current release](https://github.com/EWSoftware/SHFB/releases)
* [Ask a question or open an issue](https://github.com/EWSoftware/SHFB/issues)
* [Frequently Asked Questions (FAQ)](http://ewsoftware.github.io/SHFB/html/1aea789d-b226-4b39-b534-4c97c256fac8.htm)
* [Sandcastle Help File Builder Help](http://EWSoftware.github.io/SHFB)
* [Sandcastle MAML Guide](http://EWSoftware.github.io/MAMLGuide)
* [Sandcastle XML Comments Guide](http://EWSoftware.github.io/XMLCommentsGuide)

This project is composed of two separate parts that work together: the Sandcastle tools and the Sandcastle Help
File Builder.  The Sandcastle tools are used to create help files for managed class libraries containing both
conceptual and API reference topics.  API reference topics are created by combining the XML comments that are
embedded in your source code with the syntax and structure of the types which is acquired by reflecting against
the associated .NET Framework assemblies.  Conceptual topics are created by converting topic that you author
containing Markdown or Microsoft Assistance Markup Language (MAML).  The Sandcastle tools are command-line based
and have no GUI front-end, project management features, or an automated build process.

The Sandcastle Help File Builder was created to fill in the gaps, provide the missing NDoc-like features that
are used most often, and provide standalone GUI and command line based tools to build a help file in an
automated fashion.  A Visual Studio integration package is also available for it so that help projects can be
created and managed entirely from within Visual Studio.

Sandcastle was originally created by Microsoft back in 2006.  The last official release from Microsoft occurred
in June 2010.  Until October 2012, it was hosted at the Sandcastle project site on CodePlex.  In October 2012,
Microsoft officially declared that they were ceasing support and development of Sandcastle.  The Sandcastle
tools have been merged into the Sandcastle Help File Builder project and all future development and support for
them will be handled at this project site.

See the [Installation Instructions](http://EWSoftware.GitHub.io/SHFB/html/8c0c97d0-c968-4c15-9fe9-e8f3a443c50a.htm)
for information about the required set of additional tools that need to be installed, where to get them, and how
to make sure everything is configured correctly.  The guided installer also provides information on the
necessary tools and walks you through the installation steps.

If you are new to Sandcastle and the help file builder, see the topics in the
[Getting Started](http://EWSoftware.GitHub.io/SHFB/html/b772e00e-1705-4062-adb6-774826ce6700.htm) section to get
familiar with it, set up your projects to produce XML comments, and create a help file project.

See the [Project Wiki](https://github.com/EWSoftware/SHFB/wiki) for information on requirements for building the
code, contributing to the project, and links to other useful topics.
