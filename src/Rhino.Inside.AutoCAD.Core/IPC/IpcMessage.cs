using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// High-speed binary framing protocol for the AutoCAD - Rhino 8 IPC link.
/// Frame format: [Magic:2B][MsgType:2B][PayloadLength:4B][PayloadData:NB]
/// </summary>
public class IpcMessage
{
    private const ushort MagicHeader = 0x5249; // 'RI' for Rhino.Inside

    public IpcCommandType CommandType { get; set; }
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    public string GetPayloadAsString()
    {
        return Payload.Length > 0 ? Encoding.UTF8.GetString(Payload) : string.Empty;
    }

    public T? DeserializePayload<T>()
    {
        if (Payload.Length == 0) return default;
        return JsonSerializer.Deserialize<T>(Payload);
    }

    public static IpcMessage Create<T>(IpcCommandType commandType, T data)
    {
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data);
        return new IpcMessage
        {
            CommandType = commandType,
            Payload = jsonBytes
        };
    }

    public static IpcMessage Create(IpcCommandType commandType, string stringData)
    {
        return new IpcMessage
        {
            CommandType = commandType,
            Payload = string.IsNullOrEmpty(stringData) ? Array.Empty<byte>() : Encoding.UTF8.GetBytes(stringData)
        };
    }

    public static async Task WriteMessageAsync(Stream stream, IpcMessage message, CancellationToken cancellationToken = default)
    {
        var header = new byte[8];
        // Magic
        header[0] = (byte)(MagicHeader & 0xFF);
        header[1] = (byte)((MagicHeader >> 8) & 0xFF);
        // CommandType
        ushort cmd = (ushort)message.CommandType;
        header[2] = (byte)(cmd & 0xFF);
        header[3] = (byte)((cmd >> 8) & 0xFF);
        // PayloadLength
        int length = message.Payload?.Length ?? 0;
        header[4] = (byte)(length & 0xFF);
        header[5] = (byte)((length >> 8) & 0xFF);
        header[6] = (byte)((length >> 16) & 0xFF);
        header[7] = (byte)((length >> 24) & 0xFF);

        await stream.WriteAsync(header, 0, 8, cancellationToken).ConfigureAwait(false);
        if (length > 0 && message.Payload != null)
        {
            await stream.WriteAsync(message.Payload, 0, length, cancellationToken).ConfigureAwait(false);
        }
        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    public static async Task<IpcMessage?> ReadMessageAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[8];
        int read = 0;
        while (read < 8)
        {
            int r = await stream.ReadAsync(header, read, 8 - read, cancellationToken).ConfigureAwait(false);
            if (r <= 0) return null; // Stream closed
            read += r;
        }

        ushort magic = (ushort)(header[0] | (header[1] << 8));
        if (magic != MagicHeader)
        {
            return null; // Invalid frame
        }

        ushort cmd = (ushort)(header[2] | (header[3] << 8));
        int length = header[4] | (header[5] << 8) | (header[6] << 16) | (header[7] << 24);

        byte[] payload = Array.Empty<byte>();
        if (length > 0)
        {
            if (length > 64 * 1024 * 1024) return null; // Protection against oversized frames (>64MB)
            payload = new byte[length];
            int payloadRead = 0;
            while (payloadRead < length)
            {
                int r = await stream.ReadAsync(payload, payloadRead, length - payloadRead, cancellationToken).ConfigureAwait(false);
                if (r <= 0) return null;
                payloadRead += r;
            }
        }

        return new IpcMessage
        {
            CommandType = (IpcCommandType)cmd,
            Payload = payload
        };
    }
}
