using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D Assembly Properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAssemblyPropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F6A7B8C9-D0E1-2345-6789-012345678BCD");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilAssemblyPropertiesComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAssemblyPropertiesComponent"/> class.
    /// </summary>
    public CivilAssemblyPropertiesComponent()
        : base("Civil3d Assembly Properties", "CVL-AsmProps",
            "Extracts individual values from Civil 3D Assembly Properties",
            "Civil3d", "Assemblies")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAssemblyProperties(GH_ParamAccess.item), "Properties",
            "Props", "Assembly properties from a Civil3d Assembly", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Type", "T",
            "The type of the assembly.", GH_ParamAccess.item);

        pManager.AddTextParameter("Code", "C",
            "The code name of the assembly.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.list), "Subassembly Ids", "SubIds",
            "The subassembly ObjectIds in the assembly.", GH_ParamAccess.list);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this assembly as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAssemblyProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        var props = propsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, props.AssemblyType);
        DA.SetData(3, props.Code);
        DA.SetDataList(4, props.SubassemblyIds.Select(id => new GH_AutocadObjectId(id)).ToList());
        DA.SetData(5, new GH_NamedId(props.Style));
    }
}
