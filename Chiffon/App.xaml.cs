using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace Chiffon;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private TrayIconManager? _trayIconManager;

    public App()
    {
        this.InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();

        // Hide the window so the app appears only in the system tray
        var hwnd = WindowNative.GetWindowHandle(_mainWindow);
        NativeMethods.ShowWindow(hwnd, NativeMethods.SW_HIDE);

        // Remove from taskbar and Alt+Tab list
        int exStyle = NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE);
        NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_EXSTYLE,
            (exStyle | NativeMethods.WS_EX_TOOLWINDOW) & ~NativeMethods.WS_EX_APPWINDOW);

        _trayIconManager = new TrayIconManager(hwnd);
        _trayIconManager.Initialize();
    }
}
