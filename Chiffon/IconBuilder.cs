using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Chiffon;

/// <summary>
/// Creates HICON handles for the three tray icon states.
/// </summary>
internal static class IconBuilder
{
    public enum IconType
    {
        NoNumpad,   // House outline + X  (no numeric keypad detected)
        NumpadOn,   // House outline + 1  (NumLock on)
        NumpadOff,  // House outline + 1 + red strikethrough (NumLock off)
    }

    private const int Size = 32;

    /// <summary>
    /// Returns a new HICON for the given state. Caller must call
    /// <see cref="NativeMethods.DestroyIcon"/> when finished.
    /// </summary>
    public static IntPtr Create(IconType type)
    {
        using var bmp = new Bitmap(Size, Size, PixelFormat.Format32bppArgb);
        using var g   = Graphics.FromImage(bmp);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        using var whitePen = new Pen(Color.White, 1.8f) { LineJoin = LineJoin.Round };
        DrawHouse(g, whitePen);

        switch (type)
        {
            case IconType.NoNumpad:
                DrawX(g, whitePen);
                break;
            case IconType.NumpadOn:
                DrawOne(g, whitePen);
                break;
            case IconType.NumpadOff:
                DrawOne(g, whitePen);
                using var redPen = new Pen(Color.OrangeRed, 2.2f);
                DrawStrikethrough(g, redPen);
                break;
        }

        return bmp.GetHicon();
    }

    // ── Drawing helpers ───────────────────────────────────────────────────

    private static void DrawHouse(Graphics g, Pen pen)
    {
        // Roof (triangle peak → left-base → right-base)
        PointF peak     = new(16f,  2f);
        PointF roofL    = new( 3f, 13f);
        PointF roofR    = new(29f, 13f);

        g.DrawLine(pen, peak,  roofL);
        g.DrawLine(pen, peak,  roofR);

        // Walls (left and right from roof-base down to bottom)
        PointF wallBL = new( 3f, 29f);
        PointF wallBR = new(29f, 29f);

        g.DrawLine(pen, roofL, wallBL);
        g.DrawLine(pen, roofR, wallBR);

        // Floor
        g.DrawLine(pen, wallBL, wallBR);
    }

    private static void DrawX(Graphics g, Pen pen)
    {
        // Two diagonals inside the body area  (y 15 – 27, x 8 – 24)
        g.DrawLine(pen,  8f, 15f, 24f, 27f);
        g.DrawLine(pen,  8f, 27f, 24f, 15f);
    }

    private static void DrawOne(Graphics g, Pen pen)
    {
        // Short lead-in from upper-left
        g.DrawLine(pen, 13f, 17f, 16f, 14f);
        // Vertical shaft
        g.DrawLine(pen, 16f, 14f, 16f, 27f);
        // Base serif
        g.DrawLine(pen, 12f, 27f, 20f, 27f);
    }

    private static void DrawStrikethrough(Graphics g, Pen pen)
    {
        // Full diagonal from top-left to bottom-right
        g.DrawLine(pen, 1f, 1f, 31f, 31f);
    }
}
