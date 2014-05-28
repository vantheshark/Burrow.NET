del Burrow.*.nupkg

SETLOCAL
SET VERSION=1.0.25

nuget pack Burrow\Package.nuspec -Version %VERSION%
nuget pack Burrow.Extras\Package.nuspec -Version %VERSION%
nuget pack Burrow.RPC\Package.nuspec -Version %VERSION%
ENDLOCAL
pause