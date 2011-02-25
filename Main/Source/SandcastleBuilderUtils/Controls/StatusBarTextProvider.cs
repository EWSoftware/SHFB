//=============================================================================
// System  : EWSoftware Status Bar Text Provider
// File    : StatusBarTextProvider.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/21/2007
// Note    : Copyright 2005-2007, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// http://www.codeproject.com/cs/miscctrl/statusbartext.asp
//
// This file contains an IExtenderProvider component that allows you to add
// status bar text for menu items and form controls.  Comment or uncomment
// the DONET_20 definition below to disable or enabled support for the
// .NET 2.0 menu strip, tool strip, and status strip components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.0.0.0  04/28/2005  EFW  Created the code
// 2.0.0.0  01/28/2006  EFW  Updated it for use with .NET 2.0 as well
// 2.0.0.1  06/26/2006  EFW  Added static methods for progress bar controls
//=============================================================================

// When this line is commented out, the component only supports the StatusBar
// control.  When uncommented, it will also support the  new .NET 2.0 tool
// strip controls (ToolStrip, MenuStrip, StatusStrip, etc).
#define DOTNET_20

using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

// All classes go in the SandcastleBuilder.Utils.Controls namespace
namespace SandcastleBuilder.Utils.Controls
{
    /// <summary>
    /// This is an <see cref="IExtenderProvider"/> component that allows you
    /// to add status bar text for menu items and form controls.  When built
    /// for use with .NET 2.0, it also supports adding status bar text for
    /// menu strip, tool strip, and status strip components.
    /// </summary>
    [ProvideProperty("StatusBarText", typeof(Component)),
     ProvideProperty("ShowAsBlank", typeof(Component)),
     Description("A component that lets you specify text for controls that " +
       "will appear in the status bar when they have the focus")]
    public class StatusBarTextProvider : Component, IExtenderProvider
    {
        #region Construction and disposal
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <overloads>There are two overloads for the constructor</overloads>
        public StatusBarTextProvider()
        {
            htOptions = new Hashtable(25);
        }

