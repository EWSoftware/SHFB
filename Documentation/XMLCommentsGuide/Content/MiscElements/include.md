---
uid: 3de64a85-dafb-4a01-85dc-7f69a76ef790
alt-uid: include
title: include
keywords: "miscellaneous elements, include"
---

This element lets you import comments from another XML file that describe the types and members in
your source code.  The comments are imported into the XML comments file generated at build time by the compiler.



## Syntax

This element is valid on all types and type members as a top-level or an inline element.


``` xml{title=" "}
<include file="xmlFilename" path="xpath-filter-expr" />
```

The `file` attribute specifies the path to an external XML file.  The
filename can be a fully qualified or a relative path.  If no path is specified on the filename or it is prefixed
with a folder name alone, the file is assumed to be in the same folder as the Visual Studio project or a
subfolder beneath it.  If a relative path is given that starts with *..\*, the file is assumed
to be in a location relative to the current source code file.


The `path` element specifies an XPath query used to import one or more
elements into the member's XML comments.  The structure of the XML file is entirely up to you.  A common
convention is to use elements with an ID attribute and to look them up with an XPath query such as
"`rootElement/subElement[@id='idValue']/*`".  The "/*" at the end of the query pulls in
the content of the matching element.



## Remarks

Using this element allows you to keep common sets of XML comments or examples in a shared file as
an alternative to repeatedly cutting and pasting the same elements into multiple locations in your source code.
This allows you to maintain the comment elements in a single location.  When the project is built, the updated
comments will automatically be imported into each of the locations where they are referenced.  It also allows
one person to work on the code while another writes the comments.  If the goal is to cut down on duplicated
comments, the [](@86453FFB-B978-4A2A-9EB5-70E118CA8073) element may be a better choice.


> [!NOTE]
> The XPath query typically uses quote marks to surround an ID value.  If you use double quotes to
> surround the XPath expression, use single quotes to surround the ID value.  You can also use single quotes to
> surround the XPath expression and use double quotes to surround the ID value.  Examples of both ways are shown
> below.
> 
> 
> A missing XML file or an ill-formed XPath query will result in a compiler warning being generated.
> A valid XPath query that fails to locate anything will not generate any warning but will cause missing comments
> in the resulting XML comments file.
> 
>


## Examples

``` cs{title="Comments Examples" source="SampleClass.cs" region="include Examples"}
```

``` xml{title="Example XML File" source="IncludeComments.xml"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.IncludeAllExample){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.IncludeSectionsExample){prefer-overload="true"}  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
[](@86453FFB-B978-4A2A-9EB5-70E118CA8073)  
