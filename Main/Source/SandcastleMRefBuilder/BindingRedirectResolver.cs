//=============================================================================
// System  : Sandcastle Help File Builder MRefBuilder Components
// File    : CustomAssemblyResolver.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 11/19/2008
// Note    : Copyright 2008, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a custom assembly resolver for Sandcastle's MRefBuilder
// that supports assembly binding redirect elements in its configuration that
// let you redirect an unknown assembly's strong name to another when resolving
// an unknown reference.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
// 1.8.0.1  11/14/2008  EFW  Created the code
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Xml.XPath;

using System.Compiler;
using Microsoft.Ddue.Tools.CommandLine;
using Microsoft.Ddue.Tools.Reflection;

namespace SandcastleBuilder.Components
{
    /// <summary>
    /// This is a custom assembly resolver for Sandcastle's <b>MRefBuilder</b>
    /// tool that supports assembly binding redirect elements in its
    /// configuration that let you redirect an unknown assembly's strong name
    /// to another by version number when resolving an unknown reference.
    /// </summary>
    /// <remarks>This solves the problem reported in Sandcastle work item
    /// <see href="http://Sandcastle.CodePlex.com/WorkItem/View.aspx?WorkItemId=1014">#1014</see>.
    /// To use the resolver, it must be added to the <b>MRefBuilder.config</b>
    /// file by defining the <c>resolver</c> element as shown in the example
    /// below.  Within the <c>resolver</c> element, add an <c>assemblyBinding</c>
    /// element that contains one or more <c>dependentAssembly</c> elements that
    /// define the redirections.  If you have a set of redirections in an
    /// application or web configuration file, you can define a
    /// <c>dependentAssembly</c> element with an <c>importFrom</c> attribute
    /// that specifies the location of the configuration file from which to
    /// import the redirects.  The other option is to specify individual
    /// redirects using the <c>assemblyIdentity</c> and <c>bindingRedirect</c>
    /// child elements of each <c>dependentAssembly</c> element.  See the MSDN
    /// help for those elements for more information on their usage.
    ///
    /// <note type="important">The assembly version(s) to which the entries are
    /// redirected must be one of the documented assemblies or must be
    /// referenced as a dependency using the <b>/dep</b> command line
    /// switch.</note></remarks>
    /// <example>
    /// <code lang="xml" title="Sample MRefBuilder.config">
    /// <![CDATA[
    /// <configuration>
    ///   <dduetools>
    ///     <platform version="2.0"
    ///       path="%SystemRoot%\Microsoft.NET\Framework\v2.0.50727\" />
    ///
    ///     <!-- Replace the resolver element with this.  Update the path
    ///          to point to the location of the assembly on your system. -->
    ///     <resolver type="SandcastleBuilder.Components.BindingRedirectResolver"
    ///         assembly="C:\SandcastleBuilder\SandcastleBuilder.MRefBuilder.dll"
    ///         use-gac="false">
    ///
    ///       <!-- Add an assemblyBinding element to contain the redirects -->
    ///       <assemblyBinding>
    ///         <!-- Import bindings from an application or web config file -->
    ///         <dependentAssembly importFrom=".\Web.config" />
    ///
    ///         <!-- Define a redirect for a range of versions -->
    ///         <dependentAssembly>
    ///           <assemblyIdentity name="NationalInstruments.Common"
    ///               publicKeyToken="4544464cdeaab541" />
    ///           <bindingRedirect oldVersion="1.0.0.0-8.1.20.168"
    ///               newVersion="8.1.20.237" />
    ///         </dependentAssembly>
    ///
    ///         <!-- Define a redirect for a single version -->
    ///         <dependentAssembly>
    ///           <assemblyIdentity name="MyCompany.Util.ComponentFactory"
    ///               publicKeyToken="E1458197622051B1" culture="neutral"/>
    ///           <bindingRedirect oldVersion="1.2.3.4"
    ///               newVersion="2.1.4.3"/>
    ///         </dependentAssembly>
    ///
    ///       </assemblyBinding>
    ///     </resolver>
    ///
    ///     <!-- ... rest of MRefBuilder.config ... -->
    ///
    ///   </dduetools>
    /// </configuration>]]>
    /// </code>
    /// </example>
    public class BindingRedirectResolver : AssemblyResolver
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, AssemblyNode> cache;
        private Collection<BindingRedirectSettings> redirects;
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">The configuration navigator</param>
        public BindingRedirectResolver(XPathNavigator configuration) :
          base(configuration)
        {
            Collection<BindingRedirectSettings> importedSettings;
            BindingRedirectSettings brs;
            Type type;
            System.Reflection.FieldInfo field;

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            // Log the use of the resolver in case it crashes
            ConsoleApplication.WriteMessage(LogLevel.Info, String.Format(
                CultureInfo.InvariantCulture, "\r\n[{0}, version {1}]\r\n" +
                "Binding Redirect Assembly Resolver Component.\r\n{2}\r\n" +
                "http://SHFB.CodePlex.com", fvi.ProductName,
                fvi.ProductVersion, fvi.LegalCopyright));

            // Unfortunately, the base class doesn't expose its cache so we
            // have to use Reflection to get at it.
            type = this.GetType().BaseType;
            field = type.GetField("cache", System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            cache = (Dictionary<string, AssemblyNode>)field.GetValue(this);

            // Load assembly binding redirects
            redirects = new Collection<BindingRedirectSettings>();

            foreach(XPathNavigator nav in configuration.Select(
              "assemblyBinding/dependentAssembly"))
            {
                brs = BindingRedirectSettings.FromXPathNavigator(nav, null, null);

                // Import settings from a configuration file?
                if(!String.IsNullOrEmpty(brs.ConfigurationFile))
                {
                    importedSettings = BindingRedirectSettings.FromConfigFile(
                        brs.ConfigurationFile);

                    foreach(BindingRedirectSettings imported in importedSettings)
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Info,
                            imported.ToString());
                        redirects.Add(imported);
                    }
                }
                else
                {
                    ConsoleApplication.WriteMessage(LogLevel.Info, brs.ToString());
                    redirects.Add(brs);
                }
            }
        }
        #endregion

        #region Resolve reference override
        //=====================================================================

        /// <summary>
        /// This is overridden to resolve unknown assembly references
        /// </summary>
        /// <param name="reference">The reference to resolve</param>
        /// <param name="module">The module</param>
        /// <returns>The assembly node if resolved or null if not resolved</returns>
        public override AssemblyNode ResolveReference(
          AssemblyReference reference, System.Compiler.Module module)
        {
            AssemblyNode node = base.ResolveReference(reference, module);
            string name;

            // Try the redirects if not found
            if(node == null)
            {
                name = reference.StrongName;

                foreach(BindingRedirectSettings brs in redirects)
                    if(brs.IsRedirectFor(name) && cache.ContainsKey(brs.StrongName))
                    {
                        ConsoleApplication.WriteMessage(LogLevel.Info,
                            "Using redirect '{0}' in place of '{1}'",
                            brs.StrongName, name);

                        node = cache[brs.StrongName];
                        cache.Add(name, node);
                        break;
                    }
            }

            return node;
        }
        #endregion

#if DEBUG
        /// <summary>
        /// For testing, break here so that we can see the output when all done
        /// </summary>
        ~BindingRedirectResolver()
        {
            if(System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
        }
#endif
    }
}
