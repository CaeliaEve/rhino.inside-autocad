using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an individual entity (segment) from a Civil 3D Profile.
/// </summary>
/// <remarks>
/// Profile entities can be tangents (lines), circular arcs, or parabolas (vertical curves).
/// This interface provides access to the entity's geometric and stationing information.
/// </remarks>
public interface ICivilProfileEntity
{
    /// <summary>
    /// Gets the type of entity as a string.
    /// </summary>
    /// <value>
    /// Common values: "Tangent", "CircularArc", "Parabola", "PVI", etc.
    /// </value>
    string EntityType { get; }

    /// <summary>
    /// Gets the starting station of this entity along the profile.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this entity along the profile.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the elevation at the start of this entity.
    /// </summary>
    double StartElevation { get; }

    /// <summary>
    /// Gets the elevation at the end of this entity.
    /// </summary>
    double EndElevation { get; }

    /// <summary>
    /// Gets the length of this entity.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the index of this entity within the profile's entity collection.
    /// </summary>
    int EntityIndex { get; }

    /// <summary>
    /// Gets the geometry of this entity as a Rhino curve.
    /// </summary>
    /// <remarks>
    /// The curve is in 2D station-elevation space where X = Station and Y = Elevation.
    /// Tangent entities are represented as <see cref="LineCurve"/>,
    /// arc and parabola entities as interpolated <see cref="NurbsCurve"/>.
    /// </remarks>
    Curve Curve { get; }
}
