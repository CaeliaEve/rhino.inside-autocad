using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
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
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilProfilePropertiesComponent;

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

        pManager.AddTextParameter("Name", "N",
            "The name of the profile. When set this will update the name of the profile.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the profile. When set this will update the description of the profile.", GH_ParamAccess.item);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the profile.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the profile.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilStationPoint(GH_ParamAccess.item), "Start Point", "StaPt",
            "The starting station and elevation of the profile.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilStationPoint(GH_ParamAccess.item), "End Point", "EndPt",
            "The ending station and elevation of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Min Elevation", "MinElev",
            "The minimum elevation of the profile.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Max Elevation", "MaxElev",
            "The maximum elevation of the profile.", GH_ParamAccess.item);

        pManager.AddTextParameter("Profile Type", "Type",
            "The type of profile (ExistingGround, Layout, etc.).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this profile as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        ICivilProfileProperties props = propsGoo.Value;

        var newName = props.Name;
        var newDescription = props.Description;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != props.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != props.Description) updateFlag = true;

        if (updateFlag)
        {
            var document = this.GetDocumentForObjectId(props.ProfileId);
            if (document is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
                return;
            }

            var transactionManager = document.CreateTransactionManager();
            props = transactionManager.PerformTask(() =>
                props.Update(transactionManager, newName, newDescription));
        }

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, new GH_CivilStationPoint(props.Start));
        DA.SetData(3, new GH_CivilStationPoint(props.End));
        DA.SetData(4, props.MinElevation);
        DA.SetData(5, props.MaxElevation);
        DA.SetData(6, props.ProfileType.ToString());
        DA.SetData(7, new GH_NamedId(props.Style));
    }
}
