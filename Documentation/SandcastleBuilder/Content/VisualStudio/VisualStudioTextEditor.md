---
uid: 7cbf7f9b-d456-430a-8f85-9e30ed250cfa
alt-uid: VisualStudioTextEditor
title: Visual Studio Text Editor
keywords: "Visual Studio, text editor", "Visual Studio, IntelliSense", "Visual Studio, snippets", "Visual Studio, HTML encoding text"
---
The Visual Studio text editor is feature rich and very little needed to be added in the way of extra features to
support the Sandcastle Help File Builder.  The following sections describe the built-in features that you can
take advantage of as well as a couple of extra additions added by the help file builder's integration package.

<autoOutline lead="none" excludeRelatedTopics="true" />

## IntelliSense for the MAML Schemas
When editing MAML topics, you can take advantage of IntelliSense to help you enter the MAML elements, their
attributes, and attribute values.  The MAML schemas are installed in the Visual Studio schema cache by the
package.

In addition, hovering the mouse over a link or image target ID in a Markdown or MAML topic file will show you
information about the target file (title, filename, etc.).  This is useful as most items are identified by a GUID
and the quick info tooltip can provide a better idea of what the link will go to.  If you Ctrl+Click on the link
target, the item will be opened in the related text or image editor.  This feature (Go to Definition) is
available in Markdown topic files, MAML topic files, as well as XML comments elements in source code.  Note that
the Go to Definition feature must be enabled in the help file builder options in order to work.

## Drag and Drop
The text editor window also supports drag and drop operations from the **Content Layout Editor** and the
**Site Map Editor** to insert links to other topics in the project.  It also supports drag and drop from the
**Entity References Window** to insert token references, image links, code entity reference inks, code snippet
references, and table of contents links.

By default, links to other topics are inserted as self-closing link elements (MAML) and Markdown links only
specify the topic ID.  Both will use the topic's title from the content layout file when converted to HTML at
build-time.  If necessary, you can modify the link element to specify inner text which will be used instead of
the title text.

## HTML Encoding Selected Text
When editing MAML topics and other XML files, you will see an extra option on the text editor's context menu:
**HTML Encode Selected Text**.  It is only enabled when there is selected text.  If selected from the context
menu, it will HTML encode the selected text.  This is useful for the content of `code` elements.  You can paste
text into the element and then use this option to encode it rather than editing the text by hand to encode the
special characters.

## MAML Snippets
When editing MAML topics, you can take advantage of snippets to insert various block and inline MAML elements.
This is useful if you are not yet familiar with the various common MAML elements or for quickly inserting block
elements that have several parts like the `externalLink` element.  In order to do this, use the option in the
guided installer to install the MAML snippets in the Visual Studio snippet cache.  If you did not do it when you
first installed the tools, you can re-run the guided installer and skip down to the step that installs them.

To take advantage of the MAML snippets, use one of the following snippet insertion commands:

- Select **Insert Snippet** from the text editor context menu or press its shortcut key combination **Ctrl+K,
  Ctrl+X**.  This will allow you to insert a snippet at the selected location.
- Certain elements support wrapping selected text within the snippet (i.e. the various inline elements).  To wrap
  the selected text in the snippet element, select **Surround With** from the text editor context menu or press
  its shortcut key combination **Ctrl+K, Ctrl+S**.

Regardless of the option chosen, an auto-completion pop-up will appear.  Select the **My Xml Snippets** option
using either the down arrow key or by typing its name and pressing Enter or by double-clicking it with the mouse.
Repeat the process to select either the **MAML Block Elements** or **MAML Inline Elements** category.  The list
of elements will then appear.  If you selected **Insert Snippet**, you will see the full list of available
snippets in the selected category.  If you selected **Surround With**, the available elements are limited to
those that can surround the selected text.  Select the one you want by  using the down arrow key or typing its
name and pressing Enter or double-clicking it with the mouse.

Certain snippets such as `externalLink` have replacement parameters.  In such cases, you will be placed on the
first parameter and can type over its default value.  Press the Tab key to move between any subsequent
parameters.  Hovering the mouse over a parameter location will give you a tool tip describing the parameter.
Press Enter when done to complete the process.

Refer to the Visual Studio documentation for information on adding your own custom snippets.

## See Also
**Other Resources**  
[](@ba11d6d5-2f99-4b26-b384-21324ef1b49a)  
[](@b128ad2a-787e-48c7-b946-f6953080c386)  
