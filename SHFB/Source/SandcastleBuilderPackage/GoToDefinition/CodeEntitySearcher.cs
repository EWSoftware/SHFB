//===============================================================================================================
// System  : Sandcastle Help File Builder Visual Studio Package
// File    : CodeEntitySearcher.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/26/2021
// Note    : Copyright 2014-2021, Eric Woodruff, All rights reserved
//
// This file contains the class used to search for and go to code entity declarations by member ID
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
//===============================================================================================================
// 12/06/2014  EFW  Created the code
//===============================================================================================================

// Ignore Spelling: bool uint ushort ulong

using System;
using System.Collections.Generic;
using System.Linq;

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio;

namespace SandcastleBuilder.Package.GoToDefinition
{
    /// <summary>
    /// This class is used to search for and go to code entity declarations by member ID
    /// </summary>
    /// <remarks>Only the C# library is supported for now due to problems with the VB library.  Using the VB
    /// library GUID causes the search to crash Visual Studio completely.  There doesn't appear to be a GUID for
    /// finding stuff in the base framework.</remarks>
    internal class CodeEntitySearcher
    {
        #region Private data members
        //=====================================================================

        private readonly SVsServiceProvider serviceProvider;
        private IVsObjectManager2 objectManager;
        private IVsLibrary2 library;
        private _LIB_LISTTYPE searchFlags;
        private string alternateCompareText;
        private readonly List<string> typeParameters, methodParameters;
        private List<string> searchClassCandidates, searchMemberCandidates;

        #endregion

        #region Search results class
        //=====================================================================

        /// <summary>
        /// This is used to interact with the search results
        /// </summary>
        private class SearchResult
        {
            private readonly IVsObjectList2 list;
            private readonly uint idx;
            private string fullName;

            /// <summary>
            /// This returns the full name of the member
            /// </summary>
            public string FullName
            {
                get
                {
                    ThreadHelper.ThrowIfNotOnUIThread();

                    if(fullName == null)
                    {

                        if(list.GetProperty(idx, (int)_VSOBJLISTELEMPROPID.VSOBJLISTELEMPROPID_FULLNAME,
                          out object obj) == VSConstants.S_OK)
                        {
                            fullName = (string)obj;
                        }
                    }

                    return fullName;
                }
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="list"></param>
            /// <param name="idx"></param>
            public SearchResult(IVsObjectList2 list, uint idx)
            {
                this.list = list;
                this.idx = idx;
            }

            /// <summary>
            /// Go to the source for the given member
            /// </summary>
            /// <returns>True if successful, false if not</returns>
            public bool GoToSource()
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                return (list.GoToSource(idx, VSOBJGOTOSRCTYPE.GS_DEFINITION) == VSConstants.S_OK);
            }
        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serviceProvider">The service provider used to get the object manager</param>
        public CodeEntitySearcher(SVsServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.typeParameters = new List<string>();
            this.methodParameters = new List<string>();
            this.searchClassCandidates = new List<string>();
            this.searchMemberCandidates = new List<string>();
        }
        #endregion

        #region Methods
        //=====================================================================

        /// <summary>
        /// Search for and go to the definition of the given member ID
        /// </summary>
        /// <param name="memberId">The member ID for which to search</param>
        /// <returns>True if successful, false if not</returns>
        /// <remarks>We cannot guarantee any particular order for the search results so it will go to the first
        /// match it finds which may or may not be the one you are expecting.  Results are more exact when the
        /// member ID is more fully qualified.  For example, if using <c>ToString</c> you may not end up in the
        /// class you were expecting.  Using a more qualified ID such as <c>MyClass.ToString</c> will get a
        /// better result unless you have two classes by the same name in different namespaces or if the member
        /// is overloaded.</remarks>
        public bool GotoDefinitionFor(string memberId)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if(String.IsNullOrWhiteSpace(memberId) || !this.DetermineSearchCriteria(memberId))
                    return false;

                if(objectManager == null)
                {
                    objectManager = serviceProvider.GetService(typeof(SVsObjectManager)) as IVsObjectManager2;

                    if(objectManager == null)
                        return false;
                }

                // As noted, we only support the C# library
                if(library == null)
                    if(objectManager.FindLibrary(new Guid(BrowseLibraryGuids80.CSharp), out library) != VSConstants.S_OK)
                        return false;

