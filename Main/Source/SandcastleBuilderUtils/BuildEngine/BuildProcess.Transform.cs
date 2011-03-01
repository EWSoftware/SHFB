//=============================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : BuildProcess.Transform.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 02/24/2011
// Note    : Copyright 2006-2011, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains the code used to transform and generate the files used
// to define and compile the help file.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy
// of the license should be distributed with the code.  It can also be found
// at the project website: http://SHFB.CodePlex.com.   This notice, the
// author's name, and all copyright notices must remain intact in all
// applications, documentation, and source files.
//
// Version     Date     Who  Comments
// ============================================================================
#region Older history
// 1.0.0.0  08/07/2006  EFW  Created the code
// 1.3.3.0  11/24/2006  EFW  Added support for component configurations
// 1.5.0.2  07/18/2007  EFW  Added support for the MRefBuilder API filter
// 1.5.1.0  07/25/2007  EFW  Allowed for a format specifier in replacement tags
//                           and for nested tags within project values.
// 1.5.2.0  09/10/2007  EFW  Exposed some members for use by plug-ins and
//                           added support for calling the plug-ins.
// 1.6.0.2  11/01/2007  EFW  Reworked to support better handling of components
// 1.6.0.4  01/18/2008  EFW  Added support for the JavaScript syntax generator
// 1.6.0.5  02/04/2008  EFW  Added support for SandcastleHtmlExtract tool and
//                           the ability to transform general text strings.
//                           Added support for FeedbackEMailLinkText property.
//                           Added support for the <inheritdoc /> tag.
// 1.6.0.7  03/21/2008  EFW  Various updates to support new project properties.
//                           Added support for conceptual build configurations
// 1.8.0.0  07/14/2008  EFW  Various updates to support new project structure
// 1.8.0.1  01/16/2009  EFW  Added support for ShowMissingIncludeTargets
#endregion
// 1.8.0.3  07/05/2009  EFW  Added support for the F# syntax filter
// 1.8.0.3  11/10/2009  EFW  Updated to support custom language syntax filters
// 1.8.0.3  12/06/2009  EFW  Removed support for ShowFeedbackControl.  Added
//                           support for resource item files.
// 1.9.0.0  06/06/2010  EFW  Added support for multi-format build output
// 1.9.1.0  07/09/2010  EFW  Updated for use with .NET 4.0 and MSBuild 4.0.
// 1.9.2.0  01/16/2011  EFW  Updated to support selection of Silverlight
//                           Framework versions.
//=============================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

using Microsoft.Build.Evaluation;

using SandcastleBuilder.Utils.BuildComponent;
using SandcastleBuilder.Utils.Design;
using SandcastleBuilder.Utils.PlugIn;

namespace SandcastleBuilder.Utils.BuildEngine
{
    partial class BuildProcess
    {
        #region Private data members
        //=====================================================================

        // A stack used to check for circular build component dependencies
        private Stack<string> mergeStack;

        // Regular expressions used for encoding detection and parsing
        private static Regex reXmlEncoding = new Regex(
            "^<\\?xml.*?encoding\\s*=\\s*\"(?<Encoding>.*?)\".*?\\?>");

        private static Regex reField = new Regex(
            @"{@(?<Field>\w*?)(:(?<Format>.*?))?}");

        private MatchEvaluator fieldMatchEval;
        #endregion

        /// <summary>
        /// Transform the specified template text by inserting the necessary
        /// values into the place holders tags.
        /// </summary>
        /// <param name="templateText">The template text to transform</param>
        /// <param name="args">An optional list of arguments to format into the 
        /// template before transforming it.</param>
        /// <returns>The transformed text</returns>
        public string TransformText(string templateText, params object[] args)
        {
            if(String.IsNullOrEmpty(templateText))
                return String.Empty;

            if(args.Length != 0)
                templateText = String.Format(CultureInfo.InvariantCulture,
                    templateText, args);

            try
            {
                // Use a regular expression to find and replace all field
                // tags with a matching value from the project.  They can
                // be nested.
                while(reField.IsMatch(templateText))
                    templateText = reField.Replace(templateText, fieldMatchEval);
            }
            catch(BuilderException )
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
        /// Transform the specified template by inserting the necessary
        /// values into the place holders and saving it to the working folder.
        /// </summary>
        /// <param name="template">The template to transform</param>
        /// <param name="sourceFolder">The folder where the template is
        /// located</param>
        /// <param name="destFolder">The folder in which to save the
        /// transformed file</param>
        /// <returns>The path to the transformed file</returns>
        public string TransformTemplate(string template, string sourceFolder, string destFolder)
        {
            Encoding enc = Encoding.Default;
            string templateText, transformedFile;

            if(template == null)
                throw new ArgumentNullException("template");

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
                // When reading the file, use the default encoding but
                // detect the encoding if byte order marks are present.
                templateText = BuildProcess.ReadWithEncoding(sourceFolder + template, ref enc);

                // Use a regular expression to find and replace all field
                // tags with a matching value from the project.  They can
                // be nested.
                while(reField.IsMatch(templateText))
                    templateText = reField.Replace(templateText, fieldMatchEval);

                transformedFile = destFolder + template;

                // Write the file back out using its original encoding
                using(StreamWriter sw = new StreamWriter(transformedFile, false, enc))
                {
                    sw.Write(templateText);
                }
            }
            catch(BuilderException )
            {
                throw;
            }
            catch(Exception ex)
            {
                throw new BuilderException("BE0019", String.Format(CultureInfo.CurrentCulture,
                    "Unable to transform template '{0}': {1}", template, ex.Message), ex);
            }

            return transformedFile;
        }

