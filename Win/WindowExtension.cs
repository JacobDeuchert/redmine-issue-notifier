using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

internal static class WindowExtensions
{

    private static readonly bool IsWin32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const int SWP_NOSIZE = 0x0001;
    private const int SWP_NOZORDER = 0x0004;
    private const int SWP_SHOWWINDOW = 0x0040;
    private const int SWP_NOMOVE = 0x0002;

    public static void ActivateWorkaround(this Window window)
        {
            if (ReferenceEquals(window, null)) throw new ArgumentNullException(nameof(window));

            // Call default Activate() anyway.
            window.Activate();

            // Skip workaround for non-windows platforms.
            if (!IsWin32NT) return;
            
            var platformImpl = window.PlatformImpl;
            if (ReferenceEquals(platformImpl, null)) return;

            var platformHandle = platformImpl.Handle;
            if (ReferenceEquals(platformHandle, null)) return;

            var handle = platformHandle.Handle;
            if (IntPtr.Zero == handle) return;

            try
            {
                SetForegroundWindow(handle);
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            }
            catch
            {
                // ignored
            }
        }
}