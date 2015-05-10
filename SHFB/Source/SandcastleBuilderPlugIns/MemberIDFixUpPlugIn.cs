//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : MemberIdFixUpPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/24/2015
// Note    : Copyright 2014-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a plug-in that is used to fix up member IDs in the XML comments files due to quirks in
// the various compilers that cause a mismatch between the member IDs in the XML comments and the reflection
// data.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// =====================================================================================================
// 11/14/2014  EFW  Created the code
//===============================================================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.XPath;

using SandcastleBuilder.Utils;
using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.BuildEngine;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to fix up member IDs in the XML comments files due to quirks in the various
    /// compilers that cause a mismatch between the member IDs in the XML comments and the reflection data.
    /// </summary>
    [HelpFileBuilderPlugInExport("Member ID Fix-Ups", IsConfigurable = true, RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to fix up member IDs in the XML comments files due to quirks in " +
        "the various compilers that cause a mismatch between the member IDs in the XML comments and the " +
        "reflection data.")]
    public sealed class MemberIdFixUpPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private List<ExecutionPoint> executionPoints;
        private BuildProcess builder;

        private List<MemberIdMatchExpression> expressions;
        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints
        {
            get
            {
                if(executionPoints == null)
                    executionPoints = new List<ExecutionPoint>
                    {
                        // This one has a lower priority as it fixes stuff that the other plug-ins might add
                        new ExecutionPoint(BuildStep.ValidatingDocumentationSources, ExecutionBehaviors.After, 100)
                    };

                return executionPoints;
            }
        }

        /// <summary>
        /// This method is used by the Sandcastle Help File Builder to let the plug-in perform its own
        /// configuration.
        /// </summary>
        /// <param name="project">A reference to the active project</param>
        /// <param name="currentConfig">The current configuration XML fragment</param>
        /// <returns>A string containing the new configuration XML fragment</returns>
        /// <remarks>The configuration data will be stored in the help file builder project</remarks>
        public string ConfigurePlugIn(SandcastleProject project, string currentConfig)
        {
            using(MemberIdFixUpPlugInConfigDlg dlg = new MemberIdFixUpPlugInConfigDlg(currentConfig))
            {
                if(dlg.ShowDialog() == DialogResult.OK)
                    currentConfig = dlg.Configuration;
            }

            return currentConfig;
        }

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(BuildProcess buildProcess, XPathNavigator configuration)
        {
            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            expressions = new List<MemberIdMatchExpression>();

            foreach(XPathNavigator nav in configuration.Select("configuration/expressions/expression"))
                expressions.Add(new MemberIdMatchExpression
                {
                    MatchExpression = nav.GetAttribute("matchExpression", String.Empty),
                    ReplacementValue = nav.GetAttribute("replacementValue", String.Empty),
                    MatchAsRegEx = Convert.ToBoolean(nav.GetAttribute("matchAsRegEx", String.Empty),
                        CultureInfo.InvariantCulture)
                });

            if(expressions.Count == 0)
                throw new BuilderException("MNF0001", "No fix-up expressions have been defined for the Member " +
                    "Name Fix-Up plug-in");
        }

        /// <summary>
        /// This method is used to execute the plug-in during the build process
        /// </summary>
        /// <param name="context">The current execution context</param>
        public void Execute(ExecutionContext context)
        {
            builder.ReportProgress("Fixing up member IDs in comments...");

            foreach(var commentsFile in builder.CommentsFiles)
            {
                // Use the instance to get the comments in case changes were made elsewhere and not save yet
                string content = commentsFile.Comments.OuterXml;

                foreach(var matchExpr in expressions)
                    if(matchExpr.MatchAsRegEx)
                        content = matchExpr.RegularExpression.Replace(content, matchExpr.ReplacementValue);
                    else
                        content = content.Replace(matchExpr.MatchExpression, matchExpr.ReplacementValue);

                using(StreamWriter sw = new StreamWriter(commentsFile.SourcePath, false, commentsFile.Encoding))
                {
                    sw.Write(content);
                }

                // Force a reload so that any further references to the content get our updated content
                commentsFile.ForceReload();
            }
        }
        #endregion

        #region IDisposable implementation
        //=====================================================================

        /// <summary>
        /// This implements the Dispose() interface to properly dispose of the plug-in object
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose of in this one
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
