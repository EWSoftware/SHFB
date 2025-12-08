---
uid: a8ade450-e201-401a-923d-1a1169ef7828
alt-uid: exclude
title: exclude
keywords: "miscellaneous elements, exclude"
---

This element is used to indicate that a particular type or member should be excluded from the
documentation.



## Syntax

This top-level element is valid on all types and type members.


``` xml{title=" "}
<exclude />
```


## Remarks

This element takes precedence over all other visibility options and the type or member will not
appear in the resulting help file at all.


> [!NOTE]
> This is a custom XML comments element implemented by the <token>SHFB</token>.  It will not appear
> in the list of valid elements for XML comments IntelliSense.
> 
> 
> This element has been deprecated in favor of using the project's **API Filter**
> project property to exclude members.  The help file builder will translate members with this element into an API
> filter entry.  Be aware that if used on an overloaded member, all versions of the overloaded member will be
> removed from the documentation due to the way the API filter works in Sandcastle.
> 
>


## Example

``` cs{title=" "}
/// <summary>
/// This method will not appear in the help file even though it is public.
/// </summary>
/// <exclude />
public void UndocumentedMethod()
{
}
```


## See Also


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
