---
uid: 1abd1992-e3d0-45b4-b43d-91fcfc5e5574
alt-uid: code
title: code
keywords: "block elements, code"
---
<!-- Ignore Spelling: tabsize utf filespec fs fscript jscript jscriptnet js vbs vbnet vbscript htm html -->
<!-- Ignore Spelling: xsl jsharp sql sqlserver py pshell powershell -->

This element is used to indicate that a multi-line section of text should be formatted as a code block.

<autoOutline lead="none">2</autoOutline>

## Syntax
This inline block element can be used within any other element.

``` xml{title=" "}
<code language="languageId"
  [title="Optional title"]
  [source="externalCodeFile" [region="importRegion"] [removeRegionMarkers="true | false"]]
  [tabsize="###"]
  [numberLines="true | false"]
  [outlining="true | false"]
  [keepSeeTags="true | false"]>

/// Code to display

</code>
```

Use this element to mark a multi-line block of text as code.  Use the [](@d0db2290-08bb-40cc-9797-23a342b96564)
element to mark inline text as code.

> [!NOTE]
> The `language` and `title` attributes are supported by Sandcastle.  All other attributes are implemented by the
> **Code Block Component** supplied with the <token>SHFB</token>.  Refer to its help file for more information
> about the build component.

## Literal XML And Other Special Characters
If you want to include XML or other unencoded information you can do one of the following:

- Utilize the `source` attribute to read it in from an external file instead.
- Use a `CDATA` section to encapsulate the code so that special characters and formatting are preserved.  For
  example:
  ``` cs{title="CDATA Example"}
  /// <example>
  /// <code language="xml" title="Example Configuration">
  /// <![CDATA[
  /// <?xml version="1.0" encoding="utf-8"?>
  /// <config>
  ///   <path>C:\Test\</path>
  ///   <filespec mask="*.txt" />
  /// </config>]]>
  /// </code>
  /// </example>
  ```
  Be sure to start the example code on a new line after the opening "`<![CDATA[`" tag as shown so that the
  leading whitespace can be normalized correctly.

## Attributes
The following attributes can be used to control how the code block is handled.

#### language
This attribute allows you to define the code language.  Although optional, it is recommended that you always
specify a language attribute if using Sandcastle alone.  Many of the presentation styles rely on the language
attribute to classify the code block and handle it in some way such as attaching it to a language filter or
grouping it with like code blocks based on the syntax filters selected in the project.  The Sandcastle Help File
Builder's code block component will apply a default language if one is not specified.  The possible language
values are as follows:

| Language ID (case-insensitive) | Language Syntax Used |
| --- | --- |
| cs, C#, CSharp | C# |
| cpp, cpp#, C++, CPlusPlus | C++ |
| c | C |
| fs, f#, FSharp, fscript | F# |
| EcmaScript, js, JavaScript | JavaScript |
| jscript, jscript#, jscriptnet, JScript.NET | JScript.NET |
| VB, VB#, vbnet, VB.NET | VB/VB.NET |
| vbs, vbscript | VBScript |
| htm, html, xml, xsl | XML |
| XAML | XAML |
| jsharp, J# | J# |
| sql, sql server, sqlserver | SQL script |
| py, python | Python |
| pshell, powershell, ps1 | PowerShell script |
| bat, batch | batch file script |
| Anything else (i.e. "none") | No language, no special handling. |


> [!IMPORTANT]
> The full name "`language`" should be used for the attribute name rather than the abbreviated form "`lang`".
> The reason is that the transformations used by the presentation styles expect the full name when they search
> for the language attribute.  The abbreviated form is a legacy syntax that should be avoided.  The code block
> component will auto-correct the attribute name so that it is not missed by the transformations.
> 
> It is recommended that you always specify a language attribute.

#### title
This attribute allows you to add a title that appears before the code block.  An example of its use would be to
label the example with a description.  If omitted, the language name will appear for the title.  If you do not
want a title on a particular block, set the `title` attribute to a single space (" ").

#### source, region, and removeRegionMarkers
These three attributes are extensions implemented by the code block build component.  The `source` attribute is
used to specify that the code block's content should be read from an external source code file.  If used alone,
the entire file is imported.  The optional `region` attribute can be used to limit the code to a specific section
of the file delimited with the named `#region` (`#pragma region` for C++).  The `#region` and `#endregion` tags
are excluded from the extracted section of code.

This is not to be confused with the XML comments `include` element.  This extension is intended to extract code
from actual source files.  This allows you to manage your code examples in buildable projects to test them for
correctness as a project is developed and altered in the future.  It also saves you from managing the code in the
XML comments and does not require that the code be HTML encoded as it is when written in the comments. The code
will be HTML encoded when it is read in for processing.  When used, it is assumed that there is no code within
the code element itself and thus it will always be self-closing.  Here are some examples:

Retrieve all code from an external file and use the VB.NET syntax to color it.  The path is relative to the code
block component's `basePath` configuration element.  Typically, the base path is the path of the documentation
project file.  So, if your documentation project is in a subfolder and the code is in the parent folder in an
*Examples* subfolder, the path would look like the following example.

