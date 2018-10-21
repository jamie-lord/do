$version = Read-Host 'Enter version number for release'
$version = 'v' + $version

$doFolder = "C:\Users\Jamie\Projects\do\Do"
$releaseFolder = "C:\Users\Jamie\Projects\do-prod"
$buildFolder = Join-Path -Path $doFolder -ChildPath "bin\Release\netstandard2.0\publish\Do\dist"

Set-Location $doFolder

Write-Host "Deleting existing builds"
$bin = Join-Path -Path $doFolder -ChildPath "bin"
$obj = Join-Path -Path $doFolder -ChildPath "obj"
Remove-Item –path $bin –recurse
Remove-Item –path $obj –recurse

Write-Host "Starting release build"
dotnet publish -c Release

Write-Host "Adding GitHub single page script to index.html"
Set-Location ..
$script = Get-Content -Path .\GitHubPagesScript.txt
$script = $script + "`n</head>"
Set-Location $buildFolder
(Get-Content -Path .\index.html) -replace "</head>", $script | Set-Content -Path .\index.html

Write-Host "Updating release from GitHub"
Set-Location $releaseFolder
# git pull --ff-only

Write-Host "Deleting existing release files"
Get-ChildItem -Path $releaseFolder -Include * -File -Recurse | ForEach-Object { $_.Delete()}

Get-ChildItem -Path $buildFolder | Copy-Item -Destination $releaseFolder -Recurse -Container