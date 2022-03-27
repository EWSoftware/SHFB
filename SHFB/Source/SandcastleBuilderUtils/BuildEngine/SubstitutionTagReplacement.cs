//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SubstitutionTagReplacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 03/23/2022
// Note    : Copyright 2015-2022, Eric Woodruff, All rights reserved
//
// This file contains the class used to handle substitution tag replacement in build template files
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 05/10/2015  EFW  Refactored the substitution tag replacement code and moved it into its own class
//===============================================================================================================

// Ignore Spelling: concat Url img src onclick javascript

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;
using Sandcastle.Core.Reflection;

using SandcastleBuilder.Utils.Design;

namespace SandcastleBuilder.Utils.BuildEngine
{
    /// <summary>
    /// This class handles substitution tag replacement in build template files
    /// </summary>
    /// <remarks><para>Replacement tags appear in the form of a tag name prefixed with an '@' and enclosed in
    /// curly braces (<c>{@PropertyName}</c>).  An optional format specifier is also allowed if needed such
    /// as for date formatting (<c>{@BuildDate:MMMM d, yyyy}</c>).</para>
    /// 
    /// <para>Methods in this class tagged with the <see cref="SubstitutionTagAttribute"/> attribute represent
    /// substitution tags that require additional handling.  For simple types that require no special handling
    /// and those that require only minor changes, the value returned by
    /// <see cref="SandcastleProject.ReplacementValueFor"/> is used and there is no corresponding method in this
    /// class.</para></remarks>
    public class SubstitutionTagReplacement
    {
        #region Private data members
        //=====================================================================

        private readonly BuildProcess currentBuild;
        private readonly SandcastleProject sandcastleProject;
        private readonly Project msbuildProject;
        private readonly PresentationStyleSettings presentationStyle;

        private readonly Dictionary<string, MethodInfo> methodCache;

        private static readonly Regex reField = new Regex(@"{@(?<Field>\w*?)(:(?<Format>.*?))?}");

        private readonly MatchEvaluator fieldMatchEval;
        private string fieldFormat;

        private readonly StringBuilder replacementValue;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentBuild">The current build for which to perform substitution tag replacement</param>
        public SubstitutionTagReplacement(BuildProcess currentBuild)
        {
            this.currentBuild = currentBuild ?? throw new ArgumentNullException(nameof(currentBuild));

            sandcastleProject = currentBuild.CurrentProject;
            msbuildProject = sandcastleProject.MSBuildProject;
            presentationStyle = currentBuild.PresentationStyle;

            replacementValue = new StringBuilder(10240);

            fieldMatchEval = new MatchEvaluator(OnFieldMatch);

            // Get the substitution tag methods so that we can invoke them.  The dictionary keys are the method
            // names and are case-insensitive.  Substitution tag methods take no parameters and return a value
            // that is convertible to a string.
            methodCache = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).Where(
                m => m.GetCustomAttribute(typeof(SubstitutionTagAttribute)) != null).ToDictionary(
                m => m.Name, m => m, StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Transformation methods
        //=====================================================================

        /// <summary>
        /// Transform the specified template text by replacing the substitution tags with the corresponding
        /// project property values.
        /// </summary>
        /// <param name="templateText">The template text to transform</param>
        /// <param name="args">An optional list of arguments to format into the  template before transforming it</param>
        /// <returns>The transformed text</returns>
        public string TransformText(string templateText, params object[] args)
        {
            if(String.IsNullOrWhiteSpace(templateText))
                return templateText ?? String.Empty;

            if(args != null && args.Length != 0)
                templateText = String.Format(CultureInfo.InvariantCulture, templateText, args);

            try
            {
                // Find and replace all substitution tags with a matching value from the project.  They can be
                // nested.
                while(reField.IsMatch(templateText))
                    templateText = reField.Replace(templateText, fieldMatchEval);
            }
            catch(BuilderException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0018", String.Format(CultureInfo.CurrentCulture,
                    "Unable to transform template text '{0}': {1}", templateText, ex.Message), ex);
            }

            return templateText;
        }

