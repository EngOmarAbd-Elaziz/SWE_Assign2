using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

public sealed class TutorialView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;
    private readonly Label _lblTip = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI", 16, FontStyle.Italic), // تكبير الخط
        ForeColor = Color.LightGray,
        Margin = new Padding(0, 30, 0, 0)
    };

    public TutorialView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true;

        Reload(); // استدعاء الميثود الموحدة لبناء الصفحة
    }

    public void Reload()
    {
        Controls.Clear();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = false,
            Padding = new Padding(40),
            BackColor = Color.Transparent
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var header = new Label
        {
            Text = "FINANCIAL GUIDE & TUTORIAL",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 30)
        };

        var iconGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 30)
        };
        iconGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        iconGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        iconGrid.Controls.Add(CreateIconCard("ASSETS (Lending)", "Money you own and expect to return. It adds to your wealth.", ColorPalette.AccentGreen), 0, 0);
        iconGrid.Controls.Add(CreateIconCard("LIABILITIES (Debt)", "Money you owe others. It must be deducted from your net worth.", ColorPalette.WarningOrange), 1, 0);

        var tutorialsGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            RowCount = 3
        };
        tutorialsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        tutorialsGrid.Controls.Add(CreateTutorialCard(
            "Safe Limit Logic",
            "If your budget is 3,000 EGP and you have 30 days left, Masroofy sets your limit at 100 EGP/day.",
            "Calculation: Remaining Balance / Days Left = Your Daily Allowance.",
            "3,000", "100", "30 Days Left"), 0, 0);
        tutorialsGrid.Controls.Add(CreateTutorialCard(
            "Spending Velocity",
            "Velocity compares your real-time spending against your planned daily limit.",
            "If your velocity turns RED, you are spending faster than your budget allows.",
            "High Velocity", "Low Balance", "Critical Risk"), 0, 1);
        UpdateTips();
        tutorialsGrid.Controls.Add(_lblTip, 0, 2);

        mainLayout.Controls.Add(header, 0, 0);
        mainLayout.Controls.Add(iconGrid, 0, 1);
        mainLayout.Controls.Add(tutorialsGrid, 0, 2);

        Controls.Add(mainLayout);
    }

    private Panel CreateTutorialCard(string title, string body, string formula, string statA, string statB, string statC)
    {
        var card = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = Color.FromArgb(28, 28, 28),
            Margin = new Padding(0, 0, 0, 30),
            Padding = new Padding(30)
        };
        UiStyleService.ApplyRoundedCorners(card, 20);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var titleLabel = new Label { Text = title, Font = new Font("Segoe UI", 22, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        var bodyLabel = new Label { Text = body, Font = new Font("Segoe UI", 14F), ForeColor = Color.LightGray, AutoSize = true, MaximumSize = new Size(980, 0), Margin = new Padding(0, 0, 0, 10) };
        var formulaLabel = new Label { Text = formula, Font = new Font("Segoe UI", 13F, FontStyle.Italic), ForeColor = ColorPalette.AccentGreen, AutoSize = true, Margin = new Padding(0, 0, 0, 20) };

        var statPanel = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(18),
            Margin = new Padding(0, 0, 0, 10)
        };
        UiStyleService.ApplyRoundedCorners(statPanel, 16);
        var statsLabel = new Label
        {
            Text = $"📌 {statA}  ➔  {statB}  ➔  {statC}",
            Font = new Font("Consolas", 16, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true
        };
        statPanel.Controls.Add(statsLabel);

        layout.Controls.Add(titleLabel);
        layout.Controls.Add(bodyLabel);
        layout.Controls.Add(formulaLabel);
        layout.Controls.Add(statPanel);

        card.Controls.Add(layout);
        return card;
    }

    private Control CreateIconRow()
    {
        var row = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 0, 30) };
        row.Controls.Add(CreateIconCard("ASSETS (Lending)", "Money you own and expect to return. It adds to your wealth.", ColorPalette.AccentGreen));
        row.Controls.Add(CreateIconCard("LIABILITIES (Debt)", "Money you owe others. It must be deducted from your net worth.", ColorPalette.WarningOrange));
        return row;
    }

    private Control CreateIconCard(string title, string description, Color accent)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            BackColor = Color.FromArgb(35, 35, 35),
            Margin = new Padding(0, 0, 20, 0),
            Padding = new Padding(20)
        };
        UiStyleService.ApplyRoundedCorners(card, 18);

        var indicator = new Panel { Size = new Size(10, 0), BackColor = accent, Dock = DockStyle.Left, Margin = new Padding(0, 0, 20, 0) };
        var contentGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true
        };
        contentGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var titleLabel = new Label { Text = title, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        var descLabel = new Label { Text = description, Font = new Font("Segoe UI", 12F), ForeColor = Color.Silver, AutoSize = true, MaximumSize = new Size(520, 0) };

        contentGrid.Controls.Add(titleLabel);
        contentGrid.Controls.Add(descLabel);
        card.Controls.Add(indicator);
        card.Controls.Add(contentGrid);

        return card;
    }

    private void UpdateTips()
    {
        if (_controller.CurrentCycle == null)
        {
            _lblTip.Text = "💡 TIP: Start your first budget cycle to see dynamic spending analysis here.";
            return;
        }

        var balance = _controller.RemainingBalance();
        var allowance = _controller.CurrentCycle.TotalAllowance;
        var usedRatio = allowance <= 0 ? 0 : 1 - (balance / allowance);

        _lblTip.Text = "💡 SMART ANALYSIS: " + usedRatio switch
        {
            <= 0.4m => "You are in the Safe Zone. Your current pace ensures you won't run out of cash.",
            <= 0.7m => "Moderate spending detected. Consider cutting down on non-essential categories.",
            <= 0.9m => "Warning: You have consumed most of your budget. Emergency spending only!",
            _ => "Critical Limit: You are operating on borrowed time. Avoid any new expenses."
        };
    }
}