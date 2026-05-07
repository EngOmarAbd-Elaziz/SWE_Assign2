using System.Drawing.Drawing2D;
using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Service responsible for UI styling and visual formatting.
/// It handles cards, buttons, fonts, rounded corners, and effects.
/// </summary>
public static class UiStyleService
{
    // =========================================
    // Standard application fonts
    // =========================================

    /// <summary>
    /// Default font for body text.
    /// </summary>
    public static readonly Font BodyFont =
        new("Segoe UI", 10, FontStyle.Regular);

    /// <summary>
    /// Font for headings and sub-titles.
    /// </summary>
    public static readonly Font HeadingFont =
        new("Segoe UI", 12, FontStyle.Bold);

    /// <summary>
    /// Font used for large numeric values (balance, budget, etc.).
    /// </summary>
    public static readonly Font NumberFont =
        new("Segoe UI", 22, FontStyle.Bold);

    /// <summary>
    /// Font for main titles.
    /// </summary>
    public static readonly Font TitleFont =
        new("Segoe UI", 14, FontStyle.Bold);

    // =========================================
    // Card creation helper
    // =========================================

    /// <summary>
    /// Creates a styled Panel that behaves like a UI Card.
    /// </summary>
    /// <param name="size">Card size</param>
    /// <returns>Styled Panel</returns>
    public static Panel CreateCard(Size size)
    {
        var panel = new Panel
        {
            Size = size,
            BackColor = ColorPalette.DarkSurface,
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 0, 18),
            Cursor = Cursors.Default
        };

        panel.SizeChanged += (s, e) =>
            ApplyRoundedCorners(panel, 14);

        ApplyRoundedCorners(panel, 14);

        return panel;
    }

    // =========================================
    // Navigation button styling
    // =========================================

    /// <summary>
    /// Applies consistent styling for navigation sidebar buttons.
    /// </summary>
    public static void StyleNavButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;

        button.FlatAppearance.MouseDownBackColor =
            Color.FromArgb(40, 40, 40);

        button.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(50, 50, 50);

        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(18, 0, 0, 0);
        button.Font = HeadingFont;
        button.ForeColor = Color.DarkGray;
        button.Cursor = Cursors.Hand;

        // Hover effects
        button.MouseEnter += (s, e) =>
        {
            if (button.Tag?.ToString() != "active")
            {
                button.ForeColor = Color.White;
            }
        };

        button.MouseLeave += (s, e) =>
        {
            if (button.Tag?.ToString() != "active")
            {
                button.ForeColor = Color.DarkGray;
            }
        };
    }

    // =========================================
    // Rounded corners utility
    // =========================================

    /// <summary>
    /// Applies rounded corners to any control.
    /// </summary>
    /// <param name="control">Target control</param>
    /// <param name="radius">Corner radius</param>
    public static void ApplyRoundedCorners(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            control.HandleCreated += (s, e) =>
                ApplyRoundedCorners(control, radius);

            return;
        }

        using var path = new GraphicsPath();

        var rect = new Rectangle(0, 0, control.Width, control.Height);
        var d = radius * 2;

        if (d > rect.Width) d = rect.Width;
        if (d > rect.Height) d = rect.Height;

        path.StartFigure();

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);

        path.CloseFigure();

        control.Region = new Region(path);
    }

    // =========================================
    // Glow effect utility
    // =========================================

    /// <summary>
    /// Applies a glow effect around a control (basic implementation).
    /// </summary>
    /// <param name="control">Target control</param>
    /// <param name="glowColor">Glow color</param>
    public static void ApplyGlowEffect(
        Control control,
        Color glowColor)
    {
        control.Paint += (s, e) =>
        {
            using var pen = new Pen(glowColor, 2)
            {
                Alignment = PenAlignment.Inset
            };

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            // Placeholder for advanced glow rendering
        };
    }

    // =========================================
    // Separator line creation
    // =========================================

    /// <summary>
    /// Creates a horizontal separator line.
    /// </summary>
    /// <param name="width">Line width</param>
    /// <returns>Separator control</returns>
    public static Control CreateSeparator(int width)
    {
        return new Label
        {
            Width = width,
            Height = 2,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(50, 50, 50),
            AutoSize = false,
            Text = "",
            Margin = new Padding(0, 15, 0, 15)
        };
    }
}
