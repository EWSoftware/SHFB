// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 03/09/2013 - EFW - Moved the supporting syntax writer classes to the SyntaxComponents assembly project

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This build component is used to generate syntax sections for API member topics
    /// </summary>
    public class SyntaxComponent : BuildComponent
    {
        #region Private data members
        //=====================================================================

        private XPathExpression syntax_input, syntax_output;
        private Type writerType;
        private List<SyntaxGenerator> generators = new List<SyntaxGenerator>();
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="assembler">The build assembler reference</param>
        /// <param name="configuration">The component configuration</param>
        public SyntaxComponent(BuildAssembler assembler, XPathNavigator configuration) :
            base(assembler, configuration)
        {
            XPathNavigator syntax_node = configuration.SelectSingleNode("syntax");
            string syntax_input_xpath = syntax_node.GetAttribute("input", String.Empty);

            if(String.IsNullOrEmpty(syntax_input_xpath))
                throw new ConfigurationErrorsException("You must specify an XPath for input in the syntax element.");

            syntax_input = XPathExpression.Compile(syntax_input_xpath);
            string syntax_output_xpath = syntax_node.GetAttribute("output", String.Empty);

            if(String.IsNullOrEmpty(syntax_output_xpath))
                throw new ConfigurationErrorsException("You must specify an XPath for output in the syntax element.");

            syntax_output = XPathExpression.Compile(syntax_output_xpath);

            writerType = typeof(ManagedSyntaxWriter);

            XPathNodeIterator generator_nodes = configuration.Select("generators/generator");

            foreach(XPathNavigator generator_node in generator_nodes)
            {

                // get the data to load the generator
                string assembly_path = generator_node.GetAttribute("assembly", String.Empty);
                if(String.IsNullOrEmpty(assembly_path))
                    WriteMessage(MessageLevel.Error, "Each generator element must have an assembly attribute.");
                string type_name = generator_node.GetAttribute("type", String.Empty);
                if(String.IsNullOrEmpty(type_name))
                    WriteMessage(MessageLevel.Error, "Each generator element must have a type attribute.");

                // expand environment variables in the path
                assembly_path = Environment.ExpandEnvironmentVariables(assembly_path);

                try
                {
                    Assembly assembly = Assembly.LoadFrom(assembly_path);
                    SyntaxGenerator generator = (SyntaxGenerator)assembly.CreateInstance(type_name, false, BindingFlags.Public | BindingFlags.Instance, null, new Object[1] { generator_node.Clone() }, null, null);

                    if(generator == null)
                        WriteMessage(MessageLevel.Error, "The type '{0}' does not exist in the assembly '{1}'.", type_name, assembly_path);
                    else
                        generators.Add(generator);
                }
                catch(IOException e)
                {
                    WriteMessage(MessageLevel.Error, "A file access error occured while attempting to load the build component '{0}'. The error message is: {1}", assembly_path, e.Message);
                }
                catch(BadImageFormatException e)
                {
                    WriteMessage(MessageLevel.Error, "A syntax generator assembly '{0}' is invalid. The error message is: {1}.", assembly_path, e.Message);
                }
                catch(TypeLoadException e)
                {
                    WriteMessage(MessageLevel.Error, "The type '{0}' does not exist in the assembly '{1}'. The error message is: {2}", type_name, assembly_path, e.Message);
                }
                catch(MissingMethodException e)
                {
                    WriteMessage(MessageLevel.Error, "The type '{0}' in the assembly '{1}' does not have an appropriate constructor. The error message is: {2}", type_name, assembly_path, e.Message);
                }
                catch(TargetInvocationException e)
                {
                    WriteMessage(MessageLevel.Error, "An error occured while attempting to instantiate the type '{0}' in the assembly '{1}'. The error message is: {2}", type_name, assembly_path, e.InnerException.Message);
                }
                catch(InvalidCastException)
                {
                    WriteMessage(MessageLevel.Error, "The type '{0}' in the assembly '{1}' is not a SyntaxGenerator.", type_name, assembly_path);
                }
            }

            WriteMessage(MessageLevel.Info, "Loaded {0} syntax generators.", generators.Count);
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Apply(XmlDocument document, string key)
        {
            XPathNavigator input = document.CreateNavigator().SelectSingleNode(syntax_input);
            XPathNavigator output = document.CreateNavigator().SelectSingleNode(syntax_output);

            SyntaxWriter writer = (SyntaxWriter)Activator.CreateInstance(writerType, new Object[1] { output });

            foreach(SyntaxGenerator generator in generators)
                generator.WriteSyntax(input, writer);
        }
        #endregion
    }
}