//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : MSHelpAttrComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/22/2012
// Note    : Copyright 2008-2012, Eric Woodruff, All rights reserved
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
// 1.6.0.7  04/07/2008  EFW  Created the code
// 1.9.0.0  06/19/2010  EFW  Attributes are now optional.  The component will do nothing if none are specified
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
    public class MSHelpAttrComponent : BuildComponent
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
        public MSHelpAttrComponent(BuildAssembler assembler,
          XPathNavigator configuration) : base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, "\r\n    [{0}, version {1}]\r\n    MS Help 2 Attribute " +
                "Component. {2}\r\n    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion,
                fvi.LegalCopyright);

            base.WriteMessage(MessageLevel.Error, @"\r\n
This version of the MSHelpAttrComponent is obsolete. Remove this version from your\r\n
project's Component Configurations, and add the new one now located in the Sandcastle\r\n
BuildComponents assembly.  See the Sandcastle Help File Builder v1.9.7.0 release notes\r\n
for more information.");
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
