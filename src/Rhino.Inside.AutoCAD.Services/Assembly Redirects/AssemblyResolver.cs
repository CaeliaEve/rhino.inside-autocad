using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Win32;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <summary>
/// Next-Gen High-Performance assembly resolver with O(1) hash routing, dynamic Rhino registry discovery, and concurrent caching.
/// </summary>
public class AssemblyResolver : IAssemblyResolver
{
    private readonly IInstallationDirectories _installationDirectories;
    private readonly AppDomain _currentDomain;
    private readonly HashSet<string> _assemblyNameSet;
    private static readonly ConcurrentDictionary<string, Assembly> _resolvedAssemblies = new(StringComparer.OrdinalIgnoreCase);
    private static readonly List<string> _rhinoProbeDirectories = new();
    private static bool _probeDirsInitialized = false;
    private static readonly object _initLock = new();

    static AssemblyResolver()
    {
        InitializeProbeDirectories();
        AppDomain.CurrentDomain.AssemblyResolve += StaticResolveAssembly;
    }

    private static void InitializeProbeDirectories()
    {
        if (_probeDirsInitialized) return;
        lock (_initLock)
        {
            if (_probeDirsInitialized) return;

            try
            {
                using var rhinoKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\McNeel\Rhinoceros");
                if (rhinoKey != null)
                {
                    foreach (var subKeyName in rhinoKey.GetSubKeyNames())
                    {
                        try
                        {
                            using var installKey = rhinoKey.OpenSubKey($@"{subKeyName}\Install");
                            if (installKey != null)
                            {
                                var pathVal = installKey.GetValue("Path") as string;
                                if (!string.IsNullOrEmpty(pathVal) && Directory.Exists(pathVal))
                                {
                                    AddProbePath(pathVal);
                                    var parent = Path.GetDirectoryName(pathVal.TrimEnd('\\', '/'));
                                    if (!string.IsNullOrEmpty(parent))
                                    {
                                        AddProbePath(Path.Combine(parent, "Plug-ins", "Grasshopper"));
                                    }
                                }

                                var installPathVal = installKey.GetValue("InstallPath") as string;
                                if (!string.IsNullOrEmpty(installPathVal) && Directory.Exists(installPathVal))
                                {
                                    AddProbePath(Path.Combine(installPathVal, "System"));
                                    AddProbePath(Path.Combine(installPathVal, "Plug-ins", "Grasshopper"));
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
            catch { }

            // Well-known fallback paths (standard & common custom installs)
            AddProbePath(@"D:\software\Rhino8\System");
            AddProbePath(@"D:\software\Rhino8\Plug-ins\Grasshopper");
            AddProbePath(@"D:\software\Rhino 7\System");
            AddProbePath(@"D:\software\Rhino 7\Plug-ins\Grasshopper");
            AddProbePath(@"C:\Program Files\Rhino 8\System");
            AddProbePath(@"C:\Program Files\Rhino 8\Plug-ins\Grasshopper");
            AddProbePath(@"C:\Program Files\Rhino 7\System");
            AddProbePath(@"C:\Program Files\Rhino 7\Plug-ins\Grasshopper");

            _probeDirsInitialized = true;
        }
    }

    private static void AddProbePath(string path)
    {
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path) && !_rhinoProbeDirectories.Contains(path))
        {
            _rhinoProbeDirectories.Add(path);
        }
    }

    private static Assembly? StaticResolveAssembly(object? sender, ResolveEventArgs args)
    {
        if (string.IsNullOrEmpty(args.Name)) return null;

        var shortName = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(shortName)) return null;

        if (_resolvedAssemblies.TryGetValue(shortName, out var cachedAssembly))
            return cachedAssembly;

        if (shortName.StartsWith("Rhino", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("Grasshopper", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("GH_IO", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("Eto", StringComparison.OrdinalIgnoreCase))
        {
            InitializeProbeDirectories();

            foreach (var dir in _rhinoProbeDirectories)
            {
                var candidate = Path.Combine(dir, $"{shortName}.dll");
                if (File.Exists(candidate))
                {
                    try
                    {
                        var loaded = Assembly.LoadFrom(candidate);
                        _resolvedAssemblies[shortName] = loaded;
                        return loaded;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AssemblyResolver] Static load failed for {candidate}: {ex.Message}");
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Constructs a new <see cref="AssemblyResolver"/>.
    /// </summary>
    public AssemblyResolver(
        IInstallationDirectories installationDirectories,
        IAssemblyRedirectsSet assemblyRedirectsSet)
    {
        _installationDirectories = installationDirectories;
        _currentDomain = AppDomain.CurrentDomain;
        _assemblyNameSet = new HashSet<string>(assemblyRedirectsSet, StringComparer.OrdinalIgnoreCase);

        // Pre-register MaterialDesign assembly names into hash set for fast on-demand resolution
        foreach (var name in ApplicationConstants.MaterialDesignAssemblyNames)
        {
            var shortName = Path.GetFileNameWithoutExtension(name);
            _assemblyNameSet.Add(shortName);
        }

        _currentDomain.AssemblyResolve += this.ResolveAssembly;

        // Asynchronously pre-warm MaterialDesign assemblies in background without blocking startup
        Task.Run(() => this.PreWarmMaterialDesign(installationDirectories));
    }

    private void PreWarmMaterialDesign(IInstallationDirectories installationDirectories)
    {
        try
        {
            foreach (var name in ApplicationConstants.MaterialDesignAssemblyNames)
            {
                var assemblyPath = Path.Combine(installationDirectories.VersionedAssemblies, name);
                if (File.Exists(assemblyPath))
                {
                    var shortName = Path.GetFileNameWithoutExtension(name);
                    _resolvedAssemblies.GetOrAdd(shortName, _ => Assembly.LoadFrom(assemblyPath));
                }
            }
        }
        catch { }
    }

    /// <summary>
    /// High-speed O(1) assembly resolve event handler.
    /// </summary>
    private Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        if (string.IsNullOrEmpty(args.Name)) return null;

        var shortName = new AssemblyName(args.Name).Name;
        if (string.IsNullOrEmpty(shortName)) return null;

        if (_resolvedAssemblies.TryGetValue(shortName, out var cachedAssembly))
            return cachedAssembly;

        if (_assemblyNameSet.Contains(shortName))
        {
            var assemblyPath = Path.Combine(_installationDirectories.VersionedAssemblies, $"{shortName}.dll");
            if (File.Exists(assemblyPath))
            {
                var loaded = Assembly.LoadFrom(assemblyPath);
                _resolvedAssemblies[shortName] = loaded;
                return loaded;
            }
        }

        return StaticResolveAssembly(sender, args);
    }

    /// <summary>
    /// Shuts down this service.
    /// </summary>
    public void Terminate()
    {
        _currentDomain.AssemblyResolve -= this.ResolveAssembly;
    }
}