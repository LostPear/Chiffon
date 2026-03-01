using Microsoft.Win32;

namespace Chiffon;

/// <summary>
/// Manages the Windows startup registry entry for the application.
/// </summary>
internal static class StartupManager
{
    private const string RunKey  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "Chiffon";

    /// <summary>
    /// Returns <see langword="true"/> if the app's startup entry currently
    /// exists in the registry <em>and</em> points to this executable.
    /// </summary>
    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
        if (key == null) return false;

        var value = key.GetValue(AppName) as string;
        return string.Equals(value, ExePath(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Adds or removes the startup registry entry, returning the new state.
    /// </summary>
    public static bool Toggle()
    {
        bool current = IsEnabled();
        if (current)
            Disable();
        else
            Enable();
        return !current;
    }

    public static void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.SetValue(AppName, ExePath());
    }

    public static void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    private static string ExePath() =>
        System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName
        ?? System.Environment.ProcessPath
        ?? string.Empty;
}
