using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilCorridor = Autodesk.Civil.DatabaseServices.Corridor;
using RhinoMesh = Rhino.Geometry.Mesh;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Corridors.
/// </summary>
public class GH_CivilCorridor : GH_AutocadGeometricGoo<CivilCorridor, RhinoGeometryAdapter<RhinoMesh>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridor"/> class with no value.
    /// </summary>
    public GH_CivilCorridor()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilCorridor"/> class with the
    /// specified Civil 3D Corridor.
    /// </summary>
    /// <param name="corridor">The Civil 3D Corridor to wrap.</param>
    public GH_CivilCorridor(CivilCorridor corridor) : base(corridor)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input corridor.
    /// </summary>
    private GH_CivilCorridor(CivilCorridor corridor, IAutocadReferenceId referenceId) : base(corridor, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilCorridor, RhinoGeometryAdapter<RhinoMesh>> CreateClonedInstance(CivilCorridor entity)
    {
        return new GH_CivilCorridor(entity.Clone() as CivilCorridor, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilCorridor, RhinoGeometryAdapter<RhinoMesh>> CreateInstance(CivilCorridor entity)
    {
        return new GH_CivilCorridor(entity);
    }

    /// <inheritdoc />
    protected override CivilCorridor? Convert(RhinoGeometryAdapter<RhinoMesh> rhinoType)
    {
        // Converting from Rhino Mesh back to Civil 3D Corridor is not supported
        return null;
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoMesh>? Convert(CivilCorridor wrapperType)
    {
        var mesh = wrapperType.ToRhinoMesh();
        return mesh != null ? new RhinoGeometryAdapter<RhinoMesh>(mesh) : null;
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        var mesh = this.RhinoGeometry?.Geometry;
        if (mesh != null)
        {
            args.Pipeline.DrawMeshWires(mesh, args.Color, args.Thickness);
        }
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        var mesh = this.RhinoGeometry?.Geometry;
        if (mesh != null)
        {
            args.Pipeline.DrawMeshShaded(mesh, args.Material);
        }
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var mesh = this.RhinoGeometry?.Geometry;

        if (mesh == null) return;

        previewData.Meshes.Add(mesh);

        var polylines = mesh.GetNakedEdges();

        if (polylines != null)
        {
            foreach (var polyline in polylines)
            {
                previewData.Wires.Add(new PolylineCurve(polyline));
            }
        }
    }
}
