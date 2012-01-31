//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ElementProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/16/2012
// Note    : Copyright 2012, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to to pass parser state information to the
// various element handling methods.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.4  01/02/2012  EFW  Created the code
//=============================================================================

using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    /// <summary>
    /// This is used to pass parser state information to the various element handling methods
    /// </summary>
    internal class ElementProperties
    {
        #region Private data members
        //=====================================================================

        private MamlToFlowDocumentConverter converter;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a reference to the converter
        /// </summary>
        public MamlToFlowDocumentConverter Converter
        {
            get { return converter; }
        }

        /// <summary>
        /// This is used to get or set the element on which to work
        /// </summary>
        public XElement Element { get; set; }

        /// <summary>
        /// This is used to get or set whether or not the converter should parse child elements
        /// </summary>
        /// <value>This will be true by default if the element has children</value>
        public bool ParseChildren { get; set; }

        /// <summary>
        /// This is used to get or set whether or not to return to the parent element after the
        /// current element has been parsed.
        /// </summary>
        /// <value>This will always be true by default.  Set it to false to stay in the current
        /// parent after any child elements of the current element are parsed.</value>
        public bool ReturnToParent { get; set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="converter">A reference to the converter that will use the properties</param>
        public ElementProperties(MamlToFlowDocumentConverter converter)
        {
            this.converter = converter;
        }
        #endregion
    }
}
