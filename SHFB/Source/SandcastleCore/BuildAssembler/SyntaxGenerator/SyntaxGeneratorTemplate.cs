// Copyright © Microsoft Corporation.
// This source file is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

// Change history:
// 02/09/2012 - EFW - Added XPath expressions for optional parameter argument element and property
// getter and setter to list their attributes.
// 02/14/2012 - EFW - Added XPath expressions for fixed keyword support
// 12/23/2012 - EFW - Removed DeclarationSyntaxGeneratorTemplate as it was identical to SyntaxGeneratorTemplate
// with the exception of a couple of unused static methods and an abstract WriteVisibility() method.  It was
// only used by the JSharpDeclarationSyntaxGenerator which has been changed to use SyntaxGeneratorTemplate as
// its base class.
// 11/29/2013 - EFW - Added support for interop metadata
// 12/21/2013 - EFW - Moved class to Sandcastle.Core assembly and updated for use via MEF
// 10/08/2015 - EFW - Added support for writing out the value of constant fields

using System;
using System.Globalization;
using System.Xml.XPath;

namespace Sandcastle.Core.BuildAssembler.SyntaxGenerator
{
    /// <summary>
    /// This abstract class is used as the base class for syntax generators
    /// </summary>
    public abstract class SyntaxGeneratorTemplate : SyntaxGeneratorCore
    {
        #region Constants
        //=====================================================================

        /// <summary>
        /// The maximum line width for the generated syntax
        /// </summary>
        protected const int MaxPosition = 60;

        #endregion

        #region Shared XPath expressions
        //=====================================================================

// I can't be bothered to document all these right now so just ignore the warnings
#pragma warning disable 1591
        // Where data is stored

        // API data
        protected static XPathExpression apiNameExpression = XPathExpression.Compile("string(apidata/@name)");
        protected static XPathExpression apiGroupExpression = XPathExpression.Compile("string(apidata/@group)");
        protected static XPathExpression apiSubgroupExpression = XPathExpression.Compile("string(apidata/@subgroup)");
        protected static XPathExpression apiSubsubgroupExpression = XPathExpression.Compile("string(apidata/@subsubgroup)");

        // support testing
        // !EFW - Added support for "fixed" keyword check which is also unsafe
        protected static XPathExpression apiIsUnsafeExpression = XPathExpression.Compile(
            "boolean(parameters/parameter//pointerTo or returns//pointerTo or " +
            "attributes/attribute[type/@api='T:System.Runtime.CompilerServices.FixedBufferAttribute'])");

        protected static XPathExpression apiIsGenericExpression = XPathExpression.Compile("boolean(templates/template)");
        protected static XPathExpression apiIsExtensionMethod = XPathExpression.Compile("boolean(attributes/attribute/type[@api='T:System.Runtime.CompilerServices.ExtensionAttribute'])");

        // visibility attribute
        protected static XPathExpression apiVisibilityExpression = XPathExpression.Compile("string((typedata|memberdata)/@visibility)");
        protected static XPathExpression apiIsFamilyMemberExpression = XPathExpression.Compile("boolean(contains(memberdata/@visibility,'family'))");

        // type data
        protected static XPathExpression apiVisibilityOfTypeExpression = XPathExpression.Compile("string(typedata/@visibility)");
        protected static XPathExpression apiIsAbstractTypeExpression = XPathExpression.Compile("boolean(typedata[@abstract='true'])");
        protected static XPathExpression apiIsSealedTypeExpression = XPathExpression.Compile("boolean(typedata[@sealed='true'])");
        protected static XPathExpression apiIsSerializableTypeExpression = XPathExpression.Compile("boolean(typedata[@serializable='true'])");
        // !EFW - Added support for interop metadata
        protected static XPathExpression apiComImportTypeExpression = XPathExpression.Compile("boolean(typedata[@comimport='true'])");
        protected static XPathExpression apiStructLayoutTypeExpression = XPathExpression.Compile("string(typedata/@layout)");
        protected static XPathExpression apiStructLayoutSizeTypeExpression = XPathExpression.Compile("number(typedata/@size)");
        protected static XPathExpression apiStructLayoutPackTypeExpression = XPathExpression.Compile("number(typedata/@pack)");
        protected static XPathExpression apiStructLayoutFormatTypeExpression = XPathExpression.Compile("string(typedata/@format)");

