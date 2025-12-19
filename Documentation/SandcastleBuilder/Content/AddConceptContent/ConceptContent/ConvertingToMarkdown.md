---
uid: ConvertingToMarkdown
title: Converting MAML Topics To Markdown
---
The help file builder supports a mix of Markdown and MAML topics in the same project.  As such, it is not
necessary to convert MAML topics to Markdown all at once.  You can convert topics as needed when you make changes
to them.  To do so, open the content layout file, right click on a MAML topic, and select the **Convert to
Markdown** option.

## Conversion Options
When selected, a dialog box appears with the following options:

Use filenames for unique IDs
:   This option is unchecked by default.  If checked, it will use the topic filenames for unique IDs rather than
    the existing topic IDs.  Leave this option unchecked to keep using the existing topic IDs (usually GUIDs) for
    help output filenames to maintain links to existing topics in previously published content.  The filename
    will be added to the topic as an alternate ID so that you can use it or the existing ID in topic links in 
    any new content that you add.

    If checked, the existing topic IDs will be added to the converted files as alternate IDs.  This allows
    existing links in any other topics to continue working while using the friendlier filename ID for any new
    links that you add to topics and for the filenames in the help output.  When the help file is built, links
    to the alternate IDs will be converted to the topic IDs in the help content.

Topics to convert: Only the selected topic
:   This is the default option.  It will only convert the selected topic to Markdown.

Topics to convert: Only the selected topic and its children recursively
:   This option will convert the selected topic and all child topics recursively.  This is useful if you are
    making changes to a related set of topics.  Any topics that have already been converted are ignored.

Topics to convert: The selected topic and its siblings (no sub-topics)
:   This option will convert the selected topic and all sibling topics at the same level before and after it.
    Sub-topics of the converted topics are not changed.  This is useful if you are making changes to a related
    set of topics.  Any topics that have already been converted are ignored.

Topics to convert: All topics
:   This option will convert all MAML topics in the content layout file to Markdown.  Any topics that have
    already been converted are ignored.

Click the **OK** button to convert the topics.  A confirmation message box will appear telling you how many
topics will be converted and gives you the option to cancel.  If converted, the existing MAML topics will be
replaced with a like-named file with a *.md* extension.  The new file will be added to the project.  The existing
MAML topic is left in the project but is renamed with a *.aml.bak* extension.  You can delete these backup files
after confirming that the conversion was successful and that they are no longer needed.

Unlike MAML topics, Markdown topics store their titles and keywords in the file itself.  Therefore, after
conversion, those properties are removed from the content layout file and the related fields will be read-only in
the editor.  Only the properties directly related to the content layout file such as whether or not the topic
will appear, its status as the default topic, and the API content placement option remain editable.

## Common Conversion Issues
While the conversion process does its best to convert MAML topics to Markdown, there are some cases that can only
be addressed when reviewing the topics.  You can use the [](@d3c7584d-73c0-4725-87f8-51e4ad956694) to view the
topics and look for issues.  If the issue causes parsing the topic to fail, a red bordered error will appear at
the top of the preview with some information about the issue.  Bear in mind that the error may refer to elements
that do not exist in the actual Markdown file as the content it is rendering is converted to MAML behind the
scenes.  Any content that could be rendered appears below the error.  You can usually use it to find the point of
failure.  In most cases, it is caused by an incorrect indent or ill-formed content.  Below are some common issues
and how to fix them.  

- While the converter will attempt to left-justify the plain text as often as possible, it may not always do it
  for all lines in a block.  It will also not rewrap the text so as to preserve any preferred formatting you may
  have used.  This does not typically affect the rendered topics and you can reformat the text as needed when
  editing the topics.

- On a similar note, when text is left justified, certain special characters like dashes used to separate text
  may appear in the left-most column and be interpreted as the start of a Markdown list.  You will need to
  reformat the line to move the dash or escape it so that it is used as you intended.

- The converter introduces extra line breaks to ensure proper formatting of the Markdown and to avoid some common
  issues caused by Markdown elements being combined incorrectly or not being interpreted properly after
  conversion.  These extra line breaks do not typically affect the rendered content and can be removed as you
  clean up the topics.

- Special characters used for Markdown formatting such as asterisks, back ticks, and underscores may need to be
  escaped in some cases.

- If you made use of the MAML `markup` element, it can generally be removed unless you are using it to wrap Open
  XML specific content.  Much of the HTML used in the `markup` element can likely be converted to the equivalent
  Markdown text or left as-is within the topic without the `markup` element.

- The converter will attempt to use the Markdown format for general lists, definition lists, tables, and alerts.
  However, in order to preserve the original formatting as much as possible, it will only do so if it can
  determine that the content only consists of inline elements or paragraphs.  For more complex content such as
  when nested lists, tables, or alerts are present, it will convert the inner content when possible but leave
  them wrapped in the containing MAML elements.  These typically render without issue and you can decide whether
  or not to convert the content to the equivalent Markdown when reviewing the topic.

- MAML tables with simple inline content or single paragraphs that do not span lines will typically be converted
  to Markdown pipe tables.  MAML tables with more complex nested content or multi-line paragraphs will be
  converted to HTML tables.  Content within the cells will be converted to Markdown if possible by using the
  extended capabilities provided by the help file builder.  See the [](@MarkdownSupport) topic for more
  information.

- In MAML topics, heading levels are fixed at level 1 (`h1`) for the topic title rendered by the presentation
  style, level 2 (`h2`) for section headings, and level 4 (`h4`) for sub-section headings regardless of depth.
  The converter will use those heading levels for the Markdown topic.  Unlike MAML, you are free to use whatever
  heading levels you like and can change them as you see fit when reviewing the topic.  Bear in mind that the
  topic title is rendered by the presentation style and will always be a level 1 (`h1`) heading so you may want
  to use heading levels of 2 or greater for the topic content.

- Since the presentation style is no longer in control of the section heading levels, if the collapsible sections
  option is enabled, every section including sub-sections will be rendered as collapsible sections.  You may
  wish to turn this option off in the Transformation Arguments project properties.

- See the [](@MarkdownSupport) topic for information on some common Markdown equivalents to MAML elements and for
  information on how you can enable rendering of Markdown content within MAML or HTML elements in the topic which
  is typically not supported by standard Markdown files.

## See Also
**Other Resources**  
[](@5292ce5c-fda1-4a77-9155-a11755ef1730)  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)
