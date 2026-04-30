using Masroofy.App.Assets;

namespace Masroofy.App.Services;

public sealed class ThemeManager
{
    private readonly ThemeService _inner = new();
    public bool WarningMode { get; private set; }

    public bool IsDarkMode => _inner.IsDarkMode;

    // تبديل الثيم العام (Light/Dark)
    public void ToggleTheme(Form rootForm) 
    {
        _inner.ToggleTheme(rootForm);
        if (WarningMode) ApplyWarningTint(rootForm);
    }

    // تطبيق الثيم على أي عنصر مع مراعاة وضع التحذير
    public void ApplyTheme(Control control)
    {
        _inner.ApplyTheme(control);
        
        if (WarningMode)
        {
            ApplyWarningTint(control);
        }
    }

    // تفعيل أو تعطيل وضع التحذير (يستخدم عند تخطي الميزانية مثلاً)
    public void SetWarningMode(Control root, bool enabled)
    {
        WarningMode = enabled;
        // إعادة تطبيق الثيم بالكامل لتحديث الألوان بصرياً
        ApplyTheme(root);
    }

    private static void ApplyWarningTint(Control control)
    {
        // استخدام درجات ألوان متناسقة مع وضع التحذير (Dark Red Palette)
        var warningBg = Color.FromArgb(45, 20, 20); // خلفية حمراء داكنة جداً
        var warningCard = Color.FromArgb(60, 30, 30); // لون الكروت في وضع التحذير

        if (control is Form or UserControl)
        {
            control.BackColor = warningBg;
        }
        else if (control is Panel && control.Name != "PanelContent") 
        {
            // تلوين الكروت والبانلز بصبغة حمراء خفيفة
            control.BackColor = warningCard;
        }
        else if (control is Label lbl)
        {
            // التأكد من أن النصوص الهامة تظهر بلون فاتح في وضع التحذير
            if (lbl.ForeColor != Color.SpringGreen) // لا نغير لون العملات أو الأرقام الإيجابية
                lbl.ForeColor = Color.FromArgb(255, 200, 200);
        }

        // تطبيق التغييرات على جميع العناصر الأبناء بشكل تكراري (Recursively)
        foreach (Control child in control.Controls)
        {
            ApplyWarningTint(child);
        }
    }
}