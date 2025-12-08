---
uid: 163cae15-9020-4095-9b9c-da134b5b496c
alt-uid: typeparam
title: typeparam
keywords: "section elements, typeparam"
---

This element is used to describe generic parameters on generic types and methods.



## Syntax

This top-level element is valid on generic types and generic methods to describe each generic
type parameter. *typeParamName* is the name of the generic type parameter being
referenced.


``` xml{title=" "}
<typeparam name="typeParamName">Type parameter description</typeparam>
```


## Remarks

There should be one `typeparam` element for each generic type parameter in
the class or method declaration.  The description will be used as the display text for the parameter in
IntelliSense and the Object Browser.
	



## Examples

``` cs{title=" " source="GenericClass.cs" region="typeparam/typeparamref Examples"}
```


## See Also


**Reference**  
[](@T:XMLCommentsExamples.GenericClass`2)  
[](@M:XMLCommentsExamples.GenericClass`2.GenericMethod``2(``0)){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@073a5ae1-828f-4bab-b0cb-438cefb5e9fb)  
