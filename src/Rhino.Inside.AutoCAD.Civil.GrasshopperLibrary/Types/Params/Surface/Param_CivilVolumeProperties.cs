using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D volume properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilVolumeProperties"/> objects which
/// contain volume statistics from TIN Volume Surfaces.
/// </remarks>
public class Param_CivilVolumeProperties : GH_Param<GH_CivilVolumeProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilVolumeProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilVolumeProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilVolumeProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilVolumeProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilVolumeProperties"/> class.
    /// </summary>
    public Param_CivilVolumeProperties(GH_ParamAccess access)
        : base("Civil3d Volume Properties", "VolProps",
            "Volume statistics from a Civil 3D TIN Volume Surface", "Params", "Civil3d", access)
    { }
}
