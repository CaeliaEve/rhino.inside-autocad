using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Corridor Feature Line.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilCorridorFeatureLineComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("B4C5D6E7-F8A9-0123-4567-890123456F01");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCorridorFeatureLineComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorFeatureLineComponent"/> class.
    /// </summary>
    public CivilCorridorFeatureLineComponent()
        : base("Civil3d Corridor Feature Line", "CVL-CorrFL",
            "Extracts information from a Civil 3D Corridor Feature Line",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorFeatureLine(GH_ParamAccess.item), "Feature Line",
            "FL", "A Corridor feature line", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Code", "C",
            "The point code associated with this feature line.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Curve", "Crv",
             "The feature line as a Rhino curve.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridorFeatureLine? featureLineGoo = null;

        if (!DA.GetData(0, ref featureLineGoo) || featureLineGoo?.Value is null) return;

        var featureLine = featureLineGoo.Value;

        DA.SetData(0, featureLine.Code);
        DA.SetData(1, featureLine.Curve);
    }
}
