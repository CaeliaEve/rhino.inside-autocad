using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
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
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilProfileViewComponent;

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

        pManager.AddParameter(new Param_CivilProfileViewProperties(GH_ParamAccess.item), "Properties", "Props",
            "ProfileView properties (use ProfileView Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfile(), "Profiles", "P",
            "The profiles displayed in this ProfileView.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilAlignment(), "Alignment", "Align",
            "The parent alignment of this ProfileView.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileViewBand(GH_ParamAccess.list), "Bands", "B",
            "The bands (top and bottom) of this ProfileView.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_CivilFeatureLabel(GH_ParamAccess.list), "Label Groups", "LG",
            "The label groups in this ProfileView.", GH_ParamAccess.list);

        pManager.AddGeometryParameter("Geometry", "Geo",
            "The geometry on the view converted to Rhino", GH_ParamAccess.tree);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfileView? profileViewGoo = null;

        if (!DA.GetData(0, ref profileViewGoo) || profileViewGoo is null) return;

        var profileViewId = profileViewGoo.Reference.ObjectId;

        var document = this.GetDocumentForObjectId(profileViewId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var profileView = transactionManager.PerformTask(() =>
                transactionManager.Unwrap().GetObject(profileViewId.Unwrap(), OpenMode.ForRead) as
                    ProfileView);

            if (profileView == null)
                return ProfileViewGooResult.Failed;

            var profileWrapper = new CivilProfileViewWrapper(profileView);

            var properties = profileWrapper.Properties;

            var propertiesGoo = new GH_CivilProfileViewProperties(properties);

            var profileData = profileWrapper.GetDisplayedProfiles(transactionManager);

            var profileDataGoo =
                profileData.Select(profile => new GH_CivilProfile(profile.Unwrap())).ToList();

            var alignmentGoo = profileWrapper.TryGetAlignment(transactionManager, out var alignment)
                ? new GH_CivilAlignment(alignment.Unwrap())
                : null;

            var bands = profileWrapper.GetBands(transactionManager);

            var bandsGoo = bands.Select(civilProfileViewBand =>
                new GH_CivilProfileViewBand(civilProfileViewBand)).ToList();

            var labelGroups =
                profileWrapper.GetProfileViewLabelGroups(transactionManager);

            var labelGroupsGoo = labelGroups.Select(civilProfileViewLabelGroup =>
                new GH_CivilFeatureLabel(civilProfileViewLabelGroup)).ToList();

            var geometry = profileView.ToRhinoGeometry();

            var structure = new GH_Structure<IGH_GeometricGoo>();
            structure.AppendRange(
                geometry.GraphCurves.Select(c => (IGH_GeometricGoo)new GH_Curve(c)),
                new GH_Path(0));

            structure.AppendRange(
                geometry.TextEntities.Select(GH_Convert.ToGeometricGoo),
                new GH_Path(1));

            structure.AppendRange(
                geometry.ProfileCurves.Select(c => (IGH_GeometricGoo)new GH_Curve(c)),
                new GH_Path(2));

            return new ProfileViewGooResult(propertiesGoo, profileDataGoo, alignmentGoo, bandsGoo,
                labelGroupsGoo, structure);
        });

        if (result.IsSuccess == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read ProfileView");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(profileViewId));

        DA.SetData(1, result.PropertiesGoo);

        DA.SetDataList(2, result.ProfileDataGoo);

        DA.SetData(3, result.AlignmentGoo);

        DA.SetDataList(4, result.BandsGoo);

        DA.SetDataList(5, result.LabelGroupsGoo);

        DA.SetDataTree(6, result.Geometry);

    }
}