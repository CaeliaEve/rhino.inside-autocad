using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps rail alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilRailAlignmentInfo : ICivilRailAlignmentInfo
{
    private readonly RailAlignmentInfo _railInfo;

    /// <inheritdoc />
    public bool IsRailAlignment { get; }

    /// <inheritdoc />
    public double TrackWidth { get; }

    /// <summary>
    /// Gets an empty rail alignment info instance.
    /// </summary>
    public static CivilRailAlignmentInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilRailAlignmentInfo"/>.
    /// </summary>
    private CivilRailAlignmentInfo()
    {
        this.IsRailAlignment = false;
        this.TrackWidth = 0;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilRailAlignmentInfo"/> from an Alignment.
    /// </summary>
    public CivilRailAlignmentInfo(RailAlignmentInfo railInfo)
    {
        _railInfo = railInfo;
        this.IsRailAlignment = true;
        this.TrackWidth = railInfo.TrackWidth;
    }

    /// <inheritdoc />
    public ICivilRailAlignmentInfo ShallowClone()
    {
        return this.IsRailAlignment
            ? new CivilRailAlignmentInfo(_railInfo)
            : CivilRailAlignmentInfo.Empty;
    }

    /// <summary>
    /// Returns a string representation of this rail alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!this.IsRailAlignment)
            return "Not Rail Alignment";

        return $"Rail Alignment: TrackWidth={this.TrackWidth:F2}";
    }
}
