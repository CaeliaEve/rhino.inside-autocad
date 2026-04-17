using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D ProfileView.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfileViewComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D4E5F6A7-B8C9-0123-DE45-F67890123456");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfileViewComponent"/> class.
    /// </summary>
    public CivilProfileViewComponent()
        : base("Civil3d ProfileView", "CVL-PV",
            "Extracts information from a Civil 3D ProfileView",
            "Civil3d", "ProfileViews")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfileView(), "ProfileView",
            "PV", "A Civil3d ProfileView", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the ProfileView.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the ProfileView.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileViewProperties(GH_ParamAccess.item), "Properties", "Props",
            "ProfileView properties (use ProfileView Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfile(), "Profiles", "P",
            "The profiles displayed in this ProfileView.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilAlignment(), "Alignment", "Align",
            "The parent alignment of this ProfileView.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileViewBand(GH_ParamAccess.list), "Bands", "B",
            "The bands (top and bottom) of this ProfileView.", GH_ParamAccess.list);

        pManager.AddPointParameter("Location", "Loc",
            "The insertion point of the ProfileView.", GH_ParamAccess.item);

        pManager.AddIntervalParameter("Station Range", "StaRng",
            "The station range (start to end) displayed.", GH_ParamAccess.item);

        pManager.AddIntervalParameter("Elevation Range", "ElevRng",
            "The elevation range (min to max) displayed.", GH_ParamAccess.item);

        pManager.AddRectangleParameter("Display Bounds", "Bounds",
            "The rectangular display bounds of the ProfileView.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileView? profileViewGoo = null;

        if (!DA.GetData(0, ref profileViewGoo) || profileViewGoo is null) return;

        var profileViewId = profileViewGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var profileView = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(profileViewId.Unwrap(), OpenMode.ForRead) as
            ProfileView);

        if (profileView == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read ProfileView");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(profileViewId));

        DA.SetData(1, new GH_AutocadObjectId(new AutocadObjectIdWrapper(profileView.StyleId)));

        // Get properties
        var properties = transactionManager.PerformTask(() =>
            profileView.GetProperties(transactionManager));

        DA.SetData(2, new GH_CivilProfileViewProperties(properties));

        // Get profiles displayed in this view
        var profileData = transactionManager.PerformTask(() =>
        {
            var profileIds = profileView.GetDisplayedProfileIds(transactionManager);
            var profiles = new List<GH_CivilProfile>();

            foreach (var profileId in profileIds)
            {
                if (profileId.IsNull || profileId.IsErased)
                    continue;

                var profile = transactionManager.Unwrap()
                    .GetObject(profileId, OpenMode.ForRead) as Profile;

                if (profile != null)
                {
                    profiles.Add(new GH_CivilProfile(profile));
                }
            }

            return profiles;
        });

        DA.SetDataList(3, profileData);

        // Get parent alignment
        var parentAlignment = transactionManager.PerformTask(() =>
        {
            var alignmentId = profileView.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return null;

            return transactionManager.Unwrap().GetObject(alignmentId, OpenMode.ForRead) as Alignment;
        });

        if (parentAlignment != null)
        {
            DA.SetData(4, new GH_CivilAlignment(parentAlignment));
        }

        // Get bands
        var bands = transactionManager.PerformTask(() =>
            profileView.GetBands(transactionManager)
                .Select(b => new GH_CivilProfileViewBand(b))
                .ToList());

        DA.SetDataList(5, bands);

        // Get location and ranges
        var location = profileView.GetRhinoLocation();
        DA.SetData(6, location);

        var stationRange = profileView.GetStationRange();
        DA.SetData(7, stationRange);

        var elevationRange = profileView.GetElevationRange();
        DA.SetData(8, elevationRange);

        var displayBounds = profileView.GetDisplayBounds();
        DA.SetData(9, displayBounds);
    }
}
