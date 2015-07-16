// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change History
// 10/14/2012 - EFW - Added code to raise a component event to signal that the topic has been transformed.
// The event uses the new TransformedTopicEventArgs as the event arguments.
// 12/24/2013 - EFW - Updated the build component to be discoverable via MEF
// 04/27/2014 - EFW - Added support for a "transforming topic" event that happens prior to transformation

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Microsoft.Ddue.Tools.BuildComponent
{
    /// <summary>
    /// This build component is used to transform the intermediate topic to its final form such as an HTML
    /// document.
    /// </summary>
    public class TransformComponent : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        [BuildComponentExport("XSL Transform Component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new TransformComponent(base.BuildAssembler);
            }
        }
        #endregion

        #region Private data members
        //=====================================================================

        private List<Transform> transforms = new List<Transform>();
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the list of XSL transformations that will be applied to the topics
        /// </summary>
        public List<Transform> Transformations
        {
            get { return transforms; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected TransformComponent(BuildAssemblerCore buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Method overrides
        //=====================================================================

        /// <inheritdoc />
        public override void Initialize(XPathNavigator configuration)
        {
            // load the transforms
            XPathNodeIterator transform_nodes = configuration.Select("transform");

            foreach(XPathNavigator transform_node in transform_nodes)
            {
                // load the transform
                string file = transform_node.GetAttribute("file", String.Empty);

                if(String.IsNullOrEmpty(file))
                    base.WriteMessage(MessageLevel.Error, "Each transform element must specify a file attribute.");

                file = Environment.ExpandEnvironmentVariables(file);

                Transform transform = null;

                try
                {
                    transform = new Transform(file);
                }
                catch(IOException e)
                {
                    base.WriteMessage(MessageLevel.Error, "The transform file '{0}' could not be loaded. The " +
                        "error message is: {1}", file, e.GetExceptionMessage());
                }
                catch(XmlException e)
                {
                    base.WriteMessage(MessageLevel.Error, "The transform file '{0}' is not a valid XML file. " +
                        "The error message is: {1}", file, e.GetExceptionMessage());
                }
                catch(XsltException e)
                {
                    base.WriteMessage(MessageLevel.Error, "The XSL transform '{0}' contains an error. The " +
                        "error message is: {1}", file, e.GetExceptionMessage());
                }

                transforms.Add(transform);

                // load any arguments
                XPathNodeIterator argument_nodes = transform_node.Select("argument");

                foreach(XPathNavigator argument_node in argument_nodes)
                {
                    string key = argument_node.GetAttribute("key", String.Empty);

                    if((key == null) || (key.Length == 0))
                        base.WriteMessage(MessageLevel.Error, "When creating a transform argument, you must " +
                            "specify a key using the key attribute");

                    // set "expand-value" attribute to true to expand environment variables embedded in "value".
                    string expand_attr = argument_node.GetAttribute("expand-value", String.Empty);
                    bool expand_value = String.IsNullOrEmpty(expand_attr) ? false :
                        Convert.ToBoolean(expand_attr, CultureInfo.InvariantCulture);

                    string value = argument_node.GetAttribute("value", String.Empty);

                    if((value != null) && (value.Length > 0))
                        transform.Arguments.AddParam(key, String.Empty, expand_value ? Environment.ExpandEnvironmentVariables(value) : value);
                    else
                        transform.Arguments.AddParam(key, String.Empty, argument_node.Clone());
                }
            }
        }

        /// <summary>
        /// This is overridden to apply the XSL transformations to the document
        /// </summary>
        /// <param name="document">The document to transform</param>
        /// <param name="key">The topic key</param>
        /// <remarks><note type="important">An argument called <c>key</c> is automatically added to the argument
        /// list when each topic is transformed.  It will contain the current topic's key.</note></remarks>
        public override void Apply(XmlDocument document, string key)
        {
            // Raise a component event to signal that the topic is about to be transformed
            base.OnComponentEvent(new TransformingTopicEventArgs(key, document));

            foreach(Transform transform in transforms)
            {
                // Add the key as a parameter to the arguments
                transform.Arguments.RemoveParam("key", String.Empty);
                transform.Arguments.AddParam("key", String.Empty, key);

                // Create a buffer into which output can be written
                using(MemoryStream buffer = new MemoryStream())
                {
                    // Do the transform, routing output to the buffer
                    XmlWriterSettings settings = transform.Xslt.OutputSettings;
                    XmlWriter writer = XmlWriter.Create(buffer, settings);

                    try
                    {
                        transform.Xslt.Transform(document, transform.Arguments, writer);
                    }
                    catch(XsltException e)
                    {
                        base.WriteMessage(key, MessageLevel.Error, "An error occurred while executing the " +
                            "transform '{0}', on line {1}, at position {2}. The error message was: {3}",
                            e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message :
                            e.InnerException.Message);
                    }
                    catch(XmlException e)
                    {
                        base.WriteMessage(key, MessageLevel.Error, "An error occurred while executing the " +
                            "transform '{0}', on line {1}, at position {2}. The error message was: {3}",
                            e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message :
                            e.InnerException.Message);
                    }
                    finally
                    {
                        writer.Close();
                    }

                    // Replace the document by the contents of the buffer
                    buffer.Seek(0, SeekOrigin.Begin);

                    // Some settings to ensure that we don't try to go get, parse, and validate using any
                    // referenced schemas or DTDs
                    XmlReaderSettings readerSettings = new XmlReaderSettings();

                    //!EFW - Update. Uses DtdProcessing property now rather than the obsolete ProhibitDtd property.
                    // The old property value was false which translates to Parse for the new property.
                    readerSettings.DtdProcessing = DtdProcessing.Parse;
                    readerSettings.XmlResolver = null;

                    XmlReader reader = XmlReader.Create(buffer, readerSettings);

                    try
                    {
                        document.Load(reader);
                    }
                    catch(XmlException e)
                    {
                        base.WriteMessage(key, MessageLevel.Error, "An error occurred while executing the " +
                            "transform '{0}', on line {1}, at position {2}. The error message was: {3}",
                            e.SourceUri, e.LineNumber, e.LinePosition, (e.InnerException == null) ? e.Message :
                            e.InnerException.Message);
                    }
                    finally
                    {
                        reader.Close();
                    }
                }
            }

            // Raise a component event to signal that the topic has been transformed
            base.OnComponentEvent(new TransformedTopicEventArgs(key, document));
        }
        #endregion
    }
}
