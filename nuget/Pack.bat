del Burrow.*.nupkg

SETLOCAL
SET VERSION=1.0.32
powershell -Command "(gc Burrow\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Burrow\Package.nuspec"
powershell -Command "(gc Burrow.Extras\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Burrow.Extras\Package.nuspec"
powershell -Command "(gc Burrow.RPC\Package.nuspec) -replace '__OLD_VERSION__', '%VERSION%' | Out-File -encoding ASCII Burrow.RPC\Package.nuspec"

C:\tools\nuget pack Burrow\Package.nuspec -Version %VERSION% -NonInteractive
C:\tools\nuget pack Burrow.Extras\Package.nuspec -Version %VERSION% -NonInteractive
C:\tools\nuget pack Burrow.RPC\Package.nuspec -Version %VERSION% -NonInteractive


git checkout Burrow\Package.nuspec
git checkout Burrow.Extras\Package.nuspec
git checkout Burrow.RPC\Package.nuspec
ENDLOCAL
pause