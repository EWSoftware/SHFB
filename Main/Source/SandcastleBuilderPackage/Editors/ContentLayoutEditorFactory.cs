//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : ContentLayoutFileEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used generate content layout file editor instances
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/26/2011  EFW  Created the code
//=============================================================================

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
        /// <remarks>The content layout editor factory overrides this to ensure that a SHFB project is
        /// the active project before allowing the editor to open.  Content layout files are tightly coupled
        /// to the project since they refer to and can add files to a project.</remarks>
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

                Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "Content layout files must be associated " +
                    "with and opened from a Sandcastle Help File Builder project.  Please create a project or " +
                    "add this file to a project and open it from there.");

                return VSConstants.VS_E_INCOMPATIBLEDOCDATA;
            }

            return base.CreateEditorInstance(grfCreateDoc, pszMkDocument, pszPhysicalView, pvHier, itemid,
                punkDocDataExisting, out ppunkDocView, out ppunkDocData, out pbstrEditorCaption, out pguidCmdUI,
                out pgrfCDW);
        }
        #endregion
    }
}
