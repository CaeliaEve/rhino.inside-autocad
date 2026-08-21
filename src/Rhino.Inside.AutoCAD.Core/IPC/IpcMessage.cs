using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Lightweight binary/JSON RPC frame payload.
/// </summary>
public class IpcMessage
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("cmd")]
    public IpcCommandType Command { get; set; }

    [JsonPropertyName("payload")]
    public string? Payload { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; } = true;

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static IpcMessage Create(IpcCommandType command, string? payload = null)
    {
        return new IpcMessage
        {
            Command = command,
            Payload = payload
        };
    }

    public static IpcMessage Ok(string id, string? payload = null)
    {
        return new IpcMessage
        {
            Id = id,
            Success = true,
            Payload = payload
        };
    }

    public static IpcMessage Fail(string id, string error)
    {
        return new IpcMessage
        {
            Id = id,
            Success = false,
            Error = error
        };
    }

    public string ToJson() => JsonSerializer.Serialize(this);

    public static IpcMessage? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<IpcMessage>(json);
        }
        catch
        {
            return null;
        }
    }
}
