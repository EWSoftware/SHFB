---
uid: 1bef716a-235b-4d96-a23e-f43b8dcf9abd
alt-uid: example
title: example
keywords: "section elements, example"
---

This element is used to define an example for a type or one of its members to show how it is used.



## Syntax

This top-level element is valid on all types and members.  A description is optional.  One or more
`code` elements are typically included to show the example code.  More descriptive text
can be included between the `code` elements as needed.


``` xml{title=" "}
<example>
Optional code description

<code language="cs">
/// Example code
</code>

</example>
```


## Example

``` cs{title=" " source="SampleClass.cs" region="example/code Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.GetRandomNumber){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@1abd1992-e3d0-45b4-b43d-91fcfc5e5574)  
