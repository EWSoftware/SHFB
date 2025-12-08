---
uid: bbd1e65d-c87c-4b46-9a1a-259d3c5cd936
alt-uid: exception
title: exception
keywords: "section elements, exception"
---

This element is used to list exceptions that can be thrown by a type's member.



## Syntax

This top-level element is valid on property, method, event, operator, and type conversion members.
*exceptionType* is the name of the exception type that can be thrown.


``` xml{title=" "}
<exception cref="exceptionType">description</exception>
```


## Remarks

There should be one `exception` element for each exception type that can be
thrown.  The description will be used as the display text for the exception in IntelliSense and the Object
Browser.



## Example

``` cs{title=" " source="SampleClass.cs" region="exception Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.ProcessText(System.String)){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
