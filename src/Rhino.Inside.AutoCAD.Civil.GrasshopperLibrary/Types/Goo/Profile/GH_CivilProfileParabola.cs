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
/// This Goo wraps a <see cref="CivilProfileSymmetricalParabolaWrapper"/> and provides
/// preview support for displaying the parabola in viewports.
/// </remarks>
public class GH_CivilProfileParabola : GH_GeometricGoo<CivilProfileSymmetricalParabolaWrapper>, IGH_PreviewData
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
    /// <param name="symmetricalParabola">The Civil 3D profile parabola wrapper.</param>
    public GH_CivilProfileParabola(CivilProfileSymmetricalParabolaWrapper symmetricalParabola) : base(symmetricalParabola)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileParabola"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileParabola(GH_CivilProfileParabola other) : base((CivilProfileSymmetricalParabolaWrapper)other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileParabola"/> via the interface.
    /// </summary>
    public GH_CivilProfileParabola(ICivilProfileSymmetricalParabola symmetricalParabola)
        : base((symmetricalParabola as CivilProfileSymmetricalParabolaWrapper)!)
    {
    }

    /// <inheritdoc />
    public override bool IsValid
    {
        get
        {
            var rhinoCurve = this.Value?.ToRhinoCurve();
            return rhinoCurve is { IsValid: true };
        }
    }

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (this.Value == null)
                return "No profile parabola data";

            var rhinoCurve = this.Value?.ToRhinoCurve();

            if (rhinoCurve is not { IsValid: true })
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
            var rhinoCurve = this.Value?.ToRhinoCurve();
            if (rhinoCurve == null)
                return BoundingBox.Empty;

            return rhinoCurve.GetBoundingBox(true);
        }
    }

    /// <inheritdoc />
    public BoundingBox ClippingBox => this.Boundingbox;

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
        var box = this.Boundingbox;
        box.Transform(xform);
        return box;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Transform(Transform xform)
    {
        // These are read-only wrappers around Civil 3D profile entities,
        // so we won't apply transformations to the underlying geometry.
        return this;
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        // These are read-only wrappers around Civil 3D profile entities,
        // so we won't apply transformations to the underlying geometry.
        return this;
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileParabola goo)
        {
            this.Value = (CivilProfileSymmetricalParabolaWrapper)goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilProfileSymmetricalParabolaWrapper wrapper)
        {
            this.Value = (CivilProfileSymmetricalParabolaWrapper)wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilProfileSymmetricalParabola parabola)
        {
            this.Value = (CivilProfileSymmetricalParabolaWrapper)(parabola as CivilProfileSymmetricalParabolaWrapper)?.ShallowClone();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileSymmetricalParabolaWrapper)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileParabola)))
        {
            target = (Q)(object)new GH_CivilProfileParabola(this);
            return true;
        }

        var rhinoCurve = this.Value?.ToRhinoCurve();

        // Cast to GH_Curve
        if (typeof(Q).IsAssignableFrom(typeof(GH_Curve)) && rhinoCurve != null)
        {
            target = (Q)(object)new GH_Curve(rhinoCurve.DuplicateCurve());
            return true;
        }

        // Cast to Curve
        if (typeof(Q).IsAssignableFrom(typeof(Curve)) && rhinoCurve != null)
        {
            target = (Q)(object)rhinoCurve.DuplicateCurve();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void DrawViewportWires(GH_PreviewWireArgs args)
    {
        var rhinoCurve = this.Value?.ToRhinoCurve();

        if (rhinoCurve == null)
            return;

        args.Pipeline.DrawCurve(rhinoCurve, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    public void DrawViewportMeshes(GH_PreviewMeshArgs args)
    {
        // Profile parabolas are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Profile Parabola";

        return this.Value.ToString();
    }
}
