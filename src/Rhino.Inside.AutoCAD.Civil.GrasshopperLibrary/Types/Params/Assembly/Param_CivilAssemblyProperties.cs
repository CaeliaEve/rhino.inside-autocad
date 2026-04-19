using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Assembly properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilAssemblyProperties"/> objects which
/// contain properties from Assemblies.
/// </remarks>
public class Param_CivilAssemblyProperties : GH_Param<GH_CivilAssemblyProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("B2C3D4E5-F6A7-8901-BCDE-F12345678901");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.CivilDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAssemblyProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilAssemblyProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAssemblyProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilAssemblyProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilAssemblyProperties"/> class.
    /// </summary>
    public Param_CivilAssemblyProperties(GH_ParamAccess access)
        : base("Civil3d Assembly Properties", "AsmProps",
            "Properties from a Civil 3D Assembly", "Params", "Civil3d", access)
    { }
}
