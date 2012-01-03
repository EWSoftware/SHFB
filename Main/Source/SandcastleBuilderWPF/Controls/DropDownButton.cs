//=============================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : DropDownButton.cs
// Author  : Huy Pham
// Updated : 12/04/2011
// Source  : http://huydinhpham.blogspot.com/2008/09/wpf-drop-down-and-split-button.html
// Note    : Copyright 2008-2011, Huy Pham, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that implements a drop down button
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
    /// A button control that has a dropdown menu
    /// </summary>
    /// <example>
    /// <para><b>NOTE:</b> This control's resources must be included in the application, window, or user control
    /// resources.  For example:</para>
    /// 
    /// <code lang="xml">
    /// &lt;Application.Resources&gt;
    ///   &lt;ResourceDictionary Source="/SandcastleBuilder.WPF;component/Controls/DropDownButtonStyle.xaml" /&gt;
    /// &lt;/Application.Resources&gt;
    /// </code>
    /// </example>
    public class DropDownButton : ToggleButton
    {
        #region Dependency Properties

        /// <summary>
        /// The dropdown context menu dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownContextMenuProperty = DependencyProperty.Register("DropDownContextMenu", typeof(ContextMenu), typeof(DropDownButton), new UIPropertyMetadata(null));
        
        /// <summary>
        /// The image dependency property
        /// </summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(ImageSource), typeof(DropDownButton));
        
        /// <summary>
        /// The text dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DropDownButton));
        
        /// <summary>
        /// The target dependency property
        /// </summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target", typeof(UIElement), typeof(DropDownButton));
        
        /// <summary>
        /// The drop down button command dependency property
        /// </summary>
        public static readonly DependencyProperty DropDownButtonCommandProperty = DependencyProperty.Register("DropDownButtonCommand", typeof(ICommand), typeof(DropDownButton), new FrameworkPropertyMetadata(null));
       
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public DropDownButton()
        {
            // Bind the ToogleButton.IsChecked property to the drop-down's IsOpen property 
            var binding = new Binding("DropDownContextMenu.IsOpen") {Source = this};         
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
        /// The dropdown button command property
        /// </summary>
        public ICommand DropDownButtonCommand
        {
            get { return GetValue(DropDownButtonCommandProperty) as ICommand; }
            set { SetValue(DropDownButtonCommandProperty, value); }
        }

        #endregion

        #region Protected Override Methods

        /// <inheritdoc />
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == DropDownButtonCommandProperty)
                Command = DropDownButtonCommand;
        }

        /// <inheritdoc />
        protected override void OnClick()
        {
            if (DropDownContextMenu == null) return;

            if (DropDownButtonCommand != null) DropDownButtonCommand.Execute(null);

            // If there is a drop-down assigned to this button, then position and display it 
            DropDownContextMenu.PlacementTarget = this;
            DropDownContextMenu.Placement = PlacementMode.Bottom;
            DropDownContextMenu.IsOpen = !DropDownContextMenu.IsOpen;
        }

        #endregion
    }
}