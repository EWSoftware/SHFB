---
uid: Welcome
title: Welcome to the [TODO: Add project name]
keywords: Welcome
---
This is a sample conceptual topic.  You can use this as a starting point for adding more conceptual
content to your help project.

## Getting Started
To get started, add a documentation source to the project (a Visual Studio solution, project, or assembly and XML
comments file).  See the **Getting Started** topics in the Sandcastle Help File Builder's help file for more
information.  The following default items are included in this project:

- *ContentLayout.content* - Use the content layout file to manage the conceptual content in the project and define
  its layout in the table of contents.
- The *.\media* folder - Place images in this folder that you will reference from conceptual content using
  image links.  If you will not have any images in the file, you may remove this folder.
- The *.\icons* folder - This contains a default logo for the help file.  You may replace it or remove it and
  the folder if not wanted.  If removed or if you change the file name, update the **Transform Args** project
  properties page by removing or changing the filename in the `logoFile` transform argument.  Note that unlike
  images referenced from conceptual topics, the logo file should have its **BuildAction** property set to
  `Content`.
- The *.\Content* folder - Use this to store your conceptual topics.  You may name the files and organize them
  however you like.  One suggestion is to lay the files out on disk as you have them in the content layout file as
  shown in this project but the choice is yours.  Files can be added via the Solution Explorer or from within the
  content layout file editor.  Files must appear in the content layout file in order to be compiled into the help
  file.

See the **Conceptual Content** topics in the Sandcastle Help File Builder's help file for more information.

## See Also
**Other Resources**  
[](@VersionHistory)
