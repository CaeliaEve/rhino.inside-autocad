using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Grasshopper parameter for Civil 3D Alignment labels (generic - accepts any label type).
/// </summary>
public class Param_CivilFeatureLabel : GH_Param<GH_CivilFeatureLabel>
{
    public override Guid ComponentGuid => new("A1B2C3D4-E5F6-7890-ABCD-EF1234567806");
    public override GH_Exposure Exposure => GH_Exposure.primary;
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilFeatureLabel;

    public Param_CivilFeatureLabel(IGH_InstanceDescription tag) : base(tag) { }
    public Param_CivilFeatureLabel(IGH_InstanceDescription tag, GH_ParamAccess access) : base(tag, access) { }
    public Param_CivilFeatureLabel(GH_ParamAccess access)
        : base("Civil3d Feature Label", "CVL-FLbl", "A feature label from a Civil 3D", "Params", "Civil3d", access) { }
}