        // class data
        protected static XPathExpression apiBaseClassExpression = XPathExpression.Compile("family/ancestors/*[1]");
        protected static XPathExpression apiAncestorsExpression = XPathExpression.Compile("family/ancestors/*");

        // enumeration data

        // various subheadings 
        protected static XPathExpression apiAttributesExpression = XPathExpression.Compile("attributes/attribute");
        protected static XPathExpression apiTemplatesExpression = XPathExpression.Compile("templates/template");
        protected static XPathExpression apiImplementedInterfacesExpression = XPathExpression.Compile("implements/type");
        protected static XPathExpression apiParametersExpression = XPathExpression.Compile("parameters/parameter");
        protected static XPathExpression apiReturnTypeExpression = XPathExpression.Compile("returns/*[1]");

        // member data
        protected static XPathExpression apiVisibilityOfMemberExpression = XPathExpression.Compile("string(memberdata/@visibility)");
        protected static XPathExpression apiIsStaticExpression = XPathExpression.Compile("boolean(memberdata[@static='true'])");
        protected static XPathExpression apiIsSpecialExpression = XPathExpression.Compile("boolean(memberdata[@special='true'])");
        protected static XPathExpression apiIsDefaultMemberExpression = XPathExpression.Compile("boolean(memberdata[@default='true'])");

        // field data
        protected static XPathExpression apiIsLiteralFieldExpression = XPathExpression.Compile("boolean(fielddata[@literal='true'])");
        protected static XPathExpression apiIsInitOnlyFieldExpression = XPathExpression.Compile("boolean(fielddata[@initonly='true'])");
        protected static XPathExpression apiIsVolatileFieldExpression = XPathExpression.Compile("boolean(fielddata[@volatile='true'])");
        protected static XPathExpression apiIsSerializedFieldExpression = XPathExpression.Compile("boolean(fielddata[@serialized='true'])");
        // !EFW - Added support for interop metadata
        protected static XPathExpression apiFieldOffsetFieldExpression = XPathExpression.Compile("number(fielddata/@offset)");

        // !EFW - Added support for fixed keyword
        protected static XPathExpression apiFixedAttribute = XPathExpression.Compile("attributes//attribute[type/@api=" +
            "'T:System.Runtime.CompilerServices.FixedBufferAttribute']");
        protected static XPathExpression apiFixedBufferType = XPathExpression.Compile("argument[1]/typeValue/type");
        protected static XPathExpression apiFixedBufferSize = XPathExpression.Compile("argument[2]/value");

