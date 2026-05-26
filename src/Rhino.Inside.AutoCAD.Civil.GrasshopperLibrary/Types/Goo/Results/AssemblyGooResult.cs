using Rhino.Geometry;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.GrasshopperLibrary;

/// <summary>
/// Represents the result of extracting data from a Civil 3D Assembly.
/// </summary>
public class AssemblyGooResult : GooResultBase
{
    /// <summary>
    /// Gets the name of the Assembly.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the description of the Assembly.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the type of the Assembly.
    /// </summary>
    public CivilAssemblyType? AssemblyType { get; }

    /// <summary>
    /// Gets the code name of the Assembly.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Gets the style applied to the Assembly.
    /// </summary>
    public INamedId? Style { get; }

    /// <summary>
    /// Gets the list of subassemblies in the Assembly.
    /// </summary>
    public List<GH_CivilSubassembly>? SubAssembliesGoo { get; }

    /// <summary>
    /// Gets the origin location of the Assembly.
    /// </summary>
    public Point3d? Location { get; }

    /// <summary>
    /// Gets the combined geometry from all subassemblies as curves.
    /// </summary>
    public List<Curve>? AllGeometry { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AssemblyGooResult"/> class.
    /// </summary>
    /// <param name="name">The name of the Assembly.</param>
    /// <param name="description">The description of the Assembly.</param>
    /// <param name="assemblyType">The type of the Assembly.</param>
    /// <param name="code">The code name of the Assembly.</param>
    /// <param name="style">The style applied to the Assembly.</param>
    /// <param name="subAssembliesGoo">A list of subassemblies in the Assembly.</param>
    /// <param name="location">The origin location of the Assembly.</param>
    /// <param name="allGeometry">The combined geometry from all subassemblies as curves.</param>
    public AssemblyGooResult(
        string? name,
        string? description,
        CivilAssemblyType? assemblyType,
        string? code,
        INamedId? style,
        List<GH_CivilSubassembly>? subAssembliesGoo,
        Point3d? location,
        List<Curve>? allGeometry)
    {
        Name = name;
        Description = description;
        AssemblyType = assemblyType;
        Code = code;
        Style = style;
        SubAssembliesGoo = subAssembliesGoo;
        Location = location;
        AllGeometry = allGeometry;
    }

    /// <summary>
    /// Gets a failed result instance.
    /// </summary>
    public static AssemblyGooResult Failed => new(null, null, null, null, null, null, null, null) { IsSuccess = false };
}
