using Grasshopper.Kernel;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Provides metadata about the Civil 3D plugin for Grasshopper.
/// Also registers Civil 3D Goo types with the GooTypeRegistry.
/// </summary>
public class CivilGrasshopperPluginInfo : GH_AssemblyInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CivilGrasshopperPluginInfo"/> class.
    /// Registers Civil 3D Goo types with the GooTypeRegistry.
    /// </summary>
    public CivilGrasshopperPluginInfo()
    {
        // Register Civil 3D Goo types with the GooTypeRegistry
        GooTypeRegistry.Instance?.RegisterAssembly(this.GetType().Assembly);
    }

    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public override string Name => "Rhino.Inside.AutoCAD.Civil Plugin";

    /// <summary>
    /// Gets the icon representing the plugin. Returns <c>null</c> as no icon is provided.
    /// </summary>
    public override System.Drawing.Bitmap Icon => null;

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    public override string Description => "Civil 3D components for Rhino.Inside.AutoCAD";

    /// <summary>
    /// Gets the unique identifier (GUID) of the plugin.
    /// </summary>
    public override Guid Id => new Guid("C1V1L3D0-PLUG-INFO-GUID-000000000001");

    /// <summary>
    /// Gets the name of the plugin author.
    /// </summary>
    public override string AuthorName => "Bimorph";

    /// <summary>
    /// Gets the contact information for the plugin author.
    /// </summary>
    public override string AuthorContact => "support@bimorph.com";
}
