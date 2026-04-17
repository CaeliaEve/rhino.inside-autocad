using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Parcel properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilParcelProperties"/> objects which
/// contain properties from Parcels.
/// </remarks>
public class Param_CivilParcelProperties : GH_Param<GH_CivilParcelProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-CDEF-345678901234");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilParcelProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilParcelProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelProperties"/> class.
    /// </summary>
    public Param_CivilParcelProperties(GH_ParamAccess access)
        : base("Civil3d Parcel Properties", "ParcelProps",
            "Properties from a Civil 3D Parcel", "Params", "Civil3d", access)
    { }
}
