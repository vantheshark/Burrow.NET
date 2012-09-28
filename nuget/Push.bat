@echo OFF
@echo Publishing following 3 packs:
@echo:
DIR /B *.nupkg
@echo:
SETLOCAL
SET VERSION=1.0.15
pause
nuget push Burrow.NET.%VERSION%.nupkg
nuget push Burrow.Extras.%VERSION%.nupkg
nuget push Burrow.RPC.%VERSION%.nupkg
pause
ENDLOCAL