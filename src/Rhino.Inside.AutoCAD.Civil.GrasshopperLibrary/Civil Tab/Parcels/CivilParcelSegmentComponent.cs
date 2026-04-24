using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Parcel Segment.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilParcelSegmentComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B8C9D0E1-F2A3-4567-0123-890123456789");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilParcelSegmentComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilParcelSegmentComponent"/> class.
    /// </summary>
    public CivilParcelSegmentComponent()
        : base("Civil3d Parcel Segment", "CVL-ParcelSeg",
            "Extracts individual values from a Civil 3D Parcel Segment",
            "Civil3d", "Site/Parcels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilParcelSegment(GH_ParamAccess.item), "Segment",
            "Seg", "A parcel boundary segment from a Civil3d Parcel", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Type", "T",
            "The type of segment (Line, Arc).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The length of this segment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Radius", "R",
            "The radius of this segment (0 for lines).", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Index", "Idx",
            "The index of this segment in the parcel boundary.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "C",
            "The segment geometry as a Rhino curve.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilParcelSegment? segmentGoo = null;

        if (!DA.GetData(0, ref segmentGoo) || segmentGoo?.Value is null) return;

        var segment = segmentGoo.Value;

        var radius = -1.0;
        if (segment.Curve.TryGetArc(out var arc))
            radius = arc.Radius;

        DA.SetData(0, segment.Curve.GetType().Name);
        DA.SetData(1, segment.Curve.GetLength());
        DA.SetData(3, radius);
        DA.SetData(4, segment.Index);
        DA.SetData(5, segment.Curve);
    }
}
