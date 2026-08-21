using System.Runtime.InteropServices;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Fixed-size memory-mapped file header (64 bytes) for zero-copy vertex/mesh streaming.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct SharedMemoryHeader
{
    public const uint MagicValue = 0x52484E4F; // 'RHNO'
    public const int HeaderSize = 64;

    public uint Magic;            // 4 bytes: 0x52484E4F
    public uint Version;          // 4 bytes: Protocol version (1)
    public ulong FrameId;         // 8 bytes: Monotonically increasing frame sequence
    public uint VertexCount;      // 4 bytes: Number of 3D vertices
    public uint IndexCount;       // 4 bytes: Number of triangle indices
    public uint PayloadBytes;     // 4 bytes: Total payload byte length
    public uint ColorArgb;        // 4 bytes: Default ACI or ARGB color
    public long Timestamp;        // 8 bytes: Unix timestamp in ms
    public uint Reserved1;        // 4 bytes
    public uint Reserved2;        // 4 bytes
    public uint Reserved3;        // 4 bytes
    public uint Reserved4;        // 4 bytes
    public uint Reserved5;        // 4 bytes
    public uint Reserved6;        // 4 bytes
}
