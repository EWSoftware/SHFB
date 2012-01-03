//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderProjectNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that represents a project node in a Sandcastle
// Help File Builder Visual Studio project.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/22/2011  EFW  Created the code
// 1.9.3.3  11/19/2011  EFW  Added support for drag and drop from Explorer
//=============================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleDataObject = Microsoft.VisualStudio.OLE.Interop.IDataObject;
using OleConstants = Microsoft.VisualStudio.OLE.Interop.Constants;
using VsCommands = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;
using VsCommands2K = Microsoft.VisualStudio.VSConstants.VSStd2KCmdID;
using VsMenus = Microsoft.VisualStudio.Project.VsMenus;

using SandcastleBuilder.Package.Automation;
using SandcastleBuilder.Package.Properties;
using SandcastleBuilder.Package.PropertyPages;
using SandcastleBuilder.Package.UI;

using SandcastleBuilder.Utils.Design;
using SandcastleProject = SandcastleBuilder.Utils.SandcastleProject;
using SandcastleBuildAction = SandcastleBuilder.Utils.BuildAction;

namespace SandcastleBuilder.Package.Nodes
{
    /// <summary>
    /// This class represents a project node in a Sandcastle Help File Builder
    /// Visual Studio project.
    /// </summary>
    [Guid("174F6967-9561-4f98-A5D3-06FED2531173")]
    public class SandcastleBuilderProjectNode : ProjectNode
    {
        #region Private data members
        //=====================================================================

        // The index and the image list for the project node
        private static int imageIndex;
        private static ImageList imageList = Utilities.GetImageList(
            typeof(SandcastleBuilderProjectNode).Assembly.GetManifestResourceStream(
                "SandcastleBuilder.Package.Resources.SandcastleBuilderProjectNode.bmp"));

        private SandcastleProject sandcastleProject;
        private Process webServerInstance;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is overridden to return the project's GUID
        /// </summary>
        /// <returns>The Sandcastle Help File Builder package GUID</returns>
        public override Guid ProjectGuid
        {
            get { return GuidList.guidSandcastleBuilderProjectFactory; }
        }

        /// <summary>
        /// This returns a caption for <c>VSHPROPID_TypeName</c>
        /// </summary>
        /// <returns>Returns "SHFBProject"</returns>
        public override string ProjectType
        {
            get { return "SHFBProject"; }
        }

        /// <summary>
        /// This returns the index in the image list of the project node icon
        /// </summary>
        public override int ImageIndex
        {
            get { return imageIndex; }
        }

        /// <summary>
        /// This is overridden to prevent dropping on project properties nodes.  All other nodes can accept drops
        /// </summary>
        /// <param name="itemId">The drop target node ID</param>
        /// <returns>True if the drop can occur, false if not</returns>
        protected internal override bool CanTargetNodeAcceptDrop(uint itemId)
        {
            HierarchyNode targetNode = NodeFromItemId(itemId);

            if(targetNode is ProjectPropertiesContainerNode)
                return false;

            return true;
        }

