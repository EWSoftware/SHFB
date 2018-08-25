//===============================================================================================================
// System  : Sandcastle Help File Builder WPF Controls
// File    : ApiNodeInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 12/03/2017
// Note    : Copyright 2017, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to associate additional information with each tree node to make it easier
// to look stuff up.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 11/29/2017  EFW  Moved the class into its own file and updated it for use in a bound tree view
//===============================================================================================================

// Ignore Spelling: typedata memberdata

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.XPath;

namespace SandcastleBuilder.WPF.PropertyPages
{
    /// <summary>
    /// This is used to associate additional information with each tree node to make it easier to look stuff
    /// up.
    /// </summary>
    public sealed class ApiNodeInfo : INotifyPropertyChanged
    {
        #region Private data members
        //=====================================================================

        private BindingList<ApiNodeInfo> subMembers;

        private bool isIncluded, isExpanded, isSelected;
        private Brush backgroundBrush;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the parent API node for this one
        /// </summary>
        public ApiNodeInfo Parent { get; }

        /// <summary>
        /// This read-only property is used to get the API ID (the namespace, type, or member name)
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// This read-only property returns the fully qualified name
        /// </summary>
        /// <remarks>This differs from the ID in that any templates in the type name are resolved to actual
        /// type parameter names.</remarks>
        public string FullyQualifiedName { get; }

        /// <summary>
        /// This read-only property is used to get the API node from the reflection information file
        /// </summary>
        /// <remarks>This will also set the <see cref="EntryType"/> based on the reflection information in
        /// the node.</remarks>
        public XmlNode ApiNode { get; }

        /// <summary>
        /// This read-only property is used to get the text to display in the tree view
        /// </summary>
        /// <value>This will be the full namespace name or the type or member name without the namespace.</value>
        public string NodeText { get; }

        /// <summary>
        /// This read-only property is used to get the API entry type for this node
        /// </summary>
        public ApiEntryType EntryType { get; }

        /// <summary>
        /// This read-only property is used to get the visibility of this node
        /// </summary>
        public ApiVisibility Visibility { get; }

        /// <summary>
        /// This is used to get or set an optional tool tip for the node
        /// </summary>
        public string ToolTip { get; set; }

        /// <summary>
        /// For types, this will be set to the filter name to use if the class is nested within another class.
        /// </summary>
        /// <remarks>In such cases, the parent class name(s) must prefix the type so that it can be correctly
        /// excluded or included.</remarks>
        public string FilterName { get; set; }

        /// <summary>
        /// Get or set whether or not the entry is a project exclude
        /// </summary>
        /// <remarks>If excluded via the Namespace Comments project option or an <c>&lt;exclude /&gt;</c>
        /// tag, this property will be set to true and the node cannot be marked as exposed.  It is also
        /// used to disallow changes to some of the fixed and inherited API entry nodes.</remarks>
        public bool IsProjectExclude { get; set; }

