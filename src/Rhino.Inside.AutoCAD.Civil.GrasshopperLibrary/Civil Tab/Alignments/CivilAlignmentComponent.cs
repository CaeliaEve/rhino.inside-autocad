using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
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
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

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

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the Alignment.", GH_ParamAccess.item);

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

    /// <summary>
    /// Extracts all label groups from a Civil 3D Alignment.
    /// </summary>
    public List<ICivilAlignmentLabelGroup> GetAlignmentLabelGroups(
        Alignment alignment,
        IAutocadTransactionManager transactionManager)
    {
        var labelGroups = new List<ICivilAlignmentLabelGroup>();

        try
        {
            var labelGroupIds = alignment.GetAlignmentLabelGroupIds();

            foreach (ObjectId labelGroupId in labelGroupIds)
            {
                if (labelGroupId.IsNull || labelGroupId.IsErased)
                    continue;

                var labelGroup = transactionManager.Unwrap()
                    .GetObject(labelGroupId, OpenMode.ForRead) as AlignmentLabelGroup;

                if (labelGroup == null)
                    continue;

                var wrapper = new CivilAlignmentLabelGroupWrapper(labelGroup);
                labelGroups.Add(wrapper);
            }
        }
        catch
        {
            // Return empty list if label group extraction fails
        }

        return labelGroups;
    }

    /// <summary>
    /// Extracts all individual labels from a Civil 3D Alignment as a flat list.
    /// </summary>
    public List<ICivilFeatureLabel> GetAlignmentLabels(
        Alignment alignment,
        IAutocadTransactionManager transactionManager)
    {
        var labels = new List<ICivilFeatureLabel>();

        try
        {
            var labelIds = alignment.GetAlignmentLabelIds();

            foreach (ObjectId labelId in labelIds)
            {
                if (labelId.IsNull || labelId.IsErased)
                    continue;

                var featureLabel = transactionManager.Unwrap()
                    .GetObject(labelId, OpenMode.ForRead) as FeatureLabel;

                if (featureLabel == null)
                    continue;

                var wrapper = featureLabel.CreateLabelWrapper(transactionManager);
                if (wrapper != null)
                {
                    labels.Add(wrapper);
                }
            }
        }
        catch
        {
            // Return empty list if label extraction fails
        }

        return labels;
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignment? alignmentGoo = null;

        if (!DA.GetData(0, ref alignmentGoo) || alignmentGoo is null) return;

        var alignmentId = alignmentGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var alignment = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(alignmentId.Unwrap(), OpenMode.ForRead) as
            Alignment);

        if (alignment == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Alignment");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(alignmentId));

        DA.SetData(1, new GH_AutocadObjectId(new AutocadObjectIdWrapper(alignment.StyleId)));

        DA.SetData(2, new GH_CivilAlignmentProperties(new CivilAlignmentPropertiesWrapper(alignment)));

        var alignmentData = transactionManager.PerformTask(() => new
        {
            Entities = alignment.GetAlignmentEntities(transactionManager),
            Curve = alignment.ToRhinoCurve(transactionManager),
            LabelGroups = this.GetAlignmentLabelGroups(alignment, transactionManager),
            Labels = this.GetAlignmentLabels(alignment, transactionManager)
        });

        DA.SetDataList(3, alignmentData.Entities.Select(e => new GH_CivilAlignmentEntity(e)).ToList());
        DA.SetData(4, alignmentData.Curve);
        DA.SetDataList(5, alignmentData.LabelGroups.Select(lg => new GH_CivilAlignmentLabelGroup(lg)).ToList());
        DA.SetDataList(6, alignmentData.Labels.Select(l => new GH_CivilFeatureLabel()).ToList());
    }
}
