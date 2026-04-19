using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using CivilAssembly = Autodesk.Civil.DatabaseServices.Assembly;
using RhinoPoint = Rhino.Geometry.Point;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper Goo object for Civil 3D Assemblies.
/// </summary>
public class GH_CivilAssembly : GH_AutocadGeometricGoo<CivilAssembly, RhinoGeometryAdapter<RhinoPoint>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAssembly"/> class with no value.
    /// </summary>
    public GH_CivilAssembly()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GH_CivilAssembly"/> class with the
    /// specified Civil 3D Assembly.
    /// </summary>
    /// <param name="assembly">The Civil 3D Assembly to wrap.</param>
    public GH_CivilAssembly(CivilAssembly assembly) : base(assembly)
    {
    }

    /// <summary>
    /// A private constructor used to create a reference Goo which is a clone of the
    /// input assembly.
    /// </summary>
    private GH_CivilAssembly(CivilAssembly assembly, IAutocadReferenceId referenceId) : base(assembly, referenceId)
    {
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilAssembly, RhinoGeometryAdapter<RhinoPoint>> CreateClonedInstance(CivilAssembly entity)
    {
        return new GH_CivilAssembly(entity.Clone() as CivilAssembly, this.Reference);
    }

    /// <inheritdoc />
    protected override GH_AutocadGeometricGoo<CivilAssembly, RhinoGeometryAdapter<RhinoPoint>> CreateInstance(CivilAssembly entity)
    {
        return new GH_CivilAssembly(entity);
    }

    /// <inheritdoc />
    protected override CivilAssembly? Convert(RhinoGeometryAdapter<RhinoPoint> rhinoType)
    {
        // Converting from Rhino Point back to Civil 3D Assembly is not supported
        return null;
    }

    /// <inheritdoc />
    protected override RhinoGeometryAdapter<RhinoPoint>? Convert(CivilAssembly wrapperType)
    {
        var point = wrapperType.ToRhinoPoint();
        return new RhinoGeometryAdapter<RhinoPoint>(new RhinoPoint(point));
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryWires(GH_PreviewWireArgs args)
    {
        var geometry = this.RhinoGeometry?.Geometry;
        if (geometry != null)
        {
            args.Pipeline.DrawPoint(geometry.Location, args.Color);
        }
    }

    /// <inheritdoc />
    protected override void DrawViewportGeometryMeshes(GH_PreviewMeshArgs args)
    {
        // Assemblies are drawn as points only
    }

    /// <inheritdoc />
    public override void DrawAutocadPreview(IGrasshopperPreviewData previewData)
    {
        var geometry = this.RhinoGeometry?.Geometry;

        if (geometry == null) return;

        previewData.Points.Add(new RhinoPoint(geometry.Location));
    }
}