        /// <summary>
        /// This is used to get or set the background brush to use for the tree view item
        /// </summary>
        public Brush BackgroundBrush
        {
            get { return backgroundBrush; }
            set
            {
                if(backgroundBrush != value)
                {
                    backgroundBrush = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BackgroundBrush)));
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the member is included in the help file
        /// </summary>
        public bool IsIncluded
        {
            get { return isIncluded; }
            set
            {
                if(isIncluded != value)
                {
                    isIncluded = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsIncluded)));
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the node is expanded in the tree view
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if(isExpanded != value)
                {
                    isExpanded = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExpanded)));
                }
            }
        }

        /// <summary>
        /// This is used to get or set whether or not the node is selected in the tree view
        /// </summary>
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if(isSelected != value)
                {
                    isSelected = value;
                    this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        /// <summary>
        /// This read-only property returns the sub-members of this node
        /// </summary>
        public IList<ApiNodeInfo> SubMembers => subMembers;

        /// <summary>
        /// This read-only property is used to get the API icon to display in the tree view item
        /// </summary>
        public string ApiIcon
        {
            get
            {
                string uri = "pack://application:,,,/SandcastleBuilder.WPF;component/Resources/API{0}{1}.png";

                if(this.ApiNode == null)
                    uri = String.Format(uri, this.Id, String.Empty);
                else
                    switch(this.EntryType)
                    {
                        case ApiEntryType.None:
                            break;

                        case ApiEntryType.Namespace:
                            uri = String.Format(uri, this.EntryType, String.Empty);
                            break;

                        default:
                            uri = String.Format(uri, this.EntryType, this.Visibility);
                            break;
                    }

                return uri;
            }
        }

        /// <summary>
        /// This read-only property is used to get the font style for the tree view item
        /// </summary>
        public FontStyle FontStyle => !String.IsNullOrWhiteSpace(this.ToolTip) ? FontStyles.Italic : FontStyles.Normal;

        /// <summary>
        /// This read-only property is used to see if the <see cref="IsIncluded"/> state can be changed
        /// </summary>
        public bool CanChangeIncludeState => (this.ApiNode != null && !this.IsProjectExclude);

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="text">The text for the tree view</param>
        /// <param name="apiId">The API ID</param>
        /// <param name="apiNode">The API node from the reflection information file</param>
        /// <param name="apiParent">The API node that is the parent of this one</param>
        public ApiNodeInfo(string text, string apiId, XmlNode apiNode, ApiNodeInfo apiParent)
        {
            ApiVisibility visibility;

            this.Parent = apiParent;
            this.Id = apiId;
            this.ApiNode = apiNode;

            subMembers = new BindingList<ApiNodeInfo>();

            if(apiNode != null)
            {
                this.NodeText = this.DetermineNodeText(text);
                this.EntryType = this.DetermineApiEntryTypeAndVisibility(out visibility);
                this.Visibility = visibility;

                string fullName = apiId;

                if(this.EntryType >= ApiEntryType.Class && this.EntryType <= ApiEntryType.Operator)
                {
                    int pos = fullName.LastIndexOf('.');

                    if(pos != -1)
                        fullName = fullName.Substring(0, pos + 1) + this.NodeText;
                    else
                        fullName = this.NodeText;

                    // Resolve templates in the type name
                    if((this.EntryType == ApiEntryType.Constructor || this.EntryType == ApiEntryType.Method) &&
                      fullName.IndexOf('`') != -1)
                    {
                        fullName = XPathFunctionContext.ReplaceTypeTemplateMarker(apiNode, fullName);
                    }
                }
                else
                    if(fullName.IndexOf('`') != -1)
                        fullName = XPathFunctionContext.ReplaceTypeTemplateMarker(apiNode, fullName);

                this.FullyQualifiedName = fullName;
            }
            else
                this.NodeText = text;
        }
        #endregion

        #region INotifyPropertyChanged implementation
        //=====================================================================

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to determine the node text to display in the tree view
        /// </summary>
        /// <param name="nodeText">The text to use</param>
        /// <returns>The adjusted display text for the node</returns>
        private string DetermineNodeText(string nodeText)
        {
            int pos;

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
                            nodeText = nodeText.Substring(0, nodeText.Length - 1);

                        pos = nodeText.IndexOf('`');

                        if(pos != -1)
                            nodeText = nodeText.Substring(0, pos);

                        if(nodeText.StartsWith("op_", StringComparison.Ordinal))
                            nodeText = nodeText.Substring(3) + " Operator";
                    }

            // If this API node contains template information, add that info to the node's display text
            XmlNodeList templates;
            StringBuilder sb = new StringBuilder(100);
            int idx = 1;

            if(this.ApiNode.Name == "api")
                templates = this.ApiNode.SelectNodes("templates/template");
            else
            {
                templates = this.ApiNode.SelectNodes("specialization/template");

                if(templates.Count == 0)
                    templates = this.ApiNode.SelectNodes("specialization/type");
            }

            foreach(XmlNode template in templates)
            {
                if(sb.Length != 0)
                    sb.Append(',');

                if(template.Name != "type")
                    sb.Append(template.Attributes["name"].Value);
                else
                {
                    // For specializations of types, we don't want to show the type but a generic place holder
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

            return nodeText;
        }

        /// <summary>
        /// This will determine the API entry type and visibility based on the information in the reflection
        /// information node.
        /// </summary>
        private ApiEntryType DetermineApiEntryTypeAndVisibility(out ApiVisibility visibility)
        {
            XmlNode subsubgroup;
            string subgroup, entryId;

            visibility = ApiVisibility.Public;

            // Is it an inherited namespace?
            if(this.ApiNode.Name == "namespace")
                return ApiEntryType.Namespace;

            // Is it an inherited type?
            if(this.ApiNode.Name == "type")
            {
                // Assume class, it'll be close enough
                return ApiEntryType.Class;
            }

            // It's a documented or inherited member of some sort
            if(this.ApiNode.Name == "element" || this.ApiNode.Name == "type")
                entryId = this.ApiNode.Attributes["api"].Value;   // Inherited
            else
            {
                entryId = this.ApiNode.Attributes["id"].Value;    // Documented

                // Is it a namespace?
                if(entryId[0] == 'N')
                    return ApiEntryType.Namespace;
            }

            subsubgroup = this.ApiNode.SelectSingleNode("apidata/@subsubgroup");

            if(subsubgroup != null)
                subgroup = subsubgroup.Value;
            else
                subgroup = this.ApiNode.SelectSingleNode("apidata/@subgroup").Value;

            return EntryTypeFromId(entryId[0], subgroup);
        }

        /// <summary>
        /// Determine the API entry type from the ID and possible the subgroup
        /// </summary>
        /// <param name="apiType">The type character to convert</param>
        /// <param name="subgroup">The subgroup to use</param>
        /// <returns>An <see cref="ApiEntryType"/> indicating the entry type</returns>
        internal static ApiEntryType EntryTypeFromId(char apiType, string subgroup)
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
        /// <returns>An <see cref="ApiVisibility"/> indicating the entry's visibility</returns>
        internal static ApiVisibility DetermineVisibility(char apiType, XmlNode node)
        {
            ApiVisibility visibility;
            string visText;

            // Determine the visibility of the entry
            if(apiType == 'T')
                visText = node.SelectSingleNode("typedata/@visibility").Value;
            else
                visText = node.SelectSingleNode("memberdata/@visibility").Value;

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
}
