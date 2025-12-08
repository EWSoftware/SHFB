---
uid: 6e76ff26-d4f8-491e-ae81-b06086ecf902
alt-uid: CodeContracts
title: Code Contract Elements
keywords: "miscellaneous elements, code contracts", "miscellaneous elements, ensures", "miscellaneous elements, ensuresOnThrow", "miscellaneous elements, invariant", "miscellaneous elements, pure", "miscellaneous elements, requires"
---
<!-- Ignore Spelling: ccdocgen -->

The [Code Contracts Library](https://github.com/Microsoft/CodeContracts "Code Contracts") created by Microsoft contains a tool (**ccdocgen**) that can be ran
after a build to insert contract XML documentation elements into the XML comments file for an assembly.


> [!IMPORTANT]
> The Code Contracts Library project appears to have been abandoned.  As such the Sandcastle Help
> File Builder no longer provides any support for these elements.  Support could be added through a plug-in to add
> handlers for them if necessary.
> 
>

Below is a list of the elements that the tool may insert into each member.  Refer to the Code
Contracts user manual for more information (section 8 at the time this topic was written).


<table>
  <thead>
    <tr>
      <th>Element</th>
      <th>Description</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td>ensures</td>
      <td>May appear under method elements, property getters, and property setters. The element body is
the string of the original postcondition.</td>
    </tr>
    <tr>
      <td>ensuresOnThrow</td>
      <td>May appear under method elements, property getters, and property setters.  The element body
is the string of the original exceptional postcondition.</td>
    </tr>
    <tr>
      <td>invariant</td>
      <td>May appear under classes.  The element body is the string of the original invariant.</td>
    </tr>
    <tr>
      <td>pure</td>
      <td>May appear under methods marking them as pure.  No additional information is present.</td>
    </tr>
    <tr>
      <td>requires</td>
      <td>May appear under method elements, property getters, and property setters. The element body is
the string of the original precondition.</td>
    </tr>
  </tbody>
</table>


## See Also


**Other Resources**  
[](@9341fdc8-1571-405c-8e61-6a6b9b601b46)  
