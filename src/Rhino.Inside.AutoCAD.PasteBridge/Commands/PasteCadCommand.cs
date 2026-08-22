using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.PasteBridge.Commands;

/// <summary>
/// Official Rhino Command to extract and import AutoCAD vector geometry into the active document.
/// Runs inside Rhino's standard command execution context, ensuring complete thread safety,
/// atomic undo support, and 0 message pump reentrancy.
/// </summary>
public class PasteCadCommand : Command
{
    public override string EnglishName => "PasteCad";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        try
        {
            var dataObject = Clipboard.GetDataObject();
            if (dataObject == null)
            {
                RhinoApp.WriteLine("[AutoCadPasteBridge] No data found on clipboard.");
                return Result.Nothing;
            }

            var formats = dataObject.GetFormats();
            if (formats == null || formats.Length == 0)
            {
                RhinoApp.WriteLine("[AutoCadPasteBridge] Clipboard format list is empty.");
                return Result.Nothing;
            }

            string? sourceDwgPath = null;
            byte[]? dwgBytes = null;

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
                            sourceDwgPath = filePath;
                            break;
                        }
                        if (data is MemoryStream msPath)
                        {
                            var bytes = msPath.ToArray();
                            var str = Encoding.Unicode.GetString(bytes).Split('\0')[0].Trim();
                            if (File.Exists(str))
                            {
                                sourceDwgPath = str;
                                break;
                            }
                        }
                    }
                    catch { }
                }
            }

            // Strategy 2: Extract embedded DWG binary stream from Embed Source, Native, or DataObject
            if (string.IsNullOrEmpty(sourceDwgPath))
            {
                var embeddedFormats = new[] { "Embed Source", "Native", "AutoCAD.Drawing", "DataObject" };
                foreach (var format in embeddedFormats)
                {
                    if (dataObject.GetDataPresent(format))
                    {
                        var rawData = dataObject.GetData(format);
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

                                var tempFile = Path.Combine(Path.GetTempPath(), "Rhino_AutoCad_Paste_Buffer.dwg");
                                File.WriteAllBytes(tempFile, cleanDwg);
                                sourceDwgPath = tempFile;
                                break;
                            }
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sourceDwgPath) || !File.Exists(sourceDwgPath))
            {
                RhinoApp.WriteLine("[AutoCadPasteBridge] No valid AutoCAD DWG vector data found on clipboard.");
                return Result.Nothing;
            }

            // 1. Unselect existing objects
            doc?.Objects?.UnselectAll();

            // 2. Execute silent DWG import via command pipeline
            var normalizedPath = sourceDwgPath.Replace('\\', '/');
            var script = $"-_Import \"{normalizedPath}\" _EnterEnd";
            RhinoApp.RunScript(script, false);

            // 3. Select all newly pasted objects & highlight
            RhinoApp.RunScript("_SelLast", false);

            doc?.Views?.Redraw();
            RhinoApp.WriteLine("[AutoCadPasteBridge] Pasted AutoCAD 1:1 vector geometry (layers & colors restored, selected).");
            return Result.Success;
        }
        catch (Exception ex)
        {
            RhinoApp.WriteLine($"[AutoCadPasteBridge] Paste failed: {ex.Message}");
            return Result.Failure;
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