        // procedure data
        protected static XPathExpression apiIsAbstractProcedureExpression = XPathExpression.Compile("boolean(proceduredata[@abstract='true'])");
        protected static XPathExpression apiIsVirtualExpression = XPathExpression.Compile("boolean(proceduredata[@virtual='true'])");
        protected static XPathExpression apiIsFinalExpression = XPathExpression.Compile("boolean(proceduredata[@final='true'])");
        protected static XPathExpression apiIsVarargsExpression = XPathExpression.Compile("boolean(proceduredata[@varargs='true'])");
        protected static XPathExpression apiOverridesMemberExpression = XPathExpression.Compile("string(proceduredata/@overrides/member)");
        protected static XPathExpression apiIsExplicitImplementationExpression = XPathExpression.Compile("boolean(memberdata/@visibility='private' and proceduredata/@virtual='true' and boolean(implements/member))");
        protected static XPathExpression apiImplementedMembersExpression = XPathExpression.Compile("implements/member");
        protected static XPathExpression apiIsOverrideExpression = XPathExpression.Compile("boolean(overrides/member)");
        // !EFW - Added support for interop metadata
        protected static XPathExpression apiPreserveSigProcedureExpression = XPathExpression.Compile("boolean(proceduredata[@preservesig='true'])");
        protected static XPathExpression apiModuleProcedureExpression = XPathExpression.Compile("string(proceduredata/@module)");
        protected static XPathExpression apiEntryPointProcedureExpression = XPathExpression.Compile("string(proceduredata/@entrypoint)");
        protected static XPathExpression apiCallingConvProcedureExpression = XPathExpression.Compile("string(proceduredata/@callingconvention)");
        protected static XPathExpression apiCharSetProcedureExpression = XPathExpression.Compile("string(proceduredata/@charset)");
        protected static XPathExpression apiBestFitMappingProcedureExpression = XPathExpression.Compile("string(proceduredata/@bestfitmapping)");
        protected static XPathExpression apiExactSpellingProcedureExpression = XPathExpression.Compile("boolean(proceduredata[@exactspelling='true'])");
        protected static XPathExpression apiUnmappableCharProcedureExpression = XPathExpression.Compile("boolean(proceduredata[@throwonunmappablechar='true'])");
        protected static XPathExpression apiSetLastErrorProcedureExpression = XPathExpression.Compile("boolean(proceduredata[@setlasterror='true'])");

        // property data
        protected static XPathExpression apiIsReadPropertyExpression = XPathExpression.Compile("boolean(propertydata/@get='true')");
        protected static XPathExpression apiIsWritePropertyExpression = XPathExpression.Compile("boolean(propertydata/@set='true')");
        protected static XPathExpression apiGetVisibilityExpression = XPathExpression.Compile("string(propertydata/@get-visibility)");
        protected static XPathExpression apiSetVisibilityExpression = XPathExpression.Compile("string(propertydata/@set-visibility)");
        // !EFW - Added for property attributes
        protected static XPathExpression apiGetterExpression = XPathExpression.Compile("getter");
        protected static XPathExpression apiSetterExpression = XPathExpression.Compile("setter");
        
        // return data
        protected static XPathExpression apiIsUdtReturnExpression = XPathExpression.Compile("boolean(returns/type[@api='T:System.Void']/requiredModifier/type[@api='T:System.Runtime.CompilerServices.IsUdtReturn'])");
        protected static XPathExpression returnsTypeExpression = XPathExpression.Compile("returns/type");
        protected static XPathExpression returnsValueExpression = XPathExpression.Compile("value|nullValue|enumValue");

        // event data
        protected static XPathExpression apiHandlerOfEventExpression = XPathExpression.Compile("eventhandler/*[1]");
        protected static XPathExpression apiEventArgsExpression = XPathExpression.Compile("eventargs/*[1]");

        // parameter data
        //protected static XPathExpression params_expression = XPathExpression.Compile("boolean(@params='true')");
        protected static XPathExpression parameterNameExpression = XPathExpression.Compile("string(@name)");
        protected static XPathExpression parameterTypeExpression = XPathExpression.Compile("*[1]");
        protected static XPathExpression parameterIsInExpression = XPathExpression.Compile("boolean(@in='true')");
        protected static XPathExpression parameterIsOutExpression = XPathExpression.Compile("boolean(@out='true')");
        protected static XPathExpression parameterIsRefExpression = XPathExpression.Compile("boolean(referenceTo)");
        protected static XPathExpression parameterIsParamArrayExpression = XPathExpression.Compile("boolean(@params='true')");
        // !EFW - Added for optional argument expression
        protected static XPathExpression parameterArgumentExpression = XPathExpression.Compile("argument");
        protected static XPathExpression parameterIsOptionalExpression = XPathExpression.Compile("boolean(@optional='true')");

