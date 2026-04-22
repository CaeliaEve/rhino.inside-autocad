using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilAlignmentType = Autodesk.Civil.DatabaseServices.AlignmentType;
using CivilDocument = Autodesk.Civil.ApplicationServices.CivilDocument;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Provides methods for creating Civil 3D Alignments.
/// </summary>
public static class AlignmentCreator
{
    private const double _shortCurveTolerance = GeometryConstants.ShortCurveTolernace;
    private const double _angleTolerance = GeometryConstants.AngleTolernace;
    private const double _vertexTolernace = GeometryConstants.VertexTolerance;

    /// <summary>
    /// Adds geometry from a Rhino curve to the alignment.
    /// </summary>
    /// <param name="alignment">The alignment to add geometry to.</param>
    /// <param name="curve">The Rhino curve containing the geometry.</param>
    private static void AddCurveGeometry(Alignment alignment, RhinoCurve curve)
    {
        var entities = alignment.Entities;

        switch (curve)
        {
            case LineCurve lineCurve:
                {
                    entities.AddFixedLine(entities.Count,
                        lineCurve.PointAtStart.ToAutocadPoint3d(),
                        lineCurve.PointAtEnd.ToAutocadPoint3d());
                    break;
                }
            case ArcCurve arcCurve:
                {
                    entities.AddFixedCurve(entities.Count,
                        arcCurve.PointAtStart.ToAutocadPoint3d(),
                        arcCurve.PointAt(0.5).ToAutocadPoint3d(),
                        arcCurve.PointAtEnd.ToAutocadPoint3d());
                    break;
                }
            case PolylineCurve polylineCurve:
                {
                    var polyline = polylineCurve.ToPolyline();
                    for (var i = 0; i < polylineCurve.SpanCount; i++)
                    {
                        var line = polyline.SegmentAt(i);
                        entities.AddFixedLine(entities.Count, line.From.ToAutocadPoint3d(),
                            line.To.ToAutocadPoint3d());
                    }

                    break;
                }
            case PolyCurve polyCurve:
                {
                    for (var i = 0; i < polyCurve.SegmentCount; i++)
                    {
                        var segment = polyCurve.SegmentCurve(i);
                        AddCurveGeometry(alignment, segment);
                    }

                    break;
                }
            case NurbsCurve nurbsCurve:
                {
                    var arcAndLines = nurbsCurve.ToArcsAndLines(_vertexTolernace, _angleTolerance * 10.0,
                        _shortCurveTolerance, 0.0);

                    AddCurveGeometry(alignment, arcAndLines);
                    break;
                }
            default:
                break;
        }
    }

    /// <summary>
    /// Creates a Civil 3D Alignment from a Rhino curve.
    /// </summary>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <param name="curve">The Rhino curve to create the alignment from.</param>
    /// <param name="alignmentName">The name for the alignment.</param>
    /// <param name="siteId">The site ObjectId (use ObjectId.Null for siteless alignment).</param>
    /// <param name="layerId">The layer ObjectId for the alignment.</param>
    /// <param name="styleId">Optional alignment style ObjectId.</param>
    /// <param name="labelSetId">Optional alignment label set style ObjectId.</param>
    /// <param name="alignmentType">The alignment type (default: Centerline).</param>
    /// <returns>The created Alignment, or null if creation fails.</returns>
    public static Alignment? Create(
        IAutocadTransactionManager transactionManager,
        RhinoCurve curve,
        string alignmentName,
        ObjectId siteId,
        ObjectId layerId,
        ObjectId styleId,
        ObjectId labelSetId,
        CivilAlignmentType alignmentType)
    {

        var database = transactionManager.AutocadDatabase.Unwrap();
        var civilDoc = CivilDocument.GetCivilDocument(database);

        var alignmentId = Alignment.Create(
            civilDoc,
            alignmentName,
            siteId,
            layerId,
            styleId,
            labelSetId,
            alignmentType);

        if (alignmentId.IsNull)
        {
            throw new Exception($"Failed to create alignment: {alignmentName}");
        }

        var alignment = transactionManager.Unwrap()
            .GetObject(alignmentId, OpenMode.ForWrite) as Alignment;

        if (alignment == null)
        {
            throw new Exception($"Failed to access created alignment: {alignmentName}");

        }

        // Add geometry from the curve
        AddCurveGeometry(alignment, curve);

        return alignment;
    }
}

