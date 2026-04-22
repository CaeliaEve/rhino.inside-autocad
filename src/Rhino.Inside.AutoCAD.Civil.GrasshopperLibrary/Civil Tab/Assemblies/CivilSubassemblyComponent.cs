using Grasshopper.Kernel;
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
        GH_CivilSubassemblyProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        var props = propsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, props.Side);
        DA.SetData(3, props.Origin);
        DA.SetDataList(4, props.Geometry);
    }
}
