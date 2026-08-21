using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Rhino;

namespace Rhino.Inside.AutoCAD.PasteBridge;

/// <summary>
/// Intercepts Ctrl+V keystrokes in Rhino 7 to replace the default bitmap paste
/// with true 1:1 AutoCAD vector DWG geometry import.
/// </summary>
public class AutoCadPasteMessageFilter : IMessageFilter
{
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    private readonly AutoCadPasteBridgePlugIn _plugIn;
    private bool _isProcessing;

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

        if (!_plugIn.IsEnabled || _isProcessing)
        {
            return false;
        }

        var key = (Keys)(m.WParam.ToInt64() & 0xFFFF);
        if (key == Keys.V && (Control.ModifierKeys & Keys.Control) == Keys.Control && (Control.ModifierKeys & Keys.Alt) == 0)
        {
            try
            {
                _isProcessing = true;
                if (TryPasteAutoCadVector())
                {
                    // Consumed! Suppress Rhino's native Paste command so no bitmap is pasted
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCadPasteBridge] Error in PreFilterMessage: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks the clipboard and imports AutoCAD vector geometry into the active Rhino document.
    /// </summary>
    public static bool TryPasteAutoCadVector()
    {
        try
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null) return false;

            var formats = dataObject.GetFormats();
            if (formats == null || formats.Length == 0) return false;

            // Strategy 1: Check for AutoCAD.rXX / AutoCAD file path in clipboard
            foreach (var format in formats)
            {
                if (format.StartsWith("AutoCAD.r", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(format, "AutoCAD.Drawing", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        var data = dataObject.GetData(format);
                        if (data is string filePath && File.Exists(filePath))
                        {
                            return ImportDwgFile(filePath);
                        }
                        if (data is MemoryStream msPath)
                        {
                            var bytes = msPath.ToArray();
                            var str = Encoding.Unicode.GetString(bytes).Split('\0')[0].Trim();
                            if (File.Exists(str))
                            {
                                return ImportDwgFile(str);
                            }
                        }
                    }
                    catch { }
                }
            }

            // Strategy 2: Extract embedded DWG binary stream from Embed Source or Native
            var embeddedFormats = new[] { "Embed Source", "Native", "AutoCAD.Drawing", "DataObject" };
            foreach (var format in embeddedFormats)
            {
                if (dataObject.GetDataPresent(format))
                {
                    var rawData = dataObject.GetData(format);
                    byte[]? dwgBytes = null;

                    if (rawData is MemoryStream ms)
                    {
                        dwgBytes = ms.ToArray();
                    }
                    else if (rawData is byte[] bytes)
                    {
                        dwgBytes = bytes;
                    }
                    else if (rawData is Stream s)
                    {
                        using var mem = new MemoryStream();
                        s.CopyTo(mem);
                        dwgBytes = mem.ToArray();
                    }

                    if (dwgBytes != null && dwgBytes.Length > 32)
                    {
                        var offset = FindDwgHeader(dwgBytes);
                        if (offset >= 0)
                        {
                            var cleanDwg = new byte[dwgBytes.Length - offset];
                            Array.Copy(dwgBytes, offset, cleanDwg, 0, cleanDwg.Length);

                            var tempFile = Path.Combine(Path.GetTempPath(), $"AutoCadPaste_{Guid.NewGuid():N}.dwg");
                            File.WriteAllBytes(tempFile, cleanDwg);

                            try
                            {
                                return ImportDwgFile(tempFile);
                            }
                            finally
                            {
                                try { File.Delete(tempFile); } catch { }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadPasteBridge] Paste failed: {ex.Message}");
        }

        return false;
    }

    private static bool ImportDwgFile(string filePath)
    {
        try
        {
            var normalizedPath = filePath.Replace('\\', '/');
            var script = $"-_Import \"{normalizedPath}\" _Enter";

            // Run DWG import silently into active Rhino document
            RhinoApp.RunScript(script, false);
            RhinoApp.WriteLine("[AutoCadPasteBridge] Pasted AutoCAD 1:1 vector geometry (layers & colors restored).");
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadPasteBridge] ImportDwgFile failed: {ex.Message}");
            return false;
        }
    }

    private static int FindDwgHeader(byte[] data)
    {
        for (int i = 0; i <= data.Length - 6; i++)
        {
            if (data[i] == 'A' && data[i + 1] == 'C' && data[i + 2] == '1' && data[i + 3] == '0')
            {
                return i;
            }
        }
        return -1;
    }
}
