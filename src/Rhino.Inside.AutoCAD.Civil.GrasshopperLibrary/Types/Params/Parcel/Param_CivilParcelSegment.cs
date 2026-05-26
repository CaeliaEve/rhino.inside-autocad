using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Parcel segments.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilParcelSegment"/> objects which
/// contain individual parcel boundary segment data (Lines, Arcs).
/// </remarks>
public class Param_CivilParcelSegment : GH_Param<GH_CivilParcelSegment>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("D4E5F6A7-B8C9-0123-DEF0-456789012345");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilParcelSegment;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelSegment"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilParcelSegment(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelSegment"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilParcelSegment(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilParcelSegment"/> class.
    /// </summary>
    public Param_CivilParcelSegment(GH_ParamAccess access)
        : base("Civil3d Parcel Segment", "ParcelSeg",
            "A boundary segment (Line, Arc) from a Civil 3D Parcel", "Params", "Civil3d", access)
    { }
}
