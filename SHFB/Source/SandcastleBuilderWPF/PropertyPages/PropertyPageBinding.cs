//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : PropertyPageBinding.cs
// Author  : Eric Woodruff
// Updated : 04/17/2021
// Note    : Copyright 2017-2021, Eric Woodruff, All rights reserved
//
// This file contains the WPF-specific binding code for the property page class
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 10/05/2017  EFW  Created the code
//===============================================================================================================

using System;
using System.Windows;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This class contains the property name binding attached property
    /// </summary>
    public static class PropertyPageBinding
    {
        /// <summary>
        /// This defines the <see cref="P:SandcastleBuilder.WPF.PropertyPages.PropertyPageBinding.ProjectPropertyName"/>
        /// attached property.
        /// </summary>
        /// <AttachedPropertyComments>
        /// <summary>
        /// This attached property is used to define the MSBuild project property to which a WPF control will be
        /// bound in the property page editor control.
        /// /// </summary>
        /// <value>The default value is null</value>
        /// </AttachedPropertyComments>
        public static readonly DependencyProperty ProjectPropertyNameProperty = DependencyProperty.RegisterAttached(
          "ProjectPropertyName", typeof(string), typeof(PropertyPageBinding));

        /// <summary>
        /// Get the property value
        /// </summary>
        /// <param name="element">The element from which to get the value</param>
        /// <returns>The property value</returns>
        [AttachedPropertyBrowsableForType(typeof(FrameworkElement))]
        public static string GetProjectPropertyName(UIElement element)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            return (string)element.GetValue(ProjectPropertyNameProperty);
        }

        /// <summary>
        /// Set the property value
        /// </summary>
        /// <param name="element">The element on which to set the property</param>
        /// <param name="value">The property value</param>
        public static void SetProjectPropertyName(UIElement element, string value)
        {
            if(element == null)
                throw new ArgumentNullException(nameof(element));

            element.SetValue(ProjectPropertyNameProperty, value);
        }
    }
}