        /// <summary>
        /// This read-only property returns a <see cref="SandcastleProject"/>
        /// instance that wraps the current MSBuild project.
        /// </summary>
        public SandcastleProject SandcastleProject
        {
            get
            {
                if(sandcastleProject == null)
                    sandcastleProject = new SandcastleProject(this.BuildProject);
                else
                    sandcastleProject.RefreshProjectProperties();

                return sandcastleProject;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="package">The package to which the project node is
        /// related.</param>
        public SandcastleBuilderProjectNode(ProjectPackage package)
        {
            this.Package = package;

            // Add the project node images
            imageIndex = this.ImageHandler.ImageList.Images.Count;

            foreach(Image img in imageList.Images)
                this.ImageHandler.AddImage(img);

            // Allow destructive deletes
            this.CanProjectDeleteItems = true;

            // Use the project designer for the property pages
            this.SupportsProjectDesigner = true;

            this.InitializeCATIDs();
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// Provide mappings from our browse objects and automation objects to
        /// our CATIDs.
        /// </summary>
        private void InitializeCATIDs()
        {
            // The following properties classes are specific to Sandcastle
            // Builder so we can use their GUIDs directly.
            base.AddCATIDMapping(typeof(SandcastleBuilderProjectNodeProperties),
                typeof(SandcastleBuilderProjectNodeProperties).GUID);
            base.AddCATIDMapping(typeof(SandcastleBuilderFileNodeProperties),
                typeof(SandcastleBuilderFileNodeProperties).GUID);
            base.AddCATIDMapping(typeof(DocumentationSourceNodeProperties),
                typeof(DocumentationSourceNodeProperties).GUID);

            // The following are not specific to Sandcastle Builder and as such we need a separate GUID
            // (we simply used guidgen.exe to create new guids).
            base.AddCATIDMapping(typeof(ProjectNodeProperties), new Guid("CD4A4A5D-345C-4faf-9BDC-AB3F04DEE02F"));
            base.AddCATIDMapping(typeof(FolderNodeProperties), new Guid("9D0F0FAA-F7B1-43ee-A8CF-046B80F2384B"));
            base.AddCATIDMapping(typeof(ReferenceNodeProperties), new Guid("0B6EF0B6-8699-470d-A8A1-16F745810073"));
            base.AddCATIDMapping(typeof(ProjectReferencesProperties), new Guid("C67FE7AC-629F-458e-A3B9-597E69C4C41D"));

            // This one we use the same as Sandcastle Builder file nodes since both refer to files
            base.AddCATIDMapping(typeof(FileNodeProperties), typeof(SandcastleBuilderFileNodeProperties).GUID);
        }

        /// <summary>
        /// Handles common command status checking for the various node types
        /// </summary>
        /// <param name="cmdGroup">A unique identifier of the command group</param>
        /// <param name="cmd">The command for which to query the status</param>
        /// <param name="result">An out parameter specifying the QueryStatusResult of the command.</param>
        /// <returns>Returns true if handled, false if not.</returns>
        private static bool QueryStatusOnCommonCommands(Guid cmdGroup, uint cmd, ref QueryStatusResult result)
        {
            if(cmdGroup == VsMenus.guidStandardCommandSet97)
            {
                switch((VsCommands)cmd)
                {
                    case VsCommands.ClearBreakpoints:
                    case VsCommands.DebugProcesses:
                    case VsCommands.Start:
                    case VsCommands.StartNoDebug:
                    case VsCommands.StepInto:
                    case VsCommands.StepOut:
                    case VsCommands.StepOver:
                    case VsCommands.ToolboxAddItem:
                    case VsCommands.ToolsDebugProcesses:
                    case VsCommands.ToggleBreakpoint:
                        result |= QueryStatusResult.INVISIBLE | QueryStatusResult.SUPPORTED;
                        return true;
                }
            }

            if(cmdGroup == VsMenus.guidStandardCommandSet2K)
            {
                switch((VsCommands2K)cmd)
                {
                    case VsCommands2K.PROJSTARTDEBUG:
                    case VsCommands2K.PROJSTEPINTO:
                        result |= QueryStatusResult.INVISIBLE | QueryStatusResult.SUPPORTED;
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Open the current project in the standalone SHFB GUI
        /// </summary>
        private void OpenInStandaloneGui()
        {
            try
            {
                string gui = Path.Combine(Environment.ExpandEnvironmentVariables("%SHFBROOT%"),
                    "SandcastleBuilderGUI.exe");

                if(File.Exists(gui))
                {
                    // Save the project file before opening it in the standalone GUI
                    if(base.IsProjectFileDirty)
                        ErrorHandler.ThrowOnFailure(base.Save(base.FileName, 1, 0));

                    System.Diagnostics.Process.Start(gui, "\"" + this.BuildProject.FullPath + "\"");
                }
                else
                    Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO,
                        "Unable to locate the standalone GUI to open the project");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL,
                    "Unable to open project in standalone GUI.  Reason: {0}", ex.Message);
            }
        }

        /// <summary>
        /// This is used to start a web server instance used to view the ASP.NET website output
        /// </summary>
        /// <returns>The URL to the website's index page if successful or null if not</returns>
        public string StartWebServerInstance()
        {
            ProcessStartInfo psi;
            SandcastleBuilder.Utils.FilePath webServerPath = new SandcastleBuilder.Utils.FilePath(null);
            string path, outputPath;
            int serverPort = 12345, uniqueId;

            if(this.SandcastleProject == null)
                return null;

            var options = ((SandcastleBuilderPackage)this.Package).GeneralOptions;

            if(options != null)
                serverPort = options.AspNetDevelopmentServerPort;

            // Use the project filename's hash code as a unique ID for the website
            uniqueId = this.SandcastleProject.Filename.GetHashCode();

            // Make sure we start out in the project's output folder
            // in case the output folder is relative to it.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(this.SandcastleProject.Filename)));
            outputPath = this.SandcastleProject.OutputPath;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            outputPath += "Index.aspx";

            if(!File.Exists(outputPath))
            {
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "A copy of the website does not appear to exist yet.  " +
                    "It may need to be built.");
                return null;
            }

            try
            {
                // See if the web server needs to be started
                if(webServerInstance == null || webServerInstance.HasExited)
                {
                    if(webServerInstance != null)
                    {
                        webServerInstance.Dispose();
                        webServerInstance = null;
                    }

                    outputPath = Path.GetDirectoryName(outputPath);

                    // Visual Studio conveniently provides a development web server that doesn't require IIS
                    webServerPath.Path = String.Format(CultureInfo.InvariantCulture, "%SystemRoot%" +
                        @"\Microsoft.NET\Framework\v{0}\WebDev.WebServer.exe",
                        FrameworkVersionTypeConverter.LatestFrameworkMatching(".NET 2"));

                    // Visual Studio 2008 and later put it in a different location
                    if(!File.Exists(webServerPath))
                    {
                        path = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Common Files\" +
                            @"Microsoft Shared\DevServer");

                        if(Directory.Exists(path))
                            webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer.exe",
                                SearchOption.AllDirectories).FirstOrDefault();

                        if(!File.Exists(webServerPath))
                        {
                            path = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\" +
                                @"Common Files\Microsoft Shared\DevServer");

                            if(Directory.Exists(path))
                                webServerPath.Path = Directory.EnumerateFiles(path, "WebDev.WebServer.exe",
                                    SearchOption.AllDirectories).FirstOrDefault();
                        }
                    }

                    if(!File.Exists(webServerPath))
                    {
                        Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "Unable to locate ASP.NET " +
                            "Development Web Server.  View the HTML website instead.");
                        return null;
                    }

                    webServerInstance = new Process();
                    psi = webServerInstance.StartInfo;

                    psi.FileName = webServerPath;
                    psi.Arguments = String.Format(CultureInfo.InvariantCulture,
                        "/port:{0} /path:\"{1}\" /vpath:\"/SHFBOutput_{2}\"", serverPort, outputPath, uniqueId);
                    psi.WorkingDirectory = outputPath;
                    psi.UseShellExecute = false;

                    webServerInstance.Start();
                    webServerInstance.WaitForInputIdle(30000);
                }

                // This form's handle is used to keep the URL unique in case multiple copies of SHFB are
                // running so that each can view website output.
                outputPath = String.Format(CultureInfo.InvariantCulture,
                    "http://localhost:{0}/SHFBOutput_{1}/Index.aspx", serverPort, uniqueId);

                return outputPath;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL,
                    "Unable to open ASP.NET website '{0}'\r\nReason: {1}", outputPath, ex.Message);
            }

