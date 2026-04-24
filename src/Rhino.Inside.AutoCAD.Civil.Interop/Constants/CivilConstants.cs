namespace Rhino.Inside.AutoCAD.Civil.Interop.Constants;

public class CivilConstants
{
    /// <summary>
    /// Represents the prefix used for unique TIN Surface names.
    /// </summary>
    public const string GhPrefix = "Gh_";

    /// <summary>
    /// Represents the prefix used for unique TIN Volume Surface names.
    /// </summary>
    public const string GhVolumeSurfacePrefix = "Gh_Vol_";

    /// <summary>
    /// Represents the prefix used for unique Subassembly names.
    /// </summary>
    public const string GhSubassemblyPrefix = "Gh_Sub_";

    /// <summary>
    /// The folder name for subassembly-related temporary files.
    /// </summary>
    public const string SubassemblyFolder = "Subassemblies";

    /// <summary>
    /// The file extension for subassembly CSV files.
    /// </summary>
    public const string SubassemblyCsvExtension = ".csv";

    /// <summary>
    /// The name of the ImportedPolylineSubassemblies class in the catalog.
    /// </summary>
    public const string ImportedPolylineSubassemblyName = "ImportedPolylineSubassemblies";

    /// <summary>
    /// The name of the subassembly catalog file (ATC format for Civil 3D ImportSubassembly).
    /// </summary>
    public const string SubassemblyCatalogFileName = "ImportedPolylineSubassemblies.atc";

    /// <summary>
    /// The tool item ID GUID for the ImportedPolylineSubassemblies.
    /// </summary>
    public const string ImportedPolylineSubassemblyToolId = "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}";
}