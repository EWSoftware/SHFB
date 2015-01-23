//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CtrlKeyState.cs
// Author  : Noah Richards
// Updated : 12/08/2014
// Note    : Copyright 2009-2014, Noah Richards, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class that tracks the state of the Ctrl key
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows.Input;

using Microsoft.VisualStudio.Text.Editor;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to track the state of the control key for a given view, which is kept up to date by a
    /// combination of the key processor and the mouse processor.
    /// </summary>
    internal sealed class CtrlKeyState
    {
        #region Private data members
        //=====================================================================

        private bool enabled = false;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the enabled state of the Ctrl key
        /// </summary>
        /// <remarks>Note that setting it does not change the actual state of the Ctrl key</remarks>
        public bool Enabled
        {
            get
            {
                // Check and see if Ctrl is down but we missed it somehow
                bool ctrlDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
                    (Keyboard.Modifiers & ModifierKeys.Shift) == 0;

                if(ctrlDown != enabled)
                    this.Enabled = ctrlDown;

                return enabled;
            }
            set
            {
                bool oldValue = enabled;
                enabled = value;

                if(oldValue != enabled)
                {
                    var handler = CtrlKeyStateChanged;

                    if(handler != null)
                        handler(this, new EventArgs());
                }
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when the Ctrl key state changes
        /// </summary>
        internal event EventHandler<EventArgs> CtrlKeyStateChanged;

        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This is used to get the Ctrl key state tracker for the given text view
        /// </summary>
        /// <param name="view">The view for which to get the Ctrl key state tracker</param>
        /// <returns>The Ctrl key state tracker for the view.  If one does not exist, it will be created.</returns>
        public static CtrlKeyState GetStateForView(ITextView view)
        {
            return view.Properties.GetOrCreateSingletonProperty(typeof(CtrlKeyState), () => new CtrlKeyState());
        }
        #endregion
    }
}