        /// <summary>
        /// Transform the specified template file by inserting the necessary values into the substitution tags
        /// and saving it to the destination folder.
        /// </summary>
        /// <param name="templateFile">The template file to transform</param>
        /// <param name="sourceFolder">The folder where the template is located</param>
        /// <param name="destFolder">The folder in which to save the transformed file</param>
        /// <returns>The path to the transformed file</returns>
        public string TransformTemplate(string templateFile, string sourceFolder, string destFolder)
        {
            Encoding enc = Encoding.Default;
            string templateText, transformedFile;

            if(templateFile == null)
                throw new ArgumentNullException(nameof(templateFile));

            if(sourceFolder == null)
                throw new ArgumentNullException(nameof(sourceFolder));

            if(destFolder == null)
                throw new ArgumentNullException(nameof(destFolder));

            if(sourceFolder.Length != 0 && sourceFolder[sourceFolder.Length - 1] != '\\')
                sourceFolder += @"\";

            if(destFolder.Length != 0 && destFolder[destFolder.Length - 1] != '\\')
                destFolder += @"\";

            try
            {
                // When reading the file, use the default encoding but detect the encoding if byte order marks
                // are present.
                templateText = Utility.ReadWithEncoding(sourceFolder + templateFile, ref enc);

                // Find and replace all substitution tags with a matching value from the project.  They can be
                // nested.
                while(reField.IsMatch(templateText))
                    templateText = reField.Replace(templateText, fieldMatchEval);

                transformedFile = destFolder + templateFile;

                // Write the file back out using its original encoding
                using(StreamWriter sw = new StreamWriter(transformedFile, false, enc))
                {
                    sw.Write(templateText);
                }
            }
            catch(BuilderException)
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0019", String.Format(CultureInfo.CurrentCulture,
                    "Unable to transform template '{0}': {1}", templateFile, ex.Message), ex);
            }

            return transformedFile;
        }

        /// <summary>
        /// Replace a substitution tag with a value from the project
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnFieldMatch(Match match)
        {
            string fieldName = match.Groups["Field"].Value, propertyValue;

            // See if a method exists first.  If so, we'll call it and return its value.
            if(methodCache.TryGetValue(fieldName, out MethodInfo method))
            {
                fieldFormat = match.Groups["Format"].Value;

                object result = method.Invoke(this, null);

                if(result == null)
                    return String.Empty;

                return result.ToString();
            }

            // Try for a local Sandcastle project property
            propertyValue = sandcastleProject.ReplacementValueFor(fieldName);

            if(propertyValue != null)
                return propertyValue;

            // Try for a custom MSBuild project property.  Use the last one since the original may be in a parent
            // project file or it may have been overridden from the command line.
            var buildProp = msbuildProject.AllEvaluatedProperties.LastOrDefault(
                p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if(buildProp != null)
                return buildProp.EvaluatedValue;

            // If not there, try the global properties.  If still not found, give up.
            string key = msbuildProject.GlobalProperties.Keys.FirstOrDefault(
                k => k.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

            if(key == null || !msbuildProject.GlobalProperties.TryGetValue(key, out propertyValue))
                switch(fieldName.ToUpperInvariant())
                {
                    case "REFERENCEPATH":       // These can be safely ignored if not found
                    case "OUTDIR":
                        propertyValue = String.Empty;
                        break;

                    default:
                        throw new BuilderException("BE0020", String.Format(CultureInfo.CurrentCulture,
                            "Unknown substitution tag ID: '{0}'", fieldName));
                }

            return propertyValue;
        }
        #endregion

        // All substitution tag members below are referenced indirectly, not through the code
#pragma warning disable IDE0051
#pragma warning disable CA1822

        #region Project and build folder substitution tags
        //=====================================================================

        /// <summary>
        /// The application data folder
        /// </summary>
        /// <returns>The application data folder.  This folder should exist if used.</returns>
        [SubstitutionTag]
        private string AppDataFolder()
        {
            return FolderPath.TerminatePath(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData), Constants.ProgramDataFolder));
        }

        /// <summary>
        /// The local data folder
        /// </summary>
        /// <returns>The local data folder.  This folder may not exist and we may need to create it.</returns>
        [SubstitutionTag]
        private string LocalDataFolder()
        {
            string folder = FolderPath.TerminatePath(Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), Constants.ProgramDataFolder));

            if(!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }

        /// <summary>
        /// The root Sandcastle Help File Builder folder
        /// </summary>
        /// <returns>The root Sandcastle Help File Builder folder including the trailing directory separator</returns>
        [SubstitutionTag]
        private string SHFBRoot()
        {
            return ComponentUtilities.RootFolder + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// The core components folder (those components distributed with the help file builder)
        /// </summary>
        /// <returns>The core components folder including the trailing directory separator</returns>
        [SubstitutionTag]
        private string CoreComponentsFolder()
        {
            return ComponentUtilities.CoreComponentsFolder + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// The third-party components folder
        /// </summary>
        /// <returns>The third-party components folder including the trailing directory separator</returns>
        [SubstitutionTag]
        private string ThirdPartyComponentsFolder()
        {
            return ComponentUtilities.ThirdPartyComponentsFolder + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// The current build's help file builder project folder
        /// </summary>
        /// <returns>The current build's help file builder project folder</returns>
        [SubstitutionTag]
        private string ProjectFolder()
        {
            return currentBuild.ProjectFolder;
        }

        /// <summary>
        /// The current build's HTML encoded help file builder project folder
        /// </summary>
        /// <returns>The current build's HTML encoded help file builder project folder</returns>
        [SubstitutionTag]
        private string HtmlEncProjectFolder()
        {
            return WebUtility.HtmlEncode(currentBuild.ProjectFolder);
        }

        /// <summary>
        /// The current build's output folder
        /// </summary>
        /// <returns>The current build's output folder</returns>
        [SubstitutionTag]
        private string OutputFolder()
        {
            return currentBuild.OutputFolder;
        }

        /// <summary>
        /// The current build's HTML encoded output folder
        /// </summary>
        /// <returns>The current build's HTML encoded output folder</returns>
        [SubstitutionTag]
        private string HtmlEncOutputFolder()
        {
            return WebUtility.HtmlEncode(currentBuild.OutputFolder);
        }

        /// <summary>
        /// The current build's working folder
        /// </summary>
        /// <returns>The current build's working folder including the trailing directory separator</returns>
        [SubstitutionTag]
        private string WorkingFolder()
        {
            return currentBuild.WorkingFolder;
        }

        /// <summary>
        /// The current build's HTML encoded working folder
        /// </summary>
        /// <returns>The current build's HTML encoded working folder</returns>
        [SubstitutionTag]
        private string HtmlEncWorkingFolder()
        {
            return WebUtility.HtmlEncode(currentBuild.WorkingFolder);
        }

        /// <summary>
        /// The current project's source code base path
        /// </summary>
        /// <returns>The current project's source code base path</returns>
        [SubstitutionTag]
        private string SourceCodeBasePath()
        {
            return currentBuild.CurrentProject.SourceCodeBasePath;
        }

        /// <summary>
        /// The missing source context warning setting
        /// </summary>
        /// <returns>The current project's missing source context warning setting</returns>
        [SubstitutionTag]
        private string WarnOnMissingSourceContext()
        {
            return currentBuild.CurrentProject.WarnOnMissingSourceContext.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The HTML Help 1 compiler path
        /// </summary>
        /// <returns>The HTML Help 1 compiler path</returns>
        [SubstitutionTag]
        private string HHCPath()
        {
            return currentBuild.Help1CompilerFolder;
        }

        /// <summary>
        /// The MEF component locations
        /// </summary>
        /// <returns>The MEF component locations</returns>
        [SubstitutionTag]
        private string ComponentLocations()
        {
            return String.Join("\r\n", sandcastleProject.ComponentSearchPaths.Select(p => $"<location folder=\"{p}\" />"));
        }

        /// <summary>
        /// The help format output paths
        /// </summary>
        /// <returns>The help format output paths</returns>
        [SubstitutionTag]
        private string HelpFormatOutputPaths()
        {
            replacementValue.Clear();

            // Add one entry for each help file format being generated
            foreach(string baseFolder in currentBuild.HelpFormatOutputFolders)
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<path value=\"{0}\" />",
                    baseFolder.Substring(currentBuild.WorkingFolder.Length));

            return replacementValue.ToString();
        }

        /// <summary>
        /// The old Sandcastle Tools path (obsolete)
        /// </summary>
        /// <returns>This is obsolete but will still appear in the older component and plug-in configurations.
        /// Throw an exception that describes what to do to fix it.  Eventually, this can be removed.</returns>
        [SubstitutionTag]
        private string SandcastlePath()
        {
            throw new BuilderException("BE0065", "One or more component or plug-in configurations in " +
                "this project contains an obsolete path setting.  Please remove the custom components " +
                "and plug-ins and add them again so that their configurations are updated.  See the " +
                "version release notes for information on breaking changes that require this update.");
        }
        #endregion

        #region Presentation style substitution tags
        //=====================================================================

        /// <summary>
        /// The presentation style folder
        /// </summary>
        /// <returns>The presentation style folder</returns>
        [SubstitutionTag]
        private string PresentationPath()
        {
            return FolderPath.TerminatePath(currentBuild.PresentationStyleFolder);
        }

        /// <summary>
        /// The code snippet grouping option
        /// </summary>
        /// <returns>The intermediate TOC XSL transformation parameters</returns>
        [SubstitutionTag]
        private string CodeSnippetGrouping()
        {
            return presentationStyle.SupportsCodeSnippetGrouping.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The HREF format for reference links
        /// </summary>
        /// <returns>The HREF format for reference links</returns>
        [SubstitutionTag]
        private string HRefFormat()
        {
            return ((sandcastleProject.HelpFileFormat & HelpFileFormats.Markdown) != 0) ?
                "<hrefFormat value=\"{0}\" />" : String.Empty;
        }
        #endregion

        #region Framework settings substitution tags
        //=====================================================================

        /// <summary>
        /// The target framework identifier (platform)
        /// </summary>
        /// <returns>The target framework identifier</returns>
        [SubstitutionTag]
        private string TargetFrameworkIdentifier()
        {
            // If the reflection data is for .NETStandard or .NET 5.0 use .NET Framework use v4.8
            if(currentBuild.FrameworkReflectionData.Platform == PlatformType.DotNet ||
              currentBuild.FrameworkReflectionData.Platform == PlatformType.DotNetStandard)
            {
                return PlatformType.DotNetFramework;
            }

            return currentBuild.FrameworkReflectionData.Platform;
        }

        /// <summary>
        /// The full framework version (Major.Minor[.Build[.Revision]]
        /// </summary>
        /// <returns>The full framework version</returns>
        [SubstitutionTag]
        private string FrameworkVersion()
        {
            return currentBuild.FrameworkReflectionData.Version.ToString();
        }

        /// <summary>
        /// The short framework version (Major.Minor[.Build])
        /// </summary>
        /// <returns>Typically returns a two digit version number.  However, if the build number is between 1 and
        /// 10, it will be included as well (i.e. v4.5.2, v4.6.1).</returns>
        [SubstitutionTag]
        private string FrameworkVersionShort()
        {
            Version v = currentBuild.FrameworkReflectionData.Version;

            // If the reflection data is for .NETStandard or .NET 5.0 use .NET Framework use v4.8
            if(currentBuild.FrameworkReflectionData.Platform == PlatformType.DotNet ||
              currentBuild.FrameworkReflectionData.Platform == PlatformType.DotNetStandard)
            {
                v = new Version(4, 8);
            }

            if(v.Build > 0 && v.Build < 10)
                return currentBuild.FrameworkReflectionData.Version.ToString(3);

            return v.ToString(2);
        }

        /// <summary>
        /// The reflection data set platform
        /// </summary>
        /// <returns>The reflection data set platform type</returns>
        [SubstitutionTag]
        private string ReflectionDataSetPlatform()
        {
            return currentBuild.FrameworkReflectionData.Platform;
        }

        /// <summary>
        /// The reflection data set platform version
        /// </summary>
        /// <returns>The reflection data set platform version</returns>
        [SubstitutionTag]
        private string ReflectionDataSetVersion()
        {
            return currentBuild.FrameworkReflectionData.Version.ToString();
        }

        /// <summary>
        /// The framework reflection data folder
        /// </summary>
        /// <returns>The framework reflection data folder</returns>
        [SubstitutionTag]
        private string FrameworkReflectionDataFolder()
        {
            return currentBuild.FrameworkReflectionDataFolder;
        }
        #endregion

        #region Build category substitution tags
        //=====================================================================

        /// <summary>
        /// Build assembler Save Component writer task cache capacity
        /// </summary>
        /// <returns>The cache capacity for the Save Component's writer task</returns>
        [SubstitutionTag]
        private string SaveComponentCacheCapacity()
        {
            return sandcastleProject.SaveComponentCacheCapacity.ToString(CultureInfo.InvariantCulture);
        }
        #endregion

        #region Help file category project substitution tags
        //=====================================================================

        /// <summary>
        /// The HTML encoded help name
        /// </summary>
        /// <returns>The HTML encoded help name</returns>
        [SubstitutionTag]
        private string HtmlEncHelpName()
        {
            return WebUtility.HtmlEncode(sandcastleProject.HtmlHelpName);
        }

        /// <summary>
        /// The script help title
        /// </summary>
        /// <returns>This is used when the title is passed as a parameter to a JavaScript function.  Single
        /// quotes in the value are escaped and the value is HTML encoded.</returns>
        [SubstitutionTag]
        private string ScriptHelpTitle()
        {
            return WebUtility.HtmlEncode(sandcastleProject.HelpTitle).Replace("'", @"\'");
        }

        /// <summary>
        /// The URL encoded help title
        /// </summary>
        /// <returns>Only &amp;, &lt;, &gt;, ", and space are replaced for now</returns>
        [SubstitutionTag]
        private string UrlEncHelpTitle()
        {
            // Any embedded substitution tags need to be replaced first so that their content is encoded too
            string helpTitle = TransformText(sandcastleProject.HelpTitle);

            return helpTitle.Replace("&", "%26").Replace("<", "%3C").Replace(">", "%3E").Replace(
                "\"", "%22").Replace(" ", "%20");
        }

        /// <summary>
        /// The root namespace title element
        /// </summary>
        /// <returns>If not set, it returns the localized title element include as the value</returns>
        [SubstitutionTag]
        private string RootNamespaceTitle()
        {
            return String.IsNullOrWhiteSpace(sandcastleProject.RootNamespaceTitle) ?
                "<include item=\"rootTopicTitleLocalized\"/>" : sandcastleProject.RootNamespaceTitle;
        }

        /// <summary>
        /// The language ID
        /// </summary>
        /// <returns>The language ID</returns>
        [SubstitutionTag]
        private string LangId()
        {
            return currentBuild.Language.LCID.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// The language locale ID and native name
        /// </summary>
        /// <returns>The language locale ID and native name</returns>
        [SubstitutionTag]
        private string Language()
        {
            return String.Format(CultureInfo.InvariantCulture, "0x{0:X} {1}", currentBuild.Language.LCID,
                currentBuild.Language.NativeName);
        }

        /// <summary>
        /// The locale in lowercase
        /// </summary>
        /// <returns>The locale ID in lowercase</returns>
        [SubstitutionTag]
        private string Locale()
        {
            return currentBuild.Language.Name.ToLowerInvariant();
        }

        /// <summary>
        /// The locale in mixed case
        /// </summary>
        /// <returns>The locale ID in mixed case</returns>
        [SubstitutionTag]
        private string LocaleMixedCase()
        {
            return currentBuild.Language.Name;
        }

        /// <summary>
        /// The copyright include item if wanted
        /// </summary>
        /// <returns>The include copyright info if there is a copyright HREF or copyright text</returns>
        [SubstitutionTag]
        private string Copyright()
        {
            return (sandcastleProject.CopyrightHref.Length != 0 || sandcastleProject.CopyrightText.Length != 0) ?
                "<include item=\"copyright\"/>" : String.Empty;
        }

        /// <summary>
        /// The copyright information
        /// </summary>
        /// <returns>The copyright information based on whether the HREF and/or text is specified</returns>
        [SubstitutionTag]
        private string CopyrightInfo()
        {
            if(sandcastleProject.CopyrightHref.Length == 0 && sandcastleProject.CopyrightText.Length == 0)
                return String.Empty;

            if(sandcastleProject.CopyrightHref.Length == 0)
                return sandcastleProject.DecodedCopyrightText;

            if(sandcastleProject.CopyrightText.Length == 0)
                return sandcastleProject.CopyrightHref;

            return String.Format(CultureInfo.CurrentCulture, "{0} ({1})", sandcastleProject.DecodedCopyrightText,
                sandcastleProject.CopyrightHref);
        }

        /// <summary>
        /// The HTML encoded copyright info
        /// </summary>
        /// <returns>The HTML encoded copyright info based on whether the HREF and/or text is specified</returns>
        [SubstitutionTag]
        private string HtmlEncCopyrightInfo()
        {
            if(sandcastleProject.CopyrightHref.Length == 0 && sandcastleProject.CopyrightText.Length == 0)
                return String.Empty;

            if(sandcastleProject.CopyrightHref.Length == 0)
                return "<p>" + WebUtility.HtmlEncode(sandcastleProject.DecodedCopyrightText) + "</p>";

            if(sandcastleProject.CopyrightText.Length == 0)
            {
                return String.Format(CultureInfo.CurrentCulture,
                    "<p><a href=\"{0}\" target=\"_blank\" rel=\"noopener noreferrer\">{0}</a></p>",
                    WebUtility.HtmlEncode(sandcastleProject.CopyrightHref));
            }

            return String.Format(CultureInfo.CurrentCulture,
                "<p><a href=\"{0}\" target=\"_blank\" rel=\"noopener noreferrer\">{1}</a></p>",
                WebUtility.HtmlEncode(sandcastleProject.CopyrightHref),
                WebUtility.HtmlEncode(sandcastleProject.DecodedCopyrightText));
        }

        /// <summary>
        /// The HTML encoded copyright link
        /// </summary>
        /// <returns>The HTML encoded copyright link if specified</returns>
        [SubstitutionTag]
        private string HtmlEncCopyrightHref()
        {
            return (sandcastleProject.CopyrightHref.Length == 0) ? String.Empty :
                String.Format(CultureInfo.CurrentCulture,
                    "<a href=\"{0}\" target=\"_blank\" rel=\"noopener noreferrer\">{0}</a>",
                    WebUtility.HtmlEncode(sandcastleProject.CopyrightHref));
        }

        /// <summary>
        /// The copyright text
        /// </summary>
        /// <returns>The copyright text if specified</returns>
        [SubstitutionTag]
        private string CopyrightText()
        {
            return (sandcastleProject.CopyrightText.Length == 0) ? String.Empty : sandcastleProject.DecodedCopyrightText;
        }

        /// <summary>
        /// The HTML encoded copyright text
        /// </summary>
        /// <returns>The HTML encoded copyright text if specified</returns>
        [SubstitutionTag]
        private string HtmlEncCopyrightText()
        {
            return (sandcastleProject.CopyrightText.Length == 0) ? String.Empty :
                WebUtility.HtmlEncode(sandcastleProject.DecodedCopyrightText);
        }

        /// <summary>
        /// The "Send comments" include item
        /// </summary>
        /// <returns>Include the "send comments" line if feedback e-mail address is specified</returns>
        [SubstitutionTag]
        private string Comments()
        {
            return (sandcastleProject.FeedbackEMailAddress.Length != 0) ? "<include item=\"comments\"/>" : String.Empty;
        }

        /// <summary>
        /// The URL encoded feedback e-mail address
        /// </summary>
        /// <returns>The URL encoded feedback e-mail address</returns>
        [SubstitutionTag]
        private string UrlEncFeedbackEMailAddress()
        {
            // Any embedded substitution tags need to be replaced first so that their content is encoded too
            string feedbackEMailAddress = TransformText(sandcastleProject.FeedbackEMailAddress);

            return (feedbackEMailAddress.Length == 0) ? String.Empty :
                WebUtility.UrlEncode(sandcastleProject.FeedbackEMailAddress);
        }

        /// <summary>
        /// The HTML encoded feedback e-mail address
        /// </summary>
        /// <returns>The HTML encoded feedback e-mail address.  If link text is specified, it will be used instead.</returns>
        [SubstitutionTag]
        private string HtmlEncFeedbackEMailAddress()
        {
            if(sandcastleProject.FeedbackEMailAddress.Length == 0)
                return String.Empty;
            
            if(sandcastleProject.FeedbackEMailLinkText.Length == 0)
                return WebUtility.HtmlEncode(sandcastleProject.FeedbackEMailAddress);
            
            return WebUtility.HtmlEncode(sandcastleProject.FeedbackEMailLinkText);
        }

        /// <summary>
        /// The "preliminary" warning in the header text
        /// </summary>
        /// <returns>Include the "preliminary" warning in the header text if wanted</returns>
        [SubstitutionTag]
        private string Preliminary()
        {
            return sandcastleProject.Preliminary ? "<include item=\"preliminary\"/>" : String.Empty;
        }

        /// <summary>
        /// The SDK link target
        /// </summary>
        /// <returns>The SDK link target converted to lowercase and prefixed with an underscore</returns>
        [SubstitutionTag]
        private string SdkLinkTarget()
        {
            return "_" + sandcastleProject.SdkLinkTarget.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// The syntax filter generator settings
        /// </summary>
        /// <returns>The syntax filter generator settings</returns>
        [SubstitutionTag]
        private string SyntaxFilters()
        {
            return ComponentUtilities.SyntaxFilterGeneratorsFrom(currentBuild.SyntaxGenerators,
                sandcastleProject.SyntaxFilters);
        }
        #endregion

        #region Help 1 format substitution tags
        //=====================================================================

        /// <summary>
        /// The binary TOC option
        /// </summary>
        /// <returns>The binary TOC option as a "Yes" or "No" value</returns>
        [SubstitutionTag]
        private string BinaryTOC()
        {
            return sandcastleProject.BinaryTOC ? "Yes" : "No";
        }

        /// <summary>
        /// The help window options
        /// </summary>
        /// <returns>Currently, we use a default set of options and only allow showing or hiding the Favorites tab</returns>
        [SubstitutionTag]
        private string WindowOptions()
        {
            return sandcastleProject.IncludeFavorites ? "0x63520" : "0x62520";
        }
        #endregion

        #region MS Help Viewer substitution tags
        //=====================================================================

        /// <summary>
        /// The help viewer setup name
        /// </summary>
        /// <returns>Help viewer setup names cannot contain periods so we'll replace them with underscores.  The
        /// value is also HTML encoded.</returns>
        [SubstitutionTag]
        private string HelpViewerSetupName()
        {
            return WebUtility.HtmlEncode(sandcastleProject.HtmlHelpName.Replace('.', '_'));
        }

        /// <summary>
        /// The catalog name
        /// </summary>
        /// <returns>The catalog name</returns>
        [SubstitutionTag]
        private string CatalogName()
        {
            return String.IsNullOrWhiteSpace(sandcastleProject.CatalogName) ? String.Empty :
                "/catalogName \"" + sandcastleProject.CatalogName + "\"";
        }

        /// <summary>
        /// The vendor name
        /// </summary>
        /// <returns>The vendor name</returns>
        [SubstitutionTag]
        private string VendorName()
        {
            return !String.IsNullOrWhiteSpace(sandcastleProject.VendorName) ? sandcastleProject.VendorName : "Vendor Name";
        }

        /// <summary>
        /// The HTML encoded vendor name
        /// </summary>
        /// <returns>The HTML encoded vendor name</returns>
        [SubstitutionTag]
        private string HtmlEncVendorName()
        {
            return !String.IsNullOrWhiteSpace(sandcastleProject.VendorName) ?
                WebUtility.HtmlEncode(sandcastleProject.VendorName) : "Vendor Name";
        }

        /// <summary>
        /// The product title
        /// </summary>
        /// <returns>The product title</returns>
        [SubstitutionTag]
        private string ProductTitle()
        {
            return !String.IsNullOrWhiteSpace(sandcastleProject.ProductTitle) ?
                sandcastleProject.ProductTitle : sandcastleProject.HelpTitle;
        }

        /// <summary>
        /// The HTML encoded product title
        /// </summary>
        /// <returns>The HTML encoded product title</returns>
        [SubstitutionTag]
        private string HtmlEncProductTitle()
        {
            return !String.IsNullOrWhiteSpace(sandcastleProject.ProductTitle) ?
                WebUtility.HtmlEncode(sandcastleProject.ProductTitle) : WebUtility.HtmlEncode(sandcastleProject.HelpTitle);
        }

        /// <summary>
        /// The topic version
        /// </summary>
        /// <returns>The topic version</returns>
        [SubstitutionTag]
        private string TopicVersion()
        {
            return WebUtility.HtmlEncode(sandcastleProject.TopicVersion);
        }

        /// <summary>
        /// The table of contents parent ID
        /// </summary>
        /// <returns>The table of contents parent ID</returns>
        [SubstitutionTag]
        private string TocParentId()
        {
            return WebUtility.HtmlEncode(sandcastleProject.TocParentId);
        }

        /// <summary>
        /// The API table of contents parent ID
        /// </summary>
        /// <returns>The API table of contents parent ID.  If null, empty, or it starts with '*', it is parented
        /// to the root node</returns>
        [SubstitutionTag]
        private string ApiTocParentId()
        {
            if(!String.IsNullOrWhiteSpace(currentBuild.ApiTocParentId) && currentBuild.ApiTocParentId[0] != '*')
            {
                // Ensure that the ID is valid and visible in the TOC
                if(!currentBuild.ConceptualContent.Topics.Any(t => t[currentBuild.ApiTocParentId] != null &&
                  t[currentBuild.ApiTocParentId].Visible))
                {
                    // If there are no conceptual content topics, it's probably parented to a topic in an old
                    // site map file so let it pass through.
                    if(currentBuild.ConceptualContent.Topics.Count != 0)
                    {
                        throw new BuilderException("BE0022", String.Format(CultureInfo.CurrentCulture,
                            "The project's ApiTocParent property value '{0}' must be associated with a topic in " +
                            "your project's conceptual content and must have its Visible property set to True in " +
                            "the content layout file.", currentBuild.ApiTocParentId));
                    }
                }

                return WebUtility.HtmlEncode(currentBuild.ApiTocParentId);
            }

            if(!String.IsNullOrWhiteSpace(currentBuild.RootContentContainerId))
                return WebUtility.HtmlEncode(currentBuild.RootContentContainerId);
            
            return WebUtility.HtmlEncode(sandcastleProject.TocParentId);
        }

        /// <summary>
        /// The table of contents parent version
        /// </summary>
        /// <returns>The table of contents parent version</returns>
        [SubstitutionTag]
        private string TocParentVersion()
        {
            return WebUtility.HtmlEncode(sandcastleProject.TocParentVersion);
        }

        /// <summary>
        /// The search results display version
        /// </summary>
        /// <returns>The display version to show in the help viewer search results pane</returns>
        [SubstitutionTag]
        private string SearchResultsDisplayVersion()
        {
            string displayVersion = this.TransformText(sandcastleProject.SearchResultsDisplayVersion);

            if(String.IsNullOrWhiteSpace(displayVersion))
                return String.Empty;

            return $"<meta name=\"Microsoft.Help.DisplayVersion\" content=\"{displayVersion}\" />";
        }
        #endregion

        #region Website substitution tags
        //=====================================================================

        /// <summary>
        /// The website ad content that should appear in on each page
        /// </summary>
        /// <returns>The website ad content</returns>
        [SubstitutionTag]
        private string WebsiteAdContent()
        {
            // Escape braces so that they aren't interpreted as shared content item parameters.  However, we must
            // replace any other substitution tags before doing that.
            return this.TransformText(sandcastleProject.WebsiteAdContent).Replace("{", "{{").Replace("}", "}}");
        }
        #endregion

        #region Markdown substitution tags
        //=====================================================================

        /// <summary>
        /// The markdown API URL format
        /// </summary>
        /// <returns>The markdown API URL format</returns>
        [SubstitutionTag]
        private string MarkdownApiUrlFormat()
        {
            if(sandcastleProject.AppendMarkdownFileExtensionsToUrls)
                return "{0}.md";

            return "{0}";
        }

        /// <summary>
        /// The markdown conceptual URL selector
        /// </summary>
        /// <returns>The markdown API URL selector</returns>
        [SubstitutionTag]
        private string MarkdownConceptualUrlSelector()
        {
            if(sandcastleProject.AppendMarkdownFileExtensionsToUrls)
                return "concat(/metadata/topic/@id,'.md')";

            return "string(/metadata/topic/@id)";
        }
        #endregion

        #region Visibility category settings
        //=====================================================================

        /// <summary>
        /// The API filter settings
        /// </summary>
        /// <returns>The API filter settings.  In a partial build used to get API info for the API filter
        /// designer, the filter is suppressed and will not be used.</returns>
        [SubstitutionTag]
        private string ApiFilter()
        {
            return (!currentBuild.SuppressApiFilter && sandcastleProject.ApiFilter.Count != 0) ?
                sandcastleProject.ApiFilter.ToString() : String.Empty;
        }
        #endregion

        #region File list substitution tags
        //=====================================================================

        /// <summary>
        /// Framework comments file list (data elements)
        /// </summary>
        /// <returns>The framework comments file list in <c>data</c> elements</returns>
        [SubstitutionTag]
        private string FrameworkCommentList()
        {
            return this.FrameworkCommentList(false);;
        }

        /// <summary>
        /// Framework comments file list (import elements)
        /// </summary>
        /// <returns>The framework comments file list in <c>import</c> elements</returns>
        [SubstitutionTag]
        private string ImportFrameworkCommentList()
        {
            return this.FrameworkCommentList(true);
        }

        /// <summary>
        /// XML comments file list
        /// </summary>
        /// <returns>The XML comments file list</returns>
        [SubstitutionTag]
        private string CommentFileList()
        {
            return currentBuild.CommentsFiles.CommentFileList(currentBuild.WorkingFolder, false);
        }

        /// <summary>
        /// XML comments file list for inherited documentation generation
        /// </summary>
        /// <returns>The XML comments file list for inherited documentation generation</returns>
        [SubstitutionTag]
        private string InheritedCommentFileList()
        {
            return currentBuild.CommentsFiles.CommentFileList(currentBuild.WorkingFolder, true);
        }

        /// <summary>
        /// Help 1 project file list
        /// </summary>
        /// <returns>The help 1 project file list</returns>
        /// <remarks>The help file list is expanded to ensure that we get all additional content including all
        /// nested subfolders.</remarks>
        [SubstitutionTag]
        private string Help1xProjectFiles()
        {
            string sourceFolder = Path.Combine(currentBuild.WorkingFolder, "Output",
                HelpFileFormats.HtmlHelp1.ToString()) + Path.DirectorySeparatorChar,
                filename, checkName;

            if(sourceFolder.IndexOf(',') != -1 || sourceFolder.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
            {
                currentBuild.ReportWarning("BE0060", "The file path '{0}' contains a comma or '.h' which may " +
                    "cause the Help 1 compiler to fail.", sourceFolder);
            }

            if(currentBuild.ResolvedHtmlHelpName.IndexOf(',') != -1 ||
                currentBuild.ResolvedHtmlHelpName.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
            {
                currentBuild.ReportWarning("BE0060", "The HtmlHelpName property value '{0}' contains a comma " +
                    "or '.h' which may cause the Help 1 compiler to fail.", currentBuild.ResolvedHtmlHelpName);
            }

            replacementValue.Clear();

            foreach(string name in Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
            {
                filename = checkName = name.Replace(sourceFolder, String.Empty);

                // Ignore the project, index, and TOC files
                if(checkName.EndsWith(".hhc", StringComparison.OrdinalIgnoreCase) ||
                    checkName.EndsWith(".hhk", StringComparison.OrdinalIgnoreCase) ||
                    checkName.EndsWith(".hhp", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if(checkName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                  checkName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                {
                    checkName = checkName.Substring(0, checkName.LastIndexOf(".htm", StringComparison.OrdinalIgnoreCase));
                }

                if(checkName.IndexOf(',') != -1 || checkName.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    currentBuild.ReportWarning("BE0060", "The filename '{0}' contains a comma or '.h' " +
                        "which may cause the Help 1 compiler to fail.", filename);
                }

                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "{0}\r\n", filename);
            }

            return replacementValue.ToString();
        }

        /// <summary>
        /// Code snippets file list
        /// </summary>
        /// <returns>The code snippets file list</returns>
        [SubstitutionTag]
        private string CodeSnippetsFiles()
        {
            replacementValue.Clear();

            foreach(var file in currentBuild.ConceptualContent.CodeSnippetFiles)
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<examples file=\"{0}\" />\r\n", file.FullPath);

            return replacementValue.ToString();
        }

        /// <summary>
        /// Reference link namespace file list
        /// </summary>
        /// <returns>The reference link namespace file list</returns>
        [SubstitutionTag]
        private string ReferenceLinkNamespaceFiles()
        {
            replacementValue.Clear();

            foreach(string ns in currentBuild.ReferencedNamespaces)
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<namespace file=\"{0}.xml\" />\r\n", ns);

            return replacementValue.ToString();
        }

        /// <summary>
        /// Resource items file list
        /// </summary>
        /// <returns>The resource items file list</returns>
        [SubstitutionTag]
        private string ResourceItemFiles()
        {
            replacementValue.Clear();

            // Add resource items files from the presentation style.  These are always listed first so as to
            // allow the files below to override the stock items.  Files are copied and transformed as they may
            // contain substitution tags.
            foreach(string psItemFile in currentBuild.PresentationStyle.ResourceItemFiles(currentBuild.Language.Name))
            {
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<content file=\"{0}\" />\r\n",
                    Path.GetFileName(psItemFile));

                this.TransformTemplate(Path.GetFileName(psItemFile), Path.GetDirectoryName(psItemFile),
                    currentBuild.WorkingFolder);
            }

            // If generating web content, add the website content override file.  We just transform it and add
            // it to the working folder.  The website specific part of the build assembler configuration file
            // will pick it up.
            if((currentBuild.CurrentProject.HelpFileFormat & HelpFileFormats.Website) == HelpFileFormats.Website)
            {
                this.TransformTemplate("WebsiteContent.xml", Path.Combine(ComponentUtilities.CoreComponentsFolder,
                    "Shared", "Content"), currentBuild.WorkingFolder);
            }

            // Add syntax generator resource item files.  All languages are included regardless of the project
            // filter settings since code examples can be in any language.  Files are copied and transformed as
            // they may contain substitution tags.
            foreach(string itemFile in ComponentUtilities.SyntaxGeneratorResourceItemFiles(
              currentBuild.ComponentContainer, sandcastleProject.Language))
            {
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<content file=\"{0}\" />\r\n",
                    Path.GetFileName(itemFile));

                this.TransformTemplate(Path.GetFileName(itemFile), Path.GetDirectoryName(itemFile),
                    currentBuild.WorkingFolder);
            }

            // Add project resource item files last so that they override all other files
            foreach(var file in sandcastleProject.ContentFiles(BuildAction.ResourceItems).OrderBy(f => f.LinkPath))
            {
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<content file=\"{0}\" />\r\n",
                    Path.GetFileName(file.FullPath));

                this.TransformTemplate(Path.GetFileName(file.FullPath),
                    Path.GetDirectoryName(file.FullPath), currentBuild.WorkingFolder);
            }

            return replacementValue.ToString();
        }

        /// <summary>
        /// Token file list
        /// </summary>
        /// <returns>The token file list</returns>
        [SubstitutionTag]
        private string TokenFiles()
        {
            replacementValue.Clear();

            foreach(var file in currentBuild.ConceptualContent.TokenFiles)
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<content file=\"{0}\" />\r\n",
                    Path.GetFileName(file.FullPath));

            return replacementValue.ToString();
        }

        /// <summary>
        /// XAML configuration file list
        /// </summary>
        /// <returns>The XAML configuration file list</returns>
        [SubstitutionTag]
        private string XamlConfigFiles()
        {
            replacementValue.Clear();

            foreach(var file in sandcastleProject.ContentFiles(BuildAction.XamlConfiguration))
                replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<filter files=\"{0}\" />\r\n",
                    file.FullPath);

            return replacementValue.ToString();
        }
        #endregion

        #region Miscellaneous substitution tags
        //=====================================================================

        /// <summary>
        /// The build date
        /// </summary>
        /// <returns>The build date.  An optional format can be applied to the result.</returns>
        [SubstitutionTag]
        private string BuildDate()
        {
            return !String.IsNullOrWhiteSpace(fieldFormat) ? String.Format(sandcastleProject.Language,
                "{0:" + fieldFormat + "}", DateTime.Now) :  DateTime.Now.ToString(sandcastleProject.Language);
        }

        /// <summary>
        /// The build date in Universal Coordinated Time (UTC)
        /// </summary>
        /// <returns>The build date in Universal Coordinated Time (UTC).  An optional format can be applied to
        /// the result.</returns>
        [SubstitutionTag]
        private string BuildDateUtc()
        {
            return !String.IsNullOrWhiteSpace(fieldFormat) ? String.Format(sandcastleProject.Language,
                "{0:" + fieldFormat + "}", DateTime.UtcNow) : DateTime.UtcNow.ToString(sandcastleProject.Language);
        }

        /// <summary>
        /// The default topic ID
        /// </summary>
        /// <returns>The default topic ID</returns>
        [SubstitutionTag]
        private string DefaultTopic()
        {
            return currentBuild.DefaultTopicFile;
        }

        /// <summary>
        /// The default topic ID for websites
        /// </summary>
        /// <returns>The default topic ID with backslashes converted to forward slashes</returns>
        [SubstitutionTag]
        private string WebDefaultTopic()
        {
            return currentBuild.DefaultTopicFile.Replace('\\', '/');
        }

        /// <summary>
        /// Unique ID
        /// </summary>
        /// <returns>A unique ID for the project and current user</returns>
        [SubstitutionTag]
        private string UniqueId()
        {
            string userName = Environment.GetEnvironmentVariable("USERNAME");

            if(String.IsNullOrWhiteSpace(userName))
                userName = "DefaultUser";

            return (sandcastleProject.Filename + "_" + userName).GetHashCodeDeterministic().ToString("X", CultureInfo.InvariantCulture);
        }
        #endregion

#pragma warning restore IDE0051
#pragma warning restore CA1822

        #region Helper methods
        //=====================================================================

        /// <summary>
        /// This is used to generate an appropriate list of entries that represent .NET Framework comments file
        /// locations for the various configuration files.
        /// </summary>
        /// <param name="forImport">True if for import, false if not.  If true, it returns a list of the files
        /// as <c>import</c> elements.  If false, it returns them in <c>data</c> elements.</param>
        /// <returns>The list of framework comments file sources in the appropriate format</returns>
        private string FrameworkCommentList(bool forImport)
        {
            var dataSet = currentBuild.FrameworkReflectionData;
            string folder, wildcard;

            replacementValue.Clear();

            // Build the list based on the type and what actually exists
            foreach(var location in dataSet.CommentsFileLocations(sandcastleProject.Language))
            {
                folder = Path.GetDirectoryName(location);
                wildcard = Path.GetFileName(location);

                if(!forImport)
                {
                    // Files are cached by platform, version, and location.  The groupId attribute can be used by
                    // caching components to identify the cache and its location.
                    replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<data base=\"{0}\" files=\"{1}\" " +
                        "recurse=\"false\" duplicateWarning=\"false\" groupId=\"{2}_{3}_{4:X}\" />\r\n",
                        folder, wildcard, dataSet.Platform, dataSet.Version, location.GetHashCodeDeterministic());
                }
                else
                    replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<import path=\"{0}\" file=\"{1}\" " +
                        "recurse=\"false\" />\r\n", folder, wildcard);
            }

            return replacementValue.ToString();
        }
        #endregion
    }
}
