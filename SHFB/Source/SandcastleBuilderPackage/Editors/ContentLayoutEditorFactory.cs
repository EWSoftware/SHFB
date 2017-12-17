//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ContentLayoutFileEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/17/2017
// Note    : Copyright 2011-2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used generate content layout file editor instances
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 12/26/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is the factory class for content layout file editors
    /// </summary>
    [Guid(GuidList.guidContentLayoutEditorFactoryString)]
    public sealed class ContentLayoutEditorFactory : SimpleEditorFactory<ContentLayoutEditorPane>
    {
        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>The content layout editor factory overrides this to ensure that a SHFB project is the active
        /// project before allowing the editor to open.  Content layout files are tightly coupled to the project
        /// since they refer to and can add files to a project.</remarks>
        public override int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView,
            IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView,
            out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            if(SandcastleBuilderPackage.CurrentSandcastleProject == null)
            {
                ppunkDocView = ppunkDocData = IntPtr.Zero;
                pguidCmdUI = GetType().GUID;
                pgrfCDW = 0;
                pbstrEditorCaption = null;

                // This typically doesn't happen except for when a content layout file is left open when Visual
                // studio is closed and a project other than the SHFB project has the focus at that time.  In
                // such cases, either the content layout file will not be reopened when the solution is or, if it
                // didn't have the focus when closed, the tab will close when clicked.
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }

            return base.CreateEditorInstance(grfCreateDoc, pszMkDocument, pszPhysicalView, pvHier, itemid,
                punkDocDataExisting, out ppunkDocView, out ppunkDocData, out pbstrEditorCaption, out pguidCmdUI,
                out pgrfCDW);
        }
        #endregion
    }
}
