namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an asymmetrical parabola (vertical curve) entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile parabolas are the most common type of vertical curve, used for
/// smooth transitions between different grades. They are characterized by
/// their K value (rate of change of grade).
/// </remarks>
public interface ICivilProfileAsymmetricalParabola : ICivilProfileEntity
{
    /// <summary>
    /// Gets the station of the PVI (Point of Vertical Intersection), in AutoCAD units.
    /// </summary>
    double PVIStation { get; }

    /// <summary>
    /// Gets the elevation of the PVI (Point of Vertical Intersection), in AutoCAD units.
    /// </summary>
    double PVIElevation { get; }

    /// <summary>
    /// The station along the profile where the high or low point of this parabola occurs, in AutoCAD units.
    /// </summary>
    double HighLowPointStation { get; }

    /// <summary>
    /// The elevation at the high or low point of this parabola in AutoCAD units.
    /// </summary>
    double HighLowPointElevation { get; }

    /// <summary>
    /// Gets the outgoing grade (slope) of the parabola, expressed as a percentage.
    /// </summary>
    double GradeOut { get; }

    /// <summary>
    /// Gets the incoming grade (slope) of the parabola, expressed as a percentage.
    /// </summary>
    double GradeIn { get; }

    /// <summary>
    /// Gets the length of the parabola from the PVI to the high or low point, in AutoCAD units.
    /// </summary>
    double AsymmetricLength1 { get; }

    /// <summary>
    /// Gets the length of the parabola from the high or low point to the end of the curve, in AutoCAD units.
    /// </summary>
    double AsymmetricLength2 { get; }
}
