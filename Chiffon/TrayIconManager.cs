namespace Chiffon;

/// <summary>
/// Manages the system-tray icon, context menu, NumLock monitoring, and
/// startup-entry bookkeeping for the Chiffon application.
/// </summary>
internal sealed class TrayIconManager : IDisposable
{
    private readonly IntPtr _hwnd;

    // Keep delegate alive to prevent GC collection
    private readonly NativeMethods.SubclassProc _subclassProc;

    private NumLockMonitor?            _monitor;
    private IntPtr                     _currentIconHandle = IntPtr.Zero;
    private NativeMethods.NOTIFYICONDATA _nid;
    private bool                       _iconAdded;
    private bool                       _disposed;

    private static readonly IntPtr SubclassId = new(1);

    public TrayIconManager(IntPtr hwnd)
    {
        _hwnd         = hwnd;
        _subclassProc = WindowSubclassProc; // pin delegate
    }

    // ── Public surface ────────────────────────────────────────────────────

    public void Initialize()
    {
        // Check startup state (external tools may have changed it)
        // – nothing to update in memory; we read fresh on each menu open.

        // Sub-class the window so we can receive tray callback messages
        NativeMethods.SetWindowSubclass(_hwnd, _subclassProc, SubclassId, IntPtr.Zero);

        // Build base NOTIFYICONDATA
        _nid = new NativeMethods.NOTIFYICONDATA
        {
            hWnd            = _hwnd,
            uID             = 1,
            uFlags          = NativeMethods.NIF_ICON |
                              NativeMethods.NIF_TIP  |
                              NativeMethods.NIF_MESSAGE,
            uCallbackMessage = NativeMethods.WM_TRAYICON,
        };

        // Start monitor (will fire immediately with initial state)
        _monitor = new NumLockMonitor(OnStateChanged);
        UpdateIcon(_monitor.HasNumpad, _monitor.NumLockOn);
        _monitor.Start();
    }

    // szTip in NOTIFYICONDATA is 128 WCHARs including the null terminator
    private const int MaxTooltipLength = 127;

    private void OnStateChanged(bool hasNumpad, bool numLockOn)
    {
        UpdateIcon(hasNumpad, numLockOn);
    }

    private void UpdateIcon(bool hasNumpad, bool numLockOn)
    {
        IconBuilder.IconType iconType;
        string tooltip;

        if (!hasNumpad)
        {
            iconType = IconBuilder.IconType.NoNumpad;
            tooltip  = Localization.NoNumpad;
        }
        else if (numLockOn)
        {
            iconType = IconBuilder.IconType.NumpadOn;
            tooltip  = Localization.NumpadOn;
        }
        else
        {
            iconType = IconBuilder.IconType.NumpadOff;
            tooltip  = Localization.NumpadOff;
        }

        IntPtr newIcon = IconBuilder.Create(iconType);

        if (tooltip.Length > MaxTooltipLength) tooltip = tooltip[..MaxTooltipLength];

        _nid.hIcon  = newIcon;
        _nid.szTip  = tooltip;

        if (_iconAdded)
        {
            NativeMethods.Shell_NotifyIconW(NativeMethods.NIM_MODIFY, ref _nid);
        }
        else
        {
            NativeMethods.Shell_NotifyIconW(NativeMethods.NIM_ADD, ref _nid);

            // Switch to version 4 for proper per-monitor DPI balloon behaviour
            _nid.uVersion = NativeMethods.NOTIFYICON_VERSION_4;
            NativeMethods.Shell_NotifyIconW(NativeMethods.NIM_SETVERSION, ref _nid);

            _iconAdded = true;
        }

        // Free the old icon handle
        if (_currentIconHandle != IntPtr.Zero)
            NativeMethods.DestroyIcon(_currentIconHandle);
        _currentIconHandle = newIcon;
    }

    // ── Window subclass – receives tray icon callback messages ────────────

    private IntPtr WindowSubclassProc(
        IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam,
        IntPtr uIdSubclass, IntPtr dwRefData)
    {
        if (uMsg == NativeMethods.WM_TRAYICON)
        {
            // With NOTIFYICON_VERSION_4: LOWORD(lParam) = event, HIWORD(lParam) = icon y
            uint notifyMsg = (uint)(lParam.ToInt64() & 0xFFFF);
            if (notifyMsg == NativeMethods.WM_RBUTTONUP)
            {
                ShowContextMenu();
                return IntPtr.Zero;
            }
        }

        return NativeMethods.DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    // ── Context menu ──────────────────────────────────────────────────────

    private void ShowContextMenu()
    {
        bool startupEnabled = StartupManager.IsEnabled();

        IntPtr menu = NativeMethods.CreatePopupMenu();
        try
        {
            uint startupFlags = NativeMethods.MF_STRING |
                                (startupEnabled ? NativeMethods.MF_CHECKED : 0u);

            NativeMethods.AppendMenuW(menu, startupFlags,
                new IntPtr(NativeMethods.CMD_STARTUP), Localization.StartOnBoot);

            NativeMethods.AppendMenuW(menu, NativeMethods.MF_SEPARATOR,
                IntPtr.Zero, null);

            NativeMethods.AppendMenuW(menu, NativeMethods.MF_STRING,
                new IntPtr(NativeMethods.CMD_EXIT), Localization.Exit);

            NativeMethods.GetCursorPos(out var pt);

            // Required so the menu dismisses when clicking elsewhere
            NativeMethods.SetForegroundWindow(_hwnd);

            int cmd = NativeMethods.TrackPopupMenu(
                menu,
                NativeMethods.TPM_LEFTALIGN   |
                NativeMethods.TPM_RETURNCMD   |
                NativeMethods.TPM_RIGHTBUTTON |
                NativeMethods.TPM_BOTTOMALIGN,
                pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);

            // Post WM_NULL so the menu anchor is properly released
            NativeMethods.PostMessage(_hwnd, NativeMethods.WM_NULL,
                IntPtr.Zero, IntPtr.Zero);

            HandleMenuCommand(cmd);
        }
        finally
        {
            NativeMethods.DestroyMenu(menu);
        }
    }

    private void HandleMenuCommand(int cmd)
    {
        switch (cmd)
        {
            case NativeMethods.CMD_STARTUP:
                StartupManager.Toggle();
                break;

            case NativeMethods.CMD_EXIT:
                Dispose();
                Microsoft.UI.Xaml.Application.Current.Exit();
                break;
        }
    }

    // ── Cleanup ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _monitor?.Dispose();

        if (_iconAdded)
        {
            NativeMethods.Shell_NotifyIconW(NativeMethods.NIM_DELETE, ref _nid);
            _iconAdded = false;
        }

        if (_currentIconHandle != IntPtr.Zero)
        {
            NativeMethods.DestroyIcon(_currentIconHandle);
            _currentIconHandle = IntPtr.Zero;
        }
    }
}
