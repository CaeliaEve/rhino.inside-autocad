using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Subassembly.
/// </summary>
/// <remarks>
/// This interface provides access to subassembly metadata
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilSubassembly
{
    /// <summary>
    /// Gets the name of the subassembly.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the subassembly.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the side of the subassembly (Left, Right, or None).
    /// </summary>
    CivilSide Side { get; }

    /// <summary>
    /// Gets the origin point of the subassembly.
    /// </summary>
    Point3d Origin { get; }

    /// <summary>
    /// Gets the geometry of the subassembly as curves (links).
    /// </summary>
    IReadOnlyList<Curve> Geometry { get; }
}
