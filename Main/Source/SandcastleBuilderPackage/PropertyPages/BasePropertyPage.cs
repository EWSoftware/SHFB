//=============================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BasePropertyPage.cs
// Author  : Eric Woodruff
// Updated : 12/31/2011
// Note    : Copyright 2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used as the base class for package property pages
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.  This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.0  03/27/2011  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using SandcastleBuilder.Utils.Design;
using SHFBUtility = SandcastleBuilder.Utils.Utility;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used as a base class for package property pages
    /// </summary>
    /// <remarks>This control handles the common tasks of a property page such
    /// as binding controls to project properties and storing the values when
    /// they change.  The property "binding" is done by specifying the project
    /// property name in the control's <c>Tag</c> property.</remarks>
    [ComVisible(true), ToolboxItem(false)]
    public partial class BasePropertyPage : UserControl, IPropertyPage2
    {
        #region Private data members
        //=====================================================================

        // This is used to define custom user controls that should be scanned
        private static Collection<string> customUserControls = new Collection<string>();

        // This is used to define custom controls and their value property
        private static Dictionary<string, string> customControls = new Dictionary<string, string>();

        private bool isDirty, isBinding;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property can be overridden to provide custom validation
        /// for the property page.
        /// </summary>
        /// <value>Return true if valid, or false if not</value>
        protected virtual bool IsValid
        {
            get { return true; }
        }

        /// <summary>
        /// This is used to get or set the title of the property page
        /// </summary>
        protected string Title { get; set; }

        /// <summary>
        /// This is used to get or set an optional help keyword associated with the property page
        /// </summary>
        protected string HelpKeyword { get; set; }

        /// <summary>
        /// This read-only property returns true if binding is occurring or false if not
        /// </summary>
        protected bool IsBinding
        {
            get { return isBinding; }
        }

        /// <summary>
        /// This is used to get or set the dirty state of the property page
        /// </summary>
        protected bool IsDirty
        {
            get { return isDirty; }
            set
            {
                if(isDirty != value && !isBinding)
                {
                    isDirty = value;

                    if(this.PropertyPageSite != null)
                        this.PropertyPageSite.OnStatusChange((uint)(isDirty ? PropPageStatus.Dirty : PropPageStatus.Clean));
                }
            }
        }

        /// <summary>
        /// This read-only property returns the site for the property page
        /// </summary>
        protected IPropertyPageSite PropertyPageSite { get; private set; }

        /// <summary>
        /// This read-only property returns the project manager for the property page
        /// </summary>
        protected ProjectNode ProjectMgr { get; private set; }

        /// <summary>
        /// This read-only property returns the project configurations for the property page
        /// </summary>
        protected ReadOnlyCollection<ProjectConfig> ProjectConfigs { get; private set; }

        /// <summary>
        /// This is used to access the custom control property mapping dictionary
        /// </summary>
        /// <remarks>The class recognizes the basic edit controls and those
        /// derived from them.  If you have custom controls that it does not
        /// recognize (i.e. those derived from <b>UserControl</b>), you can
        /// add them to this dictionary using the fully qualified type name
        /// including namespaces and the property value to use for binding as
        /// the value for the entry.
        /// <p/><b>NOTE:</b>You only need to add controls to this property
        /// if it is not derived from one of the standard edit controls (see
        /// <see cref="BindProperties"/> for more information).
        /// <p/>This property is static so it can be populated once at start-up
        /// for use throughout the application's lifetime.</remarks>
        /// <example>
        /// <code lang="cs">
        /// // Use the SelectedValue property for binding in controls of type
        /// // EWSoftware.ListControls.MultiColumnComboBox.  This control is
        /// // not derived from a standard control so we need to add it manually.
        /// BasePropertyPage.CustomControls.Add(
        ///     "EWSoftware.ListControls.MultiColumnComboBox",
        ///     "SelectedValue");
        /// </code>
        /// </example>
        public static Dictionary<string, string> CustomControls
        {
            get { return customControls; }
        }

        /// <summary>
        /// This is used to access the custom user control list
        /// </summary>
        /// <remarks>The class recognizes the basic panel and container controls
        /// but will ignore all <c>UserControl</c> derived objects.  If you have
        /// a user control that you want included in the binding procedure, add
        /// its full type name to this collection.
        /// <p/><b>NOTE:</b>You only need to add type names to this property
        /// if it is not derived from one of the container controls (see
        /// <see cref="BindProperties"/> for more information).
        /// <p/>This property is static so it can be populated once at start-up
        /// for use throughout the application's lifetime.</remarks>
        /// <example>
        /// <code lang="cs">
        /// // Add custom user controls to the change tracking container list
        /// BasePropertyPage.CustomUserControls.Add("SomeCompany.Controls.UITab");
        /// BasePropertyPage.CustomUserControls.Add("SomeCompany.Controls.UITabPage");
        /// </code>
        /// </example>
        public static Collection<string> CustomUserControls
        {
            get { return customUserControls; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BasePropertyPage()
        {
            InitializeComponent();
            this.Font = Utility.GetDialogFont();
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        protected override bool ProcessMnemonic(char charCode)
        {
            return base.ProcessMnemonic(charCode);
        }

        /// <summary>
        /// This is overridden to handle the F1 key to show help for the page and to process mnemonics
        /// </summary>
        /// <param name="keyData">The key data to check</param>
        /// <returns>True if the key was handled, false if not</returns>
        /// <remarks>This appears to be necessary because the <c>IPropertyPage2.TranslateAccelerator</c>
        /// method is never called.  As such, Visual Studio invokes help on F1 rather than us getting a
        /// <c>HelpRequested</c> event and it invokes menus if a mnemonic matches a menu hot key rather
        /// than focusing the associated control.</remarks>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if(keyData == Keys.F1)
            {
                if(this.ShowHelp())
                    return true;
            }
            else
                if((keyData & ~Keys.KeyCode) == Keys.Alt)
                {
                    Keys key = (keyData & ~Keys.Alt);

                    if(((Char.IsLetterOrDigit((char)((ushort)key)) && (key < Keys.F1 || key > Keys.F24)) ||
                      (key >= Keys.NumPad0 && key <= Keys.Divide)) && this.ProcessMnemonic((char)((ushort)key)))
                          return true;
                }

            return base.ProcessDialogKey(keyData);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This can be overridden to perform custom initialization for the
        /// property page.
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Derived classes can connect the change event on custom controls to this method to
        /// notify the page of changes in the control's value.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnPropertyChanged(object sender, EventArgs e)
        {
            if(!isBinding)
            {
                this.IsDirty = true;
                this.RefreshControlState(sender, e);
            }
        }

        /// <summary>
        /// This can be overridden to specify whether the property value is escaped or not.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if the property contains an escaped value, false if not.  When bound,
        /// an escaped property's value is unescaped before it is assigned to the control.  When
        /// stored, the new value is escaped before it is stored.</returns>
        protected virtual bool IsEscapedProperty(string propertyName)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to bind a control to a property in a manner
        /// other than the default handling supplied by the base class.
        /// </summary>
        /// <param name="control">The control to bind</param>
        /// <returns>True if the method bound the control or it should be ignored,
        /// false if the base class should attempt to bind it in the default manner.</returns>
        protected virtual bool BindControlValue(Control control)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to store a control value in a property in a manner
        /// other than the default handling supplied by the base class.
        /// </summary>
        /// <param name="control">The control from which to store the value</param>
        /// <returns>True if the method stored the control value or it should be ignored,
        /// false if the base class should attempt to store the value in the default manner.</returns>
        protected virtual bool StoreControlValue(Control control)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to refresh the control state when certain events occur
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected virtual void RefreshControlState(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// This can be overridden to display help for the property page
        /// </summary>
        /// <returns>True if a keyword has been set and help was shown or false if not</returns>
        protected virtual bool ShowHelp()
        {
            if(!String.IsNullOrEmpty(this.HelpKeyword))
            {
                SHFBUtility.ShowHelpTopic(this.HelpKeyword);
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used to bind the controls in the given collection to their
        /// associated project properties.
        /// </summary>
        /// <param name="controls">The control collection from which to get
        /// the bound controls.  Controls are bound if their <see cref="Control.Tag"/>
        /// property is a string that matches a project property.</param>
        /// <remarks>This method is recursive</remarks>
        protected void BindProperties(Control.ControlCollection controls)
        {
            Type t;
            PropertyInfo pi;
            string typeName, boundProperty;

            try
            {
                isBinding = true;

                foreach(Control c in controls)
                {
                    t = c.GetType();
                    typeName = t.FullName;
                    boundProperty = c.Tag as string;

                    // Ignore unbound controls
                    if(String.IsNullOrEmpty(boundProperty))
                    {
                        // Scan containers too except for user controls unless they are in the custom user control
                        // list.  They may or may not represent a single-valued item and we can't process them
                        // reliably.  The same could be true of one of these if it is a derived type.
                        if(!customControls.ContainsKey(typeName) && (c is GroupBox || c is Panel || c is TabControl ||
                          c is TabPage || c is SplitContainer || customUserControls.Contains(typeName)))
                        {
                            this.BindProperties(c.Controls);
                            continue;
                        }

                        continue;
                    }

                    // Check for custom types first
                    if(customControls.ContainsKey(typeName))
                    {
                        // Find and connect the Changed event for the named property if one exists
                        var changedEvent = t.GetEvents().Where(ev =>
                            ev.Name == customControls[typeName] + "Changed").FirstOrDefault();

                        if(changedEvent != null)
                        {
                            EventHandler h = new EventHandler(OnPropertyChanged);
                            changedEvent.RemoveEventHandler(c, h);
                            changedEvent.AddEventHandler(c, h);
                        }

                        pi = t.GetProperty(customControls[typeName], BindingFlags.Public | BindingFlags.Instance);
                    }
                    else if(c is TextBoxBase || c is Label)
                    {
                        c.TextChanged -= OnPropertyChanged;
                        c.TextChanged += OnPropertyChanged;

                        pi = t.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                    }
                    else if(c is ComboBox)
                    {
                        ComboBox cbo = (ComboBox)c;
                        cbo.SelectedIndexChanged -= OnPropertyChanged;
                        cbo.SelectedIndexChanged += OnPropertyChanged;

                        if(cbo.DataSource != null)
                            pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                        else
                            pi = t.GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance);
                    }
                    else if(c is CheckBox)
                    {
                        CheckBox cb = (CheckBox)c;
                        cb.CheckedChanged -= OnPropertyChanged;
                        cb.CheckedChanged += OnPropertyChanged;

                        pi = t.GetProperty("Checked", BindingFlags.Public | BindingFlags.Instance);
                    }
                    else if((c is DateTimePicker) || (c is UpDownBase) || (c is TrackBar))
                    {
                        DateTimePicker dtp = c as DateTimePicker;

                        if(dtp != null)
                        {
                            dtp.ValueChanged -= OnPropertyChanged;
                            dtp.ValueChanged += OnPropertyChanged;
                        }
                        else
                        {
                            UpDownBase udc = c as UpDownBase;

                            if(udc != null)
                            {
                                udc.TextChanged -= OnPropertyChanged;
                                udc.TextChanged += OnPropertyChanged;
                            }
                            else
                            {
                                TrackBar tbar = (TrackBar)c;
                                tbar.ValueChanged -= OnPropertyChanged;
                                tbar.ValueChanged += OnPropertyChanged;
                            }
                        }

                        pi = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                    }
                    else if(c is CheckedListBox)
                    {
                        CheckedListBox clb = (CheckedListBox)c;
                        clb.ItemCheck -= OnPropertyChanged;
                        clb.ItemCheck += OnPropertyChanged;

                        // Since CheckedListBox is a multi-valued control, the user will have to bind it
                        // in the BindControlValue() method override.  They'll have to store it in the
                        // StoreControlValue() method override too.
                        pi = null;
                    }
                    else if(c is ListBox)
                    {
                        ListBox lb = (ListBox)c;
                        lb.SelectedIndexChanged -= OnPropertyChanged;
                        lb.SelectedIndexChanged += OnPropertyChanged;

                        if(lb.DataSource != null)
                            pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                        else
                            pi = t.GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance);
                    }
                    else
                        pi = null;

                    // Give the user a chance to handle the control in a custom fashion.  If not handled and we
                    // couldn't figure out what to use, ignore it.
                    if(!this.BindControlValue(c) && pi != null)
                        this.Bind(c, pi, boundProperty);
                }
            }
            finally
            {
                isBinding = false;
            }
        }

        /// <summary>
        /// Bind the control to the property value by setting the property to
        /// the current value from the project.
        /// </summary>
        /// <param name="control">The control to bind</param>
        /// <param name="propertyInfo">The property information</param>
        /// <param name="boundProperty">The project property name</param>
        private void Bind(Control control, PropertyInfo propertyInfo, string boundProperty)
        {
            string propValue = null;
            object controlValue;

            if(this.ProjectMgr != null)
            {
                var projProp = this.ProjectMgr.BuildProject.GetProperty(boundProperty);

                if(projProp != null)
                    propValue = projProp.UnevaluatedValue;

                // If null, the property probably doesn't exist so ignore it
                if(propValue == null)
                    return;

                if(this.IsEscapedProperty(boundProperty))
                    propValue = EscapeValueAttribute.Unescape(propValue);

                // Set the value based on the type
                switch(Type.GetTypeCode(propertyInfo.PropertyType))
                {
                    case TypeCode.Object:
                    case TypeCode.String:
                        controlValue = propValue;
                        break;

                    case TypeCode.Char:
                        controlValue = propValue[0];
                        break;

                    case TypeCode.Byte:
                        controlValue = Convert.ToByte(propValue[0]);
                        break;

                    case TypeCode.SByte:
                        controlValue = Convert.ToSByte(propValue[0]);
                        break;

                    case TypeCode.Decimal:
                        controlValue = Convert.ToDecimal(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Double:
                        controlValue = Convert.ToDouble(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Single:
                        controlValue = Convert.ToSingle(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Int16:
                        controlValue = Convert.ToInt16(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Int32:
                        controlValue = Convert.ToInt32(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Int64:
                        controlValue = Convert.ToInt64(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.UInt16:
                        controlValue = Convert.ToUInt16(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.UInt32:
                        controlValue = Convert.ToUInt32(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.UInt64:
                        controlValue = Convert.ToUInt64(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.Boolean:
                        controlValue = Convert.ToBoolean(propValue, CultureInfo.CurrentCulture);
                        break;

                    case TypeCode.DateTime:
                        controlValue = Convert.ToDateTime(propValue, CultureInfo.CurrentCulture);
                        break;

                    default:        // Ignore unknown types
                        return;
                }

                propertyInfo.SetValue(control, controlValue, null);
            }
        }

        /// <summary>
        /// This is used to store the control values in the given collection to
        /// their associated project properties.
        /// </summary>
        /// <param name="controls">The control collection from which to get
        /// the values.  Controls are bound if their <see cref="Control.Tag"/>
        /// property is a string that matches a project property.</param>
        /// <remarks>This method is recursive</remarks>
        protected void StoreProperties(Control.ControlCollection controls)
        {
            Type t;
            PropertyInfo pi;
            object controlValue;
            string typeName, boundProperty, propValue;

            foreach(Control c in controls)
            {
                t = c.GetType();
                typeName = t.FullName;
                boundProperty = c.Tag as string;

                // Ignore unbound controls
                if(String.IsNullOrEmpty(boundProperty))
                {
                    // Scan containers too except for user controls unless they are in the custom user control
                    // list.  They may or may not represent a single-valued item and we can't process them
                    // reliably.  The same could be true of one of these if it's a derived type.
                    if(!customControls.ContainsKey(typeName) && (c is GroupBox || c is Panel || c is TabControl ||
                      c is TabPage || c is SplitContainer || customUserControls.Contains(typeName)))
                        this.StoreProperties(c.Controls);

                    continue;
                }

                // Check for custom types first
                if(customControls.ContainsKey(typeName))
                {
                    pi = t.GetProperty(customControls[typeName], BindingFlags.Public | BindingFlags.Instance);
                }
                else if(c is TextBoxBase || c is Label)
                    pi = t.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                else if(c is ComboBox)
                {
                    if(((ComboBox)c).DataSource != null)
                        pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                    else
                        pi = t.GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance);
                }
                else if(c is CheckBox)
                    pi = t.GetProperty("Checked", BindingFlags.Public | BindingFlags.Instance);
                else if((c is DateTimePicker) || (c is UpDownBase) || (c is TrackBar))
                    pi = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                else if(c is ListBox)
                {
                    if(((ListBox)c).DataSource != null)
                        pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                    else
                        pi = t.GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance);
                }
                else
                    pi = null;

                // Note that CheckedListBox is not handled here since it is most likely multi-valued.  The
                // user must store it in the StoreControLValue() method override.

                // Give the user a chance to handle the control in a custom fashion.  If not handled and we
                // couldn't figure out what to use, ignore it.
                if(!this.StoreControlValue(c) && pi != null)
                {
                    controlValue = pi.GetValue(c, null);

                    if(controlValue == null)
                        propValue = String.Empty;
                    else
                        propValue = controlValue.ToString();

                    // If the string is empty and the property doesn't exist, don't create it unnecessarily
                    if(propValue.Length != 0 || this.ProjectMgr.BuildProject.GetProperty(boundProperty) != null)
                    {
                        if(this.IsEscapedProperty(boundProperty))
                            propValue = EscapeValueAttribute.Escape(propValue);

                       this.ProjectMgr.SetProjectProperty(boundProperty, propValue);
                    }
                }
            }
        }
        #endregion

        #region IPropertyPage Members
        //=====================================================================

        /// <summary>
        /// Called when the environment wants us to create our property page.
        /// </summary>
        /// <param name="hWndParent">The HWND of the parent window.</param>
        /// <param name="pRect">The bounds of the area that we should fill.</param>
        /// <param name="bModal">Indicates whether the dialog box is shown modally or not.</param>
        void IPropertyPage.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            this.CreateControl();
            this.Initialize();

            NativeMethods.SetParent(this.Handle, hWndParent);
            ((IPropertyPage)this).Move(pRect);

            this.BindProperties(this.Controls);
        }

        /// <summary>
        /// Applies the changes made on the property page to the bound objects.
        /// </summary>
        /// <returns><b>S_OK</b> if the changes were successfully applied and the property page is current
        /// with the bound objects or <b>S_FALSE</b> if the changes were applied, but the property page cannot
        /// determine if its state is current with the objects.</returns>
        int IPropertyPage.Apply()
        {
            if(this.IsDirty)
            {
                if(this.ProjectMgr == null)
                {
                    Debug.Assert(false);
                    return VSConstants.E_INVALIDARG;
                }

                if(this.IsValid)
                {
                    this.StoreProperties(this.Controls);
                    this.IsDirty = false;
                }
                else
                    return VSConstants.S_FALSE;
            }

            return VSConstants.S_OK;
        }

        /// <summary>
        /// The environment calls this to notify us that we should clean up our resources.
        /// </summary>
        void IPropertyPage.Deactivate()
        {
            this.Dispose();
        }

        /// <summary>
        /// The environment calls this to get the parameters to describe the property page.
        /// </summary>
        /// <param name="pPageInfo">The parameters are returned in this one-sized array.</param>
        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            PROPPAGEINFO info = new PROPPAGEINFO();

            info.cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO));
            info.dwHelpContext = 0;
            info.pszDocString = info.pszHelpFile = null;
            info.pszTitle = this.Title ?? String.Empty;
            info.SIZE.cx = this.Width;
            info.SIZE.cy = this.Height;
            pPageInfo[0] = info;
        }

        /// <summary>
        /// Invokes the property page help in response to an end-user request.
        /// </summary>
        /// <param name="pszHelpDir">String under the HelpDir key in the property page's CLSID information in the
        /// registry.  If HelpDir does not exist, this will be the path found in the InProcServer32 entry minus
        /// the server filename.</param>
        void IPropertyPage.Help(string pszHelpDir)
        {
            // This is never called
        }

        /// <summary>
        /// Indicates whether this property page has changed its state since the last call to
        /// <see cref="IPropertyPage.Apply"/>. The property sheet uses this information to enable
        /// or disable the Apply button in the dialog box.
        /// </summary>
        /// <returns>Returns <b>S_OK</b> if the value state of the property page is dirty, that is, it has
        /// changed and is different from the state of the bound objects.  Returns <b>S_FALSE</b> if the value
        /// state of the page has not changed and is current with that of the bound objects.</returns>
        int IPropertyPage.IsPageDirty()
        {
            return (this.IsDirty ? VSConstants.S_OK : VSConstants.S_FALSE);
        }

        /// <summary>
        /// Repositions and resizes the property page dialog box according to the contents of
        /// <paramref name="pRect"/>. The rectangle specified by <paramref name="pRect"/> is
        /// treated identically to that passed to <see cref="IPropertyPage.Activate"/>.
        /// </summary>
        /// <param name="pRect">The bounds of the area that we should fill.</param>
        void IPropertyPage.Move(RECT[] pRect)
        {
            RECT r = pRect[0];

            // Don't resize smaller than the minimum size if defined
            this.Bounds = new Rectangle(r.left, r.top,
                Math.Max(r.right - r.left, this.MinimumSize.Width),
                Math.Max(r.bottom - r.top, this.MinimumSize.Height));
        }

        /// <summary>
        /// The environment calls this to set the currently selected objects that the property page should show.
        /// </summary>
        /// <param name="cObjects">The count of elements in <paramref name="ppunk"/>.</param>
        /// <param name="ppunk">An array of <b>IUnknown</b> objects to show in the property page.</param>
        /// <remarks>We are supposed to cache these objects until we get another call with
        /// <paramref name="cObjects"/> = 0.  Also, the environment is supposed to call this before calling
        /// <see cref="IPropertyPage.Activate"/>, but don't count on it.</remarks>
        void IPropertyPage.SetObjects(uint cObjects, object[] ppunk)
        {
            if(cObjects == 0)
            {
                this.ProjectMgr = null;
                return;
            }

            if(ppunk[0] is ProjectConfig)
            {
                List<ProjectConfig> configs = new List<ProjectConfig>();

                for(int i = 0; i < cObjects; i++)
                {
                    ProjectConfig config = (ProjectConfig)ppunk[i];

                    if(this.ProjectMgr == null)
                        this.ProjectMgr = config.ProjectMgr;

                    configs.Add(config);
                }

                this.ProjectConfigs = new ReadOnlyCollection<ProjectConfig>(configs);
            }
            else
                if(ppunk[0] is NodeProperties)
                {
                    if(this.ProjectMgr == null)
                        this.ProjectMgr = (ppunk[0] as NodeProperties).Node.ProjectMgr;

                    Dictionary<string, ProjectConfig> configsMap = new Dictionary<string, ProjectConfig>();

                    for(int i = 0; i < cObjects; i++)
                    {
                        NodeProperties property = (NodeProperties)ppunk[i];
                        IVsCfgProvider provider;
                        ErrorHandler.ThrowOnFailure(property.Node.ProjectMgr.GetCfgProvider(out provider));
                        uint[] expected = new uint[1];
                        ErrorHandler.ThrowOnFailure(provider.GetCfgs(0, null, expected, null));

                        if(expected[0] > 0)
                        {
                            ProjectConfig[] configs = new ProjectConfig[expected[0]];
                            uint[] actual = new uint[1];
                            provider.GetCfgs(expected[0], configs, actual, null);

                            foreach(ProjectConfig config in configs)
                                if(!configsMap.ContainsKey(config.ConfigName))
                                    configsMap.Add(config.ConfigName, config);
                        }
                    }

                    if(configsMap.Count > 0)
                        this.ProjectConfigs = new ReadOnlyCollection<ProjectConfig>(configsMap.Values.ToArray());
                }

            if(!this.IsDisposed && this.ProjectMgr != null)
            {
                this.BindProperties(this.Controls);
                this.IsDirty = false;
            }
        }

        /// <summary>
        /// Initializes a property page and provides the property page object with the
        /// <see cref="IPropertyPageSite"/> interface through which the property page communicates with the
        /// property frame.
        /// </summary>
        /// <param name="pPageSite">The <see cref="IPropertyPageSite"/> that manages and provides services to this
        /// property page within the entire property sheet.</param>
        void IPropertyPage.SetPageSite(IPropertyPageSite pPageSite)
        {
            this.PropertyPageSite = pPageSite;
        }

        /// <summary>
        /// Makes the property page dialog box visible or invisible according to the <paramref name="nCmdShow"/>
        /// parameter. If the page is made visible, the page should set the focus to itself, specifically to the
        /// first property on the page.
        /// </summary>
        /// <param name="nCmdShow">Command describing whether to become visible (SW_SHOW or SW_SHOWNORMAL) or
        /// hidden (SW_HIDE). No other values are valid for this parameter.</param>
        void IPropertyPage.Show(uint nCmdShow)
        {
            if(nCmdShow == 0 /*SW_HIDE*/)
                this.Visible = false;
            else
            {
                this.Visible = true;
                this.Show();
            }
        }

        /// <summary>
        /// Instructs the property page to process the keystroke described in <paramref name="pMsg"/>.
        /// </summary>
        /// <param name="pMsg">Describes the keystroke to process.</param>
        /// <returns>
        /// <list type="table">
        /// <item><term>S_OK</term><description>The property page handles the accelerator.</description></item>
        /// <item><term>S_FALSE</term><description>The property page handles accelerators, but this one was not
        /// useful to it.</description></item>
        /// <item><term>E_NOTIMPL</term><description>The property page does not handle accelerators.</description></item>
        /// </list>
        /// </returns>
        int IPropertyPage.TranslateAccelerator(MSG[] pMsg)
        {
            MSG msg = pMsg[0];

            // This never gets called so we process mnemonics and F1 in ProcessDialogKey() instead.  This seems
            // to be caused by a problem with the way Visual Studio handles keyboard input.  Tool windows suffer
            // from the same problem.

            if((msg.message < NativeMethods.WM_KEYFIRST || msg.message > NativeMethods.WM_KEYLAST) &&
              (msg.message < NativeMethods.WM_MOUSEFIRST || msg.message > NativeMethods.WM_MOUSELAST))
                return VSConstants.S_FALSE;

            return (NativeMethods.IsDialogMessageA(this.Handle, ref msg)) ? VSConstants.S_OK : VSConstants.S_FALSE;
        }
        #endregion

        #region IPropertyPage2 Members
        //=====================================================================

        /// <inheritdoc />
        void IPropertyPage2.Activate(IntPtr hWndParent, RECT[] pRect, int bModal)
        {
            ((IPropertyPage)this).Activate(hWndParent, pRect, bModal);
        }

        /// <inheritdoc />
        void IPropertyPage2.Apply()
        {
            ((IPropertyPage)this).Apply();
        }

        /// <inheritdoc />
        void IPropertyPage2.Deactivate()
        {
            ((IPropertyPage)this).Deactivate();
        }

        /// <inheritdoc />
        void IPropertyPage2.EditProperty(int DISPID)
        {
            // Not used
        }

        /// <inheritdoc />
        void IPropertyPage2.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            ((IPropertyPage)this).GetPageInfo(pPageInfo);
        }

        /// <inheritdoc />
        void IPropertyPage2.Help(string pszHelpDir)
        {
            ((IPropertyPage)this).Help(pszHelpDir);
        }

        /// <inheritdoc />
        int IPropertyPage2.IsPageDirty()
        {
            return ((IPropertyPage)this).IsPageDirty();
        }

        /// <inheritdoc />
        void IPropertyPage2.Move(RECT[] pRect)
        {
            ((IPropertyPage)this).Move(pRect);
        }

        /// <inheritdoc />
        void IPropertyPage2.SetObjects(uint cObjects, object[] ppunk)
        {
            ((IPropertyPage)this).SetObjects(cObjects, ppunk);
        }

        /// <inheritdoc />
        void IPropertyPage2.SetPageSite(IPropertyPageSite pPageSite)
        {
            ((IPropertyPage)this).SetPageSite(pPageSite);
        }

        /// <inheritdoc />
        void IPropertyPage2.Show(uint nCmdShow)
        {
            ((IPropertyPage)this).Show(nCmdShow);
        }

        /// <inheritdoc />
        int IPropertyPage2.TranslateAccelerator(MSG[] pMsg)
        {
            return ((IPropertyPage)this).TranslateAccelerator(pMsg);
        }
        #endregion
    }
}
