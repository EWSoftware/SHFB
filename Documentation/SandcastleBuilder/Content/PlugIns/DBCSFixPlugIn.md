---
uid: 31696f39-8f4e-4c4d-ab08-41a40793df03
alt-uid: DBCSFixPlugIn
title: DBCS Fix for CHM Builds Plug-In
keywords: "plug-ins, DBCS fix for CHM builds plug-in", "DbcsFixPlugIn, configuration options"
---
DBCS stands for Double Byte Character Set.  Several languages, most notably East Asian languages and Russian,
cause problems for HTML Help 1 (.chm) builds because the help compiler is not Unicode-aware.  This can cause the
table of contents, index, and in some cases the help topics to display incorrect characters.  This plug-in is
designed to correct these issues.

<autoOutline />

## Features
The plug-in modifies the topic files and the build in the following ways:

- It converts unsupported high-order characters to 7-bit ASCII equivalents.
- It converts several unsupported high-order characters to named entities.
- It replaces the UTF-8 reference in the `CHARSET` meta tag in each HTML page with a character set reference
  appropriate to the selected language.
- It saves each HTML page using the encoding appropriate to the selected language.
- It uses the **SBAppLocale** tool from [Steel Bytes](https://github.com/EWSoftware/SHFB/blob/master/ThirdPartyTools/SBAppLocale_ENG.zip)
  to run the HTML Help 1 compiler under the appropriate locale for the selected language.  This saves you from
  having to manually change the language in your Regional Settings, reboot, and perform the help compile step.

To use the plug-in, add it to the project and configure it.  The only option needed is the location of the
*SBAppLocale.exe* application.  If left blank, only the changes made by the Sandcastle HTML Extract tool will be
applied to the topics.

> [!IMPORTANT]
> If the *SBAppLocale.exe* tool is not used, the text in the table of contents, index, and search pages may not
> appear correctly if the language of the help file does not match the locale of the system on which it is built
> (i.e. building a Russian help file on an system with an English locale).  If the locales match, there typically
> will not be any issues.

## Method Naming Issues
Note that if your code uses Unicode characters in the member names, it may not be possible to use the
`MemberName` option for the `NamingMethod` property.  If you receive build errors that indicate that it cannot
find certain files and the names do not look correct, change the naming method to use either the `GUID` or
`HashedMemberName` option.

## Additional Content Files
If you add HTML pages as additional content or use a topic transformation file to create HTML additional content
pages, you should ensure that a `meta` tag specifying UTF-8 encoding appears in each file.  This will ensure that
all additional content pages are also properly encoded if necessary.  For example:

``` xml{title="Example Character Set Encoding Meta Tag"}
<head>
<title>My Additional Content<title>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8" />
</head>
```

## Language Configuration
The tool that performs the encoding translation relies on the *SandcastleHtmlExtract.config* file to provide
information about the language locale ID, code page, and character set encoding name.  The configuration file is
located in the root help file builder installation folder.  If the project's locale ID cannot be found in the
file, a warning is issued in the log file and a default code page and character set encoding will be used based
on the language selected in the project.  If the defaults are not appropriate, you can add new entries to the
configuration file.

The settings consist of a set of `language` elements, one for each language identified by locale ID, that specify
the settings.  The `id` attribute refers to the locale ID (LCID) for the language. The `codepage` attribute is
the code page to use when determining the encoding for the files based on the given locale ID.  The `charset`
attribute value will be written to the HTML files in place of the UTF-8 value when localizing the files for use
with the HTML Help 1 compiler.

If you do add entries to this file, please report them so that they can be added to a future release of the help
file builder.

## See Also
**Other Resources**  
[](@e031b14e-42f0-47e1-af4c-9fed2b88cbc7)
