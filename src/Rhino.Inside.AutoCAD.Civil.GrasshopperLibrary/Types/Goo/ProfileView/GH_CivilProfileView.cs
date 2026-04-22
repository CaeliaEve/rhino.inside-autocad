using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilProfileView = Autodesk.Civil.DatabaseServices.ProfileView;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D ProfileViews.
/// </summary>
public class GH_CivilProfileView : GH_CivilOneWayGoo<CivilProfileView, RhinoGraphAdapter>
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
    protected override GH_CivilOneWayGoo<CivilProfileView, RhinoGraphAdapter> CreateClonedInstance(CivilProfileView entity)
    {
        return new GH_CivilProfileView(entity.Clone() as CivilProfileView, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_CivilOneWayGoo<CivilProfileView, RhinoGraphAdapter> CreateInstance(CivilProfileView entity)
    {
        return new GH_CivilProfileView(entity);
    }

    /// <inheritdoc />
    protected override RhinoGraphAdapter? ConvertToRhino(CivilProfileView wrapperType)
    {
        var profileViewGeometry = wrapperType.ToRhinoCurves();

        return new RhinoGraphAdapter(profileViewGeometry.Curves, profileViewGeometry.TextEntities);
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        var curves = this.RhinoGeometry?.Curves;
        if (curves != null)
        {
            foreach (var curve in curves)
            {
                args.Pipeline.DrawCurve(curve, args.Color, args.Thickness);
            }
        }

        var texts = this.RhinoGeometry?.TextEntities;
        if (texts != null)
        {
            foreach (var text in texts)
            {
                args.Pipeline.DrawText(text, args.Color, text.DimensionScale);
            }
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
        var curve = this.RhinoGeometry?.Curves;

        if (curve == null) return;

        previewData.Wires.AddRange(curve);
    }
}
