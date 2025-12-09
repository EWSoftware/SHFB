---
uid: 4302a60f-e4f4-4b8d-a451-5f453c4ebd46
alt-uid: note
title: note
keywords: "block elements, note"
---

This element is used to create a note-like section within a topic to draw attention to some important
information.

## Syntax
This inline block element can be used within any other element.

``` xml{title=" "}
<note type="noteType" [title="Optional title override"]>note content</note>
```

The `type` attribute defines the note type.  The *noteType* value should be one of the values shown below.  If
omitted, a default note type of "note" is assumed.  The note content can be any valid inline or block XML
comments similar to a `remarks` element.

> [!IMPORTANT]
> The `type` attribute value is not case-sensitive.  Unrecognized values will default to the "note" style.

The `type` attribute can be any of the following values which fall into four different categories that use
different icons to the left of the note title.  The title will generally be the note type name or an expanded
form of it.

<table>
  <thead>
    <tr>
      <th>Category</th>
      <th>Type Value</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>General note</td>
      <td>note, tip, implement, caller, inherit</td>
    </tr>
    <tr>
      <td>Cautionary</td>
      <td>caution, warning, important</td>
    </tr>
    <tr>
      <td>Security</td>
      <td>security, security note</td>
    </tr>
    <tr>
      <td>Language note</td>
      <td>cs, CSharp, c#, C#, visual c# note, cpp, CPP, c++, C++, visual c++ note, vb, VB,
VisualBasic, visual basic note, JSharp, j#, J#, visual j# note</td>
    </tr>
    <tr>
      <td>To Do</td>
      <td>todo</td>
    </tr>
  </tbody>
</table>

An optional `title` attribute can be used to provide a user-defined title that it will override the default title.

> [!NOTE]
> This is a custom XML comments element implemented by Sandcastle.  It will not appear in the list of valid
> elements for XML comments IntelliSense.

## Examples

``` cs{title=" " source="SampleClass.cs" region="note Examples"}
```

## See Also
**Reference**  
[](@M:XMLCommentsExamples.SampleClass.VariousNoteExamples){prefer-overload="true"}  

**Other Resources**  
[](@f8464c0f-f62a-4faf-b11a-9a41173307e8)  
