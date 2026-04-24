using System.IO;
using System.IO.Compression;
using System.Xml;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Installs custom subassemblies into Civil 3D's tool palette system.
/// This is required before ImportSubassembly API can find the subassembly.
/// </summary>
public static class SubassemblyToolPaletteInstaller
{
    private const string PaletteName = "Rhino Inside Subassemblies";
    private const string PaletteFileName = "RhinoInsideSubassemblies.atc";
    private const string PaletteGuid = "{B1M0RPH1-C1V1-4SSM-8LY1-P0LYL1N31MPT}";

    private static bool _isInstalled;
    private static string? _installedAtcPath;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the path to the installed ATC file in the tool palette, or null if not installed.
    /// </summary>
    public static string? InstalledAtcPath => _installedAtcPath;

    /// <summary>
    /// Ensures the subassembly is installed in Civil 3D's tool palette.
    /// </summary>
    /// <param name="dllPath">Full path to the subassembly DLL.</param>
    /// <returns>True if installation succeeded or was already done.</returns>
    public static bool EnsureInstalled(string dllPath)
    {
        lock (_lock)
        {
            if (_isInstalled) return true;

            try
            {
                // Find Civil 3D tool palette directory
                var toolPaletteDir = FindToolPaletteDirectory();
                if (string.IsNullOrEmpty(toolPaletteDir))
                {
                    Log("ERROR: Could not find Civil 3D tool palette directory");
                    return false;
                }

                Log($"Tool palette directory: {toolPaletteDir}");

                var palettesDir = Path.Combine(toolPaletteDir, "Palettes");
                if (!Directory.Exists(palettesDir))
                {
                    Directory.CreateDirectory(palettesDir);
                }

                // Copy DLL to palettes directory
                var targetDllPath = Path.Combine(palettesDir, Path.GetFileName(dllPath));
                if (!File.Exists(targetDllPath) || File.GetLastWriteTime(dllPath) > File.GetLastWriteTime(targetDllPath))
                {
                    File.Copy(dllPath, targetDllPath, true);
                    Log($"Copied DLL to: {targetDllPath}");
                }

                // Create palette ATC file
                var paletteAtcPath = Path.Combine(palettesDir, PaletteFileName);
                CreatePaletteAtc(paletteAtcPath, Path.GetFileName(targetDllPath));
                Log($"Created palette ATC: {paletteAtcPath}");

                // Register palette in main catalog
                var mainCatalogPath = Path.Combine(toolPaletteDir, "AeccTpCatalog.atc");
                if (File.Exists(mainCatalogPath))
                {
                    RegisterPaletteInCatalog(mainCatalogPath, PaletteFileName);
                    Log($"Registered palette in: {mainCatalogPath}");
                }
                else
                {
                    Log($"WARNING: Main catalog not found: {mainCatalogPath}");
                }

                _isInstalled = true;
                _installedAtcPath = paletteAtcPath;
                Log("Subassembly tool palette installation complete");

                // Try to refresh tool palettes by toggling them
                RefreshToolPalettes();

                return true;
            }
            catch (Exception ex)
            {
                Log($"ERROR installing tool palette: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Finds the Civil 3D tool palette directory for the current user.
    /// </summary>
    private static string? FindToolPaletteDirectory()
    {
        // Try common locations
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Civil 3D 2026
        var c3d2026 = Path.Combine(appData, "Autodesk", "C3D 2026", "eng", "Support", "ToolPalette");
        if (Directory.Exists(c3d2026)) return c3d2026;

        // Civil 3D 2025
        var c3d2025 = Path.Combine(appData, "Autodesk", "C3D 2025", "eng", "Support", "ToolPalette");
        if (Directory.Exists(c3d2025)) return c3d2025;

        // Try to find any C3D version
        var autodeskDir = Path.Combine(appData, "Autodesk");
        if (Directory.Exists(autodeskDir))
        {
            foreach (var dir in Directory.GetDirectories(autodeskDir, "C3D *"))
            {
                var toolPalette = Path.Combine(dir, "eng", "Support", "ToolPalette");
                if (Directory.Exists(toolPalette)) return toolPalette;

                // Try other language codes
                foreach (var langDir in Directory.GetDirectories(dir))
                {
                    toolPalette = Path.Combine(langDir, "Support", "ToolPalette");
                    if (Directory.Exists(toolPalette)) return toolPalette;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Creates the palette ATC file for our subassembly.
    /// </summary>
    private static void CreatePaletteAtc(string path, string dllFileName)
    {
        var atcContent = $@"<Palette>
	<ItemID idValue=""{PaletteGuid}""/>
	<Properties>
		<ItemName>{PaletteName}</ItemName>
		<Images/>
		<Time createdUniversalDateTime=""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}"" modifiedUniversalDateTime=""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}""/>
	</Properties>
	<Source/>
	<Tools>
		<Tool>
			<ItemID idValue=""{CivilConstants.ImportedPolylineSubassemblyToolId}""/>
			<Properties>
				<ItemName>Imported Polyline Subassembly</ItemName>
				<Images/>
				<Description>Imports polyline geometry from Rhino/Grasshopper</Description>
				<Keywords>_importedpolyline subassembly rhino grasshopper</Keywords>
				<Time createdUniversalDateTime=""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}"" modifiedUniversalDateTime=""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}""/>
			</Properties>
			<Source/>
			<StockToolRef idValue=""{{7F55AAC0-0256-48D7-BFA5-914702663FDE}}""/>
			<Data>
				<AeccDbSubassembly>
					<GeometryGenerateMode>UseDotNet</GeometryGenerateMode>
					<DotNetClass Assembly="".\{dllFileName}"">Subassembly.ImportedPolylineSubassemblies</DotNetClass>
					<Params>
						<CsvPath DataType=""String"" DisplayName=""CSV File Path"" Description=""Path to the CSV file containing offset,elevation pairs""></CsvPath>
						<PointCode DataType=""String"" DisplayName=""Point Code"" Description=""Code to assign to all points"">Shape</PointCode>
						<LinkCode DataType=""String"" DisplayName=""Link Code"" Description=""Code to assign to all links"">Top</LinkCode>
						<Closed DataType=""Long"" DisplayName=""Close Shape"" Description=""Set to 1 to close the shape"">0
							<Enum>
								<No DisplayName=""No"">0</No>
								<Yes DisplayName=""Yes"">1</Yes>
							</Enum>
						</Closed>
					</Params>
				</AeccDbSubassembly>
				<Units>foot</Units>
			</Data>
		</Tool>
	</Tools>
</Palette>";

        File.WriteAllText(path, atcContent);
    }

    /// <summary>
    /// Registers our palette in the main Civil 3D catalog.
    /// </summary>
    private static void RegisterPaletteInCatalog(string catalogPath, string paletteFileName)
    {
        var doc = new XmlDocument();
        doc.Load(catalogPath);

        // Check if our palette is already registered
        var palettesNode = doc.SelectSingleNode("//Palettes");
        if (palettesNode == null)
        {
            Log("WARNING: Palettes node not found in catalog");
            return;
        }

        // Check if already registered
        var existingPalette = doc.SelectSingleNode($"//Palette[Url/@href='Palettes\\{paletteFileName}']");
        if (existingPalette != null)
        {
            Log("Palette already registered in catalog");
            return;
        }

        // Create new palette entry
        var paletteElement = doc.CreateElement("Palette");

        var itemIdElement = doc.CreateElement("ItemID");
        itemIdElement.SetAttribute("idValue", PaletteGuid);
        paletteElement.AppendChild(itemIdElement);

        var urlElement = doc.CreateElement("Url");
        urlElement.SetAttribute("href", $"Palettes\\{paletteFileName}");
        paletteElement.AppendChild(urlElement);

        var propsElement = doc.CreateElement("Properties");
        var itemNameElement = doc.CreateElement("ItemName");
        itemNameElement.InnerText = PaletteName;
        propsElement.AppendChild(itemNameElement);
        var imagesElement = doc.CreateElement("Images");
        propsElement.AppendChild(imagesElement);
        paletteElement.AppendChild(propsElement);

        var sourceElement = doc.CreateElement("Source");
        paletteElement.AppendChild(sourceElement);

        palettesNode.AppendChild(paletteElement);

        // Save with backup
        var backupPath = catalogPath + ".bak";
        if (!File.Exists(backupPath))
        {
            File.Copy(catalogPath, backupPath);
        }

        doc.Save(catalogPath);
    }

    /// <summary>
    /// Attempts to refresh tool palettes by sending commands to AutoCAD.
    /// This may help Civil 3D recognize newly installed palettes without restart.
    /// </summary>
    private static void RefreshToolPalettes()
    {
        try
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Log("Attempting to refresh tool palettes...");

            // Try using SendStringToExecute to toggle tool palettes
            // TOOLPALETTESCLOSE followed by TOOLPALETTES might refresh the cache
            doc.SendStringToExecute("TOOLPALETTESCLOSE ", true, false, false);
            doc.SendStringToExecute("TOOLPALETTES ", true, false, false);

            Log("Sent tool palette refresh commands");
        }
        catch (Exception ex)
        {
            Log($"Tool palette refresh failed (non-critical): {ex.Message}");
        }
    }

    private static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[ToolPaletteInstaller] {message}");
        try
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\n[ToolPaletteInstaller] {message}");
        }
        catch { }
    }
}
