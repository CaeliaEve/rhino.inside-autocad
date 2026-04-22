using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents reference station information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Bundles the reference point and reference point station values for an alignment.
/// </remarks>
public interface ICivilReferenceStation
{
    /// <summary>
    /// Gets a value indicating whether this alignment has a reference point defined.
    /// </summary>
    bool HasReferencePoint { get; }

    /// <summary>
    /// Gets the reference point location in world coordinates.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Point3d.Unset"/> if no reference point is defined.
    /// </remarks>
    Point3d ReferencePoint { get; }

    /// <summary>
    /// Gets the station value at the reference point.
    /// </summary>
    /// <remarks>
    /// Returns 0 if no reference point is defined.
    /// </remarks>
    double ReferencePointStation { get; }

    /// <summary>
    /// Creates a shallow copy of this reference station.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilReferenceStation ShallowClone();
}
