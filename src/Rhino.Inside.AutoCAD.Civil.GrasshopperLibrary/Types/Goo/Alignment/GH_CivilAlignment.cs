using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilAlignment = Autodesk.Civil.DatabaseServices.Alignment;
using RhinoCurve = Rhino.Geometry.Curve;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Alignments.
/// </summary>
public class GH_CivilAlignment : GH_CivilOneWayGoo<CivilAlignment, RhinoGeometryAdapter<RhinoCurve>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignment"/> class with no value.
    /// </summary>
    public GH_CivilAlignment()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAlignment"/> class with the
    /// specified Civil 3D Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D Alignment to wrap.</param>
    public GH_CivilAlignment(CivilAlignment alignment) : base(alignment)
    {
    }

    

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input alignment.
    /// </summary>
    private GH_CivilAlignment(CivilAlignment alignment, IAutocadReferenceId referenceId) : base(alignment, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_CivilOneWayGoo<CivilAlignment, RhinoGeometryAdapter<RhinoCurve>> CreateClonedInstance(CivilAlignment entity)
    {
        return new GH_CivilAlignment(entity.Clone() as CivilAlignment, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_CivilOneWayGoo<CivilAlignment, RhinoGeometryAdapter<RhinoCurve>> CreateInstance(CivilAlignment entity)
    {
        return new GH_CivilAlignment(entity);
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoCurve>? ConvertToRhino(CivilAlignment wrapperType)
    {
        var curve = wrapperType.ToRhinoCurve();
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
        // Alignments are drawn as wires only
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var curve = this.RhinoGeometry?.Geometry;

        if (curve == null) return;

        previewData.Wires.Add(curve);
    }
}
