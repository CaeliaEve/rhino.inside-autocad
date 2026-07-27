using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// A constant class for interop-related values.
/// </summary>
public class InteropConstants
{

    /// <summary>
    /// The default AutoCAD layer name. 
    /// </summary>
    public const string DefaultLayerName = "0";

    /// <summary>
    /// The internal unit system used by the applications.
    /// </summary>
    public const UnitSystem FallbackUnitSystem = UnitSystem.Millimeters;

    /// <summary>
    /// The internal name of the application.
    /// </summary>
    public const string ApplicationName = "RHINO.INSIDE.AUTOCAD";

    /// <summary>
    /// The length in <see cref="IUnitConverter.RhinoUnits"/> of a pattern point
    /// in a <see cref="IAutocadLinetypeTableRecord"/> that is 0-length. The length is used to represent
    /// the point as a line internally.
    /// </summary>
    public const double LinePatternPointLength = 0.1;

    /// <summary>
    /// The file name of the Grasshopper library DLL.
    /// </summary>
    public const string GrasshopperLibraryFileName = "Rhino.Inside.AutoCAD.GrasshopperLibrary.dll";

    /// <summary>
    /// The fully qualified type name for the GH_AutocadGeometricGoo generic base type.
    /// </summary>
    public const string GooBaseTypeName = "Rhino.Inside.AutoCAD.GrasshopperLibrary.GH_AutocadGeometricGoo`2, Rhino.Inside.AutoCAD.GrasshopperLibrary";

/// <summary>
    /// The file name of the Civil3d Grasshopper library DLL.
    /// </summary>
    public const string GrasshopperCivilLibraryFileName = "Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary.dll";

    /// <summary>
    /// The name of the private Grasshopper method which registers a component library with
    /// the component server.
    /// </summary>
    /// <remarks>
    /// Value: "LoadGHA", on <c>Grasshopper.Kernel.GH_ComponentServer</c>. Not part of the
    /// public Grasshopper SDK, so it is resolved by reflection and matched on its parameter
    /// shape rather than assumed to keep one signature across Rhino versions.
    /// </remarks>
    /// <seealso cref="GrasshopperLibraryFileName"/>
    public const string LoadGhaMethodName = "LoadGHA";

    /// <summary>
    /// The file extension Grasshopper recognises as a component assembly.
    /// </summary>
    /// <remarks>
    /// Value: ".gha". Grasshopper derives <c>GH_ExternalFile.FileType</c> from the
    /// extension, and Rhino 9 refuses to register a file it does not classify as an
    /// assembly. Rhino 8 accepted a ".dll" as well; Rhino 9 does not.
    /// </remarks>
    /// <seealso cref="GrasshopperLibrariesFolderName"/>
    public const string GrasshopperAssemblyExtension = ".gha";

    /// <summary>
    /// The folder, beneath the user application data folder, holding the ".gha" mirrors of
    /// the component libraries.
    /// </summary>
    /// <remarks>
    /// Value: "Libraries". The mirrors live here rather than beside the originals because
    /// the installation folder is not reliably writable, and they are what gets handed to
    /// Grasshopper for registration.
    /// </remarks>
    /// <seealso cref="GrasshopperAssemblyExtension"/>
    public const string GrasshopperLibrariesFolderName = "Libraries";


    /// <summary>
    /// The temporary directory path used for file-based conversions. This directory is
    /// created in the system's temporary folder and is specific to the
    /// Rhino.Inside.AutoCAD application to avoid conflicts with other applications.
    /// It is used to store intermediate files during conversion processes, such as
    /// DXF files when converting AutoCAD geometry to Rhino geometry.
    /// </summary>
    public static readonly string TempDirectory = Path.Combine(
        System.IO.Path.GetTempPath(),
        "RhinoInsideAutocad",
        "Converters");

    /// <summary>
    /// The file path for the temporary DXF file used in conversions between AutoCAD
    /// and Rhino geometry. This file is created in the <see cref="TempDirectory"/>
    /// and is used to store intermediate geometry data during conversion processes.
    /// The file is typically deleted after the conversion is complete to avoid
    /// cluttering the temporary directory.
    /// </summary>
    public static readonly string DxfFilePath = Path.Combine(
        TempDirectory,
        "autoCadToRhino.dxf");

    /// <summary>
    /// The DXF version used for file-based conversions. 
    /// </summary>
    public const DwgVersion DxfVersion = DwgVersion.AC1024;

    /// <summary>
    /// The precision used when writing and reading DXF files for geometry conversion.
    /// </summary>
    public const int DxfPrecision = 16;

    /// <summary>
    /// The folder name used to store Rhino.Inside.AutoCAD-specific files.
    /// </summary>
    public const string RhinoInsideAutocadFolder = "RhinoInsideAutocad";

    /// <summary>
    /// The folder name used to store converter-related files.
    /// </summary>
    public const string ConvertersFolder = "Converters";

    /// <summary>
    /// The file name of the temporary Rhino file used during conversions.
    /// </summary>
    public const string RhinoFileName = "rhinoToAutoCad.3dm";

    /// <summary>
    /// The AutoCAD command used to import geometry from external files.
    /// </summary>
    public const string ImportCommand = "._IMPORT";

    /// <summary>
    /// The error message displayed when a Brep conversion fails.
    /// </summary>
    public const string BrepConversionErrorMessage = "\nError Converting brep: ";
}
