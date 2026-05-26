namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a symmetrical parabola (vertical curve) entity from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile parabolas are the most common type of vertical curve, used for
/// smooth transitions between different grades. They are characterized by
/// their K value (rate of change of grade).
/// </remarks>
public interface ICivilProfileSymmetricalParabola : ICivilProfileEntity
{
    /// <summary>
    /// Gets the K value (rate of change of grade) of this parabola.
    /// </summary>
    /// <remarks>
    /// K value is the horizontal distance required to achieve a 1% change in grade.
    /// Higher K values indicate gentler curves.
    /// </remarks>
    double KValue { get; }

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
}