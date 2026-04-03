---
uid: TestMarkdownContent
title: Test Markdown Topic
---
This is an optional introduction section.  Any text placed before the first heading will be put into an
introduction section in the generated topic.  This is typically one or two paragraphs that give brief overview
of the topic.

## Optional Section Title
Add one or more sections and sub-sections with content.

## Include Test
Content can be included from other files.  Relative paths are resolved relative to the current file's location.

``` none
Content can be included inline: \[!INCLUDE [](InlineIncludeContent.md)]
```

Content can be included inline: [!INCLUDE [](InlineIncludeContent.md)]

It can also be included as a block:

``` none
\[!INCLUDE [](BlockIncludeContent.md)]

```

[!INCLUDE [](BlockIncludeContent.md)]

Prefix the leading brackets with a backslash to escape it and pass it through as a literal

``` none
\[!include \[](IgnoreEscapedLiteralInclude.md)]
```

\[!include \[](IgnoreEscapedLiteralInclude.md)]

Backslashes or forward slashes can be used as path separators.  Example of a relative path that includes a file from a
parent folder: `\[!INCLUDE [](../../..\ReadMe.md)]`

[!INCLUDE [](../../..\ReadMe.md)]

## See Also
**Other Resources**  
[](@303c996a-2911-4c08-b492-6496c82b3edb)  
[](@ba47a67c-4825-4fcd-b806-2b2e02d4373a)
