---
uid: e433d846-db15-4ac8-a5f5-f3428609ae6c
alt-uid: list
title: list
---
<!-- Ignore Spelling: listheader -->

This element describes content that should be displayed as a list or a table.

## Syntax
This inline block element can be used within any other element.

``` xml{title=" "}
<list type="bullet">
  <item>
    <term>Optional term</term>
    <description>Required description.  If term is omitted, the description can be
listed as the item element's inner text.</description>
  </item>
  ...
</list>

or

<list type="number" [start="###"]>
  <item>
    <term>Optional term</term>
    <description>Required description.  If term is omitted, the description can be
listed as the item element's inner text.</description>
  </item>
  ...
</list>

or

<list type="definition">
  <item>
    <term>Required term</term>
    <description>Required description</description>
  </item>
  ...
</list>

or

<list type="table">
  <listheader>
    <term>Multi-column table header</term>
    <term>Add additional term or description elements to create new header columns</term>
    ...
  </listheader>
  <item>
    <description>Multi-column table</description>
    <description>Add additional term or description elements to create new columns</description>
    ...
  </item>
  ...
</list>

```

The `type` attribute defines the list type.  The list type value should be one of the values shown below.  The
list item terms and descriptions can be any valid inline or block XML comments similar to a `remarks` element.

> [!IMPORTANT]
> The `type` attribute value is not case-sensitive.  Unrecognized values will default to the `bullet` list style.

The `type` attribute can be any of the following values.  The requirements of each format are noted in the
description.

<table>
  <thead>
    <tr>
      <th>List Type</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>bullet</td>
      <td>

This renders a bulleted list.  Each `item` element will render a new bulleted item.  If `term` and `description`
elements are omitted, the inner text is rendered after the bullet point.  The same thing happens if a
`description` element is included by itself.  If both a `term` and a `description` element are present, the term
is highlighted and separated from the description by a dash.

</td>
    </tr>
    <tr>
      <td>number</td>
      <td>

This renders a numbered list.  Each `item` element will render a new numbered item.  If `term` and `description`
elements are omitted, the inner text is rendered after the number.  The same thing happens if a `description`
element is included by itself.  If both a `term` and a `description` element are present, the term is highlighted
and separated from the description by a dash.

The `number` style supports an optional `start` attribute that can be used to indicate the starting number for
the numbered list.  This lets you create a list at a later point in the topic that continues numbering from the
end point of a prior list.  You are responsible for providing the appropriate starting number for the new list.

> [!NOTE]
> `start` is a custom attribute implemented by Sandcastle.

</td>
    </tr>
    <tr>
      <td>definition</td>
      <td>

A definition list.  A `term` and `description` element are required within each `item` element.  The term is
highlighted and rendered on a separate line with the description following in a new paragraph.

</td>
    </tr>
    <tr>
      <td>table</td>
      <td>

A multi-column table.  The `item` elements create new rows in the table.  With this type, the optional
`listheader` element can be used to define the column headers.  Use a `term` or a `description` element to define
the columns in the `listheader` and each `item` element.

</td>
    </tr>
  </tbody>
</table>


## Examples
``` cs{title=" " source="SampleClass.cs" region="list Examples"}
```

## See Also
**Reference**  
[](@M:XMLCommentsExamples.SampleClass.VariousListExamples){prefer-overload="true"}  

**Other Resources**  
[](@f8464c0f-f62a-4faf-b11a-9a41173307e8)  
