---
uid: B597E874-6D7C-4E05-B8F7-5B530C656B70
alt-uid: SharedComments
title: External XML Comments Files
keywords: "namespace comments, external comments files"
---

Project summary and namespace comments can be maintained manually by creating a standalone XML
comments file to contain the information.


> [!NOTE]
> Project summary comments will appear on the Namespace List topic in the compiled help file.
> 
>

Start by creating an XML comments file like the following and give it a unique name that will not
conflict with any other XML comments file in the related projects.


``` xml{title="Sample Project/Namespace XML Comments File"}
<?xml version="1.0"?>
<doc>
  <assembly>
    <name>_NamespaceComments_</name>
  </assembly>
  <members>
    <member name="R:Project_CustomControlsHelp">
      <summary>
        Project summary comments go in here.
      </summary>
    </member>
    <member name="N:">
      <exclude />
      <summary>
        Exclude the global namespace from the help file.
      </summary>
    </member>
    <member name="N:CustomControls.Internal">
      <exclude />
    </member>
    <member name="N:CustomControls.Design">
      <summary>
        Common namespace summary info for the CustomControls.Design
        namespace go here.
      </summary>
    </member>
  </members>
</doc>
```

> [!IMPORTANT]
> The project summary comments will be placed in the member with the ID starting with
> "`R:Project_`".  The ID must be suffixed with a unique value such as your help project's
> name (exclude any spaces in the suffix).  This is required so that the root namespace container has a unique name
> that will not cause any conflicts when building MS Help Viewer output.
> 
>

Next, do one or more of the following:


- To specify project summary comments, add them to the `<summary>`
tag in the **R:Project_[HtmlHelpName]** node.  As noted above, substitute a unique value
for the ID's suffix.  If you do not have any project summary notes, you may remove this node from the file.
- To exclude a namespace from the help file, add a `<member>`
node, set it's `name` attribute to the namespace name prefixed with "`N:`",
and place an `<exclude />` tag in it.  Any other tags are optional.  The example
above would exclude the unnamed global namespace and the `CustomControls.Internal`
namespace from the help file if your build tool supports it.
- To specify namespace summary comments, add a `<member>` node,
set it's `name` attribute to the namespace name prefixed with "`N:`",
and add the comments to the `<summary>` tag in the node.
- To specify namespace group summary comments, add a `<member>` node,
set it's `name` attribute to the namespace group name prefixed with "`G:`",
and add the comments to the `<summary>` tag in the node.


Once you have created the file, you can add it to the build tool that you use to include the comments
in the help file when it is built.  Refer to your build tool's documentation for further information



## See Also


**Other Resources**  
[](@BD91FAD4-188D-4697-A654-7C07FD47EF31)  
[](@41B2D835-DB0D-4828-8D9E-0E423EDA4590)  
