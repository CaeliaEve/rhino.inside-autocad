using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Core.Host;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts the geometry from an AutoCAD block.
/// </summary>
[ComponentVersion(introduced: "1.0.0")]
public class AutocadExtractBlockGeometryComponent : RhinoInsideAutocad_ComponentBase
{
    private readonly GooTypeRegistry _gooConverterRegister = GooTypeRegistry.Instance!;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.tertiary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("390c61af-4b81-475e-9907-598a42d95634");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.AutocadExtractBlockGeometryComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutocadExtractBlockGeometryComponent"/> class.
    /// </summary>
    public AutocadExtractBlockGeometryComponent()
        : base("Extract Block Geometry", "AC-ExtBlk",
            "Extracts the geometry from an AutoCAD Block Table Record or Block Reference",
            "AutoCAD", "Blocks")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddGenericParameter("Block", "Block",
            "The AutoCAD Block Table Record or Block Reference to extract geometry from", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddGeometryParameter("BlockObjects", "Objects",
             "The objects in the block", GH_ParamAccess.list);
    }

    /// <summary>
    /// Loads the geometry from a Block Table Record or Block Reference.
    /// </summary>
    /// <param name="objectId">The ObjectId of the block to find the correct document.</param>
    /// <param name="getObjectsFunc">Function to retrieve the entities from the block.</param>
    /// <returns>A collection of geometric goo objects, or null if no document is available.</returns>
    private IEnumerable<IGH_GeometricGoo>? LoadBlockObjects(IObjectId objectId, Func<IAutocadTransactionManager, IEntitySet> getObjectsFunc)
    {
        var document = this.GetDocumentForObjectId(objectId);
        if (document is null)
        {
            return null;
        }

        var transactionManagerWrapper = document.CreateTransactionManager();

        var objects = transactionManagerWrapper.PerformTask(() => getObjectsFunc.Invoke(transactionManagerWrapper));

        var blockObject = new List<IGH_GeometricGoo>();

        foreach (var entityObject in objects)
        {
            var goo = _gooConverterRegister.CreateGeometryGoo(entityObject);

            blockObject.Add(goo);
        }

        return blockObject;
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        object? generic = null;

        if (!DA.GetData(0, ref generic)
            || generic is null) return;

        var gooObjects = new List<IGH_GeometricGoo>();
        IEnumerable<IGH_GeometricGoo>? loadedObjects = null;

        switch (generic)
        {
            case GH_AutocadBlockReference gooBlockReference:
                loadedObjects = this.LoadBlockObjects(gooBlockReference.Value.Id, gooBlockReference.Value.GetObjects);
                break;
            case GH_AutocadBlockTableRecord gooBlockTableRecord:
                loadedObjects = this.LoadBlockObjects(gooBlockTableRecord.Value.Id, gooBlockTableRecord.Value.GetObjects);
                break;
            case AutocadBlockReferenceWrapper blockReferenceWrapper:
                loadedObjects = this.LoadBlockObjects(blockReferenceWrapper.Id, blockReferenceWrapper.GetObjects);
                break;
            case AutocadBlockTableRecordWrapper blockTableRecordWrapper:
                loadedObjects = this.LoadBlockObjects(blockTableRecordWrapper.Id, blockTableRecordWrapper.GetObjects);
                break;
            default:
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "Input must be a Block Reference or Block Table Record");
                return;
        }

        if (loadedObjects is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        gooObjects.AddRange(loadedObjects);
        DA.SetDataList(0, gooObjects.ToList());
    }
}