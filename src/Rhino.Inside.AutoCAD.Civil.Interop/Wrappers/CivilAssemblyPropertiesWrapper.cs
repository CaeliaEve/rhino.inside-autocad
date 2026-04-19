using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Assembly.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted assembly property information.
/// The data is captured at construction time from an <see cref="Assembly"/>.
/// </remarks>
public class CivilAssemblyPropertiesWrapper : ICivilAssemblyProperties
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public string AssemblyType { get; }

    /// <inheritdoc />
    public string Code { get; }

    /// <inheritdoc />
    public int SubassemblyCount { get; }

    /// <inheritdoc />
    public IReadOnlyList<IObjectId> SubassemblyIds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAssemblyPropertiesWrapper"/>
    /// from a Civil 3D Assembly.
    /// </summary>
    /// <param name="assembly">The assembly to extract properties from.</param>
    public CivilAssemblyPropertiesWrapper(Assembly assembly)
    {
        this.Name = assembly.Name;
        this.Description = assembly.Description ?? string.Empty;
        this.AssemblyType = assembly.Type.ToString();
        this.Code = assembly.CodeSetStyleName ?? string.Empty;

        var subassemblyIds = new List<IObjectId>();

        try
        {
            foreach (var group in assembly.Groups)
            {
                foreach (ObjectId subassemblyId in group.GetSubassemblyIds())
                {
                    if (!subassemblyId.IsNull && !subassemblyId.IsErased)
                    {
                        subassemblyIds.Add(new AutocadObjectIdWrapper(subassemblyId));
                    }
                }
            }
        }
        catch
        {
            // Ignore errors during subassembly extraction
        }

        this.SubassemblyCount = subassemblyIds.Count;
        this.SubassemblyIds = subassemblyIds.AsReadOnly();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilAssemblyPropertiesWrapper"/>
    /// with explicit values.
    /// </summary>
    public CivilAssemblyPropertiesWrapper(
        string name,
        string description,
        string assemblyType,
        string code,
        int subassemblyCount,
        IReadOnlyList<IObjectId> subassemblyIds)
    {
        this.Name = name;
        this.Description = description;
        this.AssemblyType = assemblyType;
        this.Code = code;
        this.SubassemblyCount = subassemblyCount;
        this.SubassemblyIds = subassemblyIds;
    }

    /// <summary>
    /// Creates a duplicate of this assembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAssemblyPropertiesWrapper Duplicate()
    {
        return new CivilAssemblyPropertiesWrapper(
            this.Name,
            this.Description,
            this.AssemblyType,
            this.Code,
            this.SubassemblyCount,
            this.SubassemblyIds.ToList().AsReadOnly());
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Assembly Properties: {this.Name} (Type: {this.AssemblyType}, Subassemblies: {this.SubassemblyCount})";
    }
}
