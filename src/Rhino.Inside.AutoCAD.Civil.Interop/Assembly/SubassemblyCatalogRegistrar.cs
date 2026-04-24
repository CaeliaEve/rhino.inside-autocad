using Microsoft.Win32;
using Rhino.Inside.AutoCAD.Civil.Interop.Constants;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Registers subassembly catalogs in the Windows registry for Civil 3D Content Browser.
/// Civil 3D requires catalogs to be registered before they can be used with ImportSubassembly().
/// </summary>
public static class SubassemblyCatalogRegistrar
{
    private const string ContentBrowserKeyBase = @"Software\Autodesk\Autodesk Content Browser";
    private const string RegisteredCatalogsSubKey = "RegisteredCatalogs";
    private const string RhinoInsideCatalogName = "RhinoInsideSubassemblies";

    // Catalog GUID - must match ATC file's Palette ItemID
    private const string CatalogGuid = "{B1M0RPH1-C1V1-4SSM-8LY1-P0LYL1N31MPT}";

    // Standard subassembly group GUID (from C3DStockTools.atc)
    private const string SubassemblyGroupGuid = "{5BD79109-BC69-41eb-9AC8-7E9CD469C8D3}";

    private static bool _isRegistered;
    private static readonly object _lock = new();

    /// <summary>
    /// Ensures the subassembly catalog is registered in the Windows registry.
    /// This is idempotent - calling it multiple times has no additional effect.
    /// </summary>
    /// <param name="atcFilePath">The full path to the ATC catalog file.</param>
    public static void EnsureRegistered(string atcFilePath)
    {
        lock (_lock)
        {
            if (_isRegistered) return;

            // Find Content Browser version by scanning registry
            var contentBrowserVersion = FindContentBrowserVersion();
            if (string.IsNullOrEmpty(contentBrowserVersion))
            {
                // Log warning but don't fail - the import might still work
                System.Diagnostics.Debug.WriteLine("Could not find Content Browser version in registry");
                LogToEditor("WARNING: Could not find Content Browser version in registry");
                return;
            }

            var catalogsKeyPath = $@"{ContentBrowserKeyBase}\{contentBrowserVersion}\{RegisteredCatalogsSubKey}\{RhinoInsideCatalogName}";

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(catalogsKeyPath);
                if (key == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create registry key: {catalogsKeyPath}");
                    LogToEditor($"WARNING: Failed to create registry key: {catalogsKeyPath}");
                    return;
                }

                key.SetValue("ItemID", CatalogGuid);
                key.SetValue("Url", atcFilePath);
                key.SetValue("DisplayName", "Rhino Inside Subassemblies");
                key.SetValue("Description", "Custom subassemblies for Rhino Inside AutoCAD");
                key.SetValue("Publisher", "Bimorph Digital Engineering");
                key.SetValue("GroupType", SubassemblyGroupGuid);

                _isRegistered = true;
                System.Diagnostics.Debug.WriteLine($"Registered catalog at: {catalogsKeyPath}");
                LogToEditor($"Registered subassembly catalog at: {catalogsKeyPath}");
                LogToEditor($"Catalog file: {atcFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register catalog: {ex.Message}");
                LogToEditor($"WARNING: Failed to register catalog: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Finds the Content Browser version from the AutoCAD registry key.
    /// </summary>
    /// <returns>The version string (e.g., "26" for 2026), or null if not found.</returns>
    private static string? FindContentBrowserVersion()
    {
        try
        {
            // Get version from AutoCAD's registry key path
            // e.g., "Software\Autodesk\AutoCAD\R26.0\ACAD-8101:409" -> extract "26"
            var registryKey = Autodesk.AutoCAD.DatabaseServices.HostApplicationServices.Current.UserRegistryProductRootKey;
            System.Diagnostics.Debug.WriteLine($"AutoCAD registry key: {registryKey}");

            // Extract version number from path like "Software\Autodesk\AutoCAD\R26.0\..."
            var match = System.Text.RegularExpressions.Regex.Match(registryKey, @"R(\d+)\.");
            if (match.Success)
            {
                var version = match.Groups[1].Value;
                System.Diagnostics.Debug.WriteLine($"Extracted Content Browser version: {version}");
                return version;
            }

            // Fallback: scan existing Content Browser registry keys
            using var baseKey = Registry.CurrentUser.OpenSubKey(ContentBrowserKeyBase);
            if (baseKey == null)
            {
                System.Diagnostics.Debug.WriteLine($"Content Browser registry key not found: {ContentBrowserKeyBase}");
                return null;
            }

            var subKeys = baseKey.GetSubKeyNames();

            // Filter to reasonable version numbers (20-50 range for modern AutoCAD)
            var version2 = subKeys
                .Where(k => int.TryParse(k, out var v) && v >= 20 && v <= 50)
                .OrderByDescending(k => int.Parse(k))
                .FirstOrDefault();

            System.Diagnostics.Debug.WriteLine($"Found Content Browser version from scan: {version2}");
            return version2;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error finding Content Browser version: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Logs a message to the AutoCAD editor command line.
    /// </summary>
    private static void LogToEditor(string message)
    {
        try
        {
            var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?.Editor;
            ed?.WriteMessage($"\n{message}");
        }
        catch
        {
            // Ignore if editor is not available
        }
    }
}
