@ECHO OFF

REM Get rid of these as we don't care to include them and it saves us excluding them manually
DEL /S /Q ..\SHFB\Deploy\*.pdb > NUL 2> NUL
DEL /S /Q ..\SHFB\Deploy\*vshost* > NUL 2> NUL
DEL ..\SHFB\Deploy\ESent*.xml > NUL 2> NUL
DEL ..\SHFB\Deploy\GenerateInheritedDocs.xml > NUL 2> NUL
DEL ..\SHFB\Deploy\HelpLibraryManagerLauncher.xml > NUL 2> NUL
DEL ..\SHFB\Deploy\NHunSpell.xml > NUL 2> NUL
DEL ..\SHFB\Deploy\SandcastleBuilderGUI.xml > NUL 2> NUL
DEL ..\SHFB\Deploy\SandcastleHtmlExtract.xml > NUL 2> NUL
DEL /S /Q ..\SHFB\Deploy\*CodeAnalysis* > NUL 2> NUL
DEL ..\SHFB\Deploy\reflection.org > NUL 2> NUL

..\SHFB\Source\.nuget\NuGet Pack SHFB.nuspec -NoPackageAnalysis -OutputDirectory ..\Deployment
