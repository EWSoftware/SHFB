---
uid: 2a973959-9c9a-4b3b-abcb-48bb30382400
alt-uid: revisionHistory
title: revisionHistory
keywords: "section elements, revisionHistory"
---

This element is used to display revision history for a type or its members.  It originated in the
old NDoc and VBCommenter tools but is still useful today to communicate revision history in the documentation.



## Syntax

This top-level element is valid on all types and type members.


``` xml{title=" "}
<revisionHistory [visible="true|false"]>
	<revision date="MM/dd/yyyy" version="#.#.#.#" [author="XXXXX"] [visible="true|false"]>
		Comments about the revision.
	</revision>
</revisionHistory>
```

> [!NOTE]
> This is a custom XML comments element implemented by the <token>SHFB</token>.  It will not appear
> in the list of valid elements for XML comments IntelliSense.
> 
>

An optional `visible` attribute on the `revisionHistory`
parent element controls visibility of the section within the generated topic.  If omitted or set to true, the
revision history is included.  If set to false, it is not included in the topic.


The `revision` elements describe one or more revisions to the type or
member.  The following attributes are supported:


date
:   This attribute specifies the date of the revision.  The value is treated as a string field and
    is displayed as-is in the generated help topic.  No locale date formatting is applied.
    

version
:   This attribute specifies the version in which the revision was made. As with the date, the
    value is treated as a string field and is displayed as-is in the generated help topic.
    

author
:   This attribute is optional and specifies the name of the person that made the revision.  This
    attribute's value will not appear in the generated help topic.
    

visible
:   This optional attribute can be used to control visibility of the revision in the generated
    topic.  If omitted or set to true, the revision entry is included.  If set to false, it is not included in the
    topic.
    


The content of the `revision` element can contain other XML comments
elements similar to a remarks section to add additional details.



## Example

``` cs{title=" " source="RevisionHistoryClass.cs" region="Revision history examples"}
```


## See Also


**Reference**  
[](@T:XMLCommentsExamples.RevisionHistoryClass)  
[](@P:XMLCommentsExamples.RevisionHistoryClass.Revision)  
[](@M:XMLCommentsExamples.RevisionHistoryClass.ExampleMethod){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
