---
uid: 54e3dc97-5125-441e-8e84-7f9303e95f26
alt-uid: ContentLayoutEditor
title: Content Layout File Editor
keywords: "conceptual content, content layout file editor"
---
Adding a content layout file to the project and setting its `BuildAction` to `ContentLayout` allows you to
specify the conceptual content topics that will be included in the help file and define their layout in the
table of contents.

<autoOutline lead="none" excludeRelatedTopics="true" />

## The Content Layout Editor Window
You can edit the content layout file from within the help file builder by double-clicking it in the Project
Explorer window or by selecting the **Open** option from its context menu.  The content layout editor window is
shown below.

^^^{placement="center"}
![](@ContentLayoutEditor)
^^^

The left side of the window shows the table of contents as it is currently defined in the file.  Entries can be
arranged hierarchically to any number of levels.  The right side of the window shows the properties for the
currently selected topic.  You can modify the topic's details at any time.  Changes are stored automatically
before moving off to another topic in the tree.  The content layout file identifies conceptual content topics by
their unique ID rather than by filename.  As such, you are free to rearrange the topic files in the Project
Explorer as you see fit and even change their filenames.  As long as you do not change the topic's unique ID, it
will be correctly associated with its entry in the content layout file.  See the
[](@4b8ab701-2321-4d24-a287-8848de086f68) topic for a description of each of the topic file properties and how
they are used.

The **Find** textbox allows you to search for a topic by ID, display name, or filename.  Enter some text to
find and hit Enter or click the **Go** button to start the search.  Hit Enter or click **Go** again to
find subsequent matches.  Matches are found by doing a case-insensitive search of the topic IDs, display names,
and filenames.

The tree supports drag and drop to re-order the topics.  Simply click and hold the left mouse button on a topic
and then drag it to its new location.  When you drop it on a topic, it is inserted immediately after the target
item.  If you hold down the **Shift** key while dropping an item, it will become a child of the target item.

> [!TIP]
> Dragging a topic from the content layout editor and dropping it into an editor window containing a Markdown or
> MAML file will create a link to the item in the topic that is being edited.  A more convenient way of inserting
> references of various types is to use the [](@e49eea91-a9ef-4aa5-ad8f-16ebd61b798a).

## Available Commands
The toolbar above the tree is used to perform various actions on the content layout file such as adding
new topics, deleting topics, rearranging them, etc.  These options are also available in a context menu accessed
by right clicking on a topic in the tree.  The commands (from left to right) and their associated context menu
options perform the following actions:

Add Sibling Topic
:   This will add a new topic at the same level as the currently selected topic.  The sub-options for this
    command allow you to add a new topic file based on a template, add an existing topic file, add all topic
    files from an existing folder, or add an empty container node.
    
    
    > [!NOTE]
    > Adding a topic to the content layout file will automatically add its related file to the project if it is
    > not already there.  Adding all files from a folder will add all topic files found recursively.  The folder
    > structure will be duplicated in the table of contents layout.
    > 
    > New files added to the project are added in the same location as the currently selected topic.  If you want
    > more precise control over file placement, add the new topics via the **Project Explorer** and then
    > use the **Add Existing Topic File** option to add them to the content layout file.

Add Child Topic
:   This will add a new topic as a child of the currently selected topic.  The sub-options for this command
    allow you to add a new topic file from a template, add an existing topic file, add all topic files from an
    existing folder, or add an empty container node.  The notes from above for sibling topics apply to these
    commands as well.

Delete
:   Delete the selected topic and all of its sub-topics.
    
    > [!NOTE]
    > Deleting a topic from the content layout file will not delete its related file from the project.  Use the
    > **Project Explorer** to physically delete the file from the project and the file system.

Copy as Topic Link
:   Copy a link to the topic to the clipboard.  If pasted into a Markdown or MAML file, a link element is
    inserted that will link to the selected topic.

Cut
:   Cut the selected topic and its sub-topics to the clipboard.  A copy of the topic is made and it is then
    removed from the tree.

Paste
:   If a topic has been cut to the clipboard, you can click the **Paste** button to add it to the tree.  If you
    just click the button, the topic is added as a sibling of the currently selected topic immediately following
    it.  If you select the **Paste as Child** sub-option, the topic is inserted as a child of the selected topic.

Edit File
:   Edit the selected content file.  Double-clicking a topic in the tree will also open it for editing.

Move Up
:   This will move the selected topic up in the order of topics at the same level as the selected topic.  Note
    that moving a topic will also move its sub-topics.  Sub-topics will still remain in their given order below
    the parent topic.

Move Down
:   This will move the selected topic down in the order of topics at the same level as the selected topic.  Note
    that moving a topic will also move its sub-topics.  Sub-topics will still remain in their given order below
    the parent topic.

Sort Topics
:   Sort the topics alphabetically by title within the currently selected topic's group.

Convert to Markdown
:   This command only appears on the context menu when you right click on a topic.  It is only enabled if the
    topic is a MAML file.  Selecting this command will convert the MAML topic file to a Markdown topic file.
    The converted file will replace the MAML file in the project and the content layout file.  A backup copy of
    the original MAML file is created with a `.aml.bak` extension in case you want to revert to it later.
    See the [](@ConvertingToMarkdown) topic for more information.

Expand/Collapse Options
:   This command contains a set of options that lets you expand or collapse all topics in the tree or just the
    sub-topics for the selected topic.  When the file is saved, the expanded/collapsed state is saved along with
    the last selected topic making it easy to return to the spot you stopped editing the file when it was last
    opened.  This information is also used by the **Entity References Window** to make it easy to find the
    current topic when using it to insert topic references into other topic files.

Help
:   Open this help topic in the help file.

## Keyboard Shortcuts
The following keyboard shortcuts can also be used in the content tree to execute the various commands:

- **Context Menu Key** - Display the context menu.
- **Delete** - Delete the selected topic.
- **Ctrl+Up** - Move the selected topic up within its group.
- **Ctrl+Down** - Move the selected topic down within its group.
- **Ctrl+C** - Copy a link to the selected topic to the clipboard.  Performing a paste operation in a text editor
  window will insert a link to the copied topic.
- **Ctrl+X** or **Shift+Delete** - Cut the selected topic to the clipboard.
- **Ctrl+V** or **Shift+Insert** - Paste the topic on the clipboard as a sibling of the selected topic.
- **Ctrl+Shift+V** or **Ctrl+Shift+Insert** - Paste the topic on the clipboard as a child of the selected topic.
- **Ctrl+E** - Edit the selected topic.
- **Ctrl+Shift+T** - Sort the topics at the same level as the selected topic alphabetically.

## See Also
**Other Resources**  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)  
[](@4b8ab701-2321-4d24-a287-8848de086f68)  
