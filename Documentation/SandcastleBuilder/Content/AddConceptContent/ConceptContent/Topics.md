---
uid: 4b8ab701-2321-4d24-a287-8848de086f68
alt-uid: Topics
title: Topic Files
keywords: "conceptual content, topic files"
---
Topic files contain Markdown or Microsoft Assistance Markup Language (MAML) to define non-reference content such
as usage notes, walkthroughs, tutorials, etc.  Markdown topic files have a *\*.md* extension and MAML topic files
have a *\*.aml* extension.  Topic files should have their `BuildAction` set to `None` in the Project Explorer.
A [content layout file](@54e3dc97-5125-441e-8e84-7f9303e95f26) is used to determine which topics are included in
the compiled help file and sets their layout in the table of contents.

<autoOutline lead="none" excludeRelatedTopics="true" />

## Content Layout Properties
Each topic is assigned a unique ID that can be referenced in topics using a Markdown link (`[](@TopicId)`) or a
MAML `link` element.  Changes can be made to the topic properties such as the filename associated with the ID and
its title text without having to visit each topic in which it is referenced.  When creating new topics using the
help file builder, templates are used and a new ID is assigned automatically but you can change it as you see
fit.  The content layout editor tracks topics by ID rather than by filename.  As such, you can rearrange and
rename the files in the project without being concerned about updating references to them in the content layout
files.  As long as the ID remains the same, the same topic will be associated with the entry
regardless of where it is moved or what it is named.

> [!NOTE]
> The value of the ID assigned to a topic should not be changed.  If it is, you will be required to manually
> locate and update the old ID in all topics that reference it.

Within the content layout editor, each topic contains several properties which are described below.

ID and Topic Filename
:   These read-only properties display the ID of the topic and the project file related to the ID.  The three
    buttons to the right of the currently associated topic filename let you do the following:
    
    - **Associate Topic** - The layout file tracks topics by their unique ID rather than by filename.  This
      button allows you to associate a different topic file with a content layout file entry or add an
      association to an empty container node.
    - **Clear Topic Association** - This button allows you to clear the topic file associated with a content
      layout file entry converting it into an empty container node.  The node will not have a topic associated
      with it in the compiled help file nor will it display a topic when selected in the table of contents.  It
      simply serves as a container for other topics and must have at least one child topic defined for it.
      
      > [!NOTE]
      > The Microsoft Help Viewer file format does not support empty container nodes.  All entries must have an
      > associated topic.  If one is not specified, an empty topic will be generated at build time so that a
      > valid help file is produced.
      
    - **Refresh Associations** - If you move one or more files around or rename them in the Project Explorer
      or change a topic's unique ID, the currently open content layout file editor will not be aware of these
      changes.  Use this button after making such changes so that the content layout editor refreshes the files
      associated with each topic.  This allows you to open them for editing from the layout editor after moving
      or renaming them without closing and reopening the layout editor.  This option refreshes the entire
      collection of topics, not just the one currently being viewed.

Title
:   This is used as the title for the topic.  In addition, it will be used as the table of contents title unless
    a **TOC Title** value is specified.  It will also be used as the link text unless a **Link Text** value is
    specified.  If left blank, the topic filename is used as a default title.  The title is only editable for
    MAML topics.  Markdown topics store the title within the topic file itself and it must be edited there.

TOC Title
:   This is used to specify an optional table of contents title for a topic.  If not set, the **Title** value
    will be used. In some cases, the title may not be appropriate for the table of contents or is too long to be
    practical.  In such cases you can specify shorter or more appropriate text for the table of contents via this
    property.  The TOC title is only editable for MAML topics.  Markdown topics store the TOC title within the
    topic file itself and it must be edited there.

Link Text
:   This property can be used to specify an alternate value to use as the inner text for links.  If not set, the
    **Title** value will be used.  In some cases the title may not be appropriate or may be too long to be
    practical for links that refer to the topic.  In such cases you can specify shorter or more appropriate text
    for the link via this property.  This saves you from having to specify the shorter inner text on each link
    that uses the topic ID.  The link text is only editable for MAML topics.  Markdown topics store the link text
    within the topic file itself and it must be edited there.

Show in the table of contents
:   This allows you to specify whether or not a topic will appear in the table of contents.  The default is
    checked to show it.  If unchecked, the topic will be included in the help file but will only be accessible
    via links from other topics.  This is a convenient way of adding content to the help file but limiting the
    amount of information displayed in the table of contents.  A hollow bullet point appears next to invisible
    topics in the content layout tree to note their status.  Making a parent topic invisible will also remove its
    child topics from the table of contents.

Use as the default topic
:   This will mark the selected topic as the default topic for the help file.  The default topic option acts as a
    toggle.  Check it on a topic to set it as the default.  If another topic was previously marked as the default
    topic, the option is removed from it.  Unchecking it on the same topic turns it off and no topic will be
    selected as the default topic.  In such cases, the root namespaces page will be the default topic for the
    help file.  When set, a green circle icon with a check mark appears next to the topic in the content layout
    tree to indicate its status.

    > [!NOTE]
    > This option only has an effect on Help 1, markdown, and website output.  MS Help Viewer does not support
    > the specification of a default topic.

