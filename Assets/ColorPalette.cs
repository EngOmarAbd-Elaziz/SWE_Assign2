namespace Masroofy.App.Assets;

/// <summary>
/// Provides a centralized collection of colors used throughout the Masroofy application.
/// This class helps maintain consistent UI styling and supports both dark and light themes.
/// </summary>
public static class ColorPalette
{
    /// <summary>
    /// Main accent color used in the application interface.
    /// </summary>
    public static readonly Color AccentGreen = Color.FromArgb(0x50, 0xC8, 0x78);

    /// <summary>
    /// Color representing safe budget or positive financial status.
    /// </summary>
    public static readonly Color SafeGreen = Color.FromArgb(0x50, 0xC8, 0x78);

    /// <summary>
    /// Color used for warnings or caution states.
    /// </summary>
    public static readonly Color WarningOrange = Color.FromArgb(255, 165, 0);

    /// <summary>
    /// Color used when the budget limit has been exceeded.
    /// </summary>
    public static readonly Color OverspentRed = Color.FromArgb(220, 50, 50);

    /// <summary>
    /// Background color for dark mode screens.
    /// </summary>
    public static readonly Color DarkBackground = Color.FromArgb(0x0C, 0x0C, 0x0C);

    /// <summary>
    /// Surface color for cards and containers in dark mode.
    /// </summary>
    public static readonly Color DarkSurface = Color.FromArgb(0x1D, 0x1D, 0x1D);

    /// <summary>
    /// Text color used in dark mode.
    /// </summary>
    public static readonly Color DarkText = Color.Gainsboro;

    /// <summary>
    /// Background color for light mode screens.
    /// </summary>
    public static readonly Color LightBackground = Color.FromArgb(246, 246, 250);

    /// <summary>
    /// Surface color for cards and containers in light mode.
    /// </summary>
    public static readonly Color LightSurface = Color.White;

    /// <summary>
    /// Text color used in light mode.
    /// </summary>
    public static readonly Color LightText = Color.FromArgb(32, 32, 36);
}
