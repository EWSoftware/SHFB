---
uid: 3d4edd2d-7883-4508-b9d2-bd7b4d848b0d
alt-uid: ConceptContent
title: Conceptual Content
keywords: "conceptual content, overview"
---
Conceptual content is similar in nature to the additional content used in the help file builder prior to version
1.7.0.0.  It allows you to define non-API documentation that is included in the help file for usage notes,
walkthroughs, tutorials, etc.  Instead of HTML, conceptual content topics use Markdown or Microsoft Assistance
Markup Language (MAML).  There is no layout or style information within the Markdown or MAML files. Instead, they
are ran through a presentation style transformation using Sandcastle's **BuildAssembler** tool similar to the
reference (API) content so that they match it in appearance and features.  This allows you to utilize the various
build components to alter and extend the presentation style of the topics.

> [!TIP]
> Starting with version 2025.12.18.0, Markdown is the preferred conceptual topic format.  An option to convert
> MAML topics to Markdown is available in the [](@54e3dc97-5125-441e-8e84-7f9303e95f26).
>
> A guide to using MAML with Sandcastle is
> [available online](https://EWSoftware.GitHub.io/MAMLGuide "Sandcastle MAML Guide").  If you need to convert
> older HTML topics to MAML topics, the
> [HTML to MAML Converter](https://EWSoftware.GitHub.io/HtmlToMamlDocs "HTML to MAML Converter") is available
> to help you.

## Conceptual Content Files
Conceptual content is composed of several different file types within a project.  You can use folders within the
project to group these files as you see fit.

- [Content layout files](@54e3dc97-5125-441e-8e84-7f9303e95f26) define which topics are included in the help file
  and how they are arranged in the help file's table of contents.  Content layout files have a `BuildAction` of
  `ContentLayout`.
- [Topic files](@4b8ab701-2321-4d24-a287-8848de086f68) contain the Markdown or MAML that defines each topic.
  These files should have a `BuildAction` of `None`.  The build process handles converting the topics to HTML
  and adding them to the help file based on a content layout file.
- [Image files](@c38461a6-6edd-42cf-9d91-73c4b11cdd70#Image) define image resources used in the conceptual
  content.  These have a `BuildAction` of `Image`.
- [Token files](@ed6870bb-772d-4596-9fc1-5638ae6d621b) allow the definition of common Markdown or MAML snippets
  or content used in topic files.  These have a `BuildAction` of `Tokens`.
- [Code snippets files](@c38461a6-6edd-42cf-9d91-73c4b11cdd70#CodeSnippets) allow you to define code snippets
  that will be used in conceptual content topics and are rendered by Sandcastle's `ExampleComponent`.  These
  have a `BuildAction` of `CodeSnippets`.  See the MAML Guide for more information on creating code snippets
  files.  These are rarely used anymore, if ever, since code blocks defined within the Markdown or MAML topics
  are capable of importing code snippets directly from source code files.

## Custom Template Files
In the standalone GUI, the designer uses template files to create new conceptual topics.  You can create your own
templates for use in the designer by creating a *Conceptual Templates* folder in the **Local Application Data**
folder.  Any Markdown (*\*.md*) or MAML (*\*.aml*) files found in this folder will be added to the **Custom
Templates** menu option in the **Add Sibling Topic** and **Add Child Topic** submenus.  See the
[](@a24489fb-45d6-46f4-9eaa-9a9c4e0919b2#FileTemplates) topic for more information.

See the [](@ece3a395-589f-45c3-9f0e-2a25b8b6c537) topic for information on creating file templates for use with
content layout file editor in the Visual Studio package.

## See Also
**Other Resources**  
[](@5292ce5c-fda1-4a77-9155-a11755ef1730)  
