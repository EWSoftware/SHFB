---
uid: db2703b4-12bc-4cf5-8642-544b41002809
alt-uid: conceptualLink
title: conceptualLink
keywords: "miscellaneous elements, conceptualLink"
---

This element is used to create a link to a MAML topic within the **See Also**
section of a topic or an inline link to a MAML topic within one of the other XML comments elements.



## Syntax

This element is valid on all types and type members as a top-level or an inline element.


``` xml{title=" "}
<conceptualLink target="topicGUID" />

or

<conceptualLink target="topicGUID">inner text</conceptualLink>
```

The `target` attribute specifies the GUID of the MAML topic to which the
link should be connected.  When used as a top-level element, a link will be created to the MAML topic in the
**See Also** section.  When used as an inline element within another XML comments element,
it creates an inline link to the MAML topic.  The self-closing form will use the topic title as the link text.
Specifying inner text on the element will use that text instead for the link text.


> [!NOTE]
> This is a custom XML comments element implemented by the <token>SHFB</token>.  It will not appear
> in the list of valid elements for XML comments IntelliSense.
> 
>


## Example

``` cs{title=" " source="SampleClass.cs" region="conceptualLink Examples"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.ConceptualLinkExample){prefer-overload="true"}  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
[](@983fed56-321c-4daf-af16-e3169b28ffcd)  
[](@16cdb957-a35b-4c17-bf5e-ea511b0218e3)  
