using Autodesk.AutoCAD.DatabaseServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Generic deconstructor component for Civil 3D Alignment labels.
/// Works with any label type (Curve, Spiral, Tangent, PI, IndexedPI).
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentLabelComponent : RhinoInsideAutocad_ComponentBase
{
    private readonly GooTypeRegistry _gooConverterRegister = GooTypeRegistry.Instance!;

    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDEF2");
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    public CivilAlignmentLabelComponent()
        : base("Civil3d Alignment Label", "CVL-AlignLbl",
            "Extracts values from any Civil 3D Alignment Label type.",
            "Civil3d", "Labels")
    { }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilFeatureLabel(GH_ParamAccess.item),
            "Label", "L", "An alignment label", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddPointParameter("Location", "Loc", "The location of the label.", GH_ParamAccess.item);
        pManager.AddTextParameter("Style Name", "Style", "The label style name.", GH_ParamAccess.item);
        pManager.AddTextParameter("Label Type", "Type", "The specific type of alignment label.", GH_ParamAccess.item);
        pManager.AddGeometryParameter("Geometric Entities", "Entities",
            "The geometric entities extracted from the label", GH_ParamAccess.list);
    }

    private List<IDbObject> RemoveBlocks(List<IDbObject> dbObjects,
        IAutocadTransactionManager transactionManager)
    {
        var listEntities = new List<IDbObject>();

        foreach (var dbObject in dbObjects)
        {
            if (dbObject.UnwrapObject() is BlockReference blockReference == false)
            {
                listEntities.Add(dbObject);
                continue;
            }

            var referenceWrapper = new AutocadBlockReferenceWrapper(blockReference);

            var explodedEntities = referenceWrapper.GetObjects(transactionManager);

            var castDown = explodedEntities.Cast<IDbObject>().ToList();

            listEntities.AddRange(this.RemoveBlocks(castDown, transactionManager));
        }

        return listEntities;
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilFeatureLabel? goo = null;
        if (!DA.GetData(0, ref goo) || goo?.Value == null) return;

        var label = goo.Value;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        // Extract geometry by exploding the label
        var geometryGoo = transactionManager.PerformTask(() =>
        {
            // Get the label entity for exploding using the Reference ObjectId
            // (goo.Value is a clone without a valid ObjectId)
            var labelEntity = transactionManager.Unwrap()
                .GetObject(goo.Reference.ObjectId.Unwrap(), OpenMode.ForRead) as Entity;

            if (labelEntity == null)
                return new List<IGH_GeometricGoo>();

            var objectCollection = new DBObjectCollection();
            labelEntity.Explode(objectCollection);

            var listEntities = new List<IDbObject>();
            foreach (DBObject dbObject in objectCollection)
            {
                listEntities.Add(new AutocadDbObjectWrapper(dbObject));
            }

            var blocklessEntities = this.RemoveBlocks(listEntities, transactionManager);

            var gooObjects = new List<IGH_GeometricGoo>();
            foreach (var dbObject in blocklessEntities)
            {
                if (dbObject is not IEntity entity) continue;
                var entityGoo = _gooConverterRegister.CreateGeometryGoo(entity);
                if (entityGoo != null) gooObjects.Add(entityGoo);
            }

            return gooObjects;
        });

        DA.SetData(0, label.LabelLocation.ToRhinoPoint3d());
        DA.SetData(1, label.StyleName);
        DA.SetData(2, label.LabelType);
        DA.SetDataList(3, geometryGoo);
    }
}
