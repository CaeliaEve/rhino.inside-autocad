using Rhino.Input;
using Rhino.Inside.AutoCAD.Core;
using Rhino.Inside.AutoCAD.Core.Interfaces;
using System.Runtime.InteropServices;

namespace Rhino.Inside.AutoCAD.Interop;

/// <inheritdoc cref="IRhinoWindowManager"/>
public class RhinoWindowManager : IRhinoWindowManager
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr windowHandle, int windowShowStyle);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr windowHandle);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr windowHandle);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    private const int WH_CBT = 5;
    private const int HCBT_ACTIVATE = 5;

    private IntPtr _mainWindow;
    private IntPtr _hookHandle = IntPtr.Zero;
    private HookProc? _hookDelegate;
    private bool _disposed;

    public RhinoWindowManager()
    {
        _mainWindow = IntPtr.Zero;
    }

    /// <inheritdoc />
    public void SetWindow(IntPtr mainWindow)
    {
        _mainWindow = mainWindow;
    }

    /// <inheritdoc />
    public void HideWindow()
    {
        if (_mainWindow == IntPtr.Zero)
            return;

        ShowWindow(_mainWindow, (int)WindowShowStyle.Hide);
    }

    /// <inheritdoc />
    public void BringToFront()
    {
        if (_mainWindow == IntPtr.Zero)
            return;

        ShowWindow(_mainWindow, (int)WindowShowStyle.Restore);
        BringWindowToTop(_mainWindow);
        SetForegroundWindow(_mainWindow);
    }

    /// <inheritdoc />
    public void ShowWindow()
    {
        if (_mainWindow == IntPtr.Zero)
            return;

        ShowWindow(_mainWindow, (int)WindowShowStyle.Show);
    }

    /// <inheritdoc />
    public void ShowWindowNoActivate()
    {
        if (_mainWindow == IntPtr.Zero)
            return;

        ShowWindow(_mainWindow, (int)WindowShowStyle.ShowNA);
    }

    /// <inheritdoc />
    public void InstallActivationHook()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(RhinoWindowManager));

        if (_hookHandle != IntPtr.Zero)
            return;

        if (_mainWindow == IntPtr.Zero)
            return;

        _hookDelegate = new HookProc(this.CbtHookProc);

        var windowThreadId = GetWindowThreadProcessId(_mainWindow, out _);

        _hookHandle = SetWindowsHookEx(
            WH_CBT,
            _hookDelegate,
            IntPtr.Zero,
            windowThreadId);

        if (_hookHandle == IntPtr.Zero)
            _hookDelegate = null;
    }

    /// <inheritdoc />
    public void UninstallActivationHook()
    {
        if (_hookHandle == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(_hookHandle);
        _hookHandle = IntPtr.Zero;
        _hookDelegate = null;
    }

    private IntPtr CbtHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && nCode == HCBT_ACTIVATE && wParam == _mainWindow)
        {
            // If window is already visible, always allow activation (normal interaction)
            if (IsWindowVisible(_mainWindow))
            {
                return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
            }

            // Window is hidden - check if Rhino needs user input
            if (RhinoDoc.ActiveDoc is RhinoDoc rhinoDoc && RhinoGet.InGet(rhinoDoc))
            {
                // InGet is true - show window and allow activation
                this.ShowWindowNoActivate();
            }
            else
            {
                // Window hidden, InGet false - block activation so Rhino retries later
                return (IntPtr)1;
            }
        }

        return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
    }



    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        this.UninstallActivationHook();
        _disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~RhinoWindowManager()
    {
        this.Dispose(false);
    }
}
