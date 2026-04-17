using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using RhinoArc = Rhino.Geometry.Arc;
using RhinoArcCurve = Rhino.Geometry.ArcCurve;
using RhinoCurve = Rhino.Geometry.Curve;
using RhinoLine = Rhino.Geometry.Line;
using RhinoLineCurve = Rhino.Geometry.LineCurve;
using RhinoNurbsCurve = Rhino.Geometry.NurbsCurve;
using RhinoPoint3d = Rhino.Geometry.Point3d;
using RhinoPolyCurve = Rhino.Geometry.PolyCurve;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D Profile types to Rhino geometry types.
/// </summary>
public static class CivilProfileExtensions
{
    /// <summary>
    /// Converts a Civil 3D Profile to a Rhino Curve (PolyCurve) in station-elevation space.
    /// </summary>
    /// <param name="profile">The Civil 3D Profile to convert.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A Rhino Curve representing the profile in 2D (X=Station, Y=Elevation).</returns>
    public static RhinoCurve? ToRhinoCurve(this Profile profile, IAutocadTransactionManager transactionManager)
    {
        var entities = profile.GetProfileEntities(transactionManager);

        if (entities.Count == 0)
            return null;

        if (entities.Count == 1)
            return entities[0].Curve;

        // Join multiple entities into a PolyCurve
        var polyCurve = new RhinoPolyCurve();
        foreach (var entity in entities)
        {
            if (entity.Curve != null)
            {
                polyCurve.Append(entity.Curve);
            }
        }

        return polyCurve;
    }

    /// <summary>
    /// Extracts all entities from a Civil 3D Profile as wrapper objects.
    /// </summary>
    /// <param name="profile">The Civil 3D Profile to extract entities from.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>A list of profile entity wrappers.</returns>
    public static List<CivilProfileEntityWrapper> GetProfileEntities(
        this Profile profile,
        IAutocadTransactionManager transactionManager)
    {
        var entities = new List<CivilProfileEntityWrapper>();
        var entityCollection = profile.Entities;

        for (var i = 0; i < entityCollection.Count; i++)
        {
            var entity = entityCollection[i];
            var wrapper = ConvertEntityToWrapper(entity, i);

            if (wrapper != null)
            {
                entities.Add(wrapper);
            }
        }

        return entities;
    }

    /// <summary>
    /// Converts a profile entity to the appropriate wrapper type.
    /// </summary>
    private static CivilProfileEntityWrapper? ConvertEntityToWrapper(ProfileEntity entity, int index)
    {
        return entity switch
        {
            ProfileTangent tangent => ConvertTangent(tangent, index),
            ProfileCircular arc => ConvertCircularArc(arc, index),
            ProfileParabolaSymmetric parabola => ConvertParabola(parabola, index),
            ProfileParabolaAsymmetric asymParabola => ConvertAsymmetricParabola(asymParabola, index),
            _ => ConvertGenericEntity(entity, index)
        };
    }

    /// <summary>
    /// Converts a ProfileTangent to a wrapper.
    /// </summary>
    private static CivilProfileTangentWrapper ConvertTangent(ProfileTangent tangent, int index)
    {
        var startPoint = new RhinoPoint3d(
                UnitConverter.ToRhinoLength(tangent.StartStation),
                UnitConverter.ToRhinoLength(tangent.StartElevation),
                0);
        var endPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(tangent.EndStation),
            UnitConverter.ToRhinoLength(tangent.EndElevation),
            0);
        var line = new RhinoLine(startPoint, endPoint);
        var curve = new RhinoLineCurve(line);

