//===============================================================================================================
// System  : Sandcastle Tools - Add Namespace Groups Utility
// File    : AddNamespaceGroupsCore.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/06/2015
// Note    : Copyright 2013-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This utility is used to add namespace groups to a reflection data file.  The namespace groups can be used to
// combine namespaces with a common root into entries in the table of contents in the generated help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.   This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Date        Who  Comments
// ==============================================================================================================
// 12/07/2013  EFW  Created the code
// 03/12/2014  EFW  Updated to merge sub-groups into the parent if they are the only child of the parent group
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.CommandLine;

namespace Microsoft.Ddue.Tools
{
    public static class AddNamespaceGroupsCore
    {
        #region Namespace group class
        //=====================================================================

        /// <summary>
        /// This is used to keep track of the namespace groups and their children
        /// </summary>
        private class NamespaceGroup
        {
            private List<string> children;

            /// <summary>
            /// This is used to get or set the namespace name
            /// </summary>
            public string Namespace { get; set; }

            /// <summary>
            /// This read-only property returns a list of the child namespaces if this is a group
            /// </summary>
            /// <remarks>If empty, this is a normal namespace entry</remarks>
            public List<string> Children
            {
                get { return children; }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            public NamespaceGroup()
            {
                children = new List<string>();
            }
        }
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the canceled state of the process
        /// </summary>
        /// <value>If set to true, the process is stopped as soon as possible</value>
        public static bool Canceled { get; set; }

        #endregion

        #region Main program entry point
        //=====================================================================

        /// <summary>
        /// Main program entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Zero on success, a non-zero value on failure</returns>
        public static int Main(string[] args)
        {
            List<string> namespaces = new List<string>();
            XPathNavigator projectRoot = null;
            int maxParts = 2, groupCount = 0;

            ConsoleApplication.WriteBanner();

            OptionCollection options = new OptionCollection {
                new SwitchOption("?", "Show this help page."),
                new StringOption("out", "Specify an output filename. If unspecified, output goes to the " +
                    "console.", "outputFile"),
                new StringOption("maxParts", "Specify the maximum number of namespace parts to consider " +
                    "when creating groups.  A higher value creates more groups.  The default (and minimum) " +
                    "is 2.", "999")
            };

            ParseArgumentsResult result = options.ParseArguments(args);

            if(result.Options["?"].IsPresent)
            {
                Console.WriteLine("AddNamespaceGroups [options] reflectionDataFile");
                options.WriteOptionSummary(Console.Out);
                return 0;
            }

            if(!result.Success)
            {
                result.WriteParseErrors(Console.Out);
                return 1;
            }

            if(result.UnusedArguments.Count != 1)
            {
                Console.WriteLine("Specify one reflection data file.");
                return 1;
            }

            if(result.Options["maxParts"].IsPresent && !Int32.TryParse((string)result.Options["maxParts"].Value, out maxParts))
                maxParts = 0;

            if(maxParts < 2)
                ConsoleApplication.WriteMessage(LogLevel.Warn, "maxParts option value is not valid.  It must " +
                    "be a valid integer with a minimum value of 2");

            // Get a text writer for output
            TextWriter output = Console.Out;

            if(result.Options["out"].IsPresent)
            {
                string file = (string)result.Options["out"].Value;

                try
                {
                    output = new StreamWriter(file, false, Encoding.UTF8);
                }
                catch(IOException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to " +
                        "create an output file. The error message is: {0}", e.Message);
                    return 1;
                }
                catch(UnauthorizedAccessException e)
                {
                    ConsoleApplication.WriteMessage(LogLevel.Error, "An error occurred while attempting to " +
                        "create an output file. The error message is: {0}", e.Message);
                    return 1;
                }
            }

