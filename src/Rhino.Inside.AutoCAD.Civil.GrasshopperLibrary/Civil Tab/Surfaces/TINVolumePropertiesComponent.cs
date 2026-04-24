using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.GrasshopperLibrary;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from Civil 3D TIN Volume Properties.
/// </summary>
[ComponentVersion(introduced: "1.1.19")]
public class TINVolumePropertiesComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new("C3D4E5F6-A7B8-9012-CDEF-234567890ABC");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.TINVolumePropertiesComponent;

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <summary>
    /// Initializes a new instance of the <see cref="TINVolumePropertiesComponent"/> class.
    /// </summary>
    public TINVolumePropertiesComponent()
        : base("Civil3d Volume Properties", "CVL-VolProps",
            "Extracts individual values from Civil 3D TIN Volume Properties",
            "Civil3d", "Surfaces")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_CivilVolumeProperties(GH_ParamAccess.item), "Volume Properties",
            "VP", "Volume properties from a Civil3d Volume Surface", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddNumberParameter("Unadjusted Cut", "UCut",
            "Raw cut volume before factors (cubic units).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Unadjusted Fill", "UFill",
            "Raw fill volume before factors (cubic units).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Unadjusted Net", "UNet",
            "Raw net volume (unadjusted cut - unadjusted fill).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Cut Factor", "CutF",
            "Cut volume adjustment factor.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Fill Factor", "FillF",
            "Fill volume adjustment factor.", GH_ParamAccess.item);

        pManager.AddNumberParameter("Adjusted Cut", "ACut",
            "Adjusted cut volume (raw * factor).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Adjusted Fill", "AFill",
            "Adjusted fill volume (raw * factor).", GH_ParamAccess.item);

        pManager.AddNumberParameter("Adjusted Net", "ANet",
            "Adjusted net volume.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Style",
            "Style", "The style applied to this volume surface as a NamedId.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        GH_CivilVolumeProperties? volumePropsGoo = null;

        if (!DA.GetData(0, ref volumePropsGoo) || volumePropsGoo?.Value is null) return;

        var props = volumePropsGoo.Value;

        // Unadjusted volumes
        DA.SetData(0, props.UnadjustedCutVolume);
        DA.SetData(1, props.UnadjustedFillVolume);
        DA.SetData(2, props.UnadjustedNetVolume);

        // Factors
        DA.SetData(3, props.CutFactor);
        DA.SetData(4, props.FillFactor);

        // Adjusted volumes
        DA.SetData(5, props.AdjustedCutVolume);
        DA.SetData(6, props.AdjustedFillVolume);
        DA.SetData(7, props.AdjustedNetVolume);
        DA.SetData(8, new GH_NamedId(props.Style));
    }
}
