//===============================================================================================================
// System  : Sandcastle Tools - Sandcastle Tools Core Class Library
// File    : IBuildAssembler.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/18/2025
// Note    : Copyright 2022-2025, Eric Woodruff, All rights reserved
//
// This file contains the interface used to define the common Build Assembler tool methods
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 02/26/2021  EFW  Created the code
//===============================================================================================================


using System;
using System.Collections.Generic;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler.BuildComponent;
using Sandcastle.Core.BuildAssembler.SyntaxGenerator;
using Sandcastle.Core.PresentationStyle.Transformation;

namespace Sandcastle.Core.BuildAssembler
{
    /// <summary>
    /// This interface defines the common Build Assembler tool properties, events, and methods used to generate
    /// help topics.
    /// </summary>
    public interface IBuildAssembler
    {
        /// <summary>
        /// This read-only property returns the verbosity level for the message handlers
        /// </summary>
        /// <value>The value can be <c>Info</c>, <c>Warn</c>, or <c>Error</c>.  The default level is
        /// <see cref="MessageLevel.Info"/> so that all messages are displayed.  Setting it to a higher
        /// level will suppress messages below the given level.</value>
        /// <remarks>It is up to the message handler to make use of this property</remarks>
        MessageLevel VerbosityLevel { get; }
        
        /// <summary>
        /// This read-only property returns the topic transformation to use for the presentation style
        /// </summary>
        TopicTransformationCore TopicTransformation { get; }

        /// <summary>
        /// This read-only property returns the current list of build components
        /// </summary>
        IEnumerable<BuildComponentCore> BuildComponents { get; }

        /// <summary>
        /// This read-only property returns the syntax generator component factories
        /// </summary>
        IEnumerable<Lazy<ISyntaxGeneratorFactory, ISyntaxGeneratorMetadata>> SyntaxGeneratorComponents { get; }

        /// <summary>
        /// This read-only property returns the copy from index component factories
        /// </summary>
        IEnumerable<Lazy<ICopyComponentFactory, ICopyComponentMetadata>> CopyFromIndexComponents { get; }

        /// <summary>
        /// This read-only property returns a dictionary that can be used to store information shared between
        /// build components.
        /// </summary>
        IDictionary<string, object> Data { get; }

        /// <summary>
        /// This event is raised when a component wants to signal that something of interest has happened
        /// </summary>
        event EventHandler ComponentEvent;

        /// <summary>
        /// This is used to raise the <see cref="ComponentEvent" />
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event arguments</param>
        void OnComponentEvent(object sender, EventArgs e);

        /// <summary>
        /// Add a set of components based on the given configuration
        /// </summary>
        /// <param name="configuration">The configuration containing the component definitions</param>
        void AddComponents(XPathNavigator configuration);

        /// <summary>
        /// Dispose of all components and clear them from the collection
        /// </summary>
        void ClearComponents();

        /// <summary>
        /// This is used to load a set of components in a configuration and return them as an enumerable list
        /// </summary>
        /// <param name="configuration">The configuration node from which to get the components</param>
        /// <returns>An enumerable list of components created based on the configuration information</returns>
        IEnumerable<BuildComponentCore> LoadComponents(XPathNavigator configuration);

        /// <summary>
        /// This is used to create a component based on the given configuration
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        /// <returns>A component created using the given definition</returns>
        /// <exception cref="ArgumentNullException">This is thrown if <paramref name="configuration"/> is null</exception>
        BuildComponentCore LoadComponent(XPathNavigator configuration);

        /// <summary>
        /// Write a component message to the message log
        /// </summary>
        /// <param name="componentName">The name of the component writing the message</param>
        /// <param name="level">The message level</param>
        /// <param name="key">An optional topic key related to the message or null if there isn't one</param>
        /// <param name="message">The message to write to the console</param>
        /// <remarks>If the message level is below the current verbosity level setting, the message is ignored</remarks>
        void WriteMessage(string componentName, MessageLevel level, string key, string message);
    }
}
