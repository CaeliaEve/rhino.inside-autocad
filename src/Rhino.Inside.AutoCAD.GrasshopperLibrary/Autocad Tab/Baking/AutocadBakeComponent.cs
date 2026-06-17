using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;
using System.Collections;
using Exception = System.Exception;
using RhinoArc = Rhino.Geometry.Arc;
using RhinoArcCurve = Rhino.Geometry.ArcCurve;
using RhinoBox = Rhino.Geometry.Box;
using RhinoBrep = Rhino.Geometry.Brep;
using RhinoCircle = Rhino.Geometry.Circle;
using RhinoGeometryBase = Rhino.Geometry.GeometryBase;
using RhinoLine = Rhino.Geometry.Line;
using RhinoLineCurve = Rhino.Geometry.LineCurve;
using RhinoPoint = Rhino.Geometry.Point;
using RhinoPoint3d = Rhino.Geometry.Point3d;
using RhinoRectangle3d = Rhino.Geometry.Rectangle3d;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that bakes AutoCAD objects to the model space.
/// </summary>
[ComponentVersion(introduced: "1.0.0", updated: "1.0.17")]
public class AutocadBakeComponent : RhinoInsideAutocad_ComponentBase, IBakingComponent
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C5D7E9F1-A3B5-4C7D-9E1F-3A5B7C9D1E3F");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AutocadBakeComponent;

    /// <inheritdoc />
    public int OutputParamTargetIndex => 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadBakeComponent"/> class.
    /// </summary>
    public AutocadBakeComponent()
        : base("Bake to AutoCAD", "AC-Bake",
            "Bakes objects to AutoCAD's model space",
            "AutoCAD", "Baking")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadDocument(GH_ParamAccess.item), "Document",
            "Doc", "The AutoCAD document to bake to. If not provided, the active document will be used.", GH_ParamAccess.item);
        pManager[0].Optional = true;

        pManager.AddGenericParameter("Objects", "O",
            "The objects to bake to AutoCAD (curves, points, meshes, solids)",
            GH_ParamAccess.list);

        pManager.AddParameter(new Param_BakeSettings(GH_ParamAccess.item), "Settings",
            "S", "Optional bake settings (layer, linetype, color)", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddBooleanParameter("Bake", "Bake",
            "A boolean when true the Objects will be baked to AutoCAD", GH_ParamAccess.item);

    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.list), "ObjectIds",
            "Ids", "The ObjectIds of the baked objects", GH_ParamAccess.list);
    }

    /// <summary>
    /// Wraps a Rhino <see cref="RhinoGeometryBase"/> in an <see cref="IAutocadBakeable"/>
    /// using the convertible factory, or returns <c>null</c> if it cannot be converted.
    /// </summary>
    private IAutocadBakeable? Convert(RhinoGeometryBase? geometry, IRhinoConvertibleFactory factory)
    {
        if (geometry is null)
            return null;

        if (factory.MakeConvertible(geometry, out var rhinoConvertible) == false)
            return null;

        return new BakableRhinoConverter(rhinoConvertible!);
    }

    /// <summary>
    /// Extracts an <see cref="IAutocadBakeable"/> from the input object.
    /// </summary>
    private IAutocadBakeable? ExtractBakeable(object? obj, IRhinoConvertibleFactory factory)
    {
        if (obj is IAutocadBakeable bakeable)
            return bakeable;

        if (obj is Grasshopper.Kernel.Types.IGH_Goo goo)
        {
            var valueProperty = goo.GetType().GetProperty("Value");

            if (valueProperty != null)
            {
                var value = valueProperty.GetValue(goo);

                if (value is IAutocadBakeable valueBakeable)
                    return valueBakeable;

                // Several Grasshopper primitives expose a value-type struct as their Value
                // (Line, Arc, Circle, Rectangle3d, Point3d, Box) which is not a GeometryBase.
                // Normalize these into the appropriate bakeable, mirroring the conversions used
                // by GrasshopperGeometryExtractor for previews. Breps (and Boxes, which become
                // Breps) bake via GH_AutocadBrepProxy; everything else via the convertible factory.
                var valueBakeableResult = value switch
                {
                    RhinoLine line => this.Convert(new RhinoLineCurve(line), factory),
                    RhinoArc arc => this.Convert(new RhinoArcCurve(arc), factory),
                    RhinoCircle circle => this.Convert(new RhinoArcCurve(circle), factory),
                    RhinoRectangle3d rectangle => this.Convert(rectangle.ToNurbsCurve(), factory),
                    RhinoPoint3d point => this.Convert(new RhinoPoint(point), factory),
                    RhinoBox box => new GH_AutocadBrepProxy(box.ToBrep()),
                    RhinoBrep brep => new GH_AutocadBrepProxy(brep),
                    RhinoGeometryBase nativeGeometry => this.Convert(nativeGeometry, factory),
                    _ => null
                };

                if (valueBakeableResult is not null)
                    return valueBakeableResult;
            }

            if (goo is IAutocadBakeable gooBakeable)
                return gooBakeable;

        }

        return null;
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var run = false;
        DA.GetData(3, ref run);

        if (run == false)
        {
            return;
        }

        AutocadDocument? autocadDocument = null;
        DA.GetData(0, ref autocadDocument);

        var document = this.GetDocumentOrDefault(autocadDocument);

        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No active AutoCAD document available");
            return;
        }

        var objects = new List<object>();
        if (!DA.GetDataList(1, objects) || objects.Count == 0)
            return;

        GH_BakeSettings? settingsGoo = null;
        DA.GetData(2, ref settingsGoo);

        var settings = settingsGoo?.Value;

        var converterFactory = new RhinoConvertibleFactory();

        var bakeables = new List<IAutocadBakeable>();
        foreach (var obj in objects)
        {
            var bakeable = this.ExtractBakeable(obj, converterFactory);
            if (bakeable != null)
            {
                bakeables.Add(bakeable);
            }
            else
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                    $"Object of type {obj?.GetType().Name ?? "null"} is not bakeable");
            }
        }

        if (bakeables.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No bakeable objects provided");
            return;
        }

        var bakedIds = new List<GH_AutocadObjectId>();

        var transactionManagerWrapper = document.CreateTransactionManager();

        _ = transactionManagerWrapper.PerformTask(() =>
         {
             foreach (var bakeable in bakeables)
             {
                 try
                 {
                     var objectIds = bakeable.BakeToAutocad(transactionManagerWrapper, this, settings);

                     foreach (var objectId in objectIds)
                     {
                         bakedIds.Add(new GH_AutocadObjectId(objectId));
                     }
                 }
                 catch (Exception ex)
                 {
                     this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                         $"Failed to bake object: {ex.Message}");
                 }
             }

             return true;
         });

        DA.SetDataList(0, bakedIds);
    }

    /// <inheritdoc />
    public void AddWarningMessage(string message)
    {
        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
    }

    /// <inheritdoc />
    public bool AppendDataList(IEnumerable list)
    {
        var ghParam = this.Params.Output[this.OutputParamTargetIndex];

        var path = new GH_Path(0);

        if (ghParam.VolatileData.PathCount > 0)
        {
            var lastPath = ghParam.VolatileData.Paths[ghParam.VolatileData.PathCount - 1];

            var indices = lastPath.Indices;

            indices[indices.Length - 1]++;

            path = new GH_Path(indices);
        }

        var result = ghParam.AddVolatileDataList(path, list);

        if (result)
        {
            ghParam.ExpireSolution(false);

            this.OnPingDocument()?.NewSolution(false);
        }

        return result;
    }
}