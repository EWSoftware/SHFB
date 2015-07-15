//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : SandcastleCompletion.cs
// Author  : Sam Harwell  (sam@tunnelvisionlabs.com)
// Updated : 07/15/2015
// Note    : Copyright 2015, Sam Harwell, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom completion class which is distinguishable from the Completion class provided by the
// Visual Studio SDK.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/15/2015  SCH  Initial implementation.
//===============================================================================================================

using System.Windows.Media;

using Microsoft.VisualStudio.Language.Intellisense;

namespace SandcastleBuilder.Package.IntelliSense
{
    /// <summary>
    /// Represents a completion item, including the icon, insertion text, and display text, in a
    /// <see cref="CompletionSet"/>.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="Completion"/> class, allowing the SHFB editor extensions to distinguish
    /// between completion items provided by SHFB, and those provided by other packages.
    /// </remarks>
    internal class SandcastleCompletion : Completion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleCompletion"/> class.
        /// </summary>
        public SandcastleCompletion()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleCompletion"/> class with the specified display text.
        /// </summary>
        /// <param name="displayText">The text that is to be displayed by an IntelliSense presenter.</param>
        public SandcastleCompletion(string displayText) : base(displayText)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SandcastleCompletion"/> class with the specified text and
        /// description.
        /// </summary>
        /// <param name="displayText">The text that is to be displayed by an IntelliSense presenter.</param>
        /// <param name="insertionText">The text that is to be inserted into the buffer if this completion is committed.</param>
        /// <param name="description">A description that can be displayed with the display text of the completion.</param>
        /// <param name="iconSource">The icon.</param>
        /// <param name="iconAutomationText">The text to be used as the automation name for the icon.</param>
        public SandcastleCompletion(string displayText, string insertionText, string description, ImageSource iconSource, string iconAutomationText)
            : base(displayText, insertionText, description, iconSource, iconAutomationText)
        {
        }
    }
}
