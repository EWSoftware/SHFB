---
uid: 0522f3bf-0a57-4d70-a2a5-d64a14c5bcc9
alt-uid: filterpriority
title: filterpriority
keywords: "miscellaneous elements, filterpriority"
---

This element is used by the Visual Basic editor to control the IntelliSense visibility for methods
and properties.  It has no effect on the appearance of the topic in the compiled help file.



## Syntax

This top-level element can be used on methods and properties.


``` xml{title=" "}
<filterpriority>1 | 2 | 3</filterpriority>
```


## Remarks

This element determines how a member appears in Visual Basic IntelliSense.  A value of 1 means
that it should appear in the **Common** tab, 2 means it should appear in the
**All** tab, and 3 means it should be hidden from IntelliSense completely.


> [!NOTE]
> This is effectively equivalent to using
> [](@T:System.ComponentModel.EditorBrowsableAttribute).
> However, unlike the attribute, the XML comments element only has effect in Visual Basic and is ignored by all
> other languages.  As such, it is rarely used.  The most common place to see it is in the XML comments for the
> base .NET Framework classes themselves.
> 
>


## Example

``` cs{title=" " source="SampleClass.cs" region="filterpriority Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.FilterPriorityExample){prefer-overload="true"}  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
