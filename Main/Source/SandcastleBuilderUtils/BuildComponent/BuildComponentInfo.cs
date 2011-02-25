//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildComponentInfo.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/03/2008
// Note    : Copyright 2007-2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the class used to provide information about the loaded
// build components.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.6.0.1  11/01/2007  EFW  Created the code
// 1.6.0.7  05/03/2008  EFW  Added support for conceptual build configurations
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.XPath;

namespace SandcastleBuilder.Utils.BuildComponent
{
    /// <summary>
    /// This class contains information about the loaded build components.
    /// </summary>
    public class BuildComponentInfo
    {
        #region Private data members
        //=====================================================================
        // Private data members

        private string id, typeName, assemblyPath, copyright, description,
            configureMethod, defaultConfig, invalidReason;
        private Version version;
        private bool isValid, isHidden;
        private ComponentPosition referencePosition, conceptualPosition;
        private ReadOnlyCollection<string> dependencies;
        #endregion

        #region Properties
        //=====================================================================
        // Properties

        /// <summary>
        /// This read-only property returns the ID of the build component
        /// </summary>
        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// This read-only property returns the type name of the component
        /// </summary>
        public string TypeName
        {
            get { return typeName; }
        }

        /// <summary>
        /// This read-only property returns the path to the assembly containing
        /// the component
        /// </summary>
        public string AssemblyPath
        {
            get { return assemblyPath; }
        }

        /// <summary>
        /// This read-only property returns copyright information for the
        /// component.
        /// </summary>
        public string Copyright
        {
            get { return copyright; }
        }

        /// <summary>
        /// This read-only property returns version information for the
        /// component.
        /// </summary>
        public Version Version
        {
            get { return version; }
        }

        /// <summary>
        /// This read-only property returns a description of the component
        /// </summary>
        public string Description
        {
            get { return description; }
        }

        /// <summary>
        /// This read-only property returns the hidden flag
        /// </summary>
        /// <remarks>If hidden, the component will not be selectable from
        /// within the help file builder.</remarks>
        public bool IsHidden
        {
            get { return isHidden; }
        }

        /// <summary>
        /// This read-only property returns true if the component configuration
        /// is valid or false if it is not.
        /// </summary>
        /// <remarks>If not valid, the component cannot be used.</remarks>
        public bool IsValid
        {
            get { return isValid; }
        }

        /// <summary>
        /// If not valid, this read-only property returns the reason
        /// </summary>
        public string InvalidReason
        {
            get { return invalidReason; }
        }

        /// <summary>
        /// This read-only property returns the method name used to configure
        /// the component interactively from within the help file builder.
        /// </summary>
        /// <remarks>If not specifed, a default editor will be used to allow
        /// modifying the raw XML configuration text.</remarks>
        public string ConfigureMethod
        {
            get { return configureMethod; }
        }

        /// <summary>
        /// This read-only property returns the default configuration for
        /// the component.
        /// </summary>
        /// <remarks>This will include the enclosing &lt;component&gt; tag.</remarks>
        public string DefaultConfiguration
        {
            get { return defaultConfig; }
        }

        /// <summary>
        /// This read-only property returns the position of the component in
        /// the reference build configuration file (sandcastle.config).
        /// </summary>
        /// <remarks>If not defined, it will not be used.</remarks>
        public ComponentPosition ReferenceBuildPosition
        {
            get { return referencePosition; }
        }

        /// <summary>
        /// This read-only property returns the position of the component in
        /// the conceptual build configuration file (conceptual.config).
        /// </summary>
        /// <remarks>If not defined, it will not be used.</remarks>
        public ComponentPosition ConceptualBuildPosition
        {
            get { return conceptualPosition; }
        }

        /// <summary>
        /// This read-only property returns the collection of component IDs
        /// on which this component depends.
        /// </summary>
        public ReadOnlyCollection<string> Dependencies
        {
            get { return dependencies; }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="component">The XPath navigator containing the
        /// component's configuration information</param>
        internal BuildComponentInfo(XPathNavigator component)
        {
            StringBuilder sb = new StringBuilder();
            FileVersionInfo fvi;
            XPathNavigator item;
            string asmPath, attrValue;

            isValid = true;

            // Load the configuration information from the node
            try
            {
                attrValue = component.GetAttribute("type", String.Empty);

                if(String.IsNullOrEmpty(attrValue))
                {
                    isValid = false;
                    typeName = "??";
                    invalidReason = "Missing type name attribute";
                }
                else
                    typeName = attrValue;

                // If no ID, use the type name
                attrValue = component.GetAttribute("id", String.Empty);

                if(!String.IsNullOrEmpty(attrValue))
                    id = attrValue;
                else
                    id = typeName;

                attrValue = component.GetAttribute("assembly", String.Empty);

                if(String.IsNullOrEmpty(attrValue))
                {
                    isValid = false;
                    assemblyPath = "??";
                    invalidReason = "Missing assembly attribute";
                }
                else
                {
                    assemblyPath = attrValue;

                    // Try to get copyright and version information
                    asmPath = BuildComponentManager.ResolveComponentPath(
                        assemblyPath);

                    if(File.Exists(asmPath))
                    {
                        fvi = FileVersionInfo.GetVersionInfo(asmPath);
                        version = new Version(fvi.FileVersion);
                        copyright = fvi.LegalCopyright;
                    }
                    else
                    {
                        version = new Version("0.0.0.0");
                        copyright = "?? - Unable to locate assembly";
                        isValid = false;
                        invalidReason = "The build component assembly " +
                            asmPath + " could not be found";
                    }
                }

                // If no description use the type name and assembly path
                item = component.SelectSingleNode("description");
                if(item != null)
                    description = item.Value.Trim();
                else
                    description = typeName + ", " + assemblyPath;

                if(component.SelectSingleNode("hidden") != null)
                    isHidden = true;

                item = component.SelectSingleNode("configureMethod");
                if(item != null)
                {
                    attrValue = item.GetAttribute("name", String.Empty);

                    if(!String.IsNullOrEmpty(attrValue))
                        configureMethod = attrValue;
                }

                // The default configuration is wrapped in a <component> tag
                item = component.SelectSingleNode("defaultConfiguration");

                // We need to retain custom attributes on the component as well
                // as the standard id, type, and assembly attributes.
                sb.Append("<component");

                if(component.MoveToFirstAttribute())
                {
                    do
                    {
                        sb.AppendFormat(" {0}=\"{1}\"", component.Name,
                            component.Value);
                    } while(component.MoveToNextAttribute());

                    component.MoveToParent();
                }

                sb.AppendFormat(CultureInfo.InvariantCulture,
                    ">\r\n{0}\r\n</component>",
                    (item == null) ? String.Empty : item.InnerXml);

                defaultConfig = sb.ToString();

                item = component.SelectSingleNode("insert");
                if(item != null)
                    referencePosition = new ComponentPosition(item);
                else
                    referencePosition = new ComponentPosition();

                item = component.SelectSingleNode("insertConceptual");
                if(item != null)
                    conceptualPosition = new ComponentPosition(item);
                else
                    conceptualPosition = new ComponentPosition();

                List<string> deps = new List<string>();

                foreach(XPathNavigator dep in component.Select(
                  "dependencies/component"))
                {
                    attrValue = dep.GetAttribute("id", String.Empty);
                    if(!String.IsNullOrEmpty(attrValue))
                        deps.Add(attrValue);
                }

                dependencies = new ReadOnlyCollection<string>(deps);
            }
            catch(Exception ex)
            {
                isValid = false;
                invalidReason = ex.Message;
            }
        }
    }
}
