using System;
using System.Collections.Generic;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Inside.AutoCAD.Core.IPC;
using Rhino.Inside.AutoCAD.Interop;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Rhino.Inside.AutoCAD.Applications.IPC;

/// <summary>
/// Handles incoming Live Link IPC messages on the AutoCAD host side.
/// Executes transactional Baking and Transient Graphics rendering in AutoCAD.
/// </summary>
public static class LiveLinkServerHandler
{
    private static bool _isInitialized;
    private static System.Threading.SynchronizationContext? _uiContext;

    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        _uiContext = System.Threading.SynchronizationContext.Current;

        LiveLinkManager.Instance.MessageReceived += OnMessageReceived;
    }

    private static void OnMessageReceived(IpcMessage msg)
    {
        try
        {
            switch (msg.CommandType)
            {
                case IpcCommandType.BakeRequest:
                    var payload = msg.DeserializePayload<BakePayload>();
                    if (payload != null)
                    {
                        if (_uiContext != null)
                            _uiContext.Post(_ => ExecuteBake(payload), null);
                        else
                            ExecuteBake(payload);
                    }
                    break;

                case IpcCommandType.SelectInCad:
                    var selReq = msg.DeserializePayload<SelectRequestPayload>();
                    if (selReq != null)
                    {
                        if (_uiContext != null)
                            _uiContext.Post(_ => ExecuteSelectInCad(selReq), null);
                        else
                            ExecuteSelectInCad(selReq);
                    }
                    break;

                case IpcCommandType.QueryMetadataRequest:
                    var metaReq = msg.DeserializePayload<MetadataQueryRequest>();
                    if (metaReq != null)
                    {
                        if (_uiContext != null)
                            _uiContext.Post(_ => ExecuteQueryMetadata(metaReq), null);
                        else
                            ExecuteQueryMetadata(metaReq);
                    }
                    break;

                case IpcCommandType.ClearPreview:
                    ClearTransientPreview();
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LiveLinkServerHandler] OnMessageReceived error: {ex.Message}");
        }
    }

    private static void ExecuteBake(BakePayload payload)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null) return;

        try
        {
            using (doc.LockDocument())
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                var db = doc.Database;
                var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                // Ensure target layer exists
                var layerName = string.IsNullOrWhiteSpace(payload.TargetLayer) ? "0" : payload.TargetLayer;
                if (!lt.Has(layerName))
                {
                    lt.UpgradeOpen();
                    var ltr = new LayerTableRecord { Name = layerName };
                    if (payload.ColorRgb >= 0)
                    {
                        var r = (byte)((payload.ColorRgb >> 16) & 0xFF);
                        var g = (byte)((payload.ColorRgb >> 8) & 0xFF);
                        var b = (byte)(payload.ColorRgb & 0xFF);
                        ltr.Color = Color.FromRgb(r, g, b);
                    }
                    lt.Add(ltr);
                    tr.AddNewlyCreatedDBObject(ltr, true);
                }

                var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                var btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                int bakedCount = 0;
                if (payload.Geometry3dmBytes != null && payload.Geometry3dmBytes.Length > 0)
                {
                    var file3dm = File3dm.FromByteArray(payload.Geometry3dmBytes);
                    if (file3dm != null)
                    {
                        foreach (var obj in file3dm.Objects)
                        {
                            var geom = obj.Geometry;
                            if (geom == null) continue;

                            Entity? cadEnt = null;

                            if (geom is Rhino.Geometry.Curve crv)
                            {
                                cadEnt = ConvertRhinoCurveToCadEntity(crv);
                            }
                            else if (geom is Rhino.Geometry.Mesh mesh)
                            {
                                cadEnt = mesh.ToAutocadSubDMesh();
                            }
                            else if (geom is Rhino.Geometry.Point pt)
                            {
                                cadEnt = new DBPoint(pt.Location.ToAutocadPoint3d());
                            }

                            if (cadEnt != null)
                            {
                                cadEnt.Layer = layerName;
                                if (payload.ColorRgb >= 0)
                                {
                                    var r = (byte)((payload.ColorRgb >> 16) & 0xFF);
                                    var g = (byte)((payload.ColorRgb >> 8) & 0xFF);
                                    var b = (byte)(payload.ColorRgb & 0xFF);
                                    cadEnt.Color = Color.FromRgb(r, g, b);
                                }

                                btr.AppendEntity(cadEnt);
                                tr.AddNewlyCreatedDBObject(cadEnt, true);
                                bakedCount++;
                            }
                        }
                    }
                }

                tr.Commit();
                doc.Editor.WriteMessage($"\n[Rhino Live Link] Successfully baked {bakedCount} object(s) to layer '{layerName}'.\n");
                doc.Editor.UpdateScreen();
            }
        }
        catch (Exception ex)
        {
            doc.Editor.WriteMessage($"\n[Rhino Live Link] Bake error: {ex.Message}\n");
        }
    }

    private static Entity? ConvertRhinoCurveToCadEntity(Rhino.Geometry.Curve crv)
    {
        if (crv is Rhino.Geometry.LineCurve lc)
            return lc.ToAutocadLine();
        if (crv is Rhino.Geometry.ArcCurve ac && ac.IsArc())
            return ac.Arc.ToAutocadArc();
        if (crv is Rhino.Geometry.PolylineCurve plc)
            return plc.ToAutocadPolyline3d();

        var nurbs = crv.ToNurbsCurve();
        return nurbs != null ? nurbs.ToAutocadSpline() : null;
    }

    private static void ExecuteSelectInCad(SelectRequestPayload req)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
        {
            var emptyResp = IpcMessage.Create(IpcCommandType.CadObjectsResult, new SelectResponsePayload { Success = false });
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(emptyResp));
            return;
        }

        try
        {
            var cadHandle = Application.MainWindow?.Handle ?? IntPtr.Zero;
            if (cadHandle != IntPtr.Zero)
            {
                Rhino.Inside.AutoCAD.Core.UI.WindowHelper.BringToFront(cadHandle);
            }

            var resp = new SelectResponsePayload { Success = true };

            // Interactive Selection must be performed outside of transaction/lock
            var opt = new Autodesk.AutoCAD.EditorInput.PromptSelectionOptions
            {
                SingleOnly = req.SingleOnly,
                MessageForAdding = "\n" + (string.IsNullOrWhiteSpace(req.PromptMessage) ? "Select AutoCAD Object: " : req.PromptMessage + ": ")
            };

            var res = doc.Editor.GetSelection(opt);
            if (res.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK && res.Value != null)
            {
                using (doc.LockDocument())
                using (var tr = doc.TransactionManager.StartTransaction())
                {
                    foreach (Autodesk.AutoCAD.EditorInput.SelectedObject selObj in res.Value)
                    {
                        if (selObj == null) continue;
                        var ent = tr.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                        if (ent == null) continue;

                        var dto = new SelectedObjectDto
                        {
                            Handle = ent.Handle.ToString(),
                            Layer = ent.Layer,
                            ColorRgb = ent.Color.IsByLayer ? -1 : (ent.Color.ColorValue.R << 16 | ent.Color.ColorValue.G << 8 | ent.Color.ColorValue.B),
                            ObjectType = ent.GetType().Name
                        };

                        var file3dm = new File3dm();
                        if (ent is Curve cadCrv)
                        {
                            var rCrv = cadCrv.ToRhinoCurve();
                            if (rCrv != null) file3dm.Objects.AddCurve(rCrv);
                        }
                        else if (ent is Solid3d solid)
                        {
                            var rBrep = solid.ToRhinoBrep();
                            if (rBrep != null) file3dm.Objects.AddBrep(rBrep);
                        }
                        else if (ent is DBPoint pt)
                        {
                            file3dm.Objects.AddPoint(pt.Position.ToRhinoPoint3d());
                        }

                        dto.Geometry3dmBytes = file3dm.ToByteArray(new File3dmWriteOptions { Version = 7 });
                        resp.Objects.Add(dto);
                    }
                    tr.Commit();
                }
            }
            else
            {
                resp.Success = false;
            }

            var respMsg = IpcMessage.Create(IpcCommandType.CadObjectsResult, resp);
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(respMsg));

            // Return focus to Rhino/Grasshopper after selection
            Rhino.Inside.AutoCAD.Core.UI.WindowHelper.ActivateRhino();
        }
        catch (Exception ex)
        {
            doc.Editor.WriteMessage($"\n[Rhino Live Link] Selection error: {ex.Message}\n");
            var failResp = IpcMessage.Create(IpcCommandType.CadObjectsResult, new SelectResponsePayload { Success = false });
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(failResp));
        }
    }

    private static void ExecuteQueryMetadata(MetadataQueryRequest req)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc == null)
        {
            var emptyResp = IpcMessage.Create(IpcCommandType.QueryMetadataResponse, new MetadataQueryResponse { Success = false, ErrorMessage = "No active document" });
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(emptyResp));
            return;
        }

        try
        {
            var resp = new MetadataQueryResponse { Success = true };
            using (doc.LockDocument())
            using (var tr = doc.TransactionManager.StartTransaction())
            {
                var db = doc.Database;

                if (req.QueryType == MetadataQueryType.Layers)
                {
                    var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                    foreach (ObjectId id in lt)
                    {
                        var ltr = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        var rgb = ltr.Color.IsByLayer ? -1 : (ltr.Color.ColorValue.R << 16 | ltr.Color.ColorValue.G << 8 | ltr.Color.ColorValue.B);
                        resp.Layers.Add(new LayerInfoDto
                        {
                            Name = ltr.Name,
                            ColorRgb = rgb,
                            IsOff = ltr.IsOff,
                            IsFrozen = ltr.IsFrozen,
                            IsLocked = ltr.IsLocked,
                            Handle = ltr.Handle.ToString()
                        });
                    }
                }
                else if (req.QueryType == MetadataQueryType.Blocks)
                {
                    var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    foreach (ObjectId id in bt)
                    {
                        var btr = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        resp.Blocks.Add(new BlockInfoDto
                        {
                            Name = btr.Name,
                            Handle = btr.Handle.ToString(),
                            IsAnonymous = btr.IsAnonymous,
                            IsLayout = btr.IsLayout,
                            IsDynamicBlock = btr.IsDynamicBlock
                        });
                    }
                }
                else if (req.QueryType == MetadataQueryType.LineTypes)
                {
                    var ltt = (LinetypeTable)tr.GetObject(db.LinetypeTableId, OpenMode.ForRead);
                    foreach (ObjectId id in ltt)
                    {
                        var ltr = (LinetypeTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        resp.LineTypes.Add(new LineTypeInfoDto
                        {
                            Name = ltr.Name,
                            Description = ltr.Comments,
                            Handle = ltr.Handle.ToString()
                        });
                    }
                }
                else if (req.QueryType == MetadataQueryType.Layouts)
                {
                    var layoutDict = (DBDictionary)tr.GetObject(db.LayoutDictionaryId, OpenMode.ForRead);
                    foreach (DBDictionaryEntry entry in layoutDict)
                    {
                        var layout = (Layout)tr.GetObject(entry.Value, OpenMode.ForRead);
                        resp.Layouts.Add(new LayoutInfoDto
                        {
                            Name = layout.LayoutName,
                            TabOrder = layout.TabOrder,
                            Handle = layout.Handle.ToString()
                        });
                    }
                }

                tr.Commit();
            }

            var respMsg = IpcMessage.Create(IpcCommandType.QueryMetadataResponse, resp);
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(respMsg));
        }
        catch (Exception ex)
        {
            var errResp = IpcMessage.Create(IpcCommandType.QueryMetadataResponse, new MetadataQueryResponse { Success = false, ErrorMessage = ex.Message });
            System.Threading.Tasks.Task.Run(() => LiveLinkManager.Instance.SendMessageAsync(errResp));
        }
    }

    private static void ClearTransientPreview()
    {
        // Safe placeholder for transient graphics clear
    }
}
