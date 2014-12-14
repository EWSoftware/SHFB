//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : UnderlineFormatDefinition.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/08/2014
// Note    : Copyright 2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to define the color for MAML and XML comments link underlines
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

using System.ComponentModel.Composition;
using System.Windows.Media;

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to define the color for MAML and XML comments link underlines
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = UnderlineClassifier.UnderlineClassifierType)]
    [Name("SHFB Underline Classification Format")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class UnderlineFormatDefinition : ClassificationFormatDefinition
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UnderlineFormatDefinition()
        {
            this.DisplayName = "MAML/XML Comment Link Underline";
            this.TextDecorations = System.Windows.TextDecorations.Underline;
            this.ForegroundColor = Colors.Blue;
        }
    }

}
