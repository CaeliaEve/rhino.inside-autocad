using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D surface contours.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSurfaceContour"/> and provides
/// preview support for displaying the contour curve in viewports.
/// </remarks>
public class GH_CivilSurfaceContour : GH_GeometricGoo<CivilSurfaceContour>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceContour"/> class with no value.
    /// </summary>
    public GH_CivilSurfaceContour()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceContour"/> class with the
    /// specified contour wrapper.
    /// </summary>
    /// <param name="contour">The Civil 3D surface contour wrapper.</param>
    public GH_CivilSurfaceContour(CivilSurfaceContour contour) : base(contour)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceContour"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSurfaceContour(GH_CivilSurfaceContour other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSurfaceContour"/> via the interface.
    /// </summary>
    public GH_CivilSurfaceContour(ICivilSurfaceContour contour)
        : base((contour as CivilSurfaceContour)!)
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
                return "No contour data";
            if (Value.Curve == null || !Value.Curve.IsValid)
                return "Invalid contour geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Surface Contour";

    /// <inheritdoc />
    public override string TypeDescription => "A contour line extracted from a Civil 3D TIN Surface";

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
        return new GH_CivilSurfaceContour(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilSurfaceContour(this);
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

        var transformed = new CivilSurfaceContour(
            Value.ContourType,
            transformedCurve,
            Value.Elevation);

        return new GH_CivilSurfaceContour(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilSurfaceContour(
            Value.ContourType,
            morphedCurve,
            Value.Elevation);

        return new GH_CivilSurfaceContour(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSurfaceContour goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSurfaceContour wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSurfaceContour contour)
        {
            Value = (contour as CivilSurfaceContour)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSurfaceContour)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSurfaceContour)))
        {
            target = (Q)(object)new GH_CivilSurfaceContour(this);
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
        // Contours are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Surface Contour";

        return Value.ToString();
    }
}
