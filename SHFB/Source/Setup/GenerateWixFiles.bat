@ECHO OFF
REM This can be used to regenerate new copies of the WiX files to see if anything needs adding or removing

REM Get rid of these as we don't care to include them
DEL /S /Q ..\..\Deploy\*.pdb
DEL /S /Q ..\..\Deploy\*vshost*
DEL ..\..\Deploy\ESent*.xml
DEL ..\..\Deploy\GenerateInheritedDocs.xml
DEL ..\..\Deploy\HelpLibraryManagerLauncher.xml
DEL ..\..\Deploy\NHunSpell.xml
DEL ..\..\Deploy\SandcastleBuilderGUI.xml
DEL ..\..\Deploy\SandcastleHtmlExtract.xml

"%WIX%\bin\heat.exe" dir ..\..\Deploy\ -out Temp\DeploymentFiles.wxs -cg Data -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2 -sreg -srd
