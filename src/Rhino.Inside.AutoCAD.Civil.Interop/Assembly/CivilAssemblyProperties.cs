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
public record CivilAssemblyProperties : ICivilAssemblyProperties
{
    /// <summary>
    /// Constructs a new instance of <see cref="CivilAssemblyProperties"/> by extracting
    /// data from a given <see cref="Assembly"/>.
    /// </summary>
    public static CivilAssemblyProperties CreateFromAssembly(Assembly assembly)
    {
        var subassemblyIds = ExtractSubassemblyIds(assembly);

        return new CivilAssemblyProperties()
        {
            Name = assembly.Name,
            Description = assembly.Description ?? string.Empty,
            AssemblyType = assembly.Type.ToString(),
            Code = assembly.CodeSetStyleName ?? string.Empty,
            SubassemblyCount = subassemblyIds.Count,
            SubassemblyIds = subassemblyIds,
        };
    }

    /// <inheritdoc />
    public string Name { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; init; } = string.Empty;

    /// <inheritdoc />
    public string AssemblyType { get; init; } = string.Empty;

    /// <inheritdoc />
    public string Code { get; init; } = string.Empty;

    /// <inheritdoc />
    public int SubassemblyCount { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<IObjectId> SubassemblyIds { get; init; } = Array.Empty<IObjectId>();

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilAssemblyProperties"/>
    /// </summary>
    private CivilAssemblyProperties()
    {
    }

    /// <summary>
    /// Creates a duplicate of this assembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAssemblyProperties ShallowClone()
    {
        return new CivilAssemblyProperties()
        {
            Name = this.Name,
            Description = this.Description,
            AssemblyType = this.AssemblyType,
            Code = this.Code,
            SubassemblyCount = this.SubassemblyCount,
            SubassemblyIds = this.SubassemblyIds.ToList().AsReadOnly(),
        };
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Assembly Properties: {this.Name} (Type: {this.AssemblyType}, Subassemblies: {this.SubassemblyCount})";
    }

    /// <summary>
    /// Extracts subassembly IDs from an assembly.
    /// </summary>
    private static IReadOnlyList<IObjectId> ExtractSubassemblyIds(Assembly assembly)
    {
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

        return subassemblyIds.AsReadOnly();
    }
}
