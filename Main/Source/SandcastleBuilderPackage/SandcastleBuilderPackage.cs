//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderPackage.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/30/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that defines the Sancastle Help File Builder
// Visual Studio package.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/18/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExecProcess = System.Diagnostics.Process;

using EnvDTE;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;

using SandcastleBuilder.MicrosoftHelpViewer;
using SandcastleBuilder.Package.Editors;
using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Package.PropertyPages;
using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This is the class that implements the Sandcastle Help File Builder
    /// package.
    /// </summary>
    /// <remarks>
    /// The class implements the <c>IVsPackage</c> interface and registers
    /// itself with the shell.  This package uses the helper classes defined
    /// inside the <b>Managed Package Framework</b> (MPF) to do it.  It derives
    /// from the <c>Package</c> class that provides the implementation of the 
    /// <c>IVsPackage</c> interface and uses the registration attributes
    /// defined in the framework to register itself and its components with the
    /// shell.</remarks>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#111", "SHFB", IconResourceID = 400)]
    // Define the package GUID
    [Guid(GuidList.guidSandcastleBuilderPackagePkgString)]
    // Register the project factory.  The template is in a VSTemplate so a
    // non-existent path is used for the template directory parameter and we
    // set LanguageVsTemplate to our project type value.  The display name is
    // set to "Documentation" to provide a more generic category for the Add
    // New Project and Add New Item dialog boxes.
    [ProvideProjectFactory(typeof(SandcastleBuilderProjectFactory), "Documentation", "#112", "shfbproj",
      "shfbproj", @".\NullPath", LanguageVsTemplate = "SHFBProject")]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This registers the general options page
    [ProvideOptionPage(typeof(SandcastleBuilderOptionsPage), "Sandcastle Help File Builder", "General",
      110, 200, true)]
    // Connect the options page to a settings category so that its properties can be imported/exported
    [ProvideProfile(typeof(SandcastleBuilderOptionsPage), "Sandcastle Help File Builder", "General",
      110, 110, true, DescriptionResourceID = 201)]
    // Provide project option pages
    [ProvideObject(typeof(BuildPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(ComponentPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(Help1WebsitePropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(HelpFilePropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(MissingTagPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(MSHelp2PropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(MSHelpViewerPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(PathPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(PlugInPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(SummaryPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(UserDefinedPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideObject(typeof(VisibilityPropertiesPageControl), RegisterUsing = RegistrationMethod.CodeBase)]
    // Provide custom file editors.  As above, a non-existent template path is used.
    [ProvideEditorExtension(typeof(PlaceHolderEditorFactory), ".sitemap", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 119,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(PlaceHolderEditorFactory), ".content", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 129,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(PlaceHolderEditorFactory), ".items", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 133,
      TemplateDir = @".\NullPath")]
    [ProvideEditorExtension(typeof(PlaceHolderEditorFactory), ".tokens", 50,
      ProjectGuid = GuidList.guidSandcastleBuilderProjectFactoryString, NameResourceID = 135,
      TemplateDir = @".\NullPath")]
    public sealed class SandcastleBuilderPackage : PackageBase
    {
        #region Private data members
        //=====================================================================

        private BuildCompletedEventListener buildCompletedListener;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the package instance
        /// </summary>
        internal static SandcastleBuilderPackage Instance { get; private set; }

        // TODO: Is my assumption correct here?
        /// <summary>
        /// This returns the project type for the Framework Retargeting dialog
        /// </summary>
        /// <remarks>My assumption is that this should return the same value as
        /// the SandcastleBuilderProjectNode.ProjectType property.</remarks>
        public override string ProductUserContext
        {
            get { return "SHFBProject"; }
        }

        /// <summary>
        /// Return the general options page for the package
        /// </summary>
        public SandcastleBuilderOptionsPage GeneralOptions
        {
            get
            {
                return (SandcastleBuilderOptionsPage)this.GetDialogPage(typeof(SandcastleBuilderOptionsPage));
            }
        }

        /// <summary>
        /// This returns a <see cref="SandcastleBuilderProjectNode"/> instance for the
        /// current project if it is one or null if not.
        /// </summary>
        public SandcastleBuilderProjectNode CurrentProjectNode
        {
            get
            {
                DTE dte = Utility.GetServiceFromPackage<DTE, DTE>(false);
                Array activeProjects = null;

                if(dte != null)
                    activeProjects = dte.ActiveSolutionProjects as Array;

                if(activeProjects != null && activeProjects.Length > 0)
                {
                    Project p = activeProjects.GetValue(0) as Project;

                    if(p != null)
                        return (p.Object as SandcastleBuilderProjectNode);
                }

                return null;
            }
        }

        /// <summary>
        /// This returns a <see cref="SandcastleProject"/> instance for the
        /// current project if it is one or null if not
        /// </summary>
        /// <value>The caller should not dispose of the project</value>
        public SandcastleProject CurrentSandcastleProject
        {
            get
            {
                SandcastleBuilderProjectNode pn = this.CurrentProjectNode;

                if(pn != null)
                    return pn.SandcastleProject;

                return null;
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Default constructor of the package.
        /// </summary>
        /// <remarks>Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.</remarks>
        public SandcastleBuilderPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}",
                this.ToString()));

            // Ensure that the file and folder path user controls are known by the base property page class
            if(!BasePropertyPage.CustomControls.ContainsKey(typeof(SandcastleBuilder.Utils.Controls.FilePathUserControl).Name))
            {
                BasePropertyPage.CustomControls.Add(typeof(SandcastleBuilder.Utils.Controls.FilePathUserControl).FullName,
                    "PersistablePath");
                BasePropertyPage.CustomControls.Add(typeof(SandcastleBuilder.Utils.Controls.FolderPathUserControl).FullName,
                    "PersistablePath");
            }
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            SandcastleBuilderPackage.Instance = null;

            if(buildCompletedListener != null)
            {
                buildCompletedListener.Dispose();
                buildCompletedListener = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// This is overridden to initialize the package
        /// </summary>
        /// <remarks>This method is called right after the package is sited, so
        /// this is the place where you can put all the initialization code
        /// that relies on services provided by Visual Studio.</remarks>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}",
                this.ToString()));
            base.Initialize();

            SandcastleBuilderPackage.Instance = this;

            // Register the project factory
            this.RegisterProjectFactory(new SandcastleBuilderProjectFactory(this));

            // Register the place holder editor factory.  This will eventually be replaced by editor factories
            // for each of the custom file types (.content, .tokens, .items, and .sitemap).
            base.RegisterEditorFactory(new PlaceHolderEditorFactory());

            // Create the update solution event listener for build completed events
            buildCompletedListener = new BuildCompletedEventListener(this);
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to set the state of a menu command on the View Help menu
        /// </summary>
        /// <param name="command">The command object</param>
        /// <param name="format">The help file format</param>
        private void SetViewHelpCommandState(OleMenuCommand command, HelpFileFormat? format)
        {
            DTE dte = Utility.GetServiceFromPackage<DTE, DTE>(false);
            bool visible = false, enabled = false;

            if(dte != null)
            {
                Solution s = dte.Solution;

                // Hide the menu option if a SHFB project is not loaded
                if(s != null)
                {
                    visible = s.Projects.Cast<Project>().Any(
                        p => p.UniqueName.EndsWith(".shfbproj", StringComparison.OrdinalIgnoreCase));

                    // Check the active project for the specified help format if visible
                    if(visible)
                    {
                        Array activeProjects = dte.ActiveSolutionProjects as Array;

                        if(activeProjects != null && activeProjects.Length > 0)
                        {
                            Project p = activeProjects.GetValue(0) as Project;

                            if(p != null && p.Object != null && p.UniqueName.EndsWith(".shfbproj",
                              StringComparison.OrdinalIgnoreCase))
                            {
                                SandcastleBuilderProjectNode pn = (SandcastleBuilderProjectNode)p.Object;
                                string projectHelpFormat = (pn.GetProjectProperty("HelpFileFormat") ??
                                    HelpFileFormat.HtmlHelp1.ToString());

                                enabled = (!pn.BuildInProgress && (format == null || projectHelpFormat.IndexOf(
                                    format.ToString(), StringComparison.OrdinalIgnoreCase) != -1));
                            }
                        }
                    }
                }
            }

            command.Visible = visible;
            command.Enabled = enabled;
        }

        /// <summary>
        /// View the last build HTML Help 1 file, MS Help 2 file, MS Help Viewer, or website Index.aspx page
        /// </summary>
        /// <param name="projectNode">The project node for which to open the help file</param>
        internal void ViewBuiltHelpFile(SandcastleBuilderProjectNode projectNode)
        {
            if(projectNode != null)
            {
                SandcastleProject project = projectNode.SandcastleProject;

                if(project == null)
                    return;

                if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0)
                    this.ViewBuiltHelpFile(project, PkgCmdIDList.ViewHtmlHelp);
                else
                    if((project.HelpFileFormat & HelpFileFormat.MSHelp2) != 0)
                        this.ViewBuiltHelpFile(project, PkgCmdIDList.ViewHxSHelp);
                    else
                        if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                            Utility.OpenUrl(projectNode.StartWebServerInstance());
                        else
                        {
                            // This format opens a modal dialog box so we'll use it last if nothing
                            // else is selected.
                            var options = this.GeneralOptions;

                            if(options != null)
                                using(LaunchMSHelpViewerDlg dlg = new LaunchMSHelpViewerDlg(project, options.MSHelpViewerPath))
                                {
                                    dlg.ShowDialog();
                                }
                        }
            }
        }

        /// <summary>
        /// View the last build HTML Help 1 file, MS Help 2 file, or website Index.html page
        /// </summary>
        /// <param name="project">The project to use or null to use the current project</param>
        /// <param name="commandId">The ID of the command that invoked the request which determines the help
        /// file format launched.</param>
        private void ViewBuiltHelpFile(SandcastleProject project, uint commandId)
        {
            string outputPath, help2Viewer = null;

            if(project == null)
            {
                project = this.CurrentSandcastleProject;

                if(project == null)
                    return;
            }

            var options = this.GeneralOptions;

            if(options != null)
                help2Viewer = options.HxsViewerPath;

            // Make sure we start out in the project's output folder in case the output folder
            // is relative to it.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Path.GetFullPath(project.Filename)));
            outputPath = project.OutputPath;

            if(String.IsNullOrEmpty(outputPath))
                outputPath = Directory.GetCurrentDirectory();
            else
                outputPath = Path.GetFullPath(outputPath);

            if(commandId == PkgCmdIDList.ViewHtmlHelp)
                outputPath += project.HtmlHelpName + ".chm";
            else
                if(commandId == PkgCmdIDList.ViewHxSHelp)
                {
                    outputPath += project.HtmlHelpName + ".hxs";

                    if(String.IsNullOrEmpty(help2Viewer) || !File.Exists(help2Viewer))
                    {
                        Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_WARNING, "MS Help 2 files must be registered in a " +
                            "collection to be viewed or you can use a standalone viewer.  Use Tools | Options | Sandcastle " +
                            "Help File Builder to define a standalone viewer.  See Links to Resources in the help file if " +
                            "you need one.");
                        return;
                    }
                }
                else
                    outputPath += "Index.html";

            if(!File.Exists(outputPath))
            {
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "A copy of the help file does not appear to exist yet.  " +
                    "It may need to be built.");
                return;
            }

            try
            {
                if(outputPath.EndsWith(".hxs", StringComparison.OrdinalIgnoreCase))
                    System.Diagnostics.Process.Start(help2Viewer, outputPath);
                else
                    if(outputPath.EndsWith(".chm", StringComparison.OrdinalIgnoreCase))
                        System.Diagnostics.Process.Start(outputPath);
                    else
                        Utility.OpenUrl(outputPath);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL, "Unable to open help file '{0}'\r\nReason: {1}",
                    outputPath, ex.Message);
            }
        }
        #endregion

        #region View help file event handlers
        //=====================================================================

        /// <summary>
        /// Set the state of the View Help File command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHelpFileQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// View the help file produced by the last build.  Pick the first available format
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHelpFileExecuteHandler(object sender, EventArgs e)
        {
            this.ViewBuiltHelpFile(this.CurrentProjectNode);
        }

        /// <summary>
        /// Set the state of the View HTML Help file command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHtmlHelpQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormat.HtmlHelp1);
        }

        /// <summary>
        /// View the last built HTML Help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHtmlHelpExecuteHandler(object sender, EventArgs e)
        {
            this.ViewBuiltHelpFile(null, PkgCmdIDList.ViewHtmlHelp);
        }

        /// <summary>
        /// Set the state of the View MS Help 2 file command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHxSHelpQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormat.MSHelp2);
        }

        /// <summary>
        /// View the last built MS Help 2 file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHxSHelpExecuteHandler(object sender, EventArgs e)
        {
            this.ViewBuiltHelpFile(null, PkgCmdIDList.ViewHxSHelp);
        }

        /// <summary>
        /// Set the state of the View MS Help View file command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewMshcHelpQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormat.MSHelpViewer);
        }

        /// <summary>
        /// Launch the MS Help Viewer to view the last built Microsoft Help Viewer file.  This
        /// will optionally install it first.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewMshcHelpExecuteHandler(object sender, EventArgs e)
        {
            SandcastleProject project = this.CurrentSandcastleProject;

            var options = this.GeneralOptions;

            if(project != null && options != null)
                using(LaunchMSHelpViewerDlg dlg = new LaunchMSHelpViewerDlg(project, options.MSHelpViewerPath))
                {
                    dlg.ShowDialog();
                }
        }

        /// <summary>
        /// Set the state of the Launch Help Library Manager command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void LaunchHelpLibMgrQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// Launch the Help Library Manager for interactive use based on the
        /// current project's settings.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void LaunchHelpLibMgrExecuteHandler(object sender, EventArgs e)
        {
            try
            {
                SandcastleProject project = this.CurrentSandcastleProject;

                if(project != null)
                {
                    HelpLibraryManager hlm = new HelpLibraryManager();

                    hlm.LaunchInteractive(String.Format(CultureInfo.InvariantCulture,
                        "/product \"{0}\" /version \"{1}\" /locale {2} /brandingPackage Dev10.mshc",
                        project.CatalogProductId, project.CatalogVersion, project.Language.Name));
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL,
                    "Unable to launch Help Library Manager.  Reason:\r\n{0}", ex.Message);
            }
        }

        /// <summary>
        /// Set the state of the View ASP.NET website help command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewAspNetWebsiteQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormat.Website);
        }

        /// <summary>
        /// View the last built ASP.NET help website
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewAspNetWebsiteExecuteHandler(object sender, EventArgs e)
        {
            SandcastleBuilderProjectNode pn = this.CurrentProjectNode;

            if(pn != null)
                Utility.OpenUrl(pn.StartWebServerInstance());
        }

        /// <summary>
        /// Set the state of the View HTML website help command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHtmlWebsiteQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, HelpFileFormat.Website);
        }

        /// <summary>
        /// View the last built HTML help website
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewHtmlWebsiteExecuteHandler(object sender, EventArgs e)
        {
            this.ViewBuiltHelpFile(null, PkgCmdIDList.ViewHtmlWebsite);
        }

        /// <summary>
        /// Set the state of the view build log content command
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewBuildLogQueryStatusHandler(object sender, EventArgs e)
        {
            this.SetViewHelpCommandState((OleMenuCommand)sender, null);
        }

        /// <summary>
        /// View the build log content for the current project
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewBuildLogExecuteHandler(object sender, EventArgs e)
        {
            if(this.CurrentProjectNode != null)
                this.CurrentProjectNode.OpenBuildLogToolWindow();
        }
        #endregion

        #region SHFB Help event handlers
        //=====================================================================

        /// <summary>
        /// View the FAQ topic in the help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewFaqExecuteHandler(object sender, EventArgs e)
        {
            Utility.ShowHelpTopic("1aea789d-b226-4b39-b534-4c97c256fac8");
        }

        /// <summary>
        /// View the SHFB help file
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected override void ViewShfbHelpExecuteHandler(object sender, EventArgs e)
        {
            Utility.ShowHelpTopic("bd1ddb51-1c4f-434f-bb1a-ce2135d3a909");
        }
        #endregion
    }
}
