Project and Item Template Notes
===============================
In order to correctly build the project and item template files and include them in the VSIX container ready
for installation, the following conditions must be met.

1. All template files must reside in a .\Templates folder at the root of the project.  Beneath it, a
   .\Templates\Projects folder will contain the project file templates (one subfolder beneath it for each project
   type).  Item templates go in the .\Templates\ProjectItems folder (one subfolder beneath it for each item
   type).  You can further group the files into subfolders beneath them.

2. Project item template files must have the "Include in VSIX" property set to True and the "VSIX Sub Path"
   property set to "ItemTemplates\SHFBProject\[MainFolder]\[Subfolder]\[ContainingFolder]" where
   "[MainFolder]" is the root folder name under .\ProjectItems, "[Subfolder]" is the subfolder under the main
   folder, and "[ContainingFolder]" is the containing folder of the file.  Include spaces between words as the
   folder name will be used as the category title in the Add New Item dialog box.  For example:

        ItemTemplates\SHFBProject\Conceptual Content\Project Files\CodeSnippets

3. For project templates, each project file must have the "Include in VSIX" property set to True and the
   "VSIX Sub Path" property set to "ProjectTemplates\SHFBProject\Documentation\[NameOfTemplateProject" where
   "[NameOfTemplateProject]" is the name of the folder containing the template file.  For example:

        ProjectTemplates\SHFBProject\Documentation\BuildComponentCS

    The SandcastleBuilderProject template is the exception.  Omit the "\Documentation\" subfolder from its
    VSIX Sub Path folder name.

4. Add the Microsoft.VisualStudio.ProjectTemplate and Microsoft.VisualStudio.ItemTemplate items to the Assets
   category of the source.extension.vsixmanifest file.