            try
            {
                XPathDocument source = new XPathDocument(result.UnusedArguments[0]);

                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.CloseOutput = result.Options["out"].IsPresent;

                using(XmlWriter xw = XmlWriter.Create(output, settings))
                {
                    // Copy the reflection element
                    xw.WriteStartDocument();
                    xw.WriteStartElement("reflection");

                    var reflection = source.CreateNavigator().SelectSingleNode("reflection");
                    var elementNamespaces = reflection.GetNamespacesInScope(XmlNamespaceScope.ExcludeXml);

                    foreach(var ns in elementNamespaces.Keys.Reverse())
                        xw.WriteAttributeString("xmlns", ns, null, elementNamespaces[ns]);

                    if(reflection.MoveToFirstAttribute())
                    {
                        do
                        {
                            xw.WriteAttributeString(reflection.Prefix, reflection.Name, null, reflection.Value + "X");

                        } while(reflection.MoveToNextAttribute());

                        reflection.MoveToParent();
                    }

                    // Copy assembly elements
                    var assemblies = reflection.SelectSingleNode("assemblies");

                    if(assemblies != null)
                        assemblies.WriteSubtree(xw);

                    // Copy the API elements and track all of the namespace elements
                    xw.WriteStartElement("apis");

                    foreach(XPathNavigator api in reflection.Select("apis/api"))
                    {
                        string id = (string)api.Evaluate("string(@id)");

                        if(id != null && id.Length > 1 && id[1] == ':' && (id[0] == 'N' || id[0] == 'R'))
                        {
                            if(id.StartsWith("N:", StringComparison.Ordinal))
                            {
                                namespaces.Add(id);
                                api.WriteSubtree(xw);
                            }
                            else
                                projectRoot = api;      // Project root node gets replaced if present
                        }
                        else
                            api.WriteSubtree(xw);

                        if(Canceled)
                            break;
                    }

                    // Group namespaces and write out the group entries
                    foreach(var group in GroupNamespaces(namespaces, maxParts, (projectRoot != null)))
                    {
                        if(Canceled)
                            break;

                        if(group.Namespace.Length == 0)
                        {
                            // If the namespace is blank, it's the root group.  If a project root element was
                            // specified, replace its element list with the one from this group.  If no project
                            // root element was found, write the root group out as a placeholder for the TOC
                            // transformation so that it can determine the root level content.
                            if(projectRoot != null)
                            {
                                xw.WriteStartElement("api");
                                xw.WriteAttributeString("id", projectRoot.GetAttribute("id", String.Empty));

                                projectRoot.MoveToChild("topicdata", String.Empty);
                                projectRoot.WriteSubtree(xw);

                                xw.WriteStartElement("elements");

                                foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                                {
                                    xw.WriteStartElement("element");
                                    xw.WriteAttributeString("api", child);
                                    xw.WriteEndElement();
                                }

                                xw.WriteEndElement();   // elements
                                xw.WriteEndElement();   // api
                            }
                            else
                            {
                                xw.WriteStartElement("api");
                                xw.WriteAttributeString("id", "G:");

                                xw.WriteStartElement("topicdata");
                                xw.WriteAttributeString("group", "rootGroup");
                                xw.WriteEndElement();

                                xw.WriteStartElement("elements");

                                foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                                {
                                    xw.WriteStartElement("element");
                                    xw.WriteAttributeString("api", child);
                                    xw.WriteEndElement();
                                }

                                xw.WriteEndElement();   // elements
                                xw.WriteEndElement();   // api
                            }
                        }
                        else
                        {
                            groupCount++;

                            xw.WriteStartElement("api");
                            xw.WriteAttributeString("id", group.Namespace);

                            xw.WriteStartElement("topicdata");
                            xw.WriteAttributeString("group", "api");
                            xw.WriteEndElement();

                            xw.WriteStartElement("apidata");
                            xw.WriteAttributeString("name", group.Namespace.Substring(2));
                            xw.WriteAttributeString("group", "namespaceGroup");
                            xw.WriteEndElement();

                            xw.WriteStartElement("elements");

                            foreach(string child in group.Children.OrderBy(n => n.Substring(2)))
                            {
                                xw.WriteStartElement("element");
                                xw.WriteAttributeString("api", child);
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();   // elements
                            xw.WriteEndElement();   // api
                        }
                    }

                    xw.WriteEndElement();   // apis

                    xw.WriteEndElement();   // reflection
                    xw.WriteEndDocument();
                }

                if(!Canceled)
                    ConsoleApplication.WriteMessage(LogLevel.Info, "Added {0} namespace group entries", groupCount);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unexpected error adding namespace groups to reflection data.  Reason: {0}",
                    ex.Message);
                return 1;
            }

