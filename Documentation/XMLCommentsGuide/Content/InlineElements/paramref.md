---
uid: fa7d6ea0-93ce-41f6-9417-2f98e80fe9f5
alt-uid: paramref
title: paramref
keywords: "inline elements, paramref"
---

This element is used to indicate that a word in the comments refers to a parameter, typically a
method parameter.



## Syntax

This inline element can be used within any other element.  *paramName*
is the name of the parameter being referenced.


``` xml{title=" "}
<paramref name="paramName" />
```


## Example

``` cs{title=" " source="SampleClass.cs" region="param/paramref Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.CallStoredProcedure(System.String,System.Int32)){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.op_Addition(XMLCommentsExamples.SampleClass,XMLCommentsExamples.SampleClass)){prefer-overload="true"}  


**Other Resources**  
[](@d297bc14-33aa-4152-ae36-9f658b15de87)  
[](@e54dcff7-f8f3-4a11-9d17-1cf7decd880e)  
