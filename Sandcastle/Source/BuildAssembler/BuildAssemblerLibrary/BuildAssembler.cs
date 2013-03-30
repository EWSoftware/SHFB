// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added support for setting a verbosity level.  Messages with a log level below the current
// verbosity level are ignored.
// 10/14/2012 - EFW - Added support for topic ID and message parameters in the message logging methods.
// 12/23/2012 - EFW - Updated to use and return enumerable lists of components rather than arrays.  Replaced
// TopicManifest and TopicEnumerator with a private ReadManifest() method.
// 01/12/2013 - EFW - Added the Execute() method to contain the build process and reworked it to allow for
// parallel executon of component code.  Components are still initialized and topics built sequentially for now.
// Converted the message logger to use BlockingCollection<string> to allow for parallel executon of component
// code without contention for the console.
// 03/01/2013 - EFW - Added a warning count

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    /// <summary>
    /// This class contains the build context and the build components
    /// </summary>
    public class BuildAssembler : IDisposable
    {
        #region Private data members
        //=====================================================================

        private BuildContext context = new BuildContext();

        private List<BuildComponent> components = new List<BuildComponent>();

        private BlockingCollection<string> messageLog = new BlockingCollection<string>();

        private MessageLevel verbosityLevel;
        private Action<string> messageLogger;
        private int warningCount;

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns the current build context
        /// </summary>
        public BuildContext Context
        {
            get { return context; }
        }

        /// <summary>
        /// This read-only property returns the current list of build components
        /// </summary>
        public IEnumerable<BuildComponent> BuildComponents
        {
            get { return components; }
        }

        /// <summary>
        /// The verbosity level for the message handlers
        /// </summary>
        /// <value>The value can be set to <c>Info</c>, <c>Warn</c>, or <c>Error</c>.  The default level
        /// is <see cref="MessageLevel.Info"/> so that all messages are displayed.  Setting it to a higher
        /// level will suppress messages below the given level.</value>
        /// <remarks>It is up to the message handler to make use of this property</remarks>
        public MessageLevel VerbosityLevel
        {
            get { return verbosityLevel; }
            set
            {
                if(value < MessageLevel.Info)
                    value = MessageLevel.Info;
                else
                    if(value > MessageLevel.Error)
                        value = MessageLevel.Error;

                verbosityLevel = value;
            }
        }
        #endregion

        #region Constructors
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageLogger">The message logger action</param>
        public BuildAssembler(Action<string> messageLogger)
        {
            if(messageLogger == null)
                throw new ArgumentNullException("messageLogger");

            this.messageLogger = messageLogger;
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <inheritdoc />
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of all components when being disposed
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                this.ClearComponents();

                if(messageLog != null)
                    messageLog.Dispose();
            }
        }
        #endregion

        #region Events
        //=====================================================================

        /// <summary>
        /// This event is raised when a component wants to signal that something of interest has happened
        /// </summary>
        public event EventHandler ComponentEvent;

        /// <summary>
        /// This raises the <see cref="ComponentEvent"/> event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments.  This may be <see cref="EventArgs.Empty"/> or a derived event
        /// arguments class containing information for the event handlers.</param>
        internal void OnComponentEvent(object sender, EventArgs e)
        {
            var handler = ComponentEvent;

            if(handler != null)
                handler(sender, e);
        }
        #endregion

        #region Operations
        //=====================================================================

        /// <summary>
        /// This is used to execute the build assembler instance using the specified configuration file and
        /// manifest file.
        /// </summary>
        /// <param name="configuration">The build assembler configuration</param>
        /// <param name="manifest">The manifest file</param>
        public virtual void Execute(XPathDocument configuration, string manifest)
        {
            Task builder = Task.Factory.StartNew(() =>
            {
                try
                {
                    XPathNavigator configNav = configuration.CreateNavigator();

                    // See if a verbosity level has been specified.  If so, set it.
                    var verbosity = configNav.SelectSingleNode("/configuration/@verbosity");
                    MessageLevel level;

                    if(verbosity == null || !Enum.TryParse<MessageLevel>(verbosity.Value, out level))
                        level = MessageLevel.Info;

                    this.VerbosityLevel = level;

                    if(level > MessageLevel.Info)
                        messageLog.Add("Info: Loading configuration...");

                    // Load the context
                    XPathNavigator contextNode = configNav.SelectSingleNode(
                        "/configuration/dduetools/builder/context");

                    if(contextNode != null)
                        this.Context.Load(contextNode);

                    // Load the build components
                    XPathNavigator componentsNode = configNav.SelectSingleNode(
                        "/configuration/dduetools/builder/components");

                    if(componentsNode != null)
                        this.AddComponents(componentsNode);

                    // Proceed through the build manifest, processing all topics named there
                    if(level > MessageLevel.Info)
                        messageLog.Add("Info: Processing topics...");

                    int count = this.Apply(manifest);

                    messageLog.Add(String.Format(CultureInfo.CurrentCulture, "Info: Processed {0} topic(s)", count));

                    if(warningCount != 0)
                        messageLog.Add(String.Format(CultureInfo.CurrentCulture, "Info: {0} warning(s)", warningCount));
                }
                finally
                {
                    messageLog.CompleteAdding();
                }
            });

            Task logger = Task.Factory.StartNew(() =>
            {
                foreach(string s in messageLog.GetConsumingEnumerable())
                    messageLogger(s);
            });

            Task.WaitAll(builder, logger);
        }

        /// <summary>
        /// This is used to read a manifest file to extract topic IDs for processing
        /// </summary>
        /// <param name="manifest">The manifest file to read</param>
        /// <returns>An enumerable list of topic IDs</returns>
        private IEnumerable<string> ReadManifest(string manifest)
        {
            string id;

            using(var reader = XmlReader.Create(manifest))
            {
                reader.MoveToContent();

                while(reader.Read())
                    if(reader.NodeType == XmlNodeType.Element && reader.LocalName == "topic")
                    {
                        id = reader.GetAttribute("id");

                        if(String.IsNullOrEmpty(id))
                            throw new InvalidOperationException("A manifest topic is missing the required " +
                                " id attribute");

                        yield return id;
                    }
            }
        }

        /// <summary>
        /// Apply the current set of components to the topics defined in the given manifest file
        /// </summary>
        /// <param name="manifestFile">The manifest file containing the topics to generate</param>
        /// <returns>A count of the number of topics processed</returns>
        /// <overloads>There are two overloads for this method</overloads>
        protected int Apply(string manifestFile)
        {
            return this.Apply(this.ReadManifest(manifestFile));
        }

        /// <summary>
        /// Apply the current set of components to the given list of topics
        /// </summary>
        /// <param name="topics">The enumerable list of topic IDs</param>
        /// <returns>A count of the number of topics processed</returns>
        protected int Apply(IEnumerable<string> topics)
        {
            int count = 0;

            foreach(string topic in topics)
            {
                // Create the document
                XmlDocument document = new XmlDocument();
                document.PreserveWhitespace = true;

                this.WriteMessage(MessageLevel.Info, "Building topic {0}", topic);

                // Apply the component stack
                foreach(BuildComponent component in components)
                    component.Apply(document, topic);

                count++;
            }

            return count;
        }

        // TODO: This doesn't appear to be used anymore.  Remove?
