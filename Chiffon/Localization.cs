using System.Globalization;

namespace Chiffon;

/// <summary>
/// Provides localised strings for zh-Hans, zh-Hant, and English (fallback).
/// </summary>
internal static class Localization
{
    private enum Lang { En, ZhHans, ZhHant }

    private static readonly Lang _lang = DetectLang();

    private static Lang DetectLang()
    {
        var name = CultureInfo.CurrentUICulture.Name;
        if (name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-TW",   StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-HK",   StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("zh-MO",   StringComparison.OrdinalIgnoreCase))
            return Lang.ZhHant;

        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            return Lang.ZhHans;

        return Lang.En;
    }

    // ── Tooltip strings ───────────────────────────────────────────────────

    public static string NoNumpad => _lang switch
    {
        Lang.ZhHans => "电脑没有数字小键盘",
        Lang.ZhHant => "電腦沒有數字小鍵盤",
        _           => "No numeric keypad"
    };

    public static string NumpadOn => _lang switch
    {
        Lang.ZhHans => "数字小键盘已开启",
        Lang.ZhHant => "數字小鍵盤已開啟",
        _           => "Num Lock is on"
    };

    public static string NumpadOff => _lang switch
    {
        Lang.ZhHans => "数字小键盘未开启",
        Lang.ZhHant => "數字小鍵盤未開啟",
        _           => "Num Lock is off"
    };

    // ── Context-menu strings ──────────────────────────────────────────────

    public static string StartOnBoot => _lang switch
    {
        Lang.ZhHans => "开机自启动",
        Lang.ZhHant => "開機自啟動",
        _           => "Start on boot"
    };

    public static string Exit => _lang switch
    {
        Lang.ZhHans => "退出",
        Lang.ZhHant => "退出",
        _           => "Exit"
    };
}
