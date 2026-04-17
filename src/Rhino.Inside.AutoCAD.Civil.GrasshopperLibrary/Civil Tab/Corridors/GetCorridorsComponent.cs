using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilDocument = Autodesk.Civil.ApplicationServices.CivilDocument;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that gets all Corridors from the current Civil 3D document.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class GetCorridorsComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("C5D6E7F8-A9B0-1234-5678-901234567012");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetCorridorsComponent"/> class.
    /// </summary>
    public GetCorridorsComponent()
        : base("Get Corridors", "CVL-GetCorrs",
            "Gets all Corridors from the current Civil 3D document",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        // No input parameters - gets corridors from active document
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridor(), "Corridors", "Corrs",
            "All Corridors in the current document.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var corridors = transactionManager.PerformTask(() =>
        {
            var result = new List<GH_CivilCorridor>();

            try
            {
                var civilDoc = CivilDocument.GetCivilDocument(document.Unwrap().Database);

                var corridorIds = civilDoc.CorridorCollection;

                foreach (var corridorId in corridorIds)
                {
                    if (corridorId.IsNull || corridorId.IsErased)
                        continue;

                    var corridor = transactionManager.Unwrap()
                        .GetObject(corridorId, OpenMode.ForRead) as Corridor;

                    if (corridor != null)
                    {
                        result.Add(new GH_CivilCorridor(corridor));
                    }
                }
            }
            catch
            {
                // Return empty list if extraction fails
            }

            return result;
        });

        if (corridors.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No corridors found in the document");
            return;
        }

        DA.SetDataList(0, corridors);
    }
}
