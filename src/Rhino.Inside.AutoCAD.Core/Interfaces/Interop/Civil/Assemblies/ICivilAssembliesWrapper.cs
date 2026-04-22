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
    /// The properties of this assembly, extracted from the Civil 3D database object.
    /// </summary>
    ICivilAssemblyProperties Properties { get; }

    /// <summary>
    /// Extracts assembly groups from a Civil 3D Assembly.
    /// </summary>
    /// <returns>A list of assembly group names.</returns>
    List<string> GetGroupNames();

    /// <summary>
    /// Extracts all subassemblies from a Civil 3D Assembly as property wrappers.
    /// </summary>
    List<ICivilSubassembly> GetSubassemblies(IAutocadTransactionManager transactionManager);
}