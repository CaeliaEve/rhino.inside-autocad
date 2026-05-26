using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Corridor properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilCorridorProperties"/> objects which
/// contain properties from Corridors.
/// </remarks>
public class Param_CivilCorridorProperties : GH_Param<GH_CivilCorridorProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("D4E5F6A7-B8C9-0123-DEF0-456789012345");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilCorridorProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilCorridorProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilCorridorProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilCorridorProperties"/> class.
    /// </summary>
    public Param_CivilCorridorProperties(GH_ParamAccess access)
        : base("Civil3d Corridor Properties", "CorrProps",
            "Properties from a Civil 3D Corridor", "Params", "Civil3d", access)
    { }
}
