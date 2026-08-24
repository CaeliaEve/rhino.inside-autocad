using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Rhino.Inside.AutoCAD.Core.UI;

/// <summary>
/// Native Win32 window helper for cross-process window restoration and foreground activation.
/// Bypasses Windows Foreground Lockout restriction using thread attachment.
/// </summary>
public static class WindowHelper
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    [DllImport("user32.dll")]
    private static extern bool BringWindowToTop(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_RESTORE = 9;

    /// <summary>
    /// Forcibly brings the specified window handle to the foreground.
    /// </summary>
    public static void BringToFront(IntPtr hWnd)
    {
        if (hWnd == IntPtr.Zero) return;

        try
        {
            IntPtr foregroundWnd = GetForegroundWindow();
            uint foregroundThreadId = GetWindowThreadProcessId(foregroundWnd, out _);
            uint currentThreadId = GetCurrentThreadId();

            if (foregroundThreadId != currentThreadId && foregroundThreadId != 0)
            {
                AttachThreadInput(currentThreadId, foregroundThreadId, true);
                ShowWindow(hWnd, SW_RESTORE);
                SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
                BringWindowToTop(hWnd);
                SetForegroundWindow(hWnd);
                AttachThreadInput(currentThreadId, foregroundThreadId, false);
            }
            else
            {
                ShowWindow(hWnd, SW_RESTORE);
                BringWindowToTop(hWnd);
                SetForegroundWindow(hWnd);
            }
        }
        catch
        {
            try
            {
                ShowWindow(hWnd, SW_RESTORE);
                SetForegroundWindow(hWnd);
            }
            catch { }
        }
    }

    /// <summary>
    /// Finds AutoCAD main window handle and brings it to the front.
    /// </summary>
    public static bool ActivateAutoCad()
    {
        var cadProcesses = Process.GetProcessesByName("acad");
        if (cadProcesses.Length > 0 && cadProcesses[0].MainWindowHandle != IntPtr.Zero)
        {
            BringToFront(cadProcesses[0].MainWindowHandle);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Finds Rhino main window handle and brings it to the front.
    /// </summary>
    public static bool ActivateRhino()
    {
        var rhinoProcesses = Process.GetProcessesByName("Rhino");
        if (rhinoProcesses.Length > 0 && rhinoProcesses[0].MainWindowHandle != IntPtr.Zero)
        {
            BringToFront(rhinoProcesses[0].MainWindowHandle);
            return true;
        }
        return false;
    }
}
