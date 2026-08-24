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

    public static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;

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
                        ExecuteBake(payload);
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

    private static void ClearTransientPreview()
    {
        // Safe placeholder for transient graphics clear
    }
}
