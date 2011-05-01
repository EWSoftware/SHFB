//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleBuilderProjectFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that defines the Sancastle Help File Builder
// project factory.
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils;

namespace SandcastleBuilder.Package
{
    /// <summary>
    /// This is the Sandcastle Help File Builder project factory
    /// </summary>
    [Guid(GuidList.guidSandcastleBuilderProjectFactoryString)]
    public sealed class SandcastleBuilderProjectFactory : ProjectFactory, IVsProjectUpgradeViaFactory
    {
        #region Private data members and constants
        //=====================================================================

        private const int PUVFF_SXSBACKUP = 0x00000020;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="package">The package reference</param>
        public SandcastleBuilderProjectFactory(ProjectPackage package) : base(package)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <summary>
        /// This is overridden to create the project node
        /// </summary>
        /// <returns>A project node</returns>
        protected override ProjectNode CreateProject()
        {
            ProjectPackage package = (ProjectPackage)this.Package;

            SandcastleBuilderProjectNode project = new SandcastleBuilderProjectNode(package);

            project.SetSite((IOleServiceProvider)((IServiceProvider)package).GetService(
                typeof(IOleServiceProvider)));

            return project;
        }
        #endregion

        #region IVsProjectUpgradeViaFactory Members
        //=====================================================================

        /// <inheritdoc />
        public int GetSccInfo(string bstrProjectFileName, out string pbstrSccProjectName, out string pbstrSccAuxPath,
          out string pbstrSccLocalPath, out string pbstrProvider)
        {
            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";

            pbstrSccProjectName = pbstrSccAuxPath = pbstrSccLocalPath = pbstrProvider = String.Empty;

            if(!String.IsNullOrEmpty(bstrProjectFileName) && File.Exists(bstrProjectFileName))
            {
                XDocument project = XDocument.Load(bstrProjectFileName);

                XElement property = project.Root.Descendants(msbuild + "SccProjectName").FirstOrDefault();

                if(property != null)
                    pbstrSccProjectName = property.Value;

                property = project.Root.Descendants(msbuild + "SccAuxPath").FirstOrDefault();

                if(property != null)
                    pbstrSccAuxPath = property.Value;

                property = project.Root.Descendants(msbuild + "SccLocalPath").FirstOrDefault();

                if(property != null)
                    pbstrSccLocalPath = property.Value;

                property = project.Root.Descendants(msbuild + "SccProvider").FirstOrDefault();

                if(property != null)
                    pbstrProvider = property.Value;
            }

            return VSConstants.S_OK;
        }

