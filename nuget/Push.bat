@echo OFF
@echo Publishing following 3 packs:
@echo:
DIR /B *.nupkg
@echo:
SETLOCAL
SET VERSION=1.0.32
pause
C:\tools\nuget push Burrow.NET.%VERSION%.nupkg -NonInteractive
C:\tools\nuget push Burrow.Extras.%VERSION%.nupkg -NonInteractive
C:\tools\nuget push Burrow.RPC.%VERSION%.nupkg -NonInteractive
pause
ENDLOCAL