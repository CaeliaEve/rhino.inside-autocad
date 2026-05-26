using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
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
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilProfileComponent;

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

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilProfile? profileGoo = null;

        if (!DA.GetData(0, ref profileGoo) || profileGoo is null) return;

        var profileId = profileGoo.Reference.ObjectId;

        var document = this.GetDocumentForObjectId(profileId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var profile = transactionManager.Unwrap()
                .GetObject(profileId.Unwrap(), OpenMode.ForRead) as Profile;

            if (profile == null)
            {
                return ProfileGooResult.Failed;
            }

            var wrapper = new CivilProfileWrapper(profile);

            var properties = wrapper.Properties;

            var propertiesGoo = new GH_CivilProfileProperties(properties);

            var entities = wrapper.GetProfileEntities(transactionManager);

            var entitiesGoo = entities.Select(entity => new GH_CivilProfileEntity(entity)).ToList();

            var curve = profile.ToRhinoCurve(transactionManager);

            var alignmentGoo =
                wrapper.TryGetParentAlignmentName(transactionManager, out var alignment)
                    ? new GH_CivilAlignment(alignment.Unwrap())
                    : null;

            var labelGroups = wrapper.GetProfileLabelGroups(transactionManager);

            var labelGroupsGoo = labelGroups.Select(lg => new GH_CivilProfileLabelGroup(lg)).ToList();

            return new ProfileGooResult(propertiesGoo, entitiesGoo, curve, alignmentGoo, labelGroupsGoo);
        });

        if (result.IsSuccess == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Profile");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(profileId));
        DA.SetData(1, result.PropertiesGoo);
        DA.SetDataList(2, result.EntitiesGoo);
        DA.SetData(3, result.Curve);
        DA.SetData(4, result.AlignmentGoo);
        DA.SetDataList(5, result.LabelGroupsGoo);
    }
}
