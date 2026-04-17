using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D profile parabola (vertical curve) entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileParabolaWrapper"/> and provides
/// preview support for displaying the parabola in viewports.
/// </remarks>
public class GH_CivilProfileParabola : GH_GeometricGoo<CivilProfileParabolaWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileParabola"/> class with no value.
    /// </summary>
    public GH_CivilProfileParabola()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileParabola"/> class with the
    /// specified parabola wrapper.
    /// </summary>
    /// <param name="parabola">The Civil 3D profile parabola wrapper.</param>
    public GH_CivilProfileParabola(CivilProfileParabolaWrapper parabola) : base(parabola)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileParabola"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileParabola(GH_CivilProfileParabola other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileParabola"/> via the interface.
    /// </summary>
    public GH_CivilProfileParabola(ICivilProfileParabola parabola)
        : base((parabola as CivilProfileParabolaWrapper)!)
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
                return "No profile parabola data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid parabola geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Profile Parabola";

    /// <inheritdoc />
    public override string TypeDescription => "A parabola (vertical curve) entity from a Civil 3D Profile";

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
        return new GH_CivilProfileParabola(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilProfileParabola(this);
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

        Point3d? transformedHighLow = null;
        if (Value.HighLowPoint.HasValue)
        {
            var pt = Value.HighLowPoint.Value;
            pt.Transform(xform);
            transformedHighLow = pt;
        }

        var transformed = new CivilProfileParabolaWrapper(
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            Value.KValue,
            Value.PVIStation,
            Value.PVIElevation,
            transformedHighLow,
            transformedCurve);

        return new GH_CivilProfileParabola(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        // Cannot properly morph high/low point, keep original values
        var morphed = new CivilProfileParabolaWrapper(
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            Value.KValue,
            Value.PVIStation,
            Value.PVIElevation,
            Value.HighLowPoint,
            morphedCurve);

        return new GH_CivilProfileParabola(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileParabola goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileParabolaWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileParabola parabola)
        {
            Value = (parabola as CivilProfileParabolaWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileParabolaWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileParabola)))
        {
            target = (Q)(object)new GH_CivilProfileParabola(this);
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
        // Profile parabolas are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Profile Parabola";

        return Value.ToString();
    }
}
