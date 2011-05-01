//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : PlaceHolderEditorFactory.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/30/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class used as a temporary place holder editor factory
// for the file types that will eventually have their own custom editor.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.1  04/30/2011  EFW  Created the code
//=============================================================================

using System;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace SandcastleBuilder.Package.Editors
{
    /// <summary>
    /// This serves as a place holder editor factory for those file type that
    /// will eventually have their own custom file editors.
    /// </summary>
    /// <remarks>More likely than not, the actual editor factories will use
    /// something similar to the editor factories in the Learn VSX Now package
    /// as they seem to implement a lot of the common boiler plate code.</remarks>
    [Guid(GuidList.guidPlaceHolderEditorFactoryString)]
    public class PlaceHolderEditorFactory : IVsEditorFactory
    {
        #region IVsEditorFactory Members

        /// <inheritdoc />
        public int SetSite(Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            // Not used for this one
            return VSConstants.S_OK;
        }

        /// <inheritdoc />
        public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView,
          IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView,
          out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
        {
            ppunkDocView = ppunkDocData = IntPtr.Zero;
            pguidCmdUI = GetType().GUID;
            pgrfCDW = 0;
            pbstrEditorCaption = null;

            Utility.ShowMessageBox(OLEMSGICON.OLEMSGICON_INFO, "The editor factory for this file type has " +
                "not been created yet.  You can use the standalone GUI to edit it instead.  The default XML " +
                "editor will be opened for it in Visual Studio for the time being.");

            return VSConstants.VS_E_UNSUPPORTEDFORMAT;
        }

        /// <inheritdoc />
        public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            // Not used for this one
            return VSConstants.E_NOTIMPL;
        }

        /// <inheritdoc />
        public int Close()
        {
            // Not used for this one
            return VSConstants.S_OK;
        }
        #endregion
    }
}
