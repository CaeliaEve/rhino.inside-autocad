using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using AutocadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for AutoCAD curves.
/// </summary>
public class GH_AutocadCurve : GH_AutocadGeometricGoo<AutocadCurve, RhinoGeometryAdapter<RhinoCurve>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadCurve"/> class with no
    /// value.
    /// </summary>
    public GH_AutocadCurve()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadCurve"/> class with the
    /// specified AutoCAD curve. Internally, the curve is cloned, but the autocad
    /// reference Id is maintained.
    /// </summary>
    /// <param name="curve">The AutoCAD curve to wrap.</param>
    public GH_AutocadCurve(AutocadCurve curve) : base(curve)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is not a clone of the
    /// input curve.
    /// </summary>
    private GH_AutocadCurve(AutocadCurve curve, IAutocadReferenceId referenceId) : base(curve, referenceId)
    {
    }

    private RhinoCurve? _nativeCurve;

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadCurve"/> class wrapping a native Rhino curve.
    /// </summary>
    /// <param name="curve">The Rhino curve to wrap.</param>
    public GH_AutocadCurve(RhinoCurve curve) : base()
    {
        _nativeCurve = curve;
        this.SetDirectRhinoGeometry(new RhinoGeometryAdapter<RhinoCurve>(curve));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_AutocadCurve"/> class wrapping a native Rhino curve and reference.
    /// </summary>
    /// <param name="curve">The Rhino curve to wrap.</param>
    /// <param name="referenceId">The AutoCAD reference identifier.</param>
    public GH_AutocadCurve(RhinoCurve curve, IAutocadReferenceId referenceId) : base()
    {
        _nativeCurve = curve;
        this.Reference = referenceId;
        this.SetDirectRhinoGeometry(new RhinoGeometryAdapter<RhinoCurve>(curve));
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<AutocadCurve, RhinoGeometryAdapter<RhinoCurve>> CreateClonedInstance(AutocadCurve entity)
    {
        if (_nativeCurve != null)
        {
            return new GH_AutocadCurve(_nativeCurve.DuplicateCurve(), this.Reference);
        }

        if (this.Reference.IsValid)
        {
            var picker = new AutocadObjectPicker();
            if (picker.TryGetUpdatedObject(this.Reference.ObjectId, out var updatedEntity)
                && updatedEntity!.Unwrap() is AutocadCurve curve)
            {
                return new GH_AutocadCurve(curve);
            }
        }

        if (this.Value != null)
        {
            return new GH_AutocadCurve(this.Value);
        }

        if (this.RhinoGeometry?.Geometry != null)
        {
            return new GH_AutocadCurve(this.RhinoGeometry.Geometry.DuplicateCurve(), this.Reference);
        }

        return new GH_AutocadCurve();
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<AutocadCurve, RhinoGeometryAdapter<RhinoCurve>> CreateInstance(AutocadCurve entity)
    {
        if (entity == null)
        {
            if (_nativeCurve != null) return new GH_AutocadCurve(_nativeCurve.DuplicateCurve());
            if (this.RhinoGeometry?.Geometry != null) return new GH_AutocadCurve(this.RhinoGeometry.Geometry.DuplicateCurve());
        }
        return new GH_AutocadCurve(entity);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        if (_nativeCurve != null) return $"AutoCAD Curve ({_nativeCurve.GetType().Name})";
        if (this.RhinoGeometry?.Geometry != null) return $"AutoCAD Curve ({this.RhinoGeometry.Geometry.GetType().Name})";
        if (this.Value != null) return $"AutoCAD {this.Value.GetType().Name}";
        return "Null AutoCAD Curve";
    }

    /// <inheritdoc />
    protected override AutocadCurve? Convert(RhinoGeometryAdapter<RhinoCurve> rhinoType)
    {
        return rhinoType.Geometry?.ToAutocadSingleCurve();
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoCurve>? Convert(AutocadCurve wrapperType)
    {
        return new RhinoGeometryAdapter<RhinoCurve>(wrapperType.ToRhinoCurve());
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        args.Pipeline.DrawCurve(this.RhinoGeometry?.Geometry, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        return;
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var geometry = this.RhinoGeometry?.Geometry;
        if (geometry != null)
        {
            previewData.Wires.Add(geometry);
        }
    }
}

