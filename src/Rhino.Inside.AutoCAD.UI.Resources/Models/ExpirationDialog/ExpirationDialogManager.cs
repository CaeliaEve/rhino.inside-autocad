using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.ViewModels;
using Rhino.Inside.AutoCAD.UI.Resources.Views;
using System.Windows.Threading;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <summary>
/// Manages the display of the expiration dialog window.
/// </summary>
public class ExpirationDialogManager
{
    private readonly ILoggerService _logger = LoggerService.Instance;

    private ExpirationDialogWindow? _dialogWindow;
    private ExpirationDialogViewModel? _viewModel;

    private Dispatcher? _dispatcher;
    private Thread? _dialogThread;

    private readonly object _initLock = new object();
    private bool _isDispatcherReady = false;

    private readonly string _expirationMessage;
    private readonly string _downloadUrl;
    private readonly string _appVersion;

    /// <summary>
    /// Constructs a new <see cref="ExpirationDialogManager"/>.
    /// </summary>
    /// <param name="expirationMessage">The expiration message to display.</param>
    /// <param name="downloadUrl">The URL to download the latest version.</param>
    /// <param name="appVersion">The current application version.</param>
    public ExpirationDialogManager(string expirationMessage, string downloadUrl, string appVersion)
    {
        _expirationMessage = expirationMessage;
        _downloadUrl = downloadUrl;
        _appVersion = appVersion;
    }

    /// <summary>
    /// Handles assembly resolution for WPF pack URIs and dependencies in the new thread context.
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
        catch (Exception ex)
        {
            _logger.LogError(ex);
        }

        return null;
    }

    /// <summary>
    /// The thread start point to launch the expiration dialog window.
    /// </summary>
    private void ThreadStartingPoint()
    {
        AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

        try
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            lock (_initLock)
            {
                _viewModel = new ExpirationDialogViewModel(_expirationMessage, _downloadUrl, _appVersion);

                _dialogWindow = new ExpirationDialogWindow(_viewModel)
                {
                    Topmost = true
                };

                _dialogWindow.Show();

                _isDispatcherReady = true;

                Monitor.PulseAll(_initLock);
            }

            Dispatcher.Run();
        }
        catch (Exception e)
        {
            _logger.LogError(e);
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }
    }

    /// <summary>
    /// Shows the expiration dialog.
    /// </summary>
    public void Show()
    {
        _dialogThread = new Thread(this.ThreadStartingPoint);

        _dialogThread.SetApartmentState(ApartmentState.STA);

        _dialogThread.IsBackground = true;

        _dialogThread.Start();

        // Wait for the dialog to be displayed
        lock (_initLock)
        {
            while (_isDispatcherReady == false)
            {
                Monitor.Wait(_initLock);
            }
        }
    }

    /// <summary>
    /// Closes the expiration dialog.
    /// </summary>
    public void Close()
    {
        _dispatcher?.Invoke(() =>
        {
            _dialogWindow?.Close();
        });

        _dispatcher?.InvokeShutdown();

        _dialogThread?.Join();
    }
}
