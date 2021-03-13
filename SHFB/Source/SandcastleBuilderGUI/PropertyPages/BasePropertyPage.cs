//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BasePropertyPage.cs
// Author  : Eric Woodruff
// Updated : 03/11/2021
// Note    : Copyright 2012-2021, Eric Woodruff, All rights reserved
//
// This user control is used as the base class for standalone GUI property pages
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/28/2012  EFW  Created the code
// 10/06/2017  EFW  Reworked to use WPF controls in an ElementHost for better scaling on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.Integration;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.WPF;
using SandcastleBuilder.WPF.PropertyPages;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used as a base class for standalone GUI property pages
    /// </summary>
    /// <remarks>This control handles the common tasks of a property page such as binding controls to project
    /// properties and storing the values when they change.  The property "binding" is done by specifying the
    /// project property name using the <see cref="P:SandcastleBuilder.WPF.PropertyPages.PropertyPageBinding.ProjectPropertyName"/>
    /// attached property on the necessary WPF controls.</remarks>
    [ToolboxItem(false)]
    public partial class BasePropertyPage : System.Windows.Forms.UserControl
    {
        #region Private data members
        //=====================================================================

        // This is used to define custom controls and their value property
        private static readonly Dictionary<string, string> customControls = new Dictionary<string, string>();

        private bool isDirty;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property can be overridden to provide custom validation
        /// for the property page.
        /// </summary>
        /// <value>Return true if valid, or false if not</value>
        protected virtual bool IsValid => true;

        /// <summary>
        /// This is used to get or set the title of the property page
        /// </summary>
        public string Title { get; set; }

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
        public bool IsDirty
        {
            get => isDirty;
            set
            {
                if(isDirty != value && !this.IsBinding)
                {
                    isDirty = value;

                    if(this.CurrentProject != null && isDirty)
                        this.OnDirtyChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// This read-only property returns the project manager for the property page
        /// </summary>
        protected SandcastleProject CurrentProject { get; private set; }

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
        ///     "SandcastleBuilder.WPF.PropertyPages.FilePathUserControl", "PersistablePath");
        /// </code>
        /// </example>
        public static Dictionary<string, string> CustomControls => customControls;

        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when the dirty property changes
        /// </summary>
        public event EventHandler DirtyChanged;

        /// <summary>
        /// This raises the <see cref="DirtyChanged"/> event
        /// </summary>
        /// <param name="e">The event arguments</param>
        protected void OnDirtyChanged(EventArgs e)
        {
            DirtyChanged?.Invoke(this, e);
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

            SetStyle(System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
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
                this.IsDirty = true;
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
                Utility.ShowHelpTopic(this.HelpKeyword);
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

            if(this.CurrentProject != null)
            {
                var projProp = this.CurrentProject.MSBuildProject.GetProperty(boundProperty);

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
                        if(propValue.Length != 0 || this.CurrentProject.MSBuildProject.GetProperty(boundProperty) != null)
                        {
                            if(this.IsEscapedProperty(boundProperty))
                                propValue = EscapeValueAttribute.Escape(propValue);

                            this.CurrentProject.MSBuildProject.SetProperty(boundProperty, propValue);
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
            this.OnPropertyChanged(sender, e);
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

        /// <summary>
        /// The environment calls this to set the currently selected project that the property page should show.
        /// </summary>
        /// <param name="currentProject">The current help file builder project</param>
        public void SetProject(SandcastleProject currentProject)
        {
            if(this.CurrentProject != currentProject)
            {
                this.CurrentProject = currentProject;

                if(!this.IsDisposed)
                {
                    if(this.CurrentProject != null)
                    {
                        this.Initialize();
                        this.BindProperties(this.Controls);
                    }

                    this.IsDirty = false;
                }
            }
        }

        /// <summary>
        /// Applies the changes made on the property page to the bound objects
        /// </summary>
        /// <returns>True if the changes were successfully applied and the property page is current with the
        /// bound objects or false if the changes were applied, but the property page cannot determine if its
        /// state is current with the objects.</returns>
        public bool Apply()
        {
            if(this.IsDirty)
            {
                if(this.CurrentProject == null)
                {
                    Debug.Assert(false);
                    return false;
                }

                if(this.IsValid)
                {
                    this.StoreProperties(this.Controls);
                    this.IsDirty = false;
                    this.CurrentProject.RefreshProjectProperties();
                }
                else
                    return false;
            }

            return true;
        }
        #endregion
    }
}
