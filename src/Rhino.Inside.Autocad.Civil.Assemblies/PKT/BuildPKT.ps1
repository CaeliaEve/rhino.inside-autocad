# BuildPKT.ps1
# Creates the ImportedPolylineSubassemblies.pkt file from the compiled DLL

param(
    [Parameter(Mandatory=$true)]
    [string]$OutputDir,

    [Parameter(Mandatory=$true)]
    [string]$ProjectDir
)

try {
    # Remove trailing backslashes that can cause issues
    $OutputDir = $OutputDir.TrimEnd('\', '/')
    $ProjectDir = $ProjectDir.TrimEnd('\', '/')

    # Convert relative OutputDir to absolute path
    if (-not [System.IO.Path]::IsPathRooted($OutputDir)) {
        $OutputDir = Join-Path $ProjectDir $OutputDir
    }

    $pktFolder = Join-Path $ProjectDir "PKT"
    $tempFolder = Join-Path $pktFolder "temp"
    $outputPkt = Join-Path $ProjectDir "ImportedPolylineSubassemblies.pkt"

    Write-Host "Building PKT file..."
    Write-Host "  OutputDir: $OutputDir"
    Write-Host "  ProjectDir: $ProjectDir"
    Write-Host "  PKT Folder: $pktFolder"

    # Clean up temp folder if it exists
    if (Test-Path $tempFolder) {
        Write-Host "  Cleaning temp folder..."
        Remove-Item $tempFolder -Recurse -Force
    }

    Write-Host "  Creating temp folder..."
    New-Item -ItemType Directory -Path $tempFolder -Force | Out-Null

    # Copy the DLL (renamed to match the subassembly name)
    $sourceDll = Join-Path $OutputDir "Rhino.Inside.Autocad.Civil.Assemblies.dll"
    $destDll = Join-Path $tempFolder "ImportedPolylineSubassemblies.dll"

    Write-Host "  Looking for DLL at: $sourceDll"

    if (-not (Test-Path $sourceDll)) {
        Write-Host "ERROR: Source DLL not found: $sourceDll"
        Write-Host "Contents of OutputDir:"
        if (Test-Path $OutputDir) {
            Get-ChildItem $OutputDir | ForEach-Object { Write-Host "    $_" }
        } else {
            Write-Host "    (directory does not exist)"
        }
        exit 1
    }

    Write-Host "  Copying DLL..."
    Copy-Item $sourceDll $destDll -Force

    # Copy the XML manifest
    $sourceXml = Join-Path $pktFolder "Subassembly.xml"
    $destXml = Join-Path $tempFolder "Subassembly.xml"

    Write-Host "  Looking for XML at: $sourceXml"

    if (-not (Test-Path $sourceXml)) {
        Write-Host "ERROR: Subassembly.xml not found: $sourceXml"
        exit 1
    }

    Write-Host "  Copying XML manifest..."
    Copy-Item $sourceXml $destXml -Force

    # Create the PKT (ZIP) file
    $tempZip = Join-Path $pktFolder "ImportedPolylineSubassemblies.zip"

    if (Test-Path $tempZip) {
        Remove-Item $tempZip -Force
    }
    if (Test-Path $outputPkt) {
        Remove-Item $outputPkt -Force
    }

    Write-Host "  Creating PKT archive..."
    Compress-Archive -Path "$tempFolder\*" -DestinationPath $tempZip -Force

    # Rename to .pkt
    Write-Host "  Renaming to PKT..."
    Move-Item $tempZip $outputPkt -Force

    # Also copy to output directory so it gets deployed
    $outputPktCopy = Join-Path $OutputDir "ImportedPolylineSubassemblies.pkt"
    Write-Host "  Copying PKT to output directory..."
    Copy-Item $outputPkt $outputPktCopy -Force

    # Clean up temp folder
    Write-Host "  Cleaning up..."
    Remove-Item $tempFolder -Recurse -Force

    Write-Host "SUCCESS: PKT file created at: $outputPkt"
    Write-Host "SUCCESS: PKT file copied to: $outputPktCopy"
    exit 0
}
catch {
    Write-Host "ERROR: $($_.Exception.Message)"
    Write-Host "Stack trace: $($_.ScriptStackTrace)"
    exit 1
}
