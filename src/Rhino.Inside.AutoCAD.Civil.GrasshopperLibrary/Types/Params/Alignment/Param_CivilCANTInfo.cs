using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Alignment CANT information.
/// </summary>
public class Param_CivilCANTInfo : GH_Param<GH_CivilCANTInfo>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF0123456783");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance with the specified instance description.
    /// </summary>
    public Param_CivilCANTInfo(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified instance description and access type.
    /// </summary>
    public Param_CivilCANTInfo(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified access type.
    /// </summary>
    public Param_CivilCANTInfo(GH_ParamAccess access)
        : base("Civil3d CANT Info", "CANT",
            "CANT (superelevation) information from a Civil 3D Alignment", "Params", "Civil3d", access)
    {
    }
}
