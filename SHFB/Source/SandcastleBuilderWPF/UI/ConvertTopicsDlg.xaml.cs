//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ConvertTopicsDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/05/2025
// Note    : Copyright 2025, Eric Woodruff, All rights reserved
//
// This file contains a form used to select options for the MAML to Markdown topic conversion
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and ca be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/26/2025  EFW  Created the code
//===============================================================================================================

using System.Windows;

namespace SandcastleBuilder.WPF.UI;

/// <summary>
/// This form is used to select options for the MAML to Markdown topic conversion
/// </summary>
public partial class ConvertTopicsDlg : Window
{
    #region Properties
    //=====================================================================

    /// <summary>
    /// This is used to get or set whether or not to use filenames for the unique IDs rather than the topic
    /// GUIDs.
    /// </summary>
    public bool UseFilenames => chkUseFilenames.IsChecked.Value;

    /// <summary>
    /// This is used to get or set whether or not to only convert the selected topic
    /// </summary>
    public bool SelectedTopicOnly => rbSelectedTopicOnly.IsChecked.Value;

    /// <summary>
    /// This is used to get or set whether or not to only convert the selected topic and its children recursively
    /// </summary>
    public bool SelectedTopicAndChildren => rbSelectedTopicAndChildren.IsChecked.Value;

    /// <summary>
    /// This is used to get or set whether or not to only convert the selected topic and its siblings
    /// </summary>
    public bool SelectedTopicAndSiblings => rbSelectedTopicAndSiblings.IsChecked.Value;

    /// <summary>
    /// This is used to get or set whether or not to only convert all topics
    /// </summary>
    public bool AllTopics => rbAllTopics.IsChecked.Value;

    #endregion

    #region Constructor
    //=====================================================================

    /// <summary>
    /// Conversion
    /// </summary>
    public ConvertTopicsDlg()
    {
        InitializeComponent();
    }
    #endregion

    #region Event handlers
    //=====================================================================

    /// <summary>
    /// View help for this form
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void btnHelp_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("!!TODO: Implement this");
    }

    /// <summary>
    /// Close the form and use the selection(s)
    /// </summary>
    /// <param name="sender">The sender of the event</param>
    /// <param name="e">The event arguments</param>
    private void btnOK_Click(object sender, RoutedEventArgs e)
    {
        this.DialogResult = true;
    }
    #endregion
}