        /// <inheritdoc />
        public int UpgradeProject(string bstrFileName, uint fUpgradeFlag, string bstrCopyLocation,
          out string pbstrUpgradedFullyQualifiedFileName, IVsUpgradeLogger pLogger, out int pUpgradeRequired,
          out Guid pguidNewProjectFactory)
        {
            uint verdict, moreInfo, ignored;
            string[] files = new string[1] { bstrFileName };
            string projectName = Path.GetFileNameWithoutExtension(bstrFileName);
            bool continueUpgrade = false;

            pbstrUpgradedFullyQualifiedFileName = bstrFileName;

            // Be sure we need an upgrade
            this.UpgradeProject_CheckOnly(bstrFileName, pLogger, out pUpgradeRequired, out pguidNewProjectFactory, out ignored);

            if(pUpgradeRequired == 0)
                return VSConstants.S_OK;

            // See if the file is editable
            IVsQueryEditQuerySave2 qes = Utility.GetServiceFromPackage<IVsQueryEditQuerySave2, SVsQueryEditQuerySave>(true);

            ErrorHandler.ThrowOnFailure(qes.QueryEditFiles((uint)tagVSQueryEditFlags.QEF_ReportOnly |
                (uint)__VSQueryEditFlags2.QEF_AllowUnopenedProjects, 1, files, null, null, out verdict,
                out moreInfo));

            if(verdict == (uint)tagVSQueryEditResult.QER_EditOK)
                continueUpgrade = true;

            if(verdict == (uint)tagVSQueryEditResult.QER_EditNotOK)
            {
                pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, projectName, bstrFileName,
                    "The project file is read-only.  An attempt will be made to check it out if under source control.");

                if((moreInfo & (uint)tagVSQueryEditResultFlags.QER_ReadOnlyUnderScc) != 0)
                {
                    ErrorHandler.ThrowOnFailure(qes.QueryEditFiles(
                        (uint)tagVSQueryEditFlags.QEF_DisallowInMemoryEdits |
                        (uint)__VSQueryEditFlags2.QEF_AllowUnopenedProjects |
                        (uint)tagVSQueryEditFlags.QEF_ForceEdit_NoPrompting, 1, files, null, null,
                        out verdict, out moreInfo));

                    if(verdict == (uint)tagVSQueryEditResult.QER_EditOK)
                        continueUpgrade = true;
                }

                if(continueUpgrade)
                    pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, projectName, bstrFileName,
                        "The project file was successfully checked out.");
                else
                {
                    pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_ERROR, projectName, bstrFileName,
                        "Unable to check project out of source control.  Upgrade failed.");
                    throw new InvalidOperationException("Unable to check out project file for upgrade: " + bstrFileName);
                }
            }

            // If file was modified during the checkout, confirm that it still needs upgrading
            if((moreInfo & (uint)tagVSQueryEditResultFlags.QER_MaybeChanged) != 0)
            {
                this.UpgradeProject_CheckOnly(bstrFileName, pLogger, out pUpgradeRequired, out pguidNewProjectFactory, out ignored);

                if(pUpgradeRequired == 0)
                {
                    if(pLogger != null)
                        pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, projectName, bstrFileName,
                            "The project file was checked out and is already up to date.  No upgrade needed.");

                    return VSConstants.S_OK;
                }
            }

            if(continueUpgrade)
            {
                // Make a backup?
                if(fUpgradeFlag == PUVFF_SXSBACKUP)
                    File.Copy(bstrFileName, bstrFileName + ".backup", true);

                // The SancastleProject class contains all the code needed to update the project so all we need
                // to do is load a copy and force it to save a new copy.
                using(SandcastleProject p = new SandcastleProject(bstrFileName, true))
                {
                    p.UpgradeProjectProperties();
                    p.SaveProject(bstrFileName);
                }

                pLogger.LogMessage((uint)__VSUL_ERRORLEVEL.VSUL_INFORMATIONAL, projectName, bstrFileName,
                    "The project file was upgraded successfully.");
            }

            return VSConstants.S_OK;
        }

        /// <inheritdoc />
        public int UpgradeProject_CheckOnly(string bstrFileName, IVsUpgradeLogger pLogger, out int pUpgradeRequired,
          out Guid pguidNewProjectFactory, out uint pUpgradeProjectCapabilityFlags)
        {
            XNamespace msbuild = "http://schemas.microsoft.com/developer/msbuild/2003";
            Version toolsVersion, projectVersion;

            pUpgradeRequired = 0;
            pguidNewProjectFactory = this.GetType().GUID;
            pUpgradeProjectCapabilityFlags = PUVFF_SXSBACKUP;

            XDocument project = XDocument.Load(bstrFileName);

            // Check the ToolsVerion attribute first.  It should be 4.0 or better.
            var toolsVersionAttr = project.Root.Attribute("ToolsVersion");

            if(toolsVersionAttr == null || String.IsNullOrEmpty(toolsVersionAttr.Value) ||
              !Version.TryParse(toolsVersionAttr.Value, out toolsVersion) || toolsVersion.Major < 4)
                pUpgradeRequired = 1;
            else
            {
                // Next, see if the SHFB schema version is current
                XElement property = project.Root.Descendants(msbuild + "SHFBSchemaVersion").FirstOrDefault();

                if(property == null || !Version.TryParse(property.Value, out projectVersion) ||
                  projectVersion < SandcastleProject.SchemaVersion)
                    pUpgradeRequired = 1;
            }

            // If it's of a higher version, we'll let the project node catch it.  Despite what the documentation
            // for this interface says, any value returned from here appears to be ignored and you cannot prevent
            // it from either loading or continuing with the conversion when the shell calls it.

            return VSConstants.S_OK;
        }
        #endregion
    }
}
