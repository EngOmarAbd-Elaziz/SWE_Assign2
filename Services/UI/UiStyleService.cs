using System.Drawing.Drawing2D;
using Masroofy.App.Assets;

namespace Masroofy.App.Services;

public static class UiStyleService
{
    // تعريف الخطوط القياسية للتطبيق
    public static readonly Font BodyFont = new("Segoe UI", 10, FontStyle.Regular);
    public static readonly Font HeadingFont = new("Segoe UI", 12, FontStyle.Bold);
    public static readonly Font NumberFont = new("Segoe UI", 22, FontStyle.Bold);
    public static readonly Font TitleFont = new("Segoe UI", 14, FontStyle.Bold);

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

        // ربط حدث تغيير الحجم لضمان بقاء الزوايا دائرية
        panel.SizeChanged += (s, e) => ApplyRoundedCorners(panel, 14);
        ApplyRoundedCorners(panel, 14);

        return panel;
    }

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

        // تأثير تغيير اللون عند التحويم (Hover)
        button.MouseEnter += (s, e) => { if (button.Tag?.ToString() != "active") button.ForeColor = Color.White; };
        button.MouseLeave += (s, e) => { if (button.Tag?.ToString() != "active") button.ForeColor = Color.DarkGray; };
    }

    public static void ApplyRoundedCorners(Control control, int radius)
    {
        if (control.Width <= 0 || control.Height <= 0)
        {
            // انتظار إنشاء المقبض (Handle) إذا لم يكن الكنترول جاهزاً
            control.HandleCreated += (s, e) => ApplyRoundedCorners(control, radius);
            return;
        }

        // رسم المسار المنحني بدقة عالية
        using var path = new GraphicsPath();
        var rect = new Rectangle(0, 0, control.Width, control.Height);
        var d = radius * 2;

        // التحقق من أن القطر لا يتجاوز أبعاد العنصر
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

    // إضافة لمسة "Glow" للـ Buttons أو الـ Panels
    public static void ApplyGlowEffect(Control control, Color glowColor)
    {
        // هذه الدالة يمكن استخدامها لاحقاً لإضافة ظلال مضيئة حول الكروت
        control.Paint += (s, e) =>
        {
            using var pen = new Pen(glowColor, 2);
            pen.Alignment = PenAlignment.Inset;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            // رسم إطار مضيء بسيط
            // e.Graphics.DrawPath(pen, ...); // متاح للتطوير لاحقاً
        };
    }
    public static Control CreateSeparator(int width)
    {
        return new Label
        {
            Width = width,
            Height = 2,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(50, 50, 50), // لون رمادي غامق هادي
            AutoSize = false,
            Text = "",
            Margin = new Padding(0, 15, 0, 15) // مسافات فوق وتحت الخط
        };
    }
}