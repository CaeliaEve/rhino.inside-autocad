using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D surface breaklines.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSurfaceBreakline"/> objects which
/// contain breakline definitions extracted from TIN surfaces.
/// </remarks>
public class Param_CivilSurfaceBreakline : GH_Param<GH_CivilSurfaceBreakline>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C9D0E1F2-3A4B-5C6D-7E8F-9A0B1C2D3E4F");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSurfaceBreakline;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBreakline"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSurfaceBreakline(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBreakline"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSurfaceBreakline(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBreakline"/> class.
    /// </summary>
    public Param_CivilSurfaceBreakline(GH_ParamAccess access)
        : base("Civil3d Surface Breakline", "Breakline",
            "A breakline definition extracted from a Civil 3D TIN Surface", "Params", "Civil3d", access)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSurfaceBreakline"/> class.
    /// </summary>
  /*  public Param_CivilSurfaceBreakline()
        : base("Civil3d Surface Breakline", "Breakline",
            "A breakline definition extracted from a Civil 3D TIN Surface", "Params", "Civil3d", GH_ParamAccess.item)
    { }*/
}
