---
uid: fb4625cb-52d0-428e-9c7c-7a0d88e1b692
alt-uid: threadsafety
title: threadsafety
keywords: "section elements, threadsafety"
---

This element is used to indicate whether or not a class or structure's static and instance members
are safe for use in multi-threaded scenarios.



## Syntax

This top-level element is valid on all classes and structures.


``` xml{title=" "}
<threadsafety static="true | false" instance="true | false" />

or

<threadsafety>Add a custom description of the type's thread safety</threadsafety>

```

The `static` attribute specifies whether static members of the class or 
structure are safe for use in multi-threaded operations.  The `instance` attribute
specifies whether instance members of the class or structure are safe for use in multi-threaded operations.  If
neither attribute is specified, it is the same as setting `static` to true and
`instance` to false.  Inner text can also be specified to describe the thread safety.  If
inner text is present, the attributes are ignored.


> [!NOTE]
> This is a custom XML comments element implemented by Sandcastle.  It will not appear in the list
> of valid elements for XML comments IntelliSense.
> 
>


## Examples

``` cs{title=" " source="ThreadSafetyClass.cs" region="threadsafety Example"}
```


## See Also


**Reference**  
[](@T:XMLCommentsExamples.ThreadSafetyClass)  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
