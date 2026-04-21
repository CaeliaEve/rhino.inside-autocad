using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;
using CivilDocument = Autodesk.Civil.ApplicationServices.CivilDocument;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that gets all Assemblies from the current Civil 3D document.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class GetAssembliesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D4E5F6A7-B8C9-0123-DEF0-456789012345");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAssembliesComponent"/> class.
    /// </summary>
    public GetAssembliesComponent()
        : base("Get Assemblies", "CVL-GetAsms",
            "Gets all Assemblies from the current Civil 3D document",
            "Civil3d", "Assemblies")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        // No input parameters - gets assemblies from active document
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAssembly(), "Assemblies", "Asms",
            "All Assemblies in the current document.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        var document = RhinoInsideAutoCadExtension.Application.RhinoInsideManager
            .AutoCadInstance.ActiveDocument;

        var transactionManager = document.CreateTransactionManager();

        var assemblies = transactionManager.PerformTask(() =>
        {
            var result = new List<GH_CivilAssembly>();

            try
            {
                var civilDoc = CivilDocument.GetCivilDocument(document.Unwrap().Database);

                var assemblyIds = civilDoc.AssemblyCollection;

                foreach (var assemblyId in assemblyIds)
                {
                    if (assemblyId.IsNull || assemblyId.IsErased)
                        continue;

                    var assembly = transactionManager.Unwrap()
                        .GetObject(assemblyId, OpenMode.ForRead) as Assembly;

                    if (assembly != null)
                    {
                        result.Add(new GH_CivilAssembly(assembly));
                    }
                }
            }
            catch
            {
                // Return empty list if extraction fails
            }

            return result;
        });

        if (assemblies.Count == 0)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No assemblies found in the document");
            return;
        }

        DA.SetDataList(0, assemblies);
    }
}
