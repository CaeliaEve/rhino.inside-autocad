using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Profile.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class CivilProfileComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D7E8F9A0-B1C2-3456-D567-890123456ABC");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilProfileComponent"/> class.
    /// </summary>
    public CivilProfileComponent()
        : base("Civil3d Profile", "CVL-Profile",
            "Extracts information from a Civil 3D Profile",
            "Civil3d", "Profiles")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfile(), "Profile",
            "P", "A Civil3d Profile", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Profile.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "StyleId", "StyleId",
            "The Id of the Style of the Profile.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileProperties(GH_ParamAccess.item), "Properties", "Props",
            "Profile properties (use Profile Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileEntity(GH_ParamAccess.list), "Entities", "E",
            "The individual entities (Tangents, CircularArcs, Parabolas) of the Profile.", GH_ParamAccess.list);

        pManager.AddCurveParameter("Curve", "C",
            "The profile as a 2D Rhino curve (X=Station, Y=Elevation).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilAlignment(), "Alignment", "Align",
            "The parent alignment of this profile.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilProfileLabelGroup(GH_ParamAccess.list), "Label Groups", "LG",
            "Label groups from the Profile.", GH_ParamAccess.list);
    }

    /// <summary>
    /// Extracts all label groups from a Civil 3D Profile.
    /// </summary>
    public List<CivilProfileLabelGroupWrapper> GetProfileLabelGroups(
        Profile profile,
        ObjectId profileId,
        IAutocadTransactionManager transactionManager)
    {
        var labelGroups = new List<CivilProfileLabelGroupWrapper>();

        try
        {
            // Get profile views that contain this profile through the parent alignment
            var alignmentId = profile.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return labelGroups;

            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId, OpenMode.ForRead) as Alignment;

            if (alignment == null)
                return labelGroups;

            // Get all profile view IDs for this alignment
            var profileViewIds = alignment.GetProfileViewIds();

            foreach (ObjectId profileViewId in profileViewIds)
            {
                if (profileViewId.IsNull || profileViewId.IsErased)
                    continue;

                // Get label group IDs for this profile in this profile view
                var labelGroupClass = RXObject.GetClass(typeof(ProfileLabelGroup));
                var labelGroupIds = ProfileLabelGroup.GetAvailableLabelGroupIds(
                    labelGroupClass, profileViewId, profileId, true);

                foreach (ObjectId labelGroupId in labelGroupIds)
                {
                    if (labelGroupId.IsNull || labelGroupId.IsErased)
                        continue;

                    var labelGroup = transactionManager.Unwrap()
                        .GetObject(labelGroupId, OpenMode.ForRead) as ProfileLabelGroup;

                    if (labelGroup == null)
                        continue;

                    labelGroups.Add(new CivilProfileLabelGroupWrapper(labelGroup));
                }
            }
        }
        catch
        {
            // Return empty list if label extraction fails
        }

        return labelGroups;
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfile? profileGoo = null;

        if (!DA.GetData(0, ref profileGoo) || profileGoo is null) return;

        var profileId = profileGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var profile = transactionManager.PerformTask(() =>
            transactionManager.Unwrap().GetObject(profileId.Unwrap(), OpenMode.ForRead) as
            Profile);

        if (profile == null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Profile");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(profileId));

        DA.SetData(1, new GH_AutocadObjectId(new AutocadObjectIdWrapper(profile.StyleId)));

        // Get parent alignment name
        var parentAlignmentName = transactionManager.PerformTask(() =>
            profile.GetParentAlignmentName(transactionManager));

        DA.SetData(2, new GH_CivilProfileProperties(CivilProfileProperties.CreateFromProfile(profile, parentAlignmentName)));

        var profileData = transactionManager.PerformTask(() => new
        {
            Entities = profile.GetProfileEntities(transactionManager),
            Curve = profile.ToRhinoCurve(transactionManager),
            LabelGroups = this.GetProfileLabelGroups(profile, profileId.Unwrap(), transactionManager)
        });

        DA.SetDataList(3, profileData.Entities.Select(entity => new GH_CivilProfileEntity(entity)).ToList());
        DA.SetData(4, profileData.Curve);

        // Get parent alignment
        var parentAlignment = transactionManager.PerformTask(() =>
        {
            var alignmentId = profile.AlignmentId;
            if (alignmentId.IsNull || alignmentId.IsErased)
                return null;

            return transactionManager.Unwrap().GetObject(alignmentId, OpenMode.ForRead) as Alignment;
        });

        if (parentAlignment != null)
        {
            DA.SetData(5, new GH_CivilAlignment(parentAlignment));
        }

        DA.SetDataList(6, profileData.LabelGroups.Select(lg => new GH_CivilProfileLabelGroup(lg)).ToList());
    }
}
