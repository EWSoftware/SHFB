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
// 11/20/2015 - EFW - Reworked to support a document type attribute on the created topic
// 02/25/2022 - EFW - Moved build assembler tool and execution into the build engine

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core;
using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;
using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PresentationStyle.Transformation;

namespace SandcastleBuilder.MSBuild.BuildEngine;

/// <summary>
/// This class is used to load a set of build components based on a given configuration file and use them to
/// produce a set of topics defined in a manifest file.The topics produced by the build can be then compiled
/// into a help file.
/// </summary>
[Export(typeof(IBuildAssembler))]
internal sealed class BuildAssemblerInternal : IBuildAssembler, IDisposable
{
    #region Private data members
    //=====================================================================

    private readonly BuildProcess currentBuild;
    private readonly List<BuildComponentCore> components = [];
    private readonly BlockingCollection<(MessageLevel Level, string Message)> messageLog = [];
    private int warningCount;

    #endregion

    #region Constructors
    //=====================================================================

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="currentBuild">The current build</param>
    public BuildAssemblerInternal(BuildProcess currentBuild)
    {
        this.currentBuild = currentBuild;

        this.VerbosityLevel = currentBuild.CurrentProject.BuildAssemblerVerbosity == BuildAssemblerVerbosity.AllMessages ?
            MessageLevel.Info : currentBuild.CurrentProject.BuildAssemblerVerbosity == BuildAssemblerVerbosity.OnlyWarningsAndErrors ?
            MessageLevel.Warn : MessageLevel.Error;

        // Get the transformation to use and set any transformation argument values
        this.TopicTransformation = currentBuild.PresentationStyle.TopicTransformation;

        foreach(var arg in currentBuild.CurrentProject.TransformComponentArguments)
        {
            if(this.TopicTransformation.TransformationArguments.TryGetValue(arg.Key, out var ta))
            {
                if(arg.Value != null)
                    ta.Value = arg.Value;
                else
                    ta.Content = arg.Content;
            }
        }

        this.TopicTransformation.HasHeaderText = !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.HeaderText);
        this.TopicTransformation.HasFooterContent = !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.FooterText) ||
            !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.CopyrightText) ||
            !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.CopyrightHref) ||
            !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.FeedbackEMailAddress);
        this.TopicTransformation.HasWebsiteAdContent = !String.IsNullOrWhiteSpace(currentBuild.CurrentProject.WebsiteAdContent);
        this.TopicTransformation.IsPreliminaryDocumentation = currentBuild.CurrentProject.Preliminary;
        this.TopicTransformation.Locale = currentBuild.Language.Name;

        // Special case.  If the transformation contains a BibliographyDataFile argument, set the filename
        // on the transformation.  If relative, use the project path to fully qualify it.
        if(this.TopicTransformation.TransformationArguments.TryGetValue(
          nameof(TopicTransformationCore.BibliographyDataFile), out TransformationArgument bibliographyFile))
        {
            string filename = bibliographyFile.Value;

            if(!String.IsNullOrWhiteSpace(filename))
            {
                if(!Path.IsPathRooted(filename))
                    filename = Path.Combine(currentBuild.CurrentProject.BasePath, filename);

                if(!File.Exists(filename))
                {
                    this.WriteMessage(MessageLevel.Warn, $"Bibliography data file '{filename}' not found.  " +
                        "Bibliography elements will be ignored.");
                }
                else
                    this.TopicTransformation.BibliographyDataFile = filename;
            }
        }

        // Add this instance to the component container so that the build component factories can find it
        currentBuild.ComponentContainer.ComposeParts(this);
    }
    #endregion

    #region IDisposable implementation
    //=====================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        this.ClearComponents();

        messageLog?.Dispose();

        GC.SuppressFinalize(this);
    }
    #endregion

    #region IBuildAssembler implementation
    //=====================================================================

    /// <inheritdoc />
    public MessageLevel VerbosityLevel { get; }

    /// <inheritdoc />
    public TopicTransformationCore TopicTransformation { get; }

    /// <inheritdoc />
    public IEnumerable<BuildComponentCore> BuildComponents => components;

    /// <inheritdoc />
    public IEnumerable<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> SyntaxGeneratorComponents =>
        currentBuild.SyntaxGeneratorComponents;

    /// <inheritdoc />
    public IEnumerable<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> CopyFromIndexComponents =>
        currentBuild.CopyFromIndexComponents;

    /// <inheritdoc />
    public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();

    /// <inheritdoc />
    public event EventHandler ComponentEvent;

    /// <inheritdoc />
    public void OnComponentEvent(object sender, EventArgs e)
    {
        this.ComponentEvent?.Invoke(sender, e);
    }

    /// <inheritdoc />
    public void AddComponents(XPathNavigator configuration)
    {
#if DEBUG && WAIT_FOR_DEBUGGER
        while(!Debugger.IsAttached)
        {
#if NET9_0_OR_GREATER
            Console.WriteLine("DEBUG MODE: Waiting for debugger to attach (process ID: {0})",
                    Environment.ProcessId);
#else
            Console.WriteLine("DEBUG MODE: Waiting for debugger to attach (process ID: {0})",
                    Process.GetCurrentProcess().Id);
#endif
            System.Threading.Thread.Sleep(1000);
        }

        Debugger.Break();
#endif
        components.AddRange(this.LoadComponents(configuration));
    }

    /// <inheritdoc />
    public void ClearComponents()
    {
        foreach(var component in components)
            component.Dispose();

        components.Clear();

        this.Data.Clear();
    }

    /// <inheritdoc />
    public IEnumerable<BuildComponentCore> LoadComponents(XPathNavigator configuration)
    {
        if(configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        XPathNodeIterator componentNodes = configuration.Select("component");
        List<BuildComponentCore> loadedComponents = [];

        foreach(XPathNavigator componentNode in componentNodes)
        {
            loadedComponents.Add(this.LoadComponent(componentNode));
            currentBuild.CancellationToken.ThrowIfCancellationRequested();
        }

        return loadedComponents;
    }

    /// <inheritdoc />
    public BuildComponentCore LoadComponent(XPathNavigator configuration)
    {
        if(configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // Get the component ID
        string id = configuration.GetAttribute("id", String.Empty);

        if(String.IsNullOrWhiteSpace(id))
        {
            throw new BuilderException("BE0065", "Each component element must have an id " +
                "attribute that specifies the component's unique ID.");
        }

        // Load and instantiate the component
        BuildComponentCore component;

        try
        {
            if(!currentBuild.BuildComponents.TryGetValue(id, out BuildComponentFactory factory))
            {
                throw new BuilderException("BE0065", $"The component '{id}' was not found in any of the " +
                    "component assemblies");
            }

            component = factory.Create();
            component.Initialize(configuration);
        }
        catch(Exception e)
        {
            throw new BuilderException("BE0065", "An unexpected error occurred while attempting " +
                $"to create the build component '{id}'", e);
        }

        return component;
    }

    /// <inheritdoc />
    public void WriteMessage(string componentName, MessageLevel level, string key, string message)
    {
        string text;

        componentName ??= nameof(BuildComponentCore);

        if(level >= this.VerbosityLevel)
        {
            if(String.IsNullOrWhiteSpace(key))
                text = String.Format(CultureInfo.CurrentCulture, "{0}: {1}", componentName, message);
            else
                text = String.Format(CultureInfo.CurrentCulture, "{0}: [{1}] {2}", componentName, key, message);

            // If the background task has completed, we'll call the message logger action directly
            if(level == MessageLevel.Error)
            {
                if(!messageLog.IsAddingCompleted)
                    messageLog.Add((level, text));
                else
                {
                    // Prevent a possible race condition.  Ignore if still flushing the message log.  This
                    // case is highly unlikely and the text will be in the exception below.
                    if(messageLog.Count == 0)
                        this.WriteMessage(level, text);
                }

                if(System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                // Errors need to terminate the build so we'll throw an exception to terminate it
                throw new OperationCanceledException(text);
            }

            if(level == MessageLevel.Warn)
                Interlocked.Add(ref warningCount, 1);

            if(!messageLog.IsAddingCompleted)
                messageLog.Add((level, text));
            else
            {
                // Prevent a possible race condition.  Ignore if still flushing the message log.  Not ideal
                // here but it's an extremely rare occurrence likely related to a component logging a message
                // such as the save component perhaps raising a component event after all topics have been
                // built while it waits for it's queue to empty and the event handler tries to log a message
                // and the message log is still being flushed as well.
                if(messageLog.Count == 0)
                    this.WriteMessage(level, text);
            }
        }
    }
    #endregion

    #region Other methods
    //=====================================================================

    /// <summary>
    /// This is used to build the topics for the current build
    /// </summary>
    public void BuildTopics()
    {
        Task builder = Task.Factory.StartNew(() =>
        {
            // Log unhandled elements during transformation as diagnostic messages
            void unhandledElementLogger(object s, UnhandledElementEventArgs e) =>
                this.WriteMessage(nameof(TopicTransformation), MessageLevel.Diagnostic, e.Key,
                    $"Unhandled transformation element: {e.ElementName}  Parent element: {e.ParentElementName}");

            try
            {
                this.TopicTransformation.UnhandledElement += unhandledElementLogger;

                using var reader = XmlReader.Create(currentBuild.BuildAssemblerConfigurationFile,
                  new XmlReaderSettings { CloseInput = true });
                var configuration = new XPathDocument(reader);

                XPathNavigator configNav = configuration.CreateNavigator();

                if(this.VerbosityLevel > MessageLevel.Info)
                    messageLog.Add((MessageLevel.Info, "Loading configuration..."));

                // Load the build components
                XPathNavigator componentsNode = configNav.SelectSingleNode(
                    "/configuration/dduetools/builder/components");

                if(componentsNode != null)
                    this.AddComponents(componentsNode);

                currentBuild.CancellationToken.ThrowIfCancellationRequested();

                // Proceed through the build manifest, processing all topics named there
                if(this.VerbosityLevel > MessageLevel.Info)
                    messageLog.Add((MessageLevel.Info, "Processing topics..."));

                int count = this.Apply(ReadManifest(currentBuild.BuildAssemblerManifestFile));

                messageLog.Add((MessageLevel.Info, String.Format(CultureInfo.CurrentCulture,
                    "Processed {0} topic(s)", count)));

                if(warningCount != 0)
                {
                    messageLog.Add((MessageLevel.Info, String.Format(CultureInfo.CurrentCulture,
                        "{0} warning(s)", warningCount)));
                }
            }
            finally
            {
                messageLog.CompleteAdding();
                this.TopicTransformation.UnhandledElement -= unhandledElementLogger;
            }
        }, currentBuild.CancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        Task logger = Task.Factory.StartNew(() =>
        {
            foreach(var msg in messageLog.GetConsumingEnumerable())
                this.WriteMessage(msg.Level, msg.Message);
        }, currentBuild.CancellationToken, TaskCreationOptions.None, TaskScheduler.Default);

        Task.WaitAll([builder, logger], currentBuild.CancellationToken);
    }

    /// <summary>
    /// This is used to read a manifest file to extract topic IDs for processing
    /// </summary>
    /// <param name="manifest">The manifest file to read</param>
    /// <returns>An enumerable list of topic IDs</returns>
    private static IEnumerable<(string Id, string TopicType)> ReadManifest(string manifest)
    {
        string id, type;

        using var reader = XmlReader.Create(manifest, new XmlReaderSettings { CloseInput = true });
        
        reader.MoveToContent();

        while(reader.Read())
        {
            if(reader.NodeType == XmlNodeType.Element && reader.LocalName == "topic")
            {
                id = reader.GetAttribute("id");
                type = reader.GetAttribute("type");

                if(String.IsNullOrEmpty(id))
                    throw new InvalidOperationException("A manifest topic is missing the required id attribute");

                if(String.IsNullOrEmpty(type))
                    throw new InvalidOperationException("A manifest topic is missing the required type attribute");

                yield return (id, type);
            }
        }
    }

    /// <summary>
    /// Apply the current set of components to the given list of topics
    /// </summary>
    /// <param name="topics">The enumerable list of topic IDs</param>
    /// <returns>A count of the number of topics processed</returns>
    private int Apply(IEnumerable<(string Id, string TopicType)> topics)
    {
        int count = 0;

        if(topics == null)
            throw new ArgumentNullException(nameof(topics));

        foreach(var topic in topics)
        {
            this.WriteMessage(null, MessageLevel.Info, null, $"Building topic {topic.Id}");

            // Create the document.  The root node is always called "document" and the topic type is added
            // using the "type" attribute.  This can be used to run a different set of components based on
            // the document type.
            XmlDocument document = new()
            {
                PreserveWhitespace = true
            };

            var element = document.CreateElement("document");
            var attr = document.CreateAttribute("type");
            attr.Value = topic.TopicType;

            element.Attributes.Append(attr);
            document.AppendChild(element);

            // Apply the component stack
            foreach(BuildComponentCore component in components)
            {
                component.Apply(document, topic.Id);
                currentBuild.CancellationToken.ThrowIfCancellationRequested();
            }

            count++;
        }

        return count;
    }

    /// <summary>
    /// Write a message to the build log
    /// </summary>
    /// <param name="level">The log level of the message</param>
    /// <param name="message">The message to log</param>
    private void WriteMessage(MessageLevel level, string message)
    {
        switch(level)
        {
            case MessageLevel.Warn:
                currentBuild.ReportWarning("BE0066", message);
                break;

            case MessageLevel.Error:
                currentBuild.ReportError(currentBuild.CurrentBuildStep, "BE0065", message);
                break;

            default:     // Info or diagnostic
                currentBuild.ReportProgress(message);
                break;
        }
    }
    #endregion
}
