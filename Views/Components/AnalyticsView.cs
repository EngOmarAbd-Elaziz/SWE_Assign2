using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WinForms;
using SkiaSharp;

namespace Masroofy.App.Views.Components;

public sealed class AnalyticsView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;
    private readonly CartesianChart _chart = new()
    {
        Dock = DockStyle.Fill,
        Margin = new Padding(0, 20, 0, 20)
    };

    private readonly Label _lblVelocity = new() { AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.LightGray, Margin = new Padding(0, 10, 0, 0) };
    private readonly Label _lblTopCategory = new() { AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.LightGray, Margin = new Padding(0, 10, 0, 0) };
    private readonly Label _lblHealth = new() { AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.LightGray, Margin = new Padding(0, 10, 0, 10) };
    private readonly Label _lblInsight = new() { AutoSize = true, Font = new Font("Segoe UI", 14, FontStyle.Regular), ForeColor = ColorPalette.AccentGreen, Margin = new Padding(0, 8, 0, 18) };
    private readonly Panel _healthBarBackground = new() { BackColor = Color.FromArgb(45, 45, 45), Height = 22, Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 20) };
    private readonly Panel _healthBarFill = new() { BackColor = ColorPalette.SafeGreen, Height = 22, Width = 0 };

    public AnalyticsView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;
        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(40),
            AutoSize = false,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100F)
            }
        };

        layout.Controls.Add(new Label
        {
            Text = "Analytics & Insights",
            AutoSize = true,
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            Margin = new Padding(0, 0, 0, 20),
            Dock = DockStyle.Fill
        }, 0, 0);

        var statsPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            AutoSize = true
        };
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statsPanel.Controls.Add(_lblVelocity, 0, 0);
        statsPanel.Controls.Add(_lblTopCategory, 0, 1);
        statsPanel.Controls.Add(_lblHealth, 0, 2);
        statsPanel.Controls.Add(_lblInsight, 0, 3);

        layout.Controls.Add(statsPanel, 0, 1);
        layout.Controls.Add(CreateAnalyticsCard("Spending Velocity", "If you spend 120 EGP/day while your planned pace is 100 EGP/day, you are approaching warning territory.", "Actual: 120/day", "Planned: 100/day", "Adjust your pace."), 0, 2);
        layout.Controls.Add(CreateAnalyticsCard("Budget Health", "Your current budget usage is the strongest signal for whether you can safely keep spending.", "Usage: 65%", "Safe Threshold: 70%", "Keep it green."), 0, 3);
        layout.Controls.Add(CreateAnalyticsCard("Fast Action", "When your spending velocity exceeds the plan, reduce non-essential categories first.", "Save more by reviewing categories.", "Tip: Save now", "Stay on track"), 0, 4);

        layout.Controls.Add(new Label
        {
            Text = "Detailed Chart",
            AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.White,
            Margin = new Padding(0, 20, 0, 8),
            Dock = DockStyle.Fill
        }, 0, 5);

        _healthBarBackground.Controls.Add(_healthBarFill);
        layout.Controls.Add(_healthBarBackground, 0, 6);
        layout.Controls.Add(_chart, 0, 7);

        Controls.Add(layout);

        Reload();
    }

    public void Reload()
    {
        if (_controller.CurrentCycle == null)
        {
            _lblVelocity.Text = "No active budget cycle available.";
            _lblTopCategory.Text = "Top category unavailable.";
            _lblHealth.Text = "Financial Health: N/A";
            _healthBarFill.Width = 0;
            _chart.Series = Array.Empty<ISeries>();
            return;
        }

        var expenses = _controller.GetExpenses();
        var totalAllowance = _controller.CurrentCycle.TotalAllowance;
        var totalSpent = expenses.Sum(x => x.Amount);
        var daysPassed = Math.Max(1, (DateTime.Today - _controller.CurrentCycle.StartDate.Date).Days + 1);
        var totalDays = Math.Max(1, (_controller.CurrentCycle.EndDate.Date - _controller.CurrentCycle.StartDate.Date).Days + 1);

        var actualVelocity = totalSpent / daysPassed;
        var plannedVelocity = totalAllowance / totalDays;
        var velocityRatio = plannedVelocity > 0 ? actualVelocity / plannedVelocity : 0m;

        _lblVelocity.Text = $"Spending Velocity: {actualVelocity:C2}/day vs planned {plannedVelocity:C2}/day ({velocityRatio:P0})";

        var categoryTotals = expenses.GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Total)
            .ToList();

        var top = categoryTotals.FirstOrDefault();
        if (top == null)
        {
            _lblTopCategory.Text = "Top Category: No expenses yet.";
        }
        else
        {
            var percent = totalSpent <= 0 ? 0 : top.Total / totalSpent;
            _lblTopCategory.Text = $"Top Category: {top.Category} ({percent:P0})";
        }

        var usedPercent = totalAllowance <= 0 ? 0 : Math.Min(1, totalSpent / totalAllowance);
        _lblHealth.Text = $"Financial Health: {Math.Round(usedPercent * 100, 0)}% of budget used";
        _healthBarFill.Width = (int)(_healthBarBackground.Width * usedPercent);
        _healthBarFill.BackColor = usedPercent <= 0.7m ? ColorPalette.SafeGreen : usedPercent <= 0.9m ? ColorPalette.WarningOrange : ColorPalette.OverspentRed;

        _chart.Series = categoryTotals.Select(item =>
        {
            var color = SKColor.Parse(item.Category == "Other" ? "#8BC34A" : "#50C878");
            return new ColumnSeries<double>
            {
                Name = item.Category,
                Values = new[] { (double)item.Total },
                Fill = new SolidColorPaint(color.WithAlpha(180)),
                Stroke = new SolidColorPaint(color) { StrokeThickness = 2 },
                DataLabelsPaint = new SolidColorPaint(SKColors.WhiteSmoke),
                DataLabelsPosition = DataLabelsPosition.Top,
                Padding = 8
            };
        }).Cast<ISeries>().ToArray();

        _chart.XAxes = new Axis[]
        {
            new() { Labels = categoryTotals.Select(x => x.Category).ToArray(), LabelsRotation = 0, TextSize = 12, LabelsPaint = new SolidColorPaint(SKColors.LightGray) }
        };

        _chart.YAxes = new Axis[]
        {
            new() { Name = "EGP", Labeler = value => value.ToString("N0"), TextSize = 12, LabelsPaint = new SolidColorPaint(SKColors.LightGray) }
        };

        _lblInsight.Text = GetInsightMessage(usedPercent, velocityRatio);

        _chart.LegendPosition = LegendPosition.Bottom;
        _chart.LegendTextPaint = new SolidColorPaint(SKColors.WhiteSmoke);
    }

    private string GetInsightMessage(decimal usedPercent, decimal velocityRatio)
    {
        if (usedPercent >= 0.9m)
            return "Insight: Budget is critically consumed. Reduce discretionary spending immediately.";

        if (velocityRatio > 1m)
            return "Insight: You are spending faster than usual. Rebalance priorities and save cash.";

        if (usedPercent >= 0.7m)
            return "Insight: Your budget is in warning territory. Review non-essential categories.";

        return "Insight: Healthy pace. Keep your spending aligned with your budget plan.";
    }

    private Control CreateAnalyticsCard(string title, string description, string statA, string statB, string callout)
    {
        var card = new Panel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            BackColor = ColorPalette.DarkSurface,
            Margin = new Padding(0, 0, 0, 20),
            Padding = new Padding(30)
        };
        UiStyleService.ApplyRoundedCorners(card, 20);

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            AutoSize = true
        };
        contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        var header = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        var descriptionLabel = new Label
        {
            Text = description,
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = Color.LightGray,
            MaximumSize = new Size(1000, 0),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        };

        var detailsLabel = new Label
        {
            Text = statA,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        var subLabel = new Label
        {
            Text = statB,
            Font = new Font("Segoe UI", 12, FontStyle.Regular),
            ForeColor = Color.White,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };

        var badge = new Label
        {
            Text = callout,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.FromArgb(40, 40, 43),
            AutoSize = true,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 0, 0)
        };

        contentLayout.Controls.Add(header);
        contentLayout.Controls.Add(descriptionLabel);
        contentLayout.Controls.Add(detailsLabel);
        contentLayout.Controls.Add(subLabel);
        contentLayout.Controls.Add(badge);

        card.Controls.Add(contentLayout);
        return card;
    }
}

