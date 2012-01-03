using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel.Design;

namespace SandcastleBuilder.Package.ToolWindows
{
	/// <summary>
    /// This class implements the tool window BuildLogToolWindowBase exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("1ac33b36-3ffe-46b8-a340-090d35ab9ecf")]
    public class BuildLogToolWindowBase : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public BuildLogToolWindowBase()
            : base(null)
        {
			this.Caption = "Build Log Content";
this.ToolBar = new CommandID (GuidList.guidSandcastleBuilderPackageCmdSet, (int)PkgCmdIDList.BuildLogToolbar);
        }
    }
	/// <summary>
    /// This class implements the tool window EntityReferencesToolWindowBase exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("581e89c0-e423-4453-bde3-a0403d5f380d")]
    public class EntityReferencesToolWindowBase : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public EntityReferencesToolWindowBase()
            : base(null)
        {
			this.Caption = "Entity References";
        }
    }
}