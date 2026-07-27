using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <inheritdoc cref="IRhinoVersionDialogResult"/>
public class RhinoVersionDialogResult : IRhinoVersionDialogResult
{
    /// <inheritdoc/>
    public RhinoVersionChoice Choice { get; }

    /// <inheritdoc/>
    public IRhinoInstallation? Installation { get; }

    /// <summary>
    /// Constructs a new <see cref="RhinoVersionDialogResult"/>.
    /// </summary>
    /// <param name="choice">The choice the user made.</param>
    /// <param name="installation">The installation the user selected.</param>
    public RhinoVersionDialogResult(RhinoVersionChoice choice, IRhinoInstallation? installation)
    {
        this.Choice = choice;
        this.Installation = installation;
    }
}
