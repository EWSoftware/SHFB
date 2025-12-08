---
uid: 86453FFB-B978-4A2A-9EB5-70E118CA8073
alt-uid: InheritDoc
title: inheritdoc
keywords: "miscellaneous elements, inheritdoc"
---
This element can help minimize the effort required to document complex APIs by allowing common
documentation to be inherited from base types/members.

<autoOutline lead="none" />

> [!NOTE]
> Prior to Visual Studio 2019 version 16.4, this was a custom XML comments element implemented by Sandcastle and
> the Sandcastle Help File Builder.  It does not appear in the list of IntelliSense elements for XML comments
> prior to that release.

## Syntax
Although typically used as a top-level element, it can be used as an inline element as well.  The use of the
element by itself on a member is enough to satisfy the compiler so that it will not issue warnings about missing
comments on public members.  Using it in conjunction with other elements allows you to inherit common information
such as value and parameter descriptions while overriding the inherited documentation for other elements such as
`summary` and `remarks`.  Documentation can be inherited from any member from classes within your own assemblies
or from members of other assemblies as well as the base .NET Framework class library.  The syntax of the element
is as follows:

``` xml{title=" "}
<inheritdoc [cref="member"] [path="xpath-filter-expr"] />
```

> [!IMPORTANT]
> As of November 2019, the `select` attribute has been deprecated.  Use the equivalent `path` attribute instead
> which is consistent with the XML comments IntelliSense usage in Visual Studio 2019 and later.

The optional `cref` attribute overrides the standard search method to allow documentation inheritance from an
alternate user-specified member indicated by the *member* value.

The optional `path` attribute applies the specified XPath filter expression to the inherited comments.  This is
useful if you want to limit the inherited documentation to a specific subset of elements or just select a
particular instance or set of comments.  The expression can be any valid XPath query that will result in a node
set.

By making use of the `cref` and `path` attributes either by themselves or together, you can fine tune the
inheritance of documentation.  You can also nest the element within other elements to further refine the level
of inheritance.

When using the Sandcastle Help File Builder, its **GenerateInheritedDocumentation** tool handles the task of
generating the inherited documentation.  The following documentation is based on the Sandcastle Help File
Builder's implementation.

## Top-Level Inheritance Rules
The `inheritdoc` element is valid at the root level (i.e. the same level as `summary` elements) on types,
interfaces, virtual members, interface member implementations, and constructors.  Its use on any other member
type will result in no comments being inherited unless a `cref` attribute is specified.  Note that the element
is also valid in project summary and namespace summary comments as long as a `cref` attribute is specified to
indicate from where to inherit the comments.  When specified at the root level in a set of XML comments, the
documentation search is performed as follows:

- If an explicit `cref` attribute is specified, the documentation from the specified namespace/type/member is
  inherited.  If a `cref` attribute is not specified, the following rules apply.
- For types and interfaces:
  - Inherit documentation from all base classes working backwards up the inheritance chain.
  - Inherit documentation from all interface implementations (if any) working through them in the order listed
    in the reflection information file (usually alphabetically).
- For constructors:
  - Search backwards up the type inheritance chain for a constructor with a matching signature.
  - If a match is found, its documentation is inherited.
- For virtual members and interface implementations:
  - If the member is an override, documentation is inherited from the member it overrides.
  - If the member is part of an interface, documentation is inherited from the interface member being implemented.
- Explicit interface implementations will automatically inherit documentation from the interface member that they
  implement if no documentation is supplied by the user.  This is done automatically because these members are by
  definition private and the compiler will not issue a warning if the user does not supply documentation.  As
  such, you can omit the `inheritdoc` element from them unless you want to customize the comments.
- With or without an explicit `cref` attribute, if the inherited documentation itself contains `inheritdoc`
  elements, they will be expanded recursively working backwards up the inheritance chain.
- In all cases, if a `path` attribute is present, it is used to filter the inherited comments based on the
  specified XPath query.

When inheriting documentation at the root level, if the following elements already exist in the member's
comments, the inherited versions are ignored:

