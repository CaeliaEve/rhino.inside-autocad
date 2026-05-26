using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps properties extracted from a Civil 3D Corridor.
/// </summary>
/// <remarks>
/// This is a simple data wrapper class that holds extracted corridor property information.
/// The data is captured at construction time from a <see cref="Corridor"/>.
/// </remarks>
public record CivilCorridorProperties : ICivilCorridorProperties
{
    private readonly Corridor _corridor;

    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public string Description { get; }

    /// <inheritdoc />
    public INamedId Code { get; }

    /// <inheritdoc />
    public double StartParam { get; }

    /// <inheritdoc />
    public double EndParam { get; }

    /// <inheritdoc />
    public INamedId Style { get; }

    /// <inheritdoc />
    public IObjectId CorridorId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CivilCorridorProperties"/> class
    /// by extracting data from a given <see cref="Corridor"/>.
    /// </summary>
    /// <param name="corridor">The Civil 3D corridor to extract properties from.</param>
    public CivilCorridorProperties(Corridor corridor)
    {
        _corridor = corridor;

        this.Name = corridor.Name;
        this.Description = corridor.Description ?? string.Empty;
        this.Code = new NamedId(corridor.CodeSetStyleName, corridor.CodeSetStyleId);
        this.StartParam = corridor.StartParam;
        this.EndParam = corridor.EndParam;
        this.Style = new NamedId(corridor.StyleName, corridor.StyleId);
        this.CorridorId = new AutocadObjectIdWrapper(corridor.Id);
    }

    /// <inheritdoc />
    public ICivilCorridorProperties Update(IAutocadTransactionManager transactionManager,
        string newName, string newDescription, string newCode)
    {
        var corridor = transactionManager.Unwrap().GetObject(_corridor.Id, OpenMode.ForWrite) as Corridor;

        if (corridor == null)
        {
            return this;
        }

        corridor.Name = newName;
        corridor.Description = newDescription;
        corridor.CodeSetStyleName = newCode;

        return new CivilCorridorProperties(corridor);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"Corridor Properties: {this.Name} (Param: {this.StartParam:F2} - {this.EndParam:F2})";
    }
}
