using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Generic deconstructor component for Civil 3D Alignment labels.
/// Works with any label type (Curve, Spiral, Tangent, PI, IndexedPI).
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentLabelComponent : RhinoInsideAutocad_ComponentBase
{
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
        pManager.AddTextParameter("Text", "Txt", "The text content of the label.", GH_ParamAccess.item);
        pManager.AddPointParameter("Location", "Loc", "The location of the label.", GH_ParamAccess.item);
        pManager.AddNumberParameter("Rotation", "Rot", "The rotation angle in radians.", GH_ParamAccess.item);
        pManager.AddTextParameter("Style Name", "Style", "The label style name.", GH_ParamAccess.item);
        pManager.AddTextParameter("Label Type", "Type", "The specific type of alignment label.", GH_ParamAccess.item);
    }

    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilFeatureLabel? goo = null;
        if (!DA.GetData(0, ref goo) || goo?.Value == null) return;

        var label = goo.Value;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var textEntities = transactionManager.PerformTask(() =>
        {
            var wrapper = label.CreateLabelWrapper(transactionManager);

            return wrapper.ExtractTextEntities(transactionManager);
        });

        var textGoo = textEntities.Select(GH_AutocadText.CreateFromTextEntity).ToList();

        DA.SetData(0, textGoo);
        DA.SetData(1, label.LabelLocation.ToRhinoPoint3d());
        DA.SetData(2, label.RotationAngle);
        DA.SetData(3, label.StyleName);
        DA.SetData(4, label.LabelType);
    }
}
