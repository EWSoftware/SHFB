//===============================================================================================================
// System  : Sandcastle Build Components
// File    : Transform.cs
//
// This file contains a class used to contain information for an XSLT transformation
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice and all copyright notices must remain intact in all applications, documentation, and source files.
//
// Change History
// 10/14/2012 - EFW - Moved the class into its own file and made it public
//===============================================================================================================

using System.Xml;
using System.Xml.Xsl;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class is used to contain information for an XSLT transformation
    /// </summary>
    public class Transform
    {
        #region Private data members
        //=====================================================================

        private XslCompiledTransform xslt = new XslCompiledTransform();
        private XsltArgumentList arguments = new XsltArgumentList();
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
        /// This read-only property returns the XSL transformation arguments list
        /// </summary>
        /// <remarks>Arguments can be added to this to pass them to them to the transformation when it is
        /// executed.
        /// 
        /// <note type="important">An argument called <c>key</c> is automatically added to the list when each
        /// topic is transformed.  It will contain the current topic's key.</note>
        /// </remarks>
        public XsltArgumentList Arguments
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
        /// <remarks>The transforms presumably come from a trusted source scripting and the document function
        /// are enabled in them.</remarks>
        public Transform(string file)
        {
            xslt.Load(file, new XsltSettings(true, true), new XmlUrlResolver());
        }
        #endregion
    }
}
