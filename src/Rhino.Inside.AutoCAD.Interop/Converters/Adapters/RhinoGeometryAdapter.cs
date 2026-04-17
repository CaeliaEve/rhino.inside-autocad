using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Wraps a standard GeometryBase type as an IRhinoAdapter.
/// </summary>
public class RhinoGeometryAdapter<T> : IRhinoAdapter where T : GeometryBase
{
    /// <summary>
    /// Gets the underlying geometry.
    /// </summary>
    public T? Geometry { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RhinoGeometryAdapter{T}"/> class.
    /// </summary>
    /// <param name="geometry">The geometry to wrap.</param>
    public RhinoGeometryAdapter(T? geometry) => Geometry = geometry;

    /// <inheritdoc />
    public BoundingBox GetBoundingBox() => Geometry?.GetBoundingBox(false) ?? BoundingBox.Empty;

    /// <inheritdoc />
    public void Transform(Transform xform) => Geometry?.Transform(xform);

    /// <inheritdoc />
    public void Morph(SpaceMorph morph)
    {
        if (Geometry != null) morph.Morph(Geometry);
    }

    /// <inheritdoc />
    public IRhinoAdapter Duplicate() =>
        new RhinoGeometryAdapter<T>(Geometry?.Duplicate() as T);
}