        /// <summary>
        /// This is used to read in a file using an appropriate encoding method
        /// </summary>
        /// <param name="filename">The file to load</param>
        /// <param name="encoding">Pass the default encoding to use.  On
        /// return, it contains the actual encoding for the file.</param>
        /// <returns>The contents of the file.</returns>
        /// <remarks>When reading the file, use the default encoding specified
        /// but detect the encoding if byte order marks are present.  In
        /// addition, if the template is an XML file and it contains an
        /// encoding identifier in the XML tag, the file is read using
        /// that encoding.</remarks>
        public static string ReadWithEncoding(string filename, ref Encoding encoding)
        {
            Encoding fileEnc;
            string content;

            using(StreamReader sr = new StreamReader(filename, encoding, true))
            {
                content = sr.ReadToEnd();

                // Get the actual encoding used
                encoding = sr.CurrentEncoding;
            }

            Match m = reXmlEncoding.Match(content);

            // Re-read an XML file using the correct encoding?
            if(m.Success)
            {
                fileEnc = Encoding.GetEncoding(m.Groups["Encoding"].Value);

                if(fileEnc != encoding)
                {
                    encoding = fileEnc;

                    using(StreamReader sr = new StreamReader(filename, encoding, true))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }

            return content;
        }

        /// <summary>
        /// Replace a field tag with a value from the project
        /// </summary>
        /// <param name="match">The match that was found</param>
        /// <returns>The string to use as the replacement</returns>
        private string OnFieldMatch(Match match)
        {
            ProjectProperty buildProp;
            FileItemCollection fileItems;
            StringBuilder sb;
            string replaceWith, fieldName;
            string[] parts;

            fieldName = match.Groups["Field"].Value.ToLower(CultureInfo.InvariantCulture);

            switch(fieldName)
            {
                case "appdatafolder":
                    // This folder should exist if used
                    replaceWith = FolderPath.TerminatePath(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                        Constants.ProgramDataFolder));
                    break;

                case "localdatafolder":
                    // This folder may not exist and we may need to create it
                    replaceWith = FolderPath.TerminatePath(Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        Constants.ProgramDataFolder));

                    if(!Directory.Exists(replaceWith))
                        Directory.CreateDirectory(replaceWith);
                    break;

                case "shfbfolder":
                    replaceWith = shfbFolder;
                    break;

                case "componentsfolder":
                    replaceWith = BuildComponentManager.BuildComponentsFolder;
                    break;

                case "projectfolder":
                    replaceWith = Path.GetDirectoryName(originalProjectName);

                    if(replaceWith.Length == 0)
                        replaceWith = Directory.GetCurrentDirectory();

                    replaceWith += @"\";
                    break;

                case "htmlencprojectfolder":
                    replaceWith = HttpUtility.HtmlEncode(Path.GetDirectoryName(originalProjectName));

                    if(replaceWith.Length == 0)
                        replaceWith = HttpUtility.HtmlEncode(
                            Directory.GetCurrentDirectory());

                    replaceWith += @"\";
                    break;

                case "outputfolder":
                    replaceWith = outputFolder;
                    break;

                case "htmlencoutputfolder":
                    replaceWith = HttpUtility.HtmlEncode(outputFolder);
                    break;

                case "workingfolder":
                    replaceWith = workingFolder;
                    break;

                case "htmlencworkingfolder":
                    replaceWith = HttpUtility.HtmlEncode(workingFolder);
                    break;

                case "sandcastlepath":
                    replaceWith = sandcastleFolder;
                    break;

                case "presentationpath":
                    replaceWith = presentationFolder;
                    break;

                case "presentationstyle":
                    replaceWith = project.PresentationStyle;
                    break;

                case "presentationparam":
                    replaceWith = presentationParam;
                    break;

                case "hhcpath":
                    replaceWith = hhcFolder;
                    break;

                case "hxcomppath":
                    replaceWith = hxcompFolder;
                    break;

                case "docinternals":
                    if(project.DocumentInternals || project.DocumentPrivates)
                        replaceWith = "true";
                    else
                        replaceWith = "false";
                    break;

                case "htmlhelpname":
                    replaceWith = project.HtmlHelpName;
                    break;

                case "htmlenchelpname":
                    replaceWith = HttpUtility.HtmlEncode(project.HtmlHelpName);
                    break;

                case "helpviewersetupname":
                    // Help viewer setup names cannot contain periods so we'll replace them with underscores
                    replaceWith = HttpUtility.HtmlEncode(project.HtmlHelpName.Replace('.', '_'));
                    break;

                case "frameworkcommentlist":
                case "cachedframeworkcommentlist":
                case "importframeworkcommentlist":
                    replaceWith = this.FrameworkCommentList(match.Groups["Field"].Value.ToLower(
                        CultureInfo.InvariantCulture));
                    break;

                case "commentfilelist":
                    replaceWith = commentsFiles.CommentFileList(workingFolder, false);
                    break;

                case "inheritedcommentfilelist":
                    replaceWith = commentsFiles.CommentFileList(workingFolder, true);
                    break;

                case "helptitle":
                    replaceWith = project.HelpTitle;
                    break;

                case "htmlenchelptitle":
                    replaceWith = HttpUtility.HtmlEncode(project.HelpTitle);
                    break;

                case "scripthelptitle":
                    // This is used when the title is passed as a parameter
                    // to a JavaScript function.
                    replaceWith = HttpUtility.HtmlEncode(project.HelpTitle).Replace("'", @"\'");
                    break;

                case "urlenchelptitle": // Just replace &, <, >, and " for  now
                    replaceWith = project.HelpTitle.Replace("&", "%26").Replace(
                        "<", "%3C").Replace(">", "%3E").Replace("\"", "%22");
                    break;

                case "rootnamespacetitle":
                    replaceWith = project.RootNamespaceTitle;

                    if(replaceWith.Length == 0)
                        replaceWith = "<include item=\"rootTopicTitleLocalized\"/>";
                    break;

                case "binarytoc":
                    replaceWith = project.BinaryTOC ? "Yes" : "No";
                    break;

                case "windowoptions":
                    // Currently, we use a default set of options and only
                    // allow showing or hiding the Favorites tab.
                    replaceWith = (project.IncludeFavorites) ? "0x63520" : "0x62520";
                    break;

                case "langid":
                    replaceWith = language.LCID.ToString(CultureInfo.InvariantCulture);
                    break;

                case "language":
                    replaceWith = String.Format(CultureInfo.InvariantCulture,
                        "0x{0:X} {1}", language.LCID, language.NativeName);
                    break;

                case "languagefolder":
                    replaceWith = languageFolder;
                    break;

                case "locale":
                    replaceWith = language.Name.ToLower(CultureInfo.InvariantCulture);
                    break;

                case "copyright":
                    // Include copyright info if there is a copyright HREF or
                    // copyright text.
                    if(project.CopyrightHref.Length != 0 || project.CopyrightText.Length != 0)
                        replaceWith = "<include item=\"copyright\"/>";
                    else
                        replaceWith = String.Empty;
                    break;

                case "copyrightinfo":
                    if(project.CopyrightHref.Length == 0 && project.CopyrightText.Length == 0)
                        replaceWith = String.Empty;
                    else
                        if(project.CopyrightHref.Length == 0)
                            replaceWith = project.DecodedCopyrightText;
                        else
                            if(project.CopyrightText.Length == 0)
                                replaceWith = project.CopyrightHref;
                            else
                                replaceWith = String.Format(CultureInfo.CurrentCulture, "{0} ({1})",
                                    project.DecodedCopyrightText, project.CopyrightHref);
                    break;

                case "htmlenccopyrightinfo":
                    if(project.CopyrightHref.Length == 0 && project.CopyrightText.Length == 0)
                        replaceWith = String.Empty;
                    else
                        if(project.CopyrightHref.Length == 0)
                            replaceWith = "<p/>" + HttpUtility.HtmlEncode(project.DecodedCopyrightText);
                        else
                            if(project.CopyrightText.Length == 0)
                                replaceWith = String.Format(CultureInfo.CurrentCulture,
                                    "<p/><a href='{0}' target='_blank'>{0}</a>",
                                    HttpUtility.HtmlEncode(project.CopyrightHref));
                            else
                                replaceWith = String.Format(CultureInfo.CurrentCulture,
                                    "<p/><a href='{0}' target='_blank'>{1}</a>",
                                    HttpUtility.HtmlEncode(project.CopyrightHref),
                                    HttpUtility.HtmlEncode(project.DecodedCopyrightText));
                    break;

                case "copyrighthref":
                    replaceWith = project.CopyrightHref;
                    break;

                case "htmlenccopyrighthref":
                    if(project.CopyrightHref.Length == 0)
                        replaceWith = String.Empty;
                    else
                        replaceWith = String.Format(CultureInfo.CurrentCulture,
                            "<a href='{0}' target='_blank'>{0}</a>",
                            HttpUtility.HtmlEncode(project.CopyrightHref));
                    break;

                case "copyrighttext":
                    if(project.CopyrightText.Length == 0)
                        replaceWith = String.Empty;
                    else
                        replaceWith = project.DecodedCopyrightText;
                    break;

                case "htmlenccopyrighttext":
                    if(project.CopyrightText.Length == 0)
                        replaceWith = String.Empty;
                    else
                        replaceWith = HttpUtility.HtmlEncode(project.DecodedCopyrightText);
                    break;

                case "comments":
                    // Include "send comments" line if feedback e-mail address
                    // is specified.
                    if(project.FeedbackEMailAddress.Length != 0)
                        replaceWith = "<include item=\"comments\"/>";
                    else
                        replaceWith = String.Empty;
                    break;

                case "feedbackemailaddress":
                    replaceWith = project.FeedbackEMailAddress;
                    break;

                case "feedbackemaillinktext":
                    replaceWith = project.FeedbackEMailLinkText;
                    break;

                case "urlencfeedbackemailaddress":
                    if(project.FeedbackEMailAddress.Length == 0)
                        replaceWith = String.Empty;
                    else
                        replaceWith = HttpUtility.UrlEncode(project.FeedbackEMailAddress);
                    break;

                case "htmlencfeedbackemailaddress":
                    // If link text is specified, it will be used instead
                    if(project.FeedbackEMailAddress.Length == 0)
                        replaceWith = String.Empty;
                    else
                        if(project.FeedbackEMailLinkText.Length == 0)
                            replaceWith = HttpUtility.HtmlEncode(project.FeedbackEMailAddress);
                        else
                            replaceWith = HttpUtility.HtmlEncode(project.FeedbackEMailLinkText);
                    break;

                case "headertext":
                    replaceWith = project.HeaderText;
                    break;

                case "footertext":
                    replaceWith = project.FooterText;
                    break;

                case "indenthtml":
                    replaceWith = project.IndentHtml.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "preliminary":
                    // Include the "preliminary" warning in the header text if wanted
                    if(project.Preliminary)
                        replaceWith = "<include item=\"preliminary\"/>";
                    else
                        replaceWith = String.Empty;
                    break;

                case "defaulttopic":
                    if(defaultTopic.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                        replaceWith = Path.ChangeExtension(defaultTopic, ".html");
                    else
                        replaceWith = defaultTopic;
                    break;

                case "webdefaulttopic":
                    if(defaultTopic.EndsWith(".topic", StringComparison.OrdinalIgnoreCase))
                        replaceWith = Path.ChangeExtension(defaultTopic, ".html").Replace('\\', '/');
                    else
                        replaceWith = defaultTopic.Replace('\\', '/');
                    break;

                case "frameworkversion":
                    replaceWith = project.FrameworkVersionNumber;
                    break;

                case "frameworkversionshort":
                    replaceWith = project.FrameworkVersionNumber.Substring(0, 3);
                    break;

                case "mrefframeworkversion":
                    replaceWith = project.FrameworkVersionNumber;

                    // For .NET 3.0 or higher, Microsoft says to use the .NET 2.0 framework files
                    if(replaceWith[0] >= '3')
                        replaceWith = FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(".NET 2.0");
                    break;

                case "mrefframeworkversionshort":
                    replaceWith = project.FrameworkVersionNumber.Substring(0, 3);

                    // For .NET 3.0 or higher, Microsoft says to use the .NET 2.0 framework files
                    if(replaceWith[0] >= '3')
                        replaceWith = "2.0";
                    break;

                case "targetframeworkidentifier":
                    if(project.FrameworkVersion.StartsWith(".NET", StringComparison.OrdinalIgnoreCase))
                        replaceWith = ".NETFramework";
                    else
                        replaceWith = "Silverlight";
                    break;

                case "help1xprojectfiles":
                    replaceWith = this.HelpProjectFileList(String.Format(CultureInfo.InvariantCulture,
                        @"{0}Output\{1}", workingFolder, HelpFileFormat.HtmlHelp1), HelpFileFormat.HtmlHelp1);
                    break;

                case "help2xprojectfiles":
                    replaceWith = this.HelpProjectFileList(String.Format(CultureInfo.InvariantCulture,
                        @"{0}Output\{1}", workingFolder, HelpFileFormat.MSHelp2), HelpFileFormat.MSHelp2);
                    break;

                case "htmlsdklinktype":
                    replaceWith = project.HtmlSdkLinkType.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "mshelp2sdklinktype":
                    replaceWith = project.MSHelp2SdkLinkType.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "mshelpviewersdklinktype":
                    replaceWith = project.MSHelpViewerSdkLinkType.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "websitesdklinktype":
                    replaceWith = project.WebsiteSdkLinkType.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "sdklinktarget":
                    replaceWith = "_" + project.SdkLinkTarget.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "htmltoc":
                    replaceWith = this.GenerateHtmlToc();
                    break;

                case "syntaxfilters":
                    replaceWith = BuildComponentManager.SyntaxFilterGeneratorsFrom(project.SyntaxFilters);
                    break;

                case "syntaxfiltersdropdown":
                    // Note that we can't remove the dropdown box if only a single
                    // language is selected as script still depends on it.
                    replaceWith = BuildComponentManager.SyntaxFilterLanguagesFrom(project.SyntaxFilters);
                    break;

                case "autodocumentconstructors":
                    replaceWith = project.AutoDocumentConstructors.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "autodocumentdisposemethods":
                    replaceWith = project.AutoDocumentDisposeMethods.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingparams":
                    replaceWith = project.ShowMissingParams.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingremarks":
                    replaceWith = project.ShowMissingRemarks.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingreturns":
                    replaceWith = project.ShowMissingReturns.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingsummaries":
                    replaceWith = project.ShowMissingSummaries.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingtypeparams":
                    replaceWith = project.ShowMissingTypeParams.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingvalues":
                    replaceWith = project.ShowMissingValues.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingnamespaces":
                    replaceWith = project.ShowMissingNamespaces.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "showmissingincludetargets":
                    replaceWith = project.ShowMissingIncludeTargets.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "apifilter":
                    // In a partial build used to get API info for the API
                    // filter designer, we won't apply the filter.
                    if(!suppressApiFilter)
                        replaceWith = apiFilter.ToString();
                    else
                        replaceWith = String.Empty;
                    break;

                case "builddate":
                    // Apply a format specifier?
                    if(match.Groups["Format"].Value.Length != 0)
                        replaceWith = String.Format(CultureInfo.CurrentCulture,
                            "{0:" + match.Groups["Format"].Value + "}", DateTime.Now);
                    else
                        replaceWith = DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    break;

                case "includeprojectnode":
                    if(project.RootNamespaceContainer)
                        replaceWith = "project=Project";
                    else
                        replaceWith = String.Empty;
                    break;

                case "help1folder":
                    if((project.HelpFileFormat & HelpFileFormat.HtmlHelp1) != 0)
                        replaceWith = @"Output\" + HelpFileFormat.HtmlHelp1.ToString();
                    else
                        replaceWith = String.Empty;
                    break;

                case "websitefolder":
                    if((project.HelpFileFormat & HelpFileFormat.Website) != 0)
                        replaceWith = @"Output\" + HelpFileFormat.Website.ToString();
                    else
                        replaceWith = String.Empty;
                    break;

                case "stopwordfile":
                    if(project.IncludeStopWordList)
                        replaceWith = "StopWordFile=\"StopWordList.txt\"";
                    else
                        replaceWith = String.Empty;
                    break;

                case "stopwordlistfilename":
                    if(project.IncludeStopWordList)
                        replaceWith = "StopWordList.txt";
                    else
                        replaceWith = String.Empty;
                    break;

                case "collectiontocstyle":
                    replaceWith = project.CollectionTocStyle.ToString();
                    break;

                case "helpfileversion":
                    replaceWith = project.HelpFileVersion;
                    break;

                case "h2regpluginentries":
                    parts = project.PlugInNamespaces.Split(',');
                    sb = new StringBuilder(1024);

                    foreach(string ns in parts)
                    {
                        replaceWith = ns.Trim();

                        if(replaceWith.Length != 0)
                            sb.AppendFormat("{0}|_DEFAULT|{1}|_DEFAULT\r\n", replaceWith, project.HtmlHelpName);
                    }

                    replaceWith = sb.ToString();
                    break;

                case "h2regmergenamespaces":
                    parts = project.PlugInNamespaces.Split(',');
                    sb = new StringBuilder(1024);

                    foreach(string ns in parts)
                    {
                        replaceWith = ns.Trim();

                        if(replaceWith.Length != 0)
                            sb.AppendFormat("{0}|AUTO\r\n", replaceWith);
                    }

                    replaceWith = sb.ToString();
                    break;

                case "helpattributes":
                    replaceWith = project.HelpAttributes.ToConfigurationString();
                    break;

                case "tokenfiles":
                    sb = new StringBuilder(1024);

                    if(conceptualContent != null)
                        fileItems = conceptualContent.TokenFiles;
                    else
                        fileItems = new FileItemCollection(project, BuildAction.Tokens);

                    foreach(FileItem file in fileItems)
                        sb.AppendFormat("<content file=\"{0}\" />\r\n", Path.GetFileName(file.FullPath));

                    replaceWith = sb.ToString();
                    break;

                case "codesnippetsfiles":
                    sb = new StringBuilder(1024);

                    if(conceptualContent != null)
                        fileItems = conceptualContent.CodeSnippetFiles;
                    else
                        fileItems = new FileItemCollection(project, BuildAction.CodeSnippets);

                    foreach(FileItem file in fileItems)
                        sb.AppendFormat("<examples file=\"{0}\" />\r\n", file.FullPath);

                    replaceWith = sb.ToString();
                    break;

                case "resourceitemfiles":
                    sb = new StringBuilder(1024);

                    fileItems = new FileItemCollection(project, BuildAction.ResourceItems);

                    // Files are copied and transformed as they may contain
                    // substitution tags.
                    foreach(FileItem file in fileItems)
                    {
                        sb.AppendFormat("<content file=\"{0}\" />\r\n", Path.GetFileName(file.FullPath));

                        this.TransformTemplate(Path.GetFileName(file.FullPath),
                            Path.GetDirectoryName(file.FullPath), workingFolder);
                    }

                    replaceWith = sb.ToString();
                    break;

                case "helpfileformat":
                    replaceWith = project.HelpFileFormat.ToString();
                    break;

                case "helpformatoutputpaths":
                    sb = new StringBuilder(1024);

                    // Add one entry for each help file format being generated
                    foreach(string baseFolder in this.HelpFormatOutputFolders)
                        sb.AppendFormat("<path value=\"{0}\" />", baseFolder.Substring(workingFolder.Length));

                    replaceWith = sb.ToString();
                    break;

                case "catalogproductid":
                    replaceWith = project.CatalogProductId;
                    break;

                case "catalogversion":
                    replaceWith = project.CatalogVersion;
                    break;

                case "vendorname":
                    replaceWith = !String.IsNullOrEmpty(project.VendorName) ? project.VendorName : "Vendor Name";
                    break;

                case "htmlencvendorname":
                    replaceWith = !String.IsNullOrEmpty(project.VendorName) ?
                        HttpUtility.HtmlEncode(project.VendorName) : "Vendor Name";
                    break;

                case "producttitle":
                    replaceWith = !String.IsNullOrEmpty(project.ProductTitle) ?
                        project.ProductTitle : project.HelpTitle;
                    break;

                case "htmlencproducttitle":
                    replaceWith = !String.IsNullOrEmpty(project.ProductTitle) ?
                        HttpUtility.HtmlEncode(project.ProductTitle) : HttpUtility.HtmlEncode(project.HelpTitle);
                    break;

                case "selfbranded":
                    replaceWith = project.SelfBranded.ToString().ToLower(CultureInfo.InvariantCulture);
                    break;

                case "topicversion":
                    replaceWith = HttpUtility.HtmlEncode(project.TopicVersion);
                    break;

                case "tocparentid":
                    replaceWith = HttpUtility.HtmlEncode(project.TocParentId);
                    break;

                case "tocparentversion":
                    replaceWith = HttpUtility.HtmlEncode(project.TocParentVersion);
                    break;

                case "apitocparentid":
                    // If null, empty or it starts with '*', it's parented to the root node
                    if(!String.IsNullOrEmpty(this.ApiTocParentId) && this.ApiTocParentId[0] != '*')
                    {
                        // Ensure that the ID is valid and visible in the TOC
                        if(!conceptualContent.Topics.Any(t => t[this.ApiTocParentId] != null &&
                          t[this.ApiTocParentId].Visible))
                            throw new BuilderException("BE0022", String.Format(CultureInfo.CurrentCulture,
                                "The project's ApiTocParent property value '{0}' must be associated with a topic in " +
                                "your project's conceptual content and must have its Visible property set to True in " +
                                "the content layout file.", this.ApiTocParentId));

                        replaceWith = HttpUtility.HtmlEncode(this.ApiTocParentId);
                    }
                    else
                        if(!String.IsNullOrEmpty(this.RootContentContainerId))
                            replaceWith = HttpUtility.HtmlEncode(this.RootContentContainerId);
                        else
                            replaceWith = HttpUtility.HtmlEncode(project.TocParentId);
                    break;

                default:
                    // Try for a custom project property.  Use the last one since the original may be
                    // in a parent project file or it may have been overridden from the command line.
                    buildProp = project.MSBuildProject.AllEvaluatedProperties.LastOrDefault(
                        p => p.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                    if(buildProp != null)
                        replaceWith = buildProp.EvaluatedValue;
                    else
                    {
                        // If not there, try the global properties.  If still not found, give up.
                        string key = project.MSBuildProject.GlobalProperties.Keys.FirstOrDefault(
                            k => k.Equals(fieldName, StringComparison.OrdinalIgnoreCase));

                        if(key == null || !project.MSBuildProject.GlobalProperties.TryGetValue(key, out replaceWith))
                            throw new BuilderException("BE0020", String.Format(CultureInfo.CurrentCulture,
                                "Unknown field tag: '{0}'", match.Groups["Field"].Value));
                    }
                    break;
            }

            return replaceWith;
        }

        /// <summary>
        /// This is used to get a list of .NET Framework comments file
        /// locations.
        /// </summary>
        /// <param name="frameworkLocations">The locations of the comments
        /// files.</param>
        /// <param name="cacheNames">A dictionary to contain the cache file
        /// names for the comments file sets.</param>
        /// <param name="language">If the given language version of the
        /// comments are present, they will be used.  If not, the default
        /// English version comments files are used.</param>
        /// <param name="version">The framework version for which to get
        /// comments files.</param>
        public static void GetFrameworkCommentsFiles(Collection<string> frameworkLocations,
          Dictionary<string, string> cacheNames, CultureInfo language, string version)
        {
            string folder, langFolder;

            // For Silverlight, we need to look in different locations
            if(version.StartsWith("Silverlight", StringComparison.OrdinalIgnoreCase))
            {
                version = FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(version);

                folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                    @"\Reference Assemblies\Microsoft\Framework\Silverlight\v" + version;
                frameworkLocations.Add(folder);
                cacheNames.Add(folder, "Silverlight_" + version);

                folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                    @"\Microsoft SDKs\Silverlight\v" + version + @"\Libraries\Client";
                frameworkLocations.Add(folder);
                cacheNames.Add(folder, "SilverlightSDK_" + version);

                if(version[0] == '4')
                {
                    folder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) +
                        @"\Microsoft SDKs\RIA Services\v1.0\Libraries";
                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, "RIA_SDK_V1.0");
                }

                return;
            }

