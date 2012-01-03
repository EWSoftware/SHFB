//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : SplitButton.cs
// Author  : Huy Pham
// Updated : 12/28/2011
// Source  : http://huydinhpham.blogspot.com/2008/09/wpf-drop-down-and-split-button.html
// Note    : Copyright 2008-2011, Huy Pham, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that implements a split button
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.9.3.3  12/04/2011  EFW  Added the code to the project
// 1.9.3.3  12/19/2011  EFW  Added support for MainButtonCommandParameter
// 1.9.3.3  12/28/2011  EFW  Added support for MainButtonCommandTarget
//=============================================================================

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SandcastleBuilder.WPF.Controls
{
    /// <summary>
    /// A split button control that allows a button to have a default action and a dropdown menu
    /// </summary>
    /// <example>
    /// <para><b>NOTE:</b> This control's resources must be included in the application, window, or user control
    /// resources.  For example:</para>
    /// 
    /// <code lang="xml">
    /// &lt;Application.Resources&gt;
    ///   &lt;ResourceDictionary Source="/SandcastleBuilder.WPF;component/Controls/SplitButtonStyle.xaml" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    /// </example>
    [TemplatePart(Name = "PART_Button", Type = typeof(ButtonBase))]
    public class SplitButton : ToggleButton
    {
        #region Dependency Properties

        /// <summary>
        /// The dropdown context menu dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register("DropDownContextMenu", typeof(ContextMenu), typeof(SplitButton), new UIPropertyMetadata(null));

        /// <summary>
        /// The image dependency property
        /// </summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
            typeof(ImageSource), typeof(SplitButton));

        /// <summary>
        /// The text dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string), typeof(SplitButton));

        /// <summary>
        /// The target dependency property
        /// </summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target",
            typeof(UIElement), typeof(SplitButton));

        /// <summary>
        /// The main button command dependency property
        /// </summary>
        public static readonly DependencyProperty MainButtonCommandProperty = DependencyProperty.Register(
            "MainButtonCommand", typeof(ICommand), typeof(SplitButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The main button command target dependency property
        /// </summary>
        public static readonly DependencyProperty MainButtonCommandTargetProperty = DependencyProperty.Register(
            "MainButtonCommandTarget", typeof(IInputElement), typeof(SplitButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The main button command parameter dependency property
        /// </summary>
        public static readonly DependencyProperty MainButtonCommandParameterProperty = DependencyProperty.Register(
            "MainButtonCommandParameter", typeof(object), typeof(SplitButton), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// The dropdown button command dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownButtonCommandProperty = DependencyProperty.Register(
            "DropDownButtonCommand", typeof(ICommand), typeof(SplitButton), new FrameworkPropertyMetadata(null));

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public SplitButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
            var binding = new Binding("DropDownContextMenu.IsOpen") { Source = this };
            SetBinding(IsCheckedProperty, binding);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The dropdown context menu property
        /// </summary>
        public ContextMenu DropDownContextMenu
        {
            get { return GetValue(DropDownContextMenuProperty) as ContextMenu; }
            set { SetValue(DropDownContextMenuProperty, value); }
        }

        /// <summary>
        /// The image property
        /// </summary>
        public ImageSource Image
        {
            get { return GetValue(ImageProperty) as ImageSource; }
            set { SetValue(ImageProperty, value); }
        }

        /// <summary>
        /// The text property
        /// </summary>
        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// The target property
        /// </summary>
        public UIElement Target
        {
            get { return GetValue(TargetProperty) as UIElement; }
            set { SetValue(TargetProperty, value); }
        }

        /// <summary>
        /// The main button command property
        /// </summary>
        public ICommand MainButtonCommand
        {
            get { return GetValue(MainButtonCommandProperty) as ICommand; }
            set { SetValue(MainButtonCommandProperty, value); }
        }

        /// <summary>
        /// The main button command target
        /// </summary>
        public IInputElement MainButtonCommandTarget
        {
            get { return GetValue(MainButtonCommandTargetProperty) as IInputElement; }
            set { SetValue(MainButtonCommandTargetProperty, value); }
        }

        /// <summary>
        /// The main button command parameter property
        /// </summary>
        public object MainButtonCommandParameter
        {
            get { return GetValue(MainButtonCommandParameterProperty); }
            set { SetValue(MainButtonCommandParameterProperty, value); }
        }

        /// <summary>
        /// The dropdown button command
        /// </summary>
        public ICommand DropDownButtonCommand
        {
            get { return GetValue(DropDownButtonCommandProperty) as ICommand; }
            set { SetValue(DropDownButtonCommandProperty, value); }
        }

        #endregion

        #region Public Override Methods

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetMainButtonCommand();
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc />
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(e.Property == MainButtonCommandProperty || e.Property == MainButtonCommandTargetProperty ||
              e.Property == MainButtonCommandParameterProperty)
                SetMainButtonCommand();

            if(e.Property == DropDownButtonCommandProperty)
                Command = DropDownButtonCommand;
        }

        /// <inheritdoc />
        protected override void OnClick()
        {
            if(DropDownContextMenu == null)
                return;

            if(DropDownButtonCommand != null)
                DropDownButtonCommand.Execute(null);

            // If there is a drop-down assigned to this button, then position and display it 
            DropDownContextMenu.PlacementTarget = this;
            DropDownContextMenu.Placement = PlacementMode.Bottom;
            DropDownContextMenu.IsOpen = !DropDownContextMenu.IsOpen;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Set the main button's command
        /// </summary>
        private void SetMainButtonCommand()
        {
            // Set up the event handlers
            if(Template != null)
            {
                var button = Template.FindName("PART_Button", this) as ButtonBase;

                if(button != null)
                {
                    button.Command = MainButtonCommand;
                    button.CommandTarget = MainButtonCommandTarget;
                    button.CommandParameter = MainButtonCommandParameter;
                }
            }
        }

        #endregion
    }
}
