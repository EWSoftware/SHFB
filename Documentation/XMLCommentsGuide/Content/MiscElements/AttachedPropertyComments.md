---
uid: c0346d23-f376-4948-8f9a-d17b2f1acef3
alt-uid: AttachedPropertyComments
title: AttachedPropertyComments
keywords: "miscellaneous elements, AttachedPropertyComments"
---

This element is used to define the content that should appear on the auto-generated attached property
member topic for a given WPF dependency property member.



## Syntax

This top-level element is valid on any dependency property member.  The member on which the element
appears should have its own set of member-specific XML comments as well.


``` xml{title=" "}
<AttachedPropertyComments>
  <summary>
  Summary description
  </summary>
  [<remarks>Optional remarks</remarks>]
  [<example>Optional examples</example>]
  [... other top-level comments elements as needed ...]
</AttachedPropertyComments>
```

Include any top-level XML comments elements as you would on a standard member.  These elements will
be formatted in an identical fashion and will appear in the auto-generated attached property member topic.


> [!NOTE]
> This is a custom XML comments element implemented by the <token>SHFB</token>.  It will not appear
> in the list of valid elements for XML comments IntelliSense.
> 
>


## Remarks

Because the attached property and attached event members of WPF classes are compiler-generated,
there is no way to associate XML comments with them directly without managing a standalone XML comments file.
While it is possible to do this, it is less convenient than keeping the comments in the code.  The help file
builder provides a solution to this through its **GenerateInheritedDocs** tool.  As part
of the process of generating inherited documentation, the tool will look for attached property and attached event
fields.  If it finds them, it will automatically inherit their comments for the related compiler-generated
members as default comments to prevent a "missing comments" warning.


In addition, if it finds comments for those field members, it will check for an
`AttachedPropertyComments` element (for attached properties) or an
`AttachedEventComments` element (for attached events) and, if found, will use the XML
comments nested within those elements for the related compiler-generated members.  This allows you to provide
comments for the field member and the related compiler-generated member that are entirely different but are
managed from within the code.


> [!NOTE]
> Because the attached property and event members are compiler-generated, you must fully qualify
> their names if you want to create a link to them with a `see` element as shown in the
> example below.
> 
>


## Example

``` cs{title=" " source="DocumentationInheritance\AttachedEventsAndProperties.cs" region="IsBroughtIntoViewWhenSelected attached property"}
```


## See Also


**Reference**  
[](@P:XMLCommentsExamples.DocumentationInheritance.AttachedEventsAndPropertiesTest.IsBroughtIntoViewWhenSelected){prefer-overload="true"}  
[](@F:XMLCommentsExamples.DocumentationInheritance.AttachedEventsAndPropertiesTest.IsBroughtIntoViewWhenSelectedProperty)  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
