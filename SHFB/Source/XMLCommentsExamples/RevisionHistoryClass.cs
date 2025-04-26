//===============================================================================================================
// System  : Sandcastle Tools - XML Comments Example
// File    : RevisionHistoryClass.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/24/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
//
// This class is used to demonstrate the revision history XML comments element.  It serves no useful purpose.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/24/2014  EFW  Created the code
//===============================================================================================================

namespace XMLCommentsExamples
{
    #region Revision history examples
    /// <summary>
    /// This class demonstrates the revision history
    /// </summary>
    /// <remarks>The revision history element can be used on the class and its
    /// members.</remarks>
    /// <revisionHistory>
    ///     <revision date="04/11/2014" version="9.0.1.0" author="THLUCAS">This
    /// revision will be displayed since the "visible" attribute is not present
    /// (i.e. default).</revision>
    ///     <revision date="04/12/2014" version="9.0.1.1" author="THLUCAS"
    /// visible="true">This revision will be displayed due to it's
    /// "visible=true" attribute setting.</revision>
    ///     <revision date="04/15/2014" version="9.0.1.2" author="THLUCAS"
    /// visible="false">This revision will NOT be displayed due to it's
    /// "visible=false" attribute setting.
    ///     </revision>
    ///     <revision date="04/20/2014" version="9.0.1.3" author="THLUCAS">
    ///         <para>This revision will be displayed since the "visible"
    /// attribute is not present (i.e. default).</para>
    /// 
    ///         <para>Other XML comments elements are supported here and can be
    /// used as in a remarks section to add additional details.</para>
    /// 
    ///         <list type="bullet">
    ///             <item>Point #1</item>
    ///             <item>Point #2</item>
    ///             <item>Point #3</item>
    ///         </list>
    ///     </revision>
    /// </revisionHistory>
    /// <conceptualLink target="2a973959-9c9a-4b3b-abcb-48bb30382400"/>
    public class RevisionHistoryClass
    {
        /// <summary>
        /// This property has revision history
        /// </summary>
        /// <revisionHistory>
        ///     <revision date="05/25/2014" version="9.0.1.4"
        /// author="EWOODRUFF">Added an example property</revision>
        /// </revisionHistory>
        public string Revision { get; set; }

        /// <summary>
        /// This method has revision history
        /// </summary>
        /// <revisionHistory>
        ///     <revision date="05/25/2014" version="9.0.1.4"
        /// author="EWOODRUFF">Added an example method</revision>
        /// </revisionHistory>
        public void ExampleMethod()
        {
        }
    }
    #endregion
}
