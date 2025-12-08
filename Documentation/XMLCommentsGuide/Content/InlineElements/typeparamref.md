---
uid: 073a5ae1-828f-4bab-b0cb-438cefb5e9fb
alt-uid: typeparamref
title: typeparamref
keywords: "inline elements, typeparamref"
---

This element is used to indicate that a word in the comments refers to a type parameter on a generic
type.



## Syntax

This inline element can be used within any other element on a generic type or its members.
*typeParamName* is the name of the parameter being referenced.


``` xml{title=" "}
<typeparamref name="typeParamName" />
```


## Examples

``` cs{title=" " source="GenericClass.cs" region="typeparam/typeparamref Examples"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.GenericClass`2.GenericMethod``2(``0)){prefer-overload="true"}  
[](@T:XMLCommentsExamples.GenericClass`2)  


**Other Resources**  
[](@d297bc14-33aa-4152-ae36-9f658b15de87)  
[](@163cae15-9020-4095-9b9c-da134b5b496c)  
