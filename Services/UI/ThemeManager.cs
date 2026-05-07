using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Manages the application's visual theming, including light/dark mode toggling
/// and a warning mode that overlays a dark-red tint across the UI to signal
/// budget alerts such as overspending or low balance.
/// </summary>
public sealed class ThemeManager
{
    private readonly ThemeService _inner = new();

    /// <summary>
    /// Gets a value indicating whether warning mode is currently active,
    /// causing all controls to render with a dark-red warning tint.
    /// </summary>
    public bool WarningMode { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the application is currently in dark mode.
    /// </summary>
    public bool IsDarkMode => _inner.IsDarkMode;

    /// <summary>
    /// Toggles the application theme between light and dark mode on the given root form.
    /// If warning mode is active, the warning tint is reapplied after the theme switch
    /// to ensure visual consistency.
    /// </summary>
    /// <param name="rootForm">The root form whose theme and child controls will be updated.</param>
    public void ToggleTheme(Form rootForm)
    {
        _inner.ToggleTheme(rootForm);
        if (WarningMode) ApplyWarningTint(rootForm);
    }

    /// <summary>
    /// Applies the current theme to the given control and all of its descendants.
    /// If warning mode is active, the dark-red warning tint is applied on top of
    /// the base theme colors.
    /// </summary>
    /// <param name="control">The root control to theme recursively.</param>
    public void ApplyTheme(Control control)
    {
        _inner.ApplyTheme(control);

        if (WarningMode)
        {
            ApplyWarningTint(control);
        }
    }

    /// <summary>
    /// Enables or disables warning mode on the given root control. When enabled,
    /// a dark-red palette is overlaid on all controls to visually signal a budget
    /// alert. The full theme is reapplied immediately to reflect the change.
    /// </summary>
    /// <param name="root">The root control from which theming is applied recursively.</param>
    /// <param name="enabled">True to activate warning mode; false to deactivate it.</param>
    public void SetWarningMode(Control root, bool enabled)
    {
        WarningMode = enabled;
        ApplyTheme(root);
    }

    /// <summary>
    /// Recursively applies a dark-red warning tint to the given control and all its
    /// descendants. Forms and UserControls receive a deep dark-red background, panels
    /// (excluding PanelContent) receive a slightly lighter card tone, and labels receive
    /// a light-red foreground unless they are already colored with SpringGreen, which
    /// is reserved for positive figures such as currency values.
    /// </summary>
    /// <param name="control">The root control to tint recursively.</param>
    private static void ApplyWarningTint(Control control)
    {
        var warningBg = Color.FromArgb(45, 20, 20);
        var warningCard = Color.FromArgb(60, 30, 30);

        if (control is Form or UserControl)
        {
            control.BackColor = warningBg;
        }
        else if (control is Panel && control.Name != "PanelContent")
        {
            control.BackColor = warningCard;
        }
        else if (control is Label lbl)
        {
            if (lbl.ForeColor != Color.SpringGreen)
                lbl.ForeColor = Color.FromArgb(255, 200, 200);
        }

        foreach (Control child in control.Controls)
        {
            ApplyWarningTint(child);
        }
    }
}