            version = FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(version);

            switch(version[0])
            {
                case '1':   // 1.x
                    version = FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(".NET 1.1");
                    folder = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Microsoft.NET\Framework\v" + version);
                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, version);
                    break;

                case '2':   // 2.0, 3.0, and 3.5 are cummulative
                case '3':
                    folder = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\Microsoft.NET\Framework\v" +
                        FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(".NET 2") + @"\");

                    // Check for a language-specific set of comments
                    if(Directory.Exists(folder + language.Name))
                    {
                        folder += language.Name;
                        langFolder = language.Name + "_";
                    }
                    else
                        if(Directory.Exists(folder + language.TwoLetterISOLanguageName))
                        {
                            folder += language.TwoLetterISOLanguageName;
                            langFolder = language.TwoLetterISOLanguageName + "_";
                        }
                        else
                            langFolder = String.Empty;

                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, langFolder + FrameworkVersionTypeConverter.LatestFrameworkNumberMatching(".NET 2"));

                    // Check for FSharp comments files
                    if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference " +
                            @"Assemblies\Microsoft\FSharp\2.0\Runtime\v2.0\");
                    else
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Reference " +
                            @"Assemblies\Microsoft\FSharp\2.0\Runtime\v2.0\");

                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, "FSharp_2.0");

                    // 3.0/3.5
                    if(version[0] == '3')
                    {
                        // If running on a 64-bit platform, use the x86 folder as that's where the
                        // comments files will be.
                        if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
                            folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference " +
                                @"Assemblies\Microsoft\Framework\v3.0\");
                        else
                            folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Reference Assemblies\" +
                                @"Microsoft\Framework\v3.0\");

                        // Check for a language-specific set of comments
                        if(Directory.Exists(folder + language.Name))
                        {
                            folder += language.Name;
                            langFolder = language.Name + "_";
                        }
                        else
                            if(Directory.Exists(folder + language.TwoLetterISOLanguageName))
                            {
                                folder += language.TwoLetterISOLanguageName;
                                langFolder = language.TwoLetterISOLanguageName + "_";
                            }
                            else
                                langFolder = String.Empty;

                        frameworkLocations.Add(folder);
                        cacheNames.Add(folder, langFolder + "3.0");

                        // 3.5
                        if(version == "3.5")
                        {
                            // If running on a 64-bit platform, use the x86 folder as that's where the
                            // comments files will be.
                            if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
                                folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference " +
                                    @"Assemblies\Microsoft\Framework\v3.5\");
                            else
                                folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Reference Assemblies\" +
                                    @"Microsoft\Framework\v3.5\");

                            // Check for a language-specific set of comments
                            if(Directory.Exists(folder + language.Name))
                            {
                                folder += language.Name;
                                langFolder = language.Name + "_";
                            }
                            else
                                if(Directory.Exists(folder + language.TwoLetterISOLanguageName))
                                {
                                    folder += language.TwoLetterISOLanguageName;
                                    langFolder = language.TwoLetterISOLanguageName + "_";
                                }
                                else
                                    langFolder = String.Empty;

                            frameworkLocations.Add(folder);
                            cacheNames.Add(folder, langFolder + version);
                        }
                    }
                    break;

                case '4':   // 4.x
                    // If running on a 64-bit platform, use the x86 folder as that's where the
                    // comments files will be.
                    if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference " +
                            @"Assemblies\Microsoft\Framework\.NETFramework\v4.0\");
                    else
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Reference Assemblies\" +
                            @"Microsoft\Framework\.NETFramework\v4.0\");

                    // Check for a language-specific set of comments
                    if(Directory.Exists(folder + language.Name))
                    {
                        folder += language.Name;
                        langFolder = language.Name + "_";
                    }
                    else
                        if(Directory.Exists(folder + language.TwoLetterISOLanguageName))
                        {
                            folder += language.TwoLetterISOLanguageName;
                            langFolder = language.TwoLetterISOLanguageName + "_";
                        }
                        else
                            langFolder = String.Empty;

                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, langFolder + version);

                    // Check for FSharp comments files
                    if(Environment.GetEnvironmentVariable("ProgramFiles(x86)") != null)
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Reference " +
                            @"Assemblies\Microsoft\FSharp\2.0\Runtime\v4.0\");
                    else
                        folder = Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Reference " +
                            @"Assemblies\Microsoft\FSharp\2.0\Runtime\v4.0\");

                    frameworkLocations.Add(folder);
                    cacheNames.Add(folder, "FSharp_4.0");
                    break;

                default:    // Future version, default to 4.0 files until they actual location is determined
                    GetFrameworkCommentsFiles(frameworkLocations, cacheNames, language, ".NET 4.0");
                    break;
            }
        }

        /// <summary>
        /// This is used to generate an appropriate list of entries that
        /// represent .NET Framework comments file locations for the various
        /// configuration files.
        /// </summary>
        /// <param name="listType">The type of list to generate
        /// (frameworkcommentlist, importframeworkcommentlist, or
        /// cachedframeworkcommentlist)</param>
        /// <returns>The list of framework comments file sources in the
        /// appropriate format.</returns>
        private string FrameworkCommentList(string listType)
        {
            StringBuilder sb = new StringBuilder(1024);
            Dictionary<string, string> cacheName = new Dictionary<string, string>();
            Collection<string> frameworkLocations = new Collection<string>();

            BuildProcess.GetFrameworkCommentsFiles(frameworkLocations, cacheName, project.Language,
                project.FrameworkVersion);

            // Build the list based on the type and what actually exists
            foreach(string location in frameworkLocations)
                if(Directory.Exists(location))
                    switch(listType)
                    {
                        case "importframeworkcommentlist":
                            sb.AppendFormat(CultureInfo.InvariantCulture,
                                "<import path=\"{0}\" recurse=\"true\" />\r\n", location);
                            break;

                        case "cachedframeworkcommentlist":
                            // Files are cached by language and version
                            sb.AppendFormat(CultureInfo.InvariantCulture, "<cache base=\"{0}\" files=\"*.xml\" " +
                                "recurse=\"true\" cacheFile=\"{{@LocalDataFolder}}Cache\\{1}.cache\" />\r\n",
                                location, cacheName[location]);
                            break;

                        default:    // "frameworkcommentlist"
                            sb.AppendFormat(CultureInfo.InvariantCulture, "<data base=\"{0}\" files=\"*.xml\" " +
                                "recurse=\"true\" />\r\n", location);
                            break;
                    }

            return sb.ToString();
        }

        /// <summary>
        /// This is used to merge the component configurations from the
        /// project with the <b>sandcastle.config</b> file.
        /// </summary>
        private void MergeComponentConfigurations()
        {
            Dictionary<string, XmlNode> outputNodes = new Dictionary<string, XmlNode>();
            BuildComponentInfo info;
            BuildComponentConfiguration projectComp;
            XmlDocument config;
            XmlNode rootNode, configNode, clone;
            XmlNodeList outputFormats;
            string configName, compConfig;

            this.ReportProgress(BuildStep.MergeCustomConfigs, "Merging custom build component configurations");

            // Reset the adjusted instance values to match the configuration instance values
            foreach(BuildComponentInfo component in BuildComponentManager.BuildComponents.Values)
            {
                component.ReferenceBuildPosition.AdjustedInstance = component.ReferenceBuildPosition.Instance;
                component.ConceptualBuildPosition.AdjustedInstance = component.ConceptualBuildPosition.Instance;
            }

            if(this.ExecutePlugIns(ExecutionBehaviors.InsteadOf))
                return;

            this.ExecutePlugIns(ExecutionBehaviors.Before);

            configName = workingFolder + "sandcastle.config";
            this.ReportProgress(configName);

            config = new XmlDocument();
            config.Load(configName);
            rootNode = config.SelectSingleNode("configuration/dduetools/builder/components");
            mergeStack = new Stack<string>();

            foreach(string id in project.ComponentConfigurations.Keys)
            {
                projectComp = project.ComponentConfigurations[id];

                if(!BuildComponentManager.BuildComponents.TryGetValue(id, out info))
                    throw new BuilderException("BE0021", String.Format(CultureInfo.InvariantCulture,
                        "The project contains a reference to a custom build " +
                        "component '{0}' that could not be found.", id));

                if(info.ReferenceBuildPosition.Place == ComponentPosition.Placement.NotUsed)
                    this.ReportProgress("    Skipping component '{0}', not used in reference build", id);
                else
                    if(projectComp.Enabled)
                    {
                        compConfig = projectComp.Configuration;

                        // Replace template tags.  They may be nested.
                        while(reField.IsMatch(compConfig))
                            compConfig = reField.Replace(compConfig, fieldMatchEval);

                        configNode = config.CreateDocumentFragment();
                        configNode.InnerXml = compConfig;
                        outputNodes.Clear();

                        foreach(XmlNode match in configNode.SelectNodes("//helpOutput"))
                        {
                            outputNodes.Add(match.Attributes["format"].Value, match);
                            match.ParentNode.RemoveChild(match);
                        }

                        // Is it output format specific?
                        if(outputNodes.Count == 0)
                        {
                            // Replace the component in the file
                            this.MergeComponent(id, info, rootNode, configNode, false);
                        }
                        else
                        {
                            // Replace the component in each output format node
                            outputFormats = rootNode.SelectNodes(
                                "component[@id='Multi-format Output Component']/helpOutput");

                            foreach(XmlNode format in outputFormats)
                            {
                                clone = configNode.Clone();
                                clone.FirstChild.InnerXml = outputNodes[format.Attributes["format"].Value].InnerXml;
                                this.MergeComponent(id, info, format, clone, false);
                            }
                        }
                    }
                    else
                        this.ReportProgress("    The configuration for '{0}' is disabled and will not be used.", id);
            }

            config.Save(configName);

            // Do the same for conceptual config if necessary
            if(conceptualContent.ContentLayoutFiles.Count != 0)
            {
                configName = workingFolder + "conceptual.config";
                this.ReportProgress(configName);

                config = new XmlDocument();
                config.Load(configName);
                rootNode = config.SelectSingleNode("configuration/dduetools/builder/components");
                mergeStack.Clear();

                foreach(string id in project.ComponentConfigurations.Keys)
                {
                    projectComp = project.ComponentConfigurations[id];

                    if(!BuildComponentManager.BuildComponents.TryGetValue(id, out info))
                        throw new BuilderException("BE0021", String.Format(CultureInfo.InvariantCulture,
                            "The project contains a reference to a custom build " +
                            "component '{0}' that could not be found.", id));

                    if(info.ConceptualBuildPosition.Place == ComponentPosition.Placement.NotUsed)
                        this.ReportProgress("    Skipping component '{0}', not used in conceptual build", id);
                    else
                        if(projectComp.Enabled)
                        {
                            compConfig = projectComp.Configuration;

                            // Replace template tags.  They may be nested.
                            while(reField.IsMatch(compConfig))
                                compConfig = reField.Replace(compConfig, fieldMatchEval);

                            configNode = config.CreateDocumentFragment();
                            configNode.InnerXml = compConfig;
                            outputNodes.Clear();

                            foreach(XmlNode match in configNode.SelectNodes("//helpOutput"))
                            {
                                outputNodes.Add(match.Attributes["format"].Value, match);
                                match.ParentNode.RemoveChild(match);
                            }

                            // Is it output format specific?
                            if(outputNodes.Count == 0)
                            {
                                // Replace the component in the file
                                this.MergeComponent(id, info, rootNode, configNode, true);
                            }
                            else
                            {
                                // Replace the component in each output format node
                                outputFormats = rootNode.SelectNodes(
                                    "component[@id='Multi-format Output Component']/helpOutput");

                                foreach(XmlNode format in outputFormats)
                                {
                                    clone = configNode.Clone();
                                    clone.FirstChild.InnerXml = outputNodes[format.Attributes["format"].Value].InnerXml;
                                    this.MergeComponent(id, info, format, clone, true);
                                }
                            }
                        }
                        else
                            this.ReportProgress("    The configuration for '{0}' is disabled and will not be used.", id);
                }

                // Remove the example component if there are no snippets file
                if(conceptualContent.CodeSnippetFiles.Count == 0)
                {
                    this.ReportProgress("    Removing unused ExampleComponent.");
                    configNode = rootNode.SelectSingleNode("component[@type = 'Microsoft.Ddue.Tools.ExampleComponent']");

                    if(configNode != null)
                        configNode.ParentNode.RemoveChild(configNode);
                }

                config.Save(configName);
            }

            this.ExecutePlugIns(ExecutionBehaviors.After);
            mergeStack = null;
        }

        /// <summary>
        /// This handles merging of the custom component configurations into
        /// the configuration file including dependencies.
        /// </summary>
        /// <param name="id">The ID of the component to merge</param>
        /// <param name="info">The build component definition</param>
        /// <param name="rootNode">The root container node</param>
        /// <param name="configNode">The configuration node to merge</param>
        /// <param name="isConceptualConfig">True if this is a conceptual
        /// content configuration file or false if it is a reference build
        /// configuration file.</param>
        private void MergeComponent(string id, BuildComponentInfo info, XmlNode rootNode, XmlNode configNode,
          bool isConceptualConfig)
        {
            BuildComponentInfo depInfo;
            ComponentPosition position;
            XmlNodeList matchingNodes;
            XmlNode node;
            string replaceId;

            // Merge dependent component configurations first
            if(info.Dependencies.Count != 0)
                foreach(string dependency in info.Dependencies)
                {
                    node = rootNode.SelectSingleNode("component[@id='" + dependency + "']");

                    // If it's already there or would create a circular
                    // dependency, ignore it.
                    if(node != null || mergeStack.Contains(dependency))
                        continue;

                    // Add the dependency with a default configuration
                    if(!BuildComponentManager.BuildComponents.TryGetValue(dependency, out depInfo))
                        throw new BuilderException("BE0023", String.Format(
                            CultureInfo.InvariantCulture, "The project contains " +
                            "a reference to a custom build component '{0}' that " +
                            "has a dependency '{1}' that could not be found.",
                            id, dependency));

                    node = rootNode.OwnerDocument.CreateDocumentFragment();
                    node.InnerXml = reField.Replace(depInfo.DefaultConfiguration, fieldMatchEval);

                    this.ReportProgress("    Merging '{0}' dependency for '{1}'", dependency, id);

                    mergeStack.Push(dependency);
                    this.MergeComponent(dependency, depInfo, rootNode, node, isConceptualConfig);
                    mergeStack.Pop();
                }

            position = (!isConceptualConfig) ? info.ReferenceBuildPosition : info.ConceptualBuildPosition;

            // Find all matching components by ID or type name
            if(!String.IsNullOrEmpty(position.Id))
            {
                replaceId = position.Id;
                matchingNodes = rootNode.SelectNodes("component[@id='" + replaceId + "']");
            }
            else
            {
                replaceId = position.TypeName;
                matchingNodes = rootNode.SelectNodes("component[@type='" + replaceId + "']");
            }

            // If replacing another component, search for that by ID or
            // type and replace it if found.
            if(position.Place == ComponentPosition.Placement.Replace)
            {
                if(matchingNodes.Count < position.AdjustedInstance)
                {
                    this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to replace with " +
                        "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);

                    // If it's a dependency, that's a problem
                    if(mergeStack.Count != 0)
                        throw new BuilderException("BE0024", "Unable to add dependent configuration: " + id);

                    return;
                }

                rootNode.ReplaceChild(configNode, matchingNodes[position.AdjustedInstance - 1]);

                this.ReportProgress("    Replaced configuration for '{0}' (instance {1}) with configuration " +
                    "for '{2}'", replaceId, position.AdjustedInstance, id);

                // Adjust instance values on matching components
                foreach(BuildComponentInfo component in BuildComponentManager.BuildComponents.Values)
                  if(!isConceptualConfig)
                  {
                    if(((!String.IsNullOrEmpty(component.ReferenceBuildPosition.Id) &&
                      component.ReferenceBuildPosition.Id == replaceId) ||
                      (String.IsNullOrEmpty(component.ReferenceBuildPosition.Id) &&
                      component.ReferenceBuildPosition.TypeName == replaceId)) &&
                      component.ReferenceBuildPosition.AdjustedInstance >
                      position.AdjustedInstance)
                        component.ReferenceBuildPosition.AdjustedInstance--;
                  }
                  else
                      if(((!String.IsNullOrEmpty(component.ConceptualBuildPosition.Id) &&
                        component.ConceptualBuildPosition.Id == replaceId) ||
                        (String.IsNullOrEmpty(component.ConceptualBuildPosition.Id) &&
                        component.ConceptualBuildPosition.TypeName == replaceId)) &&
                        component.ConceptualBuildPosition.AdjustedInstance >
                        position.AdjustedInstance)
                          component.ConceptualBuildPosition.AdjustedInstance--;

                return;
            }

            // See if the configuration already exists.  If so, replace it.
            // We'll assume it's already in the correct location.
            node = rootNode.SelectSingleNode("component[@id='" + id + "']");

            if(node != null)
            {
                this.ReportProgress("    Replacing default configuration for '{0}' with the custom configuration", id);
                rootNode.ReplaceChild(configNode, node);
                return;
            }

            // Create the node and add it in the correct location
            switch(position.Place)
            {
                case ComponentPosition.Placement.Start:
                    rootNode.InsertBefore(configNode, rootNode.ChildNodes[0]);
                    this.ReportProgress("    Added configuration for '{0}' to the start of the configuration file", id);
                    break;

                case ComponentPosition.Placement.End:
                    rootNode.InsertAfter(configNode,
                        rootNode.ChildNodes[rootNode.ChildNodes.Count - 1]);
                    this.ReportProgress("    Added configuration for '{0}' to the end of the configuration file", id);
                    break;

                case ComponentPosition.Placement.Before:
                    if(matchingNodes.Count < position.AdjustedInstance)
                        this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to add " +
                            "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);
                    else
                    {
                        rootNode.InsertBefore(configNode, matchingNodes[position.AdjustedInstance - 1]);
                        this.ReportProgress("    Added configuration for '{0}' before '{1}' (instance {2})",
                            id, replaceId, position.AdjustedInstance);
                    }
                    break;

                default:    // After
                    if(matchingNodes.Count < position.AdjustedInstance)
                        this.ReportProgress("    Could not find configuration '{0}' (instance {1}) to add " +
                            "configuration for '{2}' so it will be omitted.", replaceId, position.AdjustedInstance, id);
                    else
                    {
                        rootNode.InsertAfter(configNode, matchingNodes[position.AdjustedInstance - 1]);
                        this.ReportProgress("    Added configuration for '{0}' after '{1}' (instance {2})",
                            id, replaceId, position.AdjustedInstance);
                    }
                    break;
            }
        }
    }
}
