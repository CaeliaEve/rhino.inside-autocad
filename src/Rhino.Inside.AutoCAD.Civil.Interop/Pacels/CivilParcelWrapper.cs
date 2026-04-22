using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using CadPolyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPoint = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <inheritdoc cref="ICivilParcel"/>
public class CivilParcelWrapper : AutocadEntityWrapper, ICivilParcel
{
    private readonly Parcel _parcel;

    /// <inheritdoc />
    public ICivilParcelProperties Properties { get; }

    /// <inheritdoc />
    public RhinoCurve BoundaryCurve { get; }

    /// <inheritdoc />
    public RhinoPoint Centroid { get; }

    /// <inheritdoc />
    public List<ICivilParcelSegment> Segments { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilParcelSegmentWrapper"/>.
    /// </summary>
    public CivilParcelWrapper(Parcel parcel) : base(parcel)
    {
        _parcel = parcel;

        var baseCurve = parcel.BaseCurve;

        this.BoundaryCurve = baseCurve.ToRhinoCurve();

        this.Properties = CivilParcelProperties.CreateFromParcel(parcel);

        this.Segments = this.ExtractSegments(baseCurve);

        this.Centroid = parcel.Centroid.ToRhinoPoint3d();

    }

    private List<ICivilParcelSegment> ExtractSegments(CadCurve baseCurve)
    {
        var segments = new List<ICivilParcelSegment>();

        if (baseCurve is CadPolyline polyline)
        {
            var segmentCount = polyline.NumberOfVertices;

            var finalSegmentCount = polyline.Closed && segmentCount > 0 ? segmentCount : segmentCount - 1;

            for (var i = 0; i < finalSegmentCount; i++)
            {

                var wrapper = this.CreateSegmentFromPolyline(polyline, i);

                segments.Add(wrapper);
            }

            return segments;
        }

        var rhinoCurve = baseCurve.ToRhinoCurve();

        var wholeWrapper = new CivilParcelSegment(rhinoCurve, 0);

        segments.Add(wholeWrapper);

        return segments;
    }

    /// <summary>
    /// Creates a segment wrapper from a polyline segment.
    /// </summary>
    private ICivilParcelSegment CreateSegmentFromPolyline(CadPolyline polyline, int index)
    {
        var segmentType = polyline.GetSegmentType(index);

        switch (segmentType)
        {
            default:
            case SegmentType.Line:
                {
                    var lineSegment = polyline.GetLineSegmentAt(index);

                    var rhinoCurve = lineSegment.ToRhinoCurve();

                    return new CivilParcelSegment(rhinoCurve, index);

                }

            case SegmentType.Arc:
                {
                    var arcSegment = polyline.GetArcSegmentAt(index);

                    var rhinoCurve = arcSegment.ToRhinoCurve();

                    return new CivilParcelSegment(rhinoCurve, index);
                }
        }
    }

    /// <summary>
    /// Creates a duplicate of this parcel segment wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public override CivilParcelWrapper ShallowClone()
    {
        return new CivilParcelWrapper(_parcel);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Parcel [{this.Properties.Name}] (Area: {this.Properties.Area})";
    }
}
