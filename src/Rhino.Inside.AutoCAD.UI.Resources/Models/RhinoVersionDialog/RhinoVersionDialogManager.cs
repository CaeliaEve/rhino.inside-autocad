using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.ViewModels;
using Rhino.Inside.AutoCAD.UI.Resources.Views;
using System.Windows.Interop;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <inheritdoc cref="IRhinoVersionDialogManager"/>
/// <remarks>
/// Unlike the other dialog managers this one shows its window modally on the calling
/// thread rather than pumping it on a dedicated STA thread. It runs during AutoCAD's
/// startup, on AutoCAD's own UI thread, and its answer is needed before startup can
/// continue; putting the window on a second thread there means blocking the thread which
/// owns the WPF <see cref="System.Windows.Application"/> while the window initialises
/// against it, which is a deadlock waiting to happen.
/// </remarks>
public class RhinoVersionDialogManager : IRhinoVersionDialogManager
{
    /// <summary>
    /// Handles assembly resolution for the WPF pack URIs the window's XAML references.
    /// </summary>
    private System.Reflection.Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
    {
        var assemblyName = new System.Reflection.AssemblyName(args.Name);

        if (assemblyName.Name == "Rhino.Inside.AutoCAD.UI.Resources")
        {
            return System.Reflection.Assembly.GetExecutingAssembly();
        }

        try
        {
            var executingAssemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = System.IO.Path.GetDirectoryName(executingAssemblyPath);

            if (assemblyDirectory != null)
            {
                var assemblyPath = System.IO.Path.Combine(assemblyDirectory, assemblyName.Name + ".dll");

                if (System.IO.File.Exists(assemblyPath))
                {
                    return System.Reflection.Assembly.LoadFrom(assemblyPath);
                }
            }
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);
        }

        return null;
    }

    /// <summary>
    /// Makes the dialog modal to the AutoCAD main window, so it cannot be lost behind it.
    /// </summary>
    /// <remarks>
    /// The main window handle is not guaranteed to exist this early in startup, in which
    /// case the dialog is shown unowned rather than not at all.
    /// </remarks>
    /// <param name="window">The window to set the owner of.</param>
    private void SetAutoCadOwner(RhinoVersionDialogWindow window)
    {
        try
        {
            var mainWindow = Autodesk.AutoCAD.ApplicationServices.Core.Application.MainWindow;

            if (mainWindow?.Handle is { } handle && handle != IntPtr.Zero)
                new WindowInteropHelper(window).Owner = handle;
        }
        catch (Exception e)
        {
            LoggerService.Instance.LogError(e);
        }
    }

    /// <inheritdoc />
    public IRhinoVersionDialogResult Show(IReadOnlyList<IRhinoInstallation> installations,
        IRhinoInstallation? preselected = null)
    {
        AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

        try
        {
            var viewModel = new RhinoVersionDialogViewModel(installations, preselected);

            var window = new RhinoVersionDialogWindow(viewModel)
            {
                Topmost = true
            };

            this.SetAutoCadOwner(window);

            window.ShowDialog();

            var choice = viewModel.Choice;

            var installation = choice == RhinoVersionChoice.Cancel
                ? null
                : viewModel.SelectedInstallation;

            return new RhinoVersionDialogResult(choice, installation);
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }
    }
}
