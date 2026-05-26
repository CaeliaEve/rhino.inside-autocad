using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D TIN Surface.
/// </summary>
public class TINSurfaceGooResult : GooResultBase
{
    /// <summary>
    /// Gets the properties of the TIN Surface.
    /// </summary>
    public GH_CivilTinProperties? PropertiesGoo { get; }

    /// <summary>
    /// Gets the boundary definitions of the Surface.
    /// </summary>
    public List<GH_CivilSurfaceBoundary>? BoundariesGoo { get; }

    /// <summary>
    /// Gets the contour lines of the Surface.
    /// </summary>
    public List<GH_CivilSurfaceContour>? ContoursGoo { get; }

    /// <summary>
    /// Gets the breakline definitions of the Surface.
    /// </summary>
    public List<GH_CivilSurfaceBreakline>? BreaklinesGoo { get; }

    /// <summary>
    /// Gets the surface as a Rhino mesh.
    /// </summary>
    public Mesh? Mesh { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TINSurfaceGooResult"/> class.
    /// </summary>
    /// <param name="propertiesGoo">The properties of the TIN Surface.</param>
    /// <param name="boundariesGoo">The boundary definitions of the Surface.</param>
    /// <param name="contoursGoo">The contour lines of the Surface.</param>
    /// <param name="breaklinesGoo">The breakline definitions of the Surface.</param>
    /// <param name="mesh">The surface as a Rhino mesh.</param>
    public TINSurfaceGooResult(
        GH_CivilTinProperties? propertiesGoo,
        List<GH_CivilSurfaceBoundary>? boundariesGoo,
        List<GH_CivilSurfaceContour>? contoursGoo,
        List<GH_CivilSurfaceBreakline>? breaklinesGoo,
        Mesh? mesh)
    {
        PropertiesGoo = propertiesGoo;
        BoundariesGoo = boundariesGoo;
        ContoursGoo = contoursGoo;
        BreaklinesGoo = breaklinesGoo;
        Mesh = mesh;
    }

    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static TINSurfaceGooResult Failed => new(null, null, null, null, null) { IsSuccess = false };
}
