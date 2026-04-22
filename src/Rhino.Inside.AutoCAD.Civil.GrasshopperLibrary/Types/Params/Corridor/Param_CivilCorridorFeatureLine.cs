using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Corridor feature lines.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilCorridorFeatureLine"/> objects which
/// contain data from Corridor feature lines.
/// </remarks>
public class Param_CivilCorridorFeatureLine : GH_Param<GH_CivilCorridorFeatureLine>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B8C9D0E1-F2A3-4567-1234-890123456789");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilCorridorFeatureLine;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorFeatureLine"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilCorridorFeatureLine(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorFeatureLine"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilCorridorFeatureLine(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorFeatureLine"/> class.
    /// </summary>
    public Param_CivilCorridorFeatureLine(GH_ParamAccess access)
        : base("Civil3d Corridor Feature Line", "CorrFL",
            "A feature line from a Civil 3D Corridor", "Params", "Civil3d", access)
    { }
}
