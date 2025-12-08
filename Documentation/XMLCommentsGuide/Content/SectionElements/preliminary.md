---
uid: c16bece7-694e-48ca-802d-cf3ae9205c55
alt-uid: preliminary
title: preliminary
keywords: "section elements, preliminary"
---

This element is used to indicate that a particular type or member is preliminary and is subject to
change.



## Syntax

This top-level element is valid on all types and type members.


``` xml{title=" "}
<preliminary />

or

<preliminary>description</preliminary>
```

The self-closing version will insert a default message into the topic stating "This API is
preliminary and subject to change".  You can supply inner text on the element to provide a message of your 
choosing.



## Remarks

This element is used to mark individual types or members as preliminary.  Build tools such as the
<token>SHFB</token> provide a project property that can be used to mark the entire help file as preliminary.
This saves you from having to annotate every single class and member with this element.


> [!NOTE]
> This is a custom XML comments element implemented by Sandcastle.  It will not appear in the list
> of valid elements for XML comments IntelliSense.  If applied to a type, the preliminary message will be
> propagated to all members of the type and will appear in their topics as well.
> 
>


## Examples

``` cs{title=" " source="SampleClass.cs" region="preliminary Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.PreliminaryExample){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.TemporaryMethod){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
