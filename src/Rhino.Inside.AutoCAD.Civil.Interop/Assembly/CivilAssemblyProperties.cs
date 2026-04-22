using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core;
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
    private readonly Assembly _assembly;

    /// <inheritdoc />
    public string Name { get; } = string.Empty;

    /// <inheritdoc />
    public string Description { get; } = string.Empty;

    /// <inheritdoc />
    public CivilAssemblyType AssemblyType { get; }

    /// <inheritdoc />
    public string Code { get; } = string.Empty;

    /// <inheritdoc />
    public IReadOnlyList<IObjectId> SubassemblyIds { get; } = Array.Empty<IObjectId>();

    /// <inheritdoc />
    public INamedId Style { get; } = NamedId.Empty;

    /// <summary>
    /// Initializes a new private empty instance of <see cref="CivilAssemblyProperties"/>
    /// </summary>
    public CivilAssemblyProperties(Assembly assembly)
    {
        _assembly = assembly;

        var subassemblyIds = this.ExtractSubassemblyIds(assembly);

        this.Name = assembly.Name;
        this.Description = assembly.Description ?? string.Empty;
        this.AssemblyType = assembly.Type.ToRhinoInsideAssemblyType();
        this.Code = assembly.CodeSetStyleName ?? string.Empty;
        this.SubassemblyIds = subassemblyIds;
        this.Style = new NamedId(assembly.StyleName, assembly.StyleId);

    }

    /// <summary>
    /// Extracts subassembly IDs from an assembly.
    /// </summary>
    private IReadOnlyList<IObjectId> ExtractSubassemblyIds(Assembly assembly)
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

    /// <summary>
    /// Creates a duplicate of this assembly properties wrapper.
    /// </summary>
    /// <returns>A new instance with copied data.</returns>
    public CivilAssemblyProperties Duplicate()
    {
        return new CivilAssemblyProperties(_assembly);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Assembly Properties: {this.Name} (Type: {this.AssemblyType}, Subassemblies: {this.SubassemblyIds.Count})";
    }
}
