//===============================================================================================================
// System  : Sandcastle Help File Builder Plug-Ins
// File    : MemberIdFixUpPlugIn.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 06/20/2025
// Note    : Copyright 2014-2025, Eric Woodruff, All rights reserved
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
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Sandcastle.Core.BuildEngine;
using Sandcastle.Core.PlugIn;

namespace SandcastleBuilder.PlugIns
{
    /// <summary>
    /// This plug-in class is used to fix up member IDs in the XML comments files due to quirks in the various
    /// compilers that cause a mismatch between the member IDs in the XML comments and the reflection data.
    /// </summary>
    [HelpFileBuilderPlugInExport("Member ID Fix-Ups", RunsInPartialBuild = true,
      Version = AssemblyInfo.ProductVersion, Copyright = AssemblyInfo.Copyright,
      Description = "This plug-in is used to fix up member IDs in the XML comments files due to quirks in " +
        "the various compilers that cause a mismatch between the member IDs in the XML comments and the " +
        "reflection data.")]
    public sealed class MemberIdFixUpPlugIn : IPlugIn
    {
        #region Private data members
        //=====================================================================

        private IBuildProcess builder;
        private List<MemberIdMatchExpression> expressions;

        #endregion

        #region IPlugIn implementation
        //=====================================================================

        /// <summary>
        /// This read-only property returns a collection of execution points that define when the plug-in should
        /// be invoked during the build process.
        /// </summary>
        public IEnumerable<ExecutionPoint> ExecutionPoints { get; } =
        [
            // This one has a lower priority as it fixes stuff that the other plug-ins might add
            new ExecutionPoint(BuildStep.ValidatingDocumentationSources, ExecutionBehaviors.After, 100)
        ];

        /// <summary>
        /// This method is used to initialize the plug-in at the start of the build process
        /// </summary>
        /// <param name="buildProcess">A reference to the current build process</param>
        /// <param name="configuration">The configuration data that the plug-in should use to initialize itself</param>
        public void Initialize(IBuildProcess buildProcess, XElement configuration)
        {
            if(configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            builder = buildProcess;

            var metadata = (HelpFileBuilderPlugInExportAttribute)this.GetType().GetCustomAttributes(
                typeof(HelpFileBuilderPlugInExportAttribute), false).First();

            builder.ReportProgress("{0} Version {1}\r\n{2}", metadata.Id, metadata.Version, metadata.Copyright);

            expressions = [];

            foreach(var expr in configuration.Descendants("expression"))
            {
                expressions.Add(new MemberIdMatchExpression
                {
                    MatchExpression = expr.Attribute("matchExpression").Value,
                    ReplacementValue = expr.Attribute("replacementValue").Value,
                    MatchAsRegEx = (bool)expr.Attribute("matchAsRegEx")
                });
            }

            if(expressions.Count == 0)
            {
                throw new BuilderException("MNF0001", "No fix-up expressions have been defined for the Member " +
                    "Name Fix-Up plug-in");
            }
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
                {
                    if(matchExpr.MatchAsRegEx)
                        content = matchExpr.RegularExpression.Replace(content, matchExpr.ReplacementValue);
                    else
                        content = content.Replace(matchExpr.MatchExpression, matchExpr.ReplacementValue);
                }

                // Don't use a simplified using here.  We want to ensure the file is closed before reloading it.
                using(StreamWriter sw = new(commentsFile.SourcePath, false, commentsFile.Encoding))
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
