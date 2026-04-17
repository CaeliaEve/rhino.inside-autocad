using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment arc sub-entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentArcWrapper"/> and provides
/// preview support for displaying the arc geometry in viewports.
/// </remarks>
public class GH_CivilAlignmentArc : GH_GeometricGoo<CivilAlignmentArcWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentArc"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentArc()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentArc"/> class with the
    /// specified alignment arc wrapper.
    /// </summary>
    /// <param name="alignmentArc">The Civil 3D alignment arc wrapper.</param>
    public GH_CivilAlignmentArc(CivilAlignmentArcWrapper alignmentArc) : base(alignmentArc)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentArc"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentArc(GH_CivilAlignmentArc other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentArc"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentArc(ICivilAlignmentArc alignmentArc)
        : base((alignmentArc as CivilAlignmentArcWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value != null && Value.Arc.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No alignment arc data";
            if (!Value.Arc.IsValid)
                return "Invalid arc geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Arc";

    /// <inheritdoc />
    public override string TypeDescription => "An arc sub-entity from a Civil 3D Alignment";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value == null)
                return BoundingBox.Empty;

            return Value.Arc.BoundingBox();
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilAlignmentArc(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilAlignmentArc(this);
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
        if (Value == null)
            return this;

        var transformedArc = Value.Arc;
        transformedArc.Transform(xform);
        var transformedCenter = Value.CenterPoint;
        transformedCenter.Transform(xform);

        var transformed = new CivilAlignmentArcWrapper(
            transformedArc,
            Value.StartStation,
            Value.EndStation,
            transformedArc.Length,
            transformedArc.Radius,
            transformedCenter,
            Value.IsClockwise,
            Value.Index);

        return new GH_CivilAlignmentArc(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value == null)
            return this;

        // Arcs need to be converted to curves for morphing
        var arcCurve = new ArcCurve(Value.Arc);
        xmorph.Morph(arcCurve);

        // Try to fit back to an arc
        if (arcCurve.TryGetArc(out Arc morphedArc))
        {
            var morphed = new CivilAlignmentArcWrapper(
                morphedArc,
                Value.StartStation,
                Value.EndStation,
                morphedArc.Length,
                morphedArc.Radius,
                morphedArc.Center,
                Value.IsClockwise,
                Value.Index);

            return new GH_CivilAlignmentArc(morphed);
        }

        // If morphing distorted the arc too much, return original
        return new GH_CivilAlignmentArc(this);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentArc goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAlignmentArcWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAlignmentArc alignmentArc)
        {
            Value = (alignmentArc as CivilAlignmentArcWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentArcWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentArc)))
        {
            target = (Q)(object)new GH_CivilAlignmentArc(this);
            return true;
        }

        // Cast to GH_Arc
        if (typeof(Q).IsAssignableFrom(typeof(GH_Arc)) && Value != null)
        {
            target = (Q)(object)new GH_Arc(Value.Arc);
            return true;
        }

        // Cast to Arc
        if (typeof(Q).IsAssignableFrom(typeof(Arc)) && Value != null)
        {
            target = (Q)(object)Value.Arc;
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value != null)
        {
            target = (Q)(object)new GH_Curve(new ArcCurve(Value.Arc));
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value != null)
        {
            target = (Q)(object)new ArcCurve(Value.Arc);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value == null)
            return;

        args.Pipeline.DrawArc(Value.Arc, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Arcs are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Arc";

        return Value.ToString();
    }
}
