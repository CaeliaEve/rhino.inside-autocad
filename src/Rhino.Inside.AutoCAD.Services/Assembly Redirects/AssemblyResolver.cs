using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Rhino.Inside.AutoCAD.Core.Interfaces;

namespace Rhino.Inside.AutoCAD.Services;

/// <summary>
/// Next-Gen High-Performance assembly resolver with O(1) hash routing and concurrent caching.
/// </summary>
public class AssemblyResolver : IAssemblyResolver
{
    private readonly IInstallationDirectories _installationDirectories;
    private readonly AppDomain _currentDomain;
    private readonly HashSet<string> _assemblyNameSet;
    private readonly ConcurrentDictionary<string, Assembly> _resolvedAssemblies = new(StringComparer.OrdinalIgnoreCase);

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

        // Automatic Probing for Rhino System assemblies (RhinoCommon, RhinoWindows, Grasshopper, etc.)
        if (shortName.StartsWith("Rhino", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("Grasshopper", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("GH_IO", StringComparison.OrdinalIgnoreCase) ||
            shortName.StartsWith("Eto", StringComparison.OrdinalIgnoreCase))
        {
            var probeDirs = new[]
            {
                @"C:\Program Files\Rhino 8\System",
                @"C:\Program Files\Rhino 7\System",
                @"C:\Program Files\Rhino 8\Plug-ins\Grasshopper",
                @"C:\Program Files\Rhino 7\Plug-ins\Grasshopper"
            };

            foreach (var dir in probeDirs)
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
                        System.Diagnostics.Debug.WriteLine($"[AssemblyResolver] Failed to load {candidate}: {ex.Message}");
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Shuts down this service.
    /// </summary>
    public void Terminate()
    {
        _currentDomain.AssemblyResolve -= this.ResolveAssembly;
    }
}