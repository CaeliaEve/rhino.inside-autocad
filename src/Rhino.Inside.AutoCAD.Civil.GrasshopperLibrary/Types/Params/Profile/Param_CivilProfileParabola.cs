using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile parabola entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileParabola"/> objects which
/// contain parabola (vertical curve) entity data from Profiles.
/// </remarks>
public class Param_CivilProfileParabola : GH_Param<GH_CivilProfileParabola>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C6D7E8F9-A0B1-2345-C456-789012345F01");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilProfileParabola;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileParabola"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileParabola(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileParabola"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileParabola(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileParabola"/> class.
    /// </summary>
    public Param_CivilProfileParabola(GH_ParamAccess access)
        : base("Civil3d Profile Parabola", "ProfileParabola",
            "A parabola (vertical curve) entity from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
