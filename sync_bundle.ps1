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

# Deploy AutoCadPasteBridge.rhp to Rhino 7 Plug-ins directory and register in Rhino 7 Registry
$rhino7PluginDir = "$env:APPDATA\McNeel\Rhinoceros\7.0\Plug-ins\AutoCadPasteBridge"
$rhpSource = "e:\codex\rhino\src\Rhino.Inside.AutoCAD.PasteBridge\bin\Release\net48\AutoCadPasteBridge.rhp"
if (Test-Path $rhpSource) {
    New-Item -ItemType Directory -Force -Path $rhino7PluginDir | Out-Null
    Copy-Item -Path $rhpSource -Destination "$rhino7PluginDir\AutoCadPasteBridge.rhp" -Force -ErrorAction SilentlyContinue
    
    # Register in Rhino 7 Registry
    $regKey = "HKCU:\Software\McNeel\Rhinoceros\7.0\Plug-Ins\e5a2a388-99b4-4b2d-9bc8-4664f98e18f3"
    if (!(Test-Path $regKey)) {
        New-Item -Path $regKey -Force | Out-Null
    }
    Set-ItemProperty -Path $regKey -Name "FileName" -Value "$rhino7PluginDir\AutoCadPasteBridge.rhp"
    Set-ItemProperty -Path $regKey -Name "Name" -Value "AutoCadPasteBridge"
    Set-ItemProperty -Path $regKey -Name "LoadMode" -Value 1 -Type DWord
    Write-Host "AutoCadPasteBridge.rhp deployed and registered for Rhino 7 at: $rhino7PluginDir\AutoCadPasteBridge.rhp"

    # Clean up Rhino 8 Plug-ins to guarantee 100% pure Rhino 8 without AutoCadPasteBridge
    $rhino8PluginDir = "$env:APPDATA\McNeel\Rhinoceros\8.0\Plug-ins\AutoCadPasteBridge"
    if (Test-Path $rhino8PluginDir) {
        Remove-Item -Path $rhino8PluginDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    Remove-Item -Path "HKCU:\Software\McNeel\Rhinoceros\8.0\Plug-Ins\e5a2a388-99b4-4b2d-9bc8-4664f98e18f3" -Recurse -Force -ErrorAction SilentlyContinue
}

# Deploy Grasshopper GHA Libraries to Rhino 8 / Grasshopper Libraries folder
$ghLibDir = "$env:APPDATA\Grasshopper\Libraries\Rhino.Inside.AutoCAD"
New-Item -ItemType Directory -Force -Path $ghLibDir | Out-Null

$ghSources = @(
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.GrasshopperLibrary\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Core\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Interop\bin\Release\net48",
    "e:\codex\rhino\src\Rhino.Inside.AutoCAD.Services\bin\Release\net48"
)

foreach ($dir in $ghSources) {
    if (Test-Path $dir) {
        Copy-Item -Path "$dir\*.dll" -Destination $ghLibDir -Force -ErrorAction SilentlyContinue
    }
}

# Copy as .gha for Grasshopper native recognition
$ghaSource = "$ghLibDir\Rhino.Inside.AutoCAD.GrasshopperLibrary.dll"
if (Test-Path $ghaSource) {
    Copy-Item -Path $ghaSource -Destination "$ghLibDir\Rhino.Inside.AutoCAD.GrasshopperLibrary.gha" -Force -ErrorAction SilentlyContinue
}
$civilGhaSource = "$ghLibDir\Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary.dll"
if (Test-Path $civilGhaSource) {
    Copy-Item -Path $civilGhaSource -Destination "$ghLibDir\Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary.gha" -Force -ErrorAction SilentlyContinue
}
Write-Host "AutoCAD & Civil Grasshopper GHA battery suite deployed to: $ghLibDir"

Write-Host "Full bundle deployment synchronized successfully to: $bundleRoot"