/*        protected IEnumerable<BuildContext> GetFileManifestBuildContextEnumerator(string manifestFilename)
        {
            using(XmlReader reader = XmlReader.Create(manifestFilename))
            {
                reader.MoveToContent();

                while(reader.Read())
                {
                    if(reader.NodeType == XmlNodeType.Element && reader.LocalName == "topic")
                    {
                        BuildContext thisContext = new BuildContext();

                        try
                        {
                            string id = reader.GetAttribute("id");

                            while(reader.MoveToNextAttribute())
                            {
                                string name = reader.Name;
                                string value = reader.Value;
                                thisContext.AddVariable(name, value);
                            }
                        }
                        catch(XmlException e)
                        {
                            throw new XmlException(String.Format(CultureInfo.CurrentCulture,
                                "The manifest file: '{0}' is not well-formed. The error message is: {1}",
                                manifestFilename, e.Message), e);
                        }

                        yield return thisContext;
                    }
                }
            }
        }*/

        /// <summary>
        /// This is used to create a component based on the given configuration
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>A component created using the given definition</returns>
        /// <exception cref="ArgumentNullException">This is thrown if <paramref name="configuration"/> is null</exception>
        public BuildComponent LoadComponent(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException("configuration");

            // Get the component information
            string assemblyName = configuration.GetAttribute("assembly", String.Empty);

            if(String.IsNullOrEmpty(assemblyName))
                this.WriteMessage(MessageLevel.Error, "Each component element must have an assembly attribute " +
                    "that specifies a path to the component assembly.");

            string typeName = configuration.GetAttribute("type", String.Empty);

            if(String.IsNullOrEmpty(typeName))
                this.WriteMessage(MessageLevel.Error, "Each component element must have a type attribute that " +
                    "specifies the fully qualified name of a component type.");

            // Expand environment variables in path of assembly name
            assemblyName = Environment.ExpandEnvironmentVariables(assemblyName);

            // Load and instantiate the component
            BuildComponent component = null;

            try
            {
                Assembly assembly = Assembly.LoadFrom(assemblyName);
                component = (BuildComponent)assembly.CreateInstance(typeName, false, BindingFlags.Public |
                    BindingFlags.Instance, null, new object[2] { this, configuration }, null, null);
            }
            catch(IOException e)
            {
                this.WriteMessage(MessageLevel.Error, "A file access error occured while attempting to load " +
                    "the build component assembly '{0}'. The error message is: {1}", assemblyName, e.Message);
            }
            catch(BadImageFormatException e)
            {
                this.WriteMessage(MessageLevel.Error, "The build component assembly '{0}' is not a valid " +
                    "managed assembly. The error message is: {1}", assemblyName, e.Message);
            }
            catch(TypeLoadException)
            {
                this.WriteMessage(MessageLevel.Error, "The build component '{0}' was not found in the " +
                    "assembly '{1}'.", typeName, assemblyName);
            }
            catch(MissingMethodException e)
            {
                this.WriteMessage(MessageLevel.Error, "No appropriate constructor exists for the build " +
                    "component '{0}' in the component assembly '{1}'. The error message is: {1}", typeName,
                    assemblyName, e.Message);
            }
            catch(TargetInvocationException e)
            {
                // Ignore OperationCanceledException as it is the result of logging an error message
                if(e.InnerException is OperationCanceledException)
                    throw e.InnerException;

                this.WriteMessage(MessageLevel.Error, "An error occured while initializing the build component " +
                    "'{0}' in the component assembly '{1}'. The error message and stack trace follows: {2}",
                    typeName, assemblyName, e.InnerException.ToString());
            }
            catch(InvalidCastException)
            {
                this.WriteMessage(MessageLevel.Error, "The type '{0}' in the component assembly '{1}' is not a " +
                    "build component.", typeName, assemblyName);
            }

            if(component == null)
                this.WriteMessage(MessageLevel.Error, "The type '{0}' was not found in the component " +
                    "assembly '{1}'.", typeName, assemblyName);

            return component;
        }

        /// <summary>
        /// This is used to load a set of components in a configuration and return them as an enumerable list
        /// </summary>
        /// <param name="configuration">The con</param>
        /// <returns>An enumerable list of components created based on the configuration information</returns>
        public IEnumerable<BuildComponent> LoadComponents(XPathNavigator configuration)
        {
            XPathNodeIterator componentNodes = configuration.Select("component");
            List<BuildComponent> components = new List<BuildComponent>();

            foreach(XPathNavigator componentNode in componentNodes)
                components.Add(this.LoadComponent(componentNode));

            return components;
        }
        #endregion

        #region Methods used to add and remove components
        //=====================================================================

        /// <summary>
        /// Add a set of components based on the given configuration
        /// </summary>
        /// <param name="configuration">The configuration containing the component definitions</param>
        public void AddComponents(XPathNavigator configuration)
        {
            components.AddRange(this.LoadComponents(configuration));
        }

        /// <summary>
        /// Dispose of all components and clear them from the collection
        /// </summary>
        public void ClearComponents()
        {
            foreach(BuildComponent component in components)
                component.Dispose();

            components.Clear();
        }
        #endregion

        #region Message handler methods
        //=====================================================================

        /// <summary>
        /// Write a message to the message log
        /// </summary>
        /// <param name="level">The message level</param>
        /// <param name="message">The message to write</param>
        /// <param name="args">An optional list of arguments to format into the message</param>
        private void WriteMessage(MessageLevel level, string message, params object[] args)
        {
            this.WriteMessage(this.GetType(), level, null, (args.Length == 0) ? message :
                String.Format(CultureInfo.CurrentCulture, message, args));
        }

        /// <summary>
        /// Write a component message to the message log
        /// </summary>
        /// <param name="type">The component type making the request</param>
        /// <param name="level">The message level</param>
        /// <param name="key">An optional topic key related to the message or null if there isn't one</param>
        /// <param name="message">The message to write to the console</param>
        /// <remarks>If the message level is below the current verbosity level setting, the message is ignored</remarks>
        public void WriteMessage(Type type, MessageLevel level, string key, string message)
        {
            string text;

            if(level >= this.VerbosityLevel)
            {
                if(String.IsNullOrWhiteSpace(key))
                    text = String.Format(CultureInfo.CurrentCulture, "{0}: {1}: {2}", level, type.Name, message);
                else
                {
                    // The key can contain braces so escape them
                    key = key.Replace("{", "{{").Replace("}", "}}");

                    text = String.Format(CultureInfo.CurrentCulture, "{0}: {1}: [{2}] {3}", level, type.Name,
                        key, message);
                }

                // If the background task has completed, we'll call the message logger action directly
                switch(level)
                {
                    case MessageLevel.Info:
                    case MessageLevel.Warn:
                    case MessageLevel.Diagnostic:
                        if(level == MessageLevel.Warn)
                            Interlocked.Add(ref warningCount, 1);

                        if(!messageLog.IsAddingCompleted)
                            messageLog.Add(text);
                        else
                            messageLogger(text);
                        break;

                    case MessageLevel.Error:
                        text = "\r\n" + text;

                        if(!messageLog.IsAddingCompleted)
                            messageLog.Add(text);
                        else
                            messageLogger(text);

                        if(System.Diagnostics.Debugger.IsAttached)
                            System.Diagnostics.Debugger.Break();

                        // Errors need to terminate the build so we'll throw an exception to terminate the task
                        throw new OperationCanceledException("See log for details");

                    default:    // Ignored
                        break;
                }
            }
        }
        #endregion
    }
}
