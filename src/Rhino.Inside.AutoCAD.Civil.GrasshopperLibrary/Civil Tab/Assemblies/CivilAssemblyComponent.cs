using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Assembly.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAssemblyComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("E5F6A7B8-C9D0-1234-5678-901234567ABC");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilAssemblyComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAssemblyComponent"/> class.
    /// </summary>
    public CivilAssemblyComponent()
        : base("Civil3d Assembly", "CVL-Asm",
            "Extracts information from a Civil 3D Assembly",
            "Civil3d", "Assemblies")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAssembly(), "Assembly",
            "Asm", "A Civil3d Assembly", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Assembly.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilAssemblyProperties(GH_ParamAccess.item), "Properties", "Props",
            "Assembly properties (use Assembly Properties component to extract values).", GH_ParamAccess.item);

        pManager.AddParameter(new Param_CivilSubassemblyProperties(GH_ParamAccess.list), "Subassemblies", "Subs",
            "The subassemblies in the Assembly.", GH_ParamAccess.list);

        pManager.AddPointParameter("Location", "Loc",
            "The origin location of the Assembly.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Geometry", "G",
            "The combined geometry from all subassemblies as curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAssembly? assemblyGoo = null;

        if (!DA.GetData(0, ref assemblyGoo) || assemblyGoo is null) return;

        var assemblyId = assemblyGoo.Reference.ObjectId;

        var document = this.GetDocumentForObjectId(assemblyId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        var result = transactionManager.PerformTask(() =>
        {
            var assembly = transactionManager.Unwrap()
                .GetObject(assemblyId.Unwrap(), OpenMode.ForRead) as Assembly;

            if (assembly == null)
            {
                return AssemblyGooResult.Failed;
            }

            var wrapper = new CivilAssembliesWrapper(assembly);

            var assemblyProperties = wrapper.Properties;

            var propertiesGoo = new GH_CivilAssemblyProperties(assemblyProperties);

            var subassemblies = wrapper.GetSubassemblies(transactionManager);

            var subAssembliesGoo = subassemblies
                .Select(s => new GH_CivilSubassemblyProperties(s)).ToList();

            var location = assembly.Location.ToRhinoPoint3d();

            var allGeometry = subassemblies.SelectMany(s => s.Geometry)
                .ToList();

            return new AssemblyGooResult(propertiesGoo, subAssembliesGoo, location,
                allGeometry);
        });

        if (result.IsSuccess == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Assembly");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(assemblyId));

        DA.SetData(1, result.PropertiesGoo);

        DA.SetDataList(2, result.SubAssembliesGoo);

        DA.SetData(3, result.Location);

        DA.SetDataList(4, result.AllGeometry);
    }
}