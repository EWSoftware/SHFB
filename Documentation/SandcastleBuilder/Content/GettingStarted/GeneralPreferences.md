---
uid: 6a35198a-3713-4eb7-929d-555fddc0ccb6
alt-uid: GeneralPreferences
title: General Preferences
keywords: "Getting started, user preferences - general"
---
This topic describes the general user preferences.  Most are common to both the standalone GUI and the Visual
Studio extension package.  Options that only apply to one or the other are noted in that option's comments.

- **Alternate MS Help Viewer (.mshc) Viewer Path** - This allows you to specify the path to an external
  application that can be used to view MS Help Viewer (.mshc) files if you have one.  If you want to use the
  default MS Help Viewer, you should leave this value blank.
- **ASP.NET Development Web Server Port** - This allows you to specify the port number to use when starting the
  built-in .NET development web server to view website output.  By default, it is set to port 12345.  You can
  alter this if it conflicts with something on your system.
- **Save window state per project for each user** - (Standalone GUI only)  This allows you to save the current
  window layout and currently open file editors when a project is closed.  The window state is saved to the same
  folder as the project using the project filename with a "*_[USERNAME]*" suffix where `USERNAME` is the user ID
  of the currently logged in user.  If turned off, only the location of the Project Explorer and Project
  Properties window are saved in the general user preferences.  The Visual Studio shell handles this
  automatically for the extension package.
- **Before Building** - (Standalone GUI only)  This lets you specify whether or not the help file builder should
  save the project and/or modified document windows prior to performing a build or previewing a topic.  The
  options are:
  - Save all changes - Save the project and any modified document editors.
  - Save changes to open documents only - Only open, modified document editors are saved.  Changes to the project
    file are not saved.
  - Prompt to save all changes - You will be asked to save all changes.  If documents are not saved, the
    resulting help file may not contain current information.
  - Don't save any changes - Nothing is saved.  The resulting help file may not contain current information if
    there are unsaved documents.

  The Visual Studio shell will save files for the extension package according to the **Before building** option
  in the **Tools** | **Options** | **Projects and Solutions** | **Build and Run** category.
- **Build output verbose logging enabled** - This allows you to specify whether or not full logging information
  is displayed in the output window during a build.  In the standalone GUI, it is checked by default.  In the
  Visual Studio extension package, it is unchecked by default.  When unchecked, only build step messages are
  reported during the build.  All messages are still written to the log file.  This is useful for very large
  projects where the generated output can consume large quantities of memory.
- **Open build log viewer on failed build** - (Visual Studio extension package only)  If enabled, the build log
  viewer tool window will be opened after a failed build to display the full log content so that you can diagnose
  the problem.
- **Open help file after successful build** - If enabled, the help file will be opened after a successful build.
  The format opened will depend on the format chosen in the `Help File Format` project property.  Preference is
  given to Help 1, then MS Help Viewer, markdown, and finally website output.  In the standalone GUI, this option
  can also be toggled on and off via the **Documentation | View Help File** menu.
- **Enable Markdown/MAML/XML comments Go To Definition** - (Visual Studio extension package only)  If checked,
  hovering over certain link target attribute values or element inner text within Markdown topics, MAML topics,
  and XML comments in C# projects will provide more information about the target (i.e. the topic title and
  filename for Markdown/MAML target links) and the option to go to the definition of the item if possible when
  the Go To Definition context menu command is selected or Ctrl+Click is pressed while hovering over the text.
  Some limitations apply.  See the [](@ba11d6d5-2f99-4b26-b384-21324ef1b49a) topic for details.

  The sub-option allows you to enable or disable the Ctrl+Click Go To Definition feature.  This allows you to
  disable it if you prefer to use the context menu option or its hot key (F12 by default) to invoke the Go To
  Definition option.

  These options can be disabled individually or entirely if you do not wish to use the Go To Definition support
  or if you think the options are causing issues with Visual Studio.
- **Build Output Background/Foreground/Font** - (Standalone GUI only)  These options let you specify the
  background color, foreground color, and font used in the output window for the build output (the text shown
  during the build).
- **Text Editor Font** - (Standalone GUI only)  This option lets you set the font used in the text editor
  windows.  The syntax highlighter controls the colors and they are not editable.
- **Show line numbers in text editor** - (Standalone GUI only)  If enabled, all text editor windows will show
  line numbers down the left side.  If not enabled, no line numbers are shown within the text editor windows.
- **Enter matching brackets, parentheses, and quotes** - (Standalone GUI only)  If enabled, all editor windows
  will automatically insert the matching closing character when an opening bracket, parentheses, or double quote
  is entered.  If not enabled, the matching character is not entered automatically.

## See Also
**Other Resources**  
[](@2152ed96-bf69-4b9b-b1a7-4fffc71b3095)  
