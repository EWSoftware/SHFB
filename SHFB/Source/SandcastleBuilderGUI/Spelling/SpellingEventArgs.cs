//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : SpellingEventArgs.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/11/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to contain spell checking event arguments
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.8.0  05/11/2013  EFW  Created the code
//===============================================================================================================

using System.ComponentModel;
using System.Drawing;

namespace SandcastleBuilder.Gui.Spelling
{
    /// <summary>
    /// This derived event arguments class is used for spell checking events
    /// </summary>
    internal class SpellingEventArgs : CancelEventArgs
    {
        #region Properties
        //=====================================================================

        /// <summary>
        /// The word that caused the event
        /// </summary>
        public string Word { get; private set; }

        /// <summary>
        /// The line and column at which the word being spell checked appears
        /// </summary>
        public Point Position { get; private set; }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="word">The word that caused the event</param>
        /// <param name="position">The position at which the word appears in the text</param>
        public SpellingEventArgs(string word, Point position)
        {
            this.Word = word;
            this.Position = position;
        }
        #endregion
    }
}
