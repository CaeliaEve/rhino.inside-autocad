using System;
using Rhino;
using Rhino.Commands;

namespace Rhino.Inside.AutoCAD.PasteBridge.Commands;

/// <summary>
/// Explicit Rhino Command to paste AutoCAD vector geometry from the clipboard.
/// </summary>
public class PasteCadCommand : Command
{
    public override string EnglishName => "PasteCad";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        if (AutoCadPasteMessageFilter.TryPasteAutoCadVector())
        {
            return Result.Success;
        }

        RhinoApp.WriteLine("[AutoCadPasteBridge] No AutoCAD vector data found on clipboard.");
        return Result.Nothing;
    }
}
