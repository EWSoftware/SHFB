---
uid: 4268757F-CE8D-4E6D-8502-4F7F2E22DDA3
alt-uid: Welcome
title: Welcome
keywords: Welcome
---

Welcome to the Sandcastle XML Comments Guide.  This is intended to be a reference that you can use to
find out all that you need to know about XML comments as used with Sandcastle to produce help files containing
API reference content.  If you have any information, tips, updates, or corrections that you would like to see
added to the guide, feel free to submit them to the author using the **Send Feedback** link
in the page footer.



## Conventions

The XML comment elements are separated into four categories:


- [Section](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb) - These elements are
used to define the content for the different sections of the documentation of a type or member and are always
top-level elements that appear at the root of the XML comments block.  Examples are `summary`
and `remarks`.
- [Block](@f8464c0f-f62a-4faf-b11a-9a41173307e8) - These elements format
text within the top-level section elements. They are typically used to add structure to the text inside those
elements.  Examples are `code` and `list`.
- [Inline](@d297bc14-33aa-4152-ae36-9f658b15de87) - These elements are
typically used inside the other section and block elements to provide formatting such as the `c`
element or links to other API topics such as the `see` element.
- [Miscellaneous](@9341fdc8-1571-405c-8e61-6a6b9b601b46) - These elements
are typically top-level or mixed use elements that do not fit one of the other categories such as
`include` and `inheritdoc`.


For each XML comments element, information will be given about its placement and usage along with
one or more examples.  In the syntax examples, optional attributes and parameters will be enclosed in square
brackets ([ ]).  If an attribute allows you to specify a value from a defined list of values, those values will
be shown separated by a pipe character (|).  The **See Also** section will contain links
to example API members so that you can see how a topic containing the element is rendered in an actual help
file.


Certain elements or attributes they utilize are implemented by Sandcastle or are only usable with a
particular third-party tool such as the <token>SHFB</token>.  Such extensions to the standard XML comments
elements and attributes will be noted in the affected topics.  Such extensions do not appear in Visual Studio's
XML comments IntelliSense.  However, an extension is available that can add them.  See the
[](@7F003236-9AF3-4B6A-8F14-4124FFF0AD7B) topic for more information.


Since the syntax of the XML comments themselves are language neutral with the exception of the
comment characters that precede them, all example code is shown only in C# to reduce unnecessary clutter.
Substitute your language's XML comments characters (i.e. triple apostrophe (`'''`) for VB)
and the XML comments will work the same way.



## See Also


**Other Resources**  
[](@57C91630-95D6-4E3E-AF24-3415CC569AC8)  
[](@515d5a54-5047-4d6f-bf51-d47c7c699cc2)  
[](@0999CAAA-4992-4352-9ED2-965892040176)  
[](@BD2B1A8B-275D-4584-9FB1-92759FAF37D7)  
