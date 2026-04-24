using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D Corridor Properties.
/// </summary>
[ComponentVersion(introduced: "1.2.19")]
public class CivilCorridorPropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("D0E1F2A3-B4C5-6789-0123-456789012BCD");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilCorridorPropertiesComponent;

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorPropertiesComponent"/> class.
    /// </summary>
    public CivilCorridorPropertiesComponent()
        : base("Civil3d Corridor Properties", "CVL-CorrProps",
            "Extracts individual values from Civil 3D Corridor Properties",
            "Civil3d", "Corridors")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilCorridorProperties(GH_ParamAccess.item), "Properties",
            "Props", "Corridor properties from a Civil3d Corridor", GH_ParamAccess.item);

        pManager.AddTextParameter("Name", "N",
            "The name of the corridor. When set this will update the name of the corridor.", GH_ParamAccess.item);
        pManager[1].Optional = true;

        pManager.AddTextParameter("Description", "Desc",
            "The description of the corridor. When set this will update the description of the corridor.", GH_ParamAccess.item);
        pManager[2].Optional = true;

        pManager.AddTextParameter("Code", "Code",
            "The code set style name of the corridor. When set this will update the code of the corridor.", GH_ParamAccess.item);
        pManager[3].Optional = true;
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The name of the corridor.", GH_ParamAccess.item);

        pManager.AddTextParameter("Description", "Desc",
            "The description of the corridor.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Code",
            "Code", "The code of the corridor as a NamedId.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Start Param", "StaP",
            "The starting parameter of the corridor.", GH_ParamAccess.item);

        pManager.AddNumberParameter("End Param", "EndP",
            "The ending parameter of the corridor.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this corridor as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilCorridorProperties? propsGoo = null;

        if (!DA.GetData(0, ref propsGoo) || propsGoo?.Value is null) return;

        ICivilCorridorProperties props = propsGoo.Value;

        var newName = props.Name;
        var newDescription = props.Description;
        var newCode = props.Code.Name;

        var updateFlag = false;

        if (DA.GetData(1, ref newName) && newName != props.Name) updateFlag = true;
        if (DA.GetData(2, ref newDescription) && newDescription != props.Description) updateFlag = true;
        if (DA.GetData(3, ref newCode) && newCode != props.Code.Name) updateFlag = true;

        if (updateFlag)
        {
            var document = this.GetDocumentForObjectId(props.CorridorId);
            if (document is null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No document available");
                return;
            }

            var transactionManager = document.CreateTransactionManager();
            props = transactionManager.PerformTask(() =>
                props.Update(transactionManager, newName, newDescription, newCode));
        }

        DA.SetData(0, props.Name);
        DA.SetData(1, props.Description);
        DA.SetData(2, new GH_NamedId(props.Code));
        DA.SetData(3, props.StartParam);
        DA.SetData(4, props.EndParam);
        DA.SetData(5, new GH_NamedId(props.Style));
    }
}
