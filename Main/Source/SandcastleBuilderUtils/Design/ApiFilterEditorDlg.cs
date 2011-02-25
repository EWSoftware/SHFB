//=============================================================================
// System  : EWSoftware Design Time Attributes and Editors
// File    : ApiFilterEditorDlg.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 01/09/2011
// Note    : Copyright 2007-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the form used to edit the API filter items.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.1.0  07/20/2007  EFW  Created the code
// 1.6.0.4  01/17/2008  EFW  Made adjustments to support changes and fixes in
//                           the Sandcastle namespace ripping feature.
// 1.8.0.0  07/08/2008  EFW  Reworked to support MSBuild project format
//=============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using SandcastleBuilder.Utils.BuildEngine;
using SandcastleBuilder.Utils.XPath;

namespace SandcastleBuilder.Utils.Design
{
    /// <summary>
    /// This form is used to edit the API filter collection
    /// </summary>
    internal partial class ApiFilterEditorDlg : Form
    {
        #region API visibility enumeration
        //=====================================================================

        /// <summary>
        /// This is used to indicate the visibility of a member in the node
        /// information type below.
        /// </summary>
        private enum ApiVisibility
        {
            /// <summary>The member is public</summary>
            Public,
            /// <summary>The member is protected</summary>
            Protected,
            /// <summary>The member is internal (Friend)</summary>
            Internal,
            /// <summary>The member is private</summary>
            Private
        }
        #endregion

        #region Tree node information class
        //=====================================================================

        /// <summary>
        /// This is used to associate additional information with each tree
        /// node to make it easier to look stuff up.
        /// </summary>
        private sealed class NodeInfo
        {
            #region Private data members
            //=================================================================

            private string nodeText, id, filterName;
            private XmlNode apiNode;
            private ApiEntryType entryType;
            private ApiVisibility visibility;
            private bool isProjectExclude;
            #endregion

            #region Properties
            //=================================================================

            /// <summary>
            /// Get or set the text to display in the tree view
            /// </summary>
            /// <value>This will be the full namespace name or the type
            /// or member name without the namespace.</value>
            public string NodeText
            {
                get { return nodeText; }
                set
                {
                    int pos;

                    nodeText = value;

                    // Replace certain values to make it more readable
                    if(nodeText.Length == 0)
                        nodeText = "(global)";
                    else
                        if(nodeText == "#cctor")
                            nodeText = "Static Constructor";
                        else
                            if(nodeText == "#ctor")
                                nodeText = "Constructor";
                            else
                            {
                                if(nodeText.IndexOf('#') != -1)
                                    nodeText = nodeText.Replace('#', '.');

                                if(nodeText[nodeText.Length - 1] == '.')
                                    nodeText = nodeText.Substring(0,
                                        nodeText.Length - 1);

                                pos = nodeText.IndexOf('`');

                                if(pos != -1)
                                    nodeText = nodeText.Substring(0, pos);

                                if(nodeText.StartsWith("op_",
                                  StringComparison.Ordinal))
                                    nodeText = nodeText.Substring(3) +
                                        " Operator";
                            }
                }
            }

            /// <summary>
            /// This read-only property is used to get the API ID (the
            /// namespace, type, or member name).
            /// </summary>
            public string Id
            {
                get { return id; }
            }

            /// <summary>
            /// For types, this will be set to the filter name to use if the
            /// class is nested within another class.
            /// </summary>
            /// <remarks>In such cases, the parent class name(s) must prefix
            /// the type so that it can be correctly excluded or included.</remarks>
            public string FilterName
            {
                get { return filterName; }
                set { filterName = value; }
            }

            /// <summary>
            /// Get or set the API node from the reflection information file
            /// </summary>
            /// <remarks>This will also set the <see cref="EntryType"/>
            /// based on the reflection information in the node.</remarks>
            public XmlNode ApiNode
            {
                get { return apiNode; }
                set
                {
                    apiNode = value;

                    if(apiNode != null)
                    {
                        this.DetermineApiEntryType();
                        this.AppendTemplatesToNodeText();
                    }
                    else
                        entryType = ApiEntryType.Class;
                }
            }

            /// <summary>
            /// This read-only property is used to get the API entry type for
            /// this node.
            /// </summary>
            public ApiEntryType EntryType
            {
                get { return entryType; }
            }

            /// <summary>
            /// This read-only property is used to get the visibility of this
            /// node.
            /// </summary>
            public ApiVisibility Visibility
            {
                get { return visibility; }
            }

            /// <summary>
            /// Get or set whether or not the entry is a project exclude
            /// </summary>
            /// <remarks>If excluded via the Namespace Comments project option
            /// or an <code>&lt;exclude /&gt;</code> tag, this property will
            /// be set to true and the node cannot be marked as exposed.  It
            /// is also used to disallow changes to some of the fixed and
            /// inherited API entry nodes.</remarks>
            public bool IsProjectExclude
            {
                get { return isProjectExclude; }
                set { isProjectExclude = value; }
            }
            #endregion

            #region Constructor and helper methods
            //=================================================================

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="text">The text for the tree view</param>
            /// <param name="apiId">The API ID</param>
            public NodeInfo(string text, string apiId)
            {
                this.NodeText = text;
                id = apiId;
            }

            /// <summary>
            /// This will determine the API entry type and visibility based on
            /// the information in the reflection information node.
            /// </summary>
            private void DetermineApiEntryType()
            {
                XmlNode subsubgroup;
                string subgroup, id;

                // Is it an inherited namespace?
                if(apiNode.Name == "namespace")
                {
                    entryType = ApiEntryType.Namespace;
                    return;
                }

                // Is it an inherited type?
                if(apiNode.Name == "type")
                {
                    // Assume class, it'll be close enough
                    entryType = ApiEntryType.Class;
                    return;
                }
                
                // It's a documented or inherited member of some sort
                if(apiNode.Name == "element" || apiNode.Name == "type")
                    id = apiNode.Attributes["api"].Value;   // Inherited
                else
                {
                    id = apiNode.Attributes["id"].Value;    // Documented

                    // Is it a namespace?
                    if(id[0] == 'N')
                    {
                        entryType = ApiEntryType.Namespace;
                        return;
                    }
                }

                subsubgroup = apiNode.SelectSingleNode("apidata/@subsubgroup");

                if(subsubgroup != null)
                    subgroup = subsubgroup.Value;
                else
                    subgroup = apiNode.SelectSingleNode("apidata/@subgroup").Value;

                entryType = EntryTypeFromId(id[0], subgroup);
                visibility = DetermineVisibility(id[0], apiNode);
            }

            /// <summary>
            /// If this API node contains template information, add that
            /// info to the node's display text.
            /// </summary>
            private void AppendTemplatesToNodeText()
            {
                XmlNodeList templates;
                StringBuilder sb = new StringBuilder(100);
                int idx = 1;

                if(apiNode.Name == "api")
                    templates = apiNode.SelectNodes("templates/template");
                else
                {
                    templates = apiNode.SelectNodes("specialization/template");

                    if(templates.Count == 0)
                        templates = apiNode.SelectNodes("specialization/type");
                }

                foreach(XmlNode template in templates)
                {
                    if(sb.Length != 0)
                        sb.Append(',');

                    if(template.Name != "type")
                        sb.Append(template.Attributes["name"].Value);
                    else
                    {
                        // For specializations of types, we don't want to
                        // show the type but a generic place holder.
                        sb.Append('T');

                        if(idx > 1)
                            sb.Append(idx);

                        idx++;
                    }
                }

                if(sb.Length != 0)
                {
                    sb.Insert(0, "<");
                    sb.Append('>');
                    nodeText += sb.ToString();
                }
            }

