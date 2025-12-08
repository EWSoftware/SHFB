---
uid: e54dcff7-f8f3-4a11-9d17-1cf7decd880e
alt-uid: param
title: param
keywords: "section elements, param"
---

This element is used to describe method parameters.



## Syntax

This top-level element is valid on methods and operator overloads to describe each parameter.
*paramName* is the name of the parameter being referenced.


``` xml{title=" "}
<param name="paramName">Parameter description</param>
```


## Remarks

There should be one `param` element for each method or operator overload
parameter.  The description will be used as the display text for the parameter in IntelliSense and the Object
Browser.



## Examples

``` cs{title="Method Example" source="SampleClass.cs" region="param/paramref Example"}
```

``` cs{title="Operator Overload" source="SampleClass.cs" region="param Operator Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.CallStoredProcedure(System.String,System.Int32)){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.op_Addition(XMLCommentsExamples.SampleClass,XMLCommentsExamples.SampleClass)){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@fa7d6ea0-93ce-41f6-9417-2f98e80fe9f5)  
