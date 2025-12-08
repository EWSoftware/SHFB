---
uid: 461BBB39-1C25-4B05-BD25-F56FDACF54A5
alt-uid: Glossary
title: Glossary
keywords: glossary
---
A | [B](#B) | [C](#C) | D | E | F | [G](#G) | H | I | J | K | L | [M](#M) | N | O | P | Q | [R](#R) | [S](#S) | [T](#T) | U | V | W | X | Y | Z


### B{address="B"}

BuildAssembler.exe
:   This is a tool supplied with Sandcastle that is used to build the help topics for conceptual and
    reference builds.  It is passed a configuration file containing a list of build components to use in transforming
    the topics into HTML and a manifest file that lists the topics to build.
    
    
See Also: [Sandcastle](#Sandcastle)


### C{address="C"}

Code Block Component{id="CodeBlockComponent"}
:   A custom build component that can be used to colorize code, add line numbering and/or collapsible
    section, and import code from working source code files.  It can be used in both conceptual and reference builds.
    The component is integrated with the <token>SHFB</token>:    and is also available as a standalone component from the
    same project site for use in other tools or your own build scripts.
    
    
See Also: [Sandcastle](#Sandcastle), [Sandcastle Help File Builder](#SHFB)

conceptual build{id="ConceptualBuild"}
:   A build that uses Sandcastle to produce help file topics using information extracted from
    conceptual content topics and their related files.
    
    
See Also: [reference build](#ReferenceBuild), [Sandcastle](#Sandcastle)

conceptual content{id="ConceptualContent"}
:   A topic file that contains conceptual content.  These can be used to add usage notes,
    walkthroughs, tutorials, etc. to a help file.  Conceptual topics use MAML rather than HTML to define their
    content.
    
    
See Also: [MAML](#MAML)


### G{address="G"}

Globally Unique Identifier, GUID
:   A unique value that is associated with each conceptual topic and image in a project.  When
    inserting links to topics or images, the ID is used to refer to them.  This allows you to alter the names or
    locations of the topic files without having to change the name or location in each topic that references
    them.
    
    


### M{address="M"}

MAML{id="MAML"}
:   An acronym that stands for Microsoft Assistance Markup Language.  Conceptual content topics are
    composed of MAML elements.
    
    
See Also: [conceptual content](#ConceptualContent)


### R{address="R"}

reference build{id="ReferenceBuild"}
:   A build that uses Sandcastle to produce help file topics using information extracted from managed
    assemblies and their associated XML comments files.
    
    
See Also: [conceptual build](#ConceptualBuild), [Sandcastle](#Sandcastle)


### S{address="S"}

Sandcastle{id="Sandcastle"}
:   Sandcastle is a set of tools originally produced by Microsoft that can be used to build help
    files for .NET managed libraries, conceptual content, or a mix of both.  Microsoft officially discontinued
    development in October 2012.  The Sandcastle tools have been merged with the <token>SHFB</token>:    project and are
    developed and supported there now as part of that project.
    
    
See Also: [Sandcastle Help File Builder](#SHFB)

Sandcastle Help File Builder, SHFB{id="SHFB"}
:   The <token>SHFB</token>:    is a standalone tool used to automate Sandcastle.  It consists of a GUI
    front end that helps you manage and build help file projects.  It uses a standard MSBuild format project file
    which can also be built from the command line using MSBuild or integrated into Visual Studio builds or other
    build scripts to produce a help file when your application projects are built.  In addition, it provides a set of
    additional features beyond those supplied with Sandcastle that can improve your help file and make it easier to
    deploy.  A Visual Studio integration package is also available for it that integrates the project management
    and build features into <token>VisualStudioMinVersion</token>:    or later.
    
    
See Also: [Sandcastle](#Sandcastle)


### T{address="T"}

token, token file
:   A token is used as a replaceable tag within a topic and is represented using a
    `token` element.  The inner text of the element is a token name.  The tokens are defined
    in a separate token file.  They are an easy way to represent common items that you use regularly such as a common
    phrase or external link.
    
    
