// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;


namespace Microsoft.Ddue.Tools {

    public enum CodeReferenceType {
        Invalid,    // not initialized, invalid reference string
        Snippet,    // MSDN style snippet for all build targets
        Msdn,       // MSDN style snippet for MSDN build only
        Run,        // runnable code sample with pop-up source browser for WebDocs build
        View        // code sample shown in pop-up source browser for WebDocs build
    }

    /// <summary>
    /// The CodeReference class encapsulates DDUE code reference elements and provides easy access
    /// to individual code reference parts, such as the type, name, title, etc..
    /// </summary>
    /// <remarks>
    /// Examples of valid code reference strings include:
    ///   - SampleName#SnippetNumber
    ///   - SampleName#SnippetNumber1,SnippetNumber2,SnippetNumber3
    ///   - msdn:SampleName#SnippetNumber
    ///   - run:SampleName
    ///   - run:SampleName;title text
    ///   - run:SampleName#startPage.aspx
    ///   - run:SampleName/path/to/startPage.aspx
    ///   - run:SampleName#startPage.aspx;title text
    ///   - run:SampleName/path/to/startPage.aspx;title text
    ///   - view:SampleName
    ///   - view:SampleName#defaultFile
    ///   - view:SampleName/path/to/defaultFile
    ///   - view:SampleName#defaultFile;title text
    ///   - view:SampleName/path/to/defaultFile;title text
    /// </remarks>
    public class CodeReference {

        string _ddueCodeReference;
        public string DdueCodeReference {
            get { return _ddueCodeReference; }
        }

        CodeReferenceType _type;
        public CodeReferenceType Type {
            get { return _type; }
        }

        string _exampleName;
        public string ExampleName {
            get { return _exampleName; }
        }

        string _examplePath;
        public string ExamplePath {
            get { return _examplePath; }
        }

        string _snippetId;
        public string SnippetId {
            get { return _snippetId; }
        }

        string _title;
        public string Title {
            get { return _title; }
        }

        public CodeReference(string ddueCodeReference) {
            _ddueCodeReference = ddueCodeReference;
            Parse();
        }

        static Regex codeReferenceRx = new Regex(
            @"^\s*(" +
                @"((?<type>msdn|run|view):)?" +
                @"(?<examplePath>(" +
                    @"(?<exampleName>[\w\.,\-]+)" +
                    @"((#(?<snippetId>[\w,]+))|(([/\\#,\.\w\-]+)?))" +
                @"))" +
                @"(;(?<title>.*))?" +
            @")\s*$",
            RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Compiled);

        void Parse() {
            Match m = codeReferenceRx.Match(DdueCodeReference);
            if (m.Success) {
                _exampleName = m.Groups["exampleName"].Value;
                _snippetId = m.Groups["snippetId"].Value;
                _examplePath = m.Groups["examplePath"].Value;
                _title = m.Groups["title"].Value;
                // The default value of _type is CodeReferenceType.Invalid, if it isn't set in the following
                // block.
                if (m.Groups["type"].Length > 0) {
                    try {
                        _type = (CodeReferenceType)Enum.Parse(typeof(CodeReferenceType), m.Groups["type"].Value, true);
                    }
                    catch (ArgumentException) {
                        // _type = CodeReferenceType.Invalid
                    }
                }
                else if (m.Groups["exampleName"].Length > 0 && m.Groups["snippetId"].Length > 0) {
                    _type = CodeReferenceType.Snippet;
                }
            }
        }

        public override string ToString() {
            return DdueCodeReference;
        }
    }
}