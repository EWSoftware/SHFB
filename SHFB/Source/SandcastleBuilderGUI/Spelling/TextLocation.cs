//===============================================================================================================
// System  : Sandcastle Help File Builder
// File    : TextLocation.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/11/2013
// Note    : Copyright 2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to track text location information during the spell checking process
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

using System.Drawing;

namespace SandcastleBuilder.Gui.Spelling
{
    /// <summary>
    /// This class is used to track text location information during the spell checking process
    /// </summary>
    /// <remarks>Location can be represented by an absolute line number and column or by a starting offset
    /// and length</remarks>
    internal class TextLocation
    {
        #region Properties
        //=====================================================================

        /// <summary>The line number</summary>
        public int Line { get; set; }

        /// <summary>The column number</summary>
        public int Column { get; set; }

        /// <summary>The span start location</summary>
        public int Start { get; set; }

        /// <summary>The span length</summary>
        public int Length { get; set; }
        #endregion

        #region Public methods
        //=====================================================================

        /// <summary>
        /// This returns the position of the misspelled or doubled word within the text accounting for carriage
        /// returns and line feeds between the line and column and the given index.
        /// </summary>
        /// <param name="text">The text being spell checked</param>
        /// <param name="index">The index of the doubled or misspelled word</param>
        /// <returns>The point containing the actual position of the word</returns>
        public Point ToPoint(string text, int index)
        {
            int column = this.Column, line = this.Line, pos = 0;

            while(pos < index)
            {
                if(text[pos] == '\r')
                {
                    column = 0;
                    line++;

                    if(text[pos + 1] == '\n')
                        pos++;
                }
                else
                    if(text[pos] == '\n')
                    {
                        column = 0;
                        line++;
                    }

                pos++;
                column++;
            }

            return new Point(column, line); 
        }
        #endregion
    }
}