                var criteria = new VSOBSEARCHCRITERIA2
                {
                    eSrchType = VSOBSEARCHTYPE.SO_SUBSTRING,
                    grfOptions = (uint)_VSOBSEARCHOPTIONS.VSOBSO_NONE
                };

                // Give precedence to classes by searching for them first if wanted
                if((searchFlags & _LIB_LISTTYPE.LLT_CLASSES) != 0)
                    foreach(string searchText in searchClassCandidates)
                    {
                        criteria.szName = searchText;

                        foreach(var r in this.PerformSearch(_LIB_LISTTYPE.LLT_CLASSES, criteria))
                            if(this.IsMatch(searchText, r, false))
                                return r.GoToSource();
                    }

                // Search for members if wanted
                if((searchFlags & _LIB_LISTTYPE.LLT_MEMBERS) != 0)
                    foreach(string searchText in searchMemberCandidates)
                    {
                        criteria.szName = searchText;

                        var results = new List<SearchResult>(this.PerformSearch(_LIB_LISTTYPE.LLT_MEMBERS, criteria));

                        // Try for a match including parameters first
                        foreach(var r in results)
                            if(this.IsMatch(searchText, r, true))
                                return r.GoToSource();

                        // Try for a match without parameters if nothing was found
                        foreach(var r in results)
                            if(this.IsMatch(searchText, r, false))
                                return r.GoToSource();
                    }
            }
            catch(Exception ex)
            {
                // Ignore exceptions, we'll just fail the search
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return false;
        }

