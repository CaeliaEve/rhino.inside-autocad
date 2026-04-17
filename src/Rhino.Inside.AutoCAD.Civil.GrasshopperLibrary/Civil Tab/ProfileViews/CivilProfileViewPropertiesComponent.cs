using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D ProfileView Properties.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfileViewPropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E5F6A7B8-C9D0-1234-EF56-789012345678");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfileViewPropertiesComponent"/> class.
    /// </summary>
    public CivilProfileViewPropertiesComponent()
        : base("Civil3d ProfileView Properties", "CVL-PVProps",
            "Extracts individual values from Civil 3D ProfileView Properties",
            "Civil3d", "ProfileViews")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfileViewProperties(GH_ParamAccess.item), "Properties",
            "Props", "ProfileView properties from a Civil3d ProfileView", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the ProfileView.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Station Start", "StaSt",
            "The starting station of the ProfileView display range.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Station End", "StaEnd",
            "The ending station of the ProfileView display range.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Elevation Min", "ElevMin",
            "The minimum elevation of the ProfileView display range.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Elevation Max", "ElevMax",
            "The maximum elevation of the ProfileView display range.", GH_ParamAccess.item);

        pManager.AddTextParameter("Alignment Name", "AlignName",
            "The name of the parent alignment.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Profile Count", "ProfCnt",
            "The number of profiles displayed in this ProfileView.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Band Count", "BandCnt",
            "The number of bands (top and bottom) in this ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Horizontal Scale", "HScale",
            "The horizontal scale of the ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Vertical Scale", "VScale",
            "The vertical scale of the ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Vertical Exaggeration", "VExag",
            "The vertical exaggeration factor of the ProfileView.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileViewProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        var props = propsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, props.StationStart);
        DA.SetData(3, props.StationEnd);
        DA.SetData(4, props.ElevationMin);
        DA.SetData(5, props.ElevationMax);
        DA.SetData(6, props.AlignmentName);
        DA.SetData(7, props.ProfileCount);
        DA.SetData(8, props.BandCount);
        DA.SetData(9, props.HorizontalScale);
        DA.SetData(10, props.VerticalScale);
        DA.SetData(11, props.VerticalExaggeration);
    }
}
