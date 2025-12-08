---
uid: 5b11b235-2b6c-4dfc-86b0-2e7dd98f2716
alt-uid: overloads
title: overloads
keywords: "miscellaneous elements, overloads"
---

This element is used to define the content that should appear on the auto-generated overloads topic
for a given set of member overloads.



## Syntax

This top-level element is valid on any overloaded member.  However, it should only appear on one
of the members in each overload set.  The member on which the element appears should have its own set of
member-specific XML comments as well.


``` xml{title=" "}
<overloads>description</overloads>

or

<overloads>
  <summary>
  Summary description
  </summary>
  [<remarks>Optional remarks</remarks>]
  [<example>Optional examples</example>]
  [... other top-level comments elements as needed ...]
</overloads>
```

In its simplest form, the element contains a description that will appear as the summary in the
auto-generated overloads member topic.  The expanded form allows you to include any top-level XML comments
elements as you would on a standard member.  These elements will be formatted in an identical fashion and will
appear in the auto-generated overloads member topic.



## Example

``` cs{title=" " source="SampleClass.cs" region="overloads Examples"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.SumValues(System.Collections.Generic.IEnumerable{System.Int32})){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.AverageValues(System.Collections.Generic.IEnumerable{System.Double})){prefer-overload="true"}  


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
