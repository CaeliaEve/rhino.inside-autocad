namespace Rhino.Inside.AutoCAD.Core.IPC;

/// <summary>
/// Defines the high-performance RPC command types transmitted between AutoCAD Host and Rhino Worker processes.
/// </summary>
public enum IpcCommandType
{
    Unknown = 0,
    Ping = 1,
    Pong = 2,
    GetStatus = 3,
    LaunchRhino = 10,
    LaunchGrasshopper = 11,
    OpenViewport = 12,
    ToggleRhinoPreview = 20,
    SetPreviewMode = 21,
    RecomputeSolution = 22,
    ToggleSolver = 23,
    Bake = 30,
    ConvertBrep = 31,
    Suspend = 40,
    Resume = 41,
    Shutdown = 42,
    SwitchVersion = 50
}
