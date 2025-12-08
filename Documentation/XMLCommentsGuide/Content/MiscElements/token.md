---
uid: 8c9273f3-0000-43cd-bb53-932b80855297
alt-uid: token
title: token
keywords: "miscellaneous elements, token"
---

This element represents a replaceable tag within a topic.  The inner text of the element is a token
name.  The tokens are defined in a separate token file.  They are an easy way to represent common items that you
use regularly such as a common phrase or external link.



## Syntax

This element is valid on all types and type members as a top-level or an inline element.


``` xml{title=" "}
<token>tokenName</token>
```

The `tokenName` inner text specifies the ID of the token to insert into the
topic.


> [!NOTE]
> This is a custom XML comments element implemented by the <token>SHFB</token>.  It will not appear
> in the list of valid elements for XML comments IntelliSense.  The **API Token Resolution** build component
> must be added to the project's **Component Configurations** property in order for tokens in XML comments to
> be replaced.  Only general token values can be used when referenced from XML comments.  MAML elements will not
> be resolved and rendered.
> 
> 
> Those using the Sandcastle tools by themselves can achieve the same results by adding the
> `SharedContentComponent` to their reference build configuration file for
> **BuildAssembler**.  The configuration will match the settings from the copy used for
> conceptual builds.
> 
> 
> See the Sandcastle Help File Builder's help file for more information on the component and on
> token files in general.
> 
>


## Example

``` cs{title=" " source="SampleClass.cs" region="token Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.TokenExample){prefer-overload="true"}  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
