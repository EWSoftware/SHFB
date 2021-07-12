//===============================================================================================================
// System  : Code Colorizer Library
// File    : CodeColorizer.cs
// Author  : Eric Woodruff
// Updated : 04/06/2021
//
// This contains an enumerated type that defines the output format
//
//===============================================================================================================

namespace ColorizerLibrary
{
    /// <summary>
    /// This enumerated type defines the output format of the associated XSL transformation file used to
    /// colorize the text.
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>HTML</summary>
        Html,
        /// <summary>XAML flow document</summary>
        FlowDocument
    }
}
