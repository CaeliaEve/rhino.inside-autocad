using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that gets all Profiles from a Civil 3D Alignment.
/// </summary>
[ComponentVersion(introduced: "1.0.19")]
public class GetProfilesFromAlignmentComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A0B1C2D3-E4F5-6789-A890-123456789DEF");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetProfilesFromAlignmentComponent"/> class.
    /// </summary>
    public GetProfilesFromAlignmentComponent()
        : base("Get Profiles", "CVL-GetProfiles",
            "Gets all Profiles from a Civil 3D Alignment",
            "Civil3d", "Profiles")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignment(), "Alignment",
            "Align", "A Civil3d Alignment to get profiles from", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilProfile(), "Profiles", "P",
            "The Profiles associated with this Alignment.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignment? alignmentGoo = null;

        if (!DA.GetData(0, ref alignmentGoo) || alignmentGoo is null) return;

        var alignmentId = alignmentGoo.Reference.ObjectId;

        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var profiles = transactionManager.PerformTask(() =>
        {
            var alignment = transactionManager.Unwrap()
                .GetObject(alignmentId.Unwrap(), OpenMode.ForRead) as Alignment;

            if (alignment == null)
                return new List<GH_CivilProfile>();

            var profileIds = alignment.GetProfileIds();
            var result = new List<GH_CivilProfile>();

            foreach (ObjectId profileId in profileIds)
            {
                if (profileId.IsNull || profileId.IsErased)
                    continue;

                var profile = transactionManager.Unwrap()
                    .GetObject(profileId, OpenMode.ForRead) as Profile;

                if (profile != null)
                {
                    result.Add(new GH_CivilProfile(profile));
                }
            }

            return result;
        });

        if (profiles.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No profiles found for this alignment");
            return;
        }

        DA.SetDataList(0, profiles);
    }
}
