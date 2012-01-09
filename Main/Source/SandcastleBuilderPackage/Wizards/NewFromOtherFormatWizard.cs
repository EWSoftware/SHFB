//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : NewFromOtherFormatWizard.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/08/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This class implements a wizard used to create new Sandcastle Help File
// Builder projects from project files in various other formats.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/07/2012  EFW  Created the code
//=============================================================================

using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;

using SandcastleBuilder.Package.Nodes;
using SandcastleBuilder.Utils.Conversion;

namespace SandcastleBuilder.Package.Wizards
{
    /// <summary>
    /// This class implements the New Project From Other Format wizard used to
    /// create new Sandcastle Help File Builder projects from project files in
    /// various other formats.
    /// </summary>
    public class NewFromOtherFormatWizard : IWizard
    {
        #region IWizard Members

        /// <inheritdoc />
        /// <remarks>Not used by this wizard</remarks>
        public void BeforeOpeningFile(EnvDTE.ProjectItem projectItem)
        {
        }

        /// <inheritdoc />
        public void ProjectFinishedGenerating(EnvDTE.Project project)
        {
            var projectNode = project.Object as SandcastleBuilderProjectNode;

            if(projectNode == null || projectNode.SandcastleProject == null)
            {
                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_CRITICAL,
                    "Unable to object new project reference.  The import cannot be performed.");
                return;
            }

            using(NewFromOtherFormatDlg dlg = new NewFromOtherFormatDlg(projectNode.SandcastleProject))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "The project was converted " +
                        "successfully.  When prompted, reload the project to see the conversion's changes.");
            }
        }

        /// <inheritdoc />
        /// <remarks>Not used by this wizard</remarks>
        public void ProjectItemFinishedGenerating(EnvDTE.ProjectItem projectItem)
        {
        }

        /// <inheritdoc />
        /// <remarks>Not used by this wizard</remarks>
        public void RunFinished()
        {
        }

        /// <inheritdoc />
        /// <remarks>Not used by this wizard</remarks>
        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary,
          WizardRunKind runKind, object[] customParams)
        {
        }

        /// <inheritdoc />
        /// <remarks>Not used by this wizard</remarks>
        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
        #endregion
    }
}
