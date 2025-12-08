---
uid: 57C91630-95D6-4E3E-AF24-3415CC569AC8
alt-uid: ProjectSetup
title: Walkthrough: Enabling and Using XML Comments
keywords: enable XML comments, Visual Studio, Visual Studio projects, XML comments setup
---
This walkthrough will describe the steps needed to enable XML comments file output in your Visual Studio
projects, provides information on where to get more information on using XML comments to decorate your code, and
describes how to open help file builder projects from within Visual Studio.

<autoOutline lead="none" excludeRelatedTopics="true" />

## Enabling XML Comments File Generation
In order to create a help file that contains reference content (API documentation), you must enable XML comments
in your Visual Studio projects so that an XML file is generated to contain them when the project is built.

> [!TIP]
> If not using the Visual Studio IDE, the various managed language compilers support a
> */doc* command line option that will produce the XML comments file.  See your language
> compiler command line option documentation for details.

### All Projects Except Managed C++ Projects
1. In the Solution Explorer, right click on the project and select **Properties**.
2. Select the **Build** property page.

   > [!NOTE]
   > The XML comments filename is a configuration option.  As such, you can either select the
   > **All Configurations** option at the top of the page to set the XML comments options for all configurations
   > at the same time or select each configuration individually and repeat the next steps for each one.

3. In the **Output** section, check the checkbox next to the **Xml documentation file** text box and specify a
   name for the XML file.  Although not required, a common convention is to name the XML comments file after the
   related assembly (except with a *.xml* extension).  The assembly name can be found on the **Application**
   property page.
4. If you have a solution with multiple projects that need to be documented, repeat the above steps for each
   project in the solution.  It is recommended that you give each project's XML comments file a unique name.

If documenting a managed C++ project, the procedure differs and you need to follow these steps instead.

### Managed C++ Projects
1. In the Solution Explorer, right click on the project and select **Properties**.
2. Expand the **Configuration Properties** category and then expand the **C/C++** sub-category and select the
   **Output Files** option below it.

   > [!NOTE]
   > The XML comments file option is a configuration option.  As such, you can either select
   > the **All Configurations** option at the top of the dialog box to set the XML comments options for all
   > configurations at the same time or select each configuration individually and repeat the next step for each
   > one.

3. In the **Output Files** options, change the **Generate XML Documentation Files** option to **Yes (/doc)**.
4. By default, the comments file is named after the project target with a *.xml* extension and is placed in the
   target folder.  If you want to change the name, select the **XML Document Generator** category below the
   **Configuration Properties** category and change the filename using the **Output Document File** property.
5. If you have a solution with multiple projects that need to be documented, repeat the above steps for each
   project in the solution.  If you explicitly specify a name for the comments file, it is
   recommended that you give each project's XML comments file a unique name.

Once the above has been done, Visual Studio will create the XML comments file each time the project is built.

## Decorating Your Code
In addition to setting the project option to create the file, you must also add XML comments to your source code.
At a minimum, you should add a `<summary>` element to each public type and to the public and protected members of
those types.  There are many other elements available that will help improve the quality of your documentation.
See the other sections in this guide for more information

The XML comments elements and their format is consistent across all languages.  The only variable is the comment
characters that precede them.  Here are some examples.

``` cs{title="C#, C++, and F#"}
/// <summary>
/// This is an example of triple slash XML comments.  This is the most common
/// form of XML comments delimiter in C#, C++, and F# code.
/// </summary>

NOTE: The multi-line XML comments delimiters shown below are only supported by C# and C++.

/** <summary>
  * This is an example of using the multi-line XML comments delimiters.  In this
  * case, the common pattern "  * " at the start of each line after the first one
  * is ignored by the compiler and will not appear in the comments.
  * </summary>
  */

/**
<summary>This example does not use a common leading pattern on each line of
the summary comments using the multi-line XML comments delimiters.</summary>
*/

/** <summary>A single-line summary using the multi-line delimiters.</summary> */

```

``` vb
''' <summary>
''' This is an example of triple apostrophe XML comments.  These are the only
''' delimiters supported by Visual Basic.
''' </summary>
```

## Next Steps
Refer to the other sections in this guide for more information about the various XML comments elements and how to
use them.

## See Also
**Other Resources**  
[](@4268757F-CE8D-4E6D-8502-4F7F2E22DDA3)  
[](@515d5a54-5047-4d6f-bf51-d47c7c699cc2)  
[](@BD91FAD4-188D-4697-A654-7C07FD47EF31)  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
[](@f8464c0f-f62a-4faf-b11a-9a41173307e8)  
[](@d297bc14-33aa-4152-ae36-9f658b15de87)  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
