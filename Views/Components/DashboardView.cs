using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Models;
using Masroofy.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace Masroofy.App.Views.Components;

public sealed class DashboardView : UserControl
{
    private readonly AppController _controller;
    private readonly ThemeManager _themeManager;

    private readonly Form _hostForm;

    private readonly Label _lblBalance = new() { Font = UiStyleService.HeadingFont, ForeColor = Color.LightGray, AutoSize = true, Margin = new Padding(0, 0, 0, 5) };
    private readonly Label _lblDays = new() { Font = UiStyleService.HeadingFont, ForeColor = Color.LightGray, AutoSize = true, Margin = new Padding(0, 0, 0, 25) };
    private readonly Label _lblLimit = new() { Font = new Font("Segoe UI", 24, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 5, 0, 5), ForeColor = ColorPalette.AccentGreen };
    private readonly Label _lblTotalSpent = new() { Font = UiStyleService.NumberFont, AutoSize = true, Margin = new Padding(0, 5, 0, 5), ForeColor = Color.LightGray };
    private readonly Label _lblForecast = new() { Font = UiStyleService.BodyFont, AutoSize = true, Margin = new Padding(0, 5, 0, 30) };

    private readonly NumericUpDown _numAmount = new()
    {
        DecimalPlaces = 2,
        Maximum = 1000000,
        Width = 180,
        Height = 42,
        Font = new Font("Segoe UI", 12F)
    };

