using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilVolumeSurface = Autodesk.Civil.DatabaseServices.TinVolumeSurface;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D TIN Volume Surfaces.
/// </summary>
public class GH_CivilTinVolumeSurface : GH_AutocadGeometricGoo<CivilVolumeSurface, RhinoMesh>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinVolumeSurface"/> class with no value.
    /// </summary>
    public GH_CivilTinVolumeSurface()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinVolumeSurface"/> class with the
    /// specified Civil 3D TIN Volume Surface. Internally, the surface is cloned, but the autocad
    /// reference Id is maintained.
    /// </summary>
    /// <param name="volumeSurface">The Civil 3D TIN Volume Surface to wrap.</param>
    public GH_CivilTinVolumeSurface(CivilVolumeSurface volumeSurface) : base(volumeSurface)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input volume surface.
    /// </summary>
    private GH_CivilTinVolumeSurface(CivilVolumeSurface volumeSurface, IAutocadReferenceId referenceId)
        : base(volumeSurface, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilVolumeSurface, RhinoMesh> CreateClonedInstance(
        CivilVolumeSurface entity)
    {
        return new GH_CivilTinVolumeSurface(entity.Clone() as CivilVolumeSurface, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilVolumeSurface, RhinoMesh> CreateInstance(
        CivilVolumeSurface entity)
    {
        return new GH_CivilTinVolumeSurface(entity);
    }

    /// <inheritdoc />
    protected override CivilVolumeSurface? Convert(RhinoMesh rhinoType)
    {
        // Volume surfaces cannot be created from a Rhino mesh directly
        // They require two TIN surfaces to be created
        return null;
    }

    /// <inheritdoc />
    protected override RhinoMesh? Convert(CivilVolumeSurface wrapperType)
    {
        return wrapperType.ToRhinoMesh();
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        args.Pipeline.DrawMeshWires(this.RhinoGeometry, args.Color, args.Thickness);
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        args.Pipeline.DrawMeshShaded(this.RhinoGeometry, args.Material);
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var rhinoGeometry = this.RhinoGeometry;

        if (rhinoGeometry == null) return;

        previewData.Meshes.Add(rhinoGeometry);

        var polylines = rhinoGeometry.GetNakedEdges();

        foreach (var polyline in polylines)
        {
            previewData.Wires.Add(new PolylineCurve(polyline));
        }
    }
}
