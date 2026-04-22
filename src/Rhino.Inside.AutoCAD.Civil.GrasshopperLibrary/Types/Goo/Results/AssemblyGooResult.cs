using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Assembly.
/// </summary>
/// <param name="PropertiesGoo">The properties of the Assembly.</param>
/// <param name="SubAssembliesGoo">A list of subassemblies in the Assembly.</param>
/// <param name="Location">The origin location of the Assembly.</param>
/// <param name="AllGeometry">The combined geometry from all subassemblies as curves.</param>
public record AssemblyGooResult(
    GH_CivilAssemblyProperties? PropertiesGoo,
    List<GH_CivilSubassemblyProperties>? SubAssembliesGoo,
    Point3d? Location,
    List<Curve>? AllGeometry) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static AssemblyGooResult Failed => new(null, null, null, null) { IsSuccess = false };
}
