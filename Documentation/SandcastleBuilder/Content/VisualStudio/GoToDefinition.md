---
uid: ba11d6d5-2f99-4b26-b384-21324ef1b49a
alt-uid: GoToDefinition
title: Go To Definition Support
keywords: "Visual Studio, Go To Definition"
---
If enabled using the help file builder [configuration options](@2152ed96-bf69-4b9b-b1a7-4fffc71b3095), hovering
over certain link target attribute values or element inner text within Markdown and MAML topics and XML comments
in C# projects will provide more information about the target (i.e. the topic title and filename for Markdown
and MAML target links) and the option to go to the definition of the item if possible when the Go To Definition
context menu option is selected or Ctrl+Click is pressed while hovering over the text.

> [!NOTE]
> The color of the link underline can be changed using **Tools | Options | Fonts and Colors** and changing the
> color for the **MAML/XML Comments Link Underline** entry.
> 
> In order for Ctrl+Click to have any effect, the option must be enabled in the help file builder options and the
> editor window must have the focus.  If you hold down Ctrl and no underline appears under a valid element under
> the mouse, ensure the window has the focus by clicking in it.  If an underline still does not appear, check the
> help file builder options to see if the option is enabled.

## Markdown and MAML Topics
While editing a Markdown or MAML topic, you can hover over the following elements.  If the target ID appears to
be navigable, an underline will appear when the Ctrl key is held down and the tool tip will indicate that you can
Ctrl+Click on it.  Selecting the Go To Definition context menu option or Ctrl+Clicking the link will attempt to
navigate to the target element.

- A MAML `topic` element's `id` attribute value - Hovering over this shows a tool tip containing the topic's
  title and filename.  If "(Not found)" is displayed for the title, the topic's ID could not be found in a
  content layout file within the project.  This element does not offer navigation as it represents the current
  file.
- A MAML `image` element's `xlink:href` attribute value or a Markdown image link - Hovering over this shows a
  tool tip containing the image's alternate text and filename.  If "(Not found)" is displayed for the alternate
  text, the image could not be found within the project with a `BuildAction` of `Image`.  If "(Not set)" is shown
  for the alternate text, the image was found but no alternate text has been set in the file's properties.
  Select the Go To Definition context menu option or Ctrl+Click on the link to open the associated image file.
- A MAML `link` element's `xlink:href` attribute value or a Markdown topic link - Hovering over this shows a tool
  tip containing the topic's title and filename.  If "(Not found)" is displayed for the title, the topic's ID
  could not be found in a content layout file within the project.  Select the Go To Definition context menu
  option or Ctrl+click on the link to open the associated topic file.
- A MAML `codeEntityReference` element's inner text or a Markdown code entity link - This represents a code API
  member ID.  Selecting the Go To Definition context menu option or Ctrl+Clicking the link will attempt to open
  the associated code file and jump to the indicated member.  Note that navigation is only available for members
  of C# projects within the current solution.  It is not possible to navigate to members in non-C# projects or to
  members of base framework or reference assemblies.

  Since code entity references must be fully qualified, there is an excellent probability that the target ID's
  member will be found and displayed provided it is within the current solution as noted above.  However, since
  the ID must be parsed and searched for as text, there is a chance that an incorrect match will be made or no
  match will be found.  As such, check to be sure that you ended up where you expected before making any changes.
  In the event it cannot find a match, check the ID to ensure that it is correct.  The **Entity References** tool
  window can be used to verify the ID.  If the ID is correct and is within the current solution, you may have
  found a case that could not be matched with enough accuracy.

  Note that namespace identifiers (those IDs prefixed with "N:" or "G:") can never be matched to a namespace
  declaration as namespaces do not exist as code elements identifiable in a search since they may appear in
  multiple locations.  As such, these IDs are translated to the related `NamespaceDoc` or `NamespaceGroupDoc`
  type ID and a search is performed for it instead within the identified namespace.
- A MAML `codeReference` element's inner text - This represents a code snippet ID in a code snippets file.  The
  code snippet ID must appear in a file within a project in the solution and the file's `BuildAction` must be set
  to `CodeSnippets`.  Selecting the Go To Definition context menu option or Ctrl+clicking the link will attempt
  to open the associated code snippets file and jump to the indicated ID.  If the ID contains multiple snippet
  identifiers after the group identifier, only the first snippet identifier will be used for the search.
- A MAML `token` element's inner text - This represents a token name in a token file.  The token ID must appear
  in a file within a project in the solution and the file's `BuildAction` must be set to `Tokens`.  Selecting the
  Go To Definition context menu option or Ctrl+clicking the link will attempt to open the associated token file
  and, if not already open, select the indicated ID.

## XML Comments
While editing C# source code, you can hover over the following XML comments elements.  If the target ID appears
to be navigable, an underline will appear when the Ctrl key is held down and the tool tip will indicate that you
can Ctrl+click on it.  Selecting the Go To Definition context menu option or Ctrl+clicking the link will attempt
to navigate to the target element.

- A `conceptualLink` element's `target` attribute value - Hovering over this shows a tool tip containing the
  topic's title and filename.  If "(Not found)" is displayed for the title, the topic's ID could not be found in
  a content layout file within a help file builder project in the solution.  Select the Go To Definition context
  menu option or Ctrl+click on the link to open the associated topic file.
- A `token` element's inner text - This represents a token name in a token file.  The token ID must appear in a
  file within a help file builder project in the solution and the file's `BuildAction` must be set to `Tokens`.
  Selecting the Go To Definition context menu option or Ctrl+clicking the link will attempt to open the
  associated token file and, if not already open, select the indicated ID.

## See Also
**Other Resources**  
[](@7cbf7f9b-d456-430a-8f85-9e30ed250cfa)  
[](@b128ad2a-787e-48c7-b946-f6953080c386)  
