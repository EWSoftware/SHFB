@ECHO OFF
REM This can be used to regenerate new copies of the WiX files to see if anything needs adding

"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\Data -out Temp\Data.wxs -cg Data -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\Examples -out Temp\Examples.wxs -cg Examples -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\Presentation -out Temp\Presentation.wxs -cg Presentation -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\ProductionTools -out Temp\ProductionTools.wxs -cg ProductionTools -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\ProductionTransforms -out Temp\ProductionTransforms.wxs -cg ProductionTransforms -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\..\Main\Schemas -out Temp\Schemas.wxs -cg Schemas -gg -sfrag -template fragment -dr INSTALLDIR -var var.RootFolder -indent 2
"\Program Files (x86)\Windows Installer XML v3.5\bin\heat.exe" dir ..\LanguagePack -out Temp\LanguagePack.wxs -cg LanguagePack -gg -sfrag -template fragment -dr INSTALLDIR -var var.ExtrasFolder -indent 2
