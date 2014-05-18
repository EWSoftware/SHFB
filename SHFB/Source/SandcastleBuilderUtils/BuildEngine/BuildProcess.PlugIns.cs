//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.PlugIns.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/14/2014
// Note    : Copyright 2007-2014, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the methods that handle the plug-ins during the build process
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: http://SHFB.CodePlex.com.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
// Version     Date     Who  Comments
// ==============================================================================================================
// 1.5.2.0  09/12/2007  EFW  Created the code
// 1.6.0.6  03/13/2008  EFW  Wrapped plug-in log output in an XML element
// 1.8.0.1  11/14/2008  EFW  Added execution priority support
// -------  12/18/2013  EFW  Updated to use MEF for the plug-ins
//          05/14/2014  EFW  Added support for presentation style plug-in dependencies
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using SandcastleBuilder.Utils.BuildComponent;

namespace SandcastleBuilder.Utils.BuildEngine
{
    public partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        private Dictionary<string, IPlugIn> loadedPlugIns;
        private BuildStep lastBeforeStep, lastAfterStep;
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This returns a <see cref="Dictionary{TKey, TValue}"/> containing the currently loaded plug-ins.
        /// </summary>
        /// <value>The key is the plug in ID.  The value is a reference to an <see cref="IPlugIn"/> interface
        /// for the plug-in.</value>
        /// <remarks>This allows you to access other plug-ins to facilitate sharing of information between them.</remarks>
        public Dictionary<string, IPlugIn> LoadedPlugIns
        {
            get { return loadedPlugIns; }
        }
        #endregion

        /// <summary>
        /// Load and initialize the plug-ins used by this project
        /// </summary>
        /// <exception cref="BuilderException">This is thrown if a requested plug-in is not found</exception>
        private void LoadPlugIns()
        {
            Lazy<IPlugIn, IPlugInMetadata> plugIn;
            IPlugIn instance;
            XmlDocument config;
            StringBuilder sb = new StringBuilder(256);

            this.ReportProgress("Loading and initializing plug-ins...");
            loadedPlugIns = new Dictionary<string, IPlugIn>();

            var availablePlugs = componentContainer.GetExports<IPlugIn, IPlugInMetadata>();
            var projectPlugIns = new Dictionary<string, string>();

            // Get the project plug-ins first
            foreach(var kv in project.PlugInConfigurations)
            {
                if(!kv.Value.Enabled)
                    this.ReportProgress("{0} plug-in is disabled and will not be loaded", kv.Key);
                else
                    projectPlugIns.Add(kv.Key, kv.Value.Configuration);
            }

            // Add presentation style plug-in dependencies that are not in the project already
            foreach(var dp in presentationStyle.PlugInDependencies)
                if(!projectPlugIns.ContainsKey(dp.Id))
                    projectPlugIns.Add(dp.Id, dp.Configuration);

            // Note that a list of failures is accumulated as we may need to run the Completion Notification
            // plug-in to report the failures when done.
            foreach(var kv in projectPlugIns)
            {
                plugIn = null;

                try
                {
                    plugIn = availablePlugs.FirstOrDefault(p => p.Metadata.Id == kv.Key);

                    if(plugIn == null)
                    {
                        sb.AppendFormat("Error: Unable to locate plug-in '{0}' in any of the component or " +
                            "project folders and it cannot be used.\r\n", kv.Key);
                        continue;
                    }

                    // For partial builds, only plug-ins that run in partial builds are loaded
                    if(this.PartialBuildType == PartialBuildType.None || plugIn.Metadata.RunsInPartialBuild)
                    {
                        // Plug-ins are singletons and will be disposed of by the composition container when it
                        // is disposed of.
                        instance = plugIn.Value;

                        config = new XmlDocument();
                        config.LoadXml(kv.Value);

                        instance.Initialize(this, config.CreateNavigator());

                        loadedPlugIns.Add(kv.Key, instance);
                    }
                }
                catch(Exception ex)
                {
                    BuilderException bex  = ex as BuilderException;

                    if(bex != null)
                        sb.AppendFormat("{0}: {1}\r\n", bex.ErrorCode, bex.Message);
                    else
                        sb.AppendFormat("{0}: Unexpected error: {1}\r\n",
                            (plugIn != null) ? plugIn.Metadata.Id : kv.Key, ex.ToString());
                }
            }

            if(sb.Length != 0)
            {
                sb.Insert(0, "Plug-in loading errors:\r\n");
                throw new BuilderException("BE0028", sb.ToString());
            }
        }

