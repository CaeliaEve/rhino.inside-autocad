namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Binary message types for the high-performance AutoCAD - Rhino 8 Live Link protocol.
/// </summary>
public enum IpcCommandType : ushort
{
    None = 0x0000,
    Handshake = 0x0001,
    HandshakeAck = 0x0002,
    LinkStateChanged = 0x0003,
    HeartbeatPing = 0x0004,
    HeartbeatPong = 0x0005,
    
    // Bake Pipeline
    BakeRequest = 0x0010,
    BakeAck = 0x0011,
    
    // Viewport Transient Graphics Preview
    TransientPreview = 0x0020,
    ClearPreview = 0x0021,
    
    // Bi-directional Selection & Query
    SelectInCad = 0x0030,
    QueryCadObjects = 0x0031,
    CadObjectsResult = 0x0032,
    
    // Metadata Query Pipeline (Layers, Blocks, LineTypes, Layouts)
    QueryMetadataRequest = 0x0040,
    QueryMetadataResponse = 0x0041
}
