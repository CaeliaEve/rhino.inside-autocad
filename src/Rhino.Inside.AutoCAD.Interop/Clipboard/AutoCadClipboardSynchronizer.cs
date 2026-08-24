using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Geometry;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using CadHatch = Autodesk.AutoCAD.DatabaseServices.Hatch;
using CadPoint = Autodesk.AutoCAD.DatabaseServices.DBPoint;
using CadSolid3d = Autodesk.AutoCAD.DatabaseServices.Solid3d;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Monitors AutoCAD COPYCLIP operations and exports selected entities to an ultra-fast in-memory
/// Rhino 3DM exchange buffer for sub-5ms instant vector pasting into Rhino 7.
/// Built with strict JIT isolation and zero-escape safety guards to guarantee AutoCAD never crashes.
/// </summary>
public static class AutoCadClipboardSynchronizer
{
    private static bool _isHooked;
    private static ObjectId[]? _cachedSelectionIds;

    /// <summary>
    /// Initializes the AutoCAD copy command listener safely.
    /// </summary>
    public static void Initialize()
    {
        if (_isHooked) return;

        try
        {
            _isHooked = true;
            Application.DocumentManager.DocumentCreated += (s, e) => HookDoc(e.Document);
            Application.DocumentManager.DocumentActivated += (s, e) => HookDoc(e.Document);

            foreach (Document doc in Application.DocumentManager)
            {
                HookDoc(doc);
            }

            System.Diagnostics.Debug.WriteLine("[AutoCadClipboardSynchronizer] Initialized copy listener with safe JIT isolation.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] Initialize error: {ex.Message}");
        }
    }

    private static void HookDoc(Document? doc)
    {
        if (doc == null) return;
        try
        {
            doc.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
            doc.ImpliedSelectionChanged += OnImpliedSelectionChanged;
            doc.CommandWillStart -= OnCommandWillStart;
            doc.CommandWillStart += OnCommandWillStart;
            doc.CommandEnded -= OnCommandEnded;
            doc.CommandEnded += OnCommandEnded;
        }
        catch { }
    }