        // container data
        protected static XPathExpression apiContainingTypeExpression = XPathExpression.Compile("containers/type");
        protected static XPathExpression apiContainingTypeNameExpression = XPathExpression.Compile("string(containers/type/apidata/@name)");
        protected static XPathExpression apiContainingTypeSubgroupExpression = XPathExpression.Compile("string(containers/type/apidata/@subgroup)");
        protected static XPathExpression apiContainingAssemblyExpression = XPathExpression.Compile("string(containers/library/@assembly)");
        protected static XPathExpression apiContainingNamespaceIdExpression = XPathExpression.Compile("string(containers/namespace/@api)");
        protected static XPathExpression apiContainingNamespaceNameExpression = XPathExpression.Compile("string(containers/namespace/apidata/@name)");
        // protected static XPathExpression containing_type_templates_expression = XPathExpression.Compile("containers/container[@type]/templates");

        // referenced type data
        protected static XPathExpression typeExpression = XPathExpression.Compile("*[1]");
        protected static XPathExpression typeIdExpression = XPathExpression.Compile("@api");
        protected static XPathExpression typeModifiersExpression = XPathExpression.Compile("optionalModifier|requiredModifier|specialization");
        protected static XPathExpression typeOuterTypeExpression = XPathExpression.Compile("type");
        protected static XPathExpression typeIsObjectExpression = XPathExpression.Compile("boolean(local-name()='type' and @api='T:System.Object')");
        protected static XPathExpression valueExpression = XPathExpression.Compile("*[2]");
        protected static XPathExpression nameExpression = XPathExpression.Compile("string(@name)");
        protected static XPathExpression specializationArgumentsExpression = XPathExpression.Compile("*");
        // !EFW - Added for interior_ptr<T>
        protected static XPathExpression explicitDereferenceExpression = XPathExpression.Compile("boolean(optionalModifier/type[@api='T:System.Runtime.CompilerServices.IsExplicitlyDereferenced'])");

        // referenced member data
        protected static XPathExpression memberDeclaringTypeExpression = XPathExpression.Compile("*[1]");

        // attribute data
        protected static XPathExpression attributeTypeExpression = XPathExpression.Compile("*[1]");
        protected static XPathExpression attributeArgumentsExpression = XPathExpression.Compile("argument");
        protected static XPathExpression attributeAssignmentsExpression = XPathExpression.Compile("assignment");
        protected static XPathExpression assignmentNameExpression = XPathExpression.Compile("string(@name)");

        // template data
        protected static XPathExpression templateNameExpression = XPathExpression.Compile("string(@name)");
        protected static XPathExpression templateIsConstrainedExpression = XPathExpression.Compile("boolean(constrained)");
        protected static XPathExpression templateIsValueTypeExpression = XPathExpression.Compile("boolean(constrained/@value='true')");
        protected static XPathExpression templateIsReferenceTypeExpression = XPathExpression.Compile("boolean(constrained/@ref='true')");
        protected static XPathExpression templateIsConstructableExpression = XPathExpression.Compile("boolean(constrained/@ctor='true')");
        protected static XPathExpression templateConstraintsExpression = XPathExpression.Compile("constrained/type | constrained/implements/type | constrained/implements/template");
        protected static XPathExpression templateIsCovariantExpression = XPathExpression.Compile("boolean(variance/@covariant='true')");
        protected static XPathExpression templateIsContravariantExpression = XPathExpression.Compile("boolean(variance/@contravariant='true')");

        protected static XPathExpression attachedEventAdderExpression = XPathExpression.Compile("string(attachedeventdata/adder/member/@api)");
        protected static XPathExpression attachedEventRemoverExpression = XPathExpression.Compile("string(attachedeventdata/remover/member/@api)");

        protected static XPathExpression attachedPropertyGetterExpression = XPathExpression.Compile("string(attachedpropertydata/getter/member/@api)");
        protected static XPathExpression attachedPropertySetterExpression = XPathExpression.Compile("string(attachedpropertydata/setter/member/@api)");

#pragma warning restore 1591
        #endregion

        #region Properties
        //=====================================================================

        /// <summary>
        /// This is used to get or set the language name
        /// </summary>
        /// <value>This is used as the code language name added as an attribute to the <c>div</c>element written
        /// to the topics.  The presentation style XSL transformations will also use it to name the
        /// language-specific resource items.</value>
        public string Language { get; protected set; }

