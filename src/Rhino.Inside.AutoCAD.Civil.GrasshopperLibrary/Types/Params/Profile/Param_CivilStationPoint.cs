using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D station points.
/// </summary>
public class Param_CivilStationPoint : GH_Param<GH_CivilStationPoint>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F01234567893");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap? Icon => null;

    /// <summary>
    /// Initializes a new instance with the specified instance description.
    /// </summary>
    public Param_CivilStationPoint(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified instance description and access type.
    /// </summary>
    public Param_CivilStationPoint(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified access type.
    /// </summary>
    public Param_CivilStationPoint(GH_ParamAccess access)
        : base("Civil3d Station Point", "StaPt",
            "A station value with its corresponding elevation", "Params", "Civil3d", access)
    {
    }
}
