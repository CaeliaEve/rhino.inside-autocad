using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Corridor surfaces.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilCorridorSurface"/> objects which
/// contain data from Corridor surfaces.
/// </remarks>
public class Param_CivilCorridorSurface : GH_Param<GH_CivilCorridorSurface>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A7B8C9D0-E1F2-3456-0123-789012345678");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorSurface"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilCorridorSurface(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorSurface"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilCorridorSurface(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorSurface"/> class.
    /// </summary>
    public Param_CivilCorridorSurface(GH_ParamAccess access)
        : base("Civil3d Corridor Surface", "CorrSrf",
            "A surface from a Civil 3D Corridor", "Params", "Civil3d", access)
    { }
}
