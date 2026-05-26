using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Interop;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps connected alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilConnectedAlignmentInfo : ICivilConnectedAlignmentInfo
{
    /// <summary>
    /// Constructs an empty instance of <see cref="CivilConnectedAlignmentInfo"/> representing a
    /// non connnected alignment.
    /// </summary>
    public static CivilConnectedAlignmentInfo Empty { get; } = new();

    /// <inheritdoc />
    public bool IsConnectedAlignment { get; }

    /// <inheritdoc />
    public double ConnectionOverlapLengthIn { get; }

    /// <inheritdoc />
    public double ConnectionOverlapLengthOut { get; }

    /// <inheritdoc />
    public IObjectId IncomingParentAlignmentId { get; }

    /// <inheritdoc />
    public IObjectId OutgoingParentAlignmentId { get; }

    /// <inheritdoc />
    public double OffsetIn { get; }

    /// <inheritdoc />
    public double OffsetOut { get; }

    /// <summary>
    /// Private constructor to create an empty instance of <see cref="CivilConnectedAlignmentInfo"/>
    /// with default values.
    /// </summary>
    private CivilConnectedAlignmentInfo()
    {
        this.ConnectionOverlapLengthIn = 0;
        this.ConnectionOverlapLengthOut = 0;
        this.IncomingParentAlignmentId = AutocadObjectIdWrapper.DefaultId;
        this.OutgoingParentAlignmentId = AutocadObjectIdWrapper.DefaultId;
        this.OffsetIn = 0;
        this.OffsetOut = 0;
        this.IsConnectedAlignment = false;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilConnectedAlignmentInfo"/> from an Alignment.
    /// </summary>
    /// <param name="info">The Civil 3D alignment to extract connection info from.</param>
    public CivilConnectedAlignmentInfo(ConnectedAlignmentInfo info)
    {
        this.ConnectionOverlapLengthIn = info.ConnectionOverlapLengthIn;
        this.ConnectionOverlapLengthOut = info.ConnectionOverlapLengthOut;
        this.OffsetIn = info.OffsetIn;
        this.OffsetOut = info.OffsetOut;

        this.IncomingParentAlignmentId = new AutocadObjectIdWrapper(info.IncomingParentAlignmentId);
        this.OutgoingParentAlignmentId = new AutocadObjectIdWrapper(info.OutgoingParentAlignmentId);

        this.IsConnectedAlignment = true;

    }

    /// <inheritdoc />
    public ICivilConnectedAlignmentInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this connected alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!this.IsConnectedAlignment)
            return "Not Connected Alignment";

        return $"Connected Alignment: OverlapIn={this.ConnectionOverlapLengthIn:F2}, OverlapOut={this.ConnectionOverlapLengthOut:F2}, OffsetIn={this.OffsetIn:F2}, OffsetOut={this.OffsetOut:F2}";
    }
}