Use as MS Help Viewer root container
:   This option is useful if you are generating multiple help file formats from one project file including MS
    Help Viewer output.  By default, all topics in the MS Help Viewer output are listed as root elements in their
    parent topic.  If you would prefer them to be grouped under a common root container topic, add a topic to the
    content layout file and use this option to mark it as the root container.  The root container must have its
    **Show in the table of contents** option unchecked so that it is not included in the table of contents.  This
    prevents it from appearing in the other help file formats.  It will be added automatically to the MS Help
    Viewer output when that format is built.  If you want a root container to appear in all formats, simply add
    it as a normal topic in the content layout file and manually add all of the other topics to it as sub-topics.
    This option acts as a toggle.  Check it on a topic to set it as the container.  If another topic was
    previously marked as the container topic, the option is removed from it.  For more information, see the
    [](@ba42b2c2-985d-46f1-ba4c-3d921edcafe3) topic.  When set, a file tree icon appears next to the topic in the
    content layout tree to indicate its status.

API Content
:   This is used to indicate how the API (reference) content for the help file should be inserted relative to the
    selected topic.  The following options are available:

    - **None** - This topic will not be used as the API content insertion point.
    - **Insert after this topic** - The API content will be inserted immediately after this topic in the table of
      contents.
    - **Insert before this topic** - The API content will be inserted immediately before this topic in the table
      of contents.
    - **Insert as child of this topic** - The API content topics are added as sub-topics of the selected topic.
      If the selected topic already contains sub-topics, the API topics will appear after them.
    
    This option acts as a toggle.  Select a value other than **None** to make it the API content insertion point.
    If another topic was previously marked as the API content insertion point, the option is removed from it.  An
    icon will appear next to the topic in the content layout tree to indicate its API content insertion point
    state.  This option can be combined with the default topic option.  If no topic is set as the API content
    insertion point, the API content will be added to the table of contents following the standard
    [content placement rules](@ba42b2c2-985d-46f1-ba4c-3d921edcafe3).

Index Keywords
:   This grid allows you to add `MSHelp:Keyword` attributes to the help topic to define its index keywords.  In
    the grid for this property, select the index and enter the terms for it.  Keyword terms can contain
    [substitution tags](@69d998a7-1af5-4bf5-889b-59cd00b64000) which will be converted to their appropriate value
    at build time.  To delete a keyword, select it in the grid and hit the **Delete** key.

    The most common index used in the "K" index (keywords).  The terms consist of one word or a phrase.  If you
    separate two terms with a comma, the first term becomes a group in the index.  If that same grouping term
    appears in other topics, the term that follows the comma in each of them will appear as a child of the
    grouping term in the help file's index.  If your intent is to add two terms to the same index but have them
    appear as two distinct entries, enter them on two separate rows in the grid.

    Keywords are only editable for MAML topics.  Markdown topics store the keywords within the topic file itself
    and they must be edited there.

    > [!TIP]
    > If a topic is marked as the default using the designer, entries for the `NamedUrlIndex` terms `DefaultPage`
    > and `HomePage` are added to it automatically at build time so there is no need to add them via this
    > property.

    > [!NOTE]
    > Keywords will only be added to Markdown and MAML topics, not HTML files.

## Topic Filenames and Locations
Topic files can be named however you like and can reside in any folder within the project.  As such, you can
arrange them on disk to match the layout in the table of contents and give them meaningful names to aid
in managing them outside of the project and the designer.  When the help file is built, all topic files are
copied to the help file's *.\\html* folder and are named using their unique ID with a *.htm* extension.  You can
use that name to link to the topic from other HTML pages.  For example:

``` xml{title="HTML Link Examples"}
Link to a topic from an HTML file in the .\html folder:
<a href="303c996a-2911-4c08-b492-6496c82b3edb.htm">Test Topic</a>

Link to a topic from the root folder:
<a href="html/303c996a-2911-4c08-b492-6496c82b3edb.htm">Test Topic</a>

Link to a topic from another folder:
<a href="../html/303c996a-2911-4c08-b492-6496c82b3edb.htm">Test Topic</a>
```

For XML comments, the preferred approach is to use the `conceptualLink` element to link to the topic ID.  When
used inline within another element, it creates an inline link.  When used at the top-level like a summary or
remarks element, it adds an entry to the See Also section of the API topic.  For example:

``` xml{title="XML Comments Examples"}
/// <remarks>
/// See the <conceptualLink target="303c996a-2911-4c08-b492-6496c82b3edb" /> topic for more information.
/// </remarks>

/// <remarks>
/// More information can be found <conceptualLink target="303c996a-2911-4c08-b492-6496c82b3edb">
/// in this topic</conceptualLink>
/// </remarks>

/// <remarks>
/// This example adds an entry to the See Also section of the topic
/// </remarks>
/// <conceptualLink target="303c996a-2911-4c08-b492-6496c82b3edb" />
```

## See Also
**Other Resources**  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)  
[](@54e3dc97-5125-441e-8e84-7f9303e95f26)  
