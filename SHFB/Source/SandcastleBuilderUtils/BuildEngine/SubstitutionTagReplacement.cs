//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : SubstitutionTagReplacement.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 08/28/2016
// Note    : Copyright 2015-2016, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.XPath;

using Microsoft.Build.Evaluation;

using Sandcastle.Core;
using Sandcastle.Core.PresentationStyle;

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

        private BuildProcess currentBuild;
        private SandcastleProject sandcastleProject;
        private Project msbuildProject;
        private PresentationStyleSettings presentationStyle;

        private Dictionary<string, MethodInfo> methodCache;

        private static Regex reField = new Regex(@"{@(?<Field>\w*?)(:(?<Format>.*?))?}");

        private MatchEvaluator fieldMatchEval;
        private string fieldFormat;

        private StringBuilder replacementValue;

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="currentBuild">The current build for which to perform substitution tag replacement</param>
        public SubstitutionTagReplacement(BuildProcess currentBuild)
        {
            this.currentBuild = currentBuild;

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
                return (templateText ?? String.Empty);

            if(args.Length != 0)
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
                throw new ArgumentNullException("templateFile");

            if(sourceFolder == null)
                throw new ArgumentNullException("sourceFolder");

            if(destFolder == null)
                throw new ArgumentNullException("destFolder");

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
            MethodInfo method;
            string fieldName = match.Groups["Field"].Value, propertyValue;

            // See if a method exists first.  If so, we'll call it and return its value.
            if(methodCache.TryGetValue(fieldName, out method))
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
        /// The help file builder folder
        /// </summary>
        /// <returns>The help file builder folder</returns>
        [SubstitutionTag]
        private string SHFBFolder()
        {
            return ComponentUtilities.ToolsFolder;
        }

        /// <summary>
        /// The components folder
        /// </summary>
        /// <returns>The components folder</returns>
        [SubstitutionTag]
        private string ComponentsFolder()
        {
            return ComponentUtilities.ComponentsFolder;
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
        /// <returns>The current build's working folder</returns>
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
        /// The resource items folder
        /// </summary>
        /// <returns>The resource items folder</returns>
        [SubstitutionTag]
        private string ResourceItemsFolder()
        {
            return FolderPath.TerminatePath(Path.Combine(presentationStyle.ResolvePath(presentationStyle.ResourceItemsPath),
                currentBuild.LanguageFolder));
        }

        /// <summary>
        /// The HTML Help 1 output folder
        /// </summary>
        /// <returns>The HTML Help 1 output folder</returns>
        [SubstitutionTag]
        private string Help1Folder()
        {
            return ((sandcastleProject.HelpFileFormat & HelpFileFormats.HtmlHelp1) == 0) ? String.Empty :
                @"Output\" + HelpFileFormats.HtmlHelp1.ToString();
        }

        /// <summary>
        /// The website output folder
        /// </summary>
        /// <returns>The website output folder</returns>
        [SubstitutionTag]
        private string WebsiteFolder()
        {
            return ((sandcastleProject.HelpFileFormat & HelpFileFormats.Website) == 0) ? String.Empty :
                @"Output\" + HelpFileFormats.Website.ToString();
        }

        /// <summary>
        /// The MEF component locations
        /// </summary>
        /// <returns>The MEF component locations</returns>
        [SubstitutionTag]
        private string ComponentLocations()
        {
            string locations;

            if(String.IsNullOrWhiteSpace(sandcastleProject.ComponentPath))
                locations = String.Empty;
            else
                locations = String.Format(CultureInfo.InvariantCulture, "<location folder=\"{0}\" />\r\n",
                    WebUtility.HtmlEncode(sandcastleProject.ComponentPath));

            locations += String.Format(CultureInfo.InvariantCulture, "<location folder=\"{0}\" />",
                WebUtility.HtmlEncode(Path.GetDirectoryName(sandcastleProject.Filename)));

            return locations;
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
                replacementValue.AppendFormat("<path value=\"{0}\" />", baseFolder.Substring(currentBuild.WorkingFolder.Length));

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
        /// The document model XSL transformation filename
        /// </summary>
        /// <returns>The document model XSL transformation filename</returns>
        [SubstitutionTag]
        private string DocModelTransformation()
        {
            return presentationStyle.ResolvePath(presentationStyle.DocumentModelTransformation.TransformationFilename);
        }

        /// <summary>
        /// The document model XSL transformation parameters
        /// </summary>
        /// <returns>The document model XSL transformation parameters</returns>
        [SubstitutionTag]
        private string DocModelTransformationParameters()
        {
            return String.Join(";", presentationStyle.DocumentModelTransformation.Select(
                p => String.Format(CultureInfo.InvariantCulture, "{0}={1}", p.Key, p.Value)));
        }

        /// <summary>
        /// The intermediate TOC XSL transformation filename
        /// </summary>
        /// <returns>The intermediate TOC XSL transformation filename</returns>
        [SubstitutionTag]
        private string TocTransformation()
        {
            return presentationStyle.ResolvePath(presentationStyle.IntermediateTocTransformation.TransformationFilename);
        }

        /// <summary>
        /// The intermediate TOC XSL transformation parameters
        /// </summary>
        /// <returns>The intermediate TOC XSL transformation parameters</returns>
        [SubstitutionTag]
        private string TocTransformParameters()
        {
            return String.Join(";", presentationStyle.IntermediateTocTransformation.Select(
                p => String.Format(CultureInfo.InvariantCulture, "{0}={1}", p.Key, p.Value)));
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
        /// The Transform Component argument list
        /// </summary>
        /// <returns>The Transform Component argument list</returns>
        [SubstitutionTag]
        private string TransformComponentArguments()
        {
            replacementValue.Clear();

            foreach(var arg in sandcastleProject.TransformComponentArguments)
                if(arg.Value != null)
                    replacementValue.AppendFormat("<argument key=\"{0}\" value=\"{1}\" />\r\n", arg.Key, arg.Value);
                else
                    replacementValue.AppendFormat("<argument key=\"{0}\">{1}</argument>\r\n", arg.Key, arg.Content);

            return replacementValue.ToString();
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

            if(v.Build > 0 && v.Build < 10)
                return currentBuild.FrameworkReflectionData.Version.ToString(3);

            return v.ToString(2);
        }

        /// <summary>
        /// The framework reflection data folder
        /// </summary>
        [SubstitutionTag]
        private string FrameworkReflectionDataFolder()
        {
            return currentBuild.FrameworkReflectionDataFolder;
        }
        #endregion

        #region Build category substitution tags
        //=====================================================================

        /// <summary>
        /// Build assembler verbosity
        /// </summary>
        /// <returns>The build assembler verbosity</returns>
        [SubstitutionTag]
        private string BuildAssemblerVerbosity()
        {
            return (sandcastleProject.BuildAssemblerVerbosity == Utils.BuildAssemblerVerbosity.AllMessages) ? "Info" :
                (sandcastleProject.BuildAssemblerVerbosity == Utils.BuildAssemblerVerbosity.OnlyWarningsAndErrors) ?
                "Warn" : "Error";
        }

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
        /// <returns>Only &amp;, &lt;, &gt;, and " are replaced for now</returns>
        [SubstitutionTag]
        private string UrlEncHelpTitle()
        {
            return sandcastleProject.HelpTitle.Replace("&", "%26").Replace("<", "%3C").Replace(
                ">", "%3E").Replace("\"", "%22");
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
        /// The namespace grouping setting if supported
        /// </summary>
        /// <returns>The namespace grouping option enabled state</returns>
        [SubstitutionTag]
        private string NamespaceGrouping()
        {
            if(sandcastleProject.NamespaceGrouping && presentationStyle.SupportsNamespaceGrouping)
                return "true";

            if(sandcastleProject.NamespaceGrouping)
                currentBuild.ReportWarning("BE0027", "Namespace grouping was requested but the selected " +
                    "presentation style does not support it.  Option ignored.");

            return "false";
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
                return String.Format(CultureInfo.CurrentCulture, "<p><a href='{0}' target='_blank'>{0}</a></p>",
                    WebUtility.HtmlEncode(sandcastleProject.CopyrightHref));

            return String.Format(CultureInfo.CurrentCulture, "<p><a href='{0}' target='_blank'>{1}</a></p>",
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
                String.Format(CultureInfo.CurrentCulture, "<a href='{0}' target='_blank'>{0}</a>",
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
            return (sandcastleProject.FeedbackEMailAddress.Length == 0) ? String.Empty :
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
            return sandcastleProject.Preliminary ? "<include item=\"preliminary\"/>" :String.Empty;
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

        /// <summary>
        /// The syntax filter language settings for the Transform Component
        /// </summary>
        /// <returns>The syntax filter language settings for the Transform Component.  It is not technically a
        /// dropdown anymore but I can't be bothered to go change it everywhere.</returns>
        [SubstitutionTag]
        private string SyntaxFiltersDropDown()
        {
            return ComponentUtilities.SyntaxFilterLanguagesFrom(currentBuild.SyntaxGenerators,
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
                    throw new BuilderException("BE0022", String.Format(CultureInfo.CurrentCulture,
                        "The project's ApiTocParent property value '{0}' must be associated with a topic in " +
                        "your project's conceptual content and must have its Visible property set to True in " +
                        "the content layout file.", currentBuild.ApiTocParentId));

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
        #endregion

        #region Website substitution tags
        //=====================================================================

        /// <summary>
        /// The HTML table of contents for websites
        /// </summary>
        /// <returns>The HTML table of contents for websites</returns>
        /// <remarks>If the legacy web content is ever removed, this handler and its related method,
        /// <see cref="AppendTocEntry"/>, can be removed</remarks>
        [SubstitutionTag]
        private string HtmlTOC()
        {
            XPathDocument tocDoc;
            Encoding enc = Encoding.Default;

            // When reading the file, use the default encoding but detect the encoding if byte order marks are
            // present.
            using(StringReader sr = new StringReader(Utility.ReadWithEncoding
              (currentBuild.WorkingFolder + "WebTOC.xml", ref enc)))
            {
                tocDoc = new XPathDocument(sr);
            }

            var navToc = tocDoc.CreateNavigator();

            // Get the TOC entries from the HelpTOC node
            var entries = navToc.Select("HelpTOC/*");

            replacementValue.Clear();

            this.AppendTocEntry(entries, replacementValue);

            return replacementValue.ToString();
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
        [SubstitutionTag]
        private string Help1xProjectFiles()
        {
            return this.HelpProjectFileList(String.Format(CultureInfo.InvariantCulture, @"{0}Output\{1}",
                currentBuild.WorkingFolder, HelpFileFormats.HtmlHelp1), HelpFileFormats.HtmlHelp1);
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
                replacementValue.AppendFormat("<examples file=\"{0}\" />\r\n", file.FullPath);

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
                replacementValue.AppendFormat("<namespace file=\"{0}.xml\" />\r\n", ns);

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

            // Add syntax generator resource item files.  All languages are included regardless of the project
            // filter settings since code examples can be in any language.  Files are copied and transformed as
            // they may contain substitution tags
            foreach(string itemFile in ComponentUtilities.SyntaxGeneratorResourceItemFiles(
              currentBuild.ComponentContainer, sandcastleProject.Language))
            {
                replacementValue.AppendFormat("<content file=\"{0}\" />\r\n", Path.GetFileName(itemFile));

                this.TransformTemplate(Path.GetFileName(itemFile), Path.GetDirectoryName(itemFile),
                    currentBuild.WorkingFolder);
            }

            // Add project resource item files last so that they override all other files
            foreach(var file in sandcastleProject.ContentFiles(BuildAction.ResourceItems).OrderBy(f => f.LinkPath))
            {
                replacementValue.AppendFormat("<content file=\"{0}\" />\r\n", Path.GetFileName(file.FullPath));

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
                replacementValue.AppendFormat("<content file=\"{0}\" />\r\n", Path.GetFileName(file.FullPath));

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
                replacementValue.AppendFormat("<filter files=\"{0}\" />\r\n", file.FullPath);

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
            return !String.IsNullOrWhiteSpace(fieldFormat) ? String.Format(CultureInfo.CurrentCulture,
                "{0:" + fieldFormat + "}", DateTime.Now) :  DateTime.Now.ToString(sandcastleProject.Language);
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
        /// The project node name
        /// </summary>
        /// <returns>The project node name</returns>
        [SubstitutionTag]
        private string ProjectNodeName()
        {
            return "R:Project_" + sandcastleProject.HtmlHelpName.Replace(" ", "_");
        }

        /// <summary>
        /// The optional project node ID
        /// </summary>
        /// <returns>The project node ID or an empty string if the Root Namespace Container is not enabled</returns>
        [SubstitutionTag]
        private string ProjectNodeIdOptional()
        {
            return !sandcastleProject.RootNamespaceContainer ? String.Empty :
                "Project_" + sandcastleProject.HtmlHelpName.Replace(" ", "_").Replace("&", "_");
        }

        /// <summary>
        /// The required project node ID
        /// </summary>
        /// <returns>The project node ID (always returns the ID)</returns>
        [SubstitutionTag]
        private string ProjectNodeIdRequired()
        {
            return "Project_" + sandcastleProject.HtmlHelpName.Replace(" ", "_").Replace("&", "_");
        }

        /// <summary>
        /// Add XAML syntax data transformation setting
        /// </summary>
        /// <returns>The "Add XAML syntax data" transformation setting.  If the XAML syntax generator is present,
        /// add XAML syntax data to the reflection file.</returns>
        [SubstitutionTag]
        private string AddXamlSyntaxData()
        {
            return ComponentUtilities.SyntaxFiltersFrom(currentBuild.SyntaxGenerators,
              sandcastleProject.SyntaxFilters).Any(s => s.Id == "XAML Usage") ?
                @";~\ProductionTransforms\AddXamlSyntaxData.xsl" : String.Empty;
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

            return (sandcastleProject.Filename + "_" + userName).GetHashCode().ToString("X", CultureInfo.InvariantCulture);
        }
        #endregion

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
                        folder, wildcard, dataSet.Platform, dataSet.Version, location.GetHashCode());
                }
                else
                    replacementValue.AppendFormat(CultureInfo.InvariantCulture, "<import path=\"{0}\" file=\"{1}\" " +
                        "recurse=\"false\" />\r\n", folder, wildcard);
            }

            return replacementValue.ToString();
        }

        /// <summary>
        /// This returns a complete list of files for inclusion in the compiled help file
        /// </summary>
        /// <param name="folder">The folder to expand</param>
        /// <param name="format">The help file format</param>
        /// <returns>The full list of all files for the help project</returns>
        /// <remarks>The help file list is expanded to ensure that we get all additional content including all
        /// nested subfolders.  The <paramref name="format"/> parameter determines the format of the returned
        /// file list.  For HTML Help 1, it returns a list of the filenames.  For all others, it returns the list
        /// formatted with the necessary XML markup.</remarks>
        private string HelpProjectFileList(string folder, HelpFileFormats format)
        {
            string itemFormat, filename, checkName, sourceFolder = folder;
            bool encode;

            if(folder == null)
                throw new ArgumentNullException("folder");

            if(folder.Length != 0 && folder[folder.Length - 1] != '\\')
                folder += @"\";

            if((format & HelpFileFormats.HtmlHelp1) != 0)
            {
                if(folder.IndexOf(',') != -1 || folder.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
                    currentBuild.ReportWarning("BE0060", "The file path '{0}' contains a comma or '.h' which may " +
                        "cause the Help 1 compiler to fail.", folder);

                if(currentBuild.ResolvedHtmlHelpName.IndexOf(',') != -1 ||
                  currentBuild.ResolvedHtmlHelpName.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
                    currentBuild.ReportWarning("BE0060", "The HtmlHelpName property value '{0}' contains a comma " +
                        "or '.h' which may cause the Help 1 compiler to fail.", currentBuild.ResolvedHtmlHelpName);

                itemFormat = "{0}\r\n";
                encode = false;
            }
            else
            {
                itemFormat = "	<File Url=\"{0}\" />\r\n";
                encode = true;
            }

            replacementValue.Clear();

            foreach(string name in Directory.EnumerateFiles(sourceFolder, "*.*", SearchOption.AllDirectories))
                if(!encode)
                {
                    filename = checkName = name.Replace(folder, String.Empty);

                    if(checkName.EndsWith(".htm", StringComparison.OrdinalIgnoreCase) ||
                      checkName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                        checkName = checkName.Substring(0, checkName.LastIndexOf(".htm", StringComparison.OrdinalIgnoreCase));

                    if(checkName.IndexOf(',') != -1 || checkName.IndexOf(".h", StringComparison.OrdinalIgnoreCase) != -1)
                        currentBuild.ReportWarning("BE0060", "The filename '{0}' contains a comma or '.h' " +
                            "which may cause the Help 1 compiler to fail.", filename);

                    replacementValue.AppendFormat(itemFormat, filename);
                }
                else
                    replacementValue.AppendFormat(itemFormat, WebUtility.HtmlEncode(name.Replace(folder, String.Empty)));

            return replacementValue.ToString();
        }

        /// <summary>
        /// This is called to recursively append the child nodes to the HTML table of contents in the specified
        /// string builder.
        /// </summary>
        /// <param name="entries">The list over which to iterate recursively</param>
        /// <param name="tableOfContents">The string builder to which the entries are appended</param>
        private void AppendTocEntry(XPathNodeIterator entries, StringBuilder tableOfContents)
        {
            string url, target, title;

            foreach(XPathNavigator node in entries)
                if(node.HasChildren)
                {
                    url = node.GetAttribute("Url", String.Empty);
                    title = node.GetAttribute("Title", String.Empty);

                    if(!String.IsNullOrWhiteSpace(url))
                        target = " target=\"TopicContent\"";
                    else
                    {
                        url = "#";
                        target = String.Empty;
                    }

                    tableOfContents.AppendFormat("<div class=\"TreeNode\">\r\n" +
                        "<img class=\"TreeNodeImg\" onclick=\"javascript: Toggle(this);\" " +
                        "src=\"Collapsed.gif\"/><a class=\"UnselectedNode\" " +
                        "onclick=\"javascript: return Expand(this);\" href=\"{0}\"{1}>{2}</a>\r\n" +
                        "<div class=\"Hidden\">\r\n", WebUtility.HtmlEncode(url), target, WebUtility.HtmlEncode(title));

                    this.AppendTocEntry(node.Select("*"), tableOfContents);

                    tableOfContents.Append("</div>\r\n</div>\r\n");
                }
                else
                {
                    title = node.GetAttribute("Title", String.Empty);
                    url = node.GetAttribute("Url", String.Empty);

                    if(String.IsNullOrWhiteSpace(url))
                        url = "about:blank";

                    tableOfContents.AppendFormat("<div class=\"TreeItem\">\r\n" +
                        "<img src=\"Item.gif\"/>" +
                        "<a class=\"UnselectedNode\" onclick=\"javascript: return SelectNode(this);\" " +
                        "href=\"{0}\" target=\"TopicContent\">{1}</a>\r\n" +
                        "</div>\r\n", WebUtility.HtmlEncode(url), WebUtility.HtmlEncode(title));
                }
        }
        #endregion
    }
}
