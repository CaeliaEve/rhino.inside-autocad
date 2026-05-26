using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D surface breaklines.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSurfaceBreakline"/> and provides
/// preview support for displaying the breakline curve in viewports.
/// </remarks>
public class GH_CivilSurfaceBreakline : GH_GeometricGoo<CivilSurfaceBreakline>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBreakline"/> class with no value.
    /// </summary>
    public GH_CivilSurfaceBreakline()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBreakline"/> class with the
    /// specified breakline wrapper.
    /// </summary>
    /// <param name="breakline">The Civil 3D surface breakline wrapper.</param>
    public GH_CivilSurfaceBreakline(CivilSurfaceBreakline breakline) : base(breakline)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBreakline"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSurfaceBreakline(GH_CivilSurfaceBreakline other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSurfaceBreakline"/> via the interface.
    /// </summary>
    public GH_CivilSurfaceBreakline(ICivilSurfaceBreakline breakline)
        : base((breakline as CivilSurfaceBreakline)!)
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
                return "No breakline data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid breakline geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Surface Breakline";

    /// <inheritdoc />
    public override string TypeDescription => "A breakline extracted from a Civil 3D TIN Surface";

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
        return new GH_CivilSurfaceBreakline(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilSurfaceBreakline(this);
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

        var transformed = new CivilSurfaceBreakline(
            Value.BreaklineType,
            transformedCurve,
            Value.Name);

        return new GH_CivilSurfaceBreakline(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilSurfaceBreakline(
            Value.BreaklineType,
            morphedCurve,
            Value.Name);

        return new GH_CivilSurfaceBreakline(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSurfaceBreakline goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSurfaceBreakline wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSurfaceBreakline breakline)
        {
            Value = (breakline as CivilSurfaceBreakline)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSurfaceBreakline)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSurfaceBreakline)))
        {
            target = (Q)(object)new GH_CivilSurfaceBreakline(this);
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
        // Breaklines are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Surface Breakline";

        return Value.ToString();
    }
}