``` xml{title=" "}
<code source="..\Examples\WholeDemo.vb" language="vbnet" />
```

Retrieve a specific `#region` from an external file.

``` xml{title=" "}
<code source="..\Examples\SeveralExamples.vb"
	region="Example 1" language="vbnet" title="Example #1" />
```

Note that VB.NET does not allow `#Region` and `#End Region` within a method body.  Other file types such as XML,
XAML, and SQL script do not understand `#region` statements.  However, if you want to extract a region from a
VB.NET method body or a section of another file type, you can add the region statements in comments to workaround
the limitation.  The component will still find it and extract the region.

``` none{title="Commented Region Examples"}
--------------------------------------------------------
VB.NET Example
--------------------------------------------------------
Public Sub SomeMethod()
    ' #Region "VB.NET Snippet"
    Dim x As Integer

    For x = 1 To 10
        Console.WriteLine(x)
    Next x
    ' #End Region
End Sub

--------------------------------------------------------
XAML Example
--------------------------------------------------------
<Style x:Key="SpecialButton" TargetType="{x:Type Button}">
  <Style.Triggers>
    <!-- #region XAML Snippet -->
    <Trigger Property="Button.IsMouseOver" Value="true">
      <Setter Property = "Background" Value="Red"/>
    </Trigger>
    <!-- #endregion -->
  </Style.Triggers>
</Style>

--------------------------------------------------------
SQL Example
--------------------------------------------------------
-- #region SQL snippet
Select *
From   tblName
Where  LastName = 'SMITH'
-- #endregion

/* #region Another snippet */
Select *
From   tblName
Where  FirstName = 'JOHN'
/* #endregion */
```

The `removeRegionMarkers` attribute can be used to specify whether or not region markers within the imported code
file or region are removed.  If not specified or set to false, any nested region markers are left in the imported
code block.  If specified and set to true, nested region markers are removed from the imported code.  This is
useful for removing nested region markers from larger code samples that contain smaller snippets of code used in
other examples.  The default setting (false) can be overridden via the component configuration.

#### tabSize
When the code blocks are formatted by the build component, tab characters are replaced with a set number of
spaces to preserve formatting.  This attribute can be used to override the default setting for a language which
is specified in the syntax file.  For example, if the default tab size for a language is four, adding
`tabSize="8"` will force it to use eight spaces instead.  If set to zero, the syntax file setting is used.  This
attribute sets the default tab size for unknown languages when used in the component's configuration.

#### numberLines
This attribute allows you to override the default setting in the component's configuration.  For example, if the
default setting is false to turn off line numbering, you can add `numberLines="true"` to enable numbering on a
specific code example.

#### outlining
This attribute allows you to override the default setting in the component's configuration.  For example, if the
default setting is false to not add collapsible regions, you can add `outlining="true"` to enable collapsible
regions on a specific code example.  Note that if a code block contains no `#region` or `#if` blocks, outlining
is automatically disabled and it will not reserve space in the margin for the markers.

#### keepSeeTags
When set to true, this attribute allows you to tell the build component's code colorizer to preserve `see` tags
within the code so that they can be rendered as clickable links to the related topic.  If set to false, the
default, any `see` tags within the code will be colorized and passed through as literal text.  When using this
option, you may find that you need to specify inner text for the `see` tag so that the link text appears as you
want it.  If the self-closing version of the tag is used, Sandcastle will generally set the link text to the name
of the item plus any parameters if it is a generic type or takes parameters which may not be appropriate within a
code sample.

## Nested Code Elements
You can import multiple code snippets into one common colorized code block by nesting `code` elements within a
parent `code` element.  On nested `code` elements, only the `source` and `region` attributes will be utilized.
All other attributes that control colorization of the merged code block will be retrieved from the parent `code`
element.

``` xml{title="Nested Code Elements Example"}
<code title="Nested code elements example" language="VB.NET">
<code source="..\Class1.vb" region="Snippet #1" />
<code source="..\Class1.vb" region="Snippet #2" />
</code>
```

Literal code can also be mixed in between the nested `code` elements.  However, there are some limitations.  All
elements and literal code should be flush left within the parent `code` element or you will end up with
additional spaces before the first line of each nested `code` element.  There may still be additional spaces
before the literal code.  This is caused by the indentation included when the XML comments file is created by the
compiler.  If this is a problem, move the example to an include file and remove the excess leading whitespace.
Use an `include` element to bring it into the XML comments.  When done this way, the compiler preserves the
spacing when writing the example out to the XML comments file.

``` xml{title="Nested Code Elements Example"}
<code title="Nested code elements example" language="VB.NET">
<code source="..\Class1.vb" region="Snippet #1" />

' ... Some stuff happens here ...

<code source="..\Class1.vb" region="Snippet #2" />
</code>
```

## Examples
``` cs{title=" " source="SampleClass.cs" region="example/code Example"}
```

## See Also
**Reference**  
[](@M:XMLCommentsExamples.SampleClass.GetRandomNumber){prefer-overload="true"}  

**Other Resources**  
[](@f8464c0f-f62a-4faf-b11a-9a41173307e8)  
[](@1bef716a-235b-4d96-a23e-f43b8dcf9abd)  
