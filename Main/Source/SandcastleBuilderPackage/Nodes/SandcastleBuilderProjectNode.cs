//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderProjectNode.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/22/2011
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
//=============================================================================

using System;
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
        /// <returns>Returns "SandcastleBuilderProject"</returns>
        public override string ProjectType
        {
            get { return "SandcastleBuilderProject"; }
        }

        /// <summary>
        /// This returns the index in the image list of the project node icon
        /// </summary>
        public override int ImageIndex
        {
            get { return imageIndex; }
        }

        /// <summary>
        /// This is overridden to prevent dropping on documentation sources
        /// and project properties nodes
        /// </summary>
        /// <param name="itemId">The drop target node ID</param>
        /// <returns>True if the drop can occur, false if not</returns>
        protected internal override bool CanTargetNodeAcceptDrop(uint itemId)
        {
            bool result = base.CanTargetNodeAcceptDrop(itemId);

            if(result)
            {
                HierarchyNode targetNode = NodeFromItemId(itemId);

                if(targetNode is DocumentationSourcesContainerNode || targetNode is DocumentationSourceNode ||
                  targetNode is ProjectPropertiesContainerNode)
                    result = false;
            }

            return result;
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
        private bool QueryStatusOnCommonCommands(Guid cmdGroup, uint cmd, ref QueryStatusResult result)
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
			    docSources.LoadDocSourcesFromBuildProject(this.BuildProject);

            base.LoadNonBuildInformation();
        }

        /// <inheritdoc />
        protected override QueryStatusResult QueryStatusCommandFromOleCommandTarget(Guid cmdGroup, uint cmd, out bool handled)
        {
            QueryStatusResult result = QueryStatusResult.NOTSUPPORTED;

            if(this.QueryStatusOnCommonCommands(cmdGroup, cmd, ref result))
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

            if(this.QueryStatusOnCommonCommands(cmdGroup, cmd, ref result))
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
        #endregion
    }
}
