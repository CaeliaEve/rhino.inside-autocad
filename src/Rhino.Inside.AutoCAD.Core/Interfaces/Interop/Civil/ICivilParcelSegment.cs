using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents a boundary segment from a Civil 3D Parcel.
/// </summary>
/// <remarks>
/// Parcel segments can be lines, arcs, or spirals that form the
/// boundary of a land subdivision parcel. This interface provides
/// access to the segment's geometric and property information.
/// </remarks>
public interface ICivilParcelSegment
{
    /// <summary>
    /// Gets the type of segment as a string.
    /// </summary>
    /// <value>
    /// Common values: "Line", "Arc", "Spiral".
    /// </value>
    string SegmentType { get; }

    /// <summary>
    /// Gets the length of this segment.
    /// </summary>
    double Length { get; }

    /// <summary>
    /// Gets the direction (bearing) of this segment in radians.
    /// </summary>
    /// <remarks>
    /// For arcs, this typically represents the chord direction.
    /// </remarks>
    double Direction { get; }

    /// <summary>
    /// Gets the radius of this segment.
    /// </summary>
    /// <remarks>
    /// Returns 0 for line segments.
    /// </remarks>
    double Radius { get; }

    /// <summary>
    /// Gets the index of this segment within the parcel boundary.
    /// </summary>
    int Index { get; }

    /// <summary>
    /// Gets the geometry of this segment as a Rhino curve.
    /// </summary>
    Curve Curve { get; }
}
