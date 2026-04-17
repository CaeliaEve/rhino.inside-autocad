using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilParcel = Autodesk.Civil.DatabaseServices.Parcel;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Parcels.
/// </summary>
public class GH_CivilParcel : GH_AutocadGeometricGoo<CivilParcel, RhinoGeometryAdapter<RhinoCurve>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcel"/> class with no value.
    /// </summary>
    public GH_CivilParcel()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilParcel"/> class with the
    /// specified Civil 3D Parcel.
    /// </summary>
    /// <param name="parcel">The Civil 3D Parcel to wrap.</param>
    public GH_CivilParcel(CivilParcel parcel) : base(parcel)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input parcel.
    /// </summary>
    private GH_CivilParcel(CivilParcel parcel, IAutocadReferenceId referenceId) : base(parcel, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilParcel, RhinoGeometryAdapter<RhinoCurve>> CreateClonedInstance(CivilParcel entity)
    {
        return new GH_CivilParcel(entity.Clone() as CivilParcel, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilParcel, RhinoGeometryAdapter<RhinoCurve>> CreateInstance(CivilParcel entity)
    {
        return new GH_CivilParcel(entity);
    }

    /// <inheritdoc />
    protected override CivilParcel? Convert(RhinoGeometryAdapter<RhinoCurve> rhinoType)
    {
        // Converting from Rhino Curve back to Civil 3D Parcel is not supported
        return null;
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoCurve>? Convert(CivilParcel wrapperType)
    {
        var curve = wrapperType.ToRhinoBoundary();
        return curve != null ? new RhinoGeometryAdapter<RhinoCurve>(curve) : null;
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        var curve = this.RhinoGeometry?.Geometry;
        if (curve != null)
        {
            args.Pipeline.DrawCurve(curve, args.Color, args.Thickness);
        }
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        // Parcels are drawn as wires only
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var curve = this.RhinoGeometry?.Geometry;

        if (curve == null) return;

        previewData.Wires.Add(curve);
    }
}