        return new CivilProfileTangentWrapper(
            UnitConverter.ToRhinoLength(tangent.StartStation),
            UnitConverter.ToRhinoLength(tangent.EndStation),
            UnitConverter.ToRhinoLength(tangent.StartElevation),
            UnitConverter.ToRhinoLength(tangent.EndElevation),
            UnitConverter.ToRhinoLength(tangent.Length),
            index,
            tangent.Grade * 100.0, // Convert to percentage (dimensionless, no unit conversion needed)
            line,
            curve);
    }

    /// <summary>
    /// Converts a ProfileCircular to a wrapper.
    /// </summary>
    private static CivilProfileCircularArcWrapper ConvertCircularArc(ProfileCircular arc, int index)
    {
        var startPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(arc.StartStation),
            UnitConverter.ToRhinoLength(arc.StartElevation),
            0);
        var endPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(arc.EndStation),
            UnitConverter.ToRhinoLength(arc.EndElevation),
            0);
        var radius = UnitConverter.ToRhinoLength(arc.Radius);

        var isCrest = arc.GradeIn > arc.GradeOut;

        var centerStation = arc.HighLowPointStation;
        var centerElevation = isCrest
            ? arc.HighLowPointElevation - arc.Radius
            : arc.HighLowPointElevation + arc.Radius;

        var centerPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(centerStation),
            UnitConverter.ToRhinoLength(centerElevation),
            0);

        var circle = new Rhino.Geometry.Circle(centerPoint, radius);

        if (!circle.IsValid)
        {
            throw new InvalidOperationException(
                "Failed to create a valid circle from the AlignmentArc points.");
        }

        _ = circle.ClosestParameter(startPoint, out var start);
        _ = circle.ClosestParameter(endPoint, out var end);

        var interval = new Interval(start, end);

        var rhinoArc = new RhinoArc(circle, interval);

        if (!rhinoArc.IsValid)
        {
            throw new InvalidOperationException(
                "Failed to create a valid Rhino arc from the AlignmentArc.");
        }

        var rhinoArcCurve = new RhinoArcCurve(rhinoArc);

        return new CivilProfileCircularArcWrapper(
            UnitConverter.ToRhinoLength(arc.StartStation),
            UnitConverter.ToRhinoLength(arc.EndStation),
            UnitConverter.ToRhinoLength(arc.StartElevation),
            UnitConverter.ToRhinoLength(arc.EndElevation),
            UnitConverter.ToRhinoLength(arc.Length),
            index,
            rhinoArc,
            rhinoArcCurve);
    }

    /// <summary>
    /// Converts a ProfileParabolaSymmetric to a wrapper.
    /// </summary>
    private static CivilProfileParabolaWrapper ConvertParabola(ProfileParabolaSymmetric parabola, int index)
    {
        var curve = CreateParabolaCurve(parabola);

        RhinoPoint3d? highLowPoint = null;
        try
        {
            if (parabola.HighLowPointStation >= parabola.StartStation &&
                parabola.HighLowPointStation <= parabola.EndStation)
            {
                highLowPoint = new RhinoPoint3d(
                    UnitConverter.ToRhinoLength(parabola.HighLowPointStation),
                    UnitConverter.ToRhinoLength(parabola.HighLowPointElevation),
                    0);
            }
        }
        catch
        {
            // High/low point may not exist within curve
        }

        return new CivilProfileParabolaWrapper(
            UnitConverter.ToRhinoLength(parabola.StartStation),
            UnitConverter.ToRhinoLength(parabola.EndStation),
            UnitConverter.ToRhinoLength(parabola.StartElevation),
            UnitConverter.ToRhinoLength(parabola.EndElevation),
            UnitConverter.ToRhinoLength(parabola.Length),
            index,
            parabola.K, // K value is dimensionless (rate of change)
            UnitConverter.ToRhinoLength(parabola.PVIStation),
            UnitConverter.ToRhinoLength(parabola.PVIElevation),
            highLowPoint,
            curve);
    }

    /// <summary>
    /// Converts a ProfileParabolaAsymmetric to a wrapper.
    /// </summary>
    private static CivilProfileParabolaWrapper ConvertAsymmetricParabola(ProfileParabolaAsymmetric parabola, int index)
    {
        var curve = CreateAsymmetricParabolaCurve(parabola);

        RhinoPoint3d? highLowPoint = null;
        try
        {
            if (parabola.HighLowPointStation >= parabola.StartStation &&
                parabola.HighLowPointStation <= parabola.EndStation)
            {
                highLowPoint = new RhinoPoint3d(
                    UnitConverter.ToRhinoLength(parabola.HighLowPointStation),
                    UnitConverter.ToRhinoLength(parabola.HighLowPointElevation),
                    0);
            }
        }
        catch
        {
            // High/low point may not exist within curve
        }

        // For asymmetric parabolas, calculate effective K from length and grade change
        // K = L / |A| where A = grade change in percent
        var gradeChange = Math.Abs(parabola.GradeOut - parabola.GradeIn) * 100.0;
        var effectiveK = gradeChange > 0.0001 ? parabola.Length / gradeChange : 0.0;

        return new CivilProfileParabolaWrapper(
            UnitConverter.ToRhinoLength(parabola.StartStation),
            UnitConverter.ToRhinoLength(parabola.EndStation),
            UnitConverter.ToRhinoLength(parabola.StartElevation),
            UnitConverter.ToRhinoLength(parabola.EndElevation),
            UnitConverter.ToRhinoLength(parabola.Length),
            index,
            effectiveK, // Calculated K value
            UnitConverter.ToRhinoLength(parabola.PVIStation),
            UnitConverter.ToRhinoLength(parabola.PVIElevation),
            highLowPoint,
            curve);
    }

    /// <summary>
    /// Converts a generic profile entity to a wrapper.
    /// </summary>
    private static CivilProfileEntityWrapper? ConvertGenericEntity(ProfileEntity entity, int index)
    {
        try
        {
            var startPoint = new RhinoPoint3d(
                UnitConverter.ToRhinoLength(entity.StartStation),
                UnitConverter.ToRhinoLength(entity.StartElevation),
                0);
            var endPoint = new RhinoPoint3d(
                UnitConverter.ToRhinoLength(entity.EndStation),
                UnitConverter.ToRhinoLength(entity.EndElevation),
                0);
            var curve = new RhinoLineCurve(new RhinoLine(startPoint, endPoint));

            return new CivilProfileEntityWrapper(
                entity.EntityType.ToString(),
                UnitConverter.ToRhinoLength(entity.StartStation),
                UnitConverter.ToRhinoLength(entity.EndStation),
                UnitConverter.ToRhinoLength(entity.StartElevation),
                UnitConverter.ToRhinoLength(entity.EndElevation),
                UnitConverter.ToRhinoLength(entity.Length),
                index,
                curve);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a curve representation for a symmetric parabola.
    /// </summary>
    private static RhinoCurve CreateParabolaCurve(ProfileParabolaSymmetric parabola)
    {
        var points = new List<RhinoPoint3d>();
        var numSamples = Math.Max(20, (int)(parabola.Length / 2.0));

        // Sample points along the parabola
        for (var i = 0; i <= numSamples; i++)
        {
            var t = (double)i / numSamples;
            var station = parabola.StartStation + (parabola.EndStation - parabola.StartStation) * t;

            // Calculate elevation using parabola equation
            var elevation = CalculateParabolaElevation(
                station,
                parabola.PVIStation,
                parabola.PVIElevation,
                parabola.GradeIn,
                parabola.K);

            points.Add(new RhinoPoint3d(
                UnitConverter.ToRhinoLength(station),
                UnitConverter.ToRhinoLength(elevation),
                0));
        }

        return RhinoNurbsCurve.Create(false, 3, points);
    }

    /// <summary>
    /// Creates a curve representation for an asymmetric parabola.
    /// </summary>
    private static RhinoCurve CreateAsymmetricParabolaCurve(ProfileParabolaAsymmetric parabola)
    {
        var points = new List<RhinoPoint3d>();
        var numSamples = Math.Max(20, (int)(parabola.Length / 2.0));

        // For asymmetric parabolas, calculate K values for each half
        // K = L / |A| where A = grade change in percent
        var gradeChangePercent = Math.Abs(parabola.GradeOut - parabola.GradeIn) * 100.0;

        // Get the asymmetric lengths for each half
        var length1 = parabola.AsymmetricLength1;
        var length2 = parabola.AsymmetricLength2;

        // Calculate K for each half based on their respective lengths
        var k1 = gradeChangePercent > 0.0001 ? length1 / (gradeChangePercent * length1 / parabola.Length) : parabola.Length;
        var k2 = gradeChangePercent > 0.0001 ? length2 / (gradeChangePercent * length2 / parabola.Length) : parabola.Length;

        // Sample points along the parabola
        for (var i = 0; i <= numSamples; i++)
        {
            var t = (double)i / numSamples;
            var station = parabola.StartStation + (parabola.EndStation - parabola.StartStation) * t;

            // Use appropriate K value based on position relative to PVI
            var k = station < parabola.PVIStation ? k1 : k2;

            var elevation = CalculateParabolaElevation(
                station,
                parabola.PVIStation,
                parabola.PVIElevation,
                parabola.GradeIn,
                k);

            points.Add(new RhinoPoint3d(
                UnitConverter.ToRhinoLength(station),
                UnitConverter.ToRhinoLength(elevation),
                0));
        }

        return RhinoNurbsCurve.Create(false, 3, points);
    }

    /// <summary>
    /// Calculates the elevation at a given station along a parabolic vertical curve.
    /// </summary>
    private static double CalculateParabolaElevation(
        double station,
        double pviStation,
        double pviElevation,
        double gradeIn,
        double k)
    {
        // Distance from PVI
        var x = station - pviStation;

        // Tangent elevation at this station
        var tangentElevation = pviElevation + gradeIn * x;

        // Parabolic correction
        // y = gradeIn * x + (gradeOut - gradeIn) * x^2 / (2 * L)
        // where L = K * |gradeOut - gradeIn|
        // Simplified: correction = x^2 / (2 * K * 100)
        var correction = x * x / (2.0 * k * 100.0);

        // For sag curves, add correction; for crest curves, subtract
        // The sign depends on the relationship between gradeIn and gradeOut
        return tangentElevation - correction;
    }

    /// <summary>
    /// Gets the parent alignment name for a profile.
    /// </summary>
    /// <param name="profile">The profile to get the parent alignment name for.</param>
    /// <param name="transactionManager">The transaction manager for database operations.</param>
    /// <returns>The name of the parent alignment, or empty string if not found.</returns>
    public static string GetParentAlignmentName(
        this Profile profile,
        IAutocadTransactionManager transactionManager)
    {
        try
        {
            var alignmentId = profile.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return string.Empty;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            return alignment?.Name ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
