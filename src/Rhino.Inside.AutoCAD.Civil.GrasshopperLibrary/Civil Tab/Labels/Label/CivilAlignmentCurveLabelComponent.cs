using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Deconstructor component for Civil 3D Alignment Curve labels.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentCurveLabelComponent : RhinoInsideAutocad_ComponentBase
{
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-67890ABCDE10");
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    public override GH_Exposure Exposure => GH_Exposure.secondary;

    public CivilAlignmentCurveLabelComponent()
        : base("Civil3d Curve Label", "CVL-CurveLbl",
            "Extracts values from an Alignment Curve Label. Non-matching label types are skipped.",
            "Civil3d", "Labels")
    { }

    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilFeatureLabel(GH_ParamAccess.item),
            "Label", "L", "An alignment label", GH_ParamAccess.item);
    }

    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadText(), "Text", "Text",
            "The Text Entities of the label.", GH_ParamAccess.list);
        pManager.AddPointParameter("Location", "Loc", "The location of the label.", GH_ParamAccess.item);
        pManager.AddNumberParameter("Rotation", "Rot", "The rotation angle in radians.", GH_ParamAccess.item);
        pManager.AddTextParameter("Style Name", "Style", "The label style name.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilFeatureLabel? goo = null;
        if (!DA.GetData(0, ref goo) || goo?.Value == null) return;

        if (goo.Value is not AlignmentCurveLabel labelRaw)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning,
                $"Skipping non-tangent label: {goo.Value.LabelType}");
            return;
        }

        var label = new CivilAlignmentCurveLabelWrapper(labelRaw);

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var labelData = transactionManager.PerformTask(() => new
        {
            TextEntites = label.ExtractTextEntities(transactionManager),
            StyleName = label.GetStyleName(transactionManager)
        });


        var textGoo = labelData.TextEntites.Select(GH_AutocadText.CreateFromTextEntity).ToList();

        DA.SetDataList(0, textGoo);
        DA.SetData(1, label.Location);
        DA.SetData(2, label.Rotation);
        DA.SetData(3, labelData.StyleName);
    }
}