            /// <summary>
            /// Determine the API entry type from the ID and possible the
            /// subgroup.
            /// </summary>
            /// <param name="apiType">The type character to convert</param>
            /// <param name="subgroup">The subgroup to use</param>
            /// <returns>An <see cref="ApiEntryType"/> indicating the entry
            /// type.</returns>
            internal static ApiEntryType EntryTypeFromId(char apiType,
              string subgroup)
            {
                ApiEntryType entryType;

                switch(apiType)
                {
                    case 'N':   // Namespace
                        entryType = ApiEntryType.Namespace;
                        break;

                    case 'T':   // A type
                        switch(subgroup)
                        {
                            case "structure":
                                entryType = ApiEntryType.Structure;
                                break;

                            case "interface":
                                entryType = ApiEntryType.Interface;
                                break;

                            case "enumeration":
                                entryType = ApiEntryType.Enumeration;
                                break;

                            case "delegate":
                                entryType = ApiEntryType.Delegate;
                                break;

                            default:
                                entryType = ApiEntryType.Class;
                                break;
                        }
                        break;

                    default:    // Must be a member of some sort
                        switch(subgroup)
                        {
                            case "constructor":
                                entryType = ApiEntryType.Constructor;
                                break;

                            case "operator":
                                entryType = ApiEntryType.Operator;
                                break;

                            case "property":
                                entryType = ApiEntryType.Property;
                                break;

                            case "event":
                                entryType = ApiEntryType.Event;
                                break;

                            case "field":
                                entryType = ApiEntryType.Field;
                                break;

                            default:
                                entryType = ApiEntryType.Method;
                                break;
                        }
                        break;
                }

                return entryType;
            }

            /// <summary>
            /// Determine the visibility from the API node information
            /// </summary>
            /// <param name="apiType">The API type character from the ID</param>
            /// <param name="node">The API node information</param>
            /// <returns>An <see cref="ApiVisibility"/> indicating the entry's
            /// visibility.</returns>
            internal static ApiVisibility DetermineVisibility(char apiType,
              XmlNode node)
            {
                ApiVisibility visibility;
                string visText;

                // Determine the visibility of the entry
                if(apiType == 'T')
                    visText = node.SelectSingleNode(
                        "typedata/@visibility").Value;
                else
                    visText = node.SelectSingleNode(
                        "memberdata/@visibility").Value;

                switch(visText)
                {
                    case "private":
                        visibility = ApiVisibility.Private;
                        break;

                    case "assembly":
                    case "family and assembly":
                        visibility = ApiVisibility.Internal;
                        break;

                    case "family":
                    case "family or assembly":
                        visibility = ApiVisibility.Protected;
                        break;

                    default:
                        visibility = ApiVisibility.Public;
                        break;
                }

                return visibility;
            }
            #endregion
        }
        #endregion

        #region Private data members
        //=====================================================================

        // The dialog's instance members
        private Font italicFont;
        private ApiFilterCollection apiFilter;
        private XmlDocument reflectionInfo;
        private XPathNavigator navigator;
        private XmlNode apisNode;
        private bool wasModified, changingCheckState;

        // Project-related members
        private SandcastleProject tempProject;
        private string reflectionFile;
        private Dictionary<string, ApiFilter> buildFilterEntries;

        private Thread buildThread;
        private BuildProcess buildProcess;
        #endregion

