using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Adapter holding base and comparison meshes for volume surfaces.
/// </summary>
public class VolumeSurfaceAdapter : IRhinoAdapter
{
    /// <summary>
    /// Gets or sets the base surface mesh.
    /// </summary>
    public Mesh? BaseMesh { get; }

    /// <summary>
    /// Gets or sets the comparison surface mesh.
    /// </summary>
    public Mesh? ComparisonMesh { get; }

    /// <summary>
    /// Gets combined mesh for preview (both meshes appended).
    /// </summary>
    public Mesh? CombinedMesh { get; }

    public VolumeSurfaceAdapter(Mesh? baseMesh, Mesh? comparisonMesh)
    {
        this.BaseMesh = baseMesh;
        this.ComparisonMesh = comparisonMesh;

        var combined = new Mesh();
        if (this.BaseMesh != null) combined.Append(this.BaseMesh);
        if (this.ComparisonMesh != null) combined.Append(this.ComparisonMesh);

        this.CombinedMesh = combined;
    }

    /// <inheritdoc />
    public BoundingBox GetBoundingBox()
    {
        var box = BoundingBox.Empty;
        if (this.BaseMesh != null) box.Union(this.BaseMesh.GetBoundingBox(false));
        if (this.ComparisonMesh != null) box.Union(this.ComparisonMesh.GetBoundingBox(false));
        return box;
    }

    /// <inheritdoc />
    public void Transform(Transform xform)
    {
        this.BaseMesh?.Transform(xform);
        this.ComparisonMesh?.Transform(xform);
    }

    /// <inheritdoc />
    public void Morph(SpaceMorph morph)
    {
        if (this.BaseMesh != null) morph.Morph(this.BaseMesh);
        if (this.ComparisonMesh != null) morph.Morph(this.ComparisonMesh);
    }

    /// <inheritdoc />
    public IRhinoAdapter Duplicate() => new VolumeSurfaceAdapter
    (
        this.BaseMesh?.DuplicateMesh(),
        this.ComparisonMesh?.DuplicateMesh()
    );
}
