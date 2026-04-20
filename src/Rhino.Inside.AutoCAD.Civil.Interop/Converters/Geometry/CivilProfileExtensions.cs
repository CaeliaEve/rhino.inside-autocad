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
            return entities[0].ToRhinoCurve();

        // Join multiple entities into a PolyCurve
        var polyCurve = new RhinoPolyCurve();
        foreach (var entity in entities)
        {
            var rhinoCurve = entity.ToRhinoCurve();
            if (rhinoCurve != null)
            {
                polyCurve.Append(rhinoCurve);
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
            ProfileTangent tangent => new CivilProfileTangentWrapper(tangent, index),
            ProfileCircular arc => new CivilProfileCircularArcWrapper(arc, index),
            ProfileParabolaSymmetric parabola => new CivilProfileSymmetricalParabolaWrapper(parabola, index),
            ProfileParabolaAsymmetric asymParabola => new CivilProfileAsymmetricalParabolaWrapper(asymParabola, index),
            _ => new CivilProfileEntityWrapper(entity, index)
        };
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

    public static RhinoCurve ToRhinoCurve(this CivilProfileEntityWrapper wrapper)
    {
        var startPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(wrapper.StartStation),
            UnitConverter.ToRhinoLength(wrapper.StartElevation),
            0);
        var endPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(wrapper.EndStation),
            UnitConverter.ToRhinoLength(wrapper.EndElevation),
            0);
        return new RhinoLineCurve(new RhinoLine(startPoint, endPoint));
    }

    /// <summary>
    /// Converts a ProfileCircular to a wrapper.
    /// </summary>
    public static RhinoCurve ToRhinoCurve(this CivilProfileCircularArcWrapper wrapper)
    {
        var startPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(wrapper.StartStation),
            UnitConverter.ToRhinoLength(wrapper.StartElevation),
            0);
        var endPoint = new RhinoPoint3d(
            UnitConverter.ToRhinoLength(wrapper.EndStation),
            UnitConverter.ToRhinoLength(wrapper.EndElevation),
            0);
        var radius = UnitConverter.ToRhinoLength(wrapper.Radius);

        var isCrest = wrapper.IsCrest;

        var centerStation = wrapper.HighLowPointStation;
        var centerElevation = isCrest
            ? wrapper.HighLowPointElevation - wrapper.Radius
            : wrapper.HighLowPointElevation + wrapper.Radius;

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

        return rhinoArcCurve;
    }

    /// <summary>
    /// Converts a ProfileParabolaSymmetric to a wrapper.
    /// </summary>
    public static RhinoCurve ToRhinoCurve(this CivilProfileSymmetricalParabolaWrapper symmetricalParabola)
    {
        var points = new List<RhinoPoint3d>();
        var numSamples = Math.Max(20, (int)(symmetricalParabola.Length / 2.0));

        // Sample points along the parabola
        for (var i = 0; i <= numSamples; i++)
        {
            var t = (double)i / numSamples;
            var station = symmetricalParabola.StartStation + (symmetricalParabola.EndStation - symmetricalParabola.StartStation) * t;

            // Calculate elevation using parabola equation
            var elevation = symmetricalParabola.CalculateElevation(station);

            points.Add(new RhinoPoint3d(
                UnitConverter.ToRhinoLength(station),
                UnitConverter.ToRhinoLength(elevation),
                0));
        }

        return RhinoNurbsCurve.Create(false, 3, points);
    }

    /// <summary>
    /// Converts a ProfileParabolaSymmetric to a wrapper.
    /// </summary>
    public static RhinoCurve ToRhinoCurve(this CivilProfileAsymmetricalParabolaWrapper asymmetricalParabola)
    {
        var points = new List<RhinoPoint3d>();
        var numSamples = Math.Max(20, (int)(asymmetricalParabola.Length / 2.0));

        // For asymmetric parabolas, calculate K values for each half
        // K = L / |A| where A = grade change in percent
        var gradeChangePercent = Math.Abs(asymmetricalParabola.GradeOut - asymmetricalParabola.GradeIn) * 100.0;

        // Get the asymmetric lengths for each half
        var length1 = asymmetricalParabola.AsymmetricLength1;
        var length2 = asymmetricalParabola.AsymmetricLength2;

        // Calculate K for each half based on their respective lengths
        var k1 = gradeChangePercent > 0.0001 ? length1 / (gradeChangePercent * length1 / asymmetricalParabola.Length) : asymmetricalParabola.Length;
        var k2 = gradeChangePercent > 0.0001 ? length2 / (gradeChangePercent * length2 / asymmetricalParabola.Length) : asymmetricalParabola.Length;

        // Sample points along the parabola
        for (var i = 0; i <= numSamples; i++)
        {
            var t = (double)i / numSamples;
            var station = asymmetricalParabola.StartStation + (asymmetricalParabola.EndStation - asymmetricalParabola.StartStation) * t;

            // Use appropriate K value based on position relative to PVI
            var k = station < asymmetricalParabola.PVIStation ? k1 : k2;

            // Calculate elevation using parabola equation
            var elevation = asymmetricalParabola.CalculateElevation(station, k);

            points.Add(new RhinoPoint3d(
                UnitConverter.ToRhinoLength(station),
                UnitConverter.ToRhinoLength(elevation),
                0));
        }

        return RhinoNurbsCurve.Create(false, 3, points);
    }

    /// <summary>
    /// Converts a ProfileTangent to a wrapper.
    /// </summary>
    public static RhinoCurve ToRhinoCurve(this CivilProfileTangentWrapper tangent)
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

        return new RhinoLineCurve(line);

    }
}
