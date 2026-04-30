using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Models;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

public sealed class HistoryView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;
    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        BackgroundColor = ColorPalette.DarkBackground,
        BorderStyle = BorderStyle.None,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false // شكل أنضف للجدول
    };

    private readonly ComboBox _categoryFilter = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300, Height = 55 };
    private readonly Button _btnEdit = new() { Text = "EDIT", Width = 140, Height = 55, Visible = false };
    private readonly Button _btnDelete = new() { Text = "DELETE", Width = 140, Height = 55, Visible = false };
    private readonly Button _btnClearAll = new() { Text = "CLEAR ALL", Width = 160, Height = 55, Visible = false };
    private bool _managerMode;

    public HistoryView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;
        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true;

        ConfigureGrid();
        SyncFilterCategories();
        _categoryFilter.SelectedIndexChanged += (_, _) => Reload();

        // العنوان والوصف
        var title = new Label
        {
            Text = "TRANSACTION HISTORY",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 10)
        };

        var subtitle = new Label
        {
            Text = "Manage and track your spending history. Enable Manager Mode in settings to edit or clear logs.",
            Font = new Font("Segoe UI", 12F),
            ForeColor = Color.LightGray,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 30)
        };

        // البار العلوي (الفلاتر والأزرار)
        var topBar = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 1,
            Height = 100,
            BackColor = ColorPalette.DarkSurface,
            Padding = new Padding(20),
            Margin = new Padding(0, 0, 0, 20)
        };
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F)); // للفلاتر
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // للأزرار

        // إعداد الفلتر
        _categoryFilter.BackColor = Color.FromArgb(24, 24, 24);
        _categoryFilter.ForeColor = Color.White;
        _categoryFilter.Font = new Font("Segoe UI", 12F);

        var filterPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        filterPanel.Controls.Add(new Label { Text = "FILTER:", AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold), ForeColor = ColorPalette.AccentGreen, Margin = new Padding(0, 12, 10, 0) });
        filterPanel.Controls.Add(_categoryFilter);

        // تنسيق الأزرار
        StyleActionButton(_btnEdit, Color.FromArgb(0, 122, 204)); // Blue
        StyleActionButton(_btnDelete, ColorPalette.OverspentRed);
        StyleActionButton(_btnClearAll, ColorPalette.WarningOrange);

        var actionPanel = new FlowLayoutPanel { Dock = DockStyle.Right, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, AutoSize = true };
        actionPanel.Controls.Add(_btnEdit);
        actionPanel.Controls.Add(_btnDelete);
        actionPanel.Controls.Add(_btnClearAll);

        topBar.Controls.Add(filterPanel, 0, 0);
        topBar.Controls.Add(actionPanel, 1, 0);

        // ربط أحداث الأزرار
        _btnEdit.Click += (_, _) => EditSelected();
        _btnDelete.Click += (_, _) => DeleteSelected();
        _btnClearAll.Click += (_, _) => ClearAllHistory();

        // الحاوية الأساسية (الـ Card)
        var card = UiStyleService.CreateCard(new Size(0, 0));
        card.Dock = DockStyle.Fill;
        card.BackColor = ColorPalette.DarkSurface;

        var contentLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(40),
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100F)
            }
        };

        contentLayout.Controls.Add(title, 0, 0);
        contentLayout.Controls.Add(subtitle, 0, 1);
        contentLayout.Controls.Add(topBar, 0, 2);
        contentLayout.Controls.Add(_grid, 0, 3);

        card.Controls.Add(contentLayout);
        Controls.Add(card);

        Reload();
    }

    private void StyleActionButton(Button btn, Color backColor)
    {
        btn.BackColor = backColor;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btn.Margin = new Padding(10, 0, 0, 0);
        btn.Cursor = Cursors.Hand;
        UiStyleService.ApplyRoundedCorners(btn, 12);
    }

    private void ConfigureGrid()
    {
        _grid.Columns.Clear();
        _grid.Columns.Add("Date", "DATE & TIME");
        _grid.Columns.Add("Category", "CATEGORY");
        _grid.Columns.Add("Amount", "AMOUNT (EGP)");

        _grid.RowTemplate.Height = 60; // زيادة ارتفاع الصفوف لسهولة القراءة
        _grid.ColumnHeadersHeight = 65;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(35, 35, 35);
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = ColorPalette.AccentGreen;
        _grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);

        _grid.DefaultCellStyle.BackColor = ColorPalette.DarkSurface;
        _grid.DefaultCellStyle.ForeColor = Color.White;
        _grid.DefaultCellStyle.Font = new Font("Segoe UI", 12F);
        _grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 50);
        _grid.GridColor = Color.FromArgb(45, 45, 45);
    }

    public void Reload()
    {
        _grid.Rows.Clear();
        var selected = _categoryFilter.SelectedItem?.ToString() ?? "All Categories";
        var expenses = _controller.GetExpenses()
            .Where(e => selected == "All Categories" || e.Category == selected)
            .OrderByDescending(e => e.Date);

        foreach (var expense in expenses)
        {
            _grid.Rows.Add(expense.Date.ToString("yyyy-MM-dd HH:mm"), expense.Category, expense.Amount.ToString("N2"));
        }
    }

    public void SyncFilterCategories()
    {
        _categoryFilter.Items.Clear();
        _categoryFilter.Items.Add("All Categories");
        foreach (var category in _controller.Categories) _categoryFilter.Items.Add(category);
        _categoryFilter.SelectedIndex = 0;
    }

    public void SetManagerMode(bool enabled)
    {
        _managerMode = enabled;
        _btnEdit.Visible = enabled;
        _btnDelete.Visible = enabled;
        _btnClearAll.Visible = enabled;
    }

    private void ClearAllHistory()
    {
        if (!_managerMode) return;

        var result = MessageBox.Show(
            "CRITICAL: Are you sure you want to PERMANENTLY clear ALL history? This will reset your budget progress.",
            "Wipe Data Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Stop);

        if (result == DialogResult.Yes)
        {
            _controller.ClearAllExpenses();
            _dashboard.RefreshData();
            Reload();
            MessageBox.Show("History has been wiped clean.");
        }
    }

    private void DeleteSelected()
    {
        var expense = GetSelectedExpense();
        if (expense == null) return;

        if (MessageBox.Show("Delete this transaction?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            _controller.DeleteExpense(expense);
            _dashboard.RefreshData();
            Reload();
        }
    }

    private Expense? GetSelectedExpense()
    {
        if (_grid.CurrentRow == null) return null;

        var dateText = _grid.CurrentRow.Cells[0].Value?.ToString();
        var category = _grid.CurrentRow.Cells[1].Value?.ToString();
        var amountText = _grid.CurrentRow.Cells[2].Value?.ToString()?.Replace(",", "");

        if (!DateTime.TryParse(dateText, out var date) || !decimal.TryParse(amountText, out var amount)) return null;

        return _controller.GetExpenses().FirstOrDefault(e =>
            e.Date.ToString("yyyy-MM-dd HH:mm") == dateText &&
            e.Category == category &&
            e.Amount == amount);
    }

    private void EditSelected()
    {
        var expense = GetSelectedExpense();
        if (expense == null) return;

        // ... (كود الـ Edit Form كما هو في كودك الأصلي يعمل بشكل جيد) ...
        // ملاحظة: تأكد من استدعاء Reload() بعد نجاح التعديل
    }
}