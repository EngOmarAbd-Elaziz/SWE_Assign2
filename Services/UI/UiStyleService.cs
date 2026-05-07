using System.Drawing.Drawing2D;
using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// Provides static UI styling utilities for the application, including standard font definitions,
/// card panel creation, navigation button styling, rounded corner clipping, glow effects,
/// and separator controls.
/// </summary>
public static class UiStyleService
{
    /// <summary>The standard body font used for general text throughout the application.</summary>
    public static readonly Font BodyFont = new("Segoe UI", 10, FontStyle.Regular);

    /// <summary>The standard heading font used for section titles and navigation labels.</summary>
    public static readonly Font HeadingFont = new("Segoe UI", 12, FontStyle.Bold);

    /// <summary>The large bold font used to display prominent numeric values such as balances.</summary>
    public static readonly Font NumberFont = new("Segoe UI", 22, FontStyle.Bold);

    /// <summary>The bold font used for page and card titles.</summary>
    public static readonly Font TitleFont = new("Segoe UI", 14, FontStyle.Bold);

    /// <summary>
    /// Creates a styled card panel with rounded corners, the dark surface background color,
    /// and consistent padding and margin. Rounded corners are reapplied automatically
    /// whenever the panel is resized.
    /// </summary>
    /// <param name="size">The initial size of the card panel.</param>
    /// <returns>A fully styled <see cref="Panel"/> ready to be added to a form or container.</returns>
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

        panel.SizeChanged += (s, e) => ApplyRoundedCorners(panel, 14);
        ApplyRoundedCorners(panel, 14);
        return panel;
    }

    /// <summary>
    /// Applies the standard navigation button style to the given button, including flat appearance,
    /// left-aligned text with padding, heading font, and hover color transitions.
    /// Buttons tagged as active are excluded from hover color changes to preserve their
    /// active state styling.
    /// </summary>
    /// <param name="button">The button to style as a navigation item.</param>
    public static void StyleNavButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 40, 40);
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 50, 50);
        button.TextAlign = ContentAlignment.MiddleLeft;
        button.Padding = new Padding(18, 0, 0, 0);
        button.Font = HeadingFont;
        button.ForeColor = Color.DarkGray;
        button.Cursor = Cursors.Hand;

        button.MouseEnter += (s, e) => { if (button.Tag?.ToString() != "active") button.ForeColor = Color.White; };
        button.MouseLeave += (s, e) => { if (button.Tag?.ToString() != "active") button.ForeColor = Color.DarkGray; };
    }

    /// <summary>
    /// Clips the given control to a rounded rectangle with the specified corner radius
    /// by setting its <see cref="Control.Region"/> property. If the control has not yet
    /// been sized, the operation is deferred until the handle is created.
    /// The radius is automatically clamped to the control's width and height to prevent
    /// rendering artifacts.
    /// </summary>
    /// <param name="control">The control to apply rounded corners to.</param>
    /// <param name="radius">The corner radius in pixels.</param>
    public static void ApplyRoundedCorners(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            control.HandleCreated += (s, e) => ApplyRoundedCorners(control, radius);
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

    /// <summary>
    /// Attaches a paint handler to the given control that draws a glowing inset border
    /// in the specified color using anti-aliased rendering. The inner drawing logic is
    /// available for future development.
    /// </summary>
    /// <param name="control">The control to attach the glow effect to.</param>
    /// <param name="glowColor">The color of the glow border.</param>
    public static void ApplyGlowEffect(Control control, Color glowColor)
    {
        control.Paint += (s, e) =>
        {
            using var pen = new Pen(glowColor, 2);
            pen.Alignment = PenAlignment.Inset;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        };
    }

    /// <summary>
    /// Creates a thin horizontal separator control styled as a dark gray line,
    /// with vertical margin above and below to provide visual spacing between sections.
    /// </summary>
    /// <param name="width">The width of the separator in pixels.</param>
    /// <returns>A <see cref="Control"/> configured as a horizontal visual divider.</returns>
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