        /// <summary>
        /// Determine the search criteria by parsing the member ID as best we can
        /// </summary>
        /// <param name="memberId">The member ID to parse</param>
        /// <returns>True if parsed successfully, false if not</returns>
        private bool DetermineSearchCriteria(string memberId)
        {
            string[] parts;
            string searchText, paramText;

            searchFlags = _LIB_LISTTYPE.LLT_CLASSES | _LIB_LISTTYPE.LLT_MEMBERS;
            searchText = memberId.Trim().Replace("{", "<").Replace("}", ">");
            typeParameters.Clear();
            methodParameters.Clear();
            searchClassCandidates.Clear();
            searchMemberCandidates.Clear();
            alternateCompareText = null;

            if(searchText.IsCodeEntityReference())
            {
                // We can't search for namespaces and namespace groups since they don't exist as something we can
                // go to in the code.  As such, convert them to NamespaceDoc and NamespaceGroupDoc type searches.
                if(searchText[0] == 'N' || searchText[0] == 'G')
                {
                    searchFlags = _LIB_LISTTYPE.LLT_CLASSES;
                    searchText = searchText.Substring(2) + ((searchText[0] == 'N') ? ".NamespaceDoc" :
                        ".NamespaceGroupDoc");
                }
                else
                {
                    // Limit to classes or members for the results.  For member searches, there doesn't appear to
                    // be a way to further qualify it to limit it to specific member types such as just fields,
                    // events, properties, or methods so we may get all member types in the results.
                    if(searchText[0] == 'T')
                        searchFlags = _LIB_LISTTYPE.LLT_CLASSES;
                    else
                        searchFlags = _LIB_LISTTYPE.LLT_MEMBERS;

                    searchText = searchText.Substring(2);
                }
            }

            // If parentheses are found, only search for members.  Drop the parameters because we can't guarantee
            // their format in the search string.  We will keep them for the match method though and compare them
            // as best we can.
            int pos = searchText.IndexOf('(');

            if(pos != -1)
            {
                searchFlags = _LIB_LISTTYPE.LLT_MEMBERS;
                paramText = searchText.Substring(pos + 1);
                searchText = searchText.Substring(0, pos);

                // For operators, the parameters are followed by a return type so the parenthesis may not be the
                // last character in the string.
                pos = paramText.IndexOf(')');

                if(pos != -1)
                    paramText = paramText.Substring(0, pos);

                methodParameters.AddRange(this.ParseParameters(paramText));
            }

            // If type parameters are present, strip them off too but keep them for the match method where we'll
            // compare them as best we can.  However, don't strip them if they are followed by a member name.
            // We'll have to rely on them matching the base type's type parameter names.
            pos = searchText.LastIndexOf('.');

            if(pos == -1)
                pos = searchText.IndexOf('<');
            else
                pos = searchText.IndexOf('<', pos);

            if(pos != -1)
            {
                paramText = searchText.Substring(pos + 1);

                if(paramText.Length != 0 && paramText[paramText.Length - 1] == '>')
                    paramText = paramText.Substring(0, paramText.Length - 1);

                searchText = searchText.Substring(0, pos);
                typeParameters.AddRange(this.ParseParameters(paramText));
            }

            if((searchFlags & _LIB_LISTTYPE.LLT_MEMBERS) != 0)
            {
                // Convert constructor references to their type name
                if(searchText.EndsWith("#ctor", StringComparison.Ordinal) ||
                  searchText.EndsWith("#cctor", StringComparison.Ordinal))
                {
                    parts = searchText.Split('.');

                    if(parts.Length > 1)
                        searchText = String.Join(".", parts, 0, parts.Length - 1) + "." + parts[parts.Length - 2];
                    else
                        searchText = parts[0];
                }

                // Convert finalizers to their type name with prefix
                if((searchText == "Finalize" || searchText.EndsWith(".Finalize", StringComparison.Ordinal)) &&
                  methodParameters.Count == 0)
                {
                    parts = searchText.Split('.');

                    if(parts.Length > 1)
                        searchText = String.Join(".", parts, 0, parts.Length - 1) + "." + "~" + parts[parts.Length - 2];
                    else
                        searchText = "~" + parts[0];
                }

                // Explicit implementation class name prefix?  This one's a bit tricky.  We need to search for the
                // name with the EII prefix replacing "#" with "." but compare the results without the prefix.
                if(searchText.IndexOf('#') != -1)
                {
                    parts = searchText.Split('#');

                    alternateCompareText = parts[parts.Length - 1];
                    parts = parts[0].Split('.');

                    if(parts.Length > 1)
                        alternateCompareText = String.Join(".", parts, 0, parts.Length - 1) + "." + alternateCompareText;

                    searchText = searchText.Replace("#", ".");
                }

                // Convert internal operator names to their code equivalent
                if(searchText.StartsWith("op_", StringComparison.Ordinal) ||
                  searchText.IndexOf(".op_", StringComparison.Ordinal) != -1)
                {
                    parts = searchText.Split('.');
                    paramText = parts[parts.Length - 1].ToCodeOperator() ?? "operator ????";

                    if(parts.Length < 2)
                        searchText = paramText;
                    else
                        searchText = String.Join(".", parts, 0, parts.Length - 1) + "." + paramText;

                    // The names for implicit and explicit operators are reversed in the search text
                    if(searchText.EndsWith("implicit", StringComparison.Ordinal))
                    {
                        if(parts.Length < 2)
                            alternateCompareText = "implicit operator";
                        else
                            alternateCompareText = String.Join(".", parts, 0, parts.Length - 1) + ".implicit operator";
                    }

                    if(searchText.EndsWith("explicit", StringComparison.Ordinal))
                    {
                        if(parts.Length < 2)
                            alternateCompareText = "explicit operator";
                        else
                            alternateCompareText = String.Join(".", parts, 0, parts.Length - 1) + ".explicit operator";
                    }

                    // Another odd quirk.  The "*" used for the multiply operator appears to be treated as a
                    // wildcard but only when specified by itself.  Any other search including "*" returns
                    // nothing as does search for "operator" with a class prefix.  "*" by itself returns
                    // everything.  The only thing that seems to work is to search for "operator" by itself and
                    // sort it out when comparing results.
                    if(searchText.EndsWith("operator *", StringComparison.Ordinal))
                    {
                        alternateCompareText = searchText;
                        searchText = "operator";
                    }
                }
            }

#pragma warning disable VSTHRD010
            this.DetermineSearchCandidates(searchText);
#pragma warning restore VSTHRD010

            // Add the given search text as the last resort search
            if((searchFlags & _LIB_LISTTYPE.LLT_CLASSES) != 0)
                searchClassCandidates.Add(searchText);

            if((searchFlags & _LIB_LISTTYPE.LLT_MEMBERS) != 0)
                searchMemberCandidates.Add(searchText);

            return true;
        }

