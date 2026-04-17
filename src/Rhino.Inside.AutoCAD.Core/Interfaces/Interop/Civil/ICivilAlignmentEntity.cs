using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an individual entity (segment) from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Alignment entities can be lines, arcs, spirals, or composite types
/// such as spiral-curve-spiral combinations. This interface provides
/// access to the entity's geometric and stationing information.
/// </remarks>
public interface ICivilAlignmentEntity
{
    /// <summary>
    /// Gets the type of entity as a string.
    /// </summary>
    /// <value>
    /// Common values: "Line", "Arc", "Spiral", "SpiralCurveSpiral",
    /// "SpiralLineSpiral", "SpiralLine", "LineSpiral", etc.
    /// </value>
    string EntityType { get; }

    /// <summary>
    /// Gets the starting station of this entity along the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this entity along the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the length of this entity.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the index of this entity within the alignment's entity collection.
    /// </summary>
    int EntityIndex { get; }

    /// <summary>
    /// Gets the geometry of this entity as a Rhino curve.
    /// </summary>
    /// <remarks>
    /// Line entities are represented as <see cref="LineCurve"/>,
    /// arc entities as <see cref="ArcCurve"/>, and spiral entities
    /// as interpolated <see cref="NurbsCurve"/>.
    /// </remarks>
    Curve Curve { get; }
}
