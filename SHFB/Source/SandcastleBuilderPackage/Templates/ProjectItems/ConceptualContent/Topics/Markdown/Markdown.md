---
uid: $safeitemname$
title: $itemname$
# tocTitle: Optional Title for the Table of Contents
# linkText: Optional Text to Use For Links
# keywords: keyword, term 1, term 2, "term, with comma"
# alt-uid: optional-alternate-id
# summary: Optional summary abstract
---
This is an optional introduction section.  Any text placed before the first heading will be put into an
introduction section in the generated topic.  This is typically one or two paragraphs that give brief overview
of the topic.

<!-- Comments like this one will not appear in the generated topic.
If uncommented, this will generate an auto-outline in presentation styles that don't automatically create one
such as the VS2013 and Open XML presentation styles.
<autoOutline /> -->

## YAML Front Matter Block
The section at the very top of the topic is a YAML Front Matter block.  It should appear at the very top of the
file, start with a line with three dashes, and end with a line with three dashes as shown.  Lines preceded by a
pound sign (#) within it are comments and will be ignored.  If included, you can use it to define the following
topic metadata:

- **uid** - A unique ID for the topic.  If omitted, a unique ID will be generated using the first heading title.
  If there are no heading titles, the filename is used.
- **title** - The title to use for the document.  If not specified, preference is given to the table of contents
  title, link text title, the first heading in the topic, or the topic filename as a last resort.
- **tocTitle** - An optional title to use for the table of contents.  This allows you to specify a shorter title
  for the table of contents if necessary.  If omitted, the title is used.
- **linkText** - Optional text to use for links to the topic.  This allows you to specify a shorter title for
  links if necessary.  If omitted, the title is used.
- **keywords** - This can be used to specify an optional comma-separated list of keywords used to index or
  otherwise categorize the topic.  If the term contains a comma, enclose it in quotes.  This currently only has
  use for the Help 1 and MS Help Viewer output formats.  It can typically be omitted if using a presentation
  style that generates the website or Open XML output format.
- **alt-uid** - This key is only used for backward compatibility with converted MAML topics.  It provides a means
  of linking to the topic using the older style GUID unique IDs from MAML topics if you switched to using
  filenames for the topic unique IDs during conversion.  If that is not the case, it can be omitted.
- **summary**: An optional summary for the topic.  If specified, this will be placed in the generated topic's
  `Description` metadata element for website output.  If not specified, the text from the first paragraph is used
  instead.

> [!NOTE]
> If you want to use any of the optional metadata items, be sure to uncomment them or they will be ignored.

## Section title
Add one or more top-level sections.  If titled, each section will be given an automatic ID created from the title
text.  The title will be used in the auto-outline if one is included.

Topic text can contain any mix of Markdown, HTML, and MAML elements.  HTML and MAML elements give you more
control over the structure of the document and may be of use in certain situations such as tables with complex
content such as nested tables, lists, images, etc.

Unlike standard Markdown files, those processed by the help file builder will convert Markdown within HTML
elements and MAML elements to their HTML equivalents.  As such, you can use HTML or MAML elements for higher
level structures but continue to use Markdown for simpler formatting within such elements.

> [!TIP]
> The Markdown file editor in Visual Studio will not be able to handle the help file builder specific elements
> and the topic may not render correctly in its preview pane.  Use the help file builder's topic previewer tool
> window to see an approximate representation of the topic as it will appear in the generated help file along
> with a table of contents pane that can be used to navigate and preview all of the other topics in the project.

### Sub-section Title
Any sections with a heading level greater than the prior one will be nested as sub-sections of the parent
section.

## The Last Section
The final section of a topic is typically the **See Also** section as shown below.  It usually does not contain
any text, just a list of links to API members, external links, and other topics as shown in the examples below.
Sub-headings can be used to group the different link types if wanted.

> [!NOTE]
> Put two spaces at the end of each sub-heading and link in the See Also section so that line breaks are
> generated rather than paragraphs for each link.  Another option is to use a list.

## See Also
**Reference**  
[FileStream on Microsoft Learn](@T:System.IO.FileStream)  
[](@M:System.IO.FileStream.#ctor(System.String,System.IO.FileMode)){show-container="true" show-parameters="true"}  
[](@M:System.IO.FileStream.Write(System.Byte[],System.Int32,System.Int32)){show-container="true" show-parameters="true" prefer-overload="true"}  
[](@M:System.IO.FileStream.Flush(System.Boolean)){show-container="true" show-parameters="true"}  
[](@P:System.IO.FileStream.Length)  
[](@M:System.IO.FileStream.Flush){prefer-overload="true"}

**Concepts**  
[](@link-to-other-conceptual-topic-1)  
[](@link-to-other-conceptual-topic-2)

**Other Resources**  
[Sandcastle Help File Builder Documentation](https://ewsoftware.github.io/SHFB/html/bd1ddb51-1c4f-434f-bb1a-ce2135d3a909.htm)  
[Sandcastle Help File Builder GitHub Repository](https://github.com/EWSoftware/SHFB)
