using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilProfileView = Autodesk.Civil.DatabaseServices.ProfileView;
using RhinoPolylineCurve = Rhino.Geometry.PolylineCurve;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D ProfileViews.
/// </summary>
public class GH_CivilProfileView : GH_AutocadGeometricGoo<CivilProfileView, RhinoGeometryAdapter<RhinoPolylineCurve>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileView"/> class with no value.
    /// </summary>
    public GH_CivilProfileView()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilProfileView"/> class with the
    /// specified Civil 3D ProfileView.
    /// </summary>
    /// <param name="profileView">The Civil 3D ProfileView to wrap.</param>
    public GH_CivilProfileView(CivilProfileView profileView) : base(profileView)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input ProfileView.
    /// </summary>
    private GH_CivilProfileView(CivilProfileView profileView, IAutocadReferenceId referenceId) : base(profileView, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilProfileView, RhinoGeometryAdapter<RhinoPolylineCurve>> CreateClonedInstance(CivilProfileView entity)
    {
        return new GH_CivilProfileView(entity.Clone() as CivilProfileView, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilProfileView, RhinoGeometryAdapter<RhinoPolylineCurve>> CreateInstance(CivilProfileView entity)
    {
        return new GH_CivilProfileView(entity);
    }

    /// <inheritdoc />
    protected override CivilProfileView? Convert(RhinoGeometryAdapter<RhinoPolylineCurve> rhinoType)
    {
        // Converting from Rhino PolylineCurve back to Civil 3D ProfileView is not supported
        return null;
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoPolylineCurve>? Convert(CivilProfileView wrapperType)
    {
        var database = wrapperType.Database;
        if (database == null)
            return null;

        // Get the display bounds as a rectangle and convert to polyline curve
        var bounds = wrapperType.GetDisplayBounds();

        // Convert Rectangle3d to a closed PolylineCurve
        var polyline = bounds.ToPolyline();
        var curve = new RhinoPolylineCurve(polyline);

        return new RhinoGeometryAdapter<RhinoPolylineCurve>(curve);
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
        // ProfileViews are drawn as wires only (rectangle bounds)
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var curve = this.RhinoGeometry?.Geometry;

        if (curve == null) return;

        previewData.Wires.Add(curve);
    }
}
