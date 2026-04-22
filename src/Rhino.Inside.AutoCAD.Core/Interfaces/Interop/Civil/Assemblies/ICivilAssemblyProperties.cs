namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents properties extracted from a Civil 3D Assembly.
/// </summary>
/// <remarks>
/// This interface provides access to assembly metadata
/// without requiring direct access to the Civil 3D database object.
/// </remarks>
public interface ICivilAssemblyProperties
{
    /// <summary>
    /// Gets the name of the assembly.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the description of the assembly.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the type of the assembly.
    /// </summary>
    CivilAssemblyType AssemblyType { get; }

    /// <summary>
    /// Gets the code name of the assembly.
    /// </summary>
    string Code { get; }

    /// <summary>
    /// Gets the subassembly ObjectIds in the assembly.
    /// </summary>
    IReadOnlyList<IObjectId> SubassemblyIds { get; }

    /// <summary>
    /// Gets the style applied to this assembly as a NamedId.
    /// </summary>
    /// <remarks>
    /// Provides both the style name and ObjectId reference.
    /// </remarks>
    INamedId Style { get; }
}
