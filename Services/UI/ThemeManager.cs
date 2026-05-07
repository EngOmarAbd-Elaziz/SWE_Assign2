using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Manages application themes and warning visual effects.
/// Supports switching between Light/Dark themes
/// and applying warning color overlays.
/// </summary>
public sealed class ThemeManager
{
    /// <summary>
    /// Internal theme service responsible for
    /// handling the base theme logic.
    /// </summary>
    private readonly ThemeService _inner = new();

    /// <summary>
    /// Indicates whether warning mode is enabled.
    /// </summary>
    public bool WarningMode { get; private set; }

    /// <summary>
    /// Indicates whether the application
    /// is currently using dark mode.
    /// </summary>
    public bool IsDarkMode => _inner.IsDarkMode;

    /// <summary>
    /// Toggles the application theme
    /// between Light and Dark mode.
    /// </summary>
    /// <param name="rootForm">
    /// Main application form.
    /// </param>
    public void ToggleTheme(Form rootForm)
    {
        // Toggle base theme
        _inner.ToggleTheme(rootForm);

        // Reapply warning tint if warning mode is active
        if (WarningMode)
        {
            ApplyWarningTint(rootForm);
        }
    }

    /// <summary>
    /// Applies the current theme to a control.
    /// </summary>
    /// <param name="control">
    /// Target UI control.
    /// </param>
    public void ApplyTheme(Control control)
    {
        // Apply base theme
        _inner.ApplyTheme(control);

        // Apply warning styling if enabled
        if (WarningMode)
        {
            ApplyWarningTint(control);
        }
    }

    /// <summary>
    /// Enables or disables warning mode.
    /// Usually activated when the user
    /// exceeds the budget limit.
    /// </summary>
    /// <param name="root">
    /// Root control or form.
    /// </param>
    /// <param name="enabled">
    /// True to enable warning mode.
    /// </param>
    public void SetWarningMode(
        Control root,
        bool enabled)
    {
        WarningMode = enabled;

        // Reapply full theme to refresh UI colors
        ApplyTheme(root);
    }

    /// <summary>
    /// Applies warning colors recursively
    /// to controls and child controls.
    /// </summary>
    /// <param name="control">
    /// Target control.
    /// </param>
    private static void ApplyWarningTint(Control control)
    {
        // Warning background colors
        var warningBg =
            Color.FromArgb(45, 20, 20);

        var warningCard =
            Color.FromArgb(60, 30, 30);

        // Apply styles based on control type
        if (control is Form or UserControl)
        {
            // Main background color
            control.BackColor = warningBg;
        }
        else if (
            control is Panel &&
            control.Name != "PanelContent")
        {
            // Panel/card warning tint
            control.BackColor = warningCard;
        }
        else if (control is Label lbl)
        {
            // Adjust label text colors
            if (lbl.ForeColor != Color.SpringGreen)
            {
                lbl.ForeColor =
                    Color.FromArgb(255, 200, 200);
            }
        }

        // Apply recursively to child controls
        foreach (Control child in control.Controls)
        {
            ApplyWarningTint(child);
        }
    }
}
