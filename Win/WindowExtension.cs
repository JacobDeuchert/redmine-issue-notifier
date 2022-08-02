using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;

internal static class WindowExtensions
{

    private static readonly bool IsWin32NT = Environment.OSVersion.Platform == PlatformID.Win32NT;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

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
            }
            catch
            {
                // ignored
            }
        }
}