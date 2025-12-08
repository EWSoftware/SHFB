---
uid: 515d5a54-5047-4d6f-bf51-d47c7c699cc2
alt-uid: UsageNotes
title: Usage Notes
keywords: usage notes
---
The following are some general usage notes and things to keep in mind when authoring XML comments in your code.

- XML comments support all of the common HTML tags such as `a` (anchor), `b` (bold), `img` (image), `p`
  (paragraph), etc.  As such, you can use HTML markup to add additional formatting to your XML comments if
  necessary.

  > [!CAUTION]
  > Not all help output formats support HTML elements in XML comments or may use a different
  > URL format from other help formats.  For example, MS Help Viewer's URL format differs from the other help formats
  > which may result in broken links if literal HTML anchor or image links are used.  As such, try to use XML comments
  > elements whenever possible.  For example, to link to API topics, use `see` elements and
  > for conceptual topics, use `conceptualLink`.
- All XML and HTML within XML comments must be well formed.  That means that all elements must have a
  corresponding closing element or must be self-closing, special characters such as "&lt;" and "&gt;" must
  be encoded appropriately (i.e. `&lt;` and `&gt;`), entities not recognized by the XML parser must be encoded
  in their numeric form (i.e. `&#160;` rather than `&nbsp;` for a non-breaking space).
- For larger blocks of XML that you want to insert literally such as code examples, you can use the
  `<![CDATA[ ]]>` option to place them in the element.  See the [](@1abd1992-e3d0-45b4-b43d-91fcfc5e5574) element
  for an example.
- When referencing a generic type or a member with generic parameters as the target of an element attribute such
  as the `cref` attribute on a `see` element, use curly braces rather than angle brackets on the generic type
  parameters.  For example:

  ``` cs{title=" "}
  /// <summary>
  /// This class is derived from <see cref="List{T}" />
  /// </summary>
  /// <typeparam name="T">The element type of the list</typeparam>
  /// <seealso cref="CustomDictionary{TKey, TValue}" />
  public class CustomList<T> : List<T>
  {
  }
  ```
- Namespace comments cannot be attached directly to a namespace code element.  Instead, you must use an alternate
  means to specify namespace comments.  See the [](@BD91FAD4-188D-4697-A654-7C07FD47EF31) topic for more
  information.
- XML comments for enumerations are treated differently.  The common convention in presentation styles is to
  suppress topics for the individual enumeration members.  Instead, they are listed in a table in the containing
  enumerated type's topic.  Within that table, only the summary for each member is listed as the description and
  remarks, if present, will be included.  Any additional comments elements such as
  `seealso` or `example` elements will be ignored.  As such, include
  any additional information that must appear in the help file in the enumerated type's XML comments or within
  the member's `summary` or `remarks` element.

## See Also
**Other Resources**  
[](@57C91630-95D6-4E3E-AF24-3415CC569AC8)  
[](@ee5d612e-914f-411f-bd95-23478b15e4de)  
[](@BD91FAD4-188D-4697-A654-7C07FD47EF31)  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@f8464c0f-f62a-4faf-b11a-9a41173307e8)  
[](@d297bc14-33aa-4152-ae36-9f658b15de87)  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
