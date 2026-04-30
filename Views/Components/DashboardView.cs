using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

public sealed class DashboardView : UserControl
{
    private readonly AppController _controller;
    private readonly ThemeManager _themeManager;
    private readonly TableLayoutPanel _categoriesFlow = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = false,
        Padding = new Padding(30),
        BackColor = Color.Transparent,
        ColumnCount = 4, // افتراضي، سيتم تعديله ديناميكياً
        RowCount = 1
    };
    private readonly Form _hostForm;

    // تعريف العناصر مع أحجام خطوط "Fintech-Style"
    private readonly Label _lblBalance = new() { Font = UiStyleService.HeadingFont, ForeColor = Color.LightGray, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
    private readonly Label _lblDays = new() { Font = UiStyleService.HeadingFont, ForeColor = Color.LightGray, AutoSize = true, Margin = new Padding(0, 0, 0, 25) };
    private readonly Label _lblLimit = new() { Font = UiStyleService.NumberFont, AutoSize = true, Margin = new Padding(0, 5, 0, 5), ForeColor = ColorPalette.AccentGreen };
    private readonly Label _lblForecast = new() { Font = UiStyleService.BodyFont, AutoSize = true, Margin = new Padding(0, 5, 0, 30) };

    private bool _lowBalanceAlertShown;

    public DashboardView(AppController controller, ThemeManager theme, Form parent)
    {
        _controller = controller;
        _themeManager = theme;
        _hostForm = parent;
        Dock = DockStyle.Fill; // لضمان ملء المساحة الشاسعة
        BackColor = Color.FromArgb(12, 12, 12); // اللون البنكي الداكن

        var title = new Label
        {
            Text = "MY FINANCES",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(40, 40, 40, 20)
        };

        var statsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = true,
            Margin = new Padding(40, 0, 40, 20)
        };
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        statsPanel.Controls.Add(new Label
        {
            Text = "TOTAL BALANCE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Dock = DockStyle.Fill
        }, 0, 0);
        statsPanel.Controls.Add(_lblBalance, 1, 0);
        statsPanel.Controls.Add(new Label
        {
            Text = "DAYS REMAINING",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Dock = DockStyle.Fill
        }, 0, 1);
        statsPanel.Controls.Add(_lblDays, 1, 1);

        var heroPanel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(40, 0, 40, 20) };
        var heroLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = true
        };
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        heroLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        heroLayout.Controls.Add(new Label
        {
            Text = "Daily Safe Spending Limit",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.Gray,
            AutoSize = true,
            Dock = DockStyle.Fill
        }, 0, 0);
        heroLayout.Controls.Add(_lblLimit, 0, 1);
        heroLayout.Controls.Add(_lblForecast, 0, 2);
        heroPanel.Controls.Add(heroLayout);

        var sectionDivider = new Panel { Height = 1, Dock = DockStyle.Top, BackColor = Color.FromArgb(50, 50, 50), Margin = new Padding(40, 0, 40, 20) };

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            AutoSize = false
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        mainLayout.Controls.Add(title, 0, 0);
        mainLayout.Controls.Add(statsPanel, 0, 1);
        mainLayout.Controls.Add(heroPanel, 0, 2);
        mainLayout.Controls.Add(sectionDivider, 0, 3);
        mainLayout.Controls.Add(_categoriesFlow, 0, 4);

        Controls.Add(mainLayout);
        RefreshCategories();
        RefreshData();
    }

    // --- تحديث الأزرار والبيانات ---

    public void RefreshCategories()
    {
        _categoriesFlow.Controls.Clear();
        var categories = _controller.Categories.ToList();
        if (categories.Count == 0) return;

        int columns = Math.Min(4, categories.Count);
        int rows = (int)Math.Ceiling((double)categories.Count / columns);

        _categoriesFlow.ColumnCount = columns;
        _categoriesFlow.RowCount = rows;

        // إزالة الأنماط القديمة
        _categoriesFlow.ColumnStyles.Clear();
        _categoriesFlow.RowStyles.Clear();

        for (int i = 0; i < columns; i++)
        {
            _categoriesFlow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / columns));
        }
        for (int i = 0; i < rows; i++)
        {
            _categoriesFlow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / rows));
        }

        int index = 0;
        foreach (var cat in categories)
        {
            var btn = new Button
            {
                Text = cat.ToUpper(),
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 15, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(29, 29, 29),
                ForeColor = Color.White,
                Margin = new Padding(10),
                Cursor = Cursors.Hand
            };
            btn.MouseEnter += (s, e) => btn.BackColor = ColorPalette.AccentGreen;
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(29, 29, 29);
            btn.Click += (s, e) => SelectCategoryAmount(cat);

            int row = index / columns;
            int col = index % columns;
            _categoriesFlow.Controls.Add(btn, col, row);
            index++;
        }
    }

    public void SyncCategories() => RefreshCategories();

    public void RefreshData()
    {
        if (_controller.CurrentCycle == null) return;

        var limit = _controller.SafeLimitToday();
        var balance = _controller.RemainingBalance();

        _lblBalance.Text = $"TOTAL BALANCE AVAILABLE:  {balance:C2}";
        _lblDays.Text = $"DAYS REMAINING IN CYCLE:  {_controller.RemainingDays()} Days";
        _lblLimit.Text = $"{limit:C2}";

        // تحديث لون الـ Limit بناءً على صرف اليوم
        var spentToday = _controller.GetExpenses()
                                     .Where(x => x.Date.Date == DateTime.Today)
                                     .Sum(x => x.Amount);

        _lblLimit.ForeColor = spentToday <= limit * 0.7m ? ColorPalette.SafeGreen :
                             spentToday <= limit ? ColorPalette.WarningOrange : ColorPalette.OverspentRed;

        UpdateWarningSystem();
        _lblForecast.Text = $"💡 {_controller.ForecastStatus()}";
    }

    private void BuildCategoryButtons()
    {
        RefreshCategories();
    }

    private Button CreateCategoryButton(string category, bool isOther = false, List<string>? submenu = null)
    {
        var btn = new Button
        {
            Text = category.ToUpper(),
            Width = 150,
            Height = 150,
            Margin = new Padding(12),
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(40, 40, 43),
            ForeColor = Color.White,
            Cursor = Cursors.Hand
        };

        btn.FlatAppearance.BorderSize = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 65);
        UiStyleService.ApplyRoundedCorners(btn, 15);

        if (isOther && submenu != null && submenu.Any())
        {
            var menu = new ContextMenuStrip();
            foreach (var item in submenu)
            {
                var menuItem = new ToolStripMenuItem(item) { ForeColor = Color.White, BackColor = Color.FromArgb(28, 28, 28) };
                menuItem.Click += (_, _) => SelectCategoryAmount(item);
                menu.Items.Add(menuItem);
            }

            btn.Click += (_, _) => menu.Show(btn, new Point(0, btn.Height));
        }
        else
        {
            btn.Click += (_, _) => SelectCategoryAmount(category);
        }

        return btn;
    }

    private void SelectCategoryAmount(string category)
    {
        if (_controller.CurrentCycle == null) return;

        var amount = _controller.SafeLimitToday();
        if (amount <= 0)
        {
            MessageBox.Show("Please active budget cycle first.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _controller.AddExpense(amount, category);
        RefreshData();
    }

    private void UpdateWarningSystem()
    {
        if (_controller.CurrentCycle == null) return;

        var totalAllowance = _controller.CurrentCycle.TotalAllowance;
        var currentBalance = _controller.RemainingBalance();
        var threshold = totalAllowance * 0.2m; // حد الـ 20%

        if (currentBalance <= threshold)
        {
            _themeManager.SetWarningMode(_hostForm, true);
            _lblForecast.ForeColor = ColorPalette.OverspentRed;

            if (!_lowBalanceAlertShown)
            {
                _lowBalanceAlertShown = true;
                MessageBox.Show("تنبيه: الرصيد المتبقي أقل من 20%! يرجى مراجعة مصروفاتك.",
                                "تحذير مالي", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        else
        {
            // في حال زيادة الرصيد (مثلاً عن طريق تسوية سلفية من الأدمين)
            _themeManager.SetWarningMode(_hostForm, false);
            _lowBalanceAlertShown = false;
            _lblForecast.ForeColor = Color.LightGray;
        }
    }
}