using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Alignment Label Group.
/// </summary>
/// <remarks>
/// This component works with all alignment label group types (Station, Cant, DesignSpeed,
/// GeometryPoint, StationEquation, Superelevation, VerticalGeometryPoint) and exposes
/// their common properties.
/// </remarks>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentLabelGroupComponent : RhinoInsideAutocad_ComponentBase
{
    private readonly GooTypeRegistry _gooConverterRegister = GooTypeRegistry.Instance!;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDEF1");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentLabelGroupComponent"/> class.
    /// </summary>
    public CivilAlignmentLabelGroupComponent()
        : base("Civil3d Alignment Label Group", "CVL-AlignLblGrp",
            "Extracts individual values from a Civil 3D Alignment Label Group",
            "Civil3d", "Labels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentLabelGroup(GH_ParamAccess.item), "Label Group",
            "LG", "An alignment label group from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Label Group Type", "Type",
            "The type of this label group (e.g., AlignmentStationLabelGroup).", GH_ParamAccess.item);

        pManager.AddTextParameter("Style Name", "Style",
            "The name of the label style applied to this group.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Label Count", "Count",
            "The number of sub-entity labels in this group.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Range Start", "RStart",
            "The start station of the label range.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Range End", "REnd",
            "The end station of the label range.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Range Start From Feature", "RSFeat",
            "Whether the start of the range is derived from the alignment feature.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Range End From Feature", "REFeat",
            "Whether the end of the range is derived from the alignment feature.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Is Visible", "Vis",
            "The visibility state of the label group.", GH_ParamAccess.item);

        pManager.AddGeometryParameter("Geometric Entities", "Entities",
            "The geometric entities in the label group", GH_ParamAccess.list);
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

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentLabelGroup? labelGroupGoo = null;

        if (!DA.GetData(0, ref labelGroupGoo) || labelGroupGoo?.Value is null) return;

        var labelGroup = labelGroupGoo.Value;

        DA.SetData(0, labelGroup.LabelGroupType);
        DA.SetData(1, labelGroup.StyleName);
        DA.SetData(2, labelGroup.LabelCount);
        DA.SetData(3, labelGroup.RangeStart);
        DA.SetData(4, labelGroup.RangeEnd);
        DA.SetData(5, labelGroup.RangeStartFromFeature);
        DA.SetData(6, labelGroup.RangeEndFromFeature);
        DA.SetData(7, labelGroup.IsVisible);

        var cadLabelGroup = labelGroup.Unwrap();

        if (cadLabelGroup == null) return;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var dbObjects = transactionManager.PerformTask(() =>
        {
            var labelGroup = (LabelGroup)transactionManager.Unwrap().GetObject(cadLabelGroup.Id, OpenMode.ForRead);

            var objectCollection = new DBObjectCollection();

            labelGroup.Explode(objectCollection);

            var listEntities = new List<IDbObject>();
            foreach (DBObject dbObject in objectCollection)
            {
                listEntities.Add(new AutocadDbObjectWrapper(dbObject));
            }

            var blocklessEntites = this.RemoveBlocks(listEntities, transactionManager);

            return blocklessEntites;
        }, true);

        var gooObjects = new List<IGH_GeometricGoo>();

        foreach (var dbObject in dbObjects)
        {
            if (dbObject is not IEntity entity) continue;

            var goo = _gooConverterRegister.CreateGeometryGoo(entity);

            gooObjects.Add(goo);
        }

        DA.SetDataList(8, gooObjects);
    }
}
