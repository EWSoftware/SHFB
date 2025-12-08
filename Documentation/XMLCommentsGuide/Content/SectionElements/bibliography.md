---
uid: 4C57ED2C-F373-4B34-B983-A0009D6CDAB4
alt-uid: bibliography
title: bibliography/cite
keywords: "section elements, bibliography", "section elements, cite"
---

The auto-generated bibliography feature, an extension provided by the presentation styles in the
Sandcastle Help File Builder, allows an author to create an external bibliography data XML file and then share it
between multiple topics by specifying short identifiers in `cite` elements that match one
of the references contained in the file.  When one or more `cite` elements are used,
Sandcastle will automatically generate a bibliography for the topic just before the See Also section and the
`cite` elements themselves will be replaced with hyperlinks to corresponding bookmarked
entries in the bibliography section.


> [!NOTE]
> Unlike MAML topics, the `bibliography` element is not specified anywhere in
> the XML comments.  The section is added automatically if any `cite` elements are present.
> 
>


## Usage

To use citations, a bibliography data XML file must be created to store the identifiers and entries.
Add the file to the project and specify the file's name in the Transformation Arguments section of the project
properties, typically the `BibliographyDataFile` argument.  An example file is shown
below.  Note that not all presentation styles support the bibliography elements such as Open XML-based styles.
In such cases they will be ignored.

### Defining Citation Identifiers and Bibliography Entries
1. If it does not already exist, add a new bibliography data file to the project.  An item template is supplied
   with the help file builder standalone GUI and Visual Studio package.  The suggested name is *bibliography.xml*
   but you can name it what you like.  Set the `BuildAction` to `None` so that the build ignores it.
2. Add content similar to the following to the bibliography XML file.

   ``` xml{title="Sample Bibliography.xml File"}
   <?xml version="1.0" encoding="utf-8"?>
   <bibliography>
     <reference name="sandcastle activity">
       <title>Sandcastle Help File Builder Activity Statistics</title>
       <author>Eric Woodruff</author>
       <publisher>GitHub.com</publisher>
       <link>https://GitHub.com/EWSoftware/SHFB/pulse/monthly</link>
     </reference>
     <reference name="SHFB">
       <title>Sandcastle Help File Builder</title>
       <author>Eric Woodruff</author>
       <publisher>GitHub.com</publisher>
       <link>https://GitHub.com/EWSoftware/SHFB</link>
     </reference>
   </bibliography>
   ```

   The previous example defines a bibliography XML file that contains two `reference`
   elements, which can be referred to by `cite` elements in XML comments using the values
   of the `name` attributes.

   > [!TIP]
   > Reference names are not case-sensitive.

   The `title` and `author` elements are required.  The `publisher` and `link` elements are optional.  The only
   supported content type for each element is plain text.

### Using the Bibliography in XML Code Comments
Use `cite` elements to reference items in the bibliography.  The inner text of the element is the value of the
`name` attribute for the entry.  The Sandcastle transformations will add a bibliography section to the API
member's page automatically if any `cite` elements are found.

#### Example
``` cs{title=" " source="SampleClass.cs" region="Bibliography cite Example"}
```

## See Also
**Reference**  
[](@M:XMLCommentsExamples.SampleClass.BibliographyCiteExample){prefer-overload="true"}  

**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