        /// <summary>
        /// If in a C# code file, determine the most likely search candidates based on the containing class,
        /// containing namespaces, and imported namespaces.
        /// </summary>
        /// <param name="searchText">The search text</param>
        /// <remarks>This helps further qualify reference targets that are not fully qualified in order to find
        /// a more exact and, hopefully, better match.  Without this, we tend to end up in unrelated classes and
        /// sometimes different projects due to the lack of qualification in most XML comments target IDs.</remarks>
        private void DetermineSearchCandidates(string searchText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> usingNamespaces = new List<string>(), containingNamespaces = new List<string>();
            List<CodeClass> classes = new List<CodeClass>();
            int searchLine = 0, firstCodeLine = -1;


            if(this.serviceProvider.GetService(typeof(SDTE)) is DTE2 dte2 && dte2.ActiveDocument != null &&
              dte2.ActiveDocument.ProjectItem != null && dte2.ActiveDocument.ProjectItem.FileCodeModel != null)
            {
                var fileCodeModel = dte2.ActiveDocument.ProjectItem.FileCodeModel;

                if(dte2.ActiveDocument.Selection is TextSelection selection)
                    searchLine = selection.ActivePoint.Line;

                // Get all using namespaces, containing namespaces, and classes in the current file
                foreach(CodeElement ce in fileCodeModel.CodeElements)
                    if(ce.Kind == vsCMElement.vsCMElementImportStmt)
                    {
                        usingNamespaces.Add(((CodeImport)ce).Namespace);
                        firstCodeLine = ce.EndPoint.Line + 1;
                    }
                    else
                        if(ce.Kind == vsCMElement.vsCMElementClass)
                    {
                        // A class in the global namespace
                        classes.Add((CodeClass)ce);

                        if(firstCodeLine == -1)
                            firstCodeLine = 1;
                    }
                    else
                            if(ce.Kind == vsCMElement.vsCMElementNamespace)
                    {
                        // A class within a namespace.  Typically there will only be one per file but
                        // we'll account for multiples.
                        containingNamespaces.Add(((CodeNamespace)ce).Name);

                        if(firstCodeLine == -1)
                            firstCodeLine = ce.EndPoint.Line + 1;

                        foreach(CodeElement nce in ce.Children)
                            if(nce.Kind == vsCMElement.vsCMElementClass)
                                classes.Add((CodeClass)nce);
                    }

                // Remove stuff that looks like core framework namespaces unless any containing namespaces
                // include similar names.
                if(!containingNamespaces.Any(cn => cn == "System" ||
                  cn.StartsWith("System.", StringComparison.Ordinal) ||
                  cn.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                  cn.StartsWith("EnvDTE", StringComparison.Ordinal)))
                {
                    usingNamespaces = usingNamespaces.Where(un => un != "System" &&
                        !un.StartsWith("System.", StringComparison.Ordinal) &&
                        !un.StartsWith("Microsoft.", StringComparison.Ordinal) &&
                        !un.StartsWith("EnvDTE", StringComparison.Ordinal)).ToList();
                }

                // If searching for classes, qualify the search text based on the containing and imported
                // namespaces.  These will be searched for first.
                if((searchFlags & _LIB_LISTTYPE.LLT_CLASSES) != 0)
                {
                    int pos = searchText.LastIndexOf('.');

                    if(pos != -1 && searchText.Substring(0, pos).IndexOf('<') != -1)
                        pos = searchText.Substring(0, pos).LastIndexOf('.');

                    if(pos != -1)
                    {
                        string searchNS = searchText.Substring(0, pos);

                        string match = containingNamespaces.FirstOrDefault(cn => cn.EndsWith(searchNS, StringComparison.Ordinal));

                        if(match == null)
                            match = usingNamespaces.FirstOrDefault(un => un.EndsWith(searchNS, StringComparison.Ordinal));

                        if(match != null)
                            searchClassCandidates.Add(match + searchText.Substring(pos));
                        else
                            pos = -1;
                    }

                    if(pos == -1)
                    {
                        // Add each containing namespace and its parent namespaces
                        foreach(string cn in containingNamespaces)
                        {
                            searchClassCandidates.Add(cn + "." + searchText);

                            var parts = cn.Split('.');
                            int count = parts.Length - 1;

                            while(count > 0)
                            {
                                searchClassCandidates.Add(String.Join(".", parts, 0, count) + "." + searchText);
                                count--;
                            }
                        }

                        searchClassCandidates.AddRange(usingNamespaces.Select(un => un + "." + searchText));
                    }
                }

                if(searchLine >= firstCodeLine)
                {
                    // Figure out the class for the current location.  It or one of its base classes will be the
                    // highest probability for a match.
#pragma warning disable VSTHRD010
                    var containingClass = classes.FirstOrDefault(c => searchLine >= c.StartPoint.Line &&
                        searchLine <= c.EndPoint.Line);

                    if(containingClass == null)
                        containingClass = classes.FirstOrDefault(c => c.EndPoint.Line > searchLine);
#pragma warning restore VSTHRD010

                    if(containingClass != null)
                    {
                        searchMemberCandidates.Add(containingClass.FullName + "." + searchText);

                        // If qualified, search for it in any parent namespaces too
                        if(searchText.IndexOf('.') != -1)
                        {
                            var parts = containingClass.FullName.Split('.');
                            int count = parts.Length - 1;

                            while(count > 0)
                            {
                                searchMemberCandidates.Add(String.Join(".", parts, 0, count) + "." + searchText);
                                count--;
                            }
                        }
                    }

                    // Add candidates in containing or imported namespaces if it looks like a class
                    if(searchText.IndexOf('.') != -1)
                        foreach(string ns in containingNamespaces.Concat(usingNamespaces))
                            searchMemberCandidates.Add(ns + "." + searchText);
                }

                // Get rid of duplicates
                searchClassCandidates = searchClassCandidates.Distinct().ToList();
                searchMemberCandidates = searchMemberCandidates.Distinct().ToList();
            }
        }

