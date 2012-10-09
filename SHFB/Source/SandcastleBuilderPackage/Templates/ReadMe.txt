Project and Item Template Notes
===============================
In order to correctly build the project and item template files and include them in the VSIX container ready
for installation, the following modifications must be made to the project file manually:

1. Edit the .csproj file for the package and add the following property group.  The first property defines the
   project language and should match the value from the LanguageVsTemplate named parameter on the package's
   ProvideProjectFactory attribute.  The second sets up the call to the target added in step 2.

  <PropertyGroup>
    <!-- These are needed to properly build the project and item templates and include them in the VSIX
         container ready for installation.  Deployment is suppresssed since we are using VSTemplate files. -->
    <VsTemplateLanguage>SHFBProject</VsTemplateLanguage>
    <GetVsixSourceItemsDependsOn>$(GetVsixSourceItemsDependsOn);GetVsixTemplateItems</GetVsixSourceItemsDependsOn>
    <DeployVSTemplates>false</DeployVSTemplates>
  </PropertyGroup>

2. Add the following target to the end of the project file.  This will include the project and item templates
   in the generated VSIX container automatically.

  <!-- This target is needed to include the project and item templates in the VSIX container. -->
  <Target Name="GetVsixTemplateItems" DependsOnTargets="ZipProjects;ZipItems">
    <ItemGroup>
      <VSIXSourceItem Include="@(IntermediateZipItem)">
        <VSIXSubPath>ItemTemplates\%(IntermediateZipItem.Language)\%(IntermediateZipItem.OutputSubPath)\%(IntermediateZipItem.Culture)</VSIXSubPath>
      </VSIXSourceItem>
      <VSIXSourceItem Include="@(IntermediateZipProject)">
        <VSIXSubPath>ProjectTemplates\%(IntermediateZipProject.Language)\%(IntermediateZipProject.OutputSubPath)\%(IntermediateZipProject.Culture)</VSIXSubPath>
      </VSIXSourceItem>
    </ItemGroup>
  </Target>

3. All template files must reside in a .\Templates folder at the root of the project.  Beneath it, a
   .\Templates\Projects folder will contain the project file templates (one subfolder beneath it for each project
   type).  Item templates go in the .\Templates\ProjectItems folder (one subfolder beneath it for each item
   type).  You can further group the files into subfolders beneath them.

4. If you want to group the item templates into categories, you must manually edit the .csproj file and add an
   OutputSubPath property to each ZipItem element related to the added template files as shown in the example
   below.  You must do this whenver you add item templates to the project.  The value becomes the title of the
   category in the Add New Item dialog.

    <!-- Each ZipItem must contain an OutputSubPath element to define the category into which it is grouped
         in the Add New Item dialog box. -->
    <ZipItem Include="Templates\ProjectItems\AdditionalContent\Bitmap\Bitmap.bmp">
      <OutputSubPath>Additional Content</OutputSubPath>
    </ZipItem>
    <ZipItem Include="Templates\ProjectItems\AdditionalContent\Bitmap\Bitmap.vstemplate">
      <OutputSubPath>Additional Content</OutputSubPath>
    </ZipItem>

   This does not apply to project templates unless you have more than one and want to categorize them.

5. Add the following two lines to the <content> section of the source.extension.vsixmanifest file:

    <ProjectTemplate>ProjectTemplates</ProjectTemplate>
    <ItemTemplate>ItemTemplates</ItemTemplate>

   NOTE: If you edit the file using the VSIX editor, you will need to manually add these two lines
         back to the file!
