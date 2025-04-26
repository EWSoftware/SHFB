//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BasePropertyPage.cs
// Author  : Eric Woodruff
// Updated : 05/10/2021
// Note    : Copyright 2011-2021, Eric Woodruff, All rights reserved
//
// This user control is used as the base class for package property pages
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/27/2011  EFW  Created the code
// 10/06/2017  EFW  Reworked to use WPF controls in an ElementHost for better scaling on 4K displays
//===============================================================================================================

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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.Integration;
using WinFormsKeys = System.Windows.Forms.Keys;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Project;
using Microsoft.VisualStudio.Shell.Interop;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.Utils.Design;

using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used as a base class for package property pages
    /// </summary>
    /// <remarks>This control handles the common tasks of a property page such as binding controls to project
    /// properties and storing the values when they change.  The property "binding" is done by specifying the
    /// project property name using the <see cref="P:SandcastleBuilder.WPF.PropertyPages.PropertyPageBinding.ProjectPropertyName"/>
    /// attached property on the necessary WPF controls.</remarks>
    [ComVisible(true), ToolboxItem(false)]
    public partial class BasePropertyPage : System.Windows.Forms.UserControl, IPropertyPage2
    {
        #region Private data members
        //=====================================================================

        // This is used to define custom controls and their value property
        private static readonly Dictionary<string, string> customControls = new Dictionary<string, string>();

        // This is used to track active property pages
        private static readonly List<BasePropertyPage> propertyPages = new List<BasePropertyPage>();

        private bool isDirty;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get a list of all active property pages
        /// </summary>
        /// <remarks>The <see cref="BuildCompletedEventListener"/> uses this to flush pending changes to property
        /// pages prior to a build occurring.  Typically, this happens automatically but it does not if the build
        /// is invoked using the context menu on the project node.  The build event listener is used to
        /// workaround this issue and ensure the project is current before the build takes place.</remarks>
        internal static List<BasePropertyPage> AllPropertyPages => propertyPages;

        /// <summary>
        /// This read-only property can be overridden to provide custom validation for the property page
        /// </summary>
        /// <value>Return true if valid, or false if not</value>
        protected virtual bool IsValid => true;

        /// <summary>
        /// This is used to get or set the title of the property page
        /// </summary>
        protected string Title { get; set; }

        /// <summary>
        /// This is used to get or set an optional help keyword associated with the property page
        /// </summary>
        protected string HelpKeyword { get; set; }

        /// <summary>
        /// This is used to indicate whether or not binding is occurring
        /// </summary>
        /// <value>If true, binding is occurring and property changed events will not be raised</value>
        protected bool IsBinding { get; set; }

        /// <summary>
        /// This is used to get or set the dirty state of the property page
        /// </summary>
        protected internal bool IsDirty
        {
            get => isDirty;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if(isDirty != value && !this.IsBinding)
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
        /// <remarks>The class recognizes the basic edit controls and those derived from them.  If you have
        /// custom controls that it does not recognize (i.e. those derived from <b>UserControl</b>), you can
        /// add them to this dictionary using the fully qualified type name including namespaces and the property
        /// value to use for binding as the value for the entry.
        ///
        /// <para><b>NOTE:</b>You only need to add controls to this property if it is not derived from one of the
        /// standard edit controls (see <see cref="BindProperties"/> for more information).</para>
        ///
        /// <para>This property is static so it can be populated once at start-up for use throughout the
        /// application's lifetime.</para></remarks>
        /// <example>
        /// <code language="cs">
        /// // Use the PeristablePath property for binding in controls of type
        /// // SandcastleBuilder.WPF.PropertyPages.FilePathUserControl.  This control is
        /// // not derived from a standard control so we need to add it manually.
        /// BasePropertyPage.CustomControls.Add(
        ///     "Sandcastle.Platform.Windows.UserControls.FilePathUserControl", "PersistablePath");
        /// </code>
        /// </example>
        public static Dictionary<string, string> CustomControls => customControls;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public BasePropertyPage()
        {
            InitializeComponent();
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
        /// <remarks>This appears to be necessary because the <c>IPropertyPage2.TranslateAccelerator</c> method
        /// is never called.  As such, Visual Studio invokes help on F1 rather than us getting a
        /// <c>HelpRequested</c> event and it invokes menus if a mnemonic matches a menu hot key rather than
        /// focusing the associated control.</remarks>
        protected override bool ProcessDialogKey(WinFormsKeys keyData)
        {
            if(keyData == WinFormsKeys.F1)
            {
                if(this.ShowHelp())
                    return true;
            }
            else
                if((keyData & ~WinFormsKeys.KeyCode) == WinFormsKeys.Alt)
                {
                    WinFormsKeys key = (keyData & ~WinFormsKeys.Alt);

                    if(((Char.IsLetterOrDigit((char)((ushort)key)) && (key < WinFormsKeys.F1 || key > WinFormsKeys.F24)) ||
                      (key >= WinFormsKeys.NumPad0 && key <= WinFormsKeys.Divide)) && this.ProcessMnemonic((char)((ushort)key)))
                          return true;
                }

            return base.ProcessDialogKey(keyData);
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// This can be overridden to perform custom initialization for the property page
        /// </summary>
        protected virtual void Initialize()
        {
        }

        /// <summary>
        /// Derived classes can connect the change event on custom controls to this method to notify the page of
        /// changes in the control's value.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        protected void OnPropertyChanged(object sender, EventArgs e)
        {
            if(!this.IsBinding)
            {
#pragma warning disable VSTHRD010
                this.IsDirty = true;
#pragma warning restore VSTHRD010
                this.RefreshControlState(sender, e);
            }
        }

        /// <summary>
        /// This can be overridden to specify whether the property value is escaped or not.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <returns>True if the property contains an escaped value, false if not.  When bound, an escaped
        /// property's value is unescaped before it is assigned to the control.  When stored, the new value is
        /// escaped before it is stored.</returns>
        protected virtual bool IsEscapedProperty(string propertyName)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to bind a control to a property in a manner other than the default handling
        /// supplied by the base class.
        /// </summary>
        /// <param name="propertyName">The name of the property to bind</param>
        /// <returns>True if the method bound the control or it should be ignored, false if the base class should
        /// attempt to bind it in the default manner.</returns>
        protected virtual bool BindControlValue(string propertyName)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to store a control value in a property in a manner other than the default
        /// handling supplied by the base class.
        /// </summary>
        /// <param name="propertyName">The name of the property to store</param>
        /// <returns>True if the method stored the control value or it should be ignored, false if the base class
        /// should attempt to store the value in the default manner.</returns>
        protected virtual bool StoreControlValue(string propertyName)
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
        public virtual bool ShowHelp()
        {
            if(!String.IsNullOrEmpty(this.HelpKeyword))
            {
                UiUtility.ShowHelpTopic(this.HelpKeyword);
                return true;
            }

            return false;
        }

        /// <summary>
        /// This is used to bind the controls in the given collection to their associated project properties
        /// </summary>
        /// <param name="controls">The control collection from which to get the bound controls.  WPF controls and
        /// their children in an <see cref="ElementHost"/> are bound if they declare a property name using the
        /// <see cref="P:SandcastleBuilder.WPF.PropertyPages.PropertyPageBinding.ProjectPropertyName"/> attached
        /// property.</param>
        protected void BindProperties(System.Windows.Forms.Control.ControlCollection controls)
        {
            Type t;
            PropertyInfo pi;
            string typeName, boundProperty;

            try
            {
                this.IsBinding = true;

                foreach(var control in controls.OfType<ElementHost>().Select(h => (FrameworkElement)h.Child))
                {
                    foreach(var c in control.AllChildElements().Where(c =>
                      !String.IsNullOrWhiteSpace(PropertyPageBinding.GetProjectPropertyName(c))))
                    {
                        t = c.GetType();
                        typeName = t.FullName;
                        boundProperty = PropertyPageBinding.GetProjectPropertyName(c);

                        // Check for custom types first
                        if(customControls.ContainsKey(typeName))
                        {
                            // Find and connect the Changed event for the named property if one exists
                            var changedEvent = t.GetEvents().Where(ev =>
                                ev.Name == customControls[typeName] + "Changed").FirstOrDefault();

                            if(changedEvent != null)
                            {
                                Delegate h;

                                if(changedEvent.EventHandlerType == typeof(RoutedPropertyChangedEventHandler<object>))
                                {
                                    h = new RoutedPropertyChangedEventHandler<object>(OnWpfPropertyChanged);
                                }
                                else
                                    h = new EventHandler(OnPropertyChanged);

                                changedEvent.RemoveEventHandler(c, h);
                                changedEvent.AddEventHandler(c, h);
                            }

                            pi = t.GetProperty(customControls[typeName], BindingFlags.Public | BindingFlags.Instance);
                        }
                        else if(c is Label)
                        {
                            // No change event for this one but we probably don't need it
                            pi = t.GetProperty("Content", BindingFlags.Public | BindingFlags.Instance);
                        }
                        else if(c is TextBoxBase tb)
                        {
                            tb.TextChanged -= OnPropertyChanged;
                            tb.TextChanged += OnPropertyChanged;

                            pi = t.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                        }
                        else if(c is Selector sel)
                        {
                            sel.SelectionChanged -= OnPropertyChanged;
                            sel.SelectionChanged += OnPropertyChanged;

                            pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                        }
                        else if(c is CheckBox cb)
                        {
                            cb.Click -= OnPropertyChanged;
                            cb.Click += OnPropertyChanged;

                            pi = t.GetProperty("IsChecked", BindingFlags.Public | BindingFlags.Instance);
                        }
                        else
                            pi = null;

                        // Give the user a chance to handle the control in a custom fashion.  If not handled and
                        // we couldn't figure out what to use, ignore it.
                        if(!this.BindControlValue(boundProperty) && pi != null)
                            this.Bind(c, pi, boundProperty);
                    }
                }
            }
            finally
            {
                this.IsBinding = false;
            }
        }

        /// <summary>
        /// Bind the control to the property value by setting the property to the current value from the project
        /// </summary>
        /// <param name="control">The control to bind</param>
        /// <param name="propertyInfo">The property information</param>
        /// <param name="boundProperty">The project property name</param>
        private void Bind(object control, PropertyInfo propertyInfo, string boundProperty)
        {
            string propValue = null;
            object controlValue;

            if(this.ProjectMgr != null)
            {
                var projProp = this.ProjectMgr.BuildProject.GetProperty(boundProperty);

                if(projProp != null)
                {
                    // A recent change to Microsoft.Common.targets overrides our OutputPath property with a copy
                    // containing a function call that ensures a trailing backslash as the unevaluated value.  If
                    // we see an imported property with a predecessor, use the predecessor to get our unevaluated
                    // value.
                    while(projProp.IsImported && projProp.Predecessor != null)
                        projProp = projProp.Predecessor;

                    propValue = projProp.UnevaluatedValue;
                }

                // If null, the property probably doesn't exist so ignore it
                if(propValue == null)
                    return;

                if(this.IsEscapedProperty(boundProperty))
                    propValue = EscapeValueAttribute.Unescape(propValue);

                TypeCode typeCode = Type.GetTypeCode(propertyInfo.PropertyType);

                // If it's something like a nullable type, get the type parameter type code
                if(typeCode == TypeCode.Object && propertyInfo.PropertyType.GenericTypeArguments.Length != 0)
                    typeCode = Type.GetTypeCode(propertyInfo.PropertyType.GenericTypeArguments[0]);

                // Set the value based on the type
                switch(typeCode)
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
        /// This is used to store the control values in the given collection to their associated project
        /// properties.
        /// </summary>
        /// <param name="controls">The control collection from which to get the bound controls.  WPF controls and
        /// their children in an <see cref="ElementHost"/> are bound if they declare a property name using the
        /// <see cref="P:SandcastleBuilder.WPF.PropertyPages.PropertyPageBinding.ProjectPropertyName"/> attached
        /// property.</param>
        protected void StoreProperties(System.Windows.Forms.Control.ControlCollection controls)
        {
            Type t;
            PropertyInfo pi;
            object controlValue;
            string typeName, boundProperty, propValue;

            foreach(var control in controls.OfType<ElementHost>().Select(h => (FrameworkElement)h.Child))
            {
                foreach(var c in control.AllChildElements().Where(c =>
                  !String.IsNullOrWhiteSpace(PropertyPageBinding.GetProjectPropertyName(c))))
                {
                    t = c.GetType();
                    typeName = t.FullName;
                    boundProperty = PropertyPageBinding.GetProjectPropertyName(c);

                    // Check for custom types first
                    if(customControls.ContainsKey(typeName))
                        pi = t.GetProperty(customControls[typeName], BindingFlags.Public | BindingFlags.Instance);
                    else if(c is TextBoxBase)
                        pi = t.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                    else if(c is Selector)
                        pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                    else if(c is CheckBox)
                        pi = t.GetProperty("IsChecked", BindingFlags.Public | BindingFlags.Instance);
                    else
                        pi = null;

                    // Give the user a chance to handle the control in a custom fashion.  If not handled and we
                    // couldn't figure out what to use, ignore it.
                    if(!this.StoreControlValue(boundProperty) && pi != null)
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
        }

        /// <summary>
        /// This handles property changed events for certain WPF custom controls
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void OnWpfPropertyChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
#pragma warning disable VSTHRD010
            this.OnPropertyChanged(sender, e);
#pragma warning restore VSTHRD010
        }

        /// <summary>
        /// This is used to determine the minimum size of the property page based on the content pane
        /// </summary>
        /// <param name="c">The content pane control</param>
        /// <returns>The minimum size of the property page</returns>
        /// <remarks>Even though the WPF control will be scaled correctly, the containing host control does not
        /// always have an appropriate minimum size.  As such, the host control is created with a smaller size
        /// than needed and this is called to set the minimum size on the host control based on the minimum size
        /// reported by the child WPF control.  The WPF control's minimum size is converted to pixels based on
        /// the current system's DPI.</remarks>
        protected static System.Drawing.Size DetermineMinimumSize(System.Windows.Controls.Control c)
        {
            double pixelWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / System.Windows.SystemParameters.WorkArea.Width,
                pixelHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / System.Windows.SystemParameters.WorkArea.Height;

            return new System.Drawing.Size((int)(c.MinWidth * pixelWidth), (int)(c.MinHeight * pixelHeight));
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
            ThreadHelper.ThrowIfNotOnUIThread();

            this.CreateControl();
            this.Initialize();

            // Add the property page to the tracking list
            propertyPages.Add(this);

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
#pragma warning disable VSTHRD010
                    this.IsDirty = false;
#pragma warning restore VSTHRD010
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
            if(!this.IsDisposed)
            {
                // Remove the property page from the tracking list
                propertyPages.Remove(this);

                this.Dispose();
            }
        }

        /// <summary>
        /// The environment calls this to get the parameters to describe the property page.
        /// </summary>
        /// <param name="pPageInfo">The parameters are returned in this one-sized array.</param>
        void IPropertyPage.GetPageInfo(PROPPAGEINFO[] pPageInfo)
        {
            pPageInfo[0] = new PROPPAGEINFO
            {
                cb = (uint)Marshal.SizeOf(typeof(PROPPAGEINFO)),
                dwHelpContext = 0,
                pszDocString = null,
                pszHelpFile = null,
                pszTitle = this.Title ?? String.Empty,
                SIZE = new SIZE { cx = this.Width, cy = this.Height }
            };
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
            this.Bounds = new Rectangle(r.left, r.top, Math.Max(r.right - r.left, this.MinimumSize.Width),
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
            ThreadHelper.ThrowIfNotOnUIThread();

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
                        ErrorHandler.ThrowOnFailure(property.Node.ProjectMgr.GetCfgProvider(out IVsCfgProvider provider));
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
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).Activate(hWndParent, pRect, bModal);
        }

        /// <inheritdoc />
        void IPropertyPage2.Apply()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).Apply();
        }

        /// <inheritdoc />
        void IPropertyPage2.Deactivate()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

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
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).GetPageInfo(pPageInfo);
        }

        /// <inheritdoc />
        void IPropertyPage2.Help(string pszHelpDir)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).Help(pszHelpDir);
        }

        /// <inheritdoc />
        int IPropertyPage2.IsPageDirty()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return ((IPropertyPage)this).IsPageDirty();
        }

        /// <inheritdoc />
        void IPropertyPage2.Move(RECT[] pRect)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).Move(pRect);
        }

        /// <inheritdoc />
        void IPropertyPage2.SetObjects(uint cObjects, object[] ppunk)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).SetObjects(cObjects, ppunk);
        }

        /// <inheritdoc />
        void IPropertyPage2.SetPageSite(IPropertyPageSite pPageSite)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).SetPageSite(pPageSite);
        }

        /// <inheritdoc />
        void IPropertyPage2.Show(uint nCmdShow)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ((IPropertyPage)this).Show(nCmdShow);
        }

        /// <inheritdoc />
        int IPropertyPage2.TranslateAccelerator(MSG[] pMsg)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            return ((IPropertyPage)this).TranslateAccelerator(pMsg);
        }
        #endregion
    }
}
