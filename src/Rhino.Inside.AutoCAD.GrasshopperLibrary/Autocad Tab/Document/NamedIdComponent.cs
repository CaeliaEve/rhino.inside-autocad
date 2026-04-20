using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// A Grasshopper component that extracts individual values from a NamedId.
/// </summary>
/// <remarks>
/// Decomposes a NamedId into its constituent parts: the display name and the
/// underlying AutoCAD ObjectId reference.
/// </remarks>
[ComponentVersion(introduced: "1.2.19")]
public class NamedIdComponent : RhinoInsideAutocad_ComponentBase
{
    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.secondary;

    /// <inheritdoc />
    public override Guid ComponentGuid => new("d5e6f7a8-9b0c-1d2e-3f4a-5b6c7d8e9f0a");

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_AutocadId;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedIdComponent"/> class.
    /// </summary>
    public NamedIdComponent()
        : base("Named Id", "NamedId",
            "Extracts the name and ObjectId from a Named Id",
            "AutoCAD", "Document")
    {
    }

    /// <inheritdoc />
    protected override void RegisterInputParams(GH_InputParamManager pManager)
    {
        pManager.AddParameter(new Param_NamedId(GH_ParamAccess.item), "Named Id",
            "NId", "A Named Id combining a name with an ObjectId", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void RegisterOutputParams(GH_OutputParamManager pManager)
    {
        pManager.AddTextParameter("Name", "N",
            "The display name of the referenced object.", GH_ParamAccess.item);

        pManager.AddParameter(new Param_AutocadObjectId(GH_ParamAccess.item), "ObjectId",
            "Id", "The AutoCAD ObjectId reference.", GH_ParamAccess.item);

        pManager.AddBooleanParameter("Is Valid", "V",
            "Boolean indicating if this NamedId is valid.", GH_ParamAccess.item);
    }

    /// <inheritdoc />
    protected override void SolveInstance(IGH_DataAccess DA)
    {
        NamedId? namedId = null;

        if (!DA.GetData(0, ref namedId) || namedId is null)
            return;

        DA.SetData(0, namedId.Name);
        DA.SetData(1, new GH_AutocadObjectId(namedId.ObjectId));
        DA.SetData(2, namedId.IsValid);
    }
}
