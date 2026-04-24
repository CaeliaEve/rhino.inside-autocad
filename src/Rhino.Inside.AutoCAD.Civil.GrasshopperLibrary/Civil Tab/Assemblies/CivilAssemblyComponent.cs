using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces.Assemblies;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts information from a Civil 3D Assembly.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
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

        pManager.AddTextParameter("Name", "N",
            "The name of the assembly. When set this will update the name of the assembly.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the assembly. When set this will update the description of the assembly.", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddIntegerParameter("Type", "T",
            "The type of the assembly (1=UndividedCrownedRoad, 2=UndividedPlanarRoad, 3=DividedCrownedRoad, 4=DividedPlanarRoad, 5=Other, 6=Railway). When set this will update the type of the assembly.", GH_ParamAccess.item);
        pManager[3].Optional = true;

        pManager.AddTextParameter("Code", "C",
            "The code name of the assembly. When set this will update the code of the assembly.", GH_ParamAccess.item);
        pManager[4].Optional = true;

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style", "Style",
            "The style to apply to this assembly. When set this will update the style of the assembly.", GH_ParamAccess.item);
        pManager[5].Optional = true;

        pManager.AddPointParameter("Location", "Loc",
            "The origin location of the assembly. When set this will update the location of the assembly.", GH_ParamAccess.item);
        pManager[6].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "Id", "Id",
            "The Id of the Assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Type", "T",
            "The type of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Code", "C",
            "The code name of the assembly.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style", "Style",
            "The style applied to this assembly.", GH_ParamAccess.item);

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

            // Get current values
            var newName = wrapper.Name;
            var newDescription = wrapper.Description;
            var newType = (int)wrapper.AssemblyType;
            var newCode = wrapper.Code;
            GH_NamedId? newStyleGoo = null;
            var newLocation = wrapper.Location;

            var updateFlag = false;

            if (DA.GetData(1, ref newName) && newName != wrapper.Name) updateFlag = true;
            if (DA.GetData(2, ref newDescription) && newDescription != wrapper.Description) updateFlag = true;
            if (DA.GetData(3, ref newType) && (CivilAssemblyType)newType != wrapper.AssemblyType) updateFlag = true;
            if (DA.GetData(4, ref newCode) && newCode != wrapper.Code) updateFlag = true;
            if (DA.GetData(5, ref newStyleGoo) && newStyleGoo?.Value?.ObjectId != null &&
                newStyleGoo.Value.ObjectId.Unwrap() != wrapper.Style.ObjectId.Unwrap()) updateFlag = true;
            if (DA.GetData(6, ref newLocation) && newLocation != wrapper.Location) updateFlag = true;

            // Update if any changes were detected
            ICivilAssemblies currentWrapper = wrapper;
            if (updateFlag)
            {
                var styleId = newStyleGoo?.Value?.ObjectId ?? wrapper.Style.ObjectId;

                currentWrapper = wrapper.Update(transactionManager,
                    newName ?? wrapper.Name,
                    newDescription ?? wrapper.Description,
                    (CivilAssemblyType)newType,
                    newCode ?? wrapper.Code,
                    styleId,
                    newLocation);
            }

            var subassemblies = currentWrapper.GetSubassemblies(transactionManager);

            var subAssembliesGoo = subassemblies
                .Select(s => new GH_CivilSubassembly(s)).ToList();

            var allGeometry = subassemblies.SelectMany(s => s.Geometry)
                .ToList();

            return new AssemblyGooResult(
                currentWrapper.Name,
                currentWrapper.Description,
                currentWrapper.AssemblyType,
                currentWrapper.Code,
                currentWrapper.Style,
                subAssembliesGoo,
                currentWrapper.Location,
                allGeometry);
        });

        if (result.IsSuccess == false)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Failed to read Assembly");
            return;
        }

        DA.SetData(0, new GH_AutocadObjectId(assemblyId));
        DA.SetData(1, result.Name);
        DA.SetData(2, result.Description);
        DA.SetData(3, result.AssemblyType?.ToString());
        DA.SetData(4, result.Code);
        DA.SetData(5, result.Style != null ? new GH_NamedId(result.Style) : null);
        DA.SetDataList(6, result.SubAssembliesGoo);
        DA.SetData(7, result.Location);
        DA.SetDataList(8, result.AllGeometry);
    }
}