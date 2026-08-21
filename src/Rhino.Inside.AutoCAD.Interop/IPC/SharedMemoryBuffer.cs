using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using Rhino.Inside.AutoCAD.Core.IPC;

namespace Rhino.Inside.AutoCAD.Interop.IPC;

/// <summary>
/// High-performance shared memory buffer for geometry and transient graphic frames.
/// </summary>
public class SharedMemoryBuffer : IDisposable
{
    private readonly MemoryMappedFile _mmf;
    private readonly MemoryMappedViewAccessor _accessor;
    private readonly long _capacity;
    private bool _isDisposed;
    private ulong _frameSequence;

    public long Capacity => _capacity;

    private SharedMemoryBuffer(MemoryMappedFile mmf, MemoryMappedViewAccessor accessor, long capacity)
    {
        _mmf = mmf;
        _accessor = accessor;
        _capacity = capacity;
    }

    /// <summary>
    /// Creates a new named shared memory mapping (Producer / Server side).
    /// </summary>
    public static SharedMemoryBuffer Create(string mapName, long capacity = 32 * 1024 * 1024)
    {
        var mmf = MemoryMappedFile.CreateOrOpen(mapName, capacity, MemoryMappedFileAccess.ReadWrite);
        var accessor = mmf.CreateViewAccessor(0, capacity, MemoryMappedFileAccess.ReadWrite);
        return new SharedMemoryBuffer(mmf, accessor, capacity);
    }

    /// <summary>
    /// Opens an existing named shared memory mapping (Consumer / Client side).
    /// </summary>
    public static SharedMemoryBuffer Open(string mapName, long capacity = 32 * 1024 * 1024)
    {
        var mmf = MemoryMappedFile.OpenExisting(mapName, MemoryMappedFileRights.ReadWrite);
        var accessor = mmf.CreateViewAccessor(0, capacity, MemoryMappedFileAccess.ReadWrite);
        return new SharedMemoryBuffer(mmf, accessor, capacity);
    }

    /// <summary>
    /// Writes triangle mesh vertices and indices to shared memory for AutoCAD viewport transient rendering.
    /// </summary>
    public unsafe bool WriteMeshFrame(float[] vertices, int[] indices, uint colorArgb)
    {
        if (_isDisposed || vertices == null || indices == null) return false;

        uint vertexCount = (uint)(vertices.Length / 3);
        uint indexCount = (uint)indices.Length;
        uint vertexBytes = (uint)(vertices.Length * sizeof(float));
        uint indexBytes = (uint)(indices.Length * sizeof(int));
        uint totalPayload = vertexBytes + indexBytes;

        if (SharedMemoryHeader.HeaderSize + totalPayload > _capacity)
            return false;

        var header = new SharedMemoryHeader
        {
            Magic = SharedMemoryHeader.MagicValue,
            Version = 1,
            FrameId = ++_frameSequence,
            VertexCount = vertexCount,
            IndexCount = indexCount,
            PayloadBytes = totalPayload,
            ColorArgb = colorArgb,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        _accessor.Write(0, ref header);

        byte* ptr = null;
        _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        try
        {
            byte* dataPtr = ptr + SharedMemoryHeader.HeaderSize;

            fixed (float* vPtr = vertices)
            {
                Buffer.MemoryCopy(vPtr, dataPtr, vertexBytes, vertexBytes);
            }

            fixed (int* iPtr = indices)
            {
                Buffer.MemoryCopy(iPtr, dataPtr + vertexBytes, indexBytes, indexBytes);
            }

            return true;
        }
        finally
        {
            if (ptr != null)
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }

    /// <summary>
    /// Reads mesh frame from shared memory into pre-allocated or new arrays.
    /// </summary>
    public unsafe bool ReadMeshFrame(out SharedMemoryHeader header, out float[] vertices, out int[] indices)
    {
        header = default;
        vertices = Array.Empty<float>();
        indices = Array.Empty<int>();

        if (_isDisposed) return false;

        _accessor.Read(0, out header);
        if (header.Magic != SharedMemoryHeader.MagicValue || header.VertexCount == 0)
            return false;

        vertices = new float[header.VertexCount * 3];
        indices = new int[header.IndexCount];

        int vertexBytes = (int)(header.VertexCount * 3 * sizeof(float));
        int indexBytes = (int)(header.IndexCount * sizeof(int));

        byte* ptr = null;
        _accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
        try
        {
            byte* dataPtr = ptr + SharedMemoryHeader.HeaderSize;

            fixed (float* vPtr = vertices)
            {
                Buffer.MemoryCopy(dataPtr, vPtr, vertexBytes, vertexBytes);
            }

            fixed (int* iPtr = indices)
            {
                Buffer.MemoryCopy(dataPtr + vertexBytes, iPtr, indexBytes, indexBytes);
            }

            return true;
        }
        finally
        {
            if (ptr != null)
                _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        _accessor.Dispose();
        _mmf.Dispose();
    }
}
