# Synchronizes latest build outputs to the AutoCAD ApplicationPlugins bundle directory
$bundleDir = "$env:APPDATA\Autodesk\ApplicationPlugins\Rhino.Inside.AutoCAD.bundle\1.3.1\Win64\NET48"
$sourceDirs = @(
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Worker\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Interop\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Core\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Services\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.UI.Resources\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.GrasshopperLibrary\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Civil.Interop\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary\bin\Release\net48"
)

if (!(Test-Path $bundleDir)) {
    New-Item -ItemType Directory -Force -Path $bundleDir | Out-Null
}

foreach ($dir in $sourceDirs) {
    if (Test-Path $dir) {
        Copy-Item -Path "$dir\*" -Destination $bundleDir -Force -ErrorAction SilentlyContinue
    }
}
Write-Host "Bundle deployment synchronized successfully to: $bundleDir"
