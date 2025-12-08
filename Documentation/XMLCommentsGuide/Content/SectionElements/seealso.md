---
uid: 16cdb957-a35b-4c17-bf5e-ea511b0218e3
alt-uid: seealso
title: seealso
keywords: "section elements, seealso"
---

This element is used to create a link to another API topic or an external website within the
**See Also** section of a topic.

<autoOutline lead="none">2</autoOutline>


## Syntax

This top-level element can be used on any type or its members.  Any of the following formats may be
used based on the need.



#### Code Reference

``` xml{title=" "}
<seealso cref="member" [qualifyHint="true"] [autoUpgrade="true"] />
	
or

<seealso cref="member" [autoUpgrade="true"]>inner text</seealso>
```

The code reference form uses a `cref` attribute to specify the
member name to which the link should be connected.  The compiler will check that the target member name exists
and will generate a warning if it is not found.  The member name does not have to be fully qualified as long as
the reference is within the containing class or if the appropriate `using`
(`Imports` in VB) statements are present.


If the self-closing form is used, the inner text of the link will be set to the member name
without a namespace or type qualifier.  You can specify inner text on the element to use an alternate value for
the link text or to add a qualifying namespace or type to the member name.  By adding the
`qualifyHint` attribute and setting it to true, you can indicate that the inner text
should be qualified.  Type names are fully qualified with their namespace.  Class members are qualified with
their class name.


When specifying type, field, property, event, and non-overloaded method names, the name alone
will suffice as the `cref` attribute value.  However, when referencing overloaded methods,
you must supply parameter type information to help the compiler determine to which overload you are referring.
The parameter type names are enclosed in parentheses and can be specified using the types for the language being
used (i.e. `int` or `string` for C# or `Integer`
or `String` for VB).  They can also be specified using the .NET Framework type names such
as `Int32`, `System.Int32`, etc.  If an overload takes no
parameters, just specify an empty set of parentheses.  If you want to link to the auto-generated member overloads
help topic, you can specify the `autoUpgrade` attribute.  If set to false or omitted, the
link will take you directly to the referenced member.  If set to true and the method has overloads, the link will
take you to the method overloads page instead.  If the method has no overloads, it works the same as	if set to
false and it takes you to the method page.  This is equivalent to the same attribute on the MAML
`codeEntityReference` element.  Another alternative is to use the fully qualified member
name with an "`O:`" prefix.  See the [Method Overload Examples](#Examples)
below for details.


> [!TIP]
> To link to the root namespace container page, use the ID `R:Project_[HtmlHelpName]`
> where "`[HtmlHelpName]`" is the value of your project's HTML Help Name property with
> spaces replaced by underscores. Use the **Entity References** tool window to search for the ID if in doubt
> about its value.
> 
>


#### External Reference

``` xml{title=" "}
<seealso href="url" [target="_blank | _self | _parent | _top"]
    [alt="alternate text"] />
	
or

<seealso href="url" [target="_blank | _self | _parent | _top"]
    [alt="alternate text"]>inner text</seealso>
```

The external reference form is a customization implemented by Sandcastle.  It uses an
`href` attribute to specify the URL of an external website or resource to which the link
should be connected.  If the self-closing form is used, the inner text of the link will be set to the URL.  You
can specify inner text on the element to use an alternate value for the link text.


The optional `target` attribute can be used to specify where the
content will be opened.  If not specified, `_blank` is used to open the content in a new
browser window.  The `_self` option can be used to replace the current topic with the
content of the target URL.  The `_parent` and `_top` options can be
used to force the content to be displayed in the parent frame or full window if the current topic is hosted in
one of those manners.


The optional `alt` attribute can be used to specify alternate text to
display for the link's tool tip when the mouse hovers over it.  If not specified, no alternate text is added for
the tool tip.



## Remarks

Use this element to insert a link in the **See Also** section of the topic.
Use the [](@983fed56-321c-4daf-af16-e3169b28ffcd) element to insert an inline link.


> [!NOTE]
> Although listed as a top-level element, the `seealso` element can be used
> as an inline element like the `see` element.  The difference is that this element will not
> render anything inline.  As with its top-level usage, it only adds an entry in the **See Also**
> section.
> 
>


## Examples

``` cs{title="Code Reference Examples" source="SampleClass.cs" region="see/seealso cref Examples"}
```

``` cs{title="Method Overload Examples" source="SampleClass.cs" region="see/seealso Method Overload Examples"}
```

``` cs{title="External Reference Examples" source="SampleClass.cs" region="see/seealso href Examples"}
```


## See Also


**Reference**  
[](@E:XMLCommentsExamples.SampleClass.SomethingHappened)  
[](@M:XMLCommentsExamples.SampleClass.OnSomethingHappened){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.SeeElementExternalExample){prefer-overload="true"}  
[](@M:XMLCommentsExamples.SampleClass.DoSomething){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@983fed56-321c-4daf-af16-e3169b28ffcd)  
[](@db2703b4-12bc-4cf5-8642-544b41002809)  
