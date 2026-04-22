using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Rhino.Inside.AutoCAD.Applications;
using Rhino.Inside.AutoCAD.Civil.Interop;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Diagnostics;
using CadCurve = Autodesk.AutoCAD.DatabaseServices.Curve;
using CivilProfileView = Autodesk.Civil.DatabaseServices.ProfileView;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Rhino.Inside.AutoCAD.Interop;

/// <summary>
/// Provides extension methods for converting Civil 3D ProfileView types to Rhino geometry types.
/// </summary>
public static class CivilProfileViewExtensions
{
    /// <summary>
    /// Converts a Civil 3D ProfileView into Rhino geometry by exploding the ProfileView and converting
    /// the resulting entities into Rhino types. This method extracts all geometric components of the ProfileView.
    /// </summary>
    public static RhinoGraphAdapter ToRhinoCurves(this CivilProfileView profileView)
    {
        IAutocadTransactionManager transactionManager;

        if (profileView.Database != null)
        {
            var fingerprint = profileView.Database.FingerprintGuid;

            var document =
                RhinoInsideAutoCadExtension.Application.RhinoInsideManager.AutoCadInstance
                    .FindDocumentByFingerprintGuid(fingerprint);

            if (document == null)
            {
                return new RhinoGraphAdapter(null, null);
            }

            transactionManager = document.CreateTransactionManager();
        }
        else
        {
            var activeDocument = Application.DocumentManager.MdiActiveDocument;

            if (activeDocument == null)
            {
                return new RhinoGraphAdapter(null, null);
            }

            transactionManager = new AutocadTransactionManagerWrapper(activeDocument);
        }

        var profileViewGeometry = transactionManager.PerformTask(() =>
        {
            var result = new ProfileViewGeometry();

            using var explodedObjects = new DBObjectCollection();

            profileView.Explode(explodedObjects);

            foreach (DBObject obj in explodedObjects)
            {
                ProcessExplodedObject(obj, result);
            }

            return result;

        }, true);

        return new RhinoGraphAdapter(profileViewGeometry.Curves, profileViewGeometry.TextEntities);
    }

    /// <summary>
    /// Recursively processes an exploded object and extracts geometry into the result.
    /// Handles nested BlockReferences by exploding them and processing their contents.
    /// </summary>
    private static void ProcessExplodedObject(DBObject obj, ProfileViewGeometry result)
    {
        try
        {
            switch (obj)
            {
                case BlockReference blockRef:
                    using (var nestedObjects = new DBObjectCollection())
                    {
                        blockRef.Explode(nestedObjects);

                        foreach (DBObject nestedObj in nestedObjects)
                        {
                            ProcessExplodedObject(nestedObj, result);
                        }
                    }
                    break;

                case CadCurve acadCurve:
                    var rhinoCurve = acadCurve.ToRhinoCurve();

                    if (rhinoCurve != null)
                        result.Curves.Add(rhinoCurve);

                    break;

                case DBText dbText:
                    var meText = dbText.ConvertToMText();

                    var rhinoText = meText.ToRhinoTextEntity();

                    result.TextEntities.Add(rhinoText);

                    break;

                case MText mText:
                    var rhinoMText = mText.ToRhinoTextEntity();

                    result.TextEntities.Add(rhinoMText);

                    break;
                default:
                    Debug.WriteLine(obj.GetType().Name);
                    break;
            }
        }
        finally
        {
            obj.Dispose();
        }
    }
}
