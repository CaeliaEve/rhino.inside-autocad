using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Corridor baseline regions.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilCorridorBaselineRegion"/> objects which
/// contain data from Corridor baseline regions.
/// </remarks>
public class Param_CivilCorridorBaselineRegion : GH_Param<GH_CivilCorridorBaselineRegion>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("F6A7B8C9-D0E1-2345-F012-678901234567");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorBaselineRegion"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilCorridorBaselineRegion(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorBaselineRegion"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilCorridorBaselineRegion(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorBaselineRegion"/> class.
    /// </summary>
    public Param_CivilCorridorBaselineRegion(GH_ParamAccess access)
        : base("Civil3d Corridor Baseline Region", "CorrRgn",
            "A baseline region from a Civil 3D Corridor", "Params", "Civil3d", access)
    { }
}
