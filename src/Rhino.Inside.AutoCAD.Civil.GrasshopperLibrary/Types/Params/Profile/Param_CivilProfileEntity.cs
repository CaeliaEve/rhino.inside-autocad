using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Profile entities.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilProfileEntity"/> objects which
/// contain individual profile entity data (Tangents, CircularArcs, Parabolas).
/// </remarks>
public class Param_CivilProfileEntity : GH_Param<GH_CivilProfileEntity>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("F3A4B5C6-D7E8-9012-F123-456789012CDE");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileEntity"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilProfileEntity(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileEntity"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilProfileEntity(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilProfileEntity"/> class.
    /// </summary>
    public Param_CivilProfileEntity(GH_ParamAccess access)
        : base("Civil3d Profile Entity", "ProfileEntity",
            "An entity (Tangent, CircularArc, Parabola) from a Civil 3D Profile", "Params", "Civil3d", access)
    { }
}
