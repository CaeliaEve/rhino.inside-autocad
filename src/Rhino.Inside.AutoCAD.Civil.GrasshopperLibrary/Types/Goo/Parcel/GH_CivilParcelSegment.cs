using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D parcel segments.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilParcelSegmentWrapper"/> and provides
/// preview support for displaying the segment curve in viewports.
/// </remarks>
public class GH_CivilParcelSegment : GH_GeometricGoo<CivilParcelSegment>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelSegment"/> class with no value.
    /// </summary>
    public GH_CivilParcelSegment()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelSegment"/> class with the
    /// specified segment wrapper.
    /// </summary>
    /// <param name="segment">The Civil 3D parcel segment wrapper.</param>
    public GH_CivilParcelSegment(CivilParcelSegment segment) : base(segment)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcelSegment"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilParcelSegment(GH_CivilParcelSegment other) : base(other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilParcelSegment"/> via the interface.
    /// </summary>
    public GH_CivilParcelSegment(ICivilParcelSegment segment)
        : base((segment as CivilParcelSegment)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => this.Value?.Curve != null && this.Value.Curve.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return "No parcel segment data";
            if (this.Value.Curve == null || !this.Value.Curve.IsValid)
                return "Invalid segment geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Parcel Segment";

    /// <inheritdoc />
    public override string TypeDescription => "A boundary segment (Line, Arc) from a Civil 3D Parcel";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (this.Value?.Curve == null)
                return BoundingBox.Empty;

            return this.Value.Curve.GetBoundingBox(true);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilParcelSegment(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilParcelSegment(this);
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
        if (this.Value?.Curve == null)
            return this;

        var transformedCurve = this.Value.Curve.DuplicateCurve();
        transformedCurve.Transform(xform);

        var transformed = new CivilParcelSegment(transformedCurve,
            this.Value.Index);

        return new GH_CivilParcelSegment(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (this.Value?.Curve == null)
            return this;

        var morphedCurve = this.Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilParcelSegment(morphedCurve,
            this.Value.Index);

        return new GH_CivilParcelSegment(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilParcelSegment goo)
        {
            this.Value = goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilParcelSegment wrapper)
        {
            this.Value = wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilParcelSegment segment)
        {
            this.Value = (segment as CivilParcelSegment)?.ShallowClone();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilParcelSegment)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilParcelSegment)))
        {
            target = (Q)(object)new GH_CivilParcelSegment(this);
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && this.Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(this.Value.Curve.DuplicateCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && this.Value?.Curve != null)
        {
            target = (Q)(object)this.Value.Curve.DuplicateCurve();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (this.Value?.Curve == null)
            return;

        args.Pipeline.DrawCurve(this.Value.Curve, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Parcel segments are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Parcel Segment";

        return this.Value.ToString();
    }
}
