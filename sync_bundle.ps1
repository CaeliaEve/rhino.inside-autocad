# Synchronizes latest build outputs, toolbars, and icons to the AutoCAD ApplicationPlugins bundle directory
$bundleRoot = "$env:APPDATA\Autodesk\ApplicationPlugins\Rhino.Inside.AutoCAD.bundle"
$bundleNet48Dir = "$bundleRoot\1.3.1\Win64\NET48"
$bundleToolbarDir = "$bundleRoot\Toolbar"
$bundleIconsDir = "$bundleRoot\Icons"

$sourceDirs = @(
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Interop\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Core\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Services\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.UI.Resources\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.GrasshopperLibrary\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Civil.Interop\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary\bin\Release\net48"
)

# Ensure directories exist
New-Item -ItemType Directory -Force -Path $bundleNet48Dir | Out-Null
New-Item -ItemType Directory -Force -Path $bundleToolbarDir | Out-Null
New-Item -ItemType Directory -Force -Path $bundleIconsDir | Out-Null

# Copy assemblies
foreach ($dir in $sourceDirs) {
    if (Test-Path $dir) {
        Copy-Item -Path "$dir\*" -Destination $bundleNet48Dir -Force -ErrorAction SilentlyContinue
    }
}

# Copy PackageContents.xml
Copy-Item -Path "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\PackageContents.xml" -Destination $bundleRoot -Force -ErrorAction SilentlyContinue

# Copy Toolbars (CUIX)
Copy-Item -Path "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\Toolbar\*" -Destination $bundleToolbarDir -Recurse -Force -ErrorAction SilentlyContinue

# Copy Icons
Copy-Item -Path "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Applications\Icons\*" -Destination $bundleIconsDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Full bundle deployment synchronized successfully to: $bundleRoot"
