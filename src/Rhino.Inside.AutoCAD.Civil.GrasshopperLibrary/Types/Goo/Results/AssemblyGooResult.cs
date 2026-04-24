using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Assembly.
/// </summary>
/// <param name="Name">The name of the Assembly.</param>
/// <param name="Description">The description of the Assembly.</param>
/// <param name="AssemblyType">The type of the Assembly.</param>
/// <param name="Code">The code name of the Assembly.</param>
/// <param name="Style">The style applied to the Assembly.</param>
/// <param name="SubAssembliesGoo">A list of subassemblies in the Assembly.</param>
/// <param name="Location">The origin location of the Assembly.</param>
/// <param name="AllGeometry">The combined geometry from all subassemblies as curves.</param>
public record AssemblyGooResult(
    string? Name,
    string? Description,
    CivilAssemblyType? AssemblyType,
    string? Code,
    INamedId? Style,
    List<GH_CivilSubassembly>? SubAssembliesGoo,
    Point3d? Location,
    List<Curve>? AllGeometry) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static AssemblyGooResult Failed => new(null, null, null, null, null, null, null, null) { IsSuccess = false };
}
