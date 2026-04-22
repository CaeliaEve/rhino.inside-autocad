using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents an arc sub-entity from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Alignment arcs are circular curve segments that make up part of an alignment's
/// horizontal geometry. This interface provides access to the arc's geometry,
/// station information, and curve properties.
/// </remarks>
public interface ICivilAlignmentArc
{
    /// <summary>
    /// Gets the arc geometry as a Rhino arc.
    /// </summary>
    Arc Arc { get; }

    /// <summary>
    /// Gets the starting station of this sub-entity along the alignment.
    /// </summary>
    double StartStation { get; }

    /// <summary>
    /// Gets the ending station of this sub-entity along the alignment.
    /// </summary>
    double EndStation { get; }

    /// <summary>
    /// Gets the length of this sub-entity.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the radius of the arc.
    /// </summary>
    double Radius { get; }

    /// <summary>
    /// Gets the center point of the arc.
    /// </summary>
    Point3d CenterPoint { get; }

    /// <summary>
    /// Gets a value indicating whether the arc curves clockwise.
    /// </summary>
    bool IsClockwise { get; }

    /// <summary>
    /// Gets the index of this sub-entity within the alignment.
    /// </summary>
    int Index { get; }
}
