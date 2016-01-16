//===============================================================================================================
// System  : Sandcastle Build Components
// File    : Transform.cs
//
// This file contains a class used to contain information for an XSLT transformation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 10/14/2012 - EFW - Moved the class into its own file and made it public
// 01/07/2015 - EFW - Changed the argument list handling to make it thread safe
//===============================================================================================================

using System.Collections.Generic;
using System.Xml;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This class is used to contain information for an XSLT transformation
    /// </summary>
    public class Transform
    {
        #region Private data members
        //=====================================================================

        private XslCompiledTransform xslt = new XslCompiledTransform();
        private Dictionary<string, object> arguments = new Dictionary<string, object>();

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the compiled XSL transformation
        /// </summary>
        public XslCompiledTransform Xslt
        {
            get { return xslt; }
        }

        /// <summary>
        /// This read-only property returns a dictionary containing the XSL transformation arguments list values
        /// </summary>
        /// <remarks>Arguments can be added to this to pass them to them to the transformation when it is
        /// executed.
        /// 
        /// <note type="important">An argument called <c>key</c> is automatically added to the list when each
        /// topic is transformed.  It will contain the current topic's key.</note>
        /// </remarks>
        public IDictionary<string, object> Arguments
        {
            get { return arguments; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="file">The path to the XSLT transformation file</param>
        /// <remarks>The transforms presumably come from a trusted source so scripting and the document function
        /// are enabled in them.</remarks>
        public Transform(string file)
        {
            xslt.Load(file, new XsltSettings(true, true), new XmlUrlResolver());
        }
        #endregion
    }
}
