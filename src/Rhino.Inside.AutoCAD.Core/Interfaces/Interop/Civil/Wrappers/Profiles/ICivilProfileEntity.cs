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
    /// Gets the starting station of this entity along the profile in AutoCAD units.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this entity along the profile in AutoCAD units.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the elevation at the start of this entity in AutoCAD units.
    /// </summary>
    double StartElevation { get; }

    /// <summary>
    /// Gets the elevation at the end of this entity  in AutoCAD units.
    /// </summary>
    double EndElevation { get; }

    /// <summary>
    /// Gets the length of this entity  in AutoCAD units.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the index of this entity within the profile's entity collection
    ///  in AutoCAD units.
    /// </summary>
    int EntityIndex { get; }
}
