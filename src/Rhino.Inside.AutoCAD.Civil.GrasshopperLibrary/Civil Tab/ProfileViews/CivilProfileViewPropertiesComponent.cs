using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
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
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilProfileViewPropertiesComponent;

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

        pManager.AddTextParameter("Name", "N",
            "The name of the profile view. When set this will update the name of the profile view.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the profile view. When set this will update the description of the profile view.", GH_ParamAccess.item);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the ProfileView.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the ProfileView.", GH_ParamAccess.item);

        pManager.AddPlaneParameter("Plane", "Loc",
            "The insertion Plane of the ProfileView.", GH_ParamAccess.item);

        pManager.AddIntervalParameter("Station Range", "StaRng",
            "The station range (start to end) displayed.", GH_ParamAccess.item);

        pManager.AddIntervalParameter("Elevation Range", "ElevRng",
            "The elevation range (min to max) displayed.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Horizontal Scale", "HScale",
            "The horizontal scale of the ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Vertical Scale", "VScale",
            "The vertical scale of the ProfileView.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Vertical Exaggeration", "VExag",
            "The vertical exaggeration factor of the ProfileView.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this profile view as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileViewProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        ICivilProfileViewProperties properties = propsGoo.Value;

        var newName = properties.Name;
        var newDescription = properties.Description;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != properties.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != properties.Description) updateFlag = true;

        var document = this.GetDocumentForObjectId(properties.ProfileViewId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        if (updateFlag)
        {
            properties = transactionManager.PerformTask(() =>
                properties.Update(transactionManager, newName, newDescription));
        }

        var coordinateSystem = transactionManager.PerformTask(() => properties.GetCoordinateSystem(transactionManager));

        DA.SetData(0, properties.Name);
        DA.SetData(1, properties.Description);
        DA.SetData(2, coordinateSystem.Plane);
        DA.SetData(3, properties.StationRange);
        DA.SetData(4, properties.ElevationRange);
        DA.SetData(5, coordinateSystem.HorizontalScale);
        DA.SetData(6, coordinateSystem.VerticalScale);
        DA.SetData(7, coordinateSystem.VerticalExaggeration);
        DA.SetData(8, new GH_NamedId(properties.Style));
    }
}