        /// <summary>
        /// This is used to parse a set of parameter types in string form into individual parameters
        /// </summary>
        /// <param name="parameters">The parameter type string parse</param>
        /// <returns>An enumerable list of parameter types from the string.  Generics are converted to the type
        /// name followed by "`" and the type parameter count (i.e. <c>KeyValuePair`2</c>).  This is necessary as
        /// entity reference string will use the type parameter names which may not match the member declaration
        /// and the found member names may contain concrete types which may not match the search text.  Using
        /// the count is a compromise to get as close as we can to the desired member.</returns>
        private IEnumerable<string> ParseParameters(string parameters)
        {
            if(!String.IsNullOrWhiteSpace(parameters))
            {
                int start = 0, nestedGeneric = 0, genericStart = 0, genericParamCount = 0;
                bool inGeneric = false;
                string paramName;

                for(int idx = 0; idx < parameters.Length; idx++)
                {
                    if(parameters[idx] == ',' || parameters[idx] == ' ')
                    {
                        if(inGeneric)
                        {
                            if(parameters[idx] == ',')
                                genericParamCount++;
                        }
                        else
                        {
                            paramName = parameters.Substring(start, idx - start).Trim();

                            // Ignore "ref" and "out" modifiers and empty names
                            if(paramName.Length != 0 && paramName != "ref" && paramName != "out")
                                yield return paramName;

                            start = idx + 1;
                        }
                    }
                    else
                        if(parameters[idx] == '<')
                        {
                            if(!inGeneric)
                            {
                                inGeneric = true;
                                genericStart = idx;
                                genericParamCount = 1;
                                nestedGeneric++;
                            }
                            else
                                nestedGeneric++;
                        }
                        else
                            if(parameters[idx] == '>')
                            {
                                nestedGeneric--;

                                if(nestedGeneric == 0)
                                {
                                    inGeneric = false;
                                    paramName = parameters.Substring(start, genericStart - start).Trim();

                                    if(paramName.Length != 0)
                                        yield return paramName + '`' + genericParamCount.ToString();

                                    start = idx + 1;
                                }
                            }
                }

                if(start < parameters.Length)
                {
                    paramName = parameters.Substring(start, parameters.Length - start).Trim();

                    if(paramName.Length != 0 && paramName != "ref" && paramName != "out")
                        yield return paramName;
                }
            }
        }

        /// <summary>
        /// Perform the search using the given criteria
        /// </summary>
        /// <param name="listType">The list type to return (classes or members)</param>
        /// <param name="criteria">The criteria to use for the search</param>
        /// <returns>An enumerable list of <see cref="SearchResult"/> instances that contain information about
        /// each potential match.</returns>
        private IEnumerable<SearchResult> PerformSearch(_LIB_LISTTYPE listType, VSOBSEARCHCRITERIA2 criteria)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int result = library.GetList2((uint)listType, (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER,
                new[] { criteria }, out IVsObjectList2 list);

            if(result == VSConstants.S_OK && list != null)
            {
                result = list.GetItemCount(out uint count);

                if(result == VSConstants.S_OK && count != 0)
                    for(uint idx = 0; idx < count; idx++)
                    {
                        result = list.CanGoToSource(idx, VSOBJGOTOSRCTYPE.GS_DEFINITION, out int pfOK);

                        // Ignore anything for which we can't go to the source
                        if(result == VSConstants.S_OK && pfOK == 1)
                            yield return new SearchResult(list, idx);
                    }
            }
        }

