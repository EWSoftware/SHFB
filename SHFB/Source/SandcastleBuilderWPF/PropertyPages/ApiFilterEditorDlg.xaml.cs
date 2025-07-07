//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ApiFilterEditorDlg.xaml.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/21/2025
// Note    : Copyright 2007-2025, Eric Woodruff, All rights reserved
//
// This file contains the form used to edit the API filter items.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 07/20/2007  EFW  Created the code
// 01/17/2008  EFW  Made adjustments to support changes and fixes in the Sandcastle namespace ripping feature
// 07/08/2008  EFW  Reworked to support MSBuild project format
// 11/19/2011  EFW  Updated checked state optimization to handle some odd edge cases
// 11/22/2017  EFW  Converted the form to WPF for better high DPI scaling support on 4K displays
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.Project;

using Sandcastle.Platform.Windows;

using SandcastleBuilder.WPF.XPath;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This form is used to edit the API filter items
    /// </summary>
    public partial class ApiFilterEditorDlg : Window
    {
        #region Private data members
        //=====================================================================

        private readonly ApiFilterCollection apiFilter;
        private bool wasModified;

        private string reflectionFile;
        private Dictionary<string, ApiFilter> buildFilterEntries;

        private CancellationTokenSource cancellationTokenSource;
        private readonly IProgress<string> loadApiInfoProgress;

        private XmlDocument reflectionInfo;
        private XPathNavigator navigator;
        private XmlNode apisNode;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filter">The item collection to edit</param>
        public ApiFilterEditorDlg(ApiFilterCollection filter)
        {
            InitializeComponent();

            loadApiInfoProgress = new Progress<string>(loadApiInfo_ReportProgress);
            apiFilter = filter;
        }
        #endregion

        #region Build and data loading methods
        //=====================================================================

        /// <summary>
        /// This is used to report build progress
        /// </summary>
        /// <param name="e">The event arguments</param>
        private void buildProcess_ReportProgress(BuildProgressEventArgs e)
        {
            if(e.StepChanged)
                lblProgress.Text = e.BuildStep.ToString();
        }

        /// <summary>
        /// This is used to report progress while loading the API information
        /// </summary>
        /// <param name="progress">The progress message</param>
        private void loadApiInfo_ReportProgress(string progress)
        {
            lblProgress.Text = progress;
        }

        /// <summary>
        /// This is used to build the project and load the API filter information in the background
        /// </summary>
        /// <returns>A binding list containing the API node information</returns>
        private BindingList<ApiNodeInfo> BuildProject(IBuildProcess buildProcess)
        {
            BindingList<ApiNodeInfo> apiNodes = null;

            buildProcess.Build();

            // If successful, load the namespace nodes, and enable the UI
            if(buildProcess.CurrentBuildStep == BuildStep.Completed)
            {
                loadApiInfoProgress.Report("Loading API filter information...");

                reflectionFile = buildProcess.ReflectionInfoFilename;

                // Convert the build API filter to a dictionary to make it easier to find entries
                buildFilterEntries = [];

                this.ConvertApiFilter(buildProcess.CurrentProject.ApiFilter);
                apiNodes = this.LoadNamespaces();
            }

            return apiNodes;
        }

        /// <summary>
        /// This is used to convert the API filter from the build into a dictionary so that it is easier to look
        /// up the entries.
        /// </summary>
        /// <param name="filter">The filter collection to convert to a dictionary</param>
        private void ConvertApiFilter(ApiFilterCollection filter)
        {
            foreach(ApiFilter entry in filter)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
#if DEBUG
                // This shouldn't happen.  If it does, there's a problem in generating the API filter in the
                // build process.
                if(buildFilterEntries.ContainsKey(entry.FullName))
                    System.Diagnostics.Debugger.Break();
#endif
                // It has been known to happen very rarely for unknown reasons.  Use the indexer in the release
                // builds rather than Add() so that it doesn't prevent the filter from opening.  I was never
                // given a test case to try and duplicate the issue.
                buildFilterEntries[entry.FullName] = entry;

                if(entry.Children.Count != 0)
                    this.ConvertApiFilter(entry.Children);
            }
        }

        /// <summary>
        /// This is used to load the reflection information file and to load the root namespace nodes for the
        /// tree view.
        /// </summary>
        /// <returns>A binding list containing the API node information</returns>
        /// <remarks><para>The namespace nodes and type nodes are loaded on demand to reduce the time needed to
        /// create the tree view and to conserve some memory for extremely large builds.</para>
        /// 
        /// <para>Documented APIs are loaded into the first root node.  Inherited APIs are loaded into the second
        /// node.  By splitting the inherited stuff out, we can optimize the API filter and allow the user to get
        /// rid of unwanted inherited members with a single selection.</para></remarks>
        private BindingList<ApiNodeInfo> LoadNamespaces()
        {
            ApiNodeInfo nodeInfo, parentNode;
            ApiFilter filter;

            var nodeList = new List<ApiNodeInfo>();
            var existingIds = new HashSet<string>();

            var rootApiNodes = new BindingList<ApiNodeInfo>
            {
                new("Documented APIs", "Documented", null, null)
                {
                    IsIncluded = true,
                    IsExpanded = true,
                    ToolTip = "Documented APIs in your assemblies"
                },
                new("Inherited APIs", "Inherited", null, null)
                {
                    IsIncluded = true,
                    IsExpanded = true,
                    ToolTip = "APIs inherited from classes in dependent assemblies and the .NET Framework"
                }
            };

            loadApiInfoProgress.Report("Loading documented namespace information...");

            reflectionInfo = new XmlDocument();

            using(var reader = XmlReader.Create(reflectionFile, new XmlReaderSettings { CloseInput = true }))
            {
                reflectionInfo.Load(reader);
            }

            // Get the root APIs node as that's all we'll ever search
            apisNode = reflectionInfo.SelectSingleNode("reflection/apis");
            navigator = apisNode.CreateNavigator();

            // Build a set of nodes to store the necessary information and sort it
            parentNode = rootApiNodes[0];

            foreach(XmlNode nsNode in apisNode.SelectNodes("api[starts-with(@id, 'N:') or " +
              "(starts-with(@id, 'G:') and not(apidata/@subgroup='rootGroup'))]"))
            {
                string apiID = nsNode.Attributes["id"].Value, namespaceText = apiID.Substring(2);

                if(apiID.StartsWith("G:", StringComparison.Ordinal))
                    namespaceText += " (Group)";

                nodeInfo = new ApiNodeInfo(namespaceText, apiID.Substring(2), nsNode, parentNode);

                // This node may be namespace or namespace group
                bool isNamespace = (nodeInfo.EntryType == ApiEntryType.Namespace);

                // See if it's in the current filter
                if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out filter))
                {
                    nodeInfo.IsIncluded = true;

                    // Add a placeholder node for expansion on demand
                    if(isNamespace)
                        nodeInfo.SubMembers.Add(new ApiNodeInfo("Loading...", null, null, null));
                }
                else
                {
                    if(filter.IsProjectExclude && !filter.IsExposed)
                    {
                        nodeInfo.IsProjectExclude = true;
                        nodeInfo.ToolTip = "Excluded via namespace comments or an <exclude /> tag";
                    }
                    else
                        nodeInfo.IsIncluded = filter.IsExposed;

                    // Simple tristate workaround
                    if(filter.Children.Count != 0 && !filter.IsExposed)
                        nodeInfo.BackgroundBrush = Brushes.LightBlue;

                    if(isNamespace)
                    {
                        // Add children now as it contains filtered entries and we'll need to keep them in
                        // synch with the parent.
                        foreach(var m in this.AddTypes(nodeInfo))
                            nodeInfo.SubMembers.Add(m);

                        nodeInfo.IsExpanded = true;

                        loadApiInfoProgress.Report("Loading documented namespace information...");
                    }
                }

                nodeList.Add(nodeInfo);

                // Classes with extension methods can show up in the system types too so we'll filter them out
                // below.
                existingIds.Add(nodeInfo.NodeText);
            }

            nodeList.Sort((x, y) =>
            {
                int rVal = String.Compare(x.Id, y.Id, StringComparison.Ordinal);

                // Sometimes group namespace IDs are the same as normal namespaces, return group namespaces first
                return rVal == 0 ? -String.Compare(x.NodeText, y.NodeText, StringComparison.Ordinal) : rVal;
            });

            // Load the tree view with the namespaces for documented APIs as children of the first root node
            foreach(var ni in nodeList)
                parentNode.SubMembers.Add(ni);

            // Load inherited namespaces found in dependent assemblies and the .NET Framework itself
            loadApiInfoProgress.Report("Loading inherited namespace information...");

            nodeList.Clear();
            parentNode = rootApiNodes[1];

            // Build a set of nodes to store the necessary information and sort it
            foreach(XmlNode nsNode in apisNode.SelectNodes("api/elements/element/containers/namespace"))
            {
                string fullName = nsNode.Attributes["api"].Value.Substring(2);

                // Ignore existing IDs as noted above
                if(existingIds.Add(fullName))
                {
                    nodeInfo = new ApiNodeInfo(fullName, fullName, nsNode, parentNode)
                    {
                        IsProjectExclude = true,
                        ToolTip = "Namespace contains inherited types"
                    };

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out filter))
                    {
                        nodeInfo.IsIncluded = true;

                        // Add a placeholder node for expansion on demand
                        nodeInfo.SubMembers.Add(new ApiNodeInfo("Loading...", null, null, null));
                    }
                    else
                    {
                        nodeInfo.IsIncluded = filter.IsExposed;

                        // Simple tristate workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            nodeInfo.BackgroundBrush = Brushes.LightBlue;

                        // Add children now as it contains filtered entries and we'll need to keep them in synch
                        // with the parent.
                        foreach(var m in this.AddBaseTypes(nodeInfo))
                            nodeInfo.SubMembers.Add(m);

                        nodeInfo.IsExpanded = true;

                        loadApiInfoProgress.Report("Loading inherited namespace information...");
                    }

                    nodeList.Add(nodeInfo);
                }
            }

            // Load the tree view with the namespaces of inherited APIs as children of the second root node
            foreach(var ni in nodeList.OrderBy(n => n.NodeText))
                parentNode.SubMembers.Add(ni);

            return rootApiNodes;
        }

        /// <summary>
        /// Add all types found in the specified namespace node
        /// </summary>
        /// <param name="parentInfo">The parent API node to which they are added</param>
        /// <returns>An enumerable list of API nodes containing type information</returns>
        private IEnumerable<ApiNodeInfo> AddTypes(ApiNodeInfo parentInfo)
        {
            ApiNodeInfo nodeInfo;
            var nodeList = new List<ApiNodeInfo>();
            string fullName, memberName;
            int nsLen;

            if(parentInfo.Id.Length == 0)
                nsLen = 0;      // Global namespace
            else
            {
                nsLen = parentInfo.Id.Length;

                // Handle AjaxDoc 1.0 prefix
                if(parentInfo.Id[nsLen - 1] != ':')
                    nsLen++;
            }

            loadApiInfoProgress.Report("Loading types for " + parentInfo.NodeText);

            foreach(XmlNode typeNode in parentInfo.ApiNode.SelectNodes("elements/element"))
            {
                // Remove the namespace from the type name
                fullName = typeNode.Attributes["api"].Value.Substring(2);

                if(nsLen == 0)
                    memberName = fullName;
                else
                    memberName = fullName.Substring(nsLen);

                // Convert the element node to its actual API node
                nodeInfo = new ApiNodeInfo(memberName, fullName,
                    apisNode.SelectSingleNode("api[@id='" + typeNode.Attributes["api"].Value + "']"), parentInfo);

                // For nested classes, we'll need the parent class name(s) for the filter name
                if(memberName.IndexOf('.') != -1)
                    nodeInfo.FilterName = memberName;

                // See if it's in the current filter
                if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out ApiFilter filter))
                {
                    nodeInfo.IsIncluded = parentInfo.IsIncluded;

                    // Add a placeholder node for expansion on demand except for enumerations and delegates
                    // which never have children.
                    if(nodeInfo.EntryType != ApiEntryType.Enumeration && nodeInfo.EntryType != ApiEntryType.Delegate)
                        nodeInfo.SubMembers.Add(new ApiNodeInfo("Loading...", null, null, null));
                }
                else
                {
                    if(filter.IsProjectExclude && !filter.IsExposed)
                    {
                        nodeInfo.IsProjectExclude = true;
                        nodeInfo.ToolTip = "Removed via an <exclude /> tag.";
                    }
                    else
                        nodeInfo.IsIncluded = filter.IsExposed;

                    // Simple tristate workaround
                    if(filter.Children.Count != 0 && !filter.IsExposed)
                        nodeInfo.BackgroundBrush = Brushes.LightBlue;

                    // Add children now as it contains filtered entries and we'll need to keep them in synch
                    // with the parent.
                    if(nodeInfo.EntryType != ApiEntryType.Enumeration && nodeInfo.EntryType != ApiEntryType.Delegate)
                    {
                        foreach(var m in this.AddMembers(nodeInfo))
                            nodeInfo.SubMembers.Add(m);

                        nodeInfo.IsExpanded = true;
                    }
                }

                nodeList.Add(nodeInfo);
            }

            return nodeList.OrderBy(n => n.NodeText);
        }

        /// <summary>
        /// Add all base types found in the specified namespace node
        /// </summary>
        /// <param name="parentInfo">The parent API node to which they are added</param>
        /// <returns>An enumerable list of API nodes containing type information</returns>
        private IEnumerable<ApiNodeInfo> AddBaseTypes(ApiNodeInfo parentInfo)
        {
            ApiNodeInfo nodeInfo;
            var nodeList = new List<ApiNodeInfo>();
            var existingIds = new HashSet<string>();
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

            loadApiInfoProgress.Report("Loading inherited types for " + parentInfo.NodeText);

            foreach(XmlNode typeNode in apisNode.SelectNodes(
                "api/elements/element[containers/namespace/@api='N:" + parentInfo.Id + "']/containers/type"))
            {
                // Remove the namespace from the type name
                fullName = typeNode.Attributes["api"].Value.Substring(2);

                if(existingIds.Add(fullName))
                {
                    if(nsLen == 0)
                        memberName = fullName;
                    else
                        memberName = fullName.Substring(nsLen);

                    nodeInfo = new ApiNodeInfo(memberName, fullName, typeNode, parentInfo);

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out ApiFilter filter))
                    {
                        nodeInfo.IsIncluded = parentInfo.IsIncluded;

                        // Add a placeholder node for expansion on demand.  We can't tell the difference between
                        // API types so assume it has members.
                        nodeInfo.SubMembers.Add(new ApiNodeInfo("Loading...", null, null, null));
                    }
                    else
                    {
                        nodeInfo.IsIncluded = filter.IsExposed;

                        // Simple tristate workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            nodeInfo.BackgroundBrush = Brushes.LightBlue;

                        // Add children now as it contains filtered entries and we'll need to keep them in synch
                        // with the parent.
                        foreach(var m in this.AddBaseMembers(nodeInfo))
                            nodeInfo.SubMembers.Add(m);

                        nodeInfo.IsExpanded = true;
                    }

                    nodeList.Add(nodeInfo);
                }
            }

            return nodeList.OrderBy(n => n.NodeText);
        }

        /// <summary>
        /// Add all members found in the specified type node
        /// </summary>
        /// <param name="parentInfo">The parent API node to which they are added</param>
        /// <returns>An enumerable list of API nodes containing member information</returns>
        private IEnumerable<ApiNodeInfo> AddMembers(ApiNodeInfo parentInfo)
        {
            ApiNodeInfo nodeInfo;
            var nodeList = new List<ApiNodeInfo>();
            var existingIds = new HashSet<string>();
            string fullName, typeName, memberName;
            int pos;

            loadApiInfoProgress.Report("Loading members for " + parentInfo.NodeText);

            foreach(XmlNode memberNode in parentInfo.ApiNode.SelectNodes("elements/element"))
            {
                fullName = memberNode.Attributes["api"].Value.Substring(2);

                // We'll strip off parameters as the ripping feature only supports up to a member name.  If one
                // overload is ripped, they are all ripped.
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
                        // Convert the element node to its actual API node
                        nodeInfo = new ApiNodeInfo(memberName, fullName, apisNode.SelectSingleNode(
                            "api[@id='" + memberNode.Attributes["api"].Value + "']"), parentInfo);

                        // See if it's in the current filter
                        if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out ApiFilter filter))
                            nodeInfo.IsIncluded = parentInfo.IsIncluded;
                        else
                        {
                            if(filter.IsProjectExclude && !filter.IsExposed)
                            {
                                nodeInfo.IsProjectExclude = true;
                                nodeInfo.IsIncluded = false;
                                nodeInfo.ToolTip = "Removed via an <exclude /> tag.";
                            }
                            else
                                nodeInfo.IsIncluded = filter.IsExposed;

                            // Simple tristate workaround
                            if(filter.Children.Count != 0 && !filter.IsExposed)
                                nodeInfo.BackgroundBrush = Brushes.LightBlue;
                        }

                        nodeList.Add(nodeInfo);
                    }
                }
            }

            // Member nodes are sorted by API entry type and then by name
            return nodeList.OrderBy(n => n.EntryType).ThenBy(n => n.NodeText);
        }

        /// <summary>
        /// Add all inherited members found in the specified type node
        /// </summary>
        /// <param name="parentInfo">The parent API node to which they are added</param>
        /// <returns>An enumerable list of API nodes containing member information</returns>
        private IEnumerable<ApiNodeInfo> AddBaseMembers(ApiNodeInfo parentInfo)
        {
            ApiNodeInfo nodeInfo;
            var nodeList = new List<ApiNodeInfo>();
            var existingIds = new HashSet<string>();
            string fullName, memberName;
            int pos;

            loadApiInfoProgress.Report("Loading inherited members for " + parentInfo.NodeText);

            foreach(XmlNode memberNode in apisNode.SelectNodes("api/elements/element[containers/type/@api='T:" +
              parentInfo.Id + "']"))
            {
                fullName = memberNode.Attributes["api"].Value.Substring(2);

                // We'll strip off parameters as the ripping feature only supports up to a member name.  If one
                // overload is ripped, they are all ripped.
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
                    nodeInfo = new ApiNodeInfo(memberName, fullName, memberNode, parentInfo);

                    // See if it's in the current filter
                    if(!buildFilterEntries.TryGetValue(nodeInfo.Id, out ApiFilter filter))
                        nodeInfo.IsIncluded = parentInfo.IsIncluded;
                    else
                    {
                        nodeInfo.IsIncluded = filter.IsExposed;

                        // Simple tristate workaround
                        if(filter.Children.Count != 0 && !filter.IsExposed)
                            nodeInfo.BackgroundBrush = Brushes.LightBlue;
                    }

                    nodeList.Add(nodeInfo);
                }
            }

            // Member nodes are sorted by API entry type and then by name
            return nodeList.OrderBy(n => n.EntryType).ThenBy(n => n.NodeText);
        }
        #endregion

        #region Miscellaneous helper methods
        //=====================================================================

        /// <summary>
        /// Examine the nodes and optimize the state of the filter based on various conditions
        /// </summary>
        /// <param name="currentNode">The node in which the checked state changed</param>
        /// <param name="setChildren">Set the checked state of all child nodes to the parent node's state if true</param>
        private static void OptimizeIncludedState(ApiNodeInfo currentNode, bool setChildren)
        {
            bool includedState = currentNode.IsIncluded;
            int includedCount = 0, excludedCount = 0;

            // Apply the check state to all children
            if(setChildren && currentNode.SubMembers.Count != 0)
            {
                // Reset the background color in case it's a mixed state node
                currentNode.BackgroundBrush = null;

                foreach(var child in currentNode.SubMembers)
                {
                    if(child.ApiNode != null && !child.IsProjectExclude)
                    {
                        child.IsIncluded = includedState;

                        // And members if it's a type node.  We only go to a maximum of two levels so no need
                        // for recursion.
                        foreach(var memberChild in child.SubMembers)
                        {
                            if(memberChild.ApiNode != null && !memberChild.IsProjectExclude)
                                memberChild.IsIncluded = includedState;
                        }
                    }
                }
            }

            // If it's a member or type node, count the number of checked and unchecked nodes.  Skip nodes that
            // can't be changed though.
            if(currentNode.Parent.Parent != null && !currentNode.Parent.IsProjectExclude)
            {
                foreach(var child in currentNode.Parent.SubMembers)
                {
                    if(child.IsIncluded)
                        includedCount++;
                    else
                        excludedCount++;
                }

                // Optimize the parent node's state based on the number of checked and unchecked items
                if(includedCount > 0 || currentNode.Parent.EntryType == ApiEntryType.Namespace)
                    currentNode.Parent.IsIncluded = (includedCount > excludedCount);
                else
                {
                    if(!currentNode.Parent.IsIncluded && includedCount == 0)
                    {
                        // Note that we must always set this to true here to handle some odd edge cases:
                        //
                        // 1) When you have a class with one member where you only want to document the class
                        //    but not the member.
                        // 2) When you have a class with three members but only want to document the class but
                        //    not any of the members.
                        //
                        // This may check the parent node when it wasn't checked.  In such cases, you will need
                        // to uncheck it again if you don't want it included.
                        currentNode.Parent.IsIncluded = true;
                    }
                }

                // Color mixed nodes so that they stand out.  This is a quick way of noting their state without
                // reworking the code to support tristate values.
                if(includedCount != 0 && !currentNode.Parent.IsIncluded)
                    currentNode.Parent.BackgroundBrush = Brushes.LightBlue;
                else
                    currentNode.Parent.BackgroundBrush = null;

                // Do the same for the parent's parent node
                OptimizeIncludedState(currentNode.Parent, false);
            }
        }

        /// <summary>
        /// Search the children of the specified API node looking for the given API member name
        /// </summary>
        /// <param name="memberName">The member name for which to search</param>
        /// <param name="root">The root node to search</param>
        /// <returns>The API node matching the given ID or null if not found</returns>
        private ApiNodeInfo SearchForMember(string memberName, ApiNodeInfo root)
        {
            // Expand the node if necessary
            if(root.SubMembers.Count == 1 && root.SubMembers[0].ApiNode == null)
            {
                IEnumerable<ApiNodeInfo> subMembers;

                if(root.EntryType == ApiEntryType.Namespace)
                {
                    if(root.Parent.Id == "Documented")
                        subMembers = this.AddTypes(root);
                    else
                        subMembers = this.AddBaseTypes(root);
                }
                else
                {
                    if(root.Parent.Parent.Id == "Documented")
                        subMembers = this.AddMembers(root);
                    else
                        subMembers = this.AddBaseMembers(root);
                }

                root.SubMembers.Clear();

                foreach(var m in subMembers)
                    root.SubMembers.Add(m);
            }

            foreach(var child in root.SubMembers)
            {
                if(child.Id == memberName)
                {
                    root.IsExpanded = true;
                    return child;
                }
            }

            return null;
        }

        /// <summary>
        /// This is used to find the selected API node in the tree view and select it
        /// </summary>
        /// <param name="findNode">The API node info to find</param>
        /// <returns>The node in the tree view or null if not found</returns>
        private ApiNodeInfo FindNode(XmlNode findNode)
        {
            ApiNodeInfo root, foundNode, subNode;
            string id, nameSpace, typeName = null, memberName = null;
            int pos;

            // Figure out what to search
            if(findNode.Name == "element" || findNode.Name == "type" || findNode.Name == "namespace")
            {
                id = findNode.Attributes["api"].Value;   // Inherited
                root = (ApiNodeInfo)tvApiList.Items[1];
            }
            else
            {
                id = findNode.Attributes["id"].Value;    // Documented
                root = (ApiNodeInfo)tvApiList.Items[0];
            }

            if(id[0] == 'N')
                nameSpace = id.Substring(2);
            else
            {
                if(findNode.Name != "type")
                    nameSpace = findNode.SelectSingleNode("containers/namespace/@api").Value.Substring(2);
                else
                    nameSpace = findNode.ParentNode.SelectSingleNode("namespace/@api").Value.Substring(2);

                if(id[0] == 'T')
                    typeName = id.Substring(2);
                else
                {
                    memberName = id.Substring(2);

                    pos = memberName.IndexOf('(');

                    if(pos != -1)
                        memberName = memberName.Substring(0, pos);

                    typeName = findNode.SelectSingleNode("containers/type/@api").Value.Substring(2);
                }
            }

            try
            {
                // This may expand nodes synchronously so show the wait cursor in case it takes a while
                Mouse.OverrideCursor = Cursors.Wait;

                // Search for the member from the namespace on down
                foundNode = this.SearchForMember(nameSpace, root);

                if(foundNode != null && typeName != null)
                {
                    subNode = this.SearchForMember(typeName, foundNode);

                    if(subNode != null)
                    {
                        foundNode = subNode;

                        if(memberName != null)
                        {
                            subNode = this.SearchForMember(memberName, foundNode);

                            if(subNode != null)
                                foundNode = subNode;
                        }
                    }
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }

            return foundNode;
        }
        #endregion

        #region Methods to generate new API filter
        //=====================================================================

        /// <summary>
        /// This is used to add namespace filters to the API filter
        /// </summary>
        /// <param name="root">The API node from which to start</param>
        private void AddNamespaceFilter(ApiNodeInfo root)
        {
            ApiFilter filter;

            foreach(var node in root.SubMembers)
            {
                if(AllChildrenMatchParentCheckState(node.SubMembers, node.IsIncluded))
                {
                    // We only need to add a filter in this case if the namespace is being excluded
                    if(!node.IsIncluded)
                        apiFilter.Add(new ApiFilter(node.EntryType, node.Id, false));
                }
                else
                {
                    filter = new ApiFilter(node.EntryType, node.Id, node.IsIncluded);
                    apiFilter.Add(filter);

                    // Add child filters that match the opposite state
                    AddChildFilter(filter, node.SubMembers, !node.IsIncluded);
                }
            }
        }

        /// <summary>
        /// This will add child filter entries for each API node where the included state matches the given state
        /// </summary>
        /// <param name="filter">The filter to which the entries are added</param>
        /// <param name="nodes">The API nodes to scan</param>
        /// <param name="state">The include state to match</param>
        private static void AddChildFilter(ApiFilter filter, IEnumerable<ApiNodeInfo> nodes, bool state)
        {
            ApiFilter childFilter;
            string parentId;
            int idx;

            foreach(var node in nodes)
            {
                if(AllChildrenMatchParentCheckState(node.SubMembers, node.IsIncluded))
                {
                    // We only need to add a filter in this case if the node state matches the given state or if
                    // it's a nested class with a state different than the parent class.
                    idx = node.NodeText.LastIndexOf('.');

                    if(idx != -1)
                        parentId = node.NodeText.Substring(0, idx);
                    else
                        parentId = null;

                    if(node.IsIncluded == state || (parentId != null && filter.Children.Any(
                      f => f.FilterName == parentId && f.IsExposed != node.IsIncluded)))
                    {
                        childFilter = new ApiFilter(node.EntryType, node.Id, node.IsIncluded);

                        // Override the filter name if necessary
                        if(!String.IsNullOrWhiteSpace(node.FilterName))
                            childFilter.FilterName = node.FilterName;

                        filter.Children.Add(childFilter);
                    }
                }
                else
                {
                    childFilter = new ApiFilter(node.EntryType, node.Id, node.IsIncluded);

                    // Override the filter name if necessary
                    if(!String.IsNullOrWhiteSpace(node.FilterName))
                        childFilter.FilterName = node.FilterName;

                    filter.Children.Add(childFilter);

                    // Add child filters that match the opposite state of the parent node
                    AddChildFilter(childFilter, node.SubMembers, !node.IsIncluded);
                }
            }
        }

        /// <summary>
        /// This is used to see if the given node and all of its children match the specified state
        /// </summary>
        /// <param name="nodes">The nodes to check</param>
        /// <param name="state">The state to match</param>
        /// <returns>True if all children's included states match the parent node's included state, false if not</returns>
        private static bool AllChildrenMatchParentCheckState(IEnumerable<ApiNodeInfo> nodes, bool state)
        {
            foreach(var child in nodes)
            {
                // Unexpanded placeholders are ignored
                if(child.ApiNode != null && child.IsIncluded != state)
                    return false;

                if(child.SubMembers.Count != 0)
                {
                    if(!AllChildrenMatchParentCheckState(child.SubMembers, state))
                        return false;
                }
            }

            return true;
        }
        #endregion

        #region Event handlers
        //=====================================================================

        /// <summary>
        /// This is used to start the background build process from which we will get the information to load the
        /// tree view.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void ApiFilterEditorDlg_Loaded(object sender, RoutedEventArgs e)
        {
            ISandcastleProject tempProject = null;
            string tempPath;

            tvApiList.IsEnabled = grdSearchOptions.IsEnabled = btnReset.IsEnabled = false;
            txtSearchText.Text = "Loading...";

            try
            {
                // Clone the project for the build and adjust its properties for our needs.  Build output is
                // stored in a temporary folder and it keeps the intermediate files.
                tempPath = Path.GetTempFileName();

                File.Delete(tempPath);
                tempPath = Path.Combine(Path.GetDirectoryName(tempPath), "SHFBPartialBuild");

                if(!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                tempProject = apiFilter.Project.Clone();
                tempProject.CleanIntermediates = false;
                tempProject.OutputPath = tempPath;

                cancellationTokenSource = new CancellationTokenSource();

                var buildProcess = tempProject.CreateBuildProcess(PartialBuildType.GenerateReflectionInfo);

                buildProcess.ProgressReportProvider = new Progress<BuildProgressEventArgs>(buildProcess_ReportProgress);
                buildProcess.CancellationToken = cancellationTokenSource.Token;
                buildProcess.SuppressApiFilter = true;      // We must suppress the current API filter for this build

                var apiNodes = await Task.Run(() => this.BuildProject(buildProcess),
                    cancellationTokenSource.Token).ConfigureAwait(true);

                if(!cancellationTokenSource.IsCancellationRequested)
                {
                    if(buildProcess.CurrentBuildStep == BuildStep.Completed)
                    {
                        tvApiList.ItemsSource = apiNodes;
                        tvApiList.IsEnabled = grdSearchOptions.IsEnabled = btnReset.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("Unable to build project to obtain API information.  Please perform a " +
                            "normal build to identify and correct the problem.", Constants.AppName,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                    this.Close();
            }
            catch(OperationCanceledException)
            {
                // Just close if canceled while loading the filter info after the build
                this.Close();
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                MessageBox.Show("Unable to build project to obtain API information.  Error: " +
                    ex.Message, Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                try
                {
                    // Restore the current project's base path
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(apiFilter.Project.Filename));
                }
                catch(Exception ex)
                {
                    // Ignore any exceptions
                    System.Diagnostics.Debug.WriteLine(ex);
                }

                if(tempProject != null)
                {
                    try
                    {
                        // Delete the temporary project's working files
                        if(!String.IsNullOrWhiteSpace(tempProject.OutputPath) && Directory.Exists(tempProject.OutputPath))
                            Directory.Delete(tempProject.OutputPath, true);
                    }
                    catch
                    {
                        // Eat the exception.  We'll ignore it if the temporary files cannot be deleted.
                    }

                    tempProject.Dispose();
                }

                if(cancellationTokenSource != null)
                {
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }

                grdProgress.Visibility = Visibility.Hidden;
                txtSearchText.Text = null;
            }
        }

        /// <summary>
        /// Shut down the build process thread and clean up on exit
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void ApiFilterEditorDlg_Closing(object sender, CancelEventArgs e)
        {
            if(cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                if(MessageBox.Show("A build is currently taking place to obtain API information.  Do you want " +
                  "to abort it and close this form?", Constants.AppName, MessageBoxButton.YesNo,
                  MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                if(cancellationTokenSource != null)
                {
                    cancellationTokenSource.Cancel();
                    e.Cancel = true;
                }

                return;
            }

            if(wasModified)
            {
                apiFilter.Clear();

                // Add documented namespace filters
                this.AddNamespaceFilter((ApiNodeInfo)tvApiList.Items[0]);

                // Add filters for inherited types
                this.AddNamespaceFilter((ApiNodeInfo)tvApiList.Items[1]);
            }
        }

        /// <summary>
        /// View help for this form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            UiUtility.ShowHelpTopic("7df16a60-f718-4b8f-bfa2-88c42906070c");
        }

        /// <summary>
        /// Reset the API filter by clearing its content and closing the form
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Are you sure you want to clear the API filter and reset it to its default state?",
              Constants.AppName, MessageBoxButton.YesNo, MessageBoxImage.Question,
              MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                wasModified = false;
                apiFilter.Clear();
                this.Close();
            }
        }

        /// <summary>
        /// This is used to load child tree nodes on demand which speeds up the initial form load for extremely
        /// large projects.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private async void tvApiList_Expanded(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is TreeViewItem tvi)
            {
                var apiInfo = (ApiNodeInfo)tvi.DataContext;

                if(apiInfo.Parent != null && apiInfo.SubMembers.Count != 0 && apiInfo.SubMembers[0].ApiNode == null)
                {
                    try
                    {
                        lblProgress.Text = null;
                        grdProgress.Visibility = Visibility.Visible;

                        var subMembers = await Task.Run(() =>
                        {
                            if(apiInfo.EntryType == ApiEntryType.Namespace)
                            {
                                if(apiInfo.Parent.Id == "Documented")
                                    return this.AddTypes(apiInfo);

                                return this.AddBaseTypes(apiInfo);
                            }
                            else
                            {
                                if(apiInfo.Parent.Parent.Id == "Documented")
                                    return this.AddMembers(apiInfo);

                                return this.AddBaseMembers(apiInfo);
                            }
                        }).ConfigureAwait(true);

                        apiInfo.SubMembers.Clear();

                        foreach(var m in subMembers)
                            apiInfo.SubMembers.Add(m);
                    }
                    finally
                    {
                        grdProgress.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// Optimize the filter when a node's included state changes
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void chkIncludedState_Click(object sender, RoutedEventArgs e)
        {
            if(e.OriginalSource is CheckBox c)
            {
                OptimizeIncludedState((ApiNodeInfo)c.DataContext, true);
                wasModified = true;
            }
        }
        #endregion

        #region Search related event handlers
        //=====================================================================

        /// <summary>
        /// Search for members that match the search conditions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            ApiEntryType entryType;
            ApiVisibility visibility;
            string id, nodeText, subgroup;
            int pos, ignoreCase = Convert.ToInt32(!(chkCaseSensitive.IsChecked ?? false)),
                fullyQualified = Convert.ToInt32(chkFullyQualified.IsChecked ?? false);
            List<ApiNodeInfo> nodeList;
            XPathNavigator subsubgroup;
            var entryTypeOptions = new Dictionary<ApiEntryType, bool>
            {
                { ApiEntryType.Namespace, chkNamespaces.IsChecked.Value },
                { ApiEntryType.Class, chkClasses.IsChecked.Value },
                { ApiEntryType.Structure, chkStructures.IsChecked.Value },
                { ApiEntryType.Interface, chkInterfaces.IsChecked.Value },
                { ApiEntryType.Enumeration, chkEnumerations.IsChecked.Value },
                { ApiEntryType.Delegate, chkDelegates.IsChecked.Value },
                { ApiEntryType.Constructor, chkConstructors.IsChecked.Value },
                { ApiEntryType.Method, chkMethods.IsChecked.Value },
                { ApiEntryType.Operator, chkOperators.IsChecked.Value },
                { ApiEntryType.Property, chkProperties.IsChecked.Value },
                { ApiEntryType.Event, chkEvents.IsChecked.Value },
                { ApiEntryType.Field, chkFields.IsChecked.Value }
            };
            var visibilityOptions = new Dictionary<ApiVisibility, bool>
            {
                { ApiVisibility.Public, chkPublic.IsChecked.Value },
                { ApiVisibility.Protected, chkProtected.IsChecked.Value },
                { ApiVisibility.Internal, chkInternal.IsChecked.Value },
                { ApiVisibility.Private, chkPrivate.IsChecked.Value }
            };

            if(txtSearchText.Text.Trim().Length == 0)
            {
                MessageBox.Show("A search expression is required", Constants.AppName, MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                return;
            }

            try
            {
                Regex reSearch = new(txtSearchText.Text);
            }
            catch(ArgumentException ex)
            {
                MessageBox.Show("The search regular expression is not valid: " + ex.Message, Constants.AppName,
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            nodeList = [];

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                btnGoto.IsEnabled = btnInclude.IsEnabled = btnExclude.IsEnabled = false;

                // Use the custom XPath function matches-regex to perform a regular expression search for
                // matching nodes.  The custom XPath function resolve-name is used to convert the API names to
                // their display format for the search.
                XPathFunctionContext context = new();
                XPathExpression expr = navigator.Compile(String.Format(CultureInfo.InvariantCulture,
                    "api[matches-regex(resolve-name(node(), boolean({0})), '{1}', boolean({2}))]",
                    fullyQualified, txtSearchText.Text, ignoreCase));

                // The first search uses the documented APIs
                expr.SetContext(context);
                dgSearchResults.ItemsSource = null;

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

                        entryType = ApiNodeInfo.EntryTypeFromId(id[0], subgroup);
                        visibility = ApiNodeInfo.DetermineVisibility(id[0], ((IHasXmlNode)nav).GetNode());

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
                    if(entryTypeOptions[entryType] && visibilityOptions[visibility])
                        nodeList.Add(new ApiNodeInfo(nodeText, id.Substring(2), ((IHasXmlNode)nav).GetNode(), null));
                }

                // Search inherited APIs as well.  Namespaces first...
                expr = navigator.Compile(String.Format(CultureInfo.InvariantCulture,
                    "api/elements/element/containers/namespace[matches-regex(string(@api), '{0}', boolean({1}))]",
                    txtSearchText.Text, ignoreCase));

                expr.SetContext(context);

                foreach(XPathNavigator nav in navigator.Select(expr))
                {
                    id = nav.GetAttribute("api", String.Empty).Substring(2);

                    // Only include the wanted items
                    if(entryTypeOptions[ApiEntryType.Namespace])
                        nodeList.Add(new ApiNodeInfo(id, id, ((IHasXmlNode)nav).GetNode(), null));
                }

                // ... then types ...
                expr = navigator.Compile(String.Format(CultureInfo.InvariantCulture,
                    "api/elements/element/containers/type[matches-regex(resolve-name(node(), boolean({0})), '{1}', boolean({2}))]",
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

                    // Only include the wanted items.  Since we can't tell the actual API type, we'll assume
                    // Class for these.
                    if(entryTypeOptions[ApiEntryType.Class])
                        nodeList.Add(new ApiNodeInfo(nodeText, id, ((IHasXmlNode)nav).GetNode(), null));
                }

                // ... and then members.
                expr = navigator.Compile(String.Format(CultureInfo.InvariantCulture,
                    "api/elements/element[matches-regex(resolve-name(node(), boolean({0})), '{1}', boolean({2})) and containers]",
                    fullyQualified, txtSearchText.Text, ignoreCase));

                expr.SetContext(context);

                foreach(XPathNavigator nav in navigator.Select(expr))
                {
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

                            entryType = ApiNodeInfo.EntryTypeFromId(id[0], subgroup);
                            visibility = ApiNodeInfo.DetermineVisibility(id[0], ((IHasXmlNode)nav).GetNode());

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
                        if(entryTypeOptions[entryType] && visibilityOptions[visibility])
                            nodeList.Add(new ApiNodeInfo(nodeText, id.Substring(2), ((IHasXmlNode)nav).GetNode(), null));
                    }
                }

                if(nodeList.Count == 0)
                    nodeList.Add(new ApiNodeInfo("Nothing found", "NotFound", null, null));
                else
                {
                    // Filter out duplicate inherited members and sort the results
                    nodeList = [.. nodeList.GroupBy(n => n.Id).Select(g => g.First()).OrderBy(
                        n => n.EntryType).ThenBy(n => n.Id)];
                }

                dgSearchResults.ItemsSource = nodeList;
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unexpected error occurred while performing the search: " + ex.Message,
                    Constants.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;

                btnGoto.IsEnabled = btnInclude.IsEnabled = btnExclude.IsEnabled =
                    (nodeList.Count != 0 && nodeList[0].ApiNode != null);
            }
        }

        /// <summary>
        /// Goto the selected search result member in the tree view
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnGoto_Click(object sender, RoutedEventArgs e)
        {
            ApiNodeInfo ni;

            if(dgSearchResults.SelectedItem == null)
                ni = (ApiNodeInfo)dgSearchResults.Items[0];
            else
                ni = (ApiNodeInfo)dgSearchResults.SelectedItem;

            // Figure out what to search
            if(ni.ApiNode != null)
            {
                var foundNode = this.FindNode(ni.ApiNode);

                if(foundNode != null)
                {
                    foundNode.IsSelected = true;
                    tvApiList.Focus();
                }
            }
        }

        /// <summary>
        /// Include or exclude the selected members in the search results grid
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void btnIncludeExclude_Click(object sender, RoutedEventArgs e)
        {
            bool includedState = (sender == btnInclude);

            if(dgSearchResults.SelectedItems.Count != 0)
            {
                foreach(ApiNodeInfo selection in dgSearchResults.SelectedItems)
                {
                    if(selection.ApiNode != null)
                    {
                        var foundNode = this.FindNode(selection.ApiNode);

                        if(foundNode != null)
                        {
                            foundNode.IsSelected = true;

                            if(foundNode.IsIncluded != includedState)
                            {
                                foundNode.IsIncluded = includedState;
                                OptimizeIncludedState(foundNode, true);
                                wasModified = true;
                            }
                        }
                    }
                }
            }

            tvApiList.Focus();
        }

        /// <summary>
        /// Double-clicking an item in the search results grid is the same as clicking Goto
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        private void dgSearchResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(e.OriginalSource is FrameworkElement source)
            {
                var parent = source.ParentElementOfType<DataGridCell>();

                if(parent != null)
                    this.btnGoto_Click(sender, e);
            }
        }
        #endregion
    }
}
