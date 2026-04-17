using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D Profile Properties.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfilePropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E8F9A0B1-C2D3-4567-E678-901234567BCD");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfilePropertiesComponent"/> class.
    /// </summary>
    public CivilProfilePropertiesComponent()
        : base("Civil3d Profile Properties", "CVL-ProfileProps",
            "Extracts individual values from Civil 3D Profile Properties",
            "Civil3d", "Profiles")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfileProperties(GH_ParamAccess.item), "Properties",
            "Props", "Profile properties from a Civil3d Profile", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the profile.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Min Elevation", "MinElev",
            "The minimum elevation of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Max Elevation", "MaxElev",
            "The maximum elevation of the profile.", GH_ParamAccess.item);

        pManager.AddTextParameter("Profile Type", "Type",
            "The type of profile (ExistingGround, Layout, etc.).", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Entity Count", "Count",
            "The number of entities in the profile.", GH_ParamAccess.item);

        pManager.AddTextParameter("Parent Alignment", "Align",
            "The name of the parent alignment.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        var props = propsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, props.StartStation);
        DA.SetData(3, props.EndStation);
        DA.SetData(4, props.MinElevation);
        DA.SetData(5, props.MaxElevation);
        DA.SetData(6, props.ProfileTypeName);
        DA.SetData(7, props.EntityCount);
        DA.SetData(8, props.ParentAlignmentName);
    }
}
