using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

public sealed class AdminPanelView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;

    // عناصر الإدخال
    private readonly ListBox _logs = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.LightGray, Font = new Font("Consolas", 10F), BorderStyle = BorderStyle.None };
    private readonly TextBox _txtCategory = new() { Width = 300, Height = 45, PlaceholderText = "New Category Name...", Font = new Font("Segoe UI", 12F) };
    private readonly ComboBox _comboDeleteCategory = new() { Width = 300, Height = 45, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12F) };
    private readonly NumericUpDown _numSettle = new() { Width = 200, Minimum = 0, Maximum = 1000000, Font = new Font("Segoe UI", 14F, FontStyle.Bold) };

    public AdminPanelView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        Padding = new Padding(30);

        InitializeAdminLayout();
        RefreshCategoryList();
        LoadLogs();
    }

    private void InitializeAdminLayout()
    {
        var mainGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // جهة التحكم
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F)); // جهة السجلات

        // --- النصف الأيسر: لوحة التحكم ---
        var controlPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };

        var title = new Label { Text = "SYSTEM ADMINISTRATION", Font = new Font("Segoe UI", 28, FontStyle.Bold), ForeColor = ColorPalette.AccentGreen, AutoSize = true, Margin = new Padding(0, 0, 0, 30) };
        controlPanel.Controls.Add(title);

        // 1. كارت إدارة الفئات
        var catCard = CreateAdminSection("CATEGORY MANAGEMENT", "Add or remove expense categories from the system.");
        catCard.Controls.Add(new Label { Text = "Add New:", ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 70) });
        catCard.Controls.Add(_txtCategory); _txtCategory.Location = new Point(20, 95);

        var btnAdd = CreateActionButton("ADD", ColorPalette.AccentGreen, new Point(330, 95));
        btnAdd.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtCategory.Text)) return;
            _controller.AddNewCategory(_txtCategory.Text.Trim());
            FinishUpdate("Category Added");
        };
        catCard.Controls.Add(btnAdd);

        catCard.Controls.Add(new Label { Text = "Delete Existing:", ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 160) });
        catCard.Controls.Add(_comboDeleteCategory); _comboDeleteCategory.Location = new Point(20, 185);

        var btnDel = CreateActionButton("DELETE", ColorPalette.OverspentRed, new Point(330, 185));
        btnDel.Click += (_, _) =>
        {
            var selected = _comboDeleteCategory.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            _controller.DeleteCategory(selected);
            FinishUpdate("Category Removed");
        };
        catCard.Controls.Add(btnDel);
        controlPanel.Controls.Add(catCard);

        // 2. كارت الإدارة المالية
        var financeCard = CreateAdminSection("FINANCIAL SETTLEMENT", "Process debt settlements and manual balance injections.");
        financeCard.Controls.Add(new Label { Text = "Settlement Amount (EGP):", ForeColor = Color.Gray, AutoSize = true, Location = new Point(20, 75) });
        financeCard.Controls.Add(_numSettle); _numSettle.Location = new Point(20, 100);

        var btnSettle = CreateActionButton("PROCESS SETTLEMENT", Color.FromArgb(0, 122, 204), new Point(240, 100));
        btnSettle.Width = 240;
        btnSettle.Click += (_, _) =>
        {
            if (_numSettle.Value <= 0) return;
            _controller.ProcessLendSettlement(_numSettle.Value);
            _numSettle.Value = 0;
            FinishUpdate("Debt Settled");
        };
        financeCard.Controls.Add(btnSettle);
        controlPanel.Controls.Add(financeCard);

        // 3. أزرار النظام (Backup)
        var btnBackup = CreateActionButton("BACKUP DATABASE", Color.FromArgb(63, 63, 70), new Point(0, 0));
        btnBackup.Width = 250;
        btnBackup.Click += (_, _) =>
        {
            using var save = new SaveFileDialog { Filter = "DB files (*.db)|*.db", FileName = $"backup_{DateTime.Now:yyyyMMdd}.db" };
            if (save.ShowDialog() == DialogResult.OK) _controller.BackupDatabase(save.FileName);
        };
        controlPanel.Controls.Add(btnBackup);

        // --- النصف الأيمن: السجلات (Audit Logs) ---
        var logContainer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20, 55, 0, 0) };
        var logHeader = new Label { Text = "SYSTEM AUDIT LOGS", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 40 };
        logContainer.Controls.Add(_logs);
        logContainer.Controls.Add(logHeader);

        mainGrid.Controls.Add(controlPanel, 0, 0);
        mainGrid.Controls.Add(logContainer, 1, 0);

        this.Controls.Add(mainGrid);
    }

    private Panel CreateAdminSection(string title, string desc)
    {
        var pnl = new Panel { Width = 600, Height = 280, Margin = new Padding(0, 0, 0, 30), BackColor = Color.FromArgb(30, 30, 30) };
        UiStyleService.ApplyRoundedCorners(pnl, 15);
        pnl.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = Color.White, Location = new Point(20, 15), AutoSize = true });
        pnl.Controls.Add(new Label { Text = desc, Font = new Font("Segoe UI", 9), ForeColor = Color.DimGray, Location = new Point(20, 45), AutoSize = true });
        return pnl;
    }

    private Button CreateActionButton(string text, Color color, Point loc)
    {
        var btn = new Button
        {
            Text = text,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(150, 45),
            Location = loc,
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btn, 10);
        return btn;
    }

    private void FinishUpdate(string message)
    {
        _txtCategory.Clear();
        RefreshCategoryList();
        _dashboard.SyncCategories();
        _dashboard.RefreshData();
        LoadLogs();
        MessageBox.Show(message, "Admin System", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void RefreshCategoryList()
    {
        _comboDeleteCategory.Items.Clear();
        foreach (var cat in _controller.Categories) _comboDeleteCategory.Items.Add(cat);
    }

    private void LoadLogs()
    {
        _logs.Items.Clear();
        var auditItems = _controller.GetAuditLogs(); // تأكد أن الميثود ترجع List<string>
        foreach (var log in auditItems) _logs.Items.Add($"[{DateTime.Now:HH:mm}] {log}");
    }
}