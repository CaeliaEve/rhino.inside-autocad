using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D Parcel Properties.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilParcelPropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-0123-789012345678");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilParcelPropertiesComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilParcelPropertiesComponent"/> class.
    /// </summary>
    public CivilParcelPropertiesComponent()
        : base("Civil3d Parcel Properties", "CVL-ParcelProps",
            "Extracts individual values from Civil 3D Parcel Properties",
            "Civil3d", "Site/Parcels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilParcelProperties(GH_ParamAccess.item), "Properties",
            "Props", "Parcel properties from a Civil3d Parcel", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the parcel. When set this will update the name of the parcel.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the parcel. When set this will update the description of the parcel.", GH_ParamAccess.item);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the parcel.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the parcel.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Area", "A",
            "The area of the parcel in square units.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Perimeter", "Per",
            "The perimeter of the parcel.", GH_ParamAccess.item);

        pManager.AddTextParameter("Number", "Num",
            "The parcel number.", GH_ParamAccess.item);

        pManager.AddTextParameter("Tax ID", "Tax",
            "The tax ID of the parcel.", GH_ParamAccess.item);

        pManager.AddTextParameter("Address", "Addr",
            "The address of the parcel.", GH_ParamAccess.item);

        pManager.AddTextParameter("Site Name", "Site",
            "The name of the site containing this parcel.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Segment Count", "SegCnt",
            "The number of boundary segments in the parcel.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Is Closed", "Closed",
            "Whether the parcel boundary is closed.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this parcel as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilParcelProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        ICivilParcelProperties props = propsGoo.Value;

        var newName = props.Name;
        var newDescription = props.Description;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != props.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != props.Description) updateFlag = true;

        if (updateFlag)
        {
            var document = this.GetDocumentForObjectId(props.ParcelId);
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
        DA.SetData(2, props.Area);
        DA.SetData(3, props.Perimeter);
        DA.SetData(4, props.Number);
        DA.SetData(5, props.TaxId);
        DA.SetData(6, props.Address);
        DA.SetData(7, props.SiteName);
        DA.SetData(8, props.SegmentCount);
        DA.SetData(9, props.IsClosed);
        DA.SetData(10, new GH_NamedId(props.Style));
    }
}
