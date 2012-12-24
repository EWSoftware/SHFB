//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : IntelliSenseComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/22/2012
// Note    : Copyright 2007-2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This component is now obsolete and will be removed in a future release.  Use the one in the Sandcastle
// BuildComponents assembly instead.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.6.0.1  10/09/2007  EFW  Created the code
// 1.6.0.7  03/24/2008  EFW  Updated it to handle multiple assembly references
// 1.8.0.3  07/04/2009  EFW  Add parameter to Dispose() to match base class
// 1.9.7.0  12/22/2012  EFW  Moved the component to the Sandcastle BuildAssembler assembly
//===============================================================================================================

using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This component is now obsolete and will be removed in a future release.  Use the one in the Sandcastle
    /// BuildComponents assembly instead.
    /// </summary>
    public class IntelliSenseComponent : BuildComponent
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>This component is now obsolete and will be removed in a future release.  Use the one in the
        /// Sandcastle BuildComponents assembly instead.</remarks>
        public IntelliSenseComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, "\r\n    [{0}, version {1}]\r\n    IntelliSense Component. " +
                "{2}\r\n    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            base.WriteMessage(MessageLevel.Error, @"\r\n
This version of the IntelliSenseComponent is obsolete. Remove this version from your\r\n
project's Component Configurations, add the new one now located in the Sandcastle\r\n
BuildComponents assembly, and configure it again if necessary.  See the Sandcastle\r\n
Help File Builder v1.9.7.0 release notes for more information.");
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
