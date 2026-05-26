using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D surface boundaries.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSurfaceBoundary"/> objects which
/// contain boundary definitions extracted from TIN surfaces.
/// </remarks>
public class Param_CivilSurfaceBoundary : GH_Param<GH_CivilSurfaceBoundary>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A7B8C9D0-1E2F-3A4B-5C6D-7E8F9A0B1C2D");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSurfaceBoundary;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBoundary"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSurfaceBoundary(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBoundary"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSurfaceBoundary(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBoundary"/> class.
    /// </summary>
    public Param_CivilSurfaceBoundary(GH_ParamAccess access)
        : base("Civil3d Surface Boundary", "Boundary",
            "A boundary definition from a Civil 3D TIN Surface", "Params", "Civil3d", access)
    { }
}