            return null;
        }

        /// <summary>
        /// This is used to open the build log tool window and load the log file for this project
        /// </summary>
        internal void OpenBuildLogToolWindow()
        {
            var window = this.Package.FindToolWindow(typeof(ToolWindows.BuildLogToolWindow), 0, true);

            if(window == null || window.Frame == null)
                throw new NotSupportedException("Unable to create build log tool window");

            // Make sure we start out in the project's output folder in case the output folder is relative to it
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(this.SandcastleProject.Filename)));

            ((ToolWindows.BuildLogToolWindow)window).LoadLogFile(this.SandcastleProject.LogFileLocation);

            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        /// <summary>
        /// Process data object from Drag/Drop/Cut/Copy/Paste operation
        /// </summary>
        /// <remarks>The targetNode is set if the method is called from a drop operation, otherwise it
        /// is null</remarks>
        private DropDataType HandleSelectionDataObject(IOleDataObject dataObject, HierarchyNode targetNode)
        {
            DropDataType dropDataType = DropDataType.None;
            bool isWindowsFormat = false;

            if(targetNode == null)
                targetNode = this;

            // Try to get it as a directory based project
            List<string> filesDropped = DragDropHelper.GetDroppedFiles(DragDropHelper.CF_VSSTGPROJECTITEMS,
                dataObject, out dropDataType);

            if(filesDropped.Count == 0)
                filesDropped = DragDropHelper.GetDroppedFiles(DragDropHelper.CF_VSREFPROJECTITEMS, dataObject,
                    out dropDataType);

            if(filesDropped.Count == 0)
            {
                filesDropped = DragDropHelper.GetDroppedFiles(NativeMethods.CF_HDROP, dataObject, out dropDataType);
                isWindowsFormat = (filesDropped.Count > 0);
            }

            // Handle documentation sources and references first.  These will be removed from the list before
            // passing on what's left to add as standard project files.
            if(isWindowsFormat && filesDropped.Count != 0)
            {
                List<string> docSources = filesDropped.Where(f =>
                        {
                            string ext = Path.GetExtension(f);

                            return (ext.Equals(".sln", StringComparison.OrdinalIgnoreCase) ||
                              ext.EndsWith("proj", StringComparison.OrdinalIgnoreCase));
                        }).ToList(),
                    refSources = filesDropped.Where(f =>
                        {
                            string ext = Path.GetExtension(f);

                            return (ext.Equals(".dll", StringComparison.OrdinalIgnoreCase) ||
                              ext.Equals(".exe", StringComparison.OrdinalIgnoreCase));
                        }).ToList(),
                    xmlDocSources = filesDropped.Where(f =>
                        {
                            string file = Path.GetFileNameWithoutExtension(f), ext = Path.GetExtension(f);

                            // We only want XML files with a base name that matches an assembly in the list
                            return (ext.Equals(".xml", StringComparison.OrdinalIgnoreCase) &&
                              refSources.Any(r => Path.GetFileNameWithoutExtension(r).Equals(file,
                                  StringComparison.OrdinalIgnoreCase)));
                        }).ToList(),
                    allDocSources = docSources.Concat(refSources).Concat(xmlDocSources).ToList();

                var docSourcesNode = targetNode as DocumentationSourcesContainerNode;

                // If dropped on the Documentation Sources node, add all documentation sources
                if(docSourcesNode != null)
                {
                    foreach(string f in allDocSources)
                        docSourcesNode.AddDocumentationSource(f);
                }
                else
                {
                    var refsNode = targetNode as SandcastleBuilderReferenceContainerNode;

                    // If dropped on the references node, add all reference files
                    if(refsNode != null)
                        foreach(string f in refSources)
                        {
                            var node = refsNode.AddReferenceFromSelectorData(new VSCOMPONENTSELECTORDATA
                                {
                                    type = VSCOMPONENTTYPE.VSCOMPONENTTYPE_File,
                                    bstrFile = f
                                }, null);

                            // Clear the Name and AseemblyName metadata and set the HintPath metadata so that it
                            // treats it correctly when the project is reloaded
                            if(node != null)
                            {
                                string hintPath = f;

                                if(Path.IsPathRooted(hintPath))
                                    hintPath = PackageUtilities.GetPathDistance(this.ProjectMgr.BaseURI.Uri,
                                        new Uri(hintPath));

                                node.ItemNode.SetMetadata(ProjectFileConstants.Name, null);
                                node.ItemNode.SetMetadata(ProjectFileConstants.AssemblyName, null);
                                node.ItemNode.SetMetadata(ProjectFileConstants.HintPath, hintPath);
                            }
                        }
                }

                // Remove the documentation source and reference files from the list
                filesDropped = filesDropped.Except(allDocSources).ToList();
            }

            // Handle all other file types
            if(dropDataType != DropDataType.None && filesDropped.Count > 0)
            {
                string[] filesDroppedAsArray = filesDropped.ToArray();

                // For directory based projects the content of the clipboard is a double-NULL terminated list of
                // Projref strings.
                if(isWindowsFormat)
                {
                    // This is the code path when source is Windows Explorer
                    VSADDRESULT[] vsaddresults = new VSADDRESULT[1];
                    vsaddresults[0] = VSADDRESULT.ADDRESULT_Failure;

                    int addResult = this.AddItem(targetNode.ID, VSADDITEMOPERATION.VSADDITEMOP_OPENFILE, null,
                        (uint)filesDropped.Count, filesDroppedAsArray, IntPtr.Zero, vsaddresults);

                    if(addResult != VSConstants.S_OK && addResult != VSConstants.S_FALSE &&
                      addResult != (int)OleConstants.OLECMDERR_E_CANCELED &&
                      vsaddresults[0] != VSADDRESULT.ADDRESULT_Success)
                        ErrorHandler.ThrowOnFailure(addResult);

                    return dropDataType;
                }
                else
                    if(AddFilesFromProjectReferences(targetNode, filesDroppedAsArray))
                        return dropDataType;
            }

            // If we reached this point then the drop data must be set to None.  Otherwise the OnPaste will be
            // called with a valid DropData and that would actually delete the item.
            return DropDataType.None;
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if(sandcastleProject != null)
            {
                sandcastleProject.Dispose();
                sandcastleProject = null;
            }

            try
            {
                if(webServerInstance != null && !webServerInstance.HasExited)
                {
                    webServerInstance.Kill();
                    webServerInstance.Dispose();
                }
            }
            catch(Exception ex)
            {
                // Ignore errors trying to kill the web server
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
            finally
            {
                webServerInstance = null;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override int UpgradeProject(uint grfUpgradeFlags)
        {
            Version schemaVersion;
            string propertyValue = base.GetProjectProperty("SHFBSchemaVersion");

            if(String.IsNullOrEmpty(propertyValue) || !Version.TryParse(propertyValue, out schemaVersion) ||
              schemaVersion > SandcastleProject.SchemaVersion)
            {
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL, this.FileName + " was created with a newer " +
                    "version of the Sandcastle Help File Builder and it cannot be loaded.  Please upgrade your " +
                    "copy of the help file builder in order to load it.");

                return VSConstants.OLE_E_PROMPTSAVECANCELLED;
            }
           
            return base.UpgradeProject(grfUpgradeFlags);
        }

        /// <summary>
        /// This is overridden to see if the file type is one recognized by
        /// the Sandcastle Help File builder.
        /// </summary>
        /// <param name="type">The item type to check</param>
        /// <returns>True if it is one of the recognized help file builder
        /// file type build actions, false if not.</returns>
        protected override bool IsItemTypeFileType(string type)
        {
            SandcastleBuildAction action;

            if(!Enum.TryParse<SandcastleBuildAction>(type, out action))
                return false;

            return action < SandcastleBuildAction.Folder;
        }

        /// <summary>
        /// This creates an object derived from <see cref="NodeProperties"/>
        /// that will be used to expose properties specific for this object to
        /// the property browser.
        /// </summary>
        /// <returns>A new <see cref="SandcastleBuilderProjectNodeProperties"/>
        /// object.</returns>
        protected override NodeProperties CreatePropertiesObject()
        {
            return new SandcastleBuilderProjectNodeProperties(this);
        }

        /// <summary>
        /// This is overridden to create a project folder if it does not exist
        /// </summary>
        /// <param name="path">The path to the folder</param>
        /// <param name="element">The project element</param>
        /// <returns>A reference to the folder node</returns>
        protected internal override FolderNode CreateFolderNode(string path, ProjectElement element)
        {
            string fullPath = Path.Combine(this.ProjectFolder, path);

            if(!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            return base.CreateFolderNode(path, element);
        }

        /// <summary>
        /// This is overridden to return our custom reference container node
        /// </summary>
        /// <returns>A <see cref="SandcastleBuilderReferenceContainerNode" />
        /// object.</returns>
        protected override ReferenceContainerNode CreateReferenceContainerNode()
        {
            return new SandcastleBuilderReferenceContainerNode(this);
        }

        /// <summary>
        /// This returns the automation object for the project
        /// </summary>
        /// <returns>The automation object for the project</returns>
        public override object GetAutomationObject()
        {
            return new OASandcastleBuilderProject(this);
        }

        /// <summary>
        /// Get a list of GUIDs identifying the configuration independent property pages.
        /// </summary>
        /// <returns>An array of GUIDs that identify the configuration independent project
        /// property pages and the order in which they should be displayed.</returns>
        protected override Guid[] GetConfigurationIndependentPropertyPages()
        {
            return new Guid[]
            {
                typeof(BuildPropertiesPageControl).GUID,
                typeof(HelpFilePropertiesPageControl).GUID,
                typeof(Help1WebsitePropertiesPageControl).GUID,
                typeof(MSHelp2PropertiesPageControl).GUID,
                typeof(MSHelpViewerPropertiesPageControl).GUID,
                typeof(SummaryPropertiesPageControl).GUID,
                typeof(VisibilityPropertiesPageControl).GUID,
                typeof(MissingTagPropertiesPageControl).GUID,
                typeof(PathPropertiesPageControl).GUID,
                typeof(ComponentPropertiesPageControl).GUID,
                typeof(PlugInPropertiesPageControl).GUID,
                typeof(UserDefinedPropertiesPageControl).GUID
            };
        }

        /// <summary>
        /// This creates the format list for the Open File dialog
        /// </summary>
        /// <param name="ppszFormatList">The format list to return</param>
        /// <returns><c>VSConstants.S_OK</c> if it succeeded</returns>
        public override int GetFormatList(out string ppszFormatList)
        {
            ppszFormatList = String.Format(CultureInfo.CurrentCulture,
                Resources.ShfbProjectFileAssemblyFilter, "\0", "\0");

            return VSConstants.S_OK;
        }

        /// <summary>
        /// Adds a file to the MSBuild project with metadata where appropriate
        /// </summary>
        /// <param name="file">The file to be added</param>
        /// <returns>A ProjectElement describing the newly added file</returns>
        /// <remarks>Appropriate metadata is added based on the file's extension</remarks>
        protected internal override ProjectElement AddFileToMsBuild(string file)
        {
            SandcastleBuildAction buildAction = SandcastleProject.DefaultBuildAction(file);
            string itemPath = PackageUtilities.MakeRelative(base.FileName, file);

            ProjectElement newItem = this.CreateMsBuildFileItem(itemPath, buildAction.ToString());

            // Set the default ID and alternate text if it is an Image element
            if(buildAction == SandcastleBuildAction.Image)
                newItem.SetImageMetadata();

            return newItem;
        }

        /// <summary>
        /// This creates a file node based on an MSBuild item
        /// </summary>
        /// <param name="item">The MSBuild item to be analyzed</param>
        /// <returns>SandcastleBuilderFileNode or FileNode</returns>
        public override FileNode CreateFileNode(ProjectElement item)
        {
            return new SandcastleBuilderFileNode(this, item);
        }

        /// <summary>
        /// This is overridden as a convenient place to create and load the
        /// project properties and documentation sources node.
        /// </summary>
        protected internal override void LoadNonBuildInformation()
        {
            ProjectPropertiesContainerNode projProps = this.FindChild(
                ProjectPropertiesContainerNode.PropertiesNodeVirtualName)
                as ProjectPropertiesContainerNode;

            if(projProps == null)
            {
                projProps = new ProjectPropertiesContainerNode(this);
                this.AddChild(projProps);
            }

            DocumentationSourcesContainerNode docSources = this.FindChild(
                DocumentationSourcesContainerNode.DocSourcesNodeVirtualName)
                as DocumentationSourcesContainerNode;

			if(docSources == null)
			{
                docSources = new DocumentationSourcesContainerNode(this);
				this.AddChild(docSources);
            }

            if(docSources != null)
			    docSources.LoadDocSourcesFromBuildProject();

            base.LoadNonBuildInformation();
        }

        /// <inheritdoc />
        protected override QueryStatusResult QueryStatusCommandFromOleCommandTarget(Guid cmdGroup, uint cmd, out bool handled)
        {
            QueryStatusResult result = QueryStatusResult.NOTSUPPORTED;

            if(QueryStatusOnCommonCommands(cmdGroup, cmd, ref result))
            {
                handled = true;
                return result;
            }

            return base.QueryStatusCommandFromOleCommandTarget(cmdGroup, cmd, out handled);
        }

        /// <inheritdoc />
        protected override int QueryStatusOnNode(Guid cmdGroup, uint cmd, IntPtr pCmdText,
          ref QueryStatusResult result)
        {
            if(cmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet &&
              (cmd == PkgCmdIDList.OpenInStandaloneGUI || cmd == PkgCmdIDList.AboutSHFB ||
              cmd == PkgCmdIDList.ViewBuildLog))
            {
                result |= QueryStatusResult.SUPPORTED;

                if(!base.BuildInProgress || cmd == PkgCmdIDList.AboutSHFB)
                    result |= QueryStatusResult.ENABLED;

                return VSConstants.S_OK;
            }

            if(QueryStatusOnCommonCommands(cmdGroup, cmd, ref result))
                return VSConstants.S_OK;

            return base.QueryStatusOnNode(cmdGroup, cmd, pCmdText, ref result);
        }

        /// <inheritdoc />
        protected override int ExecCommandOnNode(Guid cmdGroup, uint cmd, uint nCmdexecopt, IntPtr pvaIn,
          IntPtr pvaOut)
        {
            if(cmdGroup == GuidList.guidSandcastleBuilderPackageCmdSet)
                switch(cmd)
                {
                    case PkgCmdIDList.AboutSHFB:
                        using(AboutDlg dlg = new AboutDlg())
                        {
                            dlg.ShowDialog();
                        }
                        return VSConstants.S_OK;

                    case PkgCmdIDList.OpenInStandaloneGUI:
                        this.OpenInStandaloneGui();
                        return VSConstants.S_OK;

                    case PkgCmdIDList.ViewBuildLog:
                        this.OpenBuildLogToolWindow();
                        return VSConstants.S_OK;

                    default:
                        break;
                }

            return base.ExecCommandOnNode(cmdGroup, cmd, nCmdexecopt, pvaIn, pvaOut);
        }

        /// <summary>
        /// This is overridden to set the Verbose Logging build option
        /// </summary>
        /// <param name="config">The configuration to use</param>
        protected internal override void SetConfiguration(string config)
        {
            base.SetConfiguration(config);

            var package = (SandcastleBuilderPackage)this.Package;
            var options = package.GeneralOptions;

            if(options != null)
                this.BuildProject.SetGlobalProperty("Verbose",
                    options.VerboseLogging.ToString().ToLowerInvariant());
        }

        /// <inheritdoc />
        public override MSBuildResult Build(uint vsopts, string config, IVsOutputWindowPane output, string target)
        {
            // TODO: Opening the Tools menu during a build fails because it executes a build for the AllProjectOutputs group.
            // Don't know why yet.  Is this the best workaround?
            if(base.BuildInProgress)
                return MSBuildResult.Successful;

            return base.Build(vsopts, config, output, target);
        }

        /// <summary>
        /// This is overridden to handle file references correctly when added to the project
        /// </summary>
        /// <inheritdoc />
        public override int AddComponent(VSADDCOMPOPERATION dwAddCompOperation, uint cComponents,
           IntPtr[] rgpcsdComponents, IntPtr hwndDialog, VSADDCOMPRESULT[] pResult)
        {
            if(rgpcsdComponents == null || pResult == null)
                return VSConstants.E_FAIL;

            // Initalize the out parameter
            pResult[0] = VSADDCOMPRESULT.ADDCOMPRESULT_Success;

            IReferenceContainer references = GetReferenceContainer();

            if(null == references)
            {
                // This project does not support references or the reference container was not created.
                // In both cases this operation is not supported.
                return VSConstants.E_NOTIMPL;
            }

            for(int cCount = 0; cCount < cComponents; cCount++)
            {
                VSCOMPONENTSELECTORDATA selectorData = new VSCOMPONENTSELECTORDATA();
                IntPtr ptr = rgpcsdComponents[cCount];
                selectorData = (VSCOMPONENTSELECTORDATA)Marshal.PtrToStructure(ptr, typeof(VSCOMPONENTSELECTORDATA));

                var node = references.AddReferenceFromSelectorData(selectorData);

                if(node == null)
                {
                    //Skip further proccessing since a reference has to be added
                    pResult[0] = VSADDCOMPRESULT.ADDCOMPRESULT_Failure;
                    return VSConstants.S_OK;
                }

                // If it's a file, get rid of the Name and AssemblyName metadata and add the HintPath metadata.
                // If not, when the project is opened the next time, the reference will appear as missing
                // if it isn't in the GAC.
                if(node != null && selectorData.type == VSCOMPONENTTYPE.VSCOMPONENTTYPE_File)
                {
                    string hintPath = selectorData.bstrFile;

                    if(Path.IsPathRooted(hintPath))
                        hintPath = PackageUtilities.GetPathDistance(this.ProjectMgr.BaseURI.Uri, new Uri(hintPath));

                    node.ItemNode.SetMetadata(ProjectFileConstants.Name, null);
                    node.ItemNode.SetMetadata(ProjectFileConstants.AssemblyName, null);
                    node.ItemNode.SetMetadata(ProjectFileConstants.HintPath, hintPath);
                }

            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// This is overridden to handle drop operations correctly in a help file builder project
        /// </summary>
        /// <inheritdoc />
        public override int Drop(IOleDataObject pDataObject, uint grfKeyState, uint itemid, ref uint pdwEffect)
        {
            DropDataType dropDataType = DropDataType.None;

            if(pDataObject == null)
                return VSConstants.E_INVALIDARG;

            pdwEffect = (uint)DropEffect.None;

            // If the source is within the project, let the base class handle it
            if(this.SourceDraggedOrCutOrCopied)
                return base.Drop(pDataObject, grfKeyState, itemid, ref pdwEffect);

            // Get the node that is being dragged over and ask it which node should handle this call
            HierarchyNode targetNode = NodeFromItemId(itemid);

            if(targetNode == null)
                return VSConstants.S_FALSE;

            targetNode = targetNode.GetDragTargetHandlerNode();

            dropDataType = this.HandleSelectionDataObject(pDataObject, targetNode);

            // Since we can get a mix of files that may not necessarily be moved into the project (i.e.
            // documentation sources and references), we'll always act as if they were copied.
            pdwEffect = (uint)DropEffect.Copy;

            return (dropDataType != DropDataType.Shell) ? VSConstants.E_FAIL : VSConstants.S_OK;
        }
        #endregion
    }
}
