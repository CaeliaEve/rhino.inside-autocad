using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment sub-entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentSubEntityWrapper"/> and provides
/// preview support for displaying the sub-entity curve in viewports.
/// </remarks>
public class GH_CivilAlignmentSubEntity : GH_GeometricGoo<CivilAlignmentSubEntityWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSubEntity"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentSubEntity()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSubEntity"/> class with the
    /// specified sub-entity wrapper.
    /// </summary>
    /// <param name="subEntity">The Civil 3D alignment sub-entity wrapper.</param>
    public GH_CivilAlignmentSubEntity(CivilAlignmentSubEntityWrapper subEntity) : base(subEntity)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSubEntity"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentSubEntity(GH_CivilAlignmentSubEntity other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentSubEntity"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentSubEntity(ICivilAlignmentSubEntity subEntity)
        : base((subEntity as CivilAlignmentSubEntityWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid
    {
        get
        {
            var rhinoCurve = this.Value?.ToRhinoCurve();
            return rhinoCurve is { IsValid: true };
        }
    }

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return "No alignment sub-entity data";

            var rhinoCurve = this.Value?.ToRhinoCurve();

            if (rhinoCurve == null || !rhinoCurve.IsValid)
                return "Invalid sub-entity geometry";

            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Sub-Entity";

    /// <inheritdoc />
    public override string TypeDescription => "A sub-entity (Line, Arc, Spiral) from a Civil 3D Alignment Entity";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            var rhinoCurve = this.Value?.ToRhinoCurve();

            if (rhinoCurve == null)
                return BoundingBox.Empty;

            return rhinoCurve.GetBoundingBox(true);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentSubEntity(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilAlignmentSubEntity(this);
    }

    /// <inheritdoc />
    public override BoundingBox GetBoundingBox(Transform xform)
    {
        var box = this.Boundingbox;
        box.Transform(xform);
        return box;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Transform(Transform xform)
    {
        // These are read-only wrappers around Civil 3D alignment sub-entities,
        // so we won't apply transformations to the underlying geometry.
        return this;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        // These are read-only wrappers around Civil 3D alignment sub-entities,
        // so we won't apply transformations to the underlying geometry.
        return this;
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentSubEntity goo)
        {
            this.Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilAlignmentSubEntityWrapper wrapper)
        {
            this.Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilAlignmentSubEntity subEntity)
        {
            this.Value = (subEntity as CivilAlignmentSubEntityWrapper)?.ShallowClone();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentSubEntityWrapper)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentSubEntity)))
        {
            target = (Q)(object)new GH_CivilAlignmentSubEntity(this);
            return true;
        }

        var rhinoCurve = this.Value?.ToRhinoCurve();

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && rhinoCurve != null)
        {
            target = (Q)(object)new GH_Curve(rhinoCurve.DuplicateCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && rhinoCurve != null)
        {
            target = (Q)(object)rhinoCurve.DuplicateCurve();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        var rhinoCurve = this.Value?.ToRhinoCurve();

        if (rhinoCurve == null)
            return;

        args.Pipeline.DrawCurve(rhinoCurve, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Alignment sub-entities are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Alignment Sub-Entity";

        return this.Value.ToString();
    }
}
