//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : GoToDefinitionKeyProcessor.cs
// Author  : Noah Richards
// Updated : 05/26/2021
// Note    : Copyright 2009-2021, Noah Richards, All rights reserved
//
// This file contains the class used to listen for the control key being pressed or released to update the Ctrl
// key state tracker for a view.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/01/2014  EFW  Created the code
//===============================================================================================================

using System.Windows.Input;

using Microsoft.VisualStudio.Text.Editor;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to listen for the control key being pressed or released to update the Ctrl key state
    /// tracker for a view.
    /// </summary>
    /// <remarks>This is used in conjunction with the Go To Definition mouse tracker for XML comments and MAML
    /// link elements.</remarks>
    internal sealed class GoToDefinitionKeyProcessor : KeyProcessor
    {
        #region Private data members
        //=====================================================================

        private readonly CtrlKeyState state;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">The control key state tracker to use</param>
        public GoToDefinitionKeyProcessor(CtrlKeyState state)
        {
            this.state = state;
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <inheritdoc />
        public override void PreviewKeyDown(KeyEventArgs args)
        {
            this.UpdateState(args);
        }

        /// <inheritdoc />
        public override void PreviewKeyUp(KeyEventArgs args)
        {
            this.UpdateState(args);
        }

        /// <summary>
        /// Update the Ctrl key state based on the event arguments
        /// </summary>
        /// <param name="args">The key event arguments</param>
        private void UpdateState(KeyEventArgs args)
        {
            state.Enabled = (args.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0 &&
                (args.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0;
        }
        #endregion
    }
}
