//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : PostTransformComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/17/2012
// Note    : Copyright 2006-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This component is now obsolete and will be removed in a future release
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
//===============================================================================================================
// 1.3.3.0  11/23/2006  EFW  Created the code
// 1.4.0.0  01/31/2007  EFW  Added placement options for logo.  Made changes to support custom presentation
//                           styles.  Reworked version info code to improve performance when used with very
//                           large documentation builds.
// 1.5.0.0  06/19/2007  EFW  Various additions and updates for the June CTP
// 1.6.0.1  10/30/2007  EFW  Fixed the logo placement for the VS2005 style
// 1.6.0.3  06/20/2007  EFW  Fixed bug that caused code blocks with an unknown or unspecified language to always
//                           be hidden.
// 1.6.0.7  03/24/2008  EFW  Updated to handle multiple assembly versions. Updated to support use in conceptual
//                           builds.
// 1.7.0.0  06/01/2008  EFW  Removed language filter support for Hana and Prototype due to changes in the way the
//                           transformations implement it.
// 1.9.0.0  06/06/2010  EFW  Replaced outputPath element with an outputPaths element that supports multiple help
//                           file format output locations.
// 1.9.3.4  02/21/2012  EFW  Merged changes from Don Fehr for VS2010 style
// 1.9.6.0  10/14/2012  EFW  Moved version info and Prototype language filter stuff into the Sandcastle XSL
//                           transformations and removed it from here.  Move the code block relaetd code into
//                           the CodeBlockComponent's new event handler.  Moved the logo support into each of
//                           the presentation styles.  With these changes, this component is now obsolete.
//===============================================================================================================

using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is obsolete and will be removed in a future release.
    /// </summary>
    public class PostTransformComponent : BuildComponent
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>This component is obsolete and will be removed in a future release.</remarks>
        public PostTransformComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, "\r\n    [{0}, version {1}]\r\n    Post-Transform Component. " +
                "{2}\r\n    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            base.WriteMessage(MessageLevel.Error, "\r\nThe PostTransformComponent is obsolete and must be " +
                "removed from your project's Component Configurations.\r\nUse the Transform Arguments category " +
                "of properties to define the logo placement options.\r\nSee the Sandcastle Help File Builder " +
                "v1.9.6.0 release notes for more information.");
        }
        #endregion

        #region Apply the component
        //=====================================================================

        /// <summary>
        /// This component is obsolete and will do nothing.
        /// </summary>
        /// <param name="document">The XML document with which to work.</param>
        /// <param name="key">The key (member name) of the item being documented.</param>
        public override void Apply(XmlDocument document, string key)
        {
        }
        #endregion

        #region Static configuration method for use with SHFB
        //=====================================================================

        /// <summary>
        /// This static method is used by the Sandcastle Help File Builder to let the component perform its own
        /// configuration.
        /// </summary>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        public static string ConfigureComponent(string currentConfig)
        {
            using(PostTransformConfigDlg dlg = new PostTransformConfigDlg(currentConfig))
            {
                // This component is obsolete and cannot be configured.  It will display the old values in
                // read-only mode so that they can be copied to the new project settings.
                dlg.ShowDialog();
            }

            return currentConfig;
        }
        #endregion
    }
}
