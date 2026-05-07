using System.Drawing.Drawing2D;
using Masroofy.App.Assets;

namespace Masroofy.App.Services;

/// <summary>
/// خدمة مسؤولة عن تنسيق عناصر الواجهة (UI Styling)
/// مثل الكروت، الأزرار، الخطوط، والزوايا الدائرية.
/// </summary>
public static class UiStyleService
{
    // =========================================
    // الخطوط القياسية المستخدمة داخل التطبيق
    // =========================================

    /// <summary>
    /// الخط الأساسي للنصوص العادية.
    /// </summary>
    public static readonly Font BodyFont =
        new("Segoe UI", 10, FontStyle.Regular);

    /// <summary>
    /// خط العناوين الفرعية.
    /// </summary>
    public static readonly Font HeadingFont =
        new("Segoe UI", 12, FontStyle.Bold);

    /// <summary>
    /// خط الأرقام الكبيرة مثل الرصيد والميزانية.
    /// </summary>
    public static readonly Font NumberFont =
        new("Segoe UI", 22, FontStyle.Bold);

    /// <summary>
    /// خط العناوين الرئيسية.
    /// </summary>
    public static readonly Font TitleFont =
        new("Segoe UI", 14, FontStyle.Bold);

    // =========================================
    // إنشاء Card Panel جاهز للتصميم
    // =========================================

    /// <summary>
    /// إنشاء Panel بشكل Card مع زوايا دائرية.
    /// </summary>
    /// <param name="size">حجم الكارت</param>
    /// <returns>Panel جاهز للاستخدام</returns>
    public static Panel CreateCard(Size size)
    {
        var panel = new Panel
        {
            Size = size,

            // لون الخلفية من الـ ColorPalette
            BackColor = ColorPalette.DarkSurface,

            // مسافات داخلية
            Padding = new Padding(18),

            // مسافات خارجية
            Margin = new Padding(0, 0, 0, 18),

            // شكل الماوس الافتراضي
            Cursor = Cursors.Default
        };

        // عند تغيير الحجم يتم إعادة رسم الزوايا الدائرية
        panel.SizeChanged += (s, e) =>
            ApplyRoundedCorners(panel, 14);

        // تطبيق الزوايا الدائرية مباشرة
        ApplyRoundedCorners(panel, 14);

        return panel;
    }

    // =========================================
    // تنسيق أزرار الـ Navigation
    // =========================================

    /// <summary>
    /// تطبيق تنسيق احترافي على أزرار القائمة الجانبية.
    /// </summary>
    public static void StyleNavButton(Button button)
    {
        // شكل الزر Flat
        button.FlatStyle = FlatStyle.Flat;

        // إزالة الحدود
        button.FlatAppearance.BorderSize = 0;

        // لون الضغط
        button.FlatAppearance.MouseDownBackColor =
            Color.FromArgb(40, 40, 40);

        // لون الـ Hover
        button.FlatAppearance.MouseOverBackColor =
            Color.FromArgb(50, 50, 50);

        // محاذاة النص لليسار
        button.TextAlign = ContentAlignment.MiddleLeft;

        // Padding داخلي
        button.Padding = new Padding(18, 0, 0, 0);

        // الخط المستخدم
        button.Font = HeadingFont;

        // لون النص الأساسي
        button.ForeColor = Color.DarkGray;

        // شكل الماوس
        button.Cursor = Cursors.Hand;

        // =====================================
        // تأثير Hover
        // =====================================

        // عند مرور الماوس يتحول النص للأبيض
        button.MouseEnter += (s, e) =>
        {
            if (button.Tag?.ToString() != "active")
            {
                button.ForeColor = Color.White;
            }
        };

        // عند خروج الماوس يعود اللون الرمادي
        button.MouseLeave += (s, e) =>
        {
            if (button.Tag?.ToString() != "active")
            {
                button.ForeColor = Color.DarkGray;
            }
        };
    }

    // =========================================
    // تطبيق زوايا دائرية على أي Control
    // =========================================

    /// <summary>
    /// تطبيق Rounded Corners على عنصر معين.
    /// </summary>
    /// <param name="control">العنصر المطلوب</param>
    /// <param name="radius">نصف قطر الزاوية</param>
    public static void ApplyRoundedCorners(Control control, int radius)
    {
        // التأكد أن الكنترول جاهز للرسم
        if (control.Width <= 0 || control.Height <= 0)
        {
            // انتظار إنشاء الـ Handle
            control.HandleCreated += (s, e) =>
                ApplyRoundedCorners(control, radius);

            return;
        }

        // إنشاء مسار رسم للزوايا الدائرية
        using var path = new GraphicsPath();

        // مستطيل بحجم الكنترول
        var rect = new Rectangle(
            0,
            0,
            control.Width,
            control.Height);

        // قطر القوس
        var d = radius * 2;

        // منع القطر من تجاوز الأبعاد
        if (d > rect.Width)
        {
            d = rect.Width;
        }

        if (d > rect.Height)
        {
            d = rect.Height;
        }

        // =====================================
        // رسم الأقواس الأربع
        // =====================================

        path.StartFigure();

        // أعلى يسار
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);

        // أعلى يمين
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);

        // أسفل يمين
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);

        // أسفل يسار
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);

        path.CloseFigure();

        // تطبيق الشكل على الكنترول
        control.Region = new Region(path);
    }

    // =========================================
    // تأثير Glow
    // =========================================

    /// <summary>
    /// إضافة تأثير Glow حول العنصر.
    /// </summary>
    /// <param name="control">العنصر</param>
    /// <param name="glowColor">لون الإضاءة</param>
    public static void ApplyGlowEffect(
        Control control,
        Color glowColor)
    {
        // حدث الرسم
        control.Paint += (s, e) =>
        {
            using var pen = new Pen(glowColor, 2);

            // رسم الإطار للداخل
            pen.Alignment = PenAlignment.Inset;

            // تنعيم الحواف
            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            // يمكن تطوير رسم Glow كامل لاحقاً
            // e.Graphics.DrawPath(pen, path);
        };
    }

    // =========================================
    // إنشاء خط Separator
    // =========================================

    /// <summary>
    /// إنشاء خط فاصل بين العناصر.
    /// </summary>
    /// <param name="width">عرض الخط</param>
    /// <returns>Control يمثل Separator</returns>
    public static Control CreateSeparator(int width)
    {
        return new Label
        {
            Width = width,

            Height = 2,

            BorderStyle = BorderStyle.None,

            // لون رمادي هادئ
            BackColor = Color.FromArgb(50, 50, 50),

            AutoSize = false,

            Text = "",

            // مسافات أعلى وأسفل
            Margin = new Padding(0, 15, 0, 15)
        };
    }
}
