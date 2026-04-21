using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps offset alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilOffsetAlignmentInfo : ICivilOffsetAlignmentInfo
{
    /// <summary>
    /// Constructs an empty instance of <see cref="CivilOffsetAlignmentInfo"/> representing a
    /// non-offset alignment.
    /// </summary>
    public static CivilOffsetAlignmentInfo Empty { get; } = new();

    /// <inheritdoc />
    public bool IsOffsetAlignment { get; }

    /// <inheritdoc />
    public double NominalOffset { get; }

    /// <inheritdoc />
    public string Side { get; }

    /// <inheritdoc />
    public IObjectId ParentAlignmentId { get; }

    /// <inheritdoc />
    public IReadOnlyList<ICivilOffsetAlignmentRegion> Regions { get; }

    /// <summary>
    /// Private constructor to create an empty instance representing a non-offset alignment.
    /// </summary>
    private CivilOffsetAlignmentInfo()
    {
        this.NominalOffset = 0;
        this.Side = string.Empty;
        this.ParentAlignmentId = AutocadObjectIdWrapper.DefaultId;
        this.Regions = Array.Empty<ICivilOffsetAlignmentRegion>();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilOffsetAlignmentInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract offset info from.</param>
    public CivilOffsetAlignmentInfo(OffsetAlignmentInfo info)
    {
        this.NominalOffset = info.NominalOffset;
        this.Side = info.Side.ToString();
        this.ParentAlignmentId = new AutocadObjectIdWrapper(info.ParentAlignmentId);

        // Extract regions
        var regions = new List<ICivilOffsetAlignmentRegion>();
        foreach (var region in info.Regions)
        {
            regions.Add(new CivilOffsetAlignmentRegion(region));
        }
        this.Regions = regions;
    }

    /// <inheritdoc />
    public ICivilOffsetAlignmentInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this offset alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!this.IsOffsetAlignment)
            return "Not Offset Alignment";

        return $"Offset Alignment: NominalOffset={this.NominalOffset:F2}, Side={this.Side}, Regions={this.Regions.Count}";
    }
}
