// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added Diagnostic message level type for diagnostic messages.  This allows messages
// to appear regardless of the verbosity level.
// 10/14/2012 - EFW - Added support for topic ID and message parameters in the message logging methods.
// 01/05/2012 - EFW - Made the WriteMessage method public so that subcomponents with a reference to a build
// component can log messages easily.
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly
// 12/23/2013 - EFW - Updated the build components to be discoverable via MEF
// 12/08/2015 - EFW - Added support for component group ID

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Sandcastle.Core.BuildAssembler.BuildComponent
{
    /// <summary>
    /// This is the abstract base class for all build components
    /// </summary>
    public abstract class BuildComponentCore : IDisposable
    {
        #region Private data members
        //=====================================================================

        private static Dictionary<string, object> data = new Dictionary<string, object>();

        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This read-only property returns a reference to the build assembler instance using the component
        /// </summary>
        public BuildAssemblerCore BuildAssembler { get; private set; }

        /// <summary>
        /// This read-only property returns a static dictionary that can be used to store information shared
        /// between build components.
        /// </summary>
        protected static Dictionary<string, object> Data
        {
            get { return data; }
        }

        /// <summary>
        /// This is used to set an optional group ID for use with component events
        /// </summary>
        /// <remarks>If not overridden, the default group ID is null (no group)</remarks>
        public virtual string GroupId { get; set; }

        /// <summary>
        /// Reserved for future use
        /// </summary>
        /// <value>This property is not currently used.  It is reserved for future use.</value>
        public virtual bool IsThreadSafe
        {
            get { return false; }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected BuildComponentCore(BuildAssemblerCore buildAssembler)
        {
            this.BuildAssembler = buildAssembler;
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This handles garbage collection to ensure proper disposal of the build component if not done
        /// explicitly with <see cref="Dispose()"/>.
        /// </summary>
        ~BuildComponentCore()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the build component.
        /// </summary>
        /// <overloads>There are two overloads for this method.</overloads>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// This can be overridden by derived classes to add their own disposal code if necessary.
        /// </summary>
        /// <param name="disposing">Pass true to dispose of the managed and unmanaged resources or false to just
        /// dispose of the unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Nothing to dispose of in this one
        }
        #endregion

        #region Abstract methods
        //=====================================================================

        /// <summary>
        /// This abstract method must be overridden to initialize the component
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        public abstract void Initialize(XPathNavigator configuration);

        /// <summary>
        /// This abstract method must be overridden to apply the build component's changes to the specified
        /// document.
        /// </summary>
        /// <param name="document">The document that the build component can modify</param>
        /// <param name="key">The key that uniquely identifies the document</param>
        public abstract void Apply(XmlDocument document, string key);
        #endregion

        #region Component messaging methods
        //=====================================================================

        /// <summary>
        /// This can be used to raise the <see cref="Sandcastle.Core.BuildAssembler.BuildAssemblerCore.ComponentEvent"/>
        /// event with the specified event arguments.
        /// </summary>
        /// <param name="e">The event arguments.  This can be <see cref="EventArgs.Empty"/> or a derived event
        /// arguments class containing information to pass to the event handlers.</param>
        protected void OnComponentEvent(EventArgs e)
        {
            if(this.BuildAssembler != null)
                this.BuildAssembler.OnComponentEvent(this, e);
        }

        /// <summary>
        /// This can be used to report a message
        /// </summary>
        /// <param name="level">The message level</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">An optional list of arguments to format into the message</param>
        public void WriteMessage(MessageLevel level, string message, params object[] args)
        {
            if(level != MessageLevel.Ignore && this.BuildAssembler != null)
                this.BuildAssembler.WriteMessage(this.GetType(), level, null, (args.Length == 0) ? message :
                    String.Format(CultureInfo.CurrentCulture, message, args));
        }

        /// <summary>
        /// This can be used to report a message for a specific topic ID
        /// </summary>
        /// <param name="key">The topic key related to the message</param>
        /// <param name="level">The message level</param>
        /// <param name="message">The message to report</param>
        /// <param name="args">An optional list of arguments to format into the message</param>
        /// <remarks>This is useful for warning and error messages as the topic ID will be included even when
        /// the message level is set to warnings or higher.  In such cases, the informational messages containing
        /// the "building topic X" messages are suppressed.</remarks>
        public void WriteMessage(string key, MessageLevel level, string message, params object[] args)
        {
            if(level != MessageLevel.Ignore && this.BuildAssembler != null)
                this.BuildAssembler.WriteMessage(this.GetType(), level, key, (args.Length == 0) ? message :
                    String.Format(CultureInfo.CurrentCulture, message, args));
        }
        #endregion
    }
}
