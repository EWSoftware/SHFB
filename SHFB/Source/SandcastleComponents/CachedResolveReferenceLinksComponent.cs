//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : CachedResolveReferenceLinksComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/29/2012
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
// ==============================================================================================================
// 1.6.0.3  11/11/2007  EFW  Created the code
// 1.8.0.3  07/04/2009  EFW  Add parameter to Dispose() to match base class
// 1.9.3.3  11/19/2011  EFW  Opened cache files for shared read to allow for concurrent builds
// 1.9.7.0  12/22/2012  EFW  Moved MSDN URL caching support into the base Sandcastle component
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This build component is obsolete and will be removed in a future release.
    /// </summary>
    public class CachedResolveReferenceLinksComponent : BuildComponent
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>This component is obsolete and will be removed in a future release.</remarks>
        public CachedResolveReferenceLinksComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    Cached Resolve Reference Links Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));

            base.WriteMessage(MessageLevel.Error, "\r\nThe CachedResolveReferenceLinksComponent is obsolete " +
                "and must be removed from your project's Component Configurations.\r\nMSDN URL caching support " +
                "is built into base Sandcastle component now.\r\nSee the Sandcastle Help File Builder " +
                "v1.9.7.0 release notes for more information.");
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
    }
}
