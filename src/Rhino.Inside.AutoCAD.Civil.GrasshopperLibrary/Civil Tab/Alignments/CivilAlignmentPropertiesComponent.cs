using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D Alignment Properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilAlignmentPropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("F6A7B8C9-D0E1-2345-F012-567890123DEF");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilAlignmentPropertiesComponent"/> class.
    /// </summary>
    public CivilAlignmentPropertiesComponent()
        : base("Civil3d Alignment Properties", "CVL-AlignProps",
            "Extracts individual values from Civil 3D Alignment Properties",
            "Civil3d", "Alignments")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilAlignmentProperties(GH_ParamAccess.item), "Properties",
            "Props", "Alignment properties from a Civil3d Alignment", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Station", "StaSt",
            "The starting station of the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Station", "StaEnd",
            "The ending station of the alignment.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Length", "Len",
            "The total length of the alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Alignment Type", "Type",
            "The type of alignment (Centerline, Offset, CurbReturn, etc.).", GH_ParamAccess.item);

        pManager.AddIntegerParameter("Entity Count", "Count",
            "The number of entities in the alignment.", GH_ParamAccess.item);

        pManager.AddTextParameter("Site Name", "Site",
            "The name of the site containing this alignment.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilAlignmentProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        var props = propsGoo.Value;

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, props.StartStation);
        DA.SetData(3, props.EndStation);
        DA.SetData(4, props.Length);
        DA.SetData(5, props.AlignmentTypeName);
        DA.SetData(6, props.EntityCount);
        DA.SetData(7, props.SiteName);
    }
}
