using RhinoCurve = Rhino.Geometry.Curve;

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
    /// Gets the index of this segment within the parcel boundary.
    /// </summary>
    int Index { get; }

    /// <summary>
    /// The AutoCAD base curve of the segment.
    /// </summary>
    RhinoCurve Curve { get; }
}
