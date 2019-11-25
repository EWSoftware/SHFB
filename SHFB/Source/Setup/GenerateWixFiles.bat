@ECHO OFF
REM This can be used to regenerate new copies of the WiX files to see if anything needs adding or removing

REM Get rid of these as we don't want to include them
DEL /S /Q ..\..\Deploy\*.pdb
DEL /S /Q ..\..\Deploy\*vshost*
DEL ..\..\Deploy\ESent*.xml
DEL ..\..\Deploy\AddNamespaceGroups.xml
DEL ..\..\Deploy\GenerateInheritedDocs.xml
DEL ..\..\Deploy\HelpLibraryManagerLauncher.xml
DEL ..\..\Deploy\Microsoft*.xml
DEL ..\..\Deploy\NHunSpell.xml
DEL ..\..\Deploy\SandcastleBuilderGUI.xml
DEL ..\..\Deploy\SandcastleHtmlExtract.xml
DEL ..\..\Deploy\SegregateByNamespace.xml
DEL ..\..\Deploy\System*.xml
DEL ..\..\Deploy\XslTransform.xml
DEL /S /Q ..\..\Deploy\*CodeAnalysis*
DEL ..\..\Deploy\reflection.org

"%WIX%\bin\heat.exe" dir ..\..\Deploy\ -out Temp\DeploymentFiles.wxs -cg Data -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2 -sreg -srd
