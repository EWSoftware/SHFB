//===============================================================================================================
// System  : Sandcastle Help File Builder Components
// File    : ESentCopyFromIndexComponent.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/31/2013
// Compiler: Microsoft Visual C#
//
// This is a version of the CopyFromIndexComponent that stores the reflection index data in a persistent ESent
// database.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.7.0  01/20/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml.XPath;

using Microsoft.Ddue.Tools;
using Microsoft.Ddue.Tools.Commands;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a version of the <c>CopyFromIndexComponent</c> that stores the reflection index data in a
    /// persistent ESent database.
    /// </summary>
    public class ESentCopyFromIndexComponent : CopyFromIndexComponent
    {
        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">A reference to the build assembler.</param>
        /// <param name="configuration">The configuration information</param>
        /// <remarks>This component is obsolete and will be removed in a future release.</remarks>
        public ESentCopyFromIndexComponent(BuildAssembler assembler, XPathNavigator configuration) :
          base(assembler, configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            base.WriteMessage(MessageLevel.Info, String.Format(CultureInfo.InvariantCulture,
                "\r\n    [{0}, version {1}]\r\n    ESent Copy From Index Component.  {2}\r\n" +
                "    http://SHFB.CodePlex.com", fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright));
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override IndexedCache CreateIndex(XPathNavigator configuration)
        {
            return new ESentIndexedCache(this, base.Context, configuration);
        }
        #endregion
    }
}
