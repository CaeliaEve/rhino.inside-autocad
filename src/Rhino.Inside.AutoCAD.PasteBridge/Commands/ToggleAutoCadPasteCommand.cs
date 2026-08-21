using System;
using Rhino;
using Rhino.Commands;

namespace Rhino.Inside.AutoCAD.PasteBridge.Commands;

/// <summary>
/// Rhino Command to toggle the AutoCAD vector clipboard bridge on or off.
/// </summary>
public class ToggleAutoCadPasteCommand : Command
{
    public override string EnglishName => "ToggleAutoCadPaste";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        if (AutoCadPasteBridgePlugIn.Instance is { } plugIn)
        {
            plugIn.IsEnabled = !plugIn.IsEnabled;
            var status = plugIn.IsEnabled ? "ENABLED (1:1 vector copy-paste active)" : "DISABLED (Default Rhino paste active)";
            RhinoApp.WriteLine($"[AutoCadPasteBridge] AutoCAD vector paste bridge: {status}");
            return Result.Success;
        }

        RhinoApp.WriteLine("[AutoCadPasteBridge] Plugin instance not found.");
        return Result.Failure;
    }
}
