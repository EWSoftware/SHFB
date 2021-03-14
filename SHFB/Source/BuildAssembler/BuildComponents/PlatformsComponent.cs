// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

// still have problems with spaces

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This component is used to insert platform information into the topics
    /// </summary>
    public class PlatformsComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("Platforms Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new PlatformsComponent(base.BuildAssembler);
            }
        }
        #endregion

        private Dictionary<string, Dictionary<string, VersionFilter>> versionFilters = new Dictionary<string, Dictionary<string, VersionFilter>>();

        private XPathExpression platformQuery = XPathExpression.Compile("/platforms/platform");
        private XPathExpression referenceExpression = XPathExpression.Compile("/document/reference");
        private XPathExpression versionNodesExpression = XPathExpression.Compile("versions//version");
        private XPathExpression apiGroupExpression = XPathExpression.Compile("string(apidata/@group)");
        private XPathExpression topicdataGroupExpression = XPathExpression.Compile("string(topicdata/@group)");
        private XPathExpression topicdataSubgroupExpression = XPathExpression.Compile("string(topicdata/@subgroup)");
        private XPathExpression listTypeNameExpression = XPathExpression.Compile("string(apidata/@name)");
        private XPathExpression apiNamespaceNameExpression = XPathExpression.Compile("string(containers/namespace/apidata/@name)");
        private XPathExpression memberTypeNameExpression = XPathExpression.Compile("string(containers/type/apidata/@name)");

        private XPathExpression listTopicElementNodesExpression = XPathExpression.Compile("elements//element");

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected PlatformsComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // get the filter files
            XPathNodeIterator filterNodes = configuration.Select("filter");

            foreach (XPathNavigator filterNode in filterNodes)
            {
                string filterFiles = filterNode.GetAttribute("files", String.Empty);

                if(filterFiles == null || filterFiles.Length == 0)
                    throw new ConfigurationErrorsException("The filter/@files attribute must specify a path.");

                ParseDocuments(filterFiles);
            }
        }

        /// <summary>
        /// Parse platform information
        /// </summary>
        /// <param name="wildcardPath">The path to the platform information files</param>
        protected void ParseDocuments(string wildcardPath)
        {
            string filterFiles = Environment.ExpandEnvironmentVariables(wildcardPath);
            if ((filterFiles == null) || (filterFiles.Length == 0))
                throw new ConfigurationErrorsException("The filter path is an empty string.");

            WriteMessage(MessageLevel.Info, "Searching for files that match '{0}'.", filterFiles);
            string directoryPart = Path.GetDirectoryName(filterFiles);
            if (String.IsNullOrEmpty(directoryPart))
                directoryPart = Environment.CurrentDirectory;
            directoryPart = Path.GetFullPath(directoryPart);
            string filePart = Path.GetFileName(filterFiles);
            string[] files = Directory.GetFiles(directoryPart, filePart);
            foreach (string file in files)
                ParseDocument(file);
            WriteMessage(MessageLevel.Info, "Found {0} files in {1}.", files.Length, filterFiles);
        }

        private void AddPlatformVersionFilter(string platformId, string versionId, XPathNavigator platformNode, string file)
        {
            Dictionary<string, VersionFilter> platformFrameworks;
            if (!versionFilters.TryGetValue(platformId, out platformFrameworks))
            {
                platformFrameworks = new Dictionary<string, VersionFilter>();
                versionFilters.Add(platformId, platformFrameworks);
            }

            try
            {
                VersionFilter filter;
                XmlReader platformReader = platformNode.ReadSubtree();
                platformReader.MoveToContent();
                if (!platformFrameworks.TryGetValue(versionId, out filter))
                {
                    filter = new VersionFilter(platformReader, versionId, file);
                }
                else
                {
                    // if the platform already has a filter for this version, add the data from the current platform node
                    filter.AddPlatformNode(platformReader, file);
                }
                platformReader.Close();

                platformFrameworks.Remove(versionId);
                platformFrameworks.Add(versionId, filter);
            }
            catch (Exception e)
            {
                WriteMessage(MessageLevel.Error, e.Message);
            }
        }

        private void ParseDocument(string file)
        {
            try
            {
                XPathDocument document = new XPathDocument(file);

                XPathNodeIterator platformNodes = document.CreateNavigator().Select(platformQuery);
                foreach (XPathNavigator platformNode in platformNodes)
                {
                    string platformId = platformNode.GetAttribute("name", String.Empty);
                    string[] platformIds = platformId.Split(',');

                    string version = platformNode.GetAttribute("version", String.Empty);
                    string[] versionIds = version.Split(',');
                    for (int i = 0; i < versionIds.Length; i++)
                    {
                        for (int j = 0; j < platformIds.Length; j++)
                        {
                            XPathNavigator platformNodeClone = platformNode.Clone();
                            AddPlatformVersionFilter(platformIds[j], versionIds[i], platformNodeClone, file);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                WriteMessage(MessageLevel.Error, e.Message);
            }
        }

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            XPathNavigator targetDoc = document.CreateNavigator();
            XPathNavigator referenceNode = targetDoc.SelectSingleNode(referenceExpression);
            string apiGroup = (string)referenceNode.Evaluate(apiGroupExpression);
            string topicdataGroup = (string)referenceNode.Evaluate(topicdataGroupExpression);
            string topicdataSubgroup = (string)referenceNode.Evaluate(topicdataSubgroupExpression);

            // get the namespace and type name of the current type to locate the filter information that applies to the current topic
            // For filtering inherited members, the platform filters use the namespace and type name of the inheriting type, not the declaring type,
            string topicNamespaceName = (string)referenceNode.Evaluate(apiNamespaceNameExpression);
            string topicTypeName = (string)referenceNode.Evaluate(memberTypeNameExpression);

            // write platforms info for normal api topics (excluding member list and overload list topics
            if (topicdataGroup != "list" && topicdataSubgroup != "DerivedTypeList" && (apiGroup == "type" || apiGroup == "member") && versionFilters.Count > 0)
                WriteApiPlatforms(referenceNode, apiGroup, topicTypeName, topicNamespaceName);

            // write platforms for elements//element nodes (member list and overload topics; not root or namespace)
            if ((topicdataGroup == "list" && topicdataSubgroup != "DerivedTypeList") && apiGroup != "root" && versionFilters.Count > 0)
            {
                if (topicdataSubgroup != "overload")
                    topicTypeName = (string)referenceNode.Evaluate(listTypeNameExpression);

                XPathNodeIterator elementNodes = referenceNode.Select(listTopicElementNodesExpression);

                foreach (XPathNavigator elementNode in elementNodes)
                    WriteApiPlatforms(elementNode, "member", topicTypeName, topicNamespaceName);
            }
        }

        private void WriteApiPlatforms(XPathNavigator referenceNode, string apiGroup, string topicTypeName, string topicNamespaceName)
        {
            XPathNodeIterator versionNodes = referenceNode.Select(versionNodesExpression);
            List<string> supportedPlatforms = new List<string>();
            XmlWriter platformsWriter = referenceNode.AppendChild();
            
            foreach (string platformId in versionFilters.Keys)
            {
                Dictionary<string, VersionFilter> filters = versionFilters[platformId];
                bool included = false;
                foreach (XPathNavigator versionNode in versionNodes)
                {
                    string versionId = versionNode.GetAttribute("name", string.Empty);
                    VersionFilter filter;
                    if (filters.TryGetValue(versionId, out filter))
                    {
                        switch (apiGroup)
                        {
                            case "type":
                                included = filter.IsIncludedType(referenceNode, topicNamespaceName);
                                break;

                            case "member":
                                included = filter.IsIncludedMember(referenceNode, topicTypeName, topicNamespaceName);
                                break;
                        }
                    }
                    if (included)
                        break;
                }

                if (included)
                    supportedPlatforms.Add(platformId);
            }
            platformsWriter.WriteStartElement("platforms");
            foreach (string platformId in supportedPlatforms)
            {
                platformsWriter.WriteElementString("platform", platformId);
            }
            platformsWriter.WriteEndElement();
            platformsWriter.Close();
        }

    }

// I can't be bothered to document all these right now so just ignore the warnings
#pragma warning disable 1591

    public abstract class InclusionFilter
    {
        protected InclusionFilter(string file)
        {
            sourceFiles.Add(file);
        }

        protected List<string> sourceFiles = new List<string>();

        protected static XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");
        protected static XPathExpression apiParameterNodesExpression = XPathExpression.Compile("parameters/parameter");
        protected static XPathExpression apiParameterTypeNameExpression = XPathExpression.Compile("string(.//type/@api)");
        protected static XPathExpression apiParameterTemplateNameExpression = XPathExpression.Compile("string(.//template/@name)");
    }

    public class VersionFilter : InclusionFilter
    {

        public VersionFilter(XmlReader platformReader, string id, string file)
            : base(file)
        {
            // platform/@version can only list included framework versions; excluding versions is not supported
            included = true;
            versionId = id;
            AddPlatformNode(platformReader, file);
        }

        public void AddPlatformNode(XmlReader platformReader, string file)
        {
            XmlReader subtree = platformReader.ReadSubtree();
            while (subtree.Read())
            {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "namespace"))
                {
                    string namespaceName = subtree.GetAttribute("name");

                    NamespaceFilter namespaceFilter;
                    if (!namespaceFilters.TryGetValue(namespaceName, out namespaceFilter))
                    {
                        namespaceFilter = new NamespaceFilter(subtree, file);
                    }
                    else
                    {
                        // if the version already has a filter for this namespace, add the data from the current namespace node
                        // unless the namespace node has a different @include value, in which case log a warning
                        string nsIncludeAttr = subtree.GetAttribute("include");
                        bool nsIncluded = Convert.ToBoolean(String.IsNullOrEmpty(nsIncludeAttr) ? "true" : nsIncludeAttr,
                            CultureInfo.InvariantCulture);
                        if (nsIncluded != namespaceFilter.Included)
                        {
                            // write warning message about conflicting filters
                            // ISSUE: how to invoke WriteMessage from here
                            Console.Write("");
                        }
                        else
                        {
                            namespaceFilter.AddNamespaceNode(subtree, file);
                        }
                    
                    }
                    namespaceFilters.Remove(namespaceName);
                    namespaceFilters.Add(namespaceName, namespaceFilter);
                }
            }
            subtree.Close();
        }

        private string versionId;

        // platform/@version can only list included framework versions; excluding versions is not supported
        private bool included;

        private Dictionary<string, NamespaceFilter> namespaceFilters = new Dictionary<string, NamespaceFilter>();

        public string VersionId
        {
            get
            {
                return (versionId);
            }
        }

        public Dictionary<string, NamespaceFilter> NamespaceFilters
        {
            get
            {
                return (namespaceFilters);
            }
        }

        /// <summary>
        /// If we get here, we know that the platform supports this version, and the api is included in this version.
        /// So returns true unless the type or its namespace are explicitly excluded by this version filter.
        /// </summary>
        /// <param name="referenceNode">The type's reflection data</param>
        /// <param name="topicNamespaceName">The topic namespace name</param>
        /// <returns>True if it is an included type, false if not</returns>
        public bool IsIncludedType(XPathNavigator referenceNode, string topicNamespaceName)
        {
            // if we have a filter for the topic's namespace, check it
            NamespaceFilter namespaceFilter;

            if(namespaceFilters.TryGetValue(topicNamespaceName, out namespaceFilter))
                return namespaceFilter.IsIncludedType(referenceNode);

            return included;
        }

        public bool IsIncludedMember(XPathNavigator referenceNode, string topicTypeName, string topicNamespaceName)
        {
            // if we have a filter for the topic's namespace, check it
            NamespaceFilter namespaceFilter;

            if (namespaceFilters.TryGetValue(topicNamespaceName, out namespaceFilter))
                return namespaceFilter.IsIncludedMember(referenceNode, topicTypeName);

            return included;
        }
    }

    public class NamespaceFilter : InclusionFilter
    {

        public NamespaceFilter(XmlReader namespaceReader, string file)
            : base(file)
        {
            //name = namespaceReader.GetAttribute("name");
            string includeAttr = namespaceReader.GetAttribute("include");
            included = Convert.ToBoolean(String.IsNullOrEmpty(includeAttr) ? "true" : includeAttr,
                CultureInfo.InvariantCulture);
            AddNamespaceNode(namespaceReader, file);
        }

        public void AddNamespaceNode(XmlReader namespaceReader, string file)
        {
            XmlReader subtree = namespaceReader.ReadSubtree();
            while (subtree.Read())
            {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "type"))
                {
                    string typeName = subtree.GetAttribute("name");

                    TypeFilter typeFilter;
                    if (!typeFilters.TryGetValue(typeName, out typeFilter))
                    {
                        typeFilter = new TypeFilter(subtree, file);
                    }
                    else
                    {
                        // if the namespace already has a filter for this type, add the data from the current type node
                        // unless the type node has a different @include value, in which case log a warning
                        string typeIncludeAttr = subtree.GetAttribute("include");
                        bool typeIncluded = Convert.ToBoolean(String.IsNullOrEmpty(typeIncludeAttr) ? "true" :
                            typeIncludeAttr, CultureInfo.InvariantCulture);

                        if (typeIncluded != typeFilter.Included)
                        {
                            // write warning message about conflicting filters
                            // ISSUE: how to invoke WriteMessage from here
                            Console.Write("");
                        }
                        else
                        {
                            typeFilter.AddTypeNode(subtree, file);
                        }
                    }
                    typeFilters.Remove(typeName);
                    typeFilters.Add(typeName, typeFilter);
                }
            }
            subtree.Close();
        }

        //private string name;
        
        private bool included;
        public bool Included
        {
            get
            {
                return (included);
            }
        }
        
        private Dictionary<string, TypeFilter> typeFilters = new Dictionary<string, TypeFilter>();
        public Dictionary<string, TypeFilter> TypeFilters
        {
            get
            {
                return (typeFilters);
            }
        }

        public bool IsIncludedType(XPathNavigator referenceNode)
        {
            // get the type's name
            string typeName = (string)referenceNode.Evaluate(apiNameExpression);

            // if we have a filter for that type, check it
            TypeFilter typeFilter;

            if (typeFilters.TryGetValue(typeName, out typeFilter))
                return typeFilter.Included;

            return included;
        }

        public bool IsIncludedMember(XPathNavigator referenceNode, string topicTypeName)
        {
            // if we have a filter for the type, check it
            TypeFilter typeFilter;

            if (typeFilters.TryGetValue(topicTypeName, out typeFilter))
                return typeFilter.IsIncludedMember(referenceNode);

            return included;
        }
    }

    public class TypeFilter : InclusionFilter
    {

        public TypeFilter(XmlReader typeReader, string file)
            : base(file)
        {
            //name = typeReader.GetAttribute("name");
            string includeAttr = typeReader.GetAttribute("include");
            included = Convert.ToBoolean(String.IsNullOrEmpty(includeAttr) ? "true" : includeAttr,
                CultureInfo.InvariantCulture);
            AddTypeNode(typeReader, file);
        }

        public void AddTypeNode(XmlReader typeReader, string file)
        {
            XmlReader subtree = typeReader.ReadSubtree();
            while (subtree.Read())
            {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "member"))
                {
                    string memberName = subtree.GetAttribute("name");

                    MemberFilter memberFilter;
                    if (!memberFilters.TryGetValue(memberName, out memberFilter))
                    {
                        memberFilter = new MemberFilter(subtree, file);
                    }
                    else
                    {
                        // if the type already has a filter for this member, add the data from the current member node
                        // unless the member node has a different @include value, in which case log a warning
                        string memberIncludeAttr = subtree.GetAttribute("include");
                        bool memberIncluded = Convert.ToBoolean(String.IsNullOrEmpty(memberIncludeAttr) ?
                            "true" : memberIncludeAttr, CultureInfo.InvariantCulture);
                        if (memberIncluded != memberFilter.Included)
                        {
                            // write warning message about conflicting filters
                            // ISSUE: how to invoke WriteMessage from here
                            Console.Write("");
                        }
                        else
                        {
                            memberFilter.AddMemberNode(subtree, file);
                        }
                    }
                    memberFilters.Remove(memberName);
                    memberFilters.Add(memberName, memberFilter);
                }
            }
            subtree.Close();
        }

        //private string name;
        
        private bool included;
        public bool Included
        {
            get
            {
                return (included);
            }
        }
        
        private Dictionary<string, MemberFilter> memberFilters = new Dictionary<string, MemberFilter>();
        public Dictionary<string, MemberFilter> MemberFilters
        {
            get
            {
                return (memberFilters);
            }
        }

        public bool IsIncludedMember(XPathNavigator referenceNode)
        {
            // get the member's name
            string memberName = (string)referenceNode.Evaluate(apiNameExpression);

            // if we have a filter for that member, check it
            MemberFilter memberFilter;

            if (memberFilters.TryGetValue(memberName, out memberFilter))
                return memberFilter.IsIncludedMember(referenceNode);

            return included;
        }
    }

    public class MemberFilter : InclusionFilter
    {

        public MemberFilter(XmlReader memberReader, string file)
            : base(file)
        {
            //name = memberReader.GetAttribute("name");

            string includeAttr = memberReader.GetAttribute("include");
            included = Convert.ToBoolean(String.IsNullOrEmpty(includeAttr) ? "true" : includeAttr,
                CultureInfo.InvariantCulture);
            AddMemberNode(memberReader, file);
        }

        public void AddMemberNode(XmlReader memberReader, string file)
        {
            XmlReader subtree = memberReader.ReadSubtree();
            while (subtree.Read())
            {
                if ((subtree.NodeType == XmlNodeType.Element) && (subtree.Name == "overload"))
                {
                    string paramTypes = subtree.GetAttribute("types");
                    string overloadIncludeAttr = subtree.GetAttribute("include");
                    bool overloadIncluded = Convert.ToBoolean(String.IsNullOrEmpty(overloadIncludeAttr) ?
                        "true" : overloadIncludeAttr, CultureInfo.InvariantCulture);

                    // check for existing overload filters that identify the same overload
                    bool alreadyFiltered = false;
                    foreach (OverloadFilter overloadFilter in overloadFilters)
                    {
                        if (!string.IsNullOrEmpty(paramTypes) && paramTypes == overloadFilter.ParamTypes)
                            alreadyFiltered = true;
                        if (alreadyFiltered && (overloadIncluded != overloadFilter.Included))
                        {
                            // write warning message about conflicting filters
                            // ISSUE: how to invoke WriteMessage from here
                            Console.Write("");
                        }
                    }
                    if (!alreadyFiltered)
                    {
                        OverloadFilter overloadFilter = new OverloadFilter(subtree, file);
                        overloadFilters.Add(overloadFilter);
                    }
                }
            }
            subtree.Close();
        }

        //private string name;

        private bool included;
        public bool Included
        {
            get
            {
                return (included);
            }
        }

        private List<OverloadFilter> overloadFilters = new List<OverloadFilter>();

        public bool IsIncludedMember(XPathNavigator referenceNode)
        {
            if (overloadFilters.Count == 0)
                return included;

            // get the member's paramNames string
            XPathNodeIterator parameterNodes = referenceNode.Select(apiParameterNodesExpression);

            StringBuilder paramNames = new StringBuilder();
            StringBuilder paramTypes = new StringBuilder();
            int i = 0;
            foreach (XPathNavigator parameterNode in parameterNodes)
            {
                i++;
                paramNames.Append(parameterNode.GetAttribute("name", string.Empty));
                if (i < parameterNodes.Count)
                    paramNames.Append(",");

                // BUGBUG: code here and in the psx conversion transform is a quick hack; make it better
                string arrayOf = (parameterNode.SelectSingleNode("arrayOf") == null) ? "" : "[]";
                string typeName = (string)parameterNode.Evaluate(apiParameterTypeNameExpression);
                if (string.IsNullOrEmpty(typeName))
                    typeName = (string)parameterNode.Evaluate(apiParameterTemplateNameExpression);

                int basenameStart = typeName.LastIndexOf(':') + 1;
                if (basenameStart > 0 && basenameStart < typeName.Length)
                    typeName = typeName.Substring(basenameStart);

                paramTypes.Append(typeName + arrayOf);
                if (i < parameterNodes.Count)
                    paramTypes.Append(",");
            }

            foreach (OverloadFilter overloadFilter in overloadFilters)
            {
                if (paramTypes.ToString() == overloadFilter.ParamTypes)
                    return overloadFilter.Included;
            }
            
            return included;
        }

    }

    public class OverloadFilter : InclusionFilter
    {

        public OverloadFilter(XmlReader overloadReader, string file)
            : base(file)
        {
            //name = overloadReader.GetAttribute("name");
            string includeAttr = overloadReader.GetAttribute("include");
            included = Convert.ToBoolean(string.IsNullOrEmpty(includeAttr) ? "true" : includeAttr,
                CultureInfo.InvariantCulture);

            overloadId = overloadReader.GetAttribute("api");
            paramTypes = overloadReader.GetAttribute("types");
            paramNames = overloadReader.GetAttribute("names");
        }

        //private string name;

        private bool included;
        public bool Included
        {
            get
            {
                return (included);
            }
        }

        private string paramTypes;
        public string ParamTypes
        {
            get
            {
                return (paramTypes);
            }
        }

        private string paramNames;
        public string ParamNames
        {
            get
            {
                return (paramNames);
            }
        }
        private string overloadId;
        public string OverloadId
        {
            get
            {
                return (overloadId);
            }
        }

    }

#pragma warning restore 1591
}
