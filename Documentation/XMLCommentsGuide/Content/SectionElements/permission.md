---
uid: 4af64f3f-a9a3-42d7-a95c-bc0a40951286
alt-uid: permission
title: permission
keywords: "section elements, permission"
---

This element is used to define the permissions required to access a type or member.



## Syntax

This top-level element is valid on all types and type members.


``` xml{title=" "}
<permission cref="permissionTypeOrMember">description</permission>
```

The `cref` attribute is used to specify a permission type required to access
the type or member.  It is typically one of the .NET permission set types such as
[](@T:System.Security.PermissionSet)
or [](@T:System.Security.Permissions.SecurityPermission).
Use the inner text to describe how the permission applies to the type or member.



## Examples

``` cs{title=" " source="SampleClass.cs" region="permission Example"}
```


## See Also


**Reference**  
[](@M:XMLCommentsExamples.SampleClass.MethodRequiringSpecificPermissions){prefer-overload="true"}  


**Other Resources**  
[](@20dc8c5f-9979-4ecd-92ce-cea6ce7acaeb)  
