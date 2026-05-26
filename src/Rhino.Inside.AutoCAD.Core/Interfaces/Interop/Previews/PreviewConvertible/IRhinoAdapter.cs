using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Adapter interface allowing custom types to participate in the
/// GH_AutocadGeometricGoo system without being GeometryBase subclasses.
/// </summary>
public interface IRhinoAdapter
{
    /// <summary>
    /// Gets the bounding box encompassing all contained geometry.
    /// </summary>
    BoundingBox GetBoundingBox();

    /// <summary>
    /// Applies a transformation to all contained geometry.
    /// </summary>
    void Transform(Transform xform);

    /// <summary>
    /// Applies a space morph to all contained geometry.
    /// </summary>
    void Morph(SpaceMorph morph);

    /// <summary>
    /// Creates a deep copy of this adapter.
    /// </summary>
    IRhinoAdapter Duplicate();
}
