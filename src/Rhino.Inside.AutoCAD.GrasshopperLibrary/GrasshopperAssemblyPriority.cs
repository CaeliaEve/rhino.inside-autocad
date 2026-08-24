using System;
using System.IO;
using System.Reflection;
using Grasshopper.Kernel;
using Microsoft.Win32;
using Rhino.Inside.AutoCAD.Core.IPC;

namespace Rhino.Inside.AutoCAD.GrasshopperLibrary;

/// <summary>
/// Executed by Grasshopper before loading any components in this GHA.
/// Dynamically resolves AutoCAD dependencies for standalone Rhino 8 and initializes Live Link IPC.
/// </summary>
public class GrasshopperAssemblyPriority : GH_AssemblyPriority
{
    private static string? _autocadInstallDir;

    public override GH_LoadingInstruction PriorityLoad()
    {
        // Hook dynamic assembly resolver for AutoCAD core assemblies
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        // Pre-warm Live Link connection
        try
        {
            _ = LiveLinkClient.Instance.EnsureConnectedAsync(500);
        }
        catch { }

        return GH_LoadingInstruction.Proceed;
    }

    private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        if (string.IsNullOrEmpty(args.Name)) return null;

        var assemblyName = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(assemblyName)) return null;

        // Check if it's an AutoCAD assembly (accoremgd, acdbmgd, acmgd, AcdbmgdBrep, etc.)
        if (assemblyName.StartsWith("ac", StringComparison.OrdinalIgnoreCase) ||
            assemblyName.StartsWith("Autodesk.", StringComparison.OrdinalIgnoreCase))
        {
            var cadDir = GetAutoCadLocation();
            if (!string.IsNullOrEmpty(cadDir))
            {
                var candidatePath = Path.Combine(cadDir, $"{assemblyName}.dll");
                if (File.Exists(candidatePath))
                {
                    try
                    {
                        return Assembly.LoadFrom(candidatePath);
                    }
                    catch { }
                }
            }
        }

        return null;
    }

    private static string? GetAutoCadLocation()
    {
        if (_autocadInstallDir != null) return _autocadInstallDir;

        try
        {
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using var autocadKey = baseKey.OpenSubKey(@"SOFTWARE\Autodesk\AutoCAD");
            if (autocadKey != null)
            {
                foreach (var releaseName in autocadKey.GetSubKeyNames())
                {
                    using var releaseKey = autocadKey.OpenSubKey(releaseName);
                    if (releaseKey != null)
                    {
                        foreach (var langName in releaseKey.GetSubKeyNames())
                        {
                            using var langKey = releaseKey.OpenSubKey(langName);
                            if (langKey?.GetValue("Location") is string loc && Directory.Exists(loc))
                            {
                                return _autocadInstallDir = loc;
                            }
                            if (langKey?.GetValue("AcadLocation") is string acadLoc && Directory.Exists(acadLoc))
                            {
                                return _autocadInstallDir = acadLoc;
                            }
                        }
                    }
                }
            }
        }
        catch { }

        return null;
    }
}
