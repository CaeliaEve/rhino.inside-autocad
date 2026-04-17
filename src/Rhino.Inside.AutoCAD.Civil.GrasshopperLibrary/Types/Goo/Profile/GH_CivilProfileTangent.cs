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
    public GH_CivilProfileTangent(GH_CivilProfileTangent other) : base(other.Value?.Duplicate())
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
    public override bool IsValid => Value?.Curve != null && Value.Curve.IsValid;

    /// <inheritdoc />
    public override string IsValidWhyNot
    {
        get
        {
            if (Value == null)
                return "No profile tangent data";
            if (Value.Curve == null || !Value.Curve.IsValid)
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

        var transformedLine = Value.Line;
        transformedLine.Transform(xform);

        var transformed = new CivilProfileTangentWrapper(
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            Value.Grade,
            transformedLine,
            transformedCurve);

        return new GH_CivilProfileTangent(transformed);
    }

    /// <inheritdoc />
    public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
    {
        if (Value?.Curve == null)
            return this;

        var morphedCurve = Value.Curve.DuplicateCurve();
        xmorph.Morph(morphedCurve);

        // Line cannot be morphed directly, create from curve endpoints
        var morphedLine = new Line(morphedCurve.PointAtStart, morphedCurve.PointAtEnd);

        var morphed = new CivilProfileTangentWrapper(
            Value.StartStation,
            Value.EndStation,
            Value.StartElevation,
            Value.EndElevation,
            Value.Length,
            Value.EntityIndex,
            Value.Grade,
            morphedLine,
            morphedCurve);

        return new GH_CivilProfileTangent(morphed);
    }

    /// <inheritdoc />
    public override bool CastFrom(object source)
    {
        if (source is GH_CivilProfileTangent goo)
        {
            Value = goo.Value?.Duplicate();
            return true;
        }

        if (source is CivilProfileTangentWrapper wrapper)
        {
            Value = wrapper.Duplicate();
            return true;
        }

        if (source is ICivilProfileTangent tangent)
        {
            Value = (tangent as CivilProfileTangentWrapper)?.Duplicate();
            return Value != null;
        }

        return false;
    }

    /// <inheritdoc />
    public override bool CastTo<Q>(ref Q target)
    {
        if (typeof(Q).IsAssignableFrom(typeof(CivilProfileTangentWrapper)))
        {
            target = (Q)(object)Value!;
            return true;
        }

        if (typeof(Q).IsAssignableFrom(typeof(GH_CivilProfileTangent)))
        {
            target = (Q)(object)new GH_CivilProfileTangent(this);
            return true;
        }

        // Cast to GH_Line
        if (typeof(Q).IsAssignableFrom(typeof(GH_Line)) && Value != null)
        {
            target = (Q)(object)new GH_Line(Value.Line);
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
        // Profile tangents are drawn as wires only
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Value == null)
            return "Null Civil3d Profile Tangent";

        return Value.ToString();
    }
}
