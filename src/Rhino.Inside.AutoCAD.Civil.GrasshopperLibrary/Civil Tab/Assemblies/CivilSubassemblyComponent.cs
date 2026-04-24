using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a Civil 3D Subassembly.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilSubassemblyComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("A7B8C9D0-E1F2-3456-7890-123456789CDE");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilSubassemblyComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilSubassemblyComponent"/> class.
    /// </summary>
    public CivilSubassemblyComponent()
        : base("Civil3d Subassembly", "CVL-Sub",
            "Extracts individual values from a Civil 3D Subassembly",
            "Civil3d", "Assemblies")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilSubassemblyProperties(GH_ParamAccess.item), "Subassembly",
            "Sub", "A Subassembly from a Civil3d Assembly", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the subassembly. When set this will update the name of the subassembly.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the subassembly. When set this will update the description of the subassembly.", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddIntegerParameter("Side", "S",
            "The side of the subassembly (0=None, 1=Left, 2=Right). When set this will update the side of the subassembly.", GH_ParamAccess.item);
        pManager[3].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the subassembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the subassembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Side", "S",
            "The side of the subassembly (Left, Right, or None).", GH_ParamAccess.item);

        pManager.AddPointParameter("Origin", "O",
            "The origin point of the subassembly.", GH_ParamAccess.item);

        pManager.AddCurveParameter("Geometry", "G",
            "The geometry of the subassembly as curves.", GH_ParamAccess.list);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilSubassembly? civilSubassemblyGoo = null;

        if (!DA.GetData(0, ref civilSubassemblyGoo) || civilSubassemblyGoo?.Value is null) return;

        ICivilSubassembly subassembly = civilSubassemblyGoo.Value;

        var newName = subassembly.Name;
        var newDescription = subassembly.Description;
        var newSide = (int)subassembly.Side;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != subassembly.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != subassembly.Description) updateFlag = true;
        if (DA.GetData(3, ref newSide) && (CivilSide)newSide != subassembly.Side) updateFlag = true;

        var document = this.GetDocumentForObjectId(subassembly.SubassemblyId);
        if (document is null)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
            return;
        }

        var transactionManager = document.CreateTransactionManager();

        if (updateFlag)
        {
            subassembly = transactionManager.PerformTask(() =>
                subassembly.Update(transactionManager, newName, newDescription, (CivilSide)newSide));
        }

        DA.SetData(0, subassembly.Name);
        DA.SetData(1, subassembly.Description);
        DA.SetData(2, subassembly.Side);
        DA.SetData(3, subassembly.Origin);
        DA.SetDataList(4, subassembly.Geometry);
    }
}
