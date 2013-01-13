This folder contains the three source code files I modified for the version of Managed ESent used by the
Sandcastle Help File Builder components.  My changes are indictated by comments prefixed with "!EFW".

    - Added static property to PersistentDictionaryFile to indicate reference type serialization status.
    - Updated ColumnConverter to allow serialization of reference types.
    - Added the option to PersistentDictionary to disable column compression using a new constructor.
    - Strong named the assemblies.

Since they are only a few minor changes, I've opted not to distribute the full file set.  These files are from
the 1.7 release which can be downloaded from the following changeset URL:

    http://managedesent.codeplex.com/SourceControl/changeset/70995.
