// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added support for setting a verbosity level.  Messages with a log level below
// the current verbosity level are ignored.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Ddue.Tools.CommandLine;

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

        private MessageHandler handler;
        
        private static MessageLevel verbosityLevel;
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
        public BuildComponent[] BuildComponents
        {
            get { return components.ToArray(); }
        }

        /// <summary>
        /// This read-only property returns the current message handler
        /// </summary>
        public MessageHandler MessageHandler
        {
            get { return handler; }
        }

        /// <summary>
        /// The verbosity level for the message handlers
        /// </summary>
        /// <value>The value can be set to <c>Info</c>, <c>Warn</c>, or <c>Error</c>.  The default level
        /// is <see cref="MessageLevel.Info"/> so that all messages are displayed.  Setting it to a higher
        /// level will suppress messages below the given level.</value>
        /// <remarks>It is up to the message handler to make use of this property</remarks>
        public static MessageLevel VerbosityLevel
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
        public BuildAssembler()
        {
            this.handler = BuildAssembler.ConsoleMessageHandler;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageHandler">A custom message handler delegate</param>
        public BuildAssembler(MessageHandler messageHandler)
        {
            if(messageHandler == null)
                throw new ArgumentNullException("messageHandler");

            this.handler = messageHandler;
        }
        #endregion

        #region Events
        //=====================================================================

        public event EventHandler ComponentEvent;

        internal void OnComponentEvent(Object o, EventArgs e)
        {
            if(ComponentEvent != null)
                ComponentEvent(o, e);
        }
        #endregion

        #region Operations
        //=====================================================================

        public int Apply(IEnumerable<string> topics)
        {
            int count = 0;

            foreach(string topic in topics)
            {

                // create the document
                XmlDocument document = new XmlDocument();
                document.PreserveWhitespace = true;

                // write a log message
                WriteMessage(MessageLevel.Info, String.Format("Building topic {0}", topic));

                // apply the component stack
                foreach(BuildComponent component in components)
                {

                    component.Apply(document, topic);
                }

                count++;
            }

            return (count);
        }

        public int Apply(string manifestFile)
        {
            return (Apply(new TopicManifest(manifestFile)));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                foreach(BuildComponent component in components)
                {
                    ((IDisposable)component).Dispose();
                }
            }
        }

        public IEnumerable<BuildContext> GetFileManifestBuildContextEnumerator(string manifestFilename)
        {
            using(XmlReader reader = XmlReader.Create(manifestFilename))
            {
                reader.MoveToContent();
                while(reader.Read())
                {
                    if((reader.NodeType == XmlNodeType.Element) && (reader.LocalName == "topic"))
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
                            throw new XmlException(String.Format("The manifest file: '{0}' is not well-formed. The error message is: {1}", manifestFilename, e.Message), e);
                        }

                        yield return thisContext;
                    }
                }
            }
        }

        public BuildComponent LoadComponent(XPathNavigator configuration)
        {

            if(configuration == null)
                throw new ArgumentNullException("configuration");

            // get the component infomation
            string assemblyName = configuration.GetAttribute("assembly", String.Empty);
            if(String.IsNullOrEmpty(assemblyName))
            {
                WriteMessage(MessageLevel.Error, "Each component element must have an assembly attribute that specifys a path to the component assembly.");
            }

            string typeName = configuration.GetAttribute("type", String.Empty);
            if(String.IsNullOrEmpty(typeName))
            {
                WriteMessage(MessageLevel.Error, "Each component element must have a type attribute that specifys the fully qualified name of a component type.");
            }

            // expand environmet variables in path of assembly name
            assemblyName = Environment.ExpandEnvironmentVariables(assemblyName);

            // load and instantiate the component
            BuildComponent component = null;
            try
            {

                Assembly assembly = Assembly.LoadFrom(assemblyName);
                component = (BuildComponent)assembly.CreateInstance(typeName, false, BindingFlags.Public | BindingFlags.Instance, null, new Object[2] { this, configuration }, null, null);

            }
            catch(IOException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("A file access error occured while attempting to load the build component assembly '{0}'. The error message is: {1}", assemblyName, e.Message));
            }
            catch(BadImageFormatException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("The build component assembly '{0}' is not a valid managed assembly. The error message is: {1}", assemblyName, e.Message));
            }
            catch(TypeLoadException)
            {
                WriteMessage(MessageLevel.Error, String.Format("The build component '{0}' was not found in the assembly '{1}'.", typeName, assemblyName));
            }
            catch(MissingMethodException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("No appropriate constructor exists for the build component '{0}' in the component assembly '{1}'. The error message is: {1}", typeName, assemblyName, e.Message));
            }
            catch(TargetInvocationException e)
            {
                WriteMessage(MessageLevel.Error, String.Format("An error occured while initializing the build component '{0}' in the component assembly '{1}'. The error message and stack trace follows: {2}", typeName, assemblyName, e.InnerException.ToString()));
            }
            catch(InvalidCastException)
            {
                WriteMessage(MessageLevel.Error, String.Format("The type '{0}' in the component assembly '{1}' is not a build component.", typeName, assemblyName));
            }

            if(component == null)
            {
                WriteMessage(MessageLevel.Error, String.Format("The type '{0}' was not found in the component assembly '{1}'.", typeName, assemblyName));
            }

            return (component);
        }

        public BuildComponent[] LoadComponents(XPathNavigator configuration)
        {
            XPathNodeIterator componentNodes = configuration.Select("component");

            List<BuildComponent> components = new List<BuildComponent>();

            foreach(XPathNavigator componentNode in componentNodes)
            {
                components.Add(LoadComponent(componentNode));
            }

            return (components.ToArray());
        }
        #endregion

        #region Methods used to add and remove components
        //=====================================================================

        public void AddComponents(XPathNavigator configuration)
        {
            BuildComponent[] componentsToAdd = LoadComponents(configuration);
            foreach(BuildComponent componentToAdd in componentsToAdd)
            {
                components.Add(componentToAdd);
            }
        }

        public void ClearComponents()
        {
            components.Clear();
        }
        #endregion

        #region Message handler methods
        //=====================================================================

        private void WriteMessage(MessageLevel level, string message)
        {
            handler(this.GetType(), level, message);
        }

        public static MessageHandler ConsoleMessageHandler
        {
            get
            {
                return (new MessageHandler(WriteComponentMessageToConsole));
            }
        }

        private static void WriteComponentMessageToConsole(Type type, MessageLevel level, string message)
        {
            if(level >= VerbosityLevel)
            {
                string text = String.Format("{0}: {1}", type.Name, message);

                switch(level)
                {
                    case MessageLevel.Info:
                        ConsoleApplication.WriteMessage(LogLevel.Info, text);
                        break;

                    case MessageLevel.Warn:
                        ConsoleApplication.WriteMessage(LogLevel.Warn, text);
                        break;

                    case MessageLevel.Error:
                        ConsoleApplication.WriteMessage(LogLevel.Error, text);
                        Environment.Exit(1);
                        break;

                    case MessageLevel.Diagnostic:
                        ConsoleApplication.WriteMessage(LogLevel.Diagnostic, text);
                        break;

                    default:    // Ignored
                        break;
                }
            }
        }
        #endregion

        #region Topic manifest class
        //=====================================================================

        private class TopicManifest : IEnumerable<string>
        {

            public TopicManifest(string manifest)
            {
                this.manifest = manifest;
            }

            private string manifest;

            public IEnumerator<string> GetEnumerator()
            {
                return (new TopicEnumerator(manifest));
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return (GetEnumerator());
            }
        }
        #endregion

        #region Topic enumerator class
        //=====================================================================

        private class TopicEnumerator : IEnumerator<string>
        {

            public TopicEnumerator(string manifest)
            {
                reader = XmlReader.Create(manifest);
                reader.MoveToContent();
            }

            private XmlReader reader;

            public bool MoveNext()
            {
                while(reader.Read())
                {
                    if((reader.NodeType == XmlNodeType.Element) && (reader.LocalName == "topic"))
                        return (true);
                }

                return (false);
            }

            public string Current
            {
                get
                {
                    string id = reader.GetAttribute("id");
                    return (id);
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    return (Current);
                }
            }

            public void Reset()
            {
                throw new InvalidOperationException();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if(disposing)
                {
                    reader.Close();
                }
            }
        }
        #endregion
    }
}
