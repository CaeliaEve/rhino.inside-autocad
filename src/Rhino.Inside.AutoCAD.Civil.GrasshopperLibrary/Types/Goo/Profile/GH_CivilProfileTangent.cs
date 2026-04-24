using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D profile tangent entities.
/// </summary>
/// <remarks>
/// This Goo wraps a <see cref="CivilProfileTangentWrapper"/> and provides
/// preview support for displaying the tangent line in viewports.
/// </remarks>
public class GH_CivilProfileTangent : GH_GeometricGoo<CivilProfileTangentWrapper>, IGH_PreviewData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileTangent"/> class with no value.
    /// </summary>
    public GH_CivilProfileTangent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileTangent"/> class with the
    /// specified tangent wrapper.
    /// </summary>
    /// <param name="tangent">The Civil 3D profile tangent wrapper.</param>
    public GH_CivilProfileTangent(CivilProfileTangentWrapper tangent) : base(tangent)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileTangent"/> class by copying
    /// another instance.
    /// </summary>
    /// <param name="other">The instance to copy.</param>
    public GH_CivilProfileTangent(GH_CivilProfileTangent other) : base((CivilProfileTangentWrapper)other.Value?.ShallowClone())
    {
    }

    /// <summary>
    /// Constructs a new <see cref="GH_CivilProfileTangent"/> via the interface.
    /// </summary>
    public GH_CivilProfileTangent(ICivilProfileTangent tangent)
        : base((tangent as CivilProfileTangentWrapper)!)
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
                return "No profile tangent data";

            var rhinoCurve = this.Value?.ToRhinoCurve();

            if (rhinoCurve is not { IsValid: true })
                return "Invalid tangent geometry";

            return string.Empty;
        }
    }

    /// <inheritdoc />
    public override string TypeName => "Civil3d Profile Tangent";

    /// <inheritdoc />
    public override string TypeDescription => "A tangent (straight line) entity from a Civil 3D Profile";

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
        return new GH_CivilProfileTangent(this);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo DuplicateGeometry()
    {
        return new GH_CivilProfileTangent(this);
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
        if (source is GH_CivilProfileTangent goo)
        {
            this.Value = (CivilProfileTangentWrapper)goo.Value?.ShallowClone();
            return true;
        }

        if (source is CivilProfileTangentWrapper wrapper)
        {
            this.Value = (CivilProfileTangentWrapper)wrapper.ShallowClone();
            return true;
        }

        if (source is ICivilProfileTangent tangent)
        {
            this.Value = (CivilProfileTangentWrapper)(tangent as CivilProfileTangentWrapper)?.ShallowClone();
            return this.Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileTangentWrapper)))
        {
            target = (Q)(object)this.Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileTangent)))
        {
            target = (Q)(object)new GH_CivilProfileTangent(this);
            return true;
        }

        var rhinoCurve = this.Value?.ToRhinoCurve();

        // Cast to GH_Line
        if (typeof(Q).IsAssignableFrom(typeof(GH_Line)) && this.Value != null)
        {
            target = (Q)(object)new GH_Line(new Line(rhinoCurve.PointAtStart, rhinoCurve.PointAtEnd));
            return true;
        }

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
        // Profile tangents are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (this.Value == null)
            return "Null Civil3d Profile Tangent";

        return this.Value.ToString();
    }
}
