using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Handles the application's visual themes
/// and applies styling to all UI controls.
/// </summary>
public sealed class ThemeService
{
    /// <summary>
    /// Indicates whether dark mode is enabled.
    /// Dark mode is the default theme.
    /// </summary>
    public bool IsDarkMode { get; private set; } = true;

    /// <summary>
    /// Toggles the application theme
    /// between Light and Dark mode.
    /// </summary>
    /// <param name="rootForm">
    /// Main application form.
    /// </param>
    public void ToggleTheme(Form rootForm)
    {
        // Switch theme state
        IsDarkMode = !IsDarkMode;

        // Apply updated theme
        ApplyTheme(rootForm);
    }

    /// <summary>
    /// Applies the current theme recursively
    /// to a control and all child controls.
    /// </summary>
    /// <param name="control">
    /// Target UI control.
    /// </param>
    public void ApplyTheme(Control control)
    {
        // Main theme colors
        var background =
            ColorPalette.DarkBackground;

        var surface =
            ColorPalette.DarkSurface;

        var text =
            ColorPalette.DarkText;

        // ==================================================
        // 1. Main Containers
        // ==================================================
        if (control is Form or Panel or UserControl or TabPage)
        {
            control.BackColor = background;
        }

        // ==================================================
        // 2. Buttons
        // ==================================================
        else if (control is Button btn)
        {
            var navState = btn.Tag?.ToString();

            // Active navigation buttons
            if (
                string.Equals(
                    navState,
                    "nav-active",
                    StringComparison.OrdinalIgnoreCase)

                ||

                string.Equals(
                    navState,
                    "active",
                    StringComparison.OrdinalIgnoreCase))
            {
                btn.BackColor =
                    ColorPalette.AccentGreen;

                btn.ForeColor = Color.White;
            }
            else
            {
                // Default button style
                btn.BackColor = surface;
                btn.ForeColor = text;

                btn.FlatAppearance.BorderColor =
                    IsDarkMode
                        ? Color.FromArgb(50, 50, 50)
                        : Color.LightGray;
            }
        }

        // ==================================================
        // 3. Labels & Text
        // ==================================================
        else if (control is Label lbl)
        {
            // Ignore labels with custom status colors
            if (
                !string.Equals(
                    lbl.Tag?.ToString(),
                    "status",
                    StringComparison.OrdinalIgnoreCase))
            {
                lbl.ForeColor = text;
            }

            // Transparent background for cleaner UI
            if (lbl.Parent is not null)
            {
                lbl.BackColor = Color.Transparent;
            }
        }

        // ==================================================
        // 4. Input Controls
        // ==================================================
        else if (
            control is TextBox ||
            control is NumericUpDown ||
            control is ComboBox)
        {
            control.BackColor = surface;
            control.ForeColor = text;

            // Flat ComboBox style
            if (control is ComboBox combo)
            {
                combo.FlatStyle = FlatStyle.Flat;
            }
        }

        // ==================================================
        // 5. Other Controls
        // ==================================================
        else
        {
            control.BackColor = surface;
            control.ForeColor = text;
        }

        // ==================================================
        // Apply theme recursively to child controls
        // ==================================================
        foreach (Control child in control.Controls)
        {
            ApplyTheme(child);
        }
    }
}
