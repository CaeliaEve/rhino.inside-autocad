using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Alignment.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E5F6A7B8-C9D0-1234-EF01-456789012CDE");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilAlignmentComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentComponent"/> class.
    /// </summary>
    public CivilAlignmentComponent()
        : base("Civil3d Alignment", "CVL-Align",
            "Extracts information from a Civil 3D Alignment",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignment(), "Alignment",
            "Align", "A Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Alignment.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilAlignmentProperties(GH_ParamAccess.item), "Properties", "Props",
            "Alignment properties (use Alignment Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilAlignmentEntity(GH_ParamAccess.list), "Entities", "E",
            "The individual entities (Lines, Arcs, Spirals) of the Alignment.", GH_ParamAccess.list);

        pManager.AddCurveParameter("Curve", "C",
            "The alignment centerline as a Rhino curve.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilAlignmentLabelGroup(GH_ParamAccess.list), "Label Groups", "LG",
            "Auto-generated label groups from the Alignment.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilFeatureLabel(GH_ParamAccess.list), "Labels", "L",
            "Individual labels from the Alignment.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignment? alignmentGoo = null;

        if (!DA.GetData(0, ref alignmentGoo) || alignmentGoo is null) return;

        var alignmentId = alignmentGoo.Reference.ObjectId;

        var document = this.GetDocumentForObjectId(alignmentId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId.Unwrap(), OpenMode.ForRead) as Alignment;

            if (alignment == null)
            {
                return AlignmentGooResult.Failed;
            }

            var wrapper = new CivilAlignmentWrapper(alignment);

            var propertiesGoo = new GH_CivilAlignmentProperties(wrapper.Properties);

            var entitiesGoo = wrapper.Entities
                .Select(entity => new GH_CivilAlignmentEntity(entity))
                .ToList();

            var curve = wrapper.CenterlineCurve;

            var labelGroupsGoo = wrapper.GetLabelGroups(transactionManager)
                .Select(group => new GH_CivilAlignmentLabelGroup(group))
                .ToList();

            var labelsGoo = wrapper.GetLabels(transactionManager)
                .Select(label => new GH_CivilFeatureLabel(label.Unwrap()))
                .ToList();

            return new AlignmentGooResult(propertiesGoo, entitiesGoo, curve, labelGroupsGoo, labelsGoo);
        });

        if (result.IsSuccess == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Alignment");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(alignmentId));
        DA.SetData(1, result.PropertiesGoo);
        DA.SetDataList(2, result.EntitiesGoo);
        DA.SetData(3, result.Curve);
        DA.SetDataList(4, result.LabelGroupsGoo);
        DA.SetDataList(5, result.LabelsGoo);
    }
}
