using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

namespace Masroofy.App.Views.Components;

public sealed class AnalyticsView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;

    // Charts
    private readonly CartesianChart _barChart = new() { Dock = DockStyle.Fill };
    private readonly PieChart _pieChart = new() { Dock = DockStyle.Fill };

    // Summary Labels
    private readonly Label _lblVelocity = new() { AutoSize = false, Width = 750, Height = 35, ForeColor = Color.White };
    private readonly Label _lblTopCategory = new() { AutoSize = false, Width = 750, Height = 35, ForeColor = Color.White };
    private readonly Label _lblHealth = new() { AutoSize = false, Width = 750, Height = 35, ForeColor = Color.White };
    private readonly Label _lblInsight = new() { AutoSize = false, Width = 750, Height = 60, ForeColor = ColorPalette.AccentGreen };

    // Side Cards Labels (Dynamic)
    private readonly Label _lblPatternDesc = new() { ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill };
    private readonly Label _lblHealthDesc = new() { ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill };

    // Health Bar
    private readonly Panel _healthBarBackground = new() { BackColor = Color.FromArgb(45, 45, 45), Height = 18, Width = 650 };
    private readonly Panel _healthBarFill = new() { BackColor = ColorPalette.SafeGreen, Width = 0, Height = 18 };

    public AnalyticsView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;
        this.Dock = DockStyle.Fill;
        this.BackColor = ColorPalette.DarkBackground;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(20)
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 260F)); // Summary & Insights
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Charts

        // 1. Header
        var header = new Label
        {
            Text = "Financial Intelligence",
            Font = new Font("Segoe UI", 24, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        mainLayout.Controls.Add(header, 0, 0);
        mainLayout.SetColumnSpan(header, 2);

        // 2. Left Panel (Dynamic Cards)
        var leftPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        leftPanel.Controls.Add(CreateDynamicCard("Patterns", _lblPatternDesc));
        leftPanel.Controls.Add(CreateDynamicCard("Health Status", _lblHealthDesc));
        mainLayout.Controls.Add(leftPanel, 0, 1);

        // 3. Right Panel (Summary & Progress Bar)
        var rightSummaryPanel = new Panel { Dock = DockStyle.Fill };
        var summaryCard = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(780, 190),
            BackColor = ColorPalette.DarkSurface,
            Padding = new Padding(20)
        };
        UiStyleService.ApplyRoundedCorners(summaryCard, 15);

        int startY = 15;
        foreach (var lbl in new[] { _lblVelocity, _lblTopCategory, _lblHealth, _lblInsight })
        {
            lbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lbl.Location = new Point(25, startY);
            summaryCard.Controls.Add(lbl);
            startY += 40;
        }
        _lblInsight.Font = new Font("Segoe UI", 10, FontStyle.Italic);

        _healthBarBackground.Location = new Point(0, 205);
        _healthBarBackground.Controls.Add(_healthBarFill);
        rightSummaryPanel.Controls.Add(summaryCard);
        rightSummaryPanel.Controls.Add(_healthBarBackground);
        mainLayout.Controls.Add(rightSummaryPanel, 1, 1);

        // 4. Charts Container (Pie + Bar)
        var chartsContainer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
        chartsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        chartsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        chartsContainer.Controls.Add(_pieChart, 0, 0);
        chartsContainer.Controls.Add(_barChart, 1, 0);

        mainLayout.Controls.Add(chartsContainer, 0, 2);
        mainLayout.SetColumnSpan(chartsContainer, 2);

        this.Controls.Add(mainLayout);
        this.Load += (s, e) => Reload();
    }

    public void Reload()
    {
        if (_controller.CurrentCycle == null)
        {
            ResetToDefault();
            return;
        }

        var expenses = _controller.GetExpenses();
        if (expenses == null || !expenses.Any())
        {
            ResetToDefault();
            return;
        }

        double totalAllowance = (double)_controller.CurrentCycle.TotalAllowance;
        double totalSpent = (double)expenses.Sum(x => x.Amount);

        // --- الحسابات المالية ---
        var daysPassed = Math.Max(1, (DateTime.Today - _controller.CurrentCycle.StartDate.Date).Days + 1);
        double actualVelocity = totalSpent / daysPassed;
        double plannedVelocity = totalAllowance / Math.Max(1, (_controller.CurrentCycle.EndDate.Date - _controller.CurrentCycle.StartDate.Date).Days + 1);

        _lblVelocity.Text = $"Velocity: {actualVelocity:C}/day vs Planned {plannedVelocity:C}/day";

        var categoryTotals = expenses.GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Total = (double)g.Sum(x => x.Amount) })
            .ToList();

        var topCat = categoryTotals.OrderByDescending(x => x.Total).First();
        _lblTopCategory.Text = $"Top Spending: {topCat.Category} ({(totalSpent > 0 ? (topCat.Total / totalSpent) : 0):P0})";

        double usedPercent = totalAllowance <= 0 ? 0 : (totalSpent / totalAllowance);
        _lblHealth.Text = $"Budget Health: {usedPercent:P0} consumed";

        // --- تحديث الكروت الجانبية والـ Insights ---
        UpdateDynamicAnalytics(actualVelocity, plannedVelocity, topCat.Category, usedPercent, categoryTotals.Count);

        // --- تحديث الـ Health Bar ---
        _healthBarFill.Width = (int)(_healthBarBackground.Width * Math.Min(1.0, usedPercent));
        _healthBarFill.BackColor = usedPercent > 0.9 ? Color.Red : ColorPalette.AccentGreen;

        // --- تحديث الـ PieChart (توزيع النسب) ---
        double totalForPercentage = categoryTotals.Sum(x => x.Total);
        _pieChart.Series = categoryTotals.Select(c =>
        {
            var percentage = totalForPercentage > 0 ? (c.Total / totalForPercentage * 100) : 0;
            return new PieSeries<double>
            {
                Values = new[] { c.Total },
                Name = c.Category,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 14,
                DataLabelsFormatter = (chartPoint) => percentage.ToString("N1") + "%"
            };
        }).ToArray();

        // --- تحديث الـ BarChart (النمو السعري) ---
        _barChart.Series = new ISeries[] {
            new ColumnSeries<double> {
                Values = categoryTotals.Select(x => x.Total).ToArray(),
                Name = "Expenses",
                MaxBarWidth = 45,
                DataLabelsPosition = LiveChartsCore.Measure.DataLabelsPosition.Top,
                DataLabelsPaint = new SolidColorPaint(SKColors.White)
            }
        };

        _barChart.XAxes = new Axis[] {
            new Axis {
                Labels = categoryTotals.Select(x => x.Category).ToArray(),
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 12
            }
        };

        _barChart.YAxes = new Axis[] {
            new Axis {
                MinLimit = 0,
                LabelsPaint = new SolidColorPaint(SKColors.Gray),
                TextSize = 11
            }
        };

        // Force charts to redraw immediately
        _pieChart.Invalidate();
        _barChart.Invalidate();
    }

    private void UpdateDynamicAnalytics(double actual, double planned, string topCat, double health, int catCount)
    {
        // 1. تحديث كارت الـ Patterns
        if (actual > planned)
        {
            _lblPatternDesc.Text = $"Warning: Spending {actual - planned:C} more than planned daily limit.";
            _lblPatternDesc.ForeColor = Color.Orange;
        }
        else
        {
            _lblPatternDesc.Text = "Healthy: Spending is currently below the daily planned velocity.";
            _lblPatternDesc.ForeColor = Color.LightGreen;
        }

        // 2. تحديث كارت الـ Health Status
        if (health > 0.8)
        {
            _lblHealthDesc.Text = $"Critical: {health:P0} of budget is gone. Stop non-essential spending.";
            _lblHealthDesc.ForeColor = Color.IndianRed;
        }
        else
        {
            _lblHealthDesc.Text = $"Stable: You still have {1.0 - health:P0} of your budget available.";
            _lblHealthDesc.ForeColor = Color.LightGray;
        }

        // 3. تحديث الـ Insights الرئيسية
        if (health > 1.0)
        {
            _lblInsight.Text = $"Insight: Budget Exceeded! Most funds went to '{topCat}'. Analyze your history for leaks.";
            _lblInsight.ForeColor = Color.IndianRed;
        }
        else if (actual > planned * 1.5)
        {
            _lblInsight.Text = "Insight: Aggressive spending detected. You are 50% faster than your target speed.";
            _lblInsight.ForeColor = Color.Orange;
        }
        else
        {
            _lblInsight.Text = "Insight: All systems clear. You are managing your budget effectively.";
            _lblInsight.ForeColor = ColorPalette.AccentGreen;
        }
    }

    private void ResetToDefault()
    {
        _lblVelocity.Text = "Velocity: $0.00/day";
        _lblTopCategory.Text = "Top Spending: None";
        _lblHealth.Text = "Budget Health: 0% used";
        _lblInsight.Text = "Insight: Add expenses to start financial analysis.";
        _lblInsight.ForeColor = Color.Gray;

        _lblPatternDesc.Text = "Tracking spending velocity vs goals.";
        _lblPatternDesc.ForeColor = Color.LightGray;
        _lblHealthDesc.Text = "Real-time budget exhaustion limits.";
        _lblHealthDesc.ForeColor = Color.LightGray;

        _healthBarFill.Width = 0;
        _barChart.Series = Array.Empty<ISeries>();
        _pieChart.Series = Array.Empty<ISeries>();
    }

    private Control CreateDynamicCard(string title, Label contentLabel)
    {
        var card = new Panel { Width = 280, Height = 110, BackColor = ColorPalette.DarkSurface, Margin = new Padding(0, 0, 0, 10), Padding = new Padding(15) };
        UiStyleService.ApplyRoundedCorners(card, 12);
        var t = new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), ForeColor = Color.White, Dock = DockStyle.Top, Height = 25 };
        card.Controls.Add(contentLabel);
        card.Controls.Add(t);
        return card;
    }
}