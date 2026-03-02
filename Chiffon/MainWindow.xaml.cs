using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Chiffon;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Prevent the window from being closed normally; the app exits via tray menu only
        this.AppWindow.Closing += (_, e) => e.Cancel = true;
    }
}
