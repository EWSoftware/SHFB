//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : BasePropertyPage.cs
// Author  : Eric Woodruff
// Updated : 01/09/2013
// Note    : Copyright 2012-2013, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This user control is used as the base class for standalone GUI property pages
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com. This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.9.6.0  10/28/2012  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Package.PropertyPages
{
    /// <summary>
    /// This is used as a base class for standalone GUI property pages
    /// </summary>
    /// <remarks>This control handles the common tasks of a property page such as binding controls to project
    /// properties and storing the values when they change.  The property "binding" is done by specifying the
    /// project property name in the control's <c>Tag</c> property.</remarks>
    [ToolboxItem(false)]
    public partial class BasePropertyPage : UserControl
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
        public string Title { get; set; }

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
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                if(isDirty != value && !isBinding)
                {
                    isDirty = value;

                    if(this.CurrentProject != null && isDirty && !this.CurrentProject.IsDirty)
                        this.CurrentProject.MarkAsDirty();
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
        /// custom controls that it does not recognize (i.e. those derived from <b>UserControl</b>), you can add
        /// them to this dictionary using the fully qualified type name including namespaces and the property
        /// value to use for binding as the value for the entry.
        /// <p/><b>NOTE:</b>You only need to add controls to this property if it is not derived from one of the
        /// standard edit controls (see  <see cref="BindProperties"/> for more information).
        /// <p/>This property is static so it can be populated once at start-up for use throughout the
        /// application's lifetime.</remarks>
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
        /// <remarks>The class recognizes the basic panel and container controls but will ignore all
        /// <c>UserControl</c> derived objects.  If you have a user control that you want included in the
        /// binding procedure, add its full type name to this collection.
        /// <p/><b>NOTE:</b>You only need to add type names to this property if it is not derived from one of
        /// the container controls (see <see cref="BindProperties"/> for more information).
        /// <p/>This property is static so it can be populated once at start-up for use throughout the
        /// application's lifetime.</remarks>
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
        /// <param name="control">The control to bind</param>
        /// <returns>True if the method bound the control or it should be ignored, false if the base class
        /// should attempt to bind it in the default manner.</returns>
        protected virtual bool BindControlValue(Control control)
        {
            return false;
        }

        /// <summary>
        /// This can be overridden to store a control value in a property in a manner other than the default
        /// handling supplied by the base class.
        /// </summary>
        /// <param name="control">The control from which to store the value</param>
        /// <returns>True if the method stored the control value or it should be ignored, false if the base
        /// class should attempt to store the value in the default manner.</returns>
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
        /// This is used to bind the controls in the given collection to their associated project properties.
        /// </summary>
        /// <param name="controls">The control collection from which to get the bound controls.  Controls are
        /// bound if their <see cref="Control.Tag"/> property is a string that matches a project property.</param>
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
        /// Bind the control to the property value by setting the property to the current value from the project
        /// </summary>
        /// <param name="control">The control to bind</param>
        /// <param name="propertyInfo">The property information</param>
        /// <param name="boundProperty">The project property name</param>
        private void Bind(Control control, PropertyInfo propertyInfo, string boundProperty)
        {
            string propValue = null;
            object controlValue;

            if(this.CurrentProject != null)
            {
                var projProp = this.CurrentProject.MSBuildProject.GetProperty(boundProperty);

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
                        controlValue = (propValue ?? String.Empty);
                        break;

                    case TypeCode.Char:
                        controlValue = (propValue != null) ? propValue[0] : '\x0';
                        break;

                    case TypeCode.Byte:
                        controlValue = (propValue != null) ? Convert.ToByte(propValue[0]) : 0;
                        break;

                    case TypeCode.SByte:
                        controlValue = (propValue != null) ? Convert.ToSByte(propValue[0]) : 0;
                        break;

                    case TypeCode.Decimal:
                        controlValue = (propValue != null) ? Convert.ToDecimal(propValue,
                            CultureInfo.CurrentCulture) : 0m;
                        break;

                    case TypeCode.Double:
                        controlValue = (propValue != null) ? Convert.ToDouble(propValue,
                            CultureInfo.CurrentCulture) : 0d;
                        break;

                    case TypeCode.Single:
                        controlValue = (propValue != null) ? Convert.ToSingle(propValue,
                            CultureInfo.CurrentCulture) : 0f;
                        break;

                    case TypeCode.Int16:
                        controlValue = (propValue != null) ? Convert.ToInt16(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.Int32:
                        controlValue = (propValue != null) ? Convert.ToInt32(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.Int64:
                        controlValue = (propValue != null) ? Convert.ToInt64(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.UInt16:
                        controlValue = (propValue != null) ? Convert.ToUInt16(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.UInt32:
                        controlValue = (propValue != null) ? Convert.ToUInt32(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.UInt64:
                        controlValue = (propValue != null) ? Convert.ToUInt64(propValue,
                            CultureInfo.CurrentCulture) : 0;
                        break;

                    case TypeCode.Boolean:
                        controlValue = (propValue != null) ? Convert.ToBoolean(propValue,
                            CultureInfo.CurrentCulture) : false;
                        break;

                    case TypeCode.DateTime:
                        controlValue = (propValue != null) ? Convert.ToDateTime(propValue,
                            CultureInfo.CurrentCulture) : DateTime.Today;
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
        /// <param name="controls">The control collection from which to get the values.  Controls are bound if
        /// their <see cref="Control.Tag"/> property is a string that matches a project property.</param>
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
                {
                    pi = t.GetProperty("Text", BindingFlags.Public | BindingFlags.Instance);
                }
                else if(c is ComboBox)
                {
                    if(((ComboBox)c).DataSource != null)
                        pi = t.GetProperty("SelectedValue", BindingFlags.Public | BindingFlags.Instance);
                    else
                        pi = t.GetProperty("SelectedItem", BindingFlags.Public | BindingFlags.Instance);
                }
                else if(c is CheckBox)
                {
                    pi = t.GetProperty("Checked", BindingFlags.Public | BindingFlags.Instance);
                }
                else if((c is DateTimePicker) || (c is UpDownBase) || (c is TrackBar))
                {
                    pi = t.GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);
                }
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
                    if(propValue.Length != 0 || this.CurrentProject.MSBuildProject.GetProperty(boundProperty) != null)
                    {
                        if(this.IsEscapedProperty(boundProperty))
                            propValue = EscapeValueAttribute.Escape(propValue);

                       this.CurrentProject.MSBuildProject.SetProperty(boundProperty, propValue);
                    }
                }
            }
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
