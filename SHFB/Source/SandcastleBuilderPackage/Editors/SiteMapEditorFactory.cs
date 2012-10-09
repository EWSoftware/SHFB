//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SiteMapFileEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 09/08/2012
// Note    : Copyright 2011-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used generate site map file editor instances
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.3.3  12/27/2011  EFW  Created the code
//===============================================================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This is the factory class for site map file editors
    /// </summary>
    [Guid(GuidList.guidSiteMapEditorFactoryString)]
    public sealed class SiteMapEditorFactory : SimpleEditorFactory<SiteMapEditorPane>
    {
        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        /// <remarks>The site map editor factory overrides this to ensure that a SHFB project is the active
        /// project before allowing the editor to open.  Site map files are tightly coupled to the project since
        /// they refer to and can add files to a project.</remarks>
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

                // Site map files can be associated with web projects so we will assume it's for such a project
                // and will not prompt the user to open it from within the context of a SHFB project.
                return VSConstants.VS_E_UNSUPPORTEDFORMAT;
            }

            return base.CreateEditorInstance(grfCreateDoc, pszMkDocument, pszPhysicalView, pvHier, itemid,
                punkDocDataExisting, out ppunkDocView, out ppunkDocData, out pbstrEditorCaption, out pguidCmdUI,
                out pgrfCDW);
        }
        #endregion
    }
}