    private readonly ComboBox _cmbCategory = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 220,
        Height = 42,
        BackColor = Color.FromArgb(30, 30, 30),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI", 12F)
    };

    private readonly Button _btnQuickAdd = new()
    {
        Text = "Add Expense",
        Width = 180,
        Height = 42,
        BackColor = ColorPalette.AccentGreen,
        FlatStyle = FlatStyle.Flat,
        Font = new Font("Segoe UI Semibold", 11F),
        ForeColor = Color.White,
        Cursor = Cursors.Hand
    };

    // الهياكل اللي هتشيل الهيستوري والتحليل
    private Panel? _historyHost;
    private Panel? _breakdownHost;

    private readonly CartesianChart _weeklyTrendChart = new() { Dock = DockStyle.Fill };
    private TabControl? _pageTabControl;

    public DashboardView(AppController controller, ThemeManager theme, Form parent)
    {
        _controller = controller;
        _themeManager = theme;
        _hostForm = parent;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(18, 18, 18);
        AutoScroll = false;

        // Create tab-based navigation for two pages
        _pageTabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Padding = new Point(20, 5)
        };

        // PAGE 1: QUICK ADD EXPENSE
        var tabQuickAdd = new TabPage { Text = "➕ Quick Add Expense", BackColor = Color.FromArgb(18, 18, 18), Padding = new Padding(0) };
        CreateQuickAddExpensePage(tabQuickAdd);
        _pageTabControl.TabPages.Add(tabQuickAdd);

        // PAGE 2: DASHBOARD OVERVIEW
        var tabOverview = new TabPage { Text = "📊 Dashboard Overview", BackColor = Color.FromArgb(18, 18, 18), Padding = new Padding(0) };
        CreateDashboardOverviewPage(tabOverview);
        _pageTabControl.TabPages.Add(tabOverview);

        Controls.Add(_pageTabControl);

        // Initialize chart with empty data
        InitializeWeeklyTrendChart();

        RefreshCategories();
        RefreshData();
    }

    private void CreateQuickAddExpensePage(TabPage tab)
    {
        // 1. تعديل الـ Layout الرئيسي ليكون عمودين (نصين) بدل عمود واحد
        var pageLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2, // عمودين: شمال للإدخال - يمين للبيانات
            RowCount = 2,    // صفين: واحد للهيدر وواحد للمحتوى
            BackColor = Color.Transparent,
            Padding = new Padding(30),
            AutoSize = false
        };
        pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // النص الشمال
        pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F)); // النص اليمين
        pageLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));    // ارتفاع الهيدر
        pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));     // مساحة المحتوى

        // --- الهيدر (Header) يمتد على العمودين ---
        var headerPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var title = new Label
        {
            Text = "Add New Expense",
            Font = new Font("Segoe UI", 36, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true
        };
        var subtitle = new Label
        {
            Text = "Capture your expenses and view recent activity instantly.",
            Font = new Font("Segoe UI", 14),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point(5, 75)
        };
        headerPanel.Controls.Add(title);
        headerPanel.Controls.Add(subtitle);

        pageLayout.Controls.Add(headerPanel, 0, 0);
        pageLayout.SetColumnSpan(headerPanel, 2); // عشان الهيدر ياخد العرض كله

        // ================== الجانب الأيسر: Input Section ==================
        var leftPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 40, 0) // مسافة فاصلة عن النص اليمين
        };

        // Amount Field (من كودك الأصلي)
        var amountContainer = new Panel { Width = 500, Height = 100, BackColor = Color.Transparent };
        amountContainer.Controls.Add(new Label { Text = "💰 Amount (EGP)", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true });
        _numAmount.Width = 480;
        _numAmount.Height = 55;
        _numAmount.Font = new Font("Segoe UI", 16, FontStyle.Bold);
        _numAmount.Location = new Point(0, 40);
        amountContainer.Controls.Add(_numAmount);
        leftPanel.Controls.Add(amountContainer);

        // Category Field (من كودك الأصلي)
        var categoryContainer = new Panel { Width = 500, Height = 100, BackColor = Color.Transparent, Margin = new Padding(0, 20, 0, 0) };
        categoryContainer.Controls.Add(new Label { Text = "📂 Category", Font = new Font("Segoe UI", 15, FontStyle.Bold), ForeColor = Color.White, AutoSize = true });
        _cmbCategory.Width = 480;
        _cmbCategory.Height = 55;
        _cmbCategory.Font = new Font("Segoe UI", 13);
        _cmbCategory.Location = new Point(0, 40);
        categoryContainer.Controls.Add(_cmbCategory);
        leftPanel.Controls.Add(categoryContainer);

        // Add Button (من كودك الأصلي مع ربط التحديث)
        _btnQuickAdd.Text = "✅ CONFIRM TRANSACTION";
        _btnQuickAdd.Width = 480;
        _btnQuickAdd.Height = 65;
        _btnQuickAdd.Margin = new Padding(0, 40, 0, 0);
        _btnQuickAdd.Click += (s, e) =>
        {
            QuickAddExpense();
            UpdateSidePanels();
            UpdateWeeklyTrend();    // التعديل الجديد: تحديث الشارت في التاب التانية
            RefreshData();
        }; // نحدث اليمين فوراً
        leftPanel.Controls.Add(_btnQuickAdd);

        pageLayout.Controls.Add(leftPanel, 0, 1);

        // ================== الجانب الأيمن: Insights Section ==================
        var rightPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(40, 0, 0, 0)
        };

        // Recent Activity (History)
        rightPanel.Controls.Add(new Label { Text = "Recent Activity", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 20) });
        _historyHost = new Panel { Width = 550, Height = 250, BackColor = Color.Transparent };
        rightPanel.Controls.Add(_historyHost);

        // Spending Breakdown
        rightPanel.Controls.Add(new Label { Text = "Spending Breakdown", Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 40, 0, 20) });
        _breakdownHost = new Panel { Width = 550, Height = 250, BackColor = Color.Transparent };
        rightPanel.Controls.Add(_breakdownHost);

        pageLayout.Controls.Add(rightPanel, 1, 1);

        tab.Controls.Add(pageLayout);

        UpdateSidePanels(); // تحميل البيانات عند تشغيل التاب لأول مرة
    }

    private void UpdateSidePanels()
    {
        // التأكد إن الحاويات مش null والبيانات موجودة
        if (_historyHost == null || _breakdownHost == null || _controller.CurrentCycle == null) return;

        // 1. تحديث الهيستوري
        _historyHost.Controls.Clear();
        var lastExpenses = _controller.GetExpenses().OrderByDescending(x => x.Date).Take(3).ToList();

        int y = 0;
        foreach (var exp in lastExpenses)
        {
            var p = new Panel { Width = 530, Height = 70, BackColor = Color.FromArgb(25, 25, 25), Location = new Point(0, y) };
            UiStyleService.ApplyRoundedCorners(p, 10);

            p.Controls.Add(new Label { Text = $"{exp.Amount:N0} EGP", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.White, Location = new Point(15, 12), AutoSize = true });
            p.Controls.Add(new Label { Text = $"{exp.Category} • {exp.Date:HH:mm}", Font = new Font("Segoe UI", 10), ForeColor = Color.Gray, Location = new Point(15, 38), AutoSize = true });

            _historyHost.Controls.Add(p);
            y += 85;
        }

        // 2. تحديث الـ Breakdown
        _breakdownHost.Controls.Clear();
        var stats = _controller.GetExpenses().GroupBy(x => x.Category)
            .Select(g => new { Name = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total).Take(3).ToList();

        int yB = 0;
        foreach (var s in stats)
        {
            var lbl = new Label { Text = $"{s.Name}: {s.Total:N0} EGP", Font = new Font("Segoe UI", 13), ForeColor = Color.LightGray, Location = new Point(0, yB), AutoSize = true };
            _breakdownHost.Controls.Add(lbl); // أو _breakdownHost.Controls.Add(lbl);
            yB += 45;
        }
    }

    private void CreateDashboardOverviewPage(TabPage tab)
    {
        var mainContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(40, 30, 40, 30),
            BackColor = Color.Transparent
        };

        // --- ZONE A: HEADER (Title + Stats) ---
        var zoneA = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 40),
            Width = 1400
        };

        var title = new Label
        {
            Text = "Masroofy Dashboard",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 25)
        };
        zoneA.Controls.Add(title);

        // Stats Grid (2x2) with fixed widths
        var statsPanel = new TableLayoutPanel
        {
            ColumnCount = 2,
            RowCount = 2,
            AutoSize = false,
            Width = 1350,
            Height = 200,
            Margin = new Padding(0, 0, 0, 0)
        };
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        statsPanel.Controls.Add(CreateStatPanel("TOTAL BALANCE", _lblBalance), 0, 0);
        statsPanel.Controls.Add(CreateStatPanel("DAILY SAFE LIMIT", _lblLimit), 1, 0);
        statsPanel.Controls.Add(CreateStatPanel("DAYS REMAINING", _lblDays), 0, 1);
        statsPanel.Controls.Add(CreateStatPanel("TOTAL SPENT", _lblTotalSpent), 1, 1);

        zoneA.Controls.Add(statsPanel);
        mainContainer.Controls.Add(zoneA);

        // --- ZONE B: WEEKLY TREND CHART ---
        var zoneB = new Panel
        {
            BackColor = Color.FromArgb(24, 24, 24),
            Padding = new Padding(24),
            Width = 1350,
            Height = 420,
            Margin = new Padding(0, 40, 0, 40),
            AutoSize = false
        };
        UiStyleService.ApplyRoundedCorners(zoneB, 18);

        var chartTitle = new Label
        {
            Text = "Weekly Spending Velocity",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 18)
        };
        zoneB.Controls.Add(chartTitle);
        zoneB.Controls.Add(_weeklyTrendChart);
        mainContainer.Controls.Add(zoneB);

        tab.Controls.Add(mainContainer);
    }

    private Panel CreateStatPanel(string label, Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(10),
            BackColor = Color.FromArgb(30, 30, 30),
            AutoSize = false
        };
        UiStyleService.ApplyRoundedCorners(panel, 12);

        var lblLabel = new Label
        {
            Text = label,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            ForeColor = Color.Gray,
            AutoSize = true,
            Dock = DockStyle.Top,
            Margin = new Padding(15, 12, 15, 8)
        };
        panel.Controls.Add(lblLabel);

        valueLabel.Dock = DockStyle.Top;
        valueLabel.Margin = new Padding(15, 0, 15, 12);
        panel.Controls.Add(valueLabel);

        return panel;
    }

    // --- تحديث الأزرار والبيانات ---

    public void RefreshCategories()
    {
        _cmbCategory.Items.Clear();
        var categories = _controller.Categories.ToList();
        if (categories.Count == 0)
        {
            _cmbCategory.Items.Add("No categories available");
            _cmbCategory.SelectedIndex = 0;
            _cmbCategory.Enabled = false;
            return;
        }

        _cmbCategory.Enabled = true;
        foreach (var category in categories)
        {
            _cmbCategory.Items.Add(category);
        }

        if (_cmbCategory.Items.Count > 0)
        {
            _cmbCategory.SelectedIndex = 0;
        }
    }

    public void SyncCategories() => RefreshCategories();

    public void RefreshData()
    {
        if (_controller.CurrentCycle == null)
        {
            _lblBalance.Text = "0.00";
            _lblDays.Text = "0 Days";
            _lblLimit.Text = "0.00";
            _lblTotalSpent.Text = "0.00";
            _lblForecast.Text = "💡 Start a new cycle to view financial metrics.";
            UpdateWeeklyTrend();
            UpdateSidePanels();
            return;
        }

        var limit = _controller.SafeLimitToday();
        var balance = _controller.RemainingBalance();
        var totalSpent = _controller.GetExpenses().Sum(x => x.Amount);

        _lblBalance.Text = $"{balance:C2}";
        _lblDays.Text = $"{_controller.RemainingDays()} Days";
        _lblLimit.Text = $"{limit:C2}";
        _lblTotalSpent.Text = $"{totalSpent:C2}";

        var spentToday = _controller.GetExpenses()
                                     .Where(x => x.Date.Date == DateTime.Today)
                                     .Sum(x => x.Amount);

        _lblLimit.ForeColor = spentToday <= limit * 0.7m ? ColorPalette.SafeGreen :
                             spentToday <= limit ? ColorPalette.WarningOrange : ColorPalette.OverspentRed;

        UpdateWarningSystem();
        _lblForecast.Text = $"💡 {_controller.ForecastStatus()}";
        UpdateWeeklyTrend();
        UpdateSidePanels();
    }

    private void QuickAddExpense()
    {
        if (_controller.CurrentCycle == null)
        {
            MessageBox.Show("Please start a budget cycle before adding expenses.", "Action Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (_cmbCategory.SelectedItem is not string category || string.IsNullOrWhiteSpace(category) || !_cmbCategory.Enabled)
        {
            MessageBox.Show("Please select a valid category.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_numAmount.Value <= 0)
        {
            MessageBox.Show("Please enter an amount greater than zero.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _controller.AddExpense(_numAmount.Value, category);
        _numAmount.Value = 0;
        RefreshData();
    }

    private void UpdateWeeklyTrend()
    {
        var recentExpenses = _controller.CurrentCycle == null
            ? new List<Expense>()
            : _controller.GetExpenses().Where(x => x.Date.Date >= DateTime.Today.AddDays(-6)).ToList();

        var labels = new List<string>();
        var values = new List<double>();

        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            labels.Add(date.ToString("ddd"));
            values.Add((double)recentExpenses.Where(x => x.Date.Date == date).Sum(x => x.Amount));
        }

        _weeklyTrendChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values.ToArray(),
                Name = "Spending",
                Stroke = new SolidColorPaint(SKColors.LimeGreen, 3),
                GeometrySize = 8,
                Fill = null,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
            }
        };

        _weeklyTrendChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels.ToArray(),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        _weeklyTrendChart.YAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        // Force chart redraw
        _weeklyTrendChart.Invalidate();
    }

    private void InitializeWeeklyTrendChart()
    {
        // Initialize with empty 7-day data
        var labels = new List<string>();
        var values = new List<double>();

        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.Today.AddDays(-i);
            labels.Add(date.ToString("ddd"));
            values.Add(0);
        }

        _weeklyTrendChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = values.ToArray(),
                Name = "Spending",
                Stroke = new SolidColorPaint(SKColors.LimeGreen, 3),
                GeometrySize = 8,
                Fill = null,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top
            }
        };

        _weeklyTrendChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = labels.ToArray(),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        _weeklyTrendChart.YAxes = new Axis[]
        {
            new Axis
            {
                MinLimit = 0,
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        // Force initial chart draw
        _weeklyTrendChart.Invalidate();
    }

    private void UpdateWarningSystem()
    {
        if (_controller.CurrentCycle == null) return;

        var currentBalance = _controller.RemainingBalance();
        var threshold = _controller.CurrentCycle.TotalAllowance * 0.2m; // حد الـ 20%

        if (currentBalance <= threshold)
        {
            _themeManager.SetWarningMode(_hostForm, true);
            _lblForecast.ForeColor = ColorPalette.OverspentRed;
        }
        else
        {
            _themeManager.SetWarningMode(_hostForm, false);
            _lblForecast.ForeColor = Color.LightGray;
        }
    }
}