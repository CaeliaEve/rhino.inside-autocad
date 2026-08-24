using System;
using System.Threading;
using System.Windows.Threading;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using Rhino.Inside.AutoCAD.Services;
using Rhino.Inside.AutoCAD.UI.Resources.ViewModels;
using Rhino.Inside.AutoCAD.UI.Resources.Views;

namespace Rhino.Inside.AutoCAD.UI.Resources.Models;

/// <summary>
/// Next-Gen Lightweight Zero-Barrier Splash Screen Manager.
/// Runs completely asynchronously without blocking the AutoCAD main engine thread.
/// </summary>
public class LoadingScreenManager : ILoadingScreenManager
{
    private readonly ILoggerService _logger = LoggerService.Instance;
    private readonly ILoadingScreenConstants? _loadingScreenConstants;
    private readonly IApplicationVersionHistory? _applicationVersionHistory;
    private readonly Version? _rhinoVersion;
    private readonly string? _standaloneAppVersion;
    private readonly string? _standaloneRhinoVersion;

    private LoadingScreenWindow? _loadingScreenWindow;
    private LoadingScreenViewModel? _loadingScreenViewModel;

    private Dispatcher? _dispatcher;
    private Thread? _newWindowThread;
    private volatile bool _isClosed;
    private readonly object _initLock = new();

    /// <summary>
    /// Constructs a new <see cref="LoadingScreenManager"/> for standalone launcher.
    /// </summary>
    public LoadingScreenManager(string appVersion = "1.0.0", string rhinoVersion = "8.0")
    {
        _standaloneAppVersion = appVersion;
        _standaloneRhinoVersion = rhinoVersion;
    }

    /// <summary>
    /// Constructs a new <see cref="LoadingScreenManager"/>.
    /// </summary>
    public LoadingScreenManager(IRhinoInsideAutoCadApplication application)
    {
        _loadingScreenConstants = application.SettingsManager.Core.LoadingScreenConstants;
        _applicationVersionHistory = application.Bootstrapper.ApplicationVersionHistory;
        _rhinoVersion = application.RhinoInsideManager.RhinoInstance.ApplicationVersion;
    }

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

    private void ThreadStartingPoint()
    {
        AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

        try
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            lock (_initLock)
            {
                if (_isClosed)
                {
                    return;
                }

                if (_standaloneAppVersion != null)
                {
                    _loadingScreenViewModel = new LoadingScreenViewModel(_standaloneAppVersion, _standaloneRhinoVersion ?? "8.0");
                }
                else
                {
                    _loadingScreenViewModel = new LoadingScreenViewModel(_loadingScreenConstants!, _applicationVersionHistory!, _rhinoVersion!);
                }

                _loadingScreenWindow = new LoadingScreenWindow(_loadingScreenViewModel!)
                {
                    Topmost = true
                };

                _loadingScreenWindow.Show();
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

    /// <inheritdoc/>
    public void Show()
    {
        _isClosed = false;
        _newWindowThread = new Thread(this.ThreadStartingPoint)
        {
            IsBackground = true
        };
        _newWindowThread.SetApartmentState(ApartmentState.STA);
        _newWindowThread.Start();
    }

    /// <inheritdoc/>
    public void ShowFailedValidationInfo(IStartUpLogger startUpLogger)
    {
        var messageInfo = startUpLogger.GetLastErrorMessage();
        this.ShowFailureMessage(messageInfo);
    }

    /// <inheritdoc/>
    public void ShowExceptionInfo()
    {
        this.ShowFailureMessage(_loadingScreenConstants.FailedStartupMessage ?? "Startup failed.");
    }

    /// <inheritdoc/>
    public void ShowFailureMessage(string message)
    {
        if (_dispatcher != null && !_isClosed)
        {
            _dispatcher.BeginInvoke(new Action(() =>
            {
                _loadingScreenViewModel?.SetToFailedState(message);
            }));
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        lock (_initLock)
        {
            _isClosed = true;

            try
            {
                if (_dispatcher != null)
                {
                    _dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            _loadingScreenViewModel?.Dispose();
                            _loadingScreenWindow?.Close();
                            _dispatcher.InvokeShutdown();
                        }
                        catch { }
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex);
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.Close();
    }
}