using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.Painting;

namespace Masroofy.App.Views.Components;

/// <summary>
/// Analytics dashboard view responsible for displaying financial insights:
/// - Spending velocity analysis
/// - Category distribution (Pie chart)
/// - Expense comparison (Bar chart)
/// - Budget health monitoring
/// - AI-like insights and warnings
/// </summary>
public sealed class AnalyticsView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;

    // =========================================
    // CHARTS
    // =========================================

    /// <summary>
    /// Bar chart showing spending per category.
    /// </summary>
    private readonly CartesianChart _barChart = new() { Dock = DockStyle.Fill };

    /// <summary>
    /// Pie chart showing category distribution.
    /// </summary>
    private readonly PieChart _pieChart = new() { Dock = DockStyle.Fill };

    // =========================================
    // SUMMARY LABELS
    // =========================================

    private readonly Label _lblVelocity = new()
    {
        AutoSize = false,
        Width = 750,
        Height = 35,
        ForeColor = Color.White
    };

    private readonly Label _lblTopCategory = new()
    {
        AutoSize = false,
        Width = 750,
        Height = 35,
        ForeColor = Color.White
    };

    private readonly Label _lblHealth = new()
    {
        AutoSize = false,
        Width = 750,
        Height = 35,
        ForeColor = Color.White
    };

    private readonly Label _lblInsight = new()
    {
        AutoSize = false,
        Width = 750,
        Height = 60,
        ForeColor = ColorPalette.AccentGreen
    };

    // =========================================
    // SIDE CARDS (DYNAMIC INSIGHTS)
    // =========================================

    private readonly Label _lblPatternDesc =
        new() { ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill };

    private readonly Label _lblHealthDesc =
        new() { ForeColor = Color.LightGray, Font = new Font("Segoe UI", 9), Dock = DockStyle.Fill };

    // =========================================
    // HEALTH BAR
    // =========================================

    private readonly Panel _healthBarBackground =
        new() { BackColor = Color.FromArgb(45, 45, 45), Height = 18, Width = 650 };

    private readonly Panel _healthBarFill =
        new() { BackColor = ColorPalette.SafeGreen, Width = 0, Height = 18 };

    /// <summary>
    /// Initializes a new instance of AnalyticsView.
    /// </summary>
    /// <param name="controller">Main application controller</param>
    /// <param name="dashboard">Dashboard reference for sync updates</param>
    public AnalyticsView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(20)
        };

        // Layout structure
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 260F)); // Summary
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));  // Charts

        // =========================================
        // HEADER
        // =========================================

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

        // =========================================
        // LEFT PANEL (DYNAMIC INSIGHTS CARDS)
        // =========================================

        var leftPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        leftPanel.Controls.Add(CreateDynamicCard("Patterns", _lblPatternDesc));
        leftPanel.Controls.Add(CreateDynamicCard("Health Status", _lblHealthDesc));

        mainLayout.Controls.Add(leftPanel, 0, 1);

        // =========================================
        // RIGHT PANEL (SUMMARY)
        // =========================================

        var rightSummaryPanel = new Panel { Dock = DockStyle.Fill };

        var summaryCard = new Panel
        {
            Size = new Size(780, 190),
            BackColor = ColorPalette.DarkSurface,
            Padding = new Padding(20)
        };

        UiStyleService.ApplyRoundedCorners(summaryCard, 15);

        int y = 15;
        foreach (var lbl in new[] { _lblVelocity, _lblTopCategory, _lblHealth, _lblInsight })
        {
            lbl.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lbl.Location = new Point(25, y);
            summaryCard.Controls.Add(lbl);
            y += 40;
        }

        _lblInsight.Font = new Font("Segoe UI", 10, FontStyle.Italic);

        _healthBarBackground.Location = new Point(0, 205);
        _healthBarBackground.Controls.Add(_healthBarFill);

        rightSummaryPanel.Controls.Add(summaryCard);
        rightSummaryPanel.Controls.Add(_healthBarBackground);

        mainLayout.Controls.Add(rightSummaryPanel, 1, 1);

        // =========================================
        // CHARTS SECTION
        // =========================================

        var chartsContainer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2
        };

        chartsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        chartsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        chartsContainer.Controls.Add(_pieChart, 0, 0);
        chartsContainer.Controls.Add(_barChart, 1, 0);

        mainLayout.Controls.Add(chartsContainer, 0, 2);
        mainLayout.SetColumnSpan(chartsContainer, 2);

        Controls.Add(mainLayout);

        Load += (s, e) => Reload();
    }

    /// <summary>
    /// Reloads all analytics data from current budget cycle.
    /// Updates charts, labels, and insights.
    /// </summary>
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

        // =========================================
        // CORE METRICS
        // =========================================

        var daysPassed = Math.Max(1,
            (DateTime.Today - _controller.CurrentCycle.StartDate.Date).Days + 1);

        double actualVelocity = totalSpent / daysPassed;

        double plannedVelocity =
            totalAllowance /
            Math.Max(1,
                (_controller.CurrentCycle.EndDate.Date - _controller.CurrentCycle.StartDate.Date).Days + 1);

        _lblVelocity.Text =
            $"Velocity: {actualVelocity:C}/day vs Planned {plannedVelocity:C}/day";

        var categoryTotals = expenses
            .GroupBy(x => x.Category)
            .Select(g => new
            {
                Category = g.Key,
                Total = (double)g.Sum(x => x.Amount)
            })
            .ToList();

        var topCat = categoryTotals.OrderByDescending(x => x.Total).First();

        _lblTopCategory.Text =
            $"Top Spending: {topCat.Category} ({(totalSpent > 0 ? (topCat.Total / totalSpent) : 0):P0})";

        double usedPercent = totalAllowance <= 0 ? 0 : (totalSpent / totalAllowance);

        _lblHealth.Text = $"Budget Health: {usedPercent:P0} consumed";

        UpdateDynamicAnalytics(
            actualVelocity,
            plannedVelocity,
            topCat.Category,
            usedPercent,
            categoryTotals.Count);

        // =========================================
        // HEALTH BAR
        // =========================================

        _healthBarFill.Width =
            (int)(_healthBarBackground.Width * Math.Min(1.0, usedPercent));

        _healthBarFill.BackColor =
            usedPercent > 0.9 ? Color.Red : ColorPalette.AccentGreen;

        // =========================================
        // PIE CHART
        // =========================================

        double totalForPercentage = categoryTotals.Sum(x => x.Total);

        _pieChart.Series = categoryTotals.Select(c =>
        {
            var percentage = totalForPercentage > 0
                ? (c.Total / totalForPercentage * 100)
                : 0;

            return new PieSeries<double>
            {
                Values = new[] { c.Total },
                Name = c.Category,
                DataLabelsFormatter = _ => percentage.ToString("N1") + "%"
            };
        }).ToArray();

        // =========================================
        // BAR CHART
        // =========================================

        _barChart.Series = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = categoryTotals.Select(x => x.Total).ToArray(),
                Name = "Expenses"
            }
        };

        _barChart.XAxes = new Axis[]
        {
            new Axis
            {
                Labels = categoryTotals.Select(x => x.Category).ToArray()
            }
        };

        _barChart.YAxes = new Axis[]
        {
            new Axis { MinLimit = 0 }
        };

        _pieChart.Invalidate();
        _barChart.Invalidate();
    }

    // باقي الدوال (UpdateDynamicAnalytics / ResetToDefault / CreateDynamicCard)
    // تفضل كما هي
}