        #region Build methods
        //=====================================================================

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with the current build step.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildStepChanged(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down or it hasn't
                // completed yet.
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildStepChanged),
                        new object[] { sender, e });
            }
            else
            {
                lblProgress.Text = e.BuildStep.ToString();

                if(e.HasCompleted)
                {
                    reflectionFile = buildProcess.ReflectionInfoFilename;

                    // Restore the current project's base path
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(
                        apiFilter.Project.Filename));

                    // If successful, load the namespace nodes, and enable
                    // the UI.
                    if(e.BuildStep == BuildStep.Completed)
                    {
                        // Convert the build API filter to a dictionary to make
                        // it easier to find entries.
                        buildFilterEntries = new Dictionary<string, ApiFilter>();
                        this.ConvertApiFilter(buildProcess.BuildApiFilter);

                        this.LoadNamespaces();
                        tvApiList.Enabled = splitContainer.Panel2.Enabled =
                            btnReset.Enabled = true;
                    }

                    pbWait.Visible = lblLoading.Visible = false;

                    buildThread = null;
                    buildProcess = null;
                }
            }
        }

        /// <summary>
        /// This is called by the build process thread to update the main
        /// window with information about its progress.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void buildProcess_BuildProgress(object sender,
          BuildProgressEventArgs e)
        {
            if(this.InvokeRequired)
            {
                // Ignore it if we've already shut down
                if(!this.IsDisposed)
                    this.Invoke(new EventHandler<BuildProgressEventArgs>(
                        buildProcess_BuildProgress),
                        new object[] { sender, e });
            }
            else
            {
                if(e.BuildStep == BuildStep.Failed)
                {
                    MessageBox.Show("Unable to build project to obtain " +
                        "API information.  Please perform a normal build " +
                        "to identify and correct the problem.",
                        Constants.AppName, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Tree view loading methods
        /// <summary>
        /// This is used to convert the API filter from the build into a
        /// dictionary so that it is easier to look up the entries.
        /// </summary>
        /// <param name="filter">The filter collection to search for project
        /// level exclusions.</param>
        private void ConvertApiFilter(ApiFilterCollection filter)
        {
            foreach(ApiFilter entry in filter)
            {
#if DEBUG
                // This shouldn't happen.  If it does, there's a problem
                // in generating the API filter in the build process.
                if(buildFilterEntries.ContainsKey(entry.FullName))
                    System.Diagnostics.Debugger.Break();
#endif
                // Sometimes, it does happen for unknown reasons.  Ignore it
                // so that it doesn't prevent the filter from opening.
                if(!buildFilterEntries.ContainsKey(entry.FullName))
                    buildFilterEntries.Add(entry.FullName, entry);

                if(entry.Children.Count != 0)
                    this.ConvertApiFilter(entry.Children);
            }
        }

        /// <summary>
        /// This is used to load the reflection information file and to load
        /// the root namespace nodes into the tree view.
        /// </summary>
        /// <remarks>The namespace nodes and type nodes are loaded on demand
        /// to reduce the time needed to create the tree view and to conserve
        /// some memory for extremely large builds.
        /// <p/>Documented APIs are loaded into the first root node.  Inherited
        /// APIs are loaded into the second node.  By splitting the inherited
        /// stuff out, we can optimze the API filter and allow the user to get
        /// rid of unwanted inherited members with a single selection.</remarks>
        private void LoadNamespaces()
        {
            TreeNode node, rootNode = tvApiList.Nodes[0];
            NodeInfo nodeInfo;
            ApiFilter filter;
            List<NodeInfo> nodeList = new List<NodeInfo>();
            HashSet<string> existingIds = new HashSet<string>();
            string fullName;

            reflectionInfo = new XmlDocument();
            reflectionInfo.Load(reflectionFile);

            // Get the root APIs node as that's all we'll ever search
            apisNode = reflectionInfo.SelectSingleNode("reflection/apis");
            navigator = apisNode.CreateNavigator();

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading namespaces...";
                Application.DoEvents();
                tvApiList.BeginUpdate();

                // Build a set of nodes to store the necessary information and
                // sort it.
                foreach(XmlNode nsNode in apisNode.SelectNodes(
                    "api[starts-with(@id, 'N:')]"))
                {
                    nodeInfo = new NodeInfo(
                        nsNode.Attributes["id"].Value.Substring(2),
                        nsNode.Attributes["id"].Value.Substring(2));
                    nodeInfo.ApiNode = nsNode;

                    nodeList.Add(nodeInfo);
                }

                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        return String.Compare(x.NodeText, y.NodeText,
                            StringComparison.CurrentCulture);
                    });

                // Load the tree view with the namespaces for documented APIs
                // as children of the first root node.
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the namespace name and we'll store a
                    // reference to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Tag = ni;
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ApiEntryType.Namespace;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                    {
                        node.Checked = true;

                        // Add a placeholder node for expansion on demand
                        node.Nodes.Add(String.Empty);
                    }
                    else
                    {
                        if(filter.IsProjectExclude && !filter.IsExposed)
                        {
                            ni.IsProjectExclude = true;
                            node.Checked = false;
                            node.NodeFont = italicFont;
                            node.ToolTipText = "Excluded via namespace " +
                                "comments or an <exclude /> tag.";
                        }
                        else
                            node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;

                        // Add children now as it contains filtered entries and
                        // we'll need to keep them in synch with the parent.
                        this.AddTypes(node);
                    }

                    rootNode.Nodes.Add(node);
                }

                // Load inherited namespaces found in dependent assemblies and
                // the .NET Framework itself.
                lblProgress.Text = "Loading inherited namespaces...";
                Application.DoEvents();

                nodeList.Clear();
                rootNode = tvApiList.Nodes[1];

                // Build a set of nodes to store the necessary information and
                // sort it.
                foreach(XmlNode nsNode in apisNode.SelectNodes(
                  "api/elements/element/containers/namespace"))
                {
                    fullName = nsNode.Attributes["api"].Value.Substring(2);

                    // Ignore overloads as noted above
                    if(existingIds.Add(fullName))
                    {
                        nodeInfo = new NodeInfo(fullName, fullName);
                        nodeInfo.ApiNode = nsNode;
                        nodeInfo.IsProjectExclude = true;
                        nodeList.Add(nodeInfo);
                    }
                }

                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        return String.Compare(x.NodeText, y.NodeText,
                            StringComparison.CurrentCulture);
                    });

                // Load the tree view with the namespaces of inherited APIs as
                // children of the second root node.
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the namespace name and we'll store a
                    // reference to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Tag = ni;
                    node.NodeFont = italicFont;
                    node.ToolTipText = "Namespace contains inherited types";
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ApiEntryType.Namespace;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                    {
                        node.Checked = true;

                        // Add a placeholder node for expansion on demand
                        node.Nodes.Add(String.Empty);
                    }
                    else
                    {
                        node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;

                        // Add children now as it contains filtered entries and
                        // we'll need to keep them in synch with the parent.
                        this.AddBaseTypes(node);
                    }

                    rootNode.Nodes.Add(node);
                }

                if(tvApiList.Nodes[0].Nodes.Count != 0)
                    tvApiList.Nodes[0].Expand();

                if(tvApiList.Nodes[1].Nodes.Count != 0)
                    tvApiList.Nodes[1].Expand();

                tvApiList.SelectedNode = tvApiList.Nodes[0];
            }
            finally
            {
                tvApiList.EndUpdate();
                lblProgress.Text = null;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Add all types found in the specified namespace node to the
        /// specified tree node.
        /// </summary>
        /// <param name="parentNode">The parent tree node to which they are
        /// added.</param>
        private void AddTypes(TreeNode parentNode)
        {
            TreeNode node;
            ApiFilter filter;
            NodeInfo nodeInfo, parentInfo = (NodeInfo)parentNode.Tag;
            List<NodeInfo> nodeList = new List<NodeInfo>();
            string fullName, memberName;
            int nsLen;

            if(parentInfo.NodeText[0] == '(')
                nsLen = 0;      // Global namespace
            else
            {
                nsLen = parentInfo.NodeText.Length;

                // Handle AjaxDoc 1.0 prefix
                if(parentInfo.NodeText[nsLen - 1] != ':')
                    nsLen++;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading types for " + parentNode.Text;
                Application.DoEvents();
                tvApiList.BeginUpdate();

                foreach(XmlNode typeNode in parentInfo.ApiNode.SelectNodes(
                  "elements/element"))
                {
                    // Remove the namespace from the type name
                    fullName = typeNode.Attributes["api"].Value.Substring(2);

                    if(nsLen == 0)
                        memberName = fullName;
                    else
                        memberName = fullName.Substring(nsLen);

                    nodeInfo = new NodeInfo(memberName, fullName);

                    // Convert the element node to its actual API node
                    nodeInfo.ApiNode = apisNode.SelectSingleNode("api[@id='" +
                        typeNode.Attributes["api"].Value + "']");

                    // For nested classes, we'll need the parent class name(s)
                    // for the filter name.
                    if(memberName.IndexOf('.') != -1)
                        nodeInfo.FilterName = memberName;

                    nodeList.Add(nodeInfo);
                }

                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        return String.Compare(x.NodeText, y.NodeText,
                            StringComparison.CurrentCulture);
                    });

                // Load the nodes as children of the selected namespace
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the type name and we'll store a reference
                    // to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Tag = ni;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                    {
                        node.Checked = parentNode.Checked;

                        // Add a placeholder node for expansion on demand except
                        // for enumerations and delegates which never have
                        // children.
                        if(ni.EntryType != ApiEntryType.Enumeration &&
                          ni.EntryType != ApiEntryType.Delegate)
                            node.Nodes.Add(String.Empty);
                    }
                    else
                    {
                        if(filter.IsProjectExclude && !filter.IsExposed)
                        {
                            ni.IsProjectExclude = true;
                            node.Checked = false;
                            node.NodeFont = italicFont;
                            node.ToolTipText = "Removed via an <exclude /> tag.";
                        }
                        else
                            node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;

                        // Add children now as it contains filtered entries and
                        // we'll need to keep them in synch with the parent.
                        if(ni.EntryType != ApiEntryType.Enumeration &&
                          ni.EntryType != ApiEntryType.Delegate)
                            this.AddMembers(node);
                    }

                    // Set the image indices base on the API type and visibility
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ni.EntryType + ((int)ni.Visibility * 11);

                    parentNode.Nodes.Add(node);
                }
            }
            finally
            {
                tvApiList.EndUpdate();
                lblProgress.Text = null;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Add all base types found in the specified namespace node to the
        /// specified tree node.
        /// </summary>
        /// <param name="parentNode">The parent tree node to which they are
        /// added.</param>
        private void AddBaseTypes(TreeNode parentNode)
        {
            TreeNode node;
            ApiFilter filter;
            NodeInfo nodeInfo, parentInfo = (NodeInfo)parentNode.Tag;
            List<NodeInfo> nodeList = new List<NodeInfo>();
            HashSet<string> existingIds = new HashSet<string>();
            string fullName, memberName;
            int nsLen;

            if(parentInfo.NodeText[0] == '(')
                nsLen = 0;      // Global namespace
            else
            {
                nsLen = parentInfo.NodeText.Length;

                // Handle AjaxDoc 1.0 prefix
                if(parentInfo.NodeText[nsLen - 1] != ':')
                    nsLen++;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading inherited types for " +
                    parentNode.Text;
                Application.DoEvents();
                tvApiList.BeginUpdate();

                foreach(XmlNode typeNode in apisNode.SelectNodes(
                  "api/elements/element[containers/namespace/@api='N:" +
                  parentInfo.Id + "']/containers/type"))
                {
                    // Remove the namespace from the type name
                    fullName = typeNode.Attributes["api"].Value.Substring(2);

                    if(existingIds.Add(fullName))
                    {
                        if(nsLen == 0)
                            memberName = fullName;
                        else
                            memberName = fullName.Substring(nsLen);

                        nodeInfo = new NodeInfo(memberName, fullName);
                        nodeInfo.ApiNode = typeNode;
                        nodeList.Add(nodeInfo);
                    }
                }

                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        return String.Compare(x.NodeText, y.NodeText,
                            StringComparison.CurrentCulture);
                    });

                // Load the nodes as children of the selected namespace
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the type name and we'll store a reference
                    // to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Checked = parentNode.Checked;
                    node.Tag = ni;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                    {
                        node.Checked = parentNode.Checked;

                        // Add a placeholder node for expansion on demand.  We
                        // can't tell the difference between API types so assume
                        // it has members.
                        node.Nodes.Add(String.Empty);
                    }
                    else
                    {
                        node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;

                        // Add children now as it contains filtered entries and
                        // we'll need to keep them in synch with the parent.
                        this.AddBaseMembers(node);
                    }

                    // Set the image indices base on the API type
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ni.EntryType + ((int)ni.Visibility * 11);

                    parentNode.Nodes.Add(node);
                }
            }
            finally
            {
                tvApiList.EndUpdate();
                lblProgress.Text = null;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Add all members found in the specified type node to the specified
        /// tree node.
        /// </summary>
        /// <param name="parentNode">The parent tree node to which they are
        /// added.</param>
        private void AddMembers(TreeNode parentNode)
        {
            TreeNode node;
            ApiFilter filter;
            NodeInfo nodeInfo, parentInfo = (NodeInfo)parentNode.Tag;
            List<NodeInfo> nodeList = new List<NodeInfo>();
            HashSet<string> existingIds = new HashSet<string>();
            string fullName, typeName, memberName;
            int pos;

            if(parentInfo.ApiNode == null)
                return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading members for " + parentNode.Text;
                Application.DoEvents();
                tvApiList.BeginUpdate();

                foreach(XmlNode memberNode in parentInfo.ApiNode.SelectNodes(
                  "elements/element"))
                {
                    fullName = memberNode.Attributes["api"].Value.Substring(2);

                    // We'll strip off parameters as the ripping feature only
                    // supports up to a member name.  If one overload is ripped,
                    // they are all ripped.
                    pos = fullName.IndexOf('(');

                    if(pos != -1)
                        fullName = fullName.Substring(0, pos);

                    // Generic method?
                    pos = fullName.IndexOf("``", StringComparison.Ordinal);

                    if(pos != -1)
                        fullName = fullName.Substring(0, pos);

                    // Remove the namespace and type from the member name
                    pos = fullName.LastIndexOf('.');

                    if(pos == -1)
                    {
                        typeName = String.Empty;
                        memberName = fullName;
                    }
                    else
                    {
                        typeName = fullName.Substring(0, pos);
                        memberName = fullName.Substring(pos + 1);
                    }

                    // Only add actual members of this type
                    if(typeName == parentInfo.Id)
                    {
                        // Ignore overloads as noted above
                        if(existingIds.Add(fullName))
                        {
                            nodeInfo = new NodeInfo(memberName, fullName);

                            // Convert the element node to its actual API node
                            nodeInfo.ApiNode = apisNode.SelectSingleNode(
                                "api[@id='" + memberNode.Attributes[
                                "api"].Value + "']");

                            nodeList.Add(nodeInfo);
                        }
                    }
                }

                // Member nodes are sorted by API entry type and then by name
                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        int result = (int)x.EntryType - (int)y.EntryType;

                        if(result == 0)
                            result = String.Compare(x.NodeText, y.NodeText,
                                StringComparison.CurrentCulture);

                        return result;
                    });

                // Load the nodes as children of the selected type
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the member name and we'll store a reference
                    // to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Tag = ni;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                        node.Checked = parentNode.Checked;
                    else
                    {
                        if(filter.IsProjectExclude && !filter.IsExposed)
                        {
                            ni.IsProjectExclude = true;
                            node.Checked = false;
                            node.NodeFont = italicFont;
                            node.ToolTipText = "Removed via an <exclude /> tag.";
                        }
                        else
                            node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;
                    }

                    // Set the image indices base on the API type
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ni.EntryType + ((int)ni.Visibility * 11);

                    parentNode.Nodes.Add(node);
                }
            }
            finally
            {
                tvApiList.EndUpdate();
                lblProgress.Text = null;
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Add all inherited members found in the specified type node to the
        /// specified tree node.
        /// </summary>
        /// <param name="parentNode">The parent tree node to which they are
        /// added.</param>
        private void AddBaseMembers(TreeNode parentNode)
        {
            TreeNode node;
            ApiFilter filter;
            NodeInfo nodeInfo, parentInfo = (NodeInfo)parentNode.Tag;
            List<NodeInfo> nodeList = new List<NodeInfo>();
            HashSet<string> existingIds = new HashSet<string>();
            string fullName, memberName;
            int pos;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblProgress.Text = "Loading inherited members for " +
                    parentNode.Text;
                Application.DoEvents();
                tvApiList.BeginUpdate();

                foreach(XmlNode memberNode in apisNode.SelectNodes(
                  "api/elements/element[containers/type/@api='T:" +
                  parentInfo.Id + "']"))
                {
                    fullName = memberNode.Attributes["api"].Value.Substring(2);

                    // We'll strip off parameters as the ripping feature only
                    // supports up to a member name.  If one overload is ripped,
                    // they are all ripped.
                    pos = fullName.IndexOf('(');

                    if(pos != -1)
                        fullName = fullName.Substring(0, pos);

                    // Generic method?
                    pos = fullName.IndexOf("``", StringComparison.Ordinal);

                    if(pos != -1)
                        fullName = fullName.Substring(0, pos);

                    // Remove the namespace and type from the member name
                    pos = fullName.LastIndexOf('.');

                    if(pos == -1)
                        memberName = fullName;
                    else
                        memberName = fullName.Substring(pos + 1);

                    // Ignore overloads as noted above
                    if(existingIds.Add(fullName))
                    {
                        nodeInfo = new NodeInfo(memberName, fullName);
                        nodeInfo.ApiNode = memberNode;
                        nodeList.Add(nodeInfo);
                    }
                }

                // Member nodes are sorted by API entry type and then by name
                nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        int result = (int)x.EntryType - (int)y.EntryType;

                        if(result == 0)
                            result = String.Compare(x.NodeText, y.NodeText,
                                StringComparison.CurrentCulture);

                        return result;
                    });

                // Load the nodes as children of the selected type
                foreach(NodeInfo ni in nodeList)
                {
                    // The text is the member name and we'll store a reference
                    // to the node info in the tag.
                    node = new TreeNode(ni.NodeText);
                    node.Tag = ni;

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(ni.Id, out filter))
                        node.Checked = parentNode.Checked;
                    else
                    {
                        node.Checked = filter.IsExposed;

                        // Simple tri-state workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            node.BackColor = Color.LightBlue;
                    }

                    // Set the image indices base on the API type
                    node.ImageIndex = node.SelectedImageIndex =
                        (int)ni.EntryType + ((int)ni.Visibility * 11);

                    parentNode.Nodes.Add(node);
                }
            }
            finally
            {
                tvApiList.EndUpdate();
                lblProgress.Text = null;
                this.Cursor = Cursors.Default;
            }
        }
        #endregion

        #region Miscellaneous helper methods
        //=====================================================================

        /// <summary>
        /// Examine the nodes and optimize the state of the filter based on
        /// various conditions.
        /// </summary>
        /// <param name="currentNode">The node in which the checked state
        /// changed.</param>
        /// <param name="setChildren">Set the checked state of all child
        /// nodes to the parent node's state if true</param>
        private void OptimizeCheckedState(TreeNode currentNode, bool setChildren)
        {
            bool checkedState = currentNode.Checked;
            int checkedCount = 0, uncheckedCount = 0;

            // Apply the check state to all children
            if(setChildren && currentNode.Nodes.Count != 0)
            {
                // Reset the background color incase it's a mixed state node
                currentNode.BackColor = SystemColors.Window;

                foreach(TreeNode child in currentNode.Nodes)
                    if(child.Tag != null && !((NodeInfo)child.Tag).IsProjectExclude)
                    {
                        child.Checked = checkedState;

                        // And members if it's a type node.  We only go to
                        // a maximum of two levels so no need for recursion.
                        foreach(TreeNode memberChild in child.Nodes)
                            if(memberChild.Tag != null &&
                              !((NodeInfo)memberChild.Tag).IsProjectExclude)
                                memberChild.Checked = checkedState;
                    }
            }

            // If it's a member or type node, count the number of checked and
            // unchecked nodes.  Skip nodes that can't be changed though.
            if(currentNode.Parent.Parent != null &&
              !((NodeInfo)currentNode.Parent.Tag).IsProjectExclude)
            {
                foreach(TreeNode child in currentNode.Parent.Nodes)
                    if(child.Checked)
                        checkedCount++;
                    else
                        uncheckedCount++;

/* Fixed by the January 2008 release but hang on to the code just in case.
                // BUG WORKAROUND: As of the June 07 CTP, a filter whose parent
                // is false will rip the whole parent, not just the unwanted
                // nodes.  As such, we have to generate a less efficient
                // filter for our classes.  This doesn't affect inherited
                // classes though so they can still use the more efficient
                // filter.
                TreeNode root = currentNode.Parent.Parent;

                while(root.Parent != null)
                    root = root.Parent;

                if(root == tvApiList.Nodes[0])
                    currentNode.Parent.Checked = (checkedCount != 0);
                else
                {*/
                    // Optimize the parent node's state based on the number of
                    // checked and unchecked items.
                    currentNode.Parent.Checked = (checkedCount >= uncheckedCount);

                    // Color mixed nodes so that they stand out.  This is a
                    // quick way of noting their state without implementing a
                    // custom tree control that supports tri-state checkboxes
                    // and reworking the code to support tri-state values.
                    if(checkedCount != 0 && checkedCount < uncheckedCount)
                        currentNode.Parent.BackColor = Color.LightBlue;
                    else
                        currentNode.Parent.BackColor = SystemColors.Window;
//                }

                // Do the same for the parent's parent node
                this.OptimizeCheckedState(currentNode.Parent, false);
            }
        }

        /// <summary>
        /// Search the children of the specified tree node looking for the
        /// given API member name.
        /// </summary>
        /// <param name="memberName">The member name for which to search</param>
        /// <param name="root">The root node to search</param>
        /// <returns>The tree node matching the given ID or null if not found</returns>
        private static TreeNode SearchForMember(string memberName, TreeNode root)
        {
            // Expand the node if necessary
            if(root.Nodes.Count == 1 && root.Nodes[0].Text.Length == 0)
                root.Expand();

            foreach(TreeNode child in root.Nodes)
                if(((NodeInfo)child.Tag).Id == memberName)
                    return child;

            return null;
        }
        #endregion

        #region Methods to generate new API filter
        //=====================================================================

        /// <summary>
        /// This is used to add namespace filters to the API filter
        /// </summary>
        /// <param name="root">The tree node from which to start</param>
        private void AddNamespaceFilter(TreeNode root)
        {
            ApiFilter filter;
            NodeInfo nodeInfo;

            foreach(TreeNode node in root.Nodes)
            {
                if(this.AllChildrenMatchParentCheckState(node.Nodes,
                  node.Checked))
                {
                    // We only need to add a filter in this case if the
                    // namespace is being excluded.
                    if(!node.Checked)
                    {
                        nodeInfo = (NodeInfo)node.Tag;

                        apiFilter.Add(new ApiFilter(ApiEntryType.Namespace,
                            nodeInfo.Id, false));
                    }
                }
                else
                {
                    nodeInfo = (NodeInfo)node.Tag;

                    filter = new ApiFilter(ApiEntryType.Namespace, nodeInfo.Id,
                        node.Checked);
                    apiFilter.Add(filter);

                    // Add child filters that match the opposite state
                    this.AddChildFilter(filter, node.Nodes, !node.Checked);
                }
            }
        }

        /// <summary>
        /// This will add child filter entries for each tree node where the
        /// checked state matches the given state.
        /// </summary>
        /// <param name="filter">The filter to which the entries are added</param>
        /// <param name="nodes">The tree nodes to scan</param>
        /// <param name="state">The check state to match</param>
        private void AddChildFilter(ApiFilter filter, TreeNodeCollection nodes,
          bool state)
        {
            ApiFilter childFilter;
            NodeInfo nodeInfo;
            string parentId;
            int idx;

            foreach(TreeNode node in nodes)
            {
                if(this.AllChildrenMatchParentCheckState(node.Nodes,
                  node.Checked))
                {
                    // We only need to add a filter in this case if the
                    // node state matches the given state or if it's a nested
                    // class with a state different than the parent class.
                    idx = node.Text.LastIndexOf('.');

                    if(idx != -1)
                        parentId = node.Text.Substring(0, idx);
                    else
                        parentId = null;

                    if(node.Checked == state || (parentId != null &&
                      filter.Children.Any(f => f.FilterName == parentId && f.IsExposed != node.Checked)))
                    {
                        nodeInfo = (NodeInfo)node.Tag;
                        childFilter = new ApiFilter(nodeInfo.EntryType,
                            nodeInfo.Id, node.Checked);

                        // Override the filter name if necessary
                        if(!String.IsNullOrEmpty(nodeInfo.FilterName))
                            childFilter.FilterName = nodeInfo.FilterName;

                        filter.Children.Add(childFilter);
                    }
                }
                else
                {
                    nodeInfo = (NodeInfo)node.Tag;
                    childFilter = new ApiFilter(nodeInfo.EntryType,
                        nodeInfo.Id, node.Checked);

                    // Override the filter name if necessary
                    if(!String.IsNullOrEmpty(nodeInfo.FilterName))
                        childFilter.FilterName = nodeInfo.FilterName;

                    filter.Children.Add(childFilter);

                    // Add child filters that match the opposite state of
                    // the parent node.
                    this.AddChildFilter(childFilter, node.Nodes, !node.Checked);
                }
            }
        }

        /// <summary>
        /// This is used to see if the given node and all of its children match
        /// the specified state.
        /// </summary>
        /// <param name="nodes">The nodes to check</param>
        /// <param name="state">The state to match</param>
        /// <returns>True if all children's checked states match the parent
        /// node's checked state, false if not.</returns>
        private bool AllChildrenMatchParentCheckState(TreeNodeCollection nodes,
          bool state)
        {
            foreach(TreeNode child in nodes)
            {
                // Unexpanded placeholders are ignored
                if(child.Text.Length != 0 && child.Checked != state)
                    return false;

                if(child.Nodes.Count != 0)
                    if(!this.AllChildrenMatchParentCheckState(child.Nodes, state))
                        return false;
            }

            return true;
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">The item collection to edit</param>
        internal ApiFilterEditorDlg(ApiFilterCollection filter)
        {
            InitializeComponent();

            // An italic font is used for nodes that cannot have their
            // check state changed.  A tooltip will give further details.
            tvApiList.Nodes[0].NodeFont = tvApiList.Nodes[1].NodeFont =
                italicFont = new Font(this.Font, FontStyle.Italic);

            apiFilter = filter;
        }
        #endregion

        #region General event handlers
        //=====================================================================

        /// <summary>
        /// This is used to start the background build process from which
        /// we will get the information to load the tree view.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ApiFilterEditorDlg_Load(object sender, EventArgs e)
        {
            string tempPath;

            tvApiList.Enabled = splitContainer.Panel2.Enabled = btnReset.Enabled = false;

            try
            {
                // Clone the project for the build and adjust its properties for our needs
                tempProject = new SandcastleProject(apiFilter.Project, true);

                // The temporary project resides in the same folder as the current project (by filename
                // only, it isn't saved) to maintain relative paths.  However, build output is stored
                // in a temporary folder and it keeps the intermediate files.
                tempProject.CleanIntermediates = false;
                tempPath = Path.GetTempFileName();

                File.Delete(tempPath);
                tempPath = Path.Combine(Path.GetDirectoryName(tempPath), "SHFBPartialBuild");

                if(!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                tempProject.OutputPath = tempPath;

                buildProcess = new BuildProcess(tempProject, true);

                // We must suppress the current API filter for this build
                buildProcess.SuppressApiFilter = true;

                buildProcess.BuildStepChanged += buildProcess_BuildStepChanged;
                buildProcess.BuildProgress += buildProcess_BuildProgress;

                buildThread = new Thread(new ThreadStart(buildProcess.Build));
                buildThread.Name = "API fitler partial build thread";
                buildThread.IsBackground = true;
                buildThread.Start();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show("Unable to build project to obtain API information.  Error: " +
                    ex.Message, Constants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Shut down the build process thread and clean up on exit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ApiFilterEditorDlg_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(buildThread != null && buildThread.IsAlive)
            {
                if(MessageBox.Show("A build is currently taking place to " +
                  "obtain API information.  Do you want to abort it and " +
                  "close this form?", Constants.AppName,
                  MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                  DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    this.Cursor = Cursors.WaitCursor;

                    if(buildThread != null)
                        buildThread.Abort();

                    while(buildThread != null && !buildThread.Join(1000))
                        Application.DoEvents();

                    System.Diagnostics.Debug.WriteLine("Thread stopped");
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    buildThread = null;
                    buildProcess = null;
                }
            }

            if(wasModified)
            {
                apiFilter.Clear();

                // Add documented namespace filters
                this.AddNamespaceFilter(tvApiList.Nodes[0]);

                // Add filters for inherited types
                this.AddNamespaceFilter(tvApiList.Nodes[1]);
            }

            if(tempProject != null)
            {
                try
                {
                    // Delete the temporary project's working files
                    if(!String.IsNullOrEmpty(tempProject.OutputPath) &&
                      Directory.Exists(tempProject.OutputPath))
                        Directory.Delete(tempProject.OutputPath, true);
                }
                catch
                {
                    // Eat the exception.  We'll ignore it if the temporary files cannot be deleted.
                }
            }

            GC.Collect(2);
            GC.WaitForPendingFinalizers();
            GC.Collect(2);
        }

        /// <summary>
        /// Refresh the collection and close the form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            try
            {
#if DEBUG
                path += @"\..\..\..\Doc\Help\SandcastleBuilder.chm";
#else
                path += @"\SandcastleBuilder.chm";
#endif
                Form form = new Form();
                form.CreateControl();
                Help.ShowHelp(form, path, HelpNavigator.Topic,
                    "html/7df16a60-f718-4b8f-bfa2-88c42906070c.htm");
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                MessageBox.Show(String.Format(CultureInfo.CurrentCulture,
                    "Unable to open help file '{0}'.  Reason: {1}",
                    path, ex.Message), Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        /// Reset the API filter by clearing its content and closing the form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReset_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to clear the API filter and reset it to its default state?",
              Constants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
              MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                wasModified = false;
                apiFilter.Clear();
                this.Close();
            }
        }
        #endregion

        #region Tree node event handlers
        //=====================================================================

        /// <summary>
        /// This is used to load child tree nodes on demand which speeds up
        /// the initial form load for extremely large projects.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvApiList_BeforeExpand(object sender,
          TreeViewCancelEventArgs e)
        {
            TreeNode firstChild, currentNode = e.Node;
            NodeInfo parentNodeInfo;

            // Ignore the root nodes and nodes without children
            if(currentNode.Parent == null || currentNode.Nodes.Count == 0)
                return;

            firstChild = currentNode.Nodes[0];

            // If it needs expansion, do it
            if(firstChild.Text.Length == 0)
            {
                currentNode.Nodes.Clear();
                parentNodeInfo = (NodeInfo)currentNode.Tag;

                if(parentNodeInfo.EntryType == ApiEntryType.Namespace)
                {
                    if(currentNode.Parent == tvApiList.Nodes[0])
                        this.AddTypes(currentNode);
                    else
                        this.AddBaseTypes(currentNode);
                }
                else
                {
                    if(currentNode.Parent.Parent == tvApiList.Nodes[0])
                        this.AddMembers(currentNode);
                    else
                        this.AddBaseMembers(currentNode);
                }
            }
        }

        /// <summary>
        /// Ignore attempts to uncheck the root nodes, namespace and type nodes
        /// in the inherited APIs, and nodes that are excluded via other project
        /// settings (i.e. namespace comments and <code>&lt;exclude/&gt;</code>
        /// tags.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        /// <remarks>Unchecking a root node would get rid of everything.
        /// Unchecking a namespace or type for an inherited API may
        /// inadvertently get rid of something that is needed.  Project
        /// excludes will be removed regardless of the API filter setting so
        /// must remain excluded.</remarks>
        private void tvApiList_BeforeCheck(object sender, TreeViewCancelEventArgs e)
        {
            if(!changingCheckState)
                e.Cancel = (e.Node.Parent == null ||
                    ((NodeInfo)e.Node.Tag).IsProjectExclude);
        }

        /// <summary>
        /// Optimize the filter when a node checked state changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void tvApiList_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if(!changingCheckState)
            {
                try
                {
                    changingCheckState = true;
                    this.OptimizeCheckedState(e.Node, true);
                    wasModified = true;
                }
                finally
                {
                    changingCheckState = false;
                }
            }
        }
        #endregion

        #region Search-related event handlers
        //=====================================================================

        /// <summary>
        /// Search for members that match the search conditions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnFind_Click(object sender, EventArgs e)
        {
            ApiEntryType entryType;
            ApiVisibility visibility;
            ListViewItem lvi;
            string id, nodeText, subgroup;
            int pos, ignoreCase = Convert.ToInt32(!chkCaseSensitive.Checked),
                fullyQualified = Convert.ToInt32(chkFullyQualified.Checked);
            List<NodeInfo> nodeList;
            NodeInfo newNode;
            XPathNavigator subsubgroup;

            epErrors.Clear();

            if(txtSearchText.Text.Trim().Length == 0)
            {
                epErrors.SetError(txtSearchText,
                    "A search expression is required");
                return;

            }

            try
            {
                Regex reSearch = new Regex(txtSearchText.Text);
            }
            catch(ArgumentException ex)
            {
                epErrors.SetError(txtSearchText, "The search regular " +
                    "expression is not valid: " + ex.Message);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnGoto.Enabled = btnInclude.Enabled = btnExclude.Enabled = false;

                // Use the custom XPath function matches-regex to perform
                // a regular expression search for matching nodes.  The
                // custom XPath function resolve-name is used to convert the
                // API names to their display format for the search.
                XPathFunctionContext context = new XPathFunctionContext();
                XPathExpression expr = navigator.Compile(String.Format(
                    CultureInfo.InvariantCulture, "api[matches-regex(" +
                    "resolve-name(node(), boolean({0})), '{1}', boolean({2}))]",
                    fullyQualified, txtSearchText.Text, ignoreCase));

                // The first search uses the documented APIs
                expr.SetContext(context);
                lvSearchResults.Items.Clear();
                nodeList = new List<NodeInfo>();

                // The results are stored in the list view with each list
                // item's Tag property referencing the matching node.
                foreach(XPathNavigator nav in navigator.Select(expr))
                {
                    id = nav.GetAttribute("id", String.Empty);

                    if(id[0] == 'N')
                    {
                        entryType = ApiEntryType.Namespace;
                        visibility = ApiVisibility.Public;
                        nodeText = id.Substring(2);
                    }
                    else
                    {
                        subsubgroup = nav.SelectSingleNode("apidata/@subsubgroup");

                        if(subsubgroup != null)
                            subgroup = subsubgroup.Value;
                        else
                            subgroup = nav.SelectSingleNode("apidata/@subgroup").Value;

                        entryType = NodeInfo.EntryTypeFromId(id[0], subgroup);
                        visibility = NodeInfo.DetermineVisibility(id[0],
                            ((IHasXmlNode)nav).GetNode());

                        pos = id.IndexOf('(');

                        if(pos != -1)
                            id = id.Substring(0, pos);

                        pos = id.LastIndexOf('.');

                        if(pos != -1)
                            nodeText = id.Substring(pos + 1);
                        else
                            nodeText = id.Substring(2);
                    }

                    // Only include the wanted items
                    if(((CheckBox)pnlOptions.Controls[
                      (int)entryType + 1]).Checked &&
                      ((CheckBox)pnlOptions.Controls[
                      (int)visibility + 14]).Checked)
                    {
                        newNode = new NodeInfo(nodeText, id.Substring(2));
                        newNode.ApiNode = ((IHasXmlNode)nav).GetNode();
                        nodeList.Add(newNode);
                    }
                }

                // Search inherited APIs as well.  Namespaces first...
                expr = navigator.Compile(String.Format(
                    CultureInfo.InvariantCulture,
                    "api/elements/element/containers/namespace[matches-regex(" +
                    "string(@api), '{0}', boolean({1}))]",
                    txtSearchText.Text, ignoreCase));

                expr.SetContext(context);

                foreach(XPathNavigator nav in navigator.Select(expr))
                {
                    id = nav.GetAttribute("api", String.Empty).Substring(2);

                    // Only include the wanted items
                    if(((CheckBox)pnlOptions.Controls[
                      (int)ApiEntryType.Namespace + 1]).Checked)
                    {
                        newNode = new NodeInfo(id, id);
                        newNode.ApiNode = ((IHasXmlNode)nav).GetNode();
                        nodeList.Add(newNode);
                    }
                }

                // ... then types ...
                expr = navigator.Compile(String.Format(
                    CultureInfo.InvariantCulture,
                    "api/elements/element/containers/type[matches-regex(" +
                    "resolve-name(node(), boolean({0})), '{1}', boolean({2}))]",
                    fullyQualified, txtSearchText.Text, ignoreCase));

                expr.SetContext(context);

                foreach(XPathNavigator nav in navigator.Select(expr))
                {
                    id = nav.GetAttribute("api", String.Empty).Substring(2);
                    pos = id.LastIndexOf('.');

                    if(pos != -1)
                        nodeText = id.Substring(pos + 1);
                    else
                        nodeText = id;

                    // Only include the wanted items.  Since we can't tell the
                    // actual API type, we'll assume Class for these.
                    if(((CheckBox)pnlOptions.Controls[
                        (int)ApiEntryType.Class + 1]).Checked)
                    {
                        newNode = new NodeInfo(nodeText, id);
                        newNode.ApiNode = ((IHasXmlNode)nav).GetNode();
                        nodeList.Add(newNode);
                    }
                }

                // ... and then members.
                expr = navigator.Compile(String.Format(
                    CultureInfo.InvariantCulture,
                    "api/elements/element[matches-regex(resolve-name(" +
                    "node(), boolean({0})), '{1}', boolean({2})) and containers]",
                    fullyQualified, txtSearchText.Text, ignoreCase));

                expr.SetContext(context);

                foreach(XPathNavigator nav in navigator.Select(expr))
                    if(nav.HasChildren)
                    {
                        id = nav.GetAttribute("api", String.Empty);

                        if(id[0] == 'N')
                        {
                            entryType = ApiEntryType.Namespace;
                            visibility = ApiVisibility.Public;
                            nodeText = id;
                        }
                        else
                        {
                            subsubgroup = nav.SelectSingleNode("apidata/@subsubgroup");

                            if(subsubgroup != null)
                                subgroup = subsubgroup.Value;
                            else
                                subgroup = nav.SelectSingleNode("apidata/@subgroup").Value;

                            entryType = NodeInfo.EntryTypeFromId(id[0], subgroup);
                            visibility = NodeInfo.DetermineVisibility(id[0],
                                ((IHasXmlNode)nav).GetNode());

                            pos = id.IndexOf('(');

                            if(pos != -1)
                                id = id.Substring(0, pos);

                            pos = id.LastIndexOf('.');

                            if(pos != -1)
                                nodeText = id.Substring(pos + 1);
                            else
                                nodeText = id;
                        }

                        // Only include the wanted items
                        if(((CheckBox)pnlOptions.Controls[
                          (int)entryType + 1]).Checked &&
                          ((CheckBox)pnlOptions.Controls[
                          (int)visibility + 14]).Checked)
                        {
                            newNode = new NodeInfo(nodeText, id.Substring(2));
                            newNode.ApiNode = ((IHasXmlNode)nav).GetNode();
                            nodeList.Add(newNode);
                        }
                    }

                if(nodeList.Count == 0)
                    lvSearchResults.Items.Add(new ListViewItem(
                        "Nothing found", 0));
                else
                {
                    nodeList.Sort(
                    delegate(NodeInfo x, NodeInfo y)
                    {
                        int result = (int)x.EntryType - (int)y.EntryType;

                        if(result == 0)
                            result = String.Compare(x.Id, y.Id,
                                StringComparison.CurrentCulture);

                        return result;
                    });

                    foreach(NodeInfo ni in nodeList)
                        if(!lvSearchResults.Items.ContainsKey(ni.Id))
                        {
                            lvi = new ListViewItem(ni.NodeText,
                                (int)ni.EntryType + ((int)ni.Visibility * 11));
                            lvi.Name = ni.Id;
                            lvi.Tag = ni;

                            nodeText = ni.Id;

                            // Use the display name for the member
                            if(ni.EntryType >= ApiEntryType.Class &&
                              ni.EntryType <= ApiEntryType.Operator)
                            {
                                pos = nodeText.LastIndexOf('.');

                                if(pos != -1)
                                    nodeText = nodeText.Substring(0, pos + 1) +
                                        ni.NodeText;
                                else
                                    nodeText = ni.NodeText;

                                // Resolve templates in the type name
                                if((ni.EntryType == ApiEntryType.Constructor ||
                                  ni.EntryType == ApiEntryType.Method) &&
                                  nodeText.IndexOf('`') != -1)
                                    nodeText = ResolveNameFunction.ReplaceTypeTemplateMarker(
                                        ni.ApiNode, nodeText);
                            }
                            else
                                if(nodeText.IndexOf('`') != -1)
                                    nodeText = ResolveNameFunction.ReplaceTypeTemplateMarker(
                                        ni.ApiNode, nodeText);

                            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(
                                lvi, nodeText));

                            lvSearchResults.Items.Add(lvi);
                        }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unexpected error occurred while performing " +
                    "the search: " + ex.Message, Constants.AppName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;

                btnGoto.Enabled = btnInclude.Enabled = btnExclude.Enabled =
                    (lvSearchResults.Items.Count != 0);
            }
        }

        /// <summary>
        /// Goto the selected member in the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnGoto_Click(object sender, EventArgs e)
        {
            TreeNode root, foundNode, subNode;
            NodeInfo ni;
            XmlNode apiNode;
            string id, nameSpace, typeName = null, memberName = null;
            int pos;

            // If called to find an include/exclude item, use the sender
            if(sender != btnGoto)
                ni = (NodeInfo)((ListViewItem)sender).Tag;
            else
                if(lvSearchResults.SelectedItems.Count == 0)
                    ni = (NodeInfo)lvSearchResults.Items[0].Tag;
                else
                    ni = (NodeInfo)lvSearchResults.SelectedItems[0].Tag;

            // Figure out what to search
            apiNode = ni.ApiNode;

            if(apiNode.Name == "element" || apiNode.Name == "type" ||
              apiNode.Name == "namespace")
            {
                id = apiNode.Attributes["api"].Value;   // Inherited
                root = tvApiList.Nodes[1];
            }
            else
            {
                id = apiNode.Attributes["id"].Value;    // Documented
                root = tvApiList.Nodes[0];
            }

            if(id[0] == 'N')
                nameSpace = id.Substring(2);
            else
            {
                if(apiNode.Name != "type")
                    nameSpace = apiNode.SelectSingleNode(
                        "containers/namespace/@api").Value.Substring(2);
                else
                    nameSpace = apiNode.ParentNode.SelectSingleNode(
                        "namespace/@api").Value.Substring(2);

                if(id[0] == 'T')
                    typeName = id.Substring(2);
                else
                {
                    memberName = id.Substring(2);

                    pos = memberName.IndexOf('(');

                    if(pos != -1)
                        memberName = memberName.Substring(0, pos);

                    typeName = apiNode.SelectSingleNode(
                        "containers/type/@api").Value.Substring(2);
                }
            }

            // Search for the member from the namespace on down
            foundNode = SearchForMember(nameSpace, root);

            if(foundNode != null && typeName != null)
            {
                subNode = SearchForMember(typeName, foundNode);

                if(subNode != null)
                {
                    foundNode = subNode;

                    if(memberName != null)
                    {
                        subNode = SearchForMember(memberName, foundNode);

                        if(subNode != null)
                            foundNode = subNode;
                    }
                }
            }

            if(foundNode != null)
            {
                tvApiList.SelectedNode = foundNode;
                foundNode.EnsureVisible();
                tvApiList.Focus();
            }
            else
                tvApiList.SelectedNode = null;
        }

        /// <summary>
        /// Include or exclude the selected members
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnIncludeExclude_Click(object sender, EventArgs e)
        {
            bool checkedState = (sender == btnInclude);

            if(lvSearchResults.SelectedItems.Count == 0)
                return;

            foreach(ListViewItem lvi in lvSearchResults.SelectedItems)
            {
                btnGoto_Click(lvi, e);

                if(tvApiList.SelectedNode != null &&
                  tvApiList.SelectedNode.Checked != checkedState)
                    tvApiList.SelectedNode.Checked = checkedState;
            }
        }

        /// <summary>
        /// Double-clicking an item is the same as clicking Goto
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void lvSearchResults_DoubleClick(object sender, EventArgs e)
        {
            ListViewHitTestInfo hitInfo = lvSearchResults.HitTest(
                lvSearchResults.PointToClient(Cursor.Position));

            if(hitInfo.Item != null)
                btnGoto_Click(hitInfo.Item, e);
        }
        #endregion
    }
}
