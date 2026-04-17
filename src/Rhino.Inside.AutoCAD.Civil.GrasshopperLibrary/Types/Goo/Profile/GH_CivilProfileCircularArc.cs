using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D profile circular arc entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileCircularArcWrapper"/> and provides
/// preview support for displaying the arc in viewports.
/// </remarks>
public class GH_CivilProfileCircularArc : GH_GeometricGoo<CivilProfileCircularArcWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileCircularArc"/> class with no value.
    /// </summary>
    public GH_CivilProfileCircularArc()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileCircularArc"/> class with the
    /// specified arc wrapper.
    /// </summary>
    /// <param name="arc">The Civil 3D profile circular arc wrapper.</param>
    public GH_CivilProfileCircularArc(CivilProfileCircularArcWrapper arc) : base(arc)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileCircularArc"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileCircularArc(GH_CivilProfileCircularArc other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileCircularArc"/> via the interface.
    /// </summary>
    public GH_CivilProfileCircularArc(ICivilProfileCircularArc arc)
        : base((arc as CivilProfileCircularArcWrapper)!)
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
                return "No profile circular arc data";
            if (this.Value.Curve == null || !this.Value.Curve.IsValid)
                return "Invalid arc geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Profile Circular Arc";

    /// <inheritdoc />
    public override string TypeDescription => "A circular arc entity from a Civil 3D Profile";

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
        return new GH_CivilProfileCircularArc(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilProfileCircularArc(this);
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

        var transformedArc = this.Value.Arc;
        transformedArc.Transform(xform);

        var transformedCenter = this.Value.CenterPoint;
        transformedCenter.Transform(xform);

        var transformed = new CivilProfileCircularArcWrapper(
            this.Value.StartStation,
            this.Value.EndStation,
            this.Value.StartElevation,
            this.Value.EndElevation,
            this.Value.Length,
            this.Value.EntityIndex,
            transformedArc,
            transformedCurve);

        return new GH_CivilProfileCircularArc(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (this.Value?.Curve == null)
            return this;

        var morphedCurve = this.Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        // Cannot properly morph arc, keep original values
        var morphed = new CivilProfileCircularArcWrapper(
            this.Value.StartStation,
            this.Value.EndStation,
            this.Value.StartElevation,
            this.Value.EndElevation,
            this.Value.Length,
            this.Value.EntityIndex,
            this.Value.Arc,
            morphedCurve);

        return new GH_CivilProfileCircularArc(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileCircularArc goo)
        {
            this.Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileCircularArcWrapper wrapper)
        {
            this.Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileCircularArc arc)
        {
            this.Value = (arc as CivilProfileCircularArcWrapper)?.Duplicate();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileCircularArcWrapper)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileCircularArc)))
        {
            target = (Q)(object)new GH_CivilProfileCircularArc(this);
            return true;
        }

        // Cast to GH_Arc
        if (typeof(Q).IsAssignableFrom(typeof(GH_Arc)) && this.Value != null)
        {
            target = (Q)(object)new GH_Arc(this.Value.Arc);
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
        // Profile arcs are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Profile Circular Arc";

        return this.Value.ToString();
    }
}
