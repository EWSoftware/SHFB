---
uid: 5292ce5c-fda1-4a77-9155-a11755ef1730
alt-uid: AddConceptContent
title: Conceptual and Additional Content
keywords: "additional content, general overview", "conceptual content, general overview"
---
In versions of the help file builder prior to version 1.7.0.0, the additional content properties were used to add
files to the project to define additional help content that would appear in the table of contents and to add
other items such as image files and additional style sheets.  As an alternative to basic HTML files, you could
also create XML files with a *.topic* extension that would be ran through an XSL transformation to give them the
basic look and feel of the presentation style that was selected.  However, using the basic transformations, the
topics lacked several features present in the reference content such as collapsible sections and the language
filter and they could not take advantage of build components to resolve links to reference and online content.

Starting with version 1.7.0.0, the help file builder supported conceptual content.  This is similar in nature to
additional content in that it allows you to add non-reference content to the help file that will appear in the
table of contents.  However, instead of HTML files or *.topic* files that contained HTML, conceptual content used
topic files that contained Microsoft Assistance Markup Language (MAML).  This is basically XML conforming to a
well defined schema that describes the structure of the conceptual content much like XML comments describe the
structure of the code comments.

Starting with version 2025.12.18.0, the help file builder supports using Markdown files in place of MAML.  This
allows for editing topics in a much easier manner and allows mixing in HTML and/or MAML elements as needed when
Markdown doesn't provide the needed capabilities.  Since most people are more familiar with Markdown than MAML,
this is now the preferred approach and is the default topic format for all new projects going forward.

There is no layout or style information within the Markdown or MAML topic files.  Instead, they are ran through a
presentation style transformation using Sandcastle's **BuildAssembler** tool similar to the reference (API)
content so that they match it in appearance and features.  Whether you use Markdown or MAML, the presentation
style transformations are responsible for applying the style and formatting of the rendered topics that appear in
the help file.  The look of the conceptual topics is much more consistent with that of the reference topics. As
such, preference has been given to conceptual content when adding features to the help file builder.  Support for
the older additional content model has been reduced to simple inclusion of the files.

## See Also
**Other Resources**  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)  
[](@ba42b2c2-985d-46f1-ba4c-3d921edcafe3)  
[](@4fd3b2b6-dfad-4513-983b-5e74d2342ff0)  
[](@MarkdownSupport)  
[](@ConvertingToMarkdown)