            return Canceled ? 1 : 0;
        }
        #endregion

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to group the namespaces based on their common root
        /// </summary>
        /// <param name="namespaces">An enumerable list of namespaces to group.</param>
        /// <param name="maxParts">The maximum number of namespace parts to consider for grouping.</param>
        /// <param name="hasRootNamespaceContainer">True if the project has a root namespace container, false
        /// if not.  This controls whether or not the root group is retained.</param>
        /// <returns>An enumerable list of the grouped namespaces</returns>
        private static IEnumerable<NamespaceGroup> GroupNamespaces(IEnumerable<string> namespaces, int maxParts,
          bool hasRootNamespaceContainer)
        {
            Dictionary<string, NamespaceGroup> groups = new Dictionary<string,NamespaceGroup>();
            NamespaceGroup match;
            string[] parts;
            string root;
            int partCount;

            // This serves as the root group.  If a project element is present in the reflection file, its
            // list of elements will be replaced by the list in this group.  If not, this group is written to
            // the file to serve as a place holder for the TOC XSL transformation.
            groups.Add(String.Empty, new NamespaceGroup { Namespace = String.Empty });

            // Iterate over the namespaces so that we can figure out the common root namespaces
            foreach(string space in namespaces.Reverse())
            {
                parts = space.Split('.');
                partCount = parts.Length >= maxParts ? maxParts : parts.Length - 1;

                if(parts.Length - 1 <= partCount)
                    root = String.Join(".", parts, 0, parts.Length - 1);
                else
                    root = String.Join(".", parts, 0, partCount);

                // Create a new group to represent the namespace if there are child namespaces and it is not
                // there already.  A group is only created if it will contain more than one namespace. Namespaces
                // without any children will end up in their parent as a standard namespace entry.
                if(root.Length > 2 && !groups.ContainsKey(root) &&
                  namespaces.Count(n => n.StartsWith(root + ".", StringComparison.Ordinal)) > 1)
                    groups.Add(root, new NamespaceGroup { Namespace = root });
            }

            // Now place the namespaces in the appropriate group.  Include the group keys as they may not be
            // represented by an actual namespace.
            foreach(string space in namespaces.Concat(groups.Keys.Where(k => k.Length != 0)).GroupBy(
              n => n).Select(n => n.Key))
            {
                parts = space.Split('.');
                partCount = parts.Length >= maxParts ? maxParts : parts.Length - 1;

                while(partCount > -1)
                {
                    if(parts.Length - 1 <= partCount)
                        root = String.Join(".", parts, 0, parts.Length - 1);
                    else
                        root = String.Join(".", parts, 0, partCount);

                    if(groups.TryGetValue(root, out match))
                    {
                        match.Children.Add(space);
                        break;
                    }

                    // If not found (group key with no namespace), remove the last part and try again.  If all
                    // else fails, it'll end up in the root group.
                    partCount--;
                }
            }

            // Make a pass through the groups.  Convert each child that is a group key to a group reference.
            foreach(var kv in groups)
            {
                var children = kv.Value.Children;

                for(int idx = 0; idx < children.Count; idx++)
                    if(groups.Keys.Contains(children[idx]))
                        children[idx] = "G" + children[idx].Substring(1);
            }

            // If a group only contains one group entry, pull that sub-group up into the parent and remove
            // the sub-group.  Don't do it for the root namespace container or if the namespace itself will
            // appear with the group entry.
            foreach(var group in groups.ToList())
                if((group.Key.Length != 0 || hasRootNamespaceContainer)  && group.Value.Children.Count == 1 &&
                  group.Value.Children[0][0] == 'G' && !namespaces.Contains(group.Value.Namespace))
                {
                    root = "N" + group.Value.Children[0].Substring(1);

                    // Ignore it if already removed
                    if(groups.ContainsKey(group.Key))
                    {
                        if(namespaces.Contains(root))
                            group.Value.Children.Add(root);

                        group.Value.Children.RemoveAt(0);
                        group.Value.Children.AddRange(groups[root].Children);
                        groups.Remove(root);
                    }
                }

            var rootGroup = groups[String.Empty];
            root = "N" + rootGroup.Children[0].Substring(1);

            if(groups.Count > 1 && rootGroup.Children.Count == 1 && hasRootNamespaceContainer)
            {
                // Special case.  If we've got a root group with only one child and there is a root namespace
                // container, pull the content for the group into the root and remove the child.
                if(namespaces.Contains(root))
                    rootGroup.Children.Add(root);

                rootGroup.Children.RemoveAt(0);
                rootGroup.Children.AddRange(groups[root].Children);
                groups.Remove(root);
            }
            else
                if(groups.Count > 1 && rootGroup.Children.Count == 1 && !namespaces.Contains(root))
                {
                    // Special case.  If the root group doesn't exist as a namespace and only has one child, pull
                    // the content for the group into the root and remove the child.  However, if the group has
                    // more than one namespace starting with the first namespace, keep the group and name it
                    // after the first entry.
                    var childGroup = groups[root];

                    string name = childGroup.Children[0] + ".";

                    if(!childGroup.Children.Skip(1).All(c => c.StartsWith(name, StringComparison.Ordinal)))
                    {
                        rootGroup.Children.RemoveAt(0);
                        rootGroup.Children.AddRange(childGroup.Children);
                        groups.Remove(root);
                    }
                    else
                    {
                        childGroup.Namespace = name.Substring(0, name.Length - 1);
                        rootGroup.Children.RemoveAt(0);
                        rootGroup.Children.Add("G" + childGroup.Namespace.Substring(1));
                    }
                }

            // In the final pass, for each group key that is a namespace, add the namespace to its children.
            // Also change the name of the namespace group.  Once done, return it to the caller.
            foreach(var group in groups.Values)
            {
                if(namespaces.Contains(group.Namespace) && !group.Children.Contains(group.Namespace))
                    group.Children.Add(group.Namespace);

                if(group.Namespace.Length != 0)
                    group.Namespace = "G" + group.Namespace.Substring(1);

                yield return group;
            }
        }
        #endregion
    }
}
