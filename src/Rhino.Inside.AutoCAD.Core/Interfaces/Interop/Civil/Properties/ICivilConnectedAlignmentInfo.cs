namespace Rhino.Inside.AutoCAD.Core.Interfaces;

/// <summary>
/// Represents connected alignment information for a Civil 3D Alignment.
/// </summary>
/// <remarks>
/// Wraps the ConnectedAlignmentInfo from a Civil 3D Alignment, providing
/// connection overlap lengths, parent alignment references, and offset values.
/// </remarks>
public interface ICivilConnectedAlignmentInfo
{
    /// <summary>
    /// Gets a value indicating whether this alignment is a connected alignment.
    /// </summary>
    bool IsConnectedAlignment { get; }

    /// <summary>
    /// Gets the connection overlap length at the incoming end.
    /// </summary>
    double ConnectionOverlapLengthIn { get; }

    /// <summary>
    /// Gets the connection overlap length at the outgoing end.
    /// </summary>
    double ConnectionOverlapLengthOut { get; }

    /// <summary>
    /// Gets the incoming parent alignment ObjectId.
    /// </summary>
    IObjectId IncomingParentAlignmentId { get; }

    /// <summary>
    /// Gets the outgoing parent alignment ObjectId.
    /// </summary>
    IObjectId OutgoingParentAlignmentId { get; }

    /// <summary>
    /// Gets the offset value at the incoming connection.
    /// </summary>
    double OffsetIn { get; }

    /// <summary>
    /// Gets the offset value at the outgoing connection.
    /// </summary>
    double OffsetOut { get; }

    /// <summary>
    /// Creates a shallow copy of this connected alignment information.
    /// </summary>
    /// <returns>A new instance with the same values.</returns>
    ICivilConnectedAlignmentInfo ShallowClone();
}
