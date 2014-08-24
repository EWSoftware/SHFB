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
// parallel execution of component code.  Components are still initialized and topics built sequentially for now.
// Converted the message logger to use BlockingCollection<string> to allow for parallel execution of component
// code without contention for the console.
// 03/01/2013 - EFW - Added a warning count
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly
// 12/26/2013 - EFW - Updated to load build components via MEF
// 12/28/2013 - EFW - Added MSBuild task support

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.BuildComponent;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This class contains the build context and the build components
    /// </summary>
    [Export(typeof(BuildAssemblerCore))]
    public class BuildAssemblerCore : IDisposable
    {
        #region Private data members
        //=====================================================================

        private CancellationTokenSource tokenSource;
        private BuildContext context = new BuildContext();

        private List<BuildComponentCore> components = new List<BuildComponentCore>();

        private BlockingCollection<Tuple<LogLevel, string>> messageLog =
            new BlockingCollection<Tuple<LogLevel, string>>();

        private CompositionContainer componentContainer;
        private List<Lazy<BuildComponentFactory, IBuildComponentMetadata>> allBuildComponents;
        private HashSet<string> componentFolders;

        private MessageLevel verbosityLevel;
        private Action<LogLevel, string> messageLogger;
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
        public IEnumerable<BuildComponentCore> BuildComponents
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
        /// Private default constructor to satisfy MEF composition
        /// </summary>
        private BuildAssemblerCore()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageLogger">The message logger action</param>
        public BuildAssemblerCore(Action<LogLevel, string> messageLogger)
        {
            if(messageLogger == null)
                throw new ArgumentNullException("messageLogger");

            this.messageLogger = messageLogger;

            tokenSource = new CancellationTokenSource();

            // When ran as an MSBuild task, it won't always find dependent assemblies so we must find them
            // manually.
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
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
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                this.ClearComponents();

                if(messageLog != null)
                    messageLog.Dispose();

                if(componentContainer != null)
                    componentContainer.Dispose();

                if(tokenSource != null)
                    tokenSource.Dispose();
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
        /// This is used to cancel the build
        /// </summary>
        /// <remarks>The build will terminate as soon as possible after initializing a component or after a
        /// topic finishes being generated.</remarks>
        public void Cancel()
        {
            if(tokenSource != null)
                tokenSource.Cancel();
        }

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
                        messageLog.Add(LogMessage(LogLevel.Info, "Loading configuration..."));

                    // Load the context
                    XPathNavigator contextNode = configNav.SelectSingleNode(
                        "/configuration/dduetools/builder/context");

                    if(contextNode != null)
                        this.Context.Load(contextNode);

                    // Find all available build components
                    this.CreateComponentContainer(configNav.SelectSingleNode(
                        "/configuration/dduetools/builder/componentLocations"));

                    // Load the build components
                    XPathNavigator componentsNode = configNav.SelectSingleNode(
                        "/configuration/dduetools/builder/components");

                    if(componentsNode != null)
                        this.AddComponents(componentsNode);

                    // Proceed through the build manifest, processing all topics named there
                    if(level > MessageLevel.Info)
                        messageLog.Add(LogMessage(LogLevel.Info, "Processing topics..."));

                    int count = this.Apply(manifest);

                    messageLog.Add(LogMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                        "Processed {0} topic(s)", count)));

                    if(warningCount != 0)
                        messageLog.Add(LogMessage(LogLevel.Info, String.Format(CultureInfo.CurrentCulture,
                            "{0} warning(s)", warningCount)));
                }
                finally
                {
                    messageLog.CompleteAdding();
                }
            }, tokenSource.Token);

            Task logger = Task.Factory.StartNew(() =>
            {
                foreach(var msg in messageLog.GetConsumingEnumerable())
                    messageLogger(msg.Item1, msg.Item2);
            }, tokenSource.Token);

            Task.WaitAll(new[] { builder, logger}, tokenSource.Token);
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
                foreach(BuildComponentCore component in components)
                {
                    component.Apply(document, topic);
                    tokenSource.Token.ThrowIfCancellationRequested();
                }

                count++;
            }

            return count;
        }
        #endregion

        #region Methods used to add and remove components
        //=====================================================================

        /// <summary>
        /// This is handled to resolve dependent assemblies and load them when necessary
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments</param>
        /// <returns>The loaded assembly or null if not found</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string[] nameInfo = args.Name.Split(new char[] { ',' });
            string resolveName = nameInfo[0];

            // See if it has already been loaded
            Assembly asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);

            // If not already loaded, check for dependency assemblies in the component folders
            if(asm == null)
                foreach(string folder in componentFolders)
                {
                    resolveName = Directory.EnumerateFiles(folder, "*.dll").FirstOrDefault(
                        f => resolveName == Path.GetFileNameWithoutExtension(f));

                    if(resolveName != null)
                    {
                        asm = Assembly.LoadFile(resolveName);
                        break;
                    }
                }

            return asm;
        }

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
            foreach(var component in components)
                component.Dispose();

            components.Clear();
        }

        /// <summary>
        /// This is used to load a set of components in a configuration and return them as an enumerable list
        /// </summary>
        /// <param name="configuration">The con</param>
        /// <returns>An enumerable list of components created based on the configuration information</returns>
        public IEnumerable<BuildComponentCore> LoadComponents(XPathNavigator configuration)
        {
            XPathNodeIterator componentNodes = configuration.Select("component");
            List<BuildComponentCore> loadedComponents = new List<BuildComponentCore>();

            foreach(XPathNavigator componentNode in componentNodes)
            {
                loadedComponents.Add(this.LoadComponent(componentNode));
                tokenSource.Token.ThrowIfCancellationRequested();
            }

            return loadedComponents;
        }

        /// <summary>
        /// This is used to create a component based on the given configuration
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>A component created using the given definition</returns>
        /// <exception cref="ArgumentNullException">This is thrown if <paramref name="configuration"/> is null</exception>
        public BuildComponentCore LoadComponent(XPathNavigator configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException("configuration");

            // Get the component ID
            string id = configuration.GetAttribute("id", String.Empty);

            if(String.IsNullOrWhiteSpace(id))
                this.WriteMessage(MessageLevel.Error, "Each component element must have an id attribute " +
                    "that specifies the component's unique ID.");

            // Load and instantiate the component
            BuildComponentCore component = null;

            try
            {
                var factory = allBuildComponents.FirstOrDefault(f => f.Metadata.Id == id);

                if(factory == null)
                    this.WriteMessage(MessageLevel.Error, "The component '{0}' was not found in any of the " +
                        "component assemblies", id);

                component = factory.Value.Create();
                component.Initialize(configuration);
            }
            catch(Exception e)
            {
                this.WriteMessage(MessageLevel.Error, "An unexpected error occurred while attempting to create " +
                    "the build component '{0}'. The error message is: {1}", id, e.Message);
            }

            return component;
        }

        /// <summary>
        /// This is used to create a composition container filled with the available build components
        /// </summary>
        /// <param name="componentLocations">The component locations configuration node</param>
        /// <remarks>If any component locations are specified, they are searched recursively for component
        /// assemblies in the order given.  The custom components and tools folders are added last if not already
        /// specified as one of the component locations.  There may be duplicate component IDs across the
        /// assemblies found.  Only the first component for a unique ID will be used.  As such, assemblies in a
        /// folder that appears earlier in the list can override copies in folders lower in the list.</remarks>
        private void CreateComponentContainer(XPathNavigator componentLocations)
        {
            if(componentContainer != null)
            {
                componentContainer.Dispose();
                componentContainer = null;
                allBuildComponents = null;
            }

            componentFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Create an aggregate catalog that combines assembly catalogs for all of the possible component
            // locations.
            var catalog = new AggregateCatalog();

            if(componentLocations != null)
                foreach(XPathNavigator location in componentLocations.Select("location/@folder"))
                    if(!String.IsNullOrWhiteSpace(location.Value) && Directory.Exists(location.Value))
                        AddAssemblyCatalogs(catalog, location.Value, componentFolders, true);

            // Always include the custom components and the tools folders
            AddAssemblyCatalogs(catalog, ComponentUtilities.ComponentsFolder, componentFolders, true);
            AddAssemblyCatalogs(catalog, ComponentUtilities.ToolsFolder, componentFolders, true);

            componentContainer = new CompositionContainer(catalog);

            // The caller should have exported a property that returns this instance
            componentContainer.ComposeParts(this);

            allBuildComponents = componentContainer.GetExports<BuildComponentFactory, IBuildComponentMetadata>().ToList();
        }

        /// <summary>
        /// This adds assembly catalogs to the given aggregate catalog for the given folder and all of its
        /// subfolders recursively.
        /// </summary>
        /// <param name="catalog">The aggregate catalog to which the assembly catalogs are added.</param>
        /// <param name="folder">The root folder to search.  It and all subfolders recursively will be searched
        /// for assemblies to add to the aggregate catalog.</param>
        /// <param name="searchedFolders">A hash set of folders that have already been searched and added.</param>
        /// <param name="includeSubfolders">True to search subfolders recursively, false to only search the given
        /// folder.</param>
        /// <remarks>It is done this way to prevent a single assembly that would normally be discovered via a
        /// directory catalog from preventing all assemblies from loading if it cannot be examined when the parts
        /// are composed (i.e. trying to load a Windows Store assembly on Windows 7).</remarks>
        private static void AddAssemblyCatalogs(AggregateCatalog catalog, string folder,
          HashSet<string> searchedFolders, bool includeSubfolders)
        {
            if(!String.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) && !searchedFolders.Contains(folder))
            {
                foreach(var file in Directory.EnumerateFiles(folder, "*.dll"))
                {
                    try
                    {
                        var asmCat = new AssemblyCatalog(file);

                        // Force MEF to load the assembly and figure out if there are any exports.  Valid
                        // assemblies won't throw any exceptions and will contain parts and will be added to
                        // the catalog.  Use Count() rather than Any() to ensure it touches all parts in case
                        // that makes a difference.
                        if(asmCat.Parts.Count() > 0)
                            catalog.Catalogs.Add(asmCat);
                        else
                            asmCat.Dispose();
                    }
                    catch(FileNotFoundException ex)
                    {
                        // Ignore the errors we may expect to see but log them for debugging purposes
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(FileLoadException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(BadImageFormatException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(System.Security.SecurityException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(UnauthorizedAccessException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(ReflectionTypeLoadException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);

                        foreach(var lex in ex.LoaderExceptions)
                            System.Diagnostics.Debug.WriteLine(lex);
                    }
                }

                // Enumerate subfolders separately so that we can skip future requests for the same folder
                if(includeSubfolders)
                    try
                    {
                        foreach(string subfolder in Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories))
                            AddAssemblyCatalogs(catalog, subfolder, searchedFolders, false);
                    }
                    catch(IOException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(System.Security.SecurityException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    catch(UnauthorizedAccessException ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
            }
        }
        #endregion

        #region Message handler methods
        //=====================================================================

        /// <summary>
        /// This is a helper method used to create log message tuples
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        private static Tuple<LogLevel, string> LogMessage(LogLevel level, string message)
        {
            return Tuple.Create<LogLevel, string>(level, message);
        }

        /// <summary>
        /// This is a helper method used to create log message tuples
        /// </summary>
        /// <param name="level">The log level</param>
        /// <param name="message">The message</param>
        /// <returns></returns>
        private static Tuple<LogLevel, string> LogMessage(MessageLevel level, string message)
        {
            LogLevel logLevel;

            switch(level)
            {
                case MessageLevel.Ignore:
                case MessageLevel.Info:
                    logLevel = LogLevel.Info;
                    break;

                case MessageLevel.Warn:
                    logLevel = LogLevel.Warn;
                    break;

                case MessageLevel.Error:
                    logLevel = LogLevel.Error;
                    break;

                default:
                    logLevel = LogLevel.Diagnostic;
                    break;
            }

            return Tuple.Create<LogLevel, string>(logLevel, message);
        }

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
                    text = String.Format(CultureInfo.CurrentCulture, "{0}: {1}", type.Name, message);
                else
                    text = String.Format(CultureInfo.CurrentCulture, "{0}: [{1}] {2}", type.Name, key, message);

                // If the background task has completed, we'll call the message logger action directly
                switch(level)
                {
                    case MessageLevel.Info:
                    case MessageLevel.Warn:
                    case MessageLevel.Diagnostic:
                        if(level == MessageLevel.Warn)
                            Interlocked.Add(ref warningCount, 1);

                        var m = LogMessage(level, text);

                        if(!messageLog.IsAddingCompleted)
                            messageLog.Add(m);
                        else
                            messageLogger(m.Item1, m.Item2);
                        break;

                    case MessageLevel.Error:
                        text = "\r\n" + text;

                        var m2 = LogMessage(level, text);

                        if(!messageLog.IsAddingCompleted)
                            messageLog.Add(m2);
                        else
                            messageLogger(m2.Item1, m2.Item2);

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
