using Autodesk.Civil.DatabaseServices;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Civil.Interop;

/// <summary>
/// Wraps rail alignment information extracted from a Civil 3D Alignment.
/// </summary>
public record CivilRailAlignmentInfo : ICivilRailAlignmentInfo
{
    /// <inheritdoc />
    public bool IsRailAlignment { get; }

    /// <inheritdoc />
    public double Gauge { get; }

    /// <inheritdoc />
    public ICivilCANTInfo CANTInfo { get; }

    /// <summary>
    /// Gets an empty rail alignment info instance.
    /// </summary>
    public static CivilRailAlignmentInfo Empty { get; } = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="CivilRailAlignmentInfo"/>.
    /// </summary>
    private CivilRailAlignmentInfo()
    {
        IsRailAlignment = false;
        Gauge = 0;
        CANTInfo = CivilCANTInfo.Empty;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="CivilRailAlignmentInfo"/> from an Alignment.
    /// </summary>
    /// <param name="alignment">The Civil 3D alignment to extract rail info from.</param>
    public CivilRailAlignmentInfo(Alignment alignment)
    {
        try
        {
            IsRailAlignment = alignment.IsRailAlignment;

            if (!IsRailAlignment)
            {
                Gauge = 0;
                CANTInfo = CivilCANTInfo.Empty;
                return;
            }

            Gauge = alignment.RailAlignmentInfo?.Gauge ?? 0;
            CANTInfo = new CivilCANTInfo(alignment);
        }
        catch
        {
            IsRailAlignment = false;
            Gauge = 0;
            CANTInfo = CivilCANTInfo.Empty;
        }
    }

    /// <inheritdoc />
    public ICivilRailAlignmentInfo ShallowClone()
    {
        return this with { };
    }

    /// <summary>
    /// Returns a string representation of this rail alignment information.
    /// </summary>
    public override string ToString()
    {
        if (!IsRailAlignment)
            return "Not Rail Alignment";

        return $"Rail Alignment: Gauge={Gauge:F2}, HasCANT={CANTInfo.HasCANT}";
    }
}
