using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D surface contours.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSurfaceContour"/> objects which
/// contain contour lines extracted from TIN surfaces.
/// </remarks>
public class Param_CivilSurfaceContour : GH_Param<GH_CivilSurfaceContour>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B8C9D0E1-2F3A-4B5C-6D7E-8F9A0B1C2D3E");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSurfaceContour;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceContour"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSurfaceContour(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceContour"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSurfaceContour(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceContour"/> class.
    /// </summary>
    public Param_CivilSurfaceContour(GH_ParamAccess access)
        : base("Civil3d Surface Contour", "Contour",
            "A contour line extracted from a Civil 3D TIN Surface", "Params", "Civil3d", access)
    { }
}
