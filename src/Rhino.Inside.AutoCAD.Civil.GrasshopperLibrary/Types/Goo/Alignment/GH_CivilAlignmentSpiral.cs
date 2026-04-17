using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D alignment spiral sub-entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilAlignmentSpiralWrapper"/> and provides
/// preview support for displaying the spiral geometry in viewports.
/// </remarks>
public class GH_CivilAlignmentSpiral : GH_GeometricGoo<CivilAlignmentSpiralWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSpiral"/> class with no value.
    /// </summary>
    public GH_CivilAlignmentSpiral()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSpiral"/> class with the
    /// specified alignment spiral wrapper.
    /// </summary>
    /// <param name="alignmentSpiral">The Civil 3D alignment spiral wrapper.</param>
    public GH_CivilAlignmentSpiral(CivilAlignmentSpiralWrapper alignmentSpiral) : base(alignmentSpiral)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignmentSpiral"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilAlignmentSpiral(GH_CivilAlignmentSpiral other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilAlignmentSpiral"/> via the interface.
    /// </summary>
    public GH_CivilAlignmentSpiral(ICivilAlignmentSpiral alignmentSpiral)
        : base((alignmentSpiral as CivilAlignmentSpiralWrapper)!)
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
                return "No alignment spiral data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid spiral geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Alignment Spiral";

    /// <inheritdoc />
    public override string TypeDescription => "A spiral sub-entity from a Civil 3D Alignment";

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
        return new GH_CivilAlignmentSpiral(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilAlignmentSpiral(this);
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

        var transformedCurve = Value.Curve.DuplicateCurve();
        transformedCurve.Transform(xform);

        var transformed = new CivilAlignmentSpiralWrapper(
            transformedCurve,
            Value.StartStation,
            Value.EndStation,
            transformedCurve.GetLength(),
            Value.RadiusIn,
            Value.RadiusOut,
            Value.SpiralType,
            Value.IsClockwise,
            Value.Index);

        return new GH_CivilAlignmentSpiral(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilAlignmentSpiralWrapper(
            morphedCurve,
            Value.StartStation,
            Value.EndStation,
            morphedCurve.GetLength(),
            Value.RadiusIn,
            Value.RadiusOut,
            Value.SpiralType,
            Value.IsClockwise,
            Value.Index);

        return new GH_CivilAlignmentSpiral(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilAlignmentSpiral goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilAlignmentSpiralWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilAlignmentSpiral alignmentSpiral)
        {
            Value = (alignmentSpiral as CivilAlignmentSpiralWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilAlignmentSpiralWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilAlignmentSpiral)))
        {
            target = (Q)(object)new GH_CivilAlignmentSpiral(this);
            return true;
        }

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(Value.Curve.DuplicateCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && Value?.Curve != null)
        {
            target = (Q)(object)Value.Curve.DuplicateCurve();
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
        // Spirals are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Alignment Spiral";

        return Value.ToString();
    }
}
