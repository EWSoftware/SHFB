//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ElementProperties.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 04/17/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to pass parser state information to the various element handling methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 01/02/2012  EFW  Created the code
//===============================================================================================================

using System.Xml.Linq;

namespace SandcastleBuilder.WPF.Maml
{
    /// <summary>
    /// This is used to pass parser state information to the various element handling methods
    /// </summary>
    internal class ElementProperties
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a reference to the converter
        /// </summary>
        public MamlToFlowDocumentConverter Converter { get; }

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
            this.Converter = converter;
        }
        #endregion
    }
}
