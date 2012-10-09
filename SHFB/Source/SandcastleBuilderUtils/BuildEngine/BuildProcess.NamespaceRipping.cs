//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.NamespaceRipping.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/30/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to generate the API filter collection
// information used by MRefBuilder to exclude API entries while generating the
// reflection information file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.5.0.2  07/16/2007  EFW  Created the code
// 1.5.2.0  09/13/2007  EFW  Added support for calling plug-ins
// 1.6.0.6  03/10/2008  EFW  Added support for applying the API filter manually
//=============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================
        // Private data members

        // The API filter collection
        private ApiFilterCollection apiFilter;

        /// <summary>
        /// This read-only property returns the API filter that is used at
        /// build-time to filter the API elements.
        /// </summary>
        /// <remarks>This is a combination of the project's API filter,
        /// namespace exclusions, and <code>&lt;exclude /&gt;</code> tag
        /// exclusions.</remarks>
        public ApiFilterCollection BuildApiFilter
        {
            get { return apiFilter; }
        }
        #endregion

        #region Automatic API filter methods
        //=====================================================================

        /// <summary>
        /// This is used to generate the API filter collection used by
        /// MRefBuilder to exclude items from the reflection information file.
        /// </summary>
        /// <remarks>Namespaces and members with an <code>&lt;exclude /&gt;</code>
        /// tag in their comments are removed using the ripping feature as it
        /// is more efficient than searching for and removing them from the
        /// reflection file after it has been generated especially on large
        /// projects.</remarks>
        private void GenerateApiFilter()
        {
            XmlNodeList excludes;
            XmlNode docMember;
            List<string> ripList;
            string nameSpace, memberName, typeName, fullName;
            int pos;

            this.ReportProgress(BuildStep.GenerateApiFilter,
                "Generating API filter for MRefBuilder...");

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            ripList = new List<string>();

            // Add excluded namespaces
            foreach(NamespaceSummaryItem ns in project.NamespaceSummaries)
                if(!ns.IsDocumented)
                {
                    memberName = ns.Name;

                    if(memberName[0] == '(')
                        memberName = "N:";  // Global namespace
                    else
                        memberName = "N:" + memberName;

                    ripList.Add(memberName);
                }

            // If the namespace summaries don't contain an explicit entry
            // for the global namespace, exclude it by default.
            if(project.NamespaceSummaries[null] == null)
                ripList.Add("N:");

            // Add members excluded via comments
            foreach(XmlCommentsFile comments in commentsFiles)
            {
                excludes = comments.Members.SelectNodes("//exclude/..");

                foreach(XmlNode member in excludes)
                {
                    // It should appear at the same level as <summary> so that
                    // we can find the member name in the parent node.
                    if(member.Attributes["name"] == null)
                    {
                        this.ReportProgress("    Incorrect placement of " +
                            "<exclude/> tag.  Unable to locate member name.");
                        continue;
                    }

                    memberName = member.Attributes["name"].Value;

                    if(!ripList.Contains(memberName))
                        ripList.Add(memberName);
                }
            }

            // Sort by entry type and name so that we create the collection
            // from the namespace down to the members.
            ripList.Sort(
                delegate(string x, string y)
                {
                    ApiEntryType xType = ApiFilter.ApiEntryTypeFromLetter(x[0]),
                        yType = ApiFilter.ApiEntryTypeFromLetter(y[0]);

                    if(xType == yType)
                        return String.Compare(x, y, false,
                            CultureInfo.CurrentCulture);

                    return (int)xType - (int)yType;
                });

            // Clone the project ApiFilter and merge the members from the
            // rip list.
            apiFilter = (ApiFilterCollection)project.ApiFilter.Clone();

            // For the API filter to work, we have to nest the entries by
            // namespace, type, and member.  As such, we have to break apart
            // what we've got in the list and merge it with the stuff the user
            // may have specified using the project's API filter property.
            foreach(string member in ripList)
            {
                // Namespaces are easy
                if(member[0] == 'N')
                {
                    if(!apiFilter.MergeEntry(ApiEntryType.Namespace,
                      member.Substring(2), false, true))
                        this.ReportWarning("BE0008", "Namespace '{0}' " +
                            "excluded via namespace comments conflicted with " +
                            "API filter setting.  Exclusion ignored.", member);

                    continue;
                }

                // Types and members are a bit tricky.  Since we don't have any
                // real context, we have to assume that we can remove the last
                // part and look it up.  If a type entry isn't found, we can
                // assume it's the namespace.  Where this can fail is on a
                // nested class where the parent class is lacking XML comments.
                // Not much we can do about it in that case.
                if(member[0] == 'T')
                {
                    fullName = nameSpace = member;
                    typeName = member.Substring(2);
                    memberName = null;
                }
                else
                {
                    // Strip parameters.  The ripping feature only goes to
                    // the name level.  If one overload is ripped, they are
                    // all ripped.
                    pos = member.IndexOf('(');

                    if(pos != -1)
                        fullName = memberName = member.Substring(0, pos);
                    else
                        fullName = memberName = member;

                    // Generic method
                    pos = memberName.IndexOf("``", StringComparison.Ordinal);

                    if(pos != -1)
                        memberName = memberName.Substring(0, pos);

                    pos = memberName.LastIndexOf('.');
                    memberName = memberName.Substring(pos + 1);
                    typeName = fullName.Substring(2, pos - 2);
                    nameSpace = "T:" + typeName;
                }

                for(int idx = 0; idx < commentsFiles.Count; idx++)
                {
                    docMember = commentsFiles[idx].Members.SelectSingleNode(
                        "member[@name='" + nameSpace + "']");

                    if(docMember != null)
                    {
                        pos = nameSpace.LastIndexOf('.');

                        if(pos == -1)
                        {
                            nameSpace = "N:";
                            break;
                        }
                        else
                            nameSpace = nameSpace.Substring(0, pos);

                        idx = -1;
                    }
                }

                nameSpace = nameSpace.Substring(2);

                // If the names still match, we probably didn't find comments
                // for the type so assume the namespace is the part up to
                // the last period.
                if(nameSpace == typeName)
                {
                    pos = nameSpace.LastIndexOf('.');

                    if(pos != -1)
                        nameSpace = nameSpace.Substring(0, pos);
                    else
                        nameSpace = "N:";   // Global namespace
                }

                if(apiFilter.AddNamespaceChild(fullName, nameSpace, typeName,
                  memberName))
                {
                    if(fullName.Length > 2)
                    {
                        // If it's a nested class, adjust the filter name
                        fullName = typeName;
                        typeName = typeName.Substring(nameSpace.Length + 1);

                        if(typeName.IndexOf('.') != -1)
                            foreach(ApiFilter ns in apiFilter)
                                if(ns.FullName == nameSpace)
                                {
                                    foreach(ApiFilter t in ns.Children)
                                        if(t.FullName == fullName)
                                        {
                                            t.FilterName = typeName;
                                            break;
                                        }

                                    break;
                                }
                    }
                }
                else
                    this.ReportWarning("BE0009", "'{0}' is marked with " +
                        "<exclude /> but conflicted with the API filter " +
                        "setting.  Exclusion ignored.", member);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
        }
        #endregion

        #region Manual API filter methods
        //=====================================================================

        /// <summary>
        /// This is used to manually apply the specified API filter to the
        /// specified reflection information file.
        /// </summary>
        /// <param name="apiFilter">The API filter to apply</param>
        /// <param name="reflectionFilename">The reflection information file</param>
        /// <remarks>This can be used by any plug-in that does not produce a
        /// reflection information file using <b>MRefBuilder.exe</b>.  In such
        /// cases, the API filter is not applied unless the plug-in uses this
        /// method.  If the reflection information file is produced by
        /// <b>MRefBuilder.exe</b>, there is no need to use this method as it
        /// will apply the API filter automatically to the file that it
        /// produces.</remarks>
        public void ApplyManualApiFilter(ApiFilterCollection apiFilter,
          string reflectionFilename)
        {
            XmlDocument refInfo;
            XmlNode apis;
            string id;
            bool keep;

            refInfo = new XmlDocument();
            refInfo.Load(reflectionFilename);
            apis = refInfo.SelectSingleNode("reflection/apis");

            foreach(ApiFilter nsFilter in apiFilter)
                if(nsFilter.Children.Count == 0)
                    this.RemoveNamespace(apis, nsFilter.FullName);
                else
                    if(!nsFilter.IsExposed)
                    {
                        // Remove all but the indicated types
                        foreach(XmlNode typeNode in apis.SelectNodes(
                          "api[starts-with(@id, 'T:') and containers/" +
                          "namespace/@api='N:" + nsFilter.FullName + "']"))
                        {
                            id = typeNode.Attributes["id"].Value.Substring(2);
                            keep = false;

                            foreach(ApiFilter typeFilter in nsFilter.Children)
                                if(typeFilter.FullName == id)
                                {
                                    // Just keep or remove members
                                    this.ApplyMemberFilter(apis, typeFilter);
                                    keep = true;
                                    break;
                                }

                            if(!keep)
                                this.RemoveType(apis, id);
                        }
                    }
                    else
                    {
                        // Remove just the indicated types or their members
                        foreach(ApiFilter typeFilter in nsFilter.Children)
                        {
                            if(!typeFilter.IsExposed &&
                              typeFilter.Children.Count == 0)
                            {
                                this.RemoveType(apis, typeFilter.FullName);
                                continue;
                            }

                            // Just keep or remove members
                            this.ApplyMemberFilter(apis, typeFilter);
                        }
                    }

            refInfo.Save(reflectionFilename);
        }

        /// <summary>
        /// Apply a member filter to the specified type.
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="typeFilter">The type filter to be processed</param>
        private void ApplyMemberFilter(XmlNode apis, ApiFilter typeFilter)
        {
            string id;
            bool keep;
            int pos;

            if(!typeFilter.IsExposed)
            {
                // Remove all but the indicated members
                foreach(XmlNode memberNode in apis.SelectNodes(
                  "api[containers/type/@api='T:" + typeFilter.FullName + "']"))
                {
                    id = memberNode.Attributes["id"].Value.Substring(2);
                    pos = id.IndexOf('(');
                    keep = false;

                    // The API filter ignores parameters on methods
                    if(pos != -1)
                        id = id.Substring(0, pos);

                    foreach(ApiFilter memberFilter in typeFilter.Children)
                        if(memberFilter.FullName == id)
                        {
                            keep = true;
                            break;
                        }

                    if(!keep)
                    {
                        id = memberNode.Attributes["id"].Value;
                        this.ReportProgress("    Removing member '{0}'", id);
                        memberNode.ParentNode.RemoveChild(memberNode);

                        // Remove the element nodes too
                        foreach(XmlNode element in apis.SelectNodes(
                          "api/elements/element[@api='" + id + "']"))
                            element.ParentNode.RemoveChild(element);
                    }
                }
            }
            else
            {
                // Remove just the indicated members
                foreach(ApiFilter memberFilter in typeFilter.Children)
                    foreach(XmlNode memberNode in apis.SelectNodes(
                      "api[starts-with(substring-after(@id,':'),'" +
                      memberFilter.FullName + "')]"))
                    {
                        id = memberNode.Attributes["id"].Value.Substring(2);
                        pos = id.IndexOf('(');

                        // The API filter ignores parameters on methods
                        if(pos != -1)
                            id = id.Substring(0, pos);

                        if(id == memberFilter.FullName)
                        {
                            id = memberNode.Attributes["id"].Value;
                            this.ReportProgress("    Removing member '{0}'",
                                id);
                            memberNode.ParentNode.RemoveChild(memberNode);

                            // Remove the element nodes too
                            foreach(XmlNode element in apis.SelectNodes(
                              "api/elements/element[@api='" + id + "']"))
                                element.ParentNode.RemoveChild(element);
                        }
                    }
            }
        }

        /// <summary>
        /// Remove an entire namespace and all of its members
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="id">The namespace ID to remove</param>
        private void RemoveNamespace(XmlNode apis, string id)
        {
            XmlNode ns;
            string nodeId;

            this.ReportProgress("    Removing namespace 'N:{0}'", id);
            ns = apis.SelectSingleNode("api[@id='N:" + id + "']");

            if(ns != null)
            {
                // Remove the namespace container
                ns.ParentNode.RemoveChild(ns);

                // Remove all of the namespace members
                foreach(XmlNode xn in apis.SelectNodes(
                  "api[containers/namespace/@api='N:" + id + "']"))
                {
                    xn.ParentNode.RemoveChild(xn);

                    // Remove the element nodes too
                    nodeId = xn.Attributes["id"].Value;

                    foreach(XmlNode element in apis.SelectNodes(
                      "api/elements/element[@api='" + nodeId + "']"))
                        element.ParentNode.RemoveChild(element);
                }
            }
        }

        /// <summary>
        /// Remove an entire type and all of its members
        /// </summary>
        /// <param name="apis">The APIs node from which to remove info</param>
        /// <param name="id">The type ID to remove</param>
        private void RemoveType(XmlNode apis, string id)
        {
            XmlNode typeNode;
            string nodeId;

            this.ReportProgress("    Removing type 'T:{0}'", id);
            typeNode = apis.SelectSingleNode("api[@id='T:" + id + "']");

            if(typeNode != null)
            {
                // Remove the namespace container
                typeNode.ParentNode.RemoveChild(typeNode);

                // Remove all of the type members
                foreach(XmlNode xn in apis.SelectNodes(
                  "api[containers/type/@api='T:" + id + "']"))
                {
                    xn.ParentNode.RemoveChild(xn);

                    // Remove the element nodes too
                    nodeId = xn.Attributes["id"].Value;

                    foreach(XmlNode element in apis.SelectNodes(
                      "api/elements/element[@api='" + nodeId + "']"))
                        element.ParentNode.RemoveChild(element);
                }

                // Remove namespace element nodes
                foreach(XmlNode element in apis.SelectNodes(
                  "api/elements/element[@api='T:" + id + "']"))
                    element.ParentNode.RemoveChild(element);
            }
        }
        #endregion
    }
}
