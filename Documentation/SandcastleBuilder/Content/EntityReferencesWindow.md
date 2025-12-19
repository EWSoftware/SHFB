---
uid: e49eea91-a9ef-4aa5-ad8f-16ebd61b798a
alt-uid: EntityReferencesWindow
title: Entity References Window
keywords: Entity References window
---
The **Entity References** window allows you to insert Markdown and MAML elements into conceptual content topics
that represent tokens, images, code entities (API members), code snippets, and links to other conceptual topics.
To open it in the standalone GUI, select the **Entity References** option on the **Window** menu, click the
related main toolbar button, or hit **F3**.  In the Visual Studio extension, it can be opened by selecting the
**Entity References Window** option from the **View** | **Other Windows** menu or from the **Other Windows**
toolbar button dropdown.

^^^{placement="center"}
![](@EntityReferences)
^^^

The dropdown in the toolbar allows you to select which type of reference to insert.  The first toolbar button,
**Refresh**, allows you to refresh the selected references if you make changes to the project such as adding a
new image or token file.  The refresh option will take into account unsaved changes made to open token, content
layout, and site map files in the environment so that current information is always available without having to
save your changes frequently.  The second toolbar button, **Copy**, will copy the currently selected entity to
the clipboard in the selected format ready for pasting into a topic file.  The **Help** button at the end of the
toolbar will bring up this help topic.

The **Find** textbox allows you to search for an item by ID or display text.  Tokens, images, code snippets,
and table of contents types perform a case-insensitive search for the entered text anywhere within the entity's
ID or display title value.  All items except the table of contents entities show their ID in the entity
references tool window.  Table of contents items show their display title but the search will look at their ID
for a match too.  For code entities, a regular expression search is performed using the entered text to match
all member IDs found in the project's XML comments files and the .NET Framework comments files.  Hit Enter or
click the **Go** button to start the search.  Hit Enter or click **Go** again to find subsequent matches.

To insert a reference from the list into a topic, either drag and drop it into the topic or use the **Copy**
option (Ctrl+C) to copy it to the clipboard and then paste it into the topic.  The references that can be
inserted are as follows:

Tokens
:   When this is selected, a list of all tokens grouped by their containing token file is shown.  In order to be
    listed, the file must have a `BuildAction` of `Tokens`.  When inserted into a topic, the selected ID is
    wrapped in a MAML `token` element.  These will also work in Markdown topics.

Images
:   When this is selected, a list of all conceptual content image files is shown.  To appear, the file must have
    a `BuildAction` of `Image`.  This entity type offers a choice of formats.  For Markdown topics, select
    either **Markdown image link** or **HTML image link** depending on the type of link that you want to create.
    For MAML conceptual topics, choose either **MAML media link**, **MAML inline medial link**, or
    **MAML external link** depending on the type of link that you want to create.  For HTML topics, choose the
    **HTML image link** option.  The HTML option is for backward compatibility with the older additional content
    model and should not be used for Markdown and MAML conceptual content topics.

Table of Contents
:   When this is selected, all content layout and site map files are merged in the order defined in their project
    settings to produce a table of content list that can be used for inserting links to the topics.  For their
    topics to appear, content layout files must have a `BuildAction` of `ContentLayout` and site map files must
    have a `BuildAction` of `SiteMap`.  The expanded/collapsed and the last selected topic state is stored within
    each file.  As such, the topics will appear in the entity references window in the state you last used them
    and the last selected topic in the file will be selected as the default item in the tool window.  This is
    helpful when you have the associated content layout file open for editing in the environment.
        
    For Markdown topics, choose **Markdown link** as the type to create.  For MAML conceptual topics, choose
    **MAML link** as the type to create.  To insert topic references in XML comments in your source code,
    choose **XML comments conceptualLink** as the type to create.  For HTML topics, choose the **HTML anchor
    link** option.  The HTML option is for backward compatibility with the older additional content model and
    should not be used for Markdown and MAML conceptual content topics.  It is not recommended for XML comments
    either as the link target format may vary based on the help file format.

Code Snippets
:   When this is selected, a list of all code snippets grouped by their containing code snippet file is shown.
    In order to be listed, the file must have a `BuildAction` of `CodeSnippets`.  When inserted into a topic, the
    selected ID is wrapped in a MAML `codeReference` element.

Code Entities
:   When this option is selected for the first time, there is a short delay while all XML comments files plus the
    .NET Framework comments files are indexed to obtain a list of possible API members that can be used as code
    entity references.  Due to the large number of elements, they are not listed.  Instead, enter some text or a
    regular expression in the **Find** textbox and hit Enter or the **Go** button to list all members that match
    the search text.  The match count is limited to the first 1000 entries found to prevent exceedingly long
    lists of results.
    
    The **Markdown link** and **Markdown link (qualified)** options allow you to insert references to the
    selected element in Markdown topics.  The "qualified" option will append the `show-container` and
    `show-parameters` attributes if you want to qualify namespaces and types with their full name and show
    parameters on method links.  The **MAML code entity reference** format option allows you to insert
    references to the selected element in Markdown or MAML conceptual topic files as `codeEntityReference`
    elements.  When the help file is built, these links will be resolved to links that will take you to the help
    page for that item (i.e. the API member help page for your classes or online help pages for .NET Framework
    members).

    The **XML comments see link** option is for backward compatibility with the older additional content
    model and should not be used for Markdown and MAML conceptual content topics.  Note that these older style
    links will only be resolved to a clickable link if the item references a member found in your code.  It will
    not create links to .NET Framework members in HTML files.

## See Also
**Other Resources**  
[](@b772e00e-1705-4062-adb6-774826ce6700)  
[](@c38461a6-6edd-42cf-9d91-73c4b11cdd70)  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)  