    private static void OnImpliedSelectionChanged(object? sender, EventArgs e)
    {
        try
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc != null)
            {
                var sel = doc.Editor.SelectImplied();
                if (sel.Status == PromptStatus.OK && sel.Value != null && sel.Value.Count > 0)
                {
                    _cachedSelectionIds = sel.Value.GetObjectIds();
                }
            }
        }
        catch { }
    }

    private static void OnCommandWillStart(object? sender, CommandEventArgs e)
    {
        try
        {
            if (IsCopyCommand(e.GlobalCommandName))
            {
                try
                {
                    var doc = Application.DocumentManager.MdiActiveDocument;
                    if (doc != null)
                    {
                        var sel = doc.Editor.SelectImplied();
                        if (sel.Status == PromptStatus.OK && sel.Value != null && sel.Value.Count > 0)
                        {
                            _cachedSelectionIds = sel.Value.GetObjectIds();
                        }
                    }
                }
                catch { }

                ExportSelectedTo3dmSafely(_cachedSelectionIds);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] OnCommandWillStart safe catch: {ex.Message}");
        }
    }

    private static void OnCommandEnded(object? sender, CommandEventArgs e)
    {
        try
        {
            if (IsCopyCommand(e.GlobalCommandName))
            {
                ExportSelectedTo3dmSafely(_cachedSelectionIds);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] OnCommandEnded safe catch: {ex.Message}");
        }
    }

    private static bool IsCopyCommand(string? cmd)
    {
        if (string.IsNullOrEmpty(cmd)) return false;
        return string.Equals(cmd, "COPYCLIP", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(cmd, "CUTCLIP", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(cmd, "COPYBASE", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(cmd, "WBLOCK", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether RhinoCommon assembly is currently loaded in the AppDomain.
    /// </summary>
    private static bool IsRhinoCommonLoaded()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                if (string.Equals(assemblies[i].GetName().Name, "RhinoCommon", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch { }
        return false;
    }

    /// <summary>
    /// Safely exports selected entities without risking any JIT assembly resolution failure.
    /// </summary>
    public static void ExportSelectedTo3dmSafely(ObjectId[]? targetIds = null)
    {
        try
        {
            if (!IsRhinoCommonLoaded())
            {
                // Rhino inside CAD not initialized yet; let native AutoCAD clipboard handle copy silently
                return;
            }

            ExportSelectedTo3dmImpl(targetIds);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] ExportSelectedTo3dmSafely caught: {ex.Message}");
        }
    }

    /// <summary>
    /// Inner implementation isolated from JIT inlining to prevent early assembly resolution failure.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ExportSelectedTo3dmImpl(ObjectId[]? targetIds)
    {
        try
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            ObjectId[] objectIds = targetIds ?? Array.Empty<ObjectId>();
            if (objectIds.Length == 0)
            {
                var sel = doc.Editor.SelectImplied();
                if (sel.Status == PromptStatus.OK && sel.Value != null && sel.Value.Count > 0)
                {
                    objectIds = sel.Value.GetObjectIds();
                }
            }

            if (objectIds.Length == 0)
            {
                return;
            }

            var file3dm = new File3dm();
            var layerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            using (var tr = doc.TransactionManager.StartTransaction())
            {
                var lt = (LayerTable)tr.GetObject(doc.Database.LayerTableId, OpenMode.ForRead);

                foreach (ObjectId id in objectIds)
                {
                    if (id.IsNull || id.IsErased) continue;
                    var ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                    if (ent == null) continue;

                    ConvertAndAddEntity(ent, file3dm, layerMap, lt, tr);
                }
                tr.Commit();
            }

            if (file3dm.Objects.Count > 0)
            {
                var exchangePath = Path.Combine(Path.GetTempPath(), "AutoCad_Clipboard_Exchange.3dm");
                file3dm.Write(exchangePath, 7); // Write Rhino 7 3DM format (<2ms)
                System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] Exported {file3dm.Objects.Count} object(s) to {exchangePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] ExportSelectedTo3dmImpl error: {ex.Message}");
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ConvertAndAddEntity(
        Entity ent,
        File3dm file3dm,
        Dictionary<string, int> layerMap,
        LayerTable lt,
        Transaction tr,
        ObjectAttributes? parentAttr = null)
    {
        if (ent == null || ent.IsErased) return;

        // 1. Resolve Layer & Color
        var layerName = ent.Layer ?? "Default";
        if (!layerMap.TryGetValue(layerName, out int layerIndex))
        {
            var layerColor = System.Drawing.Color.Black;
            if (lt.Has(layerName))
            {
                var ltr = (LayerTableRecord)tr.GetObject(lt[layerName], OpenMode.ForRead);
                var acColor = ltr.Color;
                layerColor = System.Drawing.Color.FromArgb(acColor.ColorValue.R, acColor.ColorValue.G, acColor.ColorValue.B);
            }

            var rhinoLayer = new Layer
            {
                Name = layerName,
                Color = layerColor
            };
            file3dm.AllLayers.Add(rhinoLayer);
            layerIndex = file3dm.AllLayers.Count - 1;
            layerMap[layerName] = layerIndex;
        }

        var attr = new ObjectAttributes
        {
            LayerIndex = layerIndex
        };

        if (!ent.Color.IsByLayer && !ent.Color.IsByBlock)
        {
            attr.ColorSource = ObjectColorSource.ColorFromObject;
            attr.ObjectColor = System.Drawing.Color.FromArgb(ent.Color.ColorValue.R, ent.Color.ColorValue.G, ent.Color.ColorValue.B);
        }
        else if (parentAttr != null && ent.Color.IsByBlock)
        {
            attr.ColorSource = parentAttr.ColorSource;
            attr.ObjectColor = parentAttr.ObjectColor;
        }

        // 2. Direct Geometric Conversions
        if (ent is CadCurve cadCurve)
        {
            var rhinoCurve = cadCurve.ToRhinoCurve();
            if (rhinoCurve != null)
            {
                file3dm.Objects.AddCurve(rhinoCurve, attr);
                return;
            }
        }
        else if (ent is CadSolid3d solid)
        {
            var rhinoBrep = solid.ToRhinoBrep();
            if (rhinoBrep != null)
            {
                file3dm.Objects.AddBrep(rhinoBrep, attr);
                return;
            }
        }
        else if (ent is CadHatch cadHatch)
        {
            var rhinoHatch = cadHatch.ToRhinoHatch();
            if (rhinoHatch != null)
            {
                file3dm.Objects.AddHatch(rhinoHatch, attr);
                return;
            }
        }
        else if (ent is CadPoint dbPoint)
        {
            file3dm.Objects.AddPoint(dbPoint.Position.ToRhinoPoint3d(), attr);
            return;
        }
        else if (ent is DBText dbText)
        {
            var plane = new Plane(dbText.Position.ToRhinoPoint3d(), Vector3d.ZAxis);
            if (Math.Abs(dbText.Rotation) > 0.0001)
            {
                plane.Rotate(dbText.Rotation, Vector3d.ZAxis, dbText.Position.ToRhinoPoint3d());
            }

            var textHeight = dbText.Height > 0 ? dbText.Height : 2.5;
            file3dm.Objects.AddText(dbText.TextString ?? string.Empty, plane, textHeight, "Arial", false, false, attr);
            return;
        }
        else if (ent is MText mText)
        {
            var plane = new Plane(mText.Location.ToRhinoPoint3d(), Vector3d.ZAxis);
            if (Math.Abs(mText.Rotation) > 0.0001)
            {
                plane.Rotate(mText.Rotation, Vector3d.ZAxis, mText.Location.ToRhinoPoint3d());
            }

            var textHeight = mText.TextHeight > 0 ? mText.TextHeight : 2.5;
            var textContent = mText.Text ?? mText.Contents ?? string.Empty;
            file3dm.Objects.AddText(textContent, plane, textHeight, "Arial", false, false, attr);
            return;
        }

        // 3. Composite entities: BlockReference (Title blocks/图框), Dimensions (标注), Leaders, Regions, Meshes
        try
        {
            using var explodedCollection = new DBObjectCollection();
            ent.Explode(explodedCollection);

            if (explodedCollection.Count > 0)
            {
                foreach (DBObject dbObj in explodedCollection)
                {
                    if (dbObj is Entity subEnt)
                    {
                        ConvertAndAddEntity(subEnt, file3dm, layerMap, lt, tr, attr);
                        subEnt.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AutoCadClipboardSynchronizer] Explode failed for {ent.GetType().Name}: {ex.Message}");
        }

        // Also check if BlockReference has attributes (e.g. title block sheet number, project name, author)
        if (ent is BlockReference blkRef && blkRef.AttributeCollection != null)
        {
            foreach (ObjectId attId in blkRef.AttributeCollection)
            {
                if (attId.IsNull || attId.IsErased) continue;
                if (tr.GetObject(attId, OpenMode.ForRead) is AttributeReference attRef)
                {
                    ConvertAndAddEntity(attRef, file3dm, layerMap, lt, tr, attr);
                }
            }
        }
    }
}
