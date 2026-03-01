using System.Runtime.InteropServices;

namespace Chiffon;

/// <summary>
/// Detects whether a physical numeric keypad is present and monitors the
/// NumLock toggle state via a background timer.
/// </summary>
internal sealed class NumLockMonitor : IDisposable
{
    private readonly Action<bool, bool> _onChange; // (hasNumpad, numLockOn)
    private Timer? _timer;
    private bool   _hasNumpad;
    private bool   _numLockOn;
    private bool   _disposed;

    /// <param name="onChange">
    /// Callback invoked on the thread-pool whenever the state changes.
    /// Parameters: hasNumpad, numLockOn.
    /// </param>
    public NumLockMonitor(Action<bool, bool> onChange)
    {
        _onChange  = onChange;
        _hasNumpad = DetectNumpad();
        _numLockOn = ReadNumLock();
    }

    public bool HasNumpad  => _hasNumpad;
    public bool NumLockOn  => _numLockOn;

    public void Start(int pollIntervalMs = 500)
    {
        _timer = new Timer(Poll, null, pollIntervalMs, pollIntervalMs);
    }

    private void Poll(object? _)
    {
        bool padNow  = DetectNumpad();
        bool lockNow = ReadNumLock();

        if (padNow != _hasNumpad || lockNow != _numLockOn)
        {
            _hasNumpad = padNow;
            _numLockOn = lockNow;
            _onChange(padNow, lockNow);
        }
    }

    // Keyboards with >= this many total keys are assumed to have a numpad
    // (101/102/104-key boards have numpads; compact 84-88-key boards do not)
    private const int MinimumKeysForNumpad = 100;

    private static bool ReadNumLock()
    {
        // Low bit of GetKeyState indicates toggle state
        return (NativeMethods.GetKeyState(NativeMethods.VK_NUMLOCK) & 1) != 0;
    }

    // ── Numpad presence detection via Raw Input device enumeration ────────

    private static bool DetectNumpad()
    {
        uint count = 0;
        uint structSize = (uint)Marshal.SizeOf<NativeMethods.RAWINPUTDEVICELIST>();

        // First call: get device count
        NativeMethods.GetRawInputDeviceList(null, ref count, structSize);
        if (count == 0) return false;

        var devices = new NativeMethods.RAWINPUTDEVICELIST[count];
        if (NativeMethods.GetRawInputDeviceList(devices, ref count, structSize) == uint.MaxValue)
            return false;

        foreach (var dev in devices)
        {
            if (dev.dwType != NativeMethods.RIM_TYPEKEYBOARD) continue;

            uint cbSize = 0;
            NativeMethods.GetRawInputDeviceInfo(dev.hDevice, NativeMethods.RIDI_DEVICEINFO,
                IntPtr.Zero, ref cbSize);
            if (cbSize == 0) continue;

            // Allocate unmanaged memory for RID_DEVICE_INFO
            // Layout: cbSize(4) + dwType(4) + keyboard union
            IntPtr buf = Marshal.AllocHGlobal((int)cbSize);
            try
            {
                // Write cbSize into the first DWORD (required by API)
                Marshal.WriteInt32(buf, (int)cbSize);

                uint result = NativeMethods.GetRawInputDeviceInfo(
                    dev.hDevice, NativeMethods.RIDI_DEVICEINFO, buf, ref cbSize);
                if (result == uint.MaxValue) continue;

                // dwType is at offset 4
                uint devType = (uint)Marshal.ReadInt32(buf, 4);
                if (devType != NativeMethods.RIM_TYPEKEYBOARD) continue;

                // dwNumberOfKeysTotal is at:
                //   cbSize(4) + dwType(4)         = outer struct: 8 bytes
                //   + kbd.dwType(4) + kbd.dwSubType(4) + kbd.dwKeyboardMode(4)
                //   + kbd.dwNumberOfFunctionKeys(4) + kbd.dwNumberOfIndicators(4) = 20 bytes
                //   total offset = 28
                uint totalKeys = (uint)Marshal.ReadInt32(buf, 28);

                if (totalKeys >= MinimumKeysForNumpad) return true;
            }
            finally
            {
                Marshal.FreeHGlobal(buf);
            }
        }

        return false;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _timer?.Dispose();
        _timer = null;
    }
}
