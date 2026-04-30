using Masroofy.App.Assets;

namespace Masroofy.App.Services;

public sealed class ThemeService
{
    // الوضع الداكن هو الافتراضي بما يتناسب مع ذوقك في التصميم
    public bool IsDarkMode { get; private set; } = true;

    public void ToggleTheme(Form rootForm)
    {
        IsDarkMode = !IsDarkMode;
        ApplyTheme(rootForm);
    }

    public void ApplyTheme(Control control)
    {
        // تطبيق ثيم Dark Banking بشكل ثابت
        var background = ColorPalette.DarkBackground;
        var surface = ColorPalette.DarkSurface;
        var text = ColorPalette.DarkText;

        // 1. الحاويات الرئيسية
        if (control is Form or Panel or UserControl or TabPage)
        {
            control.BackColor = background;
        }

        // 2. الأزرار وتفاعلاتها
        else if (control is Button btn)
        {
            var navState = btn.Tag?.ToString();
            if (string.Equals(navState, "nav-active", StringComparison.OrdinalIgnoreCase) || string.Equals(navState, "active", StringComparison.OrdinalIgnoreCase))
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

        // 3. النصوص والعناوين
        else if (control is Label lbl)
        {
            // استثناء الملصقات التي تحمل تلويناً خاصاً (مثل الأخضر للمكسب أو الأحمر للخسارة)
            if (!string.Equals(lbl.Tag?.ToString(), "status", StringComparison.OrdinalIgnoreCase))
            {
                lbl.ForeColor = text;
            }

            // جعل الخلفية شفافة لتبدو متداخلة مع الحاوية الأب
            if (lbl.Parent is not null)
            {
                lbl.BackColor = Color.Transparent;
            }
        }

        // 4. حقول الإدخال (TextBox, NumericUpDown, ComboBox)
        else if (control is TextBox or NumericUpDown or ComboBox)
        {
            control.BackColor = surface;
            control.ForeColor = text;

            if (control is ComboBox combo)
                combo.FlatStyle = FlatStyle.Flat;
        }

        // 5. أي عناصر أخرى
        else
        {
            control.BackColor = surface;
            control.ForeColor = text;
        }

        // تطبيق التنسيق تكرارياً لضمان شمولية جميع العناصر داخل الواجهة
        foreach (Control child in control.Controls)
        {
            ApplyTheme(child);
        }
    }
}