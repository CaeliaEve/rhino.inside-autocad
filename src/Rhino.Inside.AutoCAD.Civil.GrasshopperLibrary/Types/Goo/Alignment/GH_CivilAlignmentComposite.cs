using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment composite sub-entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentCompositeWrapper"/> and provides
/// preview support for displaying the composite geometry in viewports.
/// </remarks>
public class GH_CivilAlignmentComposite : GH_GeometricGoo<CivilAlignmentCompositeWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentComposite"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentComposite()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentComposite"/> class with the
    /// specified alignment composite wrapper.
    /// </summary>
    /// <param name="alignmentComposite">The Civil 3D alignment composite wrapper.</param>
    public GH_CivilAlignmentComposite(CivilAlignmentCompositeWrapper alignmentComposite) : base(alignmentComposite)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentComposite"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentComposite(GH_CivilAlignmentComposite other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentComposite"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentComposite(ICivilAlignmentComposite alignmentComposite)
        : base((alignmentComposite as CivilAlignmentCompositeWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value?.Curve != null && Value.Curve.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No alignment composite data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid composite geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Composite";

    /// <inheritdoc />
    public override string TypeDescription => "A composite sub-entity from a Civil 3D Alignment";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value?.Curve == null)
                return BoundingBox.Empty;

            return Value.Curve.GetBoundingBox(true);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentComposite(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilAlignmentComposite(this);
    }

    /// <inheritdoc />
    public override BoundingBox GetBoundingBox(Transform xform)
    {
        var box = Boundingbox;
        box.Transform(xform);
        return box;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Transform(Transform xform)
    {
        if (Value?.Curve == null)
            return this;

        var transformedCurve = Value.Curve.DuplicatePolyCurve();
        transformedCurve.Transform(xform);

        var transformed = new CivilAlignmentCompositeWrapper(
            transformedCurve,
            Value.StartStation,
            Value.EndStation,
            transformedCurve.GetLength(),
            Value.ComponentCount,
            Value.Index);

        return new GH_CivilAlignmentComposite(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicatePolyCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilAlignmentCompositeWrapper(
            morphedCurve,
            Value.StartStation,
            Value.EndStation,
            morphedCurve.GetLength(),
            Value.ComponentCount,
            Value.Index);

        return new GH_CivilAlignmentComposite(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentComposite goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAlignmentCompositeWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAlignmentComposite alignmentComposite)
        {
            Value = (alignmentComposite as CivilAlignmentCompositeWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentCompositeWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentComposite)))
        {
            target = (Q)(object)new GH_CivilAlignmentComposite(this);
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(Value.Curve.DuplicatePolyCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)Value.Curve.DuplicatePolyCurve();
            return true;
        }

        // Cast to PolyCurve
        if (typeof(Q).IsAssignableFrom(typeof(PolyCurve)) && Value?.Curve != null)
        {
            target = (Q)(object)Value.Curve.DuplicatePolyCurve();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value?.Curve == null)
            return;

        args.Pipeline.DrawCurve(Value.Curve, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Composites are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Composite";

        return Value.ToString();
    }
}
