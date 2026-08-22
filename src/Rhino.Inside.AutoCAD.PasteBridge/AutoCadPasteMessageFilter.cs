using System;
using System.Linq;
using System.Windows.Forms;
using Rhino;

namespace Rhino.Inside.AutoCAD.PasteBridge;

/// <summary>
/// Intercepts Ctrl+V keystrokes in Rhino 7 to suppress the default bitmap paste
/// and safely routes vector paste execution through Rhino's official command pipeline.
/// </summary>
public class AutoCadPasteMessageFilter : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private readonly AutoCadPasteBridgePlugIn _plugIn;

    public AutoCadPasteMessageFilter(AutoCadPasteBridgePlugIn plugIn)
    {
        _plugIn = plugIn;
    }

    public bool PreFilterMessage(ref Message m)
    {
        if (m.Msg != WM_KEYDOWN && m.Msg != WM_SYSKEYDOWN)
        {
            return false;
        }

        if (!_plugIn.IsEnabled)
        {
            return false;
        }

        var key = (Keys)(m.WParam.ToInt64() & 0xFFFF);
        if (key == Keys.V && (Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Alt) == 0)
        {
            try
            {
                if (HasAutoCadClipboardData())
                {
                    // Asynchronously dispatch through Rhino's official command pipeline to avoid message loop reentrancy
                    RhinoApp.RunScript("!_PasteCad", false);

                    // Return true to consume the Ctrl+V keystroke so Rhino never triggers its native bitmap paste
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCadPasteBridge] PreFilterMessage error: {ex.Message}");
            }
        }

        return false;
    }

    /// <summary>
    /// Fast, non-blocking check to verify if the clipboard contains AutoCAD drawing data.
    /// </summary>
    public static bool HasAutoCadClipboardData()
    {
        try
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null) return false;

            var formats = dataObject.GetFormats();
            if (formats == null || formats.Length == 0) return false;

            return formats.Any(f => f.StartsWith("AutoCAD.r", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(f, "AutoCAD.Drawing", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(f, "Embed Source", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(f, "Native", StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }
}
