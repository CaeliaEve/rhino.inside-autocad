using Grasshopper.Kernel;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents a Grasshopper parameter for Civil 3D Subassembly properties.
/// </summary>
/// <remarks>
/// This parameter type wraps <see cref="GH_CivilSubassemblyProperties"/> objects which
/// contain properties from Subassemblies.
/// </remarks>
public class Param_CivilSubassemblyProperties : GH_Param<GH_CivilSubassemblyProperties>
{
    /// <inheritdoc />
    public override Guid ComponentGuid => new Guid("C3D4E5F6-A7B8-9012-CDEF-123456789012");

    /// <inheritdoc />
    public override GH_Exposure Exposure => GH_Exposure.primary;

    /// <inheritdoc />
    protected override System.Drawing.Bitmap Icon => Properties.Resources.Param_CivilSubassemblyProperties;

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSubassemblyProperties"/> class with the
    /// specified instance description.
    /// </summary>
    public Param_CivilSubassemblyProperties(IGH_InstanceDescription tag) : base(tag)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSubassemblyProperties"/> class with the
    /// specified instance description and parameter access type.
    /// </summary>
    public Param_CivilSubassemblyProperties(IGH_InstanceDescription tag, GH_ParamAccess access)
        : base(tag, access)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Param_CivilSubassemblyProperties"/> class.
    /// </summary>
    public Param_CivilSubassemblyProperties(GH_ParamAccess access)
        : base("Civil3d Subassembly Properties", "SubasmProps",
            "Properties from a Civil 3D Subassembly", "Params", "Civil3d", access)
    { }
}
