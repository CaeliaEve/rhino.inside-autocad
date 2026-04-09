using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilSurface = Autodesk.Civil.DatabaseServices.TinSurface;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for AutoCAD meshes.
/// </summary>
public class GH_CivilTinSurface : GH_AutocadGeometricGoo<CivilSurface, RhinoMesh>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinSurface"/> class with no value.
    /// </summary>
    public GH_CivilTinSurface()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilTinSurface"/> class with the
    /// specified AutoCAD mesh. Internally, the mesh is cloned, but the autocad
    /// reference Id is maintained.
    /// </summary>
    /// <param name="mesh">The AutoCAD mesh to wrap.</param>
    public GH_CivilTinSurface(CivilSurface mesh) : base(mesh)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input curve.
    /// </summary>
    private GH_CivilTinSurface(CivilSurface curve, IAutocadReferenceId referenceId) : base(curve, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilSurface, RhinoMesh> CreateClonedInstance(CivilSurface entity)
    {
        return new GH_CivilTinSurface(entity.Clone() as CivilSurface, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilSurface, RhinoMesh> CreateInstance(CivilSurface entity)
    {
        return new GH_CivilTinSurface(entity);
    }

    /// <inheritdoc />
    protected override CivilSurface? Convert(RhinoMesh rhinoType)
    {
        return rhinoType.ToTinSurface();
    }

    /// <inheritdoc />
    protected override RhinoMesh? Convert(CivilSurface wrapperType)
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

