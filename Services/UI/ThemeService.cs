using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Applies and toggles the application's visual theme across all controls,
/// defaulting to dark mode and recursively styling forms, panels, buttons,
/// labels, and input fields using the defined color palette.
/// </summary>
public sealed class ThemeService
{
    /// <summary>
    /// Gets a value indicating whether the application is currently in dark mode.
    /// Defaults to true on initialization.
    /// </summary>
    public bool IsDarkMode { get; private set; } = true;

    /// <summary>
    /// Toggles the theme between dark and light mode and immediately reapplies
    /// it to the given root form and all of its descendants.
    /// </summary>
    /// <param name="rootForm">The root form to restyle after the toggle.</param>
    public void ToggleTheme(Form rootForm)
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme(rootForm);
    }

    /// <summary>
    /// Recursively applies the current theme to the given control and all of its
    /// descendants using the following rules:
    /// <list type="bullet">
    /// <item>Forms, Panels, UserControls, and TabPages receive the primary background color.</item>
    /// <item>Buttons tagged as nav-active or active receive the accent green highlight;
    /// all other buttons receive the surface color with a theme-appropriate border.</item>
    /// <item>Labels receive the standard text color and a transparent background unless
    /// tagged as status, in which case their foreground color is left unchanged to
    /// preserve custom status coloring such as green for gains or red for losses.</item>
    /// <item>TextBox, NumericUpDown, and ComboBox controls receive the surface background
    /// and standard text color; ComboBox is additionally set to flat style.</item>
    /// <item>All other controls receive the surface background and standard text color.</item>
    /// </list>
    /// </summary>
    /// <param name="control">The root control to theme recursively.</param>
    public void ApplyTheme(Control control)
    {
        var background = ColorPalette.DarkBackground;
        var surface = ColorPalette.DarkSurface;
        var text = ColorPalette.DarkText;

        if (control is Form or Panel or UserControl or TabPage)
        {
            control.BackColor = background;
        }
        else if (control is Button btn)
        {
            var navState = btn.Tag?.ToString();
            if (string.Equals(navState, "nav-active", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(navState, "active", StringComparison.OrdinalIgnoreCase))
            {
                btn.BackColor = ColorPalette.AccentGreen;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = surface;
                btn.ForeColor = text;
                btn.FlatAppearance.BorderColor = IsDarkMode ? Color.FromArgb(50, 50, 50) : Color.LightGray;
            }
        }
        else if (control is Label lbl)
        {
            if (!string.Equals(lbl.Tag?.ToString(), "status", StringComparison.OrdinalIgnoreCase))
            {
                lbl.ForeColor = text;
            }

            if (lbl.Parent is not null)
            {
                lbl.BackColor = Color.Transparent;
            }
        }
        else if (control is TextBox or NumericUpDown or ComboBox)
        {
            control.BackColor = surface;
            control.ForeColor = text;
            if (control is ComboBox combo)
                combo.FlatStyle = FlatStyle.Flat;
        }
        else
        {
            control.BackColor = surface;
            control.ForeColor = text;
        }

        foreach (Control child in control.Controls)
        {
            ApplyTheme(child);
        }
    }
}
