using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a line sub-entity from a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Alignment lines are straight segments that make up part of an alignment's
/// horizontal geometry. This interface provides access to the line's geometry
/// and station information.
/// </remarks>
public interface ICivilAlignmentLine
{
    /// <summary>
    /// Gets the line geometry as a Rhino line.
    /// </summary>
    Line Line { get; }

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
    /// Gets the index of this sub-entity within the alignment.
    /// </summary>
    int Index { get; }
}
