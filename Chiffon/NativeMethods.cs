using System.Runtime.InteropServices;

namespace Chiffon;

internal static class NativeMethods
{
    // ── Window long indices ────────────────────────────────────────────────
    public const int GWL_EXSTYLE = -20;

    // ── Window extended styles ─────────────────────────────────────────────
    public const int WS_EX_TOOLWINDOW = 0x00000080;
    public const int WS_EX_APPWINDOW  = 0x00040000;

    // ── ShowWindow commands ────────────────────────────────────────────────
    public const int SW_HIDE = 0;

    // ── Shell_NotifyIcon messages ──────────────────────────────────────────
    public const uint NIM_ADD       = 0x00000000;
    public const uint NIM_MODIFY    = 0x00000001;
    public const uint NIM_DELETE    = 0x00000002;
    public const uint NIM_SETVERSION = 0x00000004;

    // ── NOTIFYICONDATA flags ───────────────────────────────────────────────
    public const uint NIF_MESSAGE = 0x00000001;
    public const uint NIF_ICON    = 0x00000002;
    public const uint NIF_TIP     = 0x00000004;

    // ── NotifyIcon version ─────────────────────────────────────────────────
    public const uint NOTIFYICON_VERSION_4 = 4;

    // ── Tray callback message (WM_APP + 1) ────────────────────────────────
    public const uint WM_TRAYICON = 0x8001;

    // ── Mouse messages used in tray callback ──────────────────────────────
    public const uint WM_RBUTTONUP = 0x0205;
    public const uint WM_NULL      = 0x0000;

    // ── Menu flags ─────────────────────────────────────────────────────────
    public const uint MF_STRING    = 0x00000000;
    public const uint MF_SEPARATOR = 0x00000800;
    public const uint MF_CHECKED   = 0x00000008;

    // ── TrackPopupMenu flags ───────────────────────────────────────────────
    public const uint TPM_LEFTALIGN   = 0x0000;
    public const uint TPM_RETURNCMD   = 0x0100;
    public const uint TPM_RIGHTBUTTON = 0x0002;
    public const uint TPM_BOTTOMALIGN = 0x0020;

    // ── Virtual key codes ─────────────────────────────────────────────────
    public const int VK_NUMLOCK = 0x90;

    // ── Raw input ─────────────────────────────────────────────────────────
    public const uint RIDI_DEVICEINFO  = 0x2000000b;
    public const uint RIM_TYPEKEYBOARD = 1;

    // ── Menu command IDs ──────────────────────────────────────────────────
    public const int CMD_STARTUP = 1;
    public const int CMD_EXIT    = 2;

    // ── P/Invoke ──────────────────────────────────────────────────────────

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATA lpdata);

    [DllImport("user32.dll")]
    public static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool AppendMenuW(IntPtr hMenu, uint uFlags, IntPtr uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll")]
    public static extern int TrackPopupMenu(
        IntPtr hMenu, uint uFlags, int x, int y,
        int nReserved, IntPtr hwnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern short GetKeyState(int nVirtKey);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("comctl32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool SetWindowSubclass(
        IntPtr hWnd, SubclassProc pfnSubclass,
        IntPtr uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll")]
    public static extern IntPtr DefSubclassProc(
        IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    public static extern uint GetRawInputDeviceList(
        [Out] RAWINPUTDEVICELIST[]? pRawInputDeviceList,
        ref uint puiNumDevices,
        uint cbSize);

    [DllImport("user32.dll")]
    public static extern uint GetRawInputDeviceInfo(
        IntPtr hDevice, uint uiCommand,
        IntPtr pData, ref uint pcbSize);

    // ── Delegate for window subclassing ───────────────────────────────────

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr SubclassProc(
        IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam,
        IntPtr uIdSubclass, IntPtr dwRefData);

    // ── Structures ────────────────────────────────────────────────────────

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NOTIFYICONDATA
    {
        public uint   cbSize;
        public IntPtr hWnd;
        public uint   uID;
        public uint   uFlags;
        public uint   uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public uint   dwState;
        public uint   dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public uint   uVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public uint   dwInfoFlags;
        public Guid   guidItem;
        public IntPtr hBalloonIcon;

        public NOTIFYICONDATA()
        {
            cbSize          = (uint)Marshal.SizeOf<NOTIFYICONDATA>();
            hWnd            = IntPtr.Zero;
            uID             = 0;
            uFlags          = 0;
            uCallbackMessage = 0;
            hIcon           = IntPtr.Zero;
            szTip           = string.Empty;
            dwState         = 0;
            dwStateMask     = 0;
            szInfo          = string.Empty;
            uVersion        = 0;
            szInfoTitle     = string.Empty;
            dwInfoFlags     = 0;
            guidItem        = Guid.Empty;
            hBalloonIcon    = IntPtr.Zero;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTDEVICELIST
    {
        public IntPtr hDevice;
        public uint   dwType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RID_DEVICE_INFO_KEYBOARD
    {
        public uint dwType;
        public uint dwSubType;
        public uint dwKeyboardMode;
        public uint dwNumberOfFunctionKeys;
        public uint dwNumberOfIndicators;
        public uint dwNumberOfKeysTotal;
    }
}
