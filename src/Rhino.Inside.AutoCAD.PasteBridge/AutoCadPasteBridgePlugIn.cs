using System.Runtime.InteropServices;
using System.Windows.Forms;
using Rhino;
using Rhino.PlugIns;

namespace Rhino.Inside.AutoCAD.PasteBridge;

/// <summary>
/// Independent Rhino 7 PlugIn that enables seamless 1:1 vector copy-paste from AutoCAD into Rhino 7.
/// </summary>
[Guid("E5A2A388-99B4-4B2D-9BC8-4664F98E18F3")]
public class AutoCadPasteBridgePlugIn : PlugIn
{
    private AutoCadPasteMessageFilter? _messageFilter;

    /// <summary>
    /// Gets the singleton instance of the <see cref="AutoCadPasteBridgePlugIn"/>.
    /// </summary>
    public static AutoCadPasteBridgePlugIn? Instance { get; private set; }

    /// <summary>
    /// Gets or sets whether AutoCAD vector clipboard interception is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoCadPasteBridgePlugIn"/> class.
    /// </summary>
    public AutoCadPasteBridgePlugIn()
    {
        Instance = this;
    }

    /// <inheritdoc />
    public override PlugInLoadTime LoadTime => PlugInLoadTime.AtStartup;

    /// <inheritdoc />
    protected override LoadReturnCode OnLoad(ref string errorMessage)
    {
        try
        {
            _messageFilter = new AutoCadPasteMessageFilter(this);
            Application.AddMessageFilter(_messageFilter);

            RhinoApp.WriteLine("[AutoCadPasteBridge] AutoCAD -> Rhino 7 Vector Copy-Paste Bridge loaded (1:1 vector scale active).");
            return LoadReturnCode.Success;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return LoadReturnCode.ErrorShowDialog;
        }
    }

    /// <inheritdoc />
    protected override void OnShutdown()
    {
        try
        {
            if (_messageFilter != null)
            {
                Application.RemoveMessageFilter(_messageFilter);
                _messageFilter = null;
            }
        }
        catch { }

        base.OnShutdown();
    }
}
