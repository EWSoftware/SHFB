using System.Diagnostics;
using System.Reflection;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

// Search for "TODO" to find changes that you need to make to this build component template.

namespace $safeprojectname$
{
    /// <summary>
    /// TODO: Set your build component's unique ID and description in the export attribute in the factory class
    /// below.
    /// </summary>
    /// <remarks>The <c>BuildComponentExportAttribute</c> is used to export your component so that the help
    /// file builder finds it and can make use of it.  The example below shows the basic usage for a common
    /// build component.  Multiple copies of build components can be created depending on their usage.  The
    /// host process will create instances as needed and will dispose of them when it is done with them.</remarks>
    public class $safeprojectname$Component : BuildComponentCore
    {
        #region Build component factory for MEF
        //=====================================================================

        /// <summary>
        /// This is used to create a new instance of the build component
        /// </summary>
        /// <remarks>TODO: The <c>IsVisible</c> property is typically set to true so that the component can be
        /// exposed in configuration tools such as the Sandcastle Help File Builder.  If set to false, the
        /// component will be hidden but can be used if referenced in a configuration file or as a dependency.</remarks>
        [BuildComponentExport("$safeprojectname$", IsVisible = true, Version = AssemblyInfo.ProductVersion,
          Copyright = AssemblyInfo.Copyright, Description = "$safeprojectname$ build component")]
        public sealed class Factory : BuildComponentFactory
        {
            /// <summary>
            /// Constructor
            /// </summary>
            public Factory()
            {
                // Build placement tells tools such as the Sandcastle Help File Builder how to insert the
                // component into build configurations in projects to which it is added.

                // TODO: Set placement for reference builds or remove if not used in reference builds
                this.ReferenceBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");

                // TODO: Set placement for conceptual builds or remove if not used in conceptual builds
                this.ConceptualBuildPlacement = new ComponentPlacement(PlacementAction.Before,
                    "XSL Transform Component");
            }

            /// <inheritdoc />
            public override BuildComponentCore Create()
            {
                return new $safeprojectname$Component(this.BuildAssembler);
            }

            /// <inheritdoc />
            public override string DefaultConfiguration => @"<!-- TODO: Define your build component's default configuration here -->
<itemOne value=""Test #1"" />
<itemTwo value=""Test #2"" />";

        }
        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="buildAssembler">A reference to the build assembler</param>
        protected $safeprojectname$Component(IBuildAssembler buildAssembler) : base(buildAssembler)
        {
        }
        #endregion

        #region Abstract method implementations
        //=====================================================================

        /// <summary>
        /// Initialize the build component
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        public override void Initialize(XPathNavigator configuration)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);

            this.WriteMessage(MessageLevel.Info, "[{0}, version {1}]\r\n    $safeprojectname$ Component.  {2}",
                fvi.ProductName, fvi.ProductVersion, fvi.LegalCopyright);

            // TODO: Add your build component initialization code here
        }

        /// <summary>
        /// Apply this build component's changes to the document
        /// </summary>
        /// <param name="document">The document to modify</param>
        /// <param name="key">The document's key</param>
        public override void Apply(XmlDocument document, string key)
        {
            // TODO: Add your document modification code here.

            this.WriteMessage(MessageLevel.Diagnostic, "In $safeprojectname$Component Apply() method");
        }
        #endregion
    }
}
