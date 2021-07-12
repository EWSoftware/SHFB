@ECHO OFF
REM This can be used to regenerate new copies of the WiX files to see if anything needs adding or removing

REM Get rid of these as we don't want to include them
DEL /S /Q ..\..\Deploy\*.pdb
DEL /S /Q ..\..\Deploy\Tools\*vshost*
DEL /S /Q ..\..\Deploy\Components\ESent*.xml
DEL /S /Q ..\..\Deploy\Components\Microsoft*.xml
DEL /S /Q ..\..\Deploy\Components\System*.xml
DEL /S /Q ..\..\Deploy\Tools\Microsoft*.xml
DEL /S /Q ..\..\Deploy\Tools\NHunSpell.xml
DEL /S /Q ..\..\Deploy\Tools\SandcastleBuilderGUI.xml
DEL /S /Q ..\..\Deploy\Tools\System*.xml
DEL /S /Q ..\..\Deploy\reflection.org

"%WIX%\bin\heat.exe" dir ..\..\Deploy\ -out Temp\DeploymentFiles.wxs -cg Data -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2 -sreg -srd