        /// <summary>
        /// This is used to get or set the style ID
        /// </summary>
        /// <value>This is used as the code style ID added as an attribute to the <c>div</c>element written
        /// to the topics.  The presentation style XSL transformations will use it to group common language
        /// elements such as language-specific text and code snippets so that they can be shown and hidden
        /// together.</value>
        public string StyleId { get; protected set; }

        #endregion

        #region Constructor
        //=====================================================================

        /// <summary>
        /// Constructor
        /// </summary>
        protected SyntaxGeneratorTemplate()
        {
        }
        #endregion

        #region Abstract methods
        //=====================================================================

        /// <summary>
        /// Write namespace syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteNamespaceSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write class syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteClassSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write structure syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteStructureSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write interface syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteInterfaceSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write delegate syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteDelegateSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write enumeration syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteEnumerationSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write constructor syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteConstructorSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write property syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WritePropertySyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write field syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteFieldSyntax(XPathNavigator reflection, SyntaxWriter writer);

        /// <summary>
        /// Write event syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public abstract void WriteEventSyntax(XPathNavigator reflection, SyntaxWriter writer);

        #endregion

        #region Overrides and other methods
        //=====================================================================

        /// <summary>
        /// Initialize the syntax generator
        /// </summary>
        /// <param name="configuration">The syntax generator configuration</param>
        /// <remarks>The base implementation just validates that a <see cref="Language"/> and
        /// <see cref="StyleId"/> have been defined.</remarks>
        /// <exception cref="InvalidOperationException">This is thrown if a <see cref="Language"/> or
        /// <see cref="StyleId"/> has not been set.</exception>
        public override void Initialize(XPathNavigator configuration)
        {
            if(String.IsNullOrWhiteSpace(this.Language))
                throw new InvalidOperationException("A language name has not been set");

            if(String.IsNullOrWhiteSpace(this.StyleId))
                throw new InvalidOperationException("A style ID has not been set");
        }

        // Methods to write syntax for different kinds of APIs

        /// <summary>
        /// This is the main syntax writing method
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public override void WriteSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            writer.WriteStartBlock(this.Language, this.StyleId);

            string group = (string)reflection.Evaluate(apiGroupExpression);

            switch(group)
            {
                case "namespace":
                    WriteNamespaceSyntax(reflection, writer);
                    break;

                case "type":
                    WriteTypeSyntax(reflection, writer);
                    break;

                case "member":
                    WriteMemberSyntax(reflection, writer);
                    break;
            }

            writer.WriteEndBlock();
        }

        /// <summary>
        /// Write type syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteTypeSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string subgroup = (string)reflection.Evaluate(apiSubgroupExpression);

