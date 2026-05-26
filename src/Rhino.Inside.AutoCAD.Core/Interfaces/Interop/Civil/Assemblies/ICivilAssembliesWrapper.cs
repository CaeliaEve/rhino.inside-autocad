namespace Rhino.Inside.AutoCAD.Core.Interfaces.Assemblies;

/// <summary>
/// A wrapper interface for Civil 3D Assemblies, providing access to assembly properties
/// and subassemblies
/// </summary>
public interface ICivilAssemblies
{
    /// <summary>
    /// The name of this assembly
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The description of this assembly
    /// </summary>
    string Description { get; }

    /// <summary>
    /// The type of this assembly
    /// </summary>
    CivilAssemblyType AssemblyType { get; }

    /// <summary>
    /// The code name of this assembly
    /// </summary>
    string Code { get; }

    /// <summary>
    /// The style applied to this assembly as a NamedId
    /// </summary>
    INamedId Style { get; }

    /// <summary>
    /// Extracts assembly groups from a Civil 3D Assembly.
    /// </summary>
    /// <returns>A list of assembly group names.</returns>
    List<string> GetGroupNames();

    /// <summary>
    /// Extracts all subassemblies from a Civil 3D Assembly as property wrappers.
    /// </summary>
    List<ICivilSubassembly> GetSubassemblies(IAutocadTransactionManager transactionManager);

    /// <summary>
    /// The object ID of the assembly.
    /// </summary>
    IObjectId AssemblyId { get; }

    /// <summary>
    /// The origin location of the assembly.
    /// </summary>
    Rhino.Geometry.Point3d Location { get; }

    /// <summary>
    /// Updates the assembly with new properties and returns a new
    /// assembly wrapper object with the updated values.
    /// </summary>
    /// <param name="transactionManager">The transaction manager to use for the update.</param>
    /// <param name="newName">The new name for the assembly.</param>
    /// <param name="newDescription">The new description for the assembly.</param>
    /// <param name="newType">The new type for the assembly.</param>
    /// <param name="newCode">The new code name for the assembly.</param>
    /// <param name="newStyleId">The new style ID for the assembly.</param>
    /// <param name="newLocation">The new location for the assembly.</param>
    /// <returns>A new assembly wrapper with the updated values.</returns>
    ICivilAssemblies Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription, CivilAssemblyType newType,
        string newCode, IObjectId newStyleId, Rhino.Geometry.Point3d newLocation);
}