        /// <summary>
        /// Execute all plug-ins that need to execute in the given build step that have the given execution
        /// behavior.
        /// </summary>
        /// <param name="behavior">The execution behavior</param>
        /// <returns>True if at least one plug-in was executed or false if no plug-ins were executed.</returns>
        /// <remarks>Plug-ins will execute based on their execution priority.  Those with a higher priority value
        /// will execute before those with a lower value.  Plug-ins with identical priority values may execute
        /// in any order within their group.</remarks>
        private bool ExecutePlugIns(ExecutionBehaviors behavior)
        {
            List<IPlugIn> executeList;
            ExecutionContext context;
            BuildStep step;
            int numberExecuted = 0;

            if(loadedPlugIns == null)
                return false;

            step = progressArgs.BuildStep;

            // Find plug-ins that need to be executed
            executeList = loadedPlugIns.Values.Where(p => p.ExecutionPoints.RunsAt(step, behavior)).ToList();

            if(executeList.Count == 0)
                return false;

            // Sort by execution priority in descending order
            executeList.Sort((x, y) => y.ExecutionPoints.PriorityFor(step, behavior) -
                x.ExecutionPoints.PriorityFor(step, behavior));

            context = new ExecutionContext(step, behavior);

            foreach(IPlugIn plugIn in executeList)
            {
                var metadata = (HelpFileBuilderPlugInExportAttribute)plugIn.GetType().GetCustomAttributes(
                    typeof(HelpFileBuilderPlugInExportAttribute), false).First();

                try
                {
                    // Wrap plug-in output in an element so that it can be formatted differently
                    swLog.WriteLine("<plugIn name=\"{0}\" behavior=\"{1}\" priority=\"{2}\">", metadata.Id,
                        behavior, plugIn.ExecutionPoints.PriorityFor(step, behavior));

                    context.Executed = true;
                    plugIn.Execute(context);

                    swLog.Write("</plugIn>");
                }
                catch(Exception ex)
                {
                    swLog.WriteLine("</plugIn>");

                    throw new BuilderException("BE0029", "Unexpected error while executing plug-in '" +
                        metadata.Id + "': " + ex.ToString(), ex);
                }

                if(context.Executed)
                    numberExecuted++;
            }

            return (numberExecuted != 0);
        }

        /// <summary>
        /// This can be used by plug-ins using the
        /// <see cref="ExecutionBehaviors.InsteadOf" /> execution behavior to execute plug-ins that want to run
        /// before the plug-in executes its main processing.
        /// </summary>
        /// <remarks>This will only run once per step.  Any subsequent calls by other plug-ins will be ignored.</remarks>
        public void ExecuteBeforeStepPlugIns()
        {
            // Only execute once per step
            if(lastBeforeStep != progressArgs.BuildStep)
            {
                this.ExecutePlugIns(ExecutionBehaviors.Before);
                lastBeforeStep = progressArgs.BuildStep;
            }
        }

        /// <summary>
        /// This can be used by plug-ins using the
        /// <see cref="ExecutionBehaviors.InsteadOf" /> execution behavior to execute plug-ins that want to run
        /// after the plug-in has executed its main processing.
        /// </summary>
        /// <remarks>This will only run once per step.  Any subsequent calls by other plug-ins will be ignored.</remarks>
        public void ExecuteAfterStepPlugIns()
        {
            // Only execute once per step
            if(lastAfterStep != progressArgs.BuildStep)
            {
                this.ExecutePlugIns(ExecutionBehaviors.After);
                lastAfterStep = progressArgs.BuildStep;
            }
        }
    }
}