            switch(subgroup)
            {
                case "class":
                    WriteClassSyntax(reflection, writer);
                    break;
                case "structure":
                    WriteStructureSyntax(reflection, writer);
                    break;
                case "interface":
                    WriteInterfaceSyntax(reflection, writer);
                    break;
                case "delegate":
                    WriteDelegateSyntax(reflection, writer);
                    break;
                case "enumeration":
                    WriteEnumerationSyntax(reflection, writer);
                    break;
            }
        }

        /// <summary>
        /// Write member syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteMemberSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string subgroup = (string)reflection.Evaluate(apiSubgroupExpression);
            string subsubgroup = (string)reflection.Evaluate(apiSubsubgroupExpression);

            switch(subgroup)
            {
                case "constructor":
                    WriteConstructorSyntax(reflection, writer);
                    break;
                case "method":
                    WriteMethodSyntax(reflection, writer);
                    break;
                case "property":
                    if(subsubgroup == "attachedProperty")
                        WriteAttachedPropertySyntax(reflection, writer);
                    else
                        WritePropertySyntax(reflection, writer);
                    break;
                case "event":
                    if(subsubgroup == "attachedEvent")
                        WriteAttachedEventSyntax(reflection, writer);
                    else
                        WriteEventSyntax(reflection, writer);
                    break;
                case "field":
                    WriteFieldSyntax(reflection, writer);
                    break;
            }
        }

        /// <summary>
        /// Write method syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isSpecialName = (bool)reflection.Evaluate(apiIsSpecialExpression);
            string name = (string)reflection.Evaluate(apiNameExpression);

            if(isSpecialName)
            {
                string subsubgroup = (string)reflection.Evaluate(apiSubsubgroupExpression);

                if(subsubgroup == "operator")
                    if(name == "Implicit" || name == "Explicit")
                        WriteCastSyntax(reflection, writer);
                    else
                        WriteOperatorSyntax(reflection, writer);

                // Write out let properties (no .Net equivalent) as methods
                if(name.StartsWith("let_", StringComparison.Ordinal))
                    WriteNormalMethodSyntax(reflection, writer);
            }
            else
                WriteNormalMethodSyntax(reflection, writer);
        }

        /// <summary>
        /// Write normal method syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteNormalMethodSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
        }

        /// <summary>
        /// Write operator syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteOperatorSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
        }

        /// <summary>
        /// Write cast syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteCastSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
        }

        /// <summary>
        /// Write attached property syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteAttachedPropertySyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string getterId = (string)reflection.Evaluate(attachedPropertyGetterExpression);
            string setterId = (string)reflection.Evaluate(attachedPropertySetterExpression);
            if(!string.IsNullOrEmpty(getterId))
            {
                writer.WriteString("See ");
                writer.WriteReferenceLink(getterId);
            }
            if(!string.IsNullOrEmpty(setterId))
            {
                if(!string.IsNullOrEmpty(getterId))
                    writer.WriteString(", ");
                writer.WriteReferenceLink(setterId);
            }
        }

        /// <summary>
        /// Write attached event syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        public virtual void WriteAttachedEventSyntax(XPathNavigator reflection, SyntaxWriter writer)
        {
            string adderId = (string)reflection.Evaluate(attachedEventAdderExpression);
            string removerId = (string)reflection.Evaluate(attachedEventRemoverExpression);
            if(!(string.IsNullOrEmpty(adderId) && string.IsNullOrEmpty(removerId)))
            {
                writer.WriteString("See ");
                writer.WriteReferenceLink(adderId);
                writer.WriteString(", ");
                writer.WriteReferenceLink(removerId);
            }
        }

        /// <summary>
        /// Write unsupported variable arguments syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        /// <returns>True if unsupported, false if it is supported</returns>
        protected virtual bool IsUnsupportedVarargs(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isVarargs = (bool)reflection.Evaluate(apiIsVarargsExpression);

            if(isVarargs)
                writer.WriteMessage("UnsupportedVarargs_" + this.Language);

            return isVarargs;
        }

        /// <summary>
        /// Write unsupported unsafe code syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        /// <returns>True if unsupported, false if it is supported</returns>
        protected virtual bool IsUnsupportedUnsafe(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isUnsafe = (bool)reflection.Evaluate(apiIsUnsafeExpression);

            if(isUnsafe)
                writer.WriteMessage("UnsupportedUnsafe_" + this.Language);

            return isUnsafe;
        }

        /// <summary>
        /// Write unsupported generic types syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        /// <returns>True if unsupported, false if it is supported</returns>
        protected virtual bool IsUnsupportedGeneric(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isGeneric = (bool)reflection.Evaluate(apiIsGenericExpression);

            if(isGeneric)
                writer.WriteMessage("UnsupportedGeneric_" + this.Language);

            return isGeneric;
        }

        /// <summary>
        /// Write unsupported explicit implementation syntax
        /// </summary>
        /// <param name="reflection">The reflection data used to produce the syntax</param>
        /// <param name="writer">The writer to which the syntax is written</param>
        /// <returns>True if unsupported, false if it is supported</returns>
        protected virtual bool IsUnsupportedExplicit(XPathNavigator reflection, SyntaxWriter writer)
        {
            bool isExplicit = (bool)reflection.Evaluate(apiIsExplicitImplementationExpression);

            if(isExplicit)
                writer.WriteMessage("UnsupportedExplicit_" + this.Language);

            return isExplicit;
        }

        /// <summary>
        /// This is used to write a string followed by an optional line break if needed (the writer position is
        /// past the maximum position afterwards).
        /// </summary>
        /// <param name="writer">The syntax writer to use</param>
        /// <param name="text">An optional text string to write before the new line</param>
        /// <param name="indent">An optional indent to write after the line break</param>
        /// <returns>True if a new line was written, false if not</returns>
        public static bool WriteWithLineBreakIfNeeded(SyntaxWriter writer, string text, string indent)
        {
            if(!String.IsNullOrEmpty(text))
                writer.WriteString(text);

            if(writer.Position > MaxPosition)
            {
                writer.WriteLine();

                if(!String.IsNullOrEmpty(indent))
                    writer.WriteString(indent);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Write out a type reference
        /// </summary>
        /// <param name="reference">The type reference to output</param>
        /// <param name="writer">The syntax writer to which the type reference is written</param>
        protected virtual void WriteTypeReference(XPathNavigator reference, SyntaxWriter writer)
        {
            switch(reference.LocalName)
            {
                case "arrayOf":
                    int rank = Convert.ToInt32(reference.GetAttribute("rank", String.Empty),
                        CultureInfo.InvariantCulture);

                    XPathNavigator element = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(element, writer);
                    writer.WriteString("[");

                    for(int i = 1; i < rank; i++)
                        writer.WriteString(",");

                    writer.WriteString("]");
                    break;

                case "pointerTo":
                    XPathNavigator pointee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(pointee, writer);
                    writer.WriteString("*");
                    break;

                case "referenceTo":
                    XPathNavigator referee = reference.SelectSingleNode(typeExpression);
                    WriteTypeReference(referee, writer);
                    break;

                case "type":
                    string id = reference.GetAttribute("api", String.Empty);
                    XPathNodeIterator typeModifiers = reference.Select(typeModifiersExpression);

                    // !EFW - Support value tuple syntax
                    if(id.StartsWith("T:System.ValueTuple`", StringComparison.Ordinal))
                    {
                        writer.WriteString("(");

                        while(typeModifiers.MoveNext())
                        {
                            XPathNodeIterator args = reference.Select(specializationArgumentsExpression);

                            while(args.MoveNext())
                            {
                                if(args.CurrentPosition > 1)
                                    writer.WriteString(", ");

                                var elArgs = args.Current.Select(specializationArgumentsExpression);

                                while(elArgs.MoveNext())
                                {
                                    if(elArgs.CurrentPosition > 1)
                                        writer.WriteString(", ");

                                    WriteTypeReference(elArgs.Current, writer);

                                    var elementName = elArgs.Current.GetAttribute("elementName", String.Empty);

                                    if(elementName != null)
                                    {
                                        writer.WriteString(" ");
                                        writer.WriteString(elementName);
                                    }
                                }
                            }
                        }

                        writer.WriteString(")");
                    }
                    else
                    {
                        WriteNormalTypeReference(id, writer);

                        while(typeModifiers.MoveNext())
                            WriteTypeReference(typeModifiers.Current, writer);
                    }
                    break;

                case "template":
                    string name = reference.GetAttribute("name", String.Empty);
                    writer.WriteString(name);
                    XPathNodeIterator modifiers = reference.Select(typeModifiersExpression);

                    while(modifiers.MoveNext())
                        WriteTypeReference(modifiers.Current, writer);

                    break;

                case "specialization":
                    writer.WriteString("<");
                    XPathNodeIterator arguments = reference.Select(specializationArgumentsExpression);

                    while(arguments.MoveNext())
                    {
                        if(arguments.CurrentPosition > 1)
                            writer.WriteString(", ");

                        WriteTypeReference(arguments.Current, writer);
                    }

                    writer.WriteString(">");
                    break;
            }
        }

        /// <summary>
        /// Write out a normal type reference
        /// </summary>
        /// <param name="api">The type reference to output</param>
        /// <param name="writer">The syntax writer to which the type reference is written</param>
        protected virtual void WriteNormalTypeReference(string api, SyntaxWriter writer)
        {
            switch(api)
            {
                case "T:System.Void":
                    writer.WriteReferenceLink(api, "void");
                    break;

                case "T:System.String":
                    writer.WriteReferenceLink(api, "string");
                    break;

                case "T:System.Boolean":
                    writer.WriteReferenceLink(api, "bool");
                    break;

                case "T:System.Byte":
                    writer.WriteReferenceLink(api, "byte");
                    break;

                case "T:System.SByte":
                    writer.WriteReferenceLink(api, "sbyte");
                    break;

                case "T:System.Char":
                    writer.WriteReferenceLink(api, "char");
                    break;

                case "T:System.Int16":
                    writer.WriteReferenceLink(api, "short");
                    break;

                case "T:System.Int32":
                    writer.WriteReferenceLink(api, "int");
                    break;

                case "T:System.Int64":
                    writer.WriteReferenceLink(api, "long");
                    break;

                case "T:System.UInt16":
                    writer.WriteReferenceLink(api, "ushort");
                    break;

                case "T:System.UInt32":
                    writer.WriteReferenceLink(api, "uint");
                    break;

                case "T:System.UInt64":
                    writer.WriteReferenceLink(api, "ulong");
                    break;

                case "T:System.Single":
                    writer.WriteReferenceLink(api, "float");
                    break;

                case "T:System.Double":
                    writer.WriteReferenceLink(api, "double");
                    break;

                case "T:System.Decimal":
                    writer.WriteReferenceLink(api, "decimal");
                    break;

                default:
                    writer.WriteReferenceLink(api);
                    break;
            }
        }

        /// <summary>
        /// Write out a constant's value
        /// </summary>
        /// <param name="parent">The parent node</param>
        /// <param name="writer">The syntax writer</param>
        protected virtual void WriteConstantValue(XPathNavigator parent, SyntaxWriter writer)
        {
            XPathNavigator type = parent.SelectSingleNode(returnsTypeExpression);
            XPathNavigator value = parent.SelectSingleNode(returnsValueExpression);

            switch(value.LocalName)
            {
                case "nullValue":
                    writer.WriteKeyword("null");
                    break;

                case "enumValue":
                    XPathNodeIterator fields = value.SelectChildren(XPathNodeType.Element);

                    while(fields.MoveNext())
                    {
                        string name = fields.Current.GetAttribute("name", String.Empty);

                        if(fields.CurrentPosition > 1)
                            writer.WriteString("|");

                        this.WriteTypeReference(type, writer);
                        writer.WriteString(".");
                        writer.WriteString(name);
                    }
                    break;

                case "value":
                    string text = value.Value;
                    string typeId = type.GetAttribute("api", String.Empty);

                    switch(typeId)
                    {
                        case "T:System.String":
                            writer.WriteString("\"");
                            writer.WriteString(text);
                            writer.WriteString("\"");
                            break;

                        case "T:System.Boolean":
                            writer.WriteKeyword(Convert.ToBoolean(text, CultureInfo.InvariantCulture) ?
                                "true" : "false");
                            break;

                        case "T:System.Char":
                            writer.WriteString("'");
                            writer.WriteString(text);
                            writer.WriteString("'");
                            break;

                        // Decimal constants get converted to static read-only fields so no need to handle them here
                        case "T:System.Byte":
                        case "T:System.Double":
                        case "T:System.SByte":
                        case "T:System.Int16":
                        case "T:System.Int64":
                        case "T:System.Int32":
                        case "T:System.UInt16":
                        case "T:System.UInt32":
                        case "T:System.UInt64":
                            writer.WriteString(text);
                            break;

                        case "T:System.Single":
                            writer.WriteString(text);
                            writer.WriteString("f");
                            break;

                        default:
                            // If not a recognized type, just write out the value so that something shows
                            writer.WriteString(text);
                            break;
                    }
                    break;
            }
        }
        #endregion
    }
}
