using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D TIN Surface.
/// </summary>
/// <param name="PropertiesGoo">The properties of the TIN Surface.</param>
/// <param name="BoundariesGoo">The boundary definitions of the Surface.</param>
/// <param name="ContoursGoo">The contour lines of the Surface.</param>
/// <param name="BreaklinesGoo">The breakline definitions of the Surface.</param>
/// <param name="Mesh">The surface as a Rhino mesh.</param>
public record TINSurfaceGooResult(
    GH_CivilTinProperties? PropertiesGoo,
    List<GH_CivilSurfaceBoundary>? BoundariesGoo,
    List<GH_CivilSurfaceContour>? ContoursGoo,
    List<GH_CivilSurfaceBreakline>? BreaklinesGoo,
    Mesh? Mesh) : GooResultBase
{
    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static TINSurfaceGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
