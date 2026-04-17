using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using Arc = Rhino.Geometry.Arc;
using CadPolyline = Autodesk.AutoCAD.DatabaseServices.Polyline;
using Circle = Rhino.Geometry.Circle;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoPoint3d = Rhino.Geometry.Point3d;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D Parcel geometry to Rhino geometry.
/// </summary>
public static class CivilParcelExtensions
{
    /// <summary>
    /// Converts the outer boundary of a Civil 3D Parcel to a Rhino PolyCurve.
    /// </summary>
    /// <param name="parcel">The parcel to convert.</param>
    /// <returns>A Rhino curve representing the outer boundary, or null if conversion fails.</returns>
    public static RhinoCurve? ToRhinoBoundary(this Parcel parcel)
    {
        return parcel.BaseCurve.ToRhinoCurve();
    }

    /// <summary>
    /// Gets all parcel segments from the boundary as wrappers.
    /// Extracts segments from the BaseCurve geometry.
    /// </summary>
    /// <param name="parcel">The parcel to extract segments from.</param>
    /// <returns>A list of segment wrappers.</returns>
    public static List<ICivilParcelSegment> GetParcelSegments(this Parcel parcel)
    {
        var segments = new List<ICivilParcelSegment>();

        try
        {
            var baseCurve = parcel.BaseCurve;
            if (baseCurve == null)
                return segments;

            // Handle polyline - extract each segment
            if (baseCurve is CadPolyline polyline)
            {
                var segmentCount = polyline.NumberOfVertices;
                if (polyline.Closed && segmentCount > 0)
                {
                    // For closed polylines, we have as many segments as vertices
                    for (var i = 0; i < segmentCount; i++)
                    {
                        var wrapper = CreateSegmentFromPolyline(polyline, i);
                        if (wrapper != null)
                        {
                            segments.Add(wrapper);
                        }
                    }
                }
                else if (segmentCount > 1)
                {
                    // For open polylines, segments = vertices - 1
                    for (var i = 0; i < segmentCount - 1; i++)
                    {
                        var wrapper = CreateSegmentFromPolyline(polyline, i);
                        if (wrapper != null)
                        {
                            segments.Add(wrapper);
                        }
                    }
                }
            }
            else
            {
                // For other curve types, create a single segment
                var rhinoCurve = baseCurve.ToRhinoCurve();
                if (rhinoCurve != null)
                {
                    var wrapper = new CivilParcelSegmentWrapper(
                        "Curve",
                        rhinoCurve.GetLength(),
                        0.0, // Direction not available
                        0.0, // Radius not applicable for general curves
                        0,
                        rhinoCurve);
                    segments.Add(wrapper);
                }
            }
        }
        catch
        {
            // Return empty list on error
        }

        return segments;
    }

    /// <summary>
    /// Creates a segment wrapper from a polyline segment.
    /// </summary>
    private static CivilParcelSegmentWrapper? CreateSegmentFromPolyline(CadPolyline polyline, int index)
    {
        try
        {
            var segmentType = polyline.GetSegmentType(index);
            RhinoCurve? rhinoCurve = null;
            var radius = 0.0;
            var direction = 0.0;

            switch (segmentType)
            {
                case SegmentType.Line:
                    var lineSegment = polyline.GetLineSegmentAt(index);

                    var lineStart = lineSegment.StartPoint.ToRhinoPoint3d();

                    var lineEnd = lineSegment.EndPoint.ToRhinoPoint3d();

                    rhinoCurve = new LineCurve(lineStart, lineEnd);

                    var lineDir = lineEnd - lineStart;
                    direction = Math.Atan2(lineDir.Y, lineDir.X);
                    break;

                case SegmentType.Arc:
                    var arcSegment = polyline.GetArcSegmentAt(index);
                    var center = arcSegment.Center.ToRhinoPoint3d();
                    radius = UnitConverter.ToRhinoLength(arcSegment.Radius);

                    var circle = new Circle(center, Math.Abs(radius));

                    var arcStart = arcSegment.StartPoint.ToRhinoPoint3d();
                    var arcEnd = arcSegment.EndPoint.ToRhinoPoint3d();

                    var arcStartAngle = arcSegment.StartAngle;
                    var arcEndAngle = arcSegment.EndAngle;

                    var interval = new Interval(arcStartAngle, arcEndAngle);

                    var arc = new Arc(circle, interval);

                    rhinoCurve = new ArcCurve(arc);

                    // Direction is the chord direction
                    var arcDir = arcEnd - arcStart;
                    direction = Math.Atan2(arcDir.Y, arcDir.X);
                    break;

                default:
                    // For other segment types, try to get as line
                    try
                    {
                        var defaultLine = polyline.GetLineSegmentAt(index);
                        var defaultStart = defaultLine.StartPoint.ToRhinoPoint3d();
                        var defaultEnd = defaultLine.EndPoint.ToRhinoPoint3d();
                        rhinoCurve = new LineCurve(defaultStart, defaultEnd);
                    }
                    catch
                    {
                        return null;
                    }
                    break;
            }

            if (rhinoCurve == null)
                return null;

            var segmentTypeName = segmentType == SegmentType.Arc ? "Arc" : "Line";
            var length = rhinoCurve.GetLength();

            return new CivilParcelSegmentWrapper(segmentTypeName, length, direction, radius, index, rhinoCurve);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the centroid of a Civil 3D Parcel.
    /// </summary>
    /// <param name="parcel">The parcel to get the centroid from.</param>
    /// <returns>The centroid as a Rhino Point3d.</returns>
    public static RhinoPoint3d GetCentroid(this Parcel parcel)
    {
        try
        {
            // Try to use the parcel's centroid property
            var centroid = parcel.Centroid;
            return new RhinoPoint3d(centroid.X, centroid.Y, centroid.Z);
        }
        catch
        {
            // If centroid is not available, compute from boundary
            try
            {
                var boundary = parcel.ToRhinoBoundary();
                if (boundary != null)
                {
                    var bbox = boundary.GetBoundingBox(true);
                    return bbox.Center;
                }
            }
            catch
            {
                // Ignore
            }
            return RhinoPoint3d.Origin;
        }
    }
}