        /// <summary>
        /// Determine if the search result is a match to the search text
        /// </summary>
        /// <param name="searchText">The text to use for comparison.  This will be ignored if alternate
        /// comparison text was set while determining the search criteria.</param>
        /// <param name="result">The result to compare.</param>
        /// <param name="matchParameters">True to match parameters for methods, false to ignore them.</param>
        /// <returns>True if it is a match, false if not</returns>
        private bool IsMatch(string searchText, SearchResult result, bool matchParameters)
        {
            List<string> methodParams = new List<string>(), typeParams = new List<string>();
#pragma warning disable VSTHRD010
            string name = result.FullName, compareText = (alternateCompareText ?? searchText), paramText, match;
#pragma warning restore VSTHRD010

            // Strip off method parameters.  We'll compare these separately.
            int pos = name.IndexOf('(');

            if(pos != -1)
            {
                paramText = name.Substring(pos + 1);

                if(paramText.Length != 0 && paramText[paramText.Length - 1] == ')')
                    paramText = paramText.Substring(0, paramText.Length - 1);

                methodParams = this.ParseParameters(paramText).ToList();

                name = name.Substring(0, pos).Trim();
                pos = name.LastIndexOf(' ');

                // Explicit and implicit operators include the return type after the name and before the
                // parameters, so drop it. Don't drop the trailing text for the true/false operators.
                if(pos != -1 && Char.IsLetter(name[pos + 1]) && name.Substring(pos) != " true" &&
                  name.Substring(pos) != " false") 
                    name = name.Substring(0, pos);
            }

            // Strip off type parameters.  We'll compare these separately.  Don't strip if followed by a member
            // name or if it's a left shift operator.
            pos = name.LastIndexOf('.');

            if(pos == -1)
                pos = name.IndexOf('<');
            else
                pos = name.IndexOf('<', pos);

            if(pos != -1 && pos < name.Length - 2)
            {
                paramText = name.Substring(pos + 1);

                if(paramText.Length != 0 && paramText[paramText.Length - 1] == '>')
                    paramText = paramText.Substring(0, paramText.Length - 1);

                typeParams = this.ParseParameters(paramText).ToList();
                name = name.Substring(0, pos);
            }

            // If the result contains a namespace or class, add a leading period so that we don't match partial
            // text at the end of a name (i.e. we don't want String to match ToString).
            if(compareText.IndexOf('.') == -1 && name.IndexOf('.') != -1)
                compareText = "." + compareText;

            if(name.EndsWith(compareText, StringComparison.Ordinal))
            {
                if(!matchParameters)
                    return true;

                if(typeParameters.Count != typeParams.Count)
                    return false;

                // Assume type parameters will match exactly
                for(int idx = 0; idx < typeParameters.Count; idx++)
                    if(typeParams[idx] != typeParameters[idx])
                        return false;
               
                if(methodParameters.Count == methodParams.Count)
                {
                    // Since we can't guarantee the format of the parameters in the search text, we'll make an
                    // assumption that if we can match it anywhere in the parameter name it's a good match.  For
                    // example, "bool" and "Boolean" will match "System.Boolean".  There is always the chance of
                    // a mismatch in some cases but we'll have to live with that.
                    for(int idx = 0; idx < methodParameters.Count; idx++)
                    {
                        match = methodParameters[idx];

                        bool isArray = match.EndsWith("[]", StringComparison.Ordinal);

                        if(isArray)
                            match = match.Substring(0, match.Length - 2);

                        // Handle a few common types that need special attention
                        switch(match)
                        {
                            case "int":
                            case "uint":
                                match += "32";
                                break;

                            case "short":
                                match = "Int16";
                                break;

                            case "ushort":
                                match = "UInt16";
                                break;

                            case "long":
                                match = "Int64";
                                break;

                            case "ulong":
                                match = "UInt64";
                                break;

                            case "float":
                                match = "Single";
                                break;

                            default:
                                break;
                        }

                        if(isArray)
                            match += "[]";

                        if(match.IndexOf('.') == -1 && methodParams[idx].IndexOf('.') != -1)
                            match = "." + match;

                        if(methodParams[idx].IndexOf(match, StringComparison.OrdinalIgnoreCase) == -1)
                            return false;
                    }

                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
