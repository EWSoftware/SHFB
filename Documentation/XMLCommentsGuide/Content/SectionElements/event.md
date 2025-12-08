---
uid: 81bf7ad3-45dc-452f-90d5-87ce2494a182
alt-uid: event
title: event
keywords: "section elements, event"
---

This element is used to list events that can be raised by a type's member.



## Syntax

This top-level element is valid on any member in which an event can be raised.
*eventType* is the name of the event type that can be raised.


``` xml{title=" "}
<event cref="eventType">description</event>
```


## Remarks

There should be one `event` element for each event type that can be
raised.


> [!NOTE]
> This is a custom XML comments element implemented by Sandcastle.  It will not appear in the list
> of valid elements for XML comments IntelliSense.
> 
>


## Example

``` cs{title=" " source="SampleClass.cs" region="event Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.PerformAnAction){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