        /// <summary>
        /// Constructor.  This one takes a reference to a container.
        /// </summary>
        /// <param name="container">The container for the component</param>
        public StatusBarTextProvider(IContainer container) : this()
        {
            if(container != null)
                container.Add(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged
        /// resources, false to just release unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(hookedMenuEvents && this.StatusBar != null)
                {
                    Form frm = this.StatusBarParentForm;

                    if(frm != null)
                        frm.MenuComplete -= new EventHandler(Form_MenuComplete);
                }

                htOptions.Clear();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Property options class
        //====================================================================
        // Property options class

        /// <summary>
        /// This class contains the options for the items that need status bar
        /// text.
        /// </summary>
        private sealed class PropertyOptions
        {
            //=================================================================
            // Private data member

            private string message;
            private bool showAsBlank;

            //=================================================================
            // Properties

            /// <summary>
            /// Set or get the message text
            /// </summary>
            public string Message
            {
                get { return message; }
                set { message = value; }
            }

            /// <summary>
            /// The "show as blank" flag
            /// </summary>
            public bool ShowAsBlank
            {
                get { return showAsBlank; }
                set { showAsBlank = value; }
            }

            //=================================================================
            // Methods, etc.

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="msg">The message text to display</param>
            /// <overloads>There are two overloads for the constructor</overloads>
            public PropertyOptions(string msg)
            {
                message = msg;
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="showBlank">The "show as blank" flag</param>
            public PropertyOptions(bool showBlank)
            {
                showAsBlank = showBlank;
            }
        }
        #endregion

        #region Private data members
        //====================================================================
        // Private data members

        // These are used for the shared status bar
        private static object appStatusBar;
        private static int appDisplayPanel;
        private static string appDefaultText;

#if DOTNET_20
        // These provide convenient access to a status label and progress bar
        // from anywhere within an application.
        private static ToolStripStatusLabel statusLabel;
        private static ToolStripProgressBar progressBar;
#endif

        // Instance status bar
        private object instanceStatusBar;
        private int instanceDisplayPanel;
        private string instanceDefaultText;

        // These are for the status text messages
        private Hashtable htOptions;
        private bool hookedMenuEvents, hookedFormEvent;
        #endregion

        #region Properties
        //====================================================================
        // Properties

        /// <summary>
        /// This is used to set or get the default status bar component to use
        /// for displaying the status bar text messages.
        /// </summary>
        /// <value>This property is static and must be set at some point
        /// during application initialization.  This component will be used
        /// unless the <see cref="InstanceStatusBar"/> property is set to
        /// override it.
        /// </value>
        /// <seealso cref="ApplicationDisplayPanel"/>
        /// <seealso cref="InstanceStatusBar"/>
        /// <seealso cref="InstanceDisplayPanel"/>
        /// <example>
        /// <code>
        /// // Define the default status bar to use in the main form's
        /// // constructor
        /// public MainForm()
        /// {
        ///     InitializeComponent();
        ///
        ///     //=========
        ///     // .NET 1.1
        ///     //=========
        ///     // Tell the StatusBarTextProvider component the status bar
        ///     // to use.
        ///     StatusBarTextProvider.ApplicationStatusBar = sbStatusBar;
        ///
        ///     // You can also use any panel you like.  The default is zero
        ///     // (the left-most panel).  For the demo, we'll use the one
        ///     // in the middle.
        ///     StatusBarTextProvider.ApplicationDisplayPanel = 1;
        ///
        ///     //=========
        ///     // .NET 2.0
        ///     //=========
        ///     // Tell the StatusBarTextProvider component the component
        ///     // to use to display the text.  When using a tool strip
        ///     // component, the ApplicationDisplayPanel property is ignored.
        ///     StatusBarTextProvider.ApplicationStatusBar = tslStatusText;
        ///
        ///     // Define the status label and progress bar too.  This allows
        ///     // easy access to those items from anywhere within the
        ///     // application.
        ///     StatusBarTextProvider.StatusLabel = tslProgressNote;
        ///     StatusBarTextProvider.ProgressBar = tspbProgressBar;
        /// }
        /// </code>
        /// <code lang="vbnet">
        /// ' Define the default status bar to use in the main form's
        /// ' constructor
        /// Public Sub New()
        ///     MyBase.New()
        ///
        ///     InitializeComponent()
        ///
        ///     '=========
        ///     ' .NET 1.1
        ///     '=========
        ///     ' Tell the StatusBarTextProvider component the status bar
        ///     ' to use.
        ///     StatusBarTextProvider.ApplicationStatusBar = sbStatusBar;
        ///
        ///     ' You can also use any panel you like.  The default is zero
        ///     ' (the left-most panel).  For the demo, we'll use the one
        ///     ' in the middle.
        ///     StatusBarTextProvider.ApplicationDisplayPanel = 1;
        ///
        ///     '=========
        ///     ' .NET 2.0
        ///     '=========
        ///     ' Tell the StatusBarTextProvider component the component
        ///     ' to use to display the text.  When using a tool strip
        ///     ' component, the ApplicationDisplayPanel property is ignored.
        ///     StatusBarTextProvider.ApplicationStatusBar = tslStatusText;
        ///
        ///     ' Define the status label and progress bar too.  This allows
        ///     ' easy access to those items from anywhere within the
        ///     ' application.
        ///     StatusBarTextProvider.StatusLabel = tslProgressNote;
        ///     StatusBarTextProvider.ProgressBar = tspbProgressBar;
        /// End Sub
        /// </code>
        /// </example>
        /// <exception cref="ArgumentException">This is thrown if the object
        /// is not a status bar control or a tool strip item.</exception>
        [Browsable(false),
          Description("The application status bar or tool strip item used to display the text")]
        public static object ApplicationStatusBar
        {
            get { return appStatusBar; }
            set
            {
#if !DOTNET_20
                if(value != null && !(value is StatusBar))
#else
                if(value != null && !(value is StatusBar) && !(value is ToolStripItem))
#endif
                    throw new ArgumentException("The object must be " +
                        "a StatusBar or a ToolStripItem");

                appStatusBar = value;
                appDefaultText = null;

                StatusBar sb = appStatusBar as StatusBar;

                if(sb != null)
                {
                    if(sb.ShowPanels && appDisplayPanel < sb.Panels.Count)
                        appDefaultText = sb.Panels[appDisplayPanel].Text;
                    else
                        appDefaultText = sb.Text;
                }
#if DOTNET_20
                else
                {
                    ToolStripItem tsi = appStatusBar as ToolStripItem;
                    if(tsi != null)
                        appDefaultText = tsi.Text;
                }
#endif
            }
        }

        /// <summary>
        /// This is used to set or get the status bar panel in which to
        /// display the messages in the common application status bar.
        /// </summary>
        /// <value>This property is static and must be set at some point
        /// during application initialization.  This display panel will be
        /// used unless the <see cref="InstanceStatusBar"/> property is set
        /// to override the status bar used.  In that case, the
        /// <see cref="InstanceDisplayPanel"/> is used instead.
        /// <p/>The default is zero (the first panel).  If the status bar does
        /// not have panels, the index exceeds the panel count, or the status
        /// bar's <see cref="System.Windows.Forms.StatusBar.ShowPanels"/>
        /// property is false, messages will be shown in the status bar's
        /// <b>Text</b> property instead.  This property is ignored if using a
        /// tool strip item to display the text.</value>
        /// <seealso cref="ApplicationStatusBar"/>
        /// <seealso cref="InstanceStatusBar"/>
        /// <seealso cref="InstanceDisplayPanel"/>
        [Browsable(false),
          Description("The panel in which to display messages if the " +
            "application status bar has panels displayed")]
        public static int ApplicationDisplayPanel
        {
            get { return appDisplayPanel; }
            set
            {
                if(value < 0)
                    value = 0;

                appDisplayPanel = value;
                appDefaultText = null;

                StatusBar sb = appStatusBar as StatusBar;

                if(sb != null)
                {
                    if(sb.ShowPanels && appDisplayPanel < sb.Panels.Count)
                        appDefaultText = sb.Panels[appDisplayPanel].Text;
                    else
                        appDefaultText = sb.Text;
                }
#if DOTNET_20
                else
                {
                    ToolStripItem tsi = appStatusBar as ToolStripItem;
                    if(tsi != null)
                        appDefaultText = tsi.Text;
                }
#endif
            }
        }

        /// <summary>
        /// This is used to set or get the default text to show when no item
        /// has the focus or no status text for the application status bar.
        /// </summary>
        /// <value>By default, it will be set to the text in the selected
        /// status bar component.  If you set it to a different value, set
        /// this property after specifying the status bar control to use.
        /// </value>
        [Browsable(false),
          Description("The default text to show when no item has the focus " +
            "for the application status bar")]
        public static string ApplicationDefaultText
        {
            get { return appDefaultText; }
            set
            {
                appDefaultText = value;

                StatusBar sb = appStatusBar as StatusBar;

                if(sb != null)
                {
                    if(sb.ShowPanels && appDisplayPanel < sb.Panels.Count)
                        sb.Panels[appDisplayPanel].Text = appDefaultText;
                    else
                        sb.Text = appDefaultText;
                }
#if DOTNET_20
                else
                {
                    ToolStripItem tsi = appStatusBar as ToolStripItem;
                    if(tsi != null)
                        tsi.Text = appDefaultText;
                }
#endif
            }
        }

#if DOTNET_20
        /// <summary>
        /// This is used to get or set the tool strip status label component
        /// that can be used to display a status message in conjunction with
        /// the <see cref="ProgressBar"/> component.
        /// </summary>
        /// <remarks>This property is only available when used with .NET 2.0.
        /// </remarks>
        [Browsable(false),
          Description("A tool strip label item used to display a status message")]
        public static ToolStripStatusLabel StatusLabel
        {
            get { return statusLabel; }
            set { statusLabel = value; }
        }

        /// <summary>
        /// This is used to get or set the tool strip progress bar component
        /// that can be used to display progress through a long running task.
        /// </summary>
        /// <remarks>This can be used in conjunction with
        /// <see cref="StatusLabel"/>.  This property is only available when
        /// used with .NET 2.0.</remarks>
        [Browsable(false),
          Description("A tool strip label item used to display a status message")]
        public static ToolStripProgressBar ProgressBar
        {
            get { return progressBar; }
            set { progressBar = value; }
        }
#endif

        /// <summary>
        /// This is used to get or set the status bar component to use for this
        /// instance.
        /// </summary>
        /// <value>If not set or set to null, it will use the status bar
        /// control assigned to the <see cref="ApplicationStatusBar"/>
        /// property.</value>
        /// <seealso cref="ApplicationStatusBar"/>
        /// <seealso cref="ApplicationDisplayPanel"/>
        /// <seealso cref="InstanceDisplayPanel"/>
        /// <exception cref="ArgumentException">This is thrown if the object
        /// is not a status bar control or a tool strip item.</exception>
        [Browsable(false),
          DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
          Description("The status bar or tool strip item to use instead of ApplicationStatusBar")]
        public object InstanceStatusBar
        {
            get { return instanceStatusBar; }
            set
            {
#if !DOTNET_20
                if(value != null && !(value is StatusBar))
#else
                if(value != null && !(value is StatusBar) && !(value is ToolStripItem))
#endif
                    throw new ArgumentException("The object must be " +
                        "a StatusBar or a ToolStripItem");

                // Restore text on the prior status bar control if necessary
                this.Form_MenuComplete(this, EventArgs.Empty);

                instanceStatusBar = value;
                instanceDefaultText = this.CurrentStatusBarText;

                // Set the current text if necessary
                if(value != null)
                    this.Form_Activated(this.StatusBarParentForm,
                        EventArgs.Empty);
            }
        }

        /// <summary>
        /// This is used to set or get the status bar panel in which to
        /// display the messages when the <see cref="InstanceStatusBar"/>
        /// property is set to a status bar.
        /// </summary>
        /// <value>The default is zero (the first panel).  If the status
        /// bar does not have panels, the index exceeds the panel count,
        /// or the status bar's
        /// <see cref="System.Windows.Forms.StatusBar.ShowPanels"/> property
        /// is false, messages will be shown in the status bar's <b>Text</b>
        /// property instead.  If using the <see cref="ApplicationStatusBar"/>,
        /// this property is ignored.  It is also ignored if using a tool
        /// strip item to display the text.</value>
        /// <seealso cref="ApplicationStatusBar"/>
        /// <seealso cref="ApplicationDisplayPanel"/>
        /// <seealso cref="InstanceStatusBar"/>
        [Browsable(false),
          DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
          Description("The panel to use for the status bar text when not " +
            "using ApplicationStatusBar")]
        public int InstanceDisplayPanel
        {
            get { return instanceDisplayPanel; }
            set
            {
                if(value < 0)
                    value = 0;

                // Restore text on the prior status bar control if necessary
                this.Form_MenuComplete(this, EventArgs.Empty);

                instanceDisplayPanel = value;
                instanceDefaultText = this.CurrentStatusBarText;
            }
        }

        /// <summary>
        /// This is used to set or get the default text to show when no item
        /// has the focus or no status text for the instance status bar.
        /// </summary>
        /// <value>By default, it will be set to the text in the selected
        /// status bar component.  If set to a different value, set this
        /// property after specifying the status bar control to use.
        /// </value>
        [Browsable(false),
          DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
          Description("The default text to show when no item has the focus " +
            "for the instance status bar")]
        public string InstanceDefaultText
        {
            get { return instanceDefaultText; }
            set
            {
                instanceDefaultText = value;
                if(instanceStatusBar != null)
                    this.CurrentStatusBarText = instanceDefaultText;
            }
        }
        #endregion

        #region Private class methods
        //=====================================================================
        // Private class properties and methods

        // <summary>
        // This is used to get a reference to the instance status bar
        // component if set or the application status bar component if not.
        // </summary>
        private object StatusBar
        {
            get
            {
                if(instanceStatusBar != null)
                    return instanceStatusBar;

                return appStatusBar;
            }
        }

        // <summary>
        // This is used to get the default status bar text
        // </summary>
        private string StatusBarDefaultText
        {
            get
            {
                if(instanceStatusBar != null)
                    return instanceDefaultText;

                return appDefaultText;
            }
        }

        // <summary>
        // This is used to get a reference to the parent form of the status
        // bar component.
        // </summary>
        private Form StatusBarParentForm
        {
            get
            {
                Form frm = null;
                Control p = this.StatusBar as Control;

                if(p != null)
                    p = p.Parent;
#if DOTNET_20
                else    // It's a control hosted in a tool strip item
                    p = ((ToolStripItem)this.StatusBar).Owner.Parent;
#endif

                while(p != null && frm == null)
                {
                    frm = p as Form;
                    p = p.Parent;
                }

                return frm;
            }
        }

        // <summary>
        // This is used to get a reference to the display panel in the
        // instance status bar if it has been set or the display panel in
        // the application status bar if not.
        // </summary>
        private int DisplayPanel
        {
            get
            {
                if(instanceStatusBar != null)
                    return instanceDisplayPanel;

                return appDisplayPanel;
            }
        }

        // <summary>
        // This is used to get or set the current status bar text
        // </summary>
        private string CurrentStatusBarText
        {
            get
            {
                StatusBar sb = this.StatusBar as StatusBar;

                if(sb != null)
                {
                    if(sb.ShowPanels && this.DisplayPanel < sb.Panels.Count)
                        return sb.Panels[this.DisplayPanel].Text;

                    return sb.Text;
                }
#if DOTNET_20
                else
                {
                    ToolStripItem tsi = this.StatusBar as ToolStripItem;
                    if(tsi != null)
                        return tsi.Text;
                }
#endif
                return null;
            }
            set
            {
                StatusBar sb = this.StatusBar as StatusBar;

                if(sb != null)
                {
                    if(sb.ShowPanels && this.DisplayPanel < sb.Panels.Count)
                        sb.Panels[this.DisplayPanel].Text = value;
                    else
                        sb.Text = value;
                }
#if DOTNET_20
                else
                {
                    ToolStripItem tsi = this.StatusBar as ToolStripItem;
                    if(tsi != null)
                        tsi.Text = value;
                }
#endif
            }
        }

        // <summary>
        // This is handled to redisplay the status bar text for the form's
        // focused control when activated.
        // </summary>
        // <param name="sender">The sender of the event</param>
        // <param name="e">The event arguments</param>
        private void Form_Activated(object sender, System.EventArgs e)
        {
            Form frm = sender as Form;

            if(frm != null && this.StatusBar != null)
            {
                Control ctl = frm.ActiveControl;

                // Nested controls may not have any so walk up
                // the parent chain to see if they do.
                while(ctl != null && !htOptions.Contains(ctl))
                    ctl = ctl.Parent;

                if(ctl != null)
                {
                    this.CurrentStatusBarText = this.ItemText(ctl);

//                    System.Diagnostics.Debug.Write(sender.GetType().FullName + "  ");
//                    System.Diagnostics.Debug.WriteLine(
//                        "Form_Activated set text: " + this.CurrentStatusBarText);
                }
            }
        }

        // <summary>
        // This is handled to display status bar text for a
        // <see cref="MenuItem"/> component.
        // </summary>
        // <param name="sender">The sender of the event</param>
        // <param name="e">The event arguments</param>
        private void Menu_Select(object sender, EventArgs e)
        {
            if(this.StatusBar == null || !htOptions.Contains(sender))
                return;

            // Hook the MenuComplete event on first use to restore the
            // existing status bar text.
            if(!hookedMenuEvents)
            {
                Form frm = this.StatusBarParentForm;

                if(frm != null)
                {
                    frm.MenuComplete += new EventHandler(Form_MenuComplete);
                    hookedMenuEvents = true;
                }
            }

            this.CurrentStatusBarText = this.ItemText(sender);

//            System.Diagnostics.Debug.Write(sender.GetType().FullName + "  ");
//            System.Diagnostics.Debug.WriteLine(
//                "Menu_Select set text: " + this.CurrentStatusBarText);
        }

        // <summary>
        // This is handled so that the old status bar text is restored when
        // the component status bar text is no longer needed.
        // </summary>
        // <param name="sender">The sender of the event</param>
        // <param name="e">The event arguments</param>
        private void Form_MenuComplete(object sender, System.EventArgs e)
        {
            if(this.StatusBar == null)
                return;

            this.CurrentStatusBarText = this.StatusBarDefaultText;

//            System.Diagnostics.Debug.Write(sender.GetType().FullName + "  ");
//            System.Diagnostics.Debug.WriteLine("MenuComplete restored text: " +
//                this.StatusBarDefaultText);
        }

        // <summary>
        // This is handled to display status bar text when a control
        // is entered or gains the focus.
        // </summary>
        // <param name="sender">The sender of the event</param>
        // <param name="e">The event arguments</param>
        private void Control_Enter(object sender, EventArgs e)
        {
            if(this.StatusBar == null || !htOptions.Contains(sender))
                return;

            // Hook the Closed event to reset the status bar text
            // when the form is closed.  When it's modal, it doesn't
            // always get the Leave event to reset the text.  The
            // Activated and Deactivated events are also hooked to
            // set and restore the text.
            if(!hookedFormEvent && this.StatusBar != null)
            {
                Control p = sender as Control;

                if(p != null)
                    p = p.Parent;
#if DOTNET_20
                else    // It's a control hosted in a tool strip item
                    p = ((ToolStripItem)sender).Owner.Parent;
#endif

                while(p != null)
                {
                    Form frm = p as Form;

                    if(frm != null)
                    {
                        frm.Activated += new EventHandler(Form_Activated);

                        // Same handler as Form.MenuComplete
                        frm.Deactivate += new EventHandler(Form_MenuComplete);
                        frm.Closed += new EventHandler(Form_MenuComplete);

                        hookedFormEvent = true;
                        break;
                    }

                    p = p.Parent;
                }
            }

            this.CurrentStatusBarText = this.ItemText(sender);

//            System.Diagnostics.Debug.Write(sender.GetType().FullName + "  ");
//            System.Diagnostics.Debug.WriteLine("Control_Enter set text: " +
//               this.CurrentStatusBarText);
        }

        // <summary>
        // This is handled to display status bar text when a control
        // is left.
        // </summary>
        // <param name="sender">The sender of the event</param>
        // <param name="e">The event arguments</param>
        private void Control_Leave(object sender, EventArgs e)
        {
            if(this.StatusBar == null || !htOptions.Contains(sender))
                return;

            this.CurrentStatusBarText = this.StatusBarDefaultText;

//            System.Diagnostics.Debug.Write(sender.GetType().FullName + "  ");
//            System.Diagnostics.Debug.WriteLine("Control_Leave restored text: " +
//                this.StatusBarDefaultText);
        }

        // <summary>
        // Get the text for the specified item.
        // </summary>
        // <param name="item">The item for which to get the message text</param>
        // <remarks>If <see cref="PropertyOptions.ShowAsBlank"/> is true, it
        // returns an empty string.  Otherwise, it returns the
        // <see cref="PropertyOptions.Message"/> value.  If the item has no
        // status bar text defined, it returns null.</remarks>
        private string ItemText(object item)
        {
            if(item == null || !htOptions.Contains(item))
                return null;

            TabControl tc = item as TabControl;

            // Show status bar text for the tab page if there is any
            if(tc != null && htOptions.Contains(tc.SelectedTab))
                item = tc.SelectedTab;

            PropertyOptions po = (PropertyOptions)htOptions[item];

            if(po.ShowAsBlank)
                return String.Empty;

            return po.Message;
        }
        #endregion

        //=====================================================================
        // IExtenderProvider implementation

        /// <summary>
        /// This is implemented to determine if the component can be extended
        /// with the extra properties.
        /// </summary>
        /// <param name="extendee">The object to check</param>
        /// <returns>True if it can be extended, false if not</returns>
        /// <remarks><see cref="MenuItem"/> components and everything derived
        /// from <b>ToolStripItem</b> (.NET 2.0) and
        /// <see cref="Control"/> are extended with the <b>StatusBarText</b>
        /// and <b>ShowAsBlank</b> properties with the following exceptions:
        /// <b>Form</b>, <b>Label</b>, <b>PictureBox</b>, <b>ProgressBar</b>,
        /// <b>ScrollBar</b>, <b>Splitter</b>, <b>StatusBar</b>,
        /// <b>ToolBar</b>, <b>ToolStrip</b> (.NET 2.0) and controls derived
        /// from them.  The <b>Form</b> contain controls and the other controls
        /// cannot receive the focus needed to display the text so there is no
        /// point in giving them status bar text.  The exception to the above
        /// rule is <b>LinkLabel</b> which can receive the focus and thus can
        /// be extended.  <b>ToolStripItem</b> components are only supported
        /// when built for use with .NET 2.0.</remarks>
        public bool CanExtend(object extendee)
        {
            // MenuItem is a Component.  LinkLabel derives from Label but
            // it can gain the focus and thus can be extended.  For .NET 2.0,
            // we also support the ToolStripItem component.
#if !DOTNET_20
            if(extendee is MenuItem || extendee is LinkLabel)
#else
            if(extendee is MenuItem || extendee is LinkLabel ||
              extendee is ToolStripItem)
#endif
                return true;

            // Non-Control types, Form, and these specific controls can't be
            // extended as it doesn't make sense for them as they don't gain
            // the focus needed to display the text.
            if(!(extendee is Control) || extendee is Form ||
              extendee is Label || extendee is PictureBox ||
              extendee is ProgressBar || extendee is ScrollBar ||
              extendee is Splitter || extendee is StatusBar ||
#if !DOTNET_20
              extendee is ToolBar)
#else
              extendee is ToolBar || extendee is ToolStrip)
#endif
                return false;

            // All other Control types can be extended
            return true;
        }

        /// <summary>
        /// This is used to retrieve the status bar text for a component.
        /// </summary>
        /// <param name="comp">The component for which to get the status bar
        /// text</param>
        /// <returns>The message string if found or null if not found</returns>
        /// <exception cref="ArgumentException">This is thrown if the component
        /// is null or if it is not a menu item, control, or a tool strip item.
        /// </exception>
        [Category("StatusBar"), Localizable(true), DefaultValue(null),
          Description("The status bar text for the item")]
        public string GetStatusBarText(Component comp)
        {
            if(comp == null)
                throw new ArgumentException("Component cannot be null");

#if !DOTNET_20
            if(!(comp is MenuItem) && !(comp is Control))
#else
            if(!(comp is MenuItem) && !(comp is Control) && !(comp is ToolStripItem))
#endif
                throw new ArgumentException(
                    "Component must be a MenuItem, ToolStripItem, or a Control");

            if(htOptions.Contains(comp))
                return ((PropertyOptions)htOptions[comp]).Message;

            return null;
        }

        /// <summary>
        /// This stores the status bar text for the specified component.
        /// </summary>
        /// <param name="comp">The component associated with the message</param>
        /// <param name="message">The status bar text for the component</param>
        /// <remarks>The designer converts strings containing nothing but
        /// spaces to empty strings and won't serialize them to code.  If you
        /// want a blank string to display, use <see cref="SetShowAsBlank"/>
        /// to set the "show as blank" flag to true instead.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the component
        /// is null or if it is not a menu item, control, or a tool strip item.
        /// </exception>
        public void SetStatusBarText(Component comp, string message)
        {
            if(comp == null)
                throw new ArgumentException("Component cannot be null");

            MenuItem mi = comp as MenuItem;
            Control ctl = comp as Control;
            TabControl tc = comp as TabControl;

#if DOTNET_20
            ToolStripItem ti = comp as ToolStripItem;
            ToolStripControlHost tsch = comp as ToolStripControlHost;

            if(mi == null && ti == null && ctl == null)
#else
            if(mi == null && ctl == null)
#endif
                throw new ArgumentException(
                    "Component must be a MenuItem, ToolStripItem, or a Control");

            if(message != null && message.Length == 0)
                message = null;

            if(!htOptions.Contains(comp))
            {
                htOptions.Add(comp, new PropertyOptions(message));

                if(!this.DesignMode && message != null)
                    if(mi != null)
                        mi.Select += new EventHandler(Menu_Select);
                    else
#if DOTNET_20
                        if(ti != null)
                        {
                            ti.MouseEnter += new EventHandler(Control_Enter);
                            ti.MouseLeave += new EventHandler(Control_Leave);

                            // If it's a control host, hook the enter and
                            // leave events too.
                            if(tsch != null)
                            {
                                tsch.Enter += new EventHandler(Control_Enter);
                                tsch.Leave += new EventHandler(Control_Leave);
                            }
                        }
                        else
#endif
                        {
                            ctl.GotFocus += new EventHandler(Control_Enter);
                            ctl.Enter += new EventHandler(Control_Enter);
                            ctl.Leave += new EventHandler(Control_Leave);

                            // If it's a tab control, hook the SelectedIndexChanged
                            // event to allow displaying status bar text for it and
                            // its pages.  The tab control and tab pages don't
                            // reliably show their status bar text due to the way
                            // they handle the focus.  As such, this event is
                            // needed to update the text.  Note that it still won't
                            // show the text if you use Shift+Tab to go from the
                            // first control on a tab back to the tab in the tab
                            // control itself or when it is the first control
                            // to have the focus.  You must also set ShowAsBlank or
                            // StatusBarText on the tab control itself for text to
                            // appear for the tab pages.
                            if(tc != null)
                                tc.SelectedIndexChanged += new EventHandler(
                                    Control_Enter);
                        }
            }
            else
            {
                PropertyOptions po = (PropertyOptions)htOptions[comp];
                po.Message = message;

                if(!this.DesignMode && message == null &&
                  po.ShowAsBlank == false)
                    if(mi != null)
                        mi.Select -= new EventHandler(Menu_Select);
                    else
#if DOTNET_20
                        if(ti != null)
                        {
                            ti.MouseEnter -= new EventHandler(Control_Enter);
                            ti.MouseLeave -= new EventHandler(Control_Leave);

                            // If it's a control host, unhook the enter and
                            // leave events too.
                            if(tsch != null)
                            {
                                tsch.Enter -= new EventHandler(Control_Enter);
                                tsch.Leave -= new EventHandler(Control_Leave);
                            }
                        }
                        else
#endif
                        {
                            ctl.GotFocus -= new EventHandler(Control_Enter);
                            ctl.Enter -= new EventHandler(Control_Enter);
                            ctl.Leave -= new EventHandler(Control_Leave);

                            if(tc != null)
                                tc.SelectedIndexChanged -= new EventHandler(
                                    Control_Enter);
                        }
            }

            // Refresh the text if the control is focused
            if(message != null && ctl != null && ctl.Focused)
                Control_Enter(ctl, EventArgs.Empty);
        }

        /// <summary>
        /// This is used to retrieve the "show as blank" flag for a menu item
        /// or control.
        /// </summary>
        /// <param name="comp">The item for which to get the flag</param>
        /// <returns>True if it will show a blank status bar message or false
        /// if not.</returns>
        /// <exception cref="ArgumentException">This is thrown if the component
        /// is null or if it is not a menu item, control, or a tool strip item.
        /// </exception>
        [Category("StatusBar"), DefaultValue(false),
          Description("The flag that determines whether or not to show a blank for the item")]
        public bool GetShowAsBlank(Component comp)
        {
            if(comp == null)
                throw new ArgumentException("Component cannot be null");

#if !DOTNET_20
            if(!(comp is MenuItem) && !(comp is Control))
#else
            if(!(comp is MenuItem) && !(comp is Control) && !(comp is ToolStripItem))
#endif
                throw new ArgumentException(
                    "Component must be a MenuItem, ToolStripItem, or a Control");

            if(htOptions.Contains(comp))
                return ((PropertyOptions)htOptions[comp]).ShowAsBlank;

            return false;
        }

        /// <summary>
        /// This stores the "show as blank" flag for the specified component.
        /// </summary>
        /// <param name="comp">The component associated with the property</param>
        /// <param name="showBlank">The flag value for the component</param>
        /// <remarks>The designer converts strings containing nothing but
        /// spaces to empty strings and this equates to the default and the
        /// value is not serialized to code. If you want a blank string to
        /// display, set this property to true instead.  This property takes
        /// precedence over the <b>StatusBarText</b> property.</remarks>
        /// <exception cref="ArgumentException">This is thrown if the component
        /// is null or if it is not a menu item, control, or a tool strip item.
        /// </exception>
        public void SetShowAsBlank(Component comp, bool showBlank)
        {
            if(comp == null)
                throw new ArgumentException("Component cannot be null");

            MenuItem mi = comp as MenuItem;
            Control ctl = comp as Control;
            TabControl tc = comp as TabControl;

#if DOTNET_20
            ToolStripItem ti = comp as ToolStripItem;
            ToolStripControlHost tsch = comp as ToolStripControlHost;

            if(mi == null && ti == null && ctl == null)
#else
            if(mi == null && ctl == null)
#endif
                throw new ArgumentException(
                    "Component must be a MenuItem, ToolStripItem, or a Control");

            if(!htOptions.Contains(comp))
            {
                htOptions.Add(comp, new PropertyOptions(showBlank));

                // Hook up the event handlers if necessary
                if(!this.DesignMode && showBlank)
                    if(mi != null)
                        mi.Select += new EventHandler(Menu_Select);
                    else
#if DOTNET_20
                        if(ti != null)
                        {
                            ti.MouseEnter += new EventHandler(Control_Enter);
                            ti.MouseLeave += new EventHandler(Control_Leave);

                            // If it's a control host, hook the enter and
                            // leave events too.
                            if(tsch != null)
                            {
                                tsch.Enter += new EventHandler(Control_Enter);
                                tsch.Leave += new EventHandler(Control_Leave);
                            }
                        }
                        else
#endif
                        {
                            ctl.GotFocus += new EventHandler(Control_Enter);
                            ctl.Enter += new EventHandler(Control_Enter);
                            ctl.Leave += new EventHandler(Control_Leave);

                            // See SetStatusBarText for why we do this
                            if(tc != null)
                                tc.SelectedIndexChanged += new EventHandler(
                                    Control_Enter);
                        }
            }
            else
            {
                PropertyOptions po = (PropertyOptions)htOptions[comp];
                po.ShowAsBlank = showBlank;

                // Disconnect the event handlers if necessary
                if(!this.DesignMode && po.Message == null && showBlank == false)
                    if(mi != null)
                        mi.Select -= new EventHandler(Menu_Select);
                    else
#if DOTNET_20
                        if(ti != null)
                        {
                            ti.MouseEnter -= new EventHandler(Control_Enter);
                            ti.MouseLeave -= new EventHandler(Control_Leave);

                            // If it's a control host, unhook the enter and
                            // leave events too.
                            if(tsch != null)
                            {
                                tsch.Enter -= new EventHandler(Control_Enter);
                                tsch.Leave -= new EventHandler(Control_Leave);
                            }
                        }
                        else
#endif
                        {
                            ctl.GotFocus -= new EventHandler(Control_Enter);
                            ctl.Enter -= new EventHandler(Control_Enter);
                            ctl.Leave -= new EventHandler(Control_Leave);

                            if(tc != null)
                                tc.SelectedIndexChanged -= new EventHandler(
                                    Control_Enter);
                        }
            }

            // Refresh the text if the control is focused
            if(showBlank && ctl != null && ctl.Focused)
                Control_Enter(ctl, EventArgs.Empty);
        }

#if DOTNET_20
        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="maximum">The maximum value</param>
        /// <overloads>There are six overloads for this method.</overloads>
        public static void InitializeProgressBar(int maximum)
        {
            StatusBarTextProvider.InitializeProgressBar(0, maximum, 0,
                1, null);
        }

        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="maximum">The maximum value</param>
        /// <param name="progressNote">A note for the progress note status
        /// label control.  If null, the current text is left alone.</param>
        public static void InitializeProgressBar(int maximum,
          string progressNote)
        {
            StatusBarTextProvider.InitializeProgressBar(0, maximum, 0,
                1, progressNote);
        }

        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="minimum">The minimum value</param>
        /// <param name="maximum">The maximum value</param>
        public static void InitializeProgressBar(int minimum, int maximum)
        {
            StatusBarTextProvider.InitializeProgressBar(minimum, maximum,
                0, 1, null);
        }

        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="minimum">The minimum value</param>
        /// <param name="maximum">The maximum value</param>
        /// <param name="current">The current value</param>
        public static void InitializeProgressBar(int minimum, int maximum,
          int current)
        {
            StatusBarTextProvider.InitializeProgressBar(minimum, maximum,
                current, 1, null);
        }

        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="minimum">The minimum value</param>
        /// <param name="maximum">The maximum value</param>
        /// <param name="progressNote">A note for the progress note status
        /// label control.  If null, the current text is left alone.</param>
        public static void InitializeProgressBar(int minimum, int maximum,
          string progressNote)
        {
            StatusBarTextProvider.InitializeProgressBar(minimum, maximum,
                0, 1, progressNote);
        }

        /// <summary>
        /// This can be used to initialize the status strip progress bar
        /// controls.
        /// </summary>
        /// <param name="minimum">The minimum value</param>
        /// <param name="maximum">The maximum value</param>
        /// <param name="current">The current value</param>
        /// <param name="step">The step value for the progress bar</param>
        /// <param name="progressNote">A note for the progress note status
        /// label control.  If null, the current text is left alone.</param>
        public static void InitializeProgressBar(int minimum, int maximum,
          int current, int step, string progressNote)
        {
            if(progressNote != null && statusLabel != null)
            {
                statusLabel.Text = progressNote;
                statusLabel.Owner.Invalidate(true);
                statusLabel.Owner.Update();
            }

            if(progressBar != null)
            {
                progressBar.Minimum = minimum;
                progressBar.Maximum = maximum;
                progressBar.Value = current;
                progressBar.Step = step;
            }
        }

        /// <summary>
        /// This can be used to update the progress bar with a new value
        /// </summary>
        /// <remarks>This version updates the progress by the step
        /// value.</remarks>
        /// <overloads>There are four overloads for this method</overloads>
        public static void UpdateProgress()
        {
            if(progressBar != null)
                progressBar.PerformStep();
        }

        /// <summary>
        /// This can be used to update the progress bar with a new value
        /// </summary>
        /// <param name="progressNote">A note for the progress note status
        /// label control.</param>
        /// <remarks>This version updates the progress by the step
        /// value and updates the progress note.</remarks>
        public static void UpdateProgress(string progressNote)
        {
            if(statusLabel != null)
            {
                statusLabel.Text = progressNote;
                statusLabel.Owner.Invalidate(true);
                statusLabel.Owner.Update();
            }

            if(progressBar != null)
                progressBar.PerformStep();
        }

        /// <summary>
        /// This can be used to update the progress bar with a new value
        /// </summary>
        /// <param name="current">The current value</param>
        public static void UpdateProgress(int current)
        {
            StatusBarTextProvider.UpdateProgress(current, null);
        }

        /// <summary>
        /// This can be used to update the progress bar with a new value
        /// </summary>
        /// <param name="current">The current value</param>
        /// <param name="progressNote">A note for the progress note status
        /// label control.  If null, the current text is left alone.</param>
        public static void UpdateProgress(int current, string progressNote)
        {
            if(progressNote != null && statusLabel != null)
            {
                statusLabel.Text = progressNote;
                statusLabel.Owner.Invalidate(true);
                statusLabel.Owner.Update();
            }

            if(progressBar != null)
                progressBar.Value = current;
        }

        /// <summary>
        /// This can be used to reset the status strip progress bar
        /// to its minimum value.
        /// </summary>
        /// <remarks>This version resets the progress bar to zero and
        /// clears the progress note.</remarks>
        /// <overloads>There are two overloads for this method.</overloads>
        public static void ResetProgressBar()
        {
            StatusBarTextProvider.ResetProgressBar(null);
        }

        /// <summary>
        /// This can be used to reset the status strip progress bar
        /// to its minimum value and display a new progress message.
        /// </summary>
        /// <param name="progressNote">The progress note to display in
        /// the progress note status label.  If set to null, the
        /// message is cleared.</param>
        public static void ResetProgressBar(string progressNote)
        {
            if(statusLabel != null)
            {
                statusLabel.Text = progressNote;
                statusLabel.Owner.Invalidate(true);
                statusLabel.Owner.Update();
            }

            if(progressBar != null)
                progressBar.Value = progressBar.Minimum;
        }
#endif
    }
}
