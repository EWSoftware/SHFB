// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/16/2012 - EFW - Added Diagnostic message level type for diagnostic messages.  This allows messages
// to appear regardless of the verbosity level.

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace Microsoft.Ddue.Tools
{
    public abstract class BuildComponent : IDisposable
    {
        protected BuildComponent(BuildAssembler assembler, XPathNavigator configuration)
        {
            this.assembler = assembler;
            WriteMessage(MessageLevel.Info, "Instantiating component.");
        }

        public abstract void Apply(XmlDocument document, string key);

        public virtual void Apply(XmlDocument document)
        {
            Apply(document, null);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        // shared data

        private BuildAssembler assembler;

        public BuildAssembler BuildAssembler
        {
            get
            {
                return (assembler);
            }
        }

        private static Dictionary<string, object> data = new Dictionary<string, object>();

        protected static Dictionary<string, object> Data
        {
            get
            {
                return (data);
            }
        }

        // component messaging facility

        protected void OnComponentEvent(EventArgs e)
        {
            assembler.OnComponentEvent(this.GetType(), e);
        }

        protected void WriteMessage(MessageLevel level, string message)
        {
            if(level == MessageLevel.Ignore)
                return;
            MessageHandler handler = assembler.MessageHandler;
            if(handler != null)
                handler(this.GetType(), level, message);
        }

    }

    /// <summary>
    /// This enumerated type defines the message logging levels
    /// </summary>
    public enum MessageLevel
    {
        /// <summary>Do not show at all</summary>
        Ignore,
        /// <summary>Informational message</summary>
        Info,
        /// <summary>A warning message (a minor problem)</summary>
        Warn,
        /// <summary>An error message (a major problem that will stop the build)</summary>
        Error,
        /// <summary>A diagnostic message, useful for debugging</summary>
        Diagnostic
    }

    /// <summary>
    /// The message handler delegate
    /// </summary>
    /// <param name="component">The component type reporting the message</param>
    /// <param name="level">The message level</param>
    /// <param name="message">The message to report</param>
    public delegate void MessageHandler(Type component, MessageLevel level, string message);
}
