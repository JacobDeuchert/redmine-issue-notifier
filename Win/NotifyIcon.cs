using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Controls;
using System.Collections.Generic;
using redmine_notifier.Win.Interop;

namespace redmine_notifier.Win
{
    /// <summary>
    /// Represents a taskbar notification area icon (aka "tray icon") on Windows.
    /// </summary>
    public class NotifyIcon 
    {
        private NotifyIconHelperWindow _helperWindow = null;
        private readonly int _uID = 0;
        private static int _nextUID = 0;
        private bool _iconAdded = false;
        private string _iconPath = string.Empty;
        private Icon _icon = null;
        private string _toolTipText = string.Empty;
        private bool _visible = false;
        private bool _doubleClick = false;

        public event EventHandler<EventArgs> Click;
        public event EventHandler<EventArgs> DoubleClick;
        public event EventHandler<EventArgs> RightClick;

        /// <summary>
        /// Gets or sets the icon for the notify icon. Either a file system path
        /// or a <c>resm:</c> manifest resource path can be specified.
        /// </summary>
        public string IconPath 
        {
            get => _iconPath;
            set
            {
                try
                {
                    // Check if path is a file system or resource path
                    if (value.StartsWith("resm:"))
                    {
                        // Resource path
                        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                        _icon = new Icon(assets.Open(new Uri(value)));

                    }
                    else
                    {
                        // File system path
                        _icon = new Icon(value);
                    }
                    _iconPath = value;
                }
                catch (Exception)
                {
                    _icon = null;
                    _iconPath = string.Empty;
                }
                finally
                {
                    UpdateIcon();
                }

            }
        }

        /// <summary>
        /// Gets or sets the tooltip text for the notify icon.
        /// </summary>
        public string ToolTipText 
        {
            get => _toolTipText;
            set
            {
                if (_toolTipText != value)
                {
                    _toolTipText = value;
                }
                UpdateIcon();
            }
        }

        /// <summary>
        /// Gets or sets the context menu for the notify icon.
        /// </summary>
        public ContextMenu ContextMenu { get; set; }

