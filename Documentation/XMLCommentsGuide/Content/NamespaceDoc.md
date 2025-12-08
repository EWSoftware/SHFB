---
uid: 41B2D835-DB0D-4828-8D9E-0E423EDA4590
alt-uid: NamespaceDoc
title: Using NamespaceDoc and NamespaceGroupDoc Classes
keywords: "namespace comments, NamespaceDoc class", "namespace comments, NamespaceGroupDoc class"
---

If your build tool supports it, namespace comments can also be specified and maintained in your
source code by adding an empty `NamespaceDoc` class to each namespace.  When comments for
this class are found in the XML comments files they will be used as the namespace comments.


To keep the `NamespaceDoc` class from appearing in the help file, leave off the
`public` keyword and mark it with a `CompilerGenerated` attribute.
This will cause the class to be automatically ignored when reflection information is generated for the assembly.
The following is an example:


``` cs
namespace Company.Product.Widgets
{
    /// <summary>
    /// These are the namespace comments for <c>Company.Product.Widgets</c>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceDoc
    {
    }
}
```

``` vbnet
Namespace Company.Product.Widgets
    ''' <summary>
    ''' These are the namespace comments for <c>Company.Product.Widgets</c>.
    ''' </summary>
    <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
    Class NamespaceDoc
    End Class
End Namespace
```

If the project has namespace grouping enabled, you can also maintain the namespace group comments using
a `NamespaceGroupDoc` class in a similar fashion.  The following is an example:


``` cs
namespace Company.Product
{
    /// <summary>
    /// These are the group comments for namespaces in <c>Company.Product</c>.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    class NamespaceGroupDoc
    {
    }
}
```

``` vbnet
Namespace Company.Product
    ''' <summary>
    ''' These are the group comments for namespaces in <c>Company.Product</c>.
    ''' </summary>
    <System.Runtime.CompilerServices.CompilerGeneratedAttribute()> _
    Class NamespaceGroupDoc
    End Class
End Namespace
```


## See Also


**Reference**  
[](@N:XMLCommentsExamples.DocumentationInheritance)  


**Other Resources**  
[](@BD91FAD4-188D-4697-A654-7C07FD47EF31)  
[](@B597E874-6D7C-4E05-B8F7-5B530C656B70)  
