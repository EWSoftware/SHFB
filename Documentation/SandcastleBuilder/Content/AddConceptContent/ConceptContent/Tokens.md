---
uid: ed6870bb-772d-4596-9fc1-5638ae6d621b
alt-uid: Tokens
title: Token Files
keywords: "conceptual content, token files"
---
A token is used as a replaceable tag within a conceptual content topic and is represented using a `token` MAML
element.  The inner text of the element is a token name.  When the help file is built, the token is replaced with
its value.  They are an easy way to represent common items that you use regularly such as a common phrase or
external link.  Updating the token's value will cause the new value to be used in all topics in which the token
appears.

Token definitions are stored in an XML file.  In the help file builder, these files have a *.tokens* extension
and will be edited using the editor window shown below.  Be sure to set the file's `BuildAction` to `Tokens` so
that it is included correctly at build time.

^^^{placement="center"}
![](@TokenFileEditor)
^^^

The **Find** textbox allows you to search for a token by ID or content value.  Enter some text to find and hit
Enter or click the **Go** button to start the search.  Hit Enter or click **Go** again to find subsequent
matches.  Matches are found by doing a case-insensitive search of the token ID and content values.

To add a new token, click the **Add** button or press Ctrl+Shift+A.  Edit the token name and its content in the
fields on the right.  The changes to the name and/or content are stored automatically as you move from token to
token in the list on the left.  To delete a token, click the **Delete** button or press the Delete key.  You can
use the **Copy** button or Ctrl+C to copy the selected token to the clipboard as a `token` element ready for
pasting into a MAML topic.  Tokens can also be dragged from the list and dropped in a MAML topic to create a
token reference.  A more convenient way of inserting references of various types is to use the
[](@e49eea91-a9ef-4aa5-ad8f-16ebd61b798a).

If you embed MAML elements within a token, you must prefix each element name with "`ddue:`" (without the quotes)
as shown in the example below.  If not, the elements are inserted using the default empty namespace and they will
not be processed by the Sandcastle transformations.

``` xml{title="Token with MAML Elements"}
<ddue:externalLink>
    <ddue:linkText>Sandcastle Help File Builder</ddue:linkText>
    <ddue:linkUri>https://GitHub.com/EWSoftware/SHFB</ddue:linkUri>
</ddue:externalLink>
```
The token value can contain help file builder substitution tag references.  See the
[](@69d998a7-1af5-4bf5-889b-59cd00b64000) topic for a list of the possible replacement tag values.

## See Also
**Other Resources**  
[](@3d4edd2d-7883-4508-b9d2-bd7b4d848b0d)  
[](@e49eea91-a9ef-4aa5-ad8f-16ebd61b798a)  
