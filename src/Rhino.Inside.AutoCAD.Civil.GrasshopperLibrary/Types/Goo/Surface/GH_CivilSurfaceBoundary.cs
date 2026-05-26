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
/// preview support for displaying the boundary Curve in viewports.
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
    public override bool IsValid => this.Value?.Curve != null;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return "No boundary data";
            if (this.Value.Curve == null)
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
            if (this.Value?.Curve == null)
                return BoundingBox.Empty;

            return this.Value.Curve.GetBoundingBox(false);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Boundingbox;

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

        var transformed = new CivilSurfaceBoundary(
            this.Value.BoundaryType,
            transformedCurve,
            this.Value.Name);

        return new GH_CivilSurfaceBoundary(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (this.Value?.Curve == null)
            return this;

        var morphedCurve = this.Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        var morphed = new CivilSurfaceBoundary(
            this.Value.BoundaryType,
            morphedCurve,
            this.Value.Name);

        return new GH_CivilSurfaceBoundary(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilSurfaceBoundary goo)
        {
            this.Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilSurfaceBoundary wrapper)
        {
            this.Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilSurfaceBoundary boundary)
        {
            this.Value = (boundary as CivilSurfaceBoundary)?.Duplicate();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilSurfaceBoundary)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilSurfaceBoundary)))
        {
            target = (Q)(object)new GH_CivilSurfaceBoundary(this);
            return true;
        }

        // Cast to curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && this.Value?.Curve != null)
        {
            target = (Q)(object)new GH_Curve(this.Value.Curve);
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && this.Value?.Curve != null)
        {
            target = (Q)(object)this.Value.Curve;
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
        // Boundaries are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Surface Boundary";

        return this.Value.ToString();
    }
}
