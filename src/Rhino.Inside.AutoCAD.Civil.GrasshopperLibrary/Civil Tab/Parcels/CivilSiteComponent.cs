using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Site.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class CivilSiteComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("C9D0E1F2-A3B4-5678-0123-901234567890");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilSiteComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSiteComponent"/> class.
    /// </summary>
    public CivilSiteComponent()
        : base("Civil3d Site", "CVL-Site",
            "Extracts information from a Civil 3D Site",
            "Civil3d", "Site/Parcels")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSite(GH_ParamAccess.item), "Site",
            "Site", "A Civil3d Site", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the site. When set this will update the name of the site.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the site. When set this will update the description of the site.", GH_ParamAccess.item);
        pManager[2].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Site.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the site.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the site.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Parcel Count", "PCnt",
            "The number of parcels in the site.", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Alignment Count", "ACnt",
            "The number of alignments in the site.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilParcel(), "Parcels", "P",
            "The parcels in the site.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilAlignment(), "Alignments", "A",
            "The alignments in the site.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilSite? siteGoo = null;

        if (!DA.GetData(0, ref siteGoo) || siteGoo?.Value is null) return;

        ICivilSite site = siteGoo.Value;

        var newName = site.Name;
        var newDescription = site.Description;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != site.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != site.Description) updateFlag = true;

        // Get parcels from the site
        var document = this.GetDocumentForObjectId(site.Id);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        if (updateFlag)
        {
            site = transactionManager.PerformTask(() =>
                site.Update(transactionManager, newName, newDescription));
        }

        DA.SetData(0, new GH_AutocadObjectId(site.Id));
        DA.SetData(1, site.Name);
        DA.SetData(2, site.Description);
        DA.SetData(3, site.ParcelCount);
        DA.SetData(4, site.AlignmentCount);

        var parcels = transactionManager.PerformTask(() =>
        {
            var parcelList = new List<GH_CivilParcel>();
            foreach (var parcelId in site.ParcelIds)
            {
                try
                {
                    var parcel = transactionManager.Unwrap()
                        .GetObject(parcelId.Unwrap(), OpenMode.ForRead) as Parcel;
                    if (parcel != null)
                    {
                        parcelList.Add(new GH_CivilParcel(parcel));
                    }
                }
                catch
                {
                    // Skip parcels that can't be read
                }
            }
            return parcelList;
        });

        var alignments = transactionManager.PerformTask(() =>
        {
            var alignmentList = new List<GH_CivilAlignment>();
            foreach (var alignmentId in site.AlignmentIds)
            {
                try
                {
                    var alignment = transactionManager.Unwrap()
                        .GetObject(alignmentId.Unwrap(), OpenMode.ForRead) as Alignment;
                    if (alignment != null)
                    {
                        alignmentList.Add(new GH_CivilAlignment(alignment));
                    }
                }
                catch
                {
                    // Skip alignments that can't be read
                }
            }
            return alignmentList;
        });

        DA.SetDataList(5, parcels);
        DA.SetDataList(6, alignments);
    }
}