- `example`
- `exclude`
- `filterpriority`
- `preliminary`
- `summary`
- `remarks`
- `returns`
- `threadsafety`
- `value`

The `overloads` element will never be inherited.  This prevents the doubling of comments on the overloads page.
However, you can inherit the contents of the `overloads` element using a `path` attribute with a value of
"`overloads/*`".  See the example below in the Examples section that inherits the elements from an `overloads`
element.  If the element only contains text and you want to inherit that text, include an `overloads` element
with a nested `inheritdoc` element.  For example:

``` cs{title=" "}
/// <inheritdoc /> 
/// 
/// <overloads>
/// The inheritdoc element outside this "overloads" element inherits the standard
/// summary, parameters, remarks, etc. for the member itself.
/// 
/// The inheritdoc element below will inherit the overload text from the implemented
/// member and it will appear on the overloads page:
/// 
/// <inheritdoc /> 
/// 
/// </overloads>
```

All other elements will be inherited unless they match an element by the same name that contains a `cref`,
`href`, `name`, `vref`, or `xref` attribute with an identical value in the member's comments.  To merge comments
in one of the above elements from one or more sources, use one or more nested `inheritdoc` elements within the
given element.  See below for examples.

Be aware that when `param` elements are inherited, the parameter's name in your class's member must match the
base member's parameter name.  If they do not match, you will not see any inherited documentation for the
parameter.  Also, if you supply comments for one parameter but omit comments for other parameters in order to
inherit their documentation from a base implementation, the compiler will issue a warning.  In this case, you can
use a `#pragma warning` directive to disable the warning temporarily or add it to the project settings to disable
the warning globally.  See below for an example.


## Inline Inheritance Rules
The `inheritdoc` element can also be nested within other XML comments elements such as `summary`, `remarks`,
`example` etc. in order to inherit specific parts of the documentation within those elements.  When nested, the
same root level inheritance rules apply and will be used to locate the first member with comments from which to
inherit documentation.  In addition, a filter will be automatically included based on the parent element or
elements within which the `inheritdoc` element is nested.  The `cref` and `path` attributes can also be applied
to further qualify how the documentation is inherited.  If you do not want to have the parent elements
automatically included in the filter, you must supply a `path` attribute with a rooted XPath query that specifies
from where to obtain the comments (i.e. `path="/summary/node()"`).

## Additional Comment File Sources and IntelliSense
In the Sandcastle Help File Builder, additional sources of inherited documentation (i.e. comments from third
party class libraries) can be added to the **Documentation Sources** project node.  This allows you to inherit
documentation from base class libraries without having to add them as documented assemblies in your project.

Since the XML comments produced by the compiler are incomplete when using `inheritdoc`, it is highly recommended
that you make use of the **IntelliSense Build Component** to produce an IntelliSense XML comments file.  It will
include the fully expanded set of inherited documentation so that Visual Studio can provide useful and accurate
API help in the code editor and object browser.

## Examples
The following show various examples of using the `inheritdoc` element.  See the comments within each for details
about what the examples are showing.

``` cs{source="DocumentationInheritance\CustomException.cs" region="Constructor documentation inheritance" outlining="true" title="Constructor Documentation Inheritance"}
```

``` cs{source="DocumentationInheritance\ExplicitImplementation.cs" region="Interface implementation documentation inheritance" outlining="true" title="Interface Implementation Examples"}
```

``` cs{source="DocumentationInheritance\DocumentationInheritance.cs" region="Various documentation inheritance examples" outlining="true" title="Various Other Examples"}
```

## See Also
**Reference**  
[](@T:XMLCommentsExamples.DocumentationInheritance.BaseInheritDoc)  
[](@T:XMLCommentsExamples.DocumentationInheritance.CustomException)  
[](@T:XMLCommentsExamples.DocumentationInheritance.DerivedClassWithInheritedDocs)  
[](@T:XMLCommentsExamples.DocumentationInheritance.ExplicitImplementation)  
[](@T:XMLCommentsExamples.DocumentationInheritance.SetDocumentation)  

**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
