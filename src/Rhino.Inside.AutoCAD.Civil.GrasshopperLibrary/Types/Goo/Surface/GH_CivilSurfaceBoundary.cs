using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D surface boundaries.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilSurfaceBoundary"/> and provides
/// preview support for displaying the boundary polyline in viewports.
/// </remarks>
public class GH_CivilSurfaceBoundary : GH_GeometricGoo<CivilSurfaceBoundary>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBoundary"/> class with no value.
    /// </summary>
    public GH_CivilSurfaceBoundary()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBoundary"/> class with the
    /// specified boundary wrapper.
    /// </summary>
    /// <param name="boundary">The Civil 3D surface boundary wrapper.</param>
    public GH_CivilSurfaceBoundary(CivilSurfaceBoundary boundary) : base(boundary)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilSurfaceBoundary"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilSurfaceBoundary(GH_CivilSurfaceBoundary other) : base(other.Value?.Duplicate())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilSurfaceBoundary"/> via the interface.
    /// </summary>
    public GH_CivilSurfaceBoundary(ICivilSurfaceBoundary boundary)
        : base((boundary as CivilSurfaceBoundary)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid => Value?.Polyline != null && Value.Polyline.Count >= 3;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No boundary data";
            if (Value.Polyline == null || Value.Polyline.Count < 3)
                return "Invalid boundary geometry";
            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Surface Boundary";

    /// <inheritdoc />
    public override string TypeDescription => "A boundary definition from a Civil 3D TIN Surface";

    /// <inheritdoc />
    public override BoundingBox Boundingbox
    {
        get
        {
            if (Value?.Polyline == null || Value.Polyline.Count == 0)
                return BoundingBox.Empty;

            return Value.Polyline.BoundingBox;
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => Boundingbox;

    /// <inheritdoc />
    public override IGH_Goo Duplicate()
    {
        return new GH_CivilSurfaceBoundary(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilSurfaceBoundary(this);
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
        if (Value?.Polyline == null)
            return this;

        var transformedPolyline = new Polyline(Value.Polyline);
        transformedPolyline.Transform(xform);

        var transformed = new CivilSurfaceBoundary(
            Value.BoundaryType,
            transformedPolyline,
            Value.Name);

        return new GH_CivilSurfaceBoundary(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Polyline == null)
            return this;

        var morphedPolyline = new Polyline(Value.Polyline.Count);
        foreach (var point in Value.Polyline)
        {
            morphedPolyline.Add(xmorph.MorphPoint(point));
        }

        var morphed = new CivilSurfaceBoundary(
            Value.BoundaryType,
            morphedPolyline,
            Value.Name);

        return new GH_CivilSurfaceBoundary(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSurfaceBoundary goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSurfaceBoundary wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSurfaceBoundary boundary)
        {
            Value = (boundary as CivilSurfaceBoundary)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSurfaceBoundary)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSurfaceBoundary)))
        {
            target = (Q)(object)new GH_CivilSurfaceBoundary(this);
            return true;
        }

        // Cast to curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && Value?.Polyline != null)
        {
            var curve = new PolylineCurve(Value.Polyline);
            target = (Q)(object)new GH_Curve(curve);
            return true;
        }

        // Cast to polyline
        if (typeof(Q).IsAssignableFrom(typeof(Polyline)) && Value?.Polyline != null)
        {
            target = (Q)(object)new Polyline(Value.Polyline);
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        if (Value?.Polyline == null || Value.Polyline.Count < 2)
            return;

        args.Pipeline.DrawPolyline(Value.Polyline, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Boundaries are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Surface Boundary";

        return Value.ToString();
    }
}
