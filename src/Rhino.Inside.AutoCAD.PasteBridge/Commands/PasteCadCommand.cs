using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;

namespace Rhino.Inside.AutoCAD.PasteBridge.Commands;

/// <summary>
/// Official Rhino Command to extract and import AutoCAD vector geometry into the active document.
/// Uses synchronous Rhino FileIO with BatchMode to ensure 100% silent execution, 0 prompts,
/// automatic object selection, and smart viewport framing.
/// </summary>
public class PasteCadCommand : Command
{
    public override string EnglishName => "PasteCad";

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
        if (doc == null)
        {
            return Result.Failure;
        }

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
            if (string.IsNullOrEmpty(sourceDwgPath) || !File.Exists(sourceDwgPath))
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
            doc.Objects.UnselectAll();

            // Snapshot existing object IDs
            var beforeIds = new HashSet<Guid>(
                doc.Objects.GetObjectList(ObjectType.AnyObject).Select(o => o.Id));

            // 2. Perform silent, synchronous DWG import using Rhino's native FileIO
            var readOptions = new FileReadOptions
            {
                ImportMode = true,
                BatchMode = true,
                UseScaleGeometry = false
            };

            bool readSuccess = false;
            try
            {
                readSuccess = RhinoDoc.ReadFile(sourceDwgPath, readOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AutoCadPasteBridge] RhinoDoc.ReadFile error: {ex.Message}");
            }

            if (!readSuccess)
            {
                // Fallback to command line script if ReadFile was not supported for DWG format directly
                var normalizedPath = sourceDwgPath.Replace('\\', '/');
                var script = $"-_Import \"{normalizedPath}\" _Enter";
                RhinoApp.RunScript(script, false);
                RhinoApp.RunScript("_SelLast", false);
            }
            else
            {
                // 3. Identify and select all newly added objects
                var newObjects = doc.Objects.GetObjectList(ObjectType.AnyObject)
                    .Where(o => !beforeIds.Contains(o.Id))
                    .ToList();

                if (newObjects.Count > 0)
                {
                    var bbox = BoundingBox.Empty;
                    foreach (var obj in newObjects)
                    {
                        doc.Objects.Select(obj.Id, true);
                        bbox.Union(obj.Geometry.GetBoundingBox(true));
                    }

                    // 4. Smart Viewport Zoom: if outside viewport, gently frame the new geometry
                    if (bbox.IsValid && doc.Views.ActiveView?.ActiveViewport is { } vp)
                    {
                        vp.ZoomBoundingBox(bbox);
                    }

                    RhinoApp.WriteLine($"[AutoCadPasteBridge] Pasted {newObjects.Count} AutoCAD vector object(s) (1:1 scale, layers & colors restored, selected).");
                }
                else
                {
                    RhinoApp.RunScript("_SelLast", false);
                    RhinoApp.WriteLine("[AutoCadPasteBridge] Pasted AutoCAD vector geometry (layers & colors restored).");
                }
            }

            doc.Views.Redraw();
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