        /// <summary>
        /// Gets or sets if the notify icon is visible in the 
        /// taskbar notification area or not.
        /// </summary>
        public bool Visible 
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
                }
                UpdateIcon();
            }
        }

        /// <summary>
        /// Creates a new <c>NotifyIcon</c> instance and sets up some 
        /// required resources.
        /// </summary>
        public NotifyIcon()
        {
            _uID = ++_nextUID;
            _helperWindow = new NotifyIconHelperWindow(this);
        }

        ~NotifyIcon()
        {
            UpdateIcon(remove: true);
        }

        /// <summary>
        /// Shows, hides or removes the notify icon based on the set properties and parameters.
        /// </summary>
        /// <param name="remove">If set to true, the notify icon will be removed.</param>
        private void UpdateIcon(bool remove = false)
        {
            UnmanagedMethods.NOTIFYICONDATA iconData = new UnmanagedMethods.NOTIFYICONDATA()
            {
                hWnd = _helperWindow.Handle,
                uID = _uID,
                uFlags = UnmanagedMethods.NIF.TIP | UnmanagedMethods.NIF.MESSAGE,
                uCallbackMessage = (int)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE,
                hIcon = IntPtr.Zero,
                szTip = ToolTipText
            };

            if (!remove && _icon != null && Visible)
            {
                iconData.uFlags |= UnmanagedMethods.NIF.ICON;
                iconData.hIcon = _icon.Handle;

                if (!_iconAdded)
                {
                    UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.ADD, iconData);
                    _iconAdded = true;
                }
                else
                {
                    UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.MODIFY, iconData);
                }
            }
            else
            {
                UnmanagedMethods.Shell_NotifyIcon(UnmanagedMethods.NIM.DELETE, iconData);
                _iconAdded = false;
            }
        }

        /// <summary>
        /// Removes the notify icon from the taskbar notification area.
        /// </summary>
        public void Remove()
        {
            UpdateIcon(remove: true);
        }

        /// <summary>
        /// If available, displays the notification icon's context menu.
        /// </summary>
        private void ShowContextMenu()
        {
            if (ContextMenu != null)
            {
                // Since we can't use the Avalonia ContextMenu directly due to shortcomings
                // regrading its positioning, we'll create a native context menu instead. 
                // This dictionary will map the menu item IDs which we'll need for the native 
                // menu to the MenuItems of the provided Avalonia ContextMenu.
                Dictionary<uint, MenuItem> contextItemLookup = new Dictionary<uint, MenuItem>();

                // Create a native (Win32) popup menu as the notify icon's context menu.
                IntPtr popupMenu = UnmanagedMethods.CreatePopupMenu();

                uint i = 1;
                foreach (var item in this.ContextMenu.Items)
                {
                    MenuItem menuItem = (MenuItem)item;

                    // Add items to the native context menu by simply reusing
                    // the information provided within the Avalonia ContextMenu.
                    UnmanagedMethods.AppendMenu(popupMenu, UnmanagedMethods.MenuFlags.MF_STRING, i, (string)menuItem.Header);

                    // Add the mapping so that we can find the selected item later
                    contextItemLookup.Add(i, menuItem);
                    i++;
                }

                // To display a context menu for a notification icon, the current window 
                // must be the foreground window before the application calls TrackPopupMenu 
                // or TrackPopupMenuEx.Otherwise, the menu will not disappear when the user 
                // clicks outside of the menu or the window that created the menu (if it is 
                // visible). If the current window is a child window, you must set the 
                // (top-level) parent window as the foreground window.
                UnmanagedMethods.SetForegroundWindow(_helperWindow.Handle);

                // Get the mouse cursor position
                UnmanagedMethods.GetCursorPos(out UnmanagedMethods.POINT pt);

                // Now display the context menu and block until we get a result
                uint commandId = UnmanagedMethods.TrackPopupMenuEx(
                    popupMenu,
                    UnmanagedMethods.UFLAGS.TPM_BOTTOMALIGN |
                    UnmanagedMethods.UFLAGS.TPM_RIGHTALIGN |
                    UnmanagedMethods.UFLAGS.TPM_NONOTIFY |
                    UnmanagedMethods.UFLAGS.TPM_RETURNCMD,
                    pt.X, pt.Y, _helperWindow.Handle, IntPtr.Zero);

                // If we have a result, execute the corresponding command
                if (commandId != 0) {
                    var lookupIndex = (uint) commandId;
                    contextItemLookup[lookupIndex].Command.Execute(null);
                }
                    
            }
        }

        /// <summary>
        /// Handles the NotifyIcon-specific window messages sent by the notification icon.
        /// </summary>
        public void WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            // We only care about tray icon messages
            if (msg != (uint)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE)
                return;

            // Determine the type of message and call the matching event handlers
            switch (lParam.ToInt32())
            {
                case (int)UnmanagedMethods.WindowsMessage.WM_LBUTTONUP:
                    if (!_doubleClick)
                    {
                        Click?.Invoke(this, new EventArgs());
                    }
                    _doubleClick = false;
                    break;

                case (int)UnmanagedMethods.WindowsMessage.WM_LBUTTONDBLCLK:
                    DoubleClick?.Invoke(this, new EventArgs());
                    _doubleClick = true;
                    break;

                case (int)UnmanagedMethods.WindowsMessage.WM_RBUTTONUP:
                    EventArgs e = new EventArgs();
                    RightClick?.Invoke(this, e);
                    ShowContextMenu();
                    break;

                default:
                    break;
            }
        }
    }



    /// <summary>
    /// A native Win32 helper window encapsulation for dealing with the window 
    /// messages sent by the notification icon.
    /// </summary>
    public class NotifyIconHelperWindow : NativeWindow
    {
        private NotifyIcon _notifyIcon;
        
        public NotifyIconHelperWindow(NotifyIcon notifyIcon) : base()
        {
            _notifyIcon = notifyIcon;
        }

        /// <summary>
        /// This function will receive all the system window messages relevant to our window.
        /// </summary>
        protected override IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case (uint)UnmanagedMethods.CustomWindowsMessage.WM_TRAYMOUSE:
                    // Forward WM_TRAYMOUSE messages to the tray icon's window procedure
                    _notifyIcon.WndProc(hWnd, msg, wParam, lParam);
                    break;
                default:
                    return base.WndProc(hWnd, msg, wParam, lParam);
            }
            return IntPtr.Zero;
        }
    }
}