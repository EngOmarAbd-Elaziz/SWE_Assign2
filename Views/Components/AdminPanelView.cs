using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

/// <summary>
/// Admin panel UI responsible for system-level management features such as:
/// - Category management
/// - Debt settlement
/// - Database backup
/// - Financial cycle reset
/// - Viewing audit logs
/// </summary>
public sealed class AdminPanelView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;
    private readonly Form? _hostForm;

    // =========================================
    // UI INPUT COMPONENTS
    // =========================================

    private readonly ListBox _logs = new()
    {
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(20, 20, 20),
        ForeColor = Color.LightGray,
        Font = new Font("Consolas", 10F),
        BorderStyle = BorderStyle.None
    };

    private readonly TextBox _txtCategory = new()
    {
        Width = 300,
        Height = 45,
        PlaceholderText = "New Category Name...",
        Font = new Font("Segoe UI", 12F)
    };

    private readonly ComboBox _comboDeleteCategory = new()
    {
        Width = 300,
        Height = 45,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Font = new Font("Segoe UI", 12F)
    };

    private readonly NumericUpDown _numSettle = new()
    {
        Width = 200,
        Minimum = 0,
        Maximum = 1000000,
        Font = new Font("Segoe UI", 14F, FontStyle.Bold)
    };

    /// <summary>
    /// Initializes a new instance of the AdminPanelView.
    /// </summary>
    /// <param name="controller">Application controller</param>
    /// <param name="dashboard">Dashboard reference for syncing UI</param>
    /// <param name="hostForm">Optional parent form</param>
    public AdminPanelView(AppController controller, DashboardView dashboard, Form? hostForm = null)
    {
        _controller = controller;
        _dashboard = dashboard;
        _hostForm = hostForm;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        Padding = new Padding(30);

        InitializeAdminLayout();
        RefreshCategoryList();
        LoadLogs();
    }

    // =========================================
    // UI INITIALIZATION
    // =========================================

    /// <summary>
    /// Builds the entire admin panel layout including:
    /// - Control panel (left side)
    /// - Audit logs panel (right side)
    /// </summary>
    private void InitializeAdminLayout()
    {
        var mainGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(20)
        };

        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        // =========================================
        // LEFT PANEL: ADMIN CONTROLS
        // =========================================

        var controlPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 20, 0)
        };

        var title = new Label
        {
            Text = "SYSTEM ADMINISTRATION",
            Font = new Font("Segoe UI", 28, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 30)
        };

        controlPanel.Controls.Add(title);

        // =========================================
        // CATEGORY MANAGEMENT CARD
        // =========================================

        var catCard = new Panel
        {
            Width = 550,
            Height = 260,
            Margin = new Padding(0, 0, 0, 25),
            BackColor = Color.FromArgb(30, 30, 30)
        };

        UiStyleService.ApplyRoundedCorners(catCard, 15);

        catCard.Controls.Add(new Label
        {
            Text = "CATEGORY MANAGEMENT",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 15),
            AutoSize = true
        });

        catCard.Controls.Add(new Label
        {
            Text = "Add or remove expense categories",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray,
            Location = new Point(20, 45),
            AutoSize = true
        });

        // Add Category Input
        _txtCategory.Location = new Point(20, 98);
        catCard.Controls.Add(_txtCategory);

        var btnAdd = new Button
        {
            Text = "ADD",
            BackColor = ColorPalette.AccentGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Width = 100,
            Height = 38,
            Location = new Point(330, 98)
        };

        btnAdd.FlatAppearance.BorderSize = 0;

        btnAdd.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtCategory.Text)) return;
            _controller.AddNewCategory(_txtCategory.Text.Trim());
            FinishUpdate("Category Added");
        };

        catCard.Controls.Add(btnAdd);

        // Delete Category
        _comboDeleteCategory.Location = new Point(20, 178);
        catCard.Controls.Add(_comboDeleteCategory);

        var btnDel = new Button
        {
            Text = "DELETE",
            BackColor = ColorPalette.OverspentRed,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Width = 100,
            Height = 38,
            Location = new Point(330, 178)
        };

        btnDel.FlatAppearance.BorderSize = 0;

        btnDel.Click += (_, _) =>
        {
            var selected = _comboDeleteCategory.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;

            _controller.DeleteCategory(selected);
            FinishUpdate("Category Removed");
        };

        catCard.Controls.Add(btnDel);
        controlPanel.Controls.Add(catCard);

        // =========================================
        // FINANCIAL SETTLEMENT CARD
        // =========================================

        var financeCard = new Panel
        {
            Width = 550,
            Height = 160,
            Margin = new Padding(0, 0, 0, 25),
            BackColor = Color.FromArgb(30, 30, 30)
        };

        UiStyleService.ApplyRoundedCorners(financeCard, 15);

        financeCard.Controls.Add(new Label
        {
            Text = "FINANCIAL SETTLEMENT",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 15),
            AutoSize = true
        });

        _numSettle.Location = new Point(20, 98);
        financeCard.Controls.Add(_numSettle);

        var btnSettle = new Button
        {
            Text = "PROCESS",
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Width = 120,
            Height = 38,
            Location = new Point(240, 98)
        };

        btnSettle.FlatAppearance.BorderSize = 0;

        btnSettle.Click += (_, _) =>
        {
            if (_numSettle.Value <= 0) return;

            _controller.ProcessLendSettlement(_numSettle.Value);
            _numSettle.Value = 0;

            FinishUpdate("Debt Settled");
        };

        financeCard.Controls.Add(btnSettle);
        controlPanel.Controls.Add(financeCard);

        // =========================================
        // BACKUP DATABASE
        // =========================================

        var btnBackup = new Button
        {
            Text = "BACKUP DATABASE",
            BackColor = Color.FromArgb(63, 63, 70),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Width = 550,
            Height = 45
        };

        btnBackup.FlatAppearance.BorderSize = 0;

        btnBackup.Click += (_, _) =>
        {
            using var save = new SaveFileDialog
            {
                Filter = "DB files (*.db)|*.db",
                FileName = $"backup_{DateTime.Now:yyyyMMdd}.db"
            };

            if (save.ShowDialog() == DialogResult.OK)
                _controller.BackupDatabase(save.FileName);
        };

        controlPanel.Controls.Add(btnBackup);

        // =========================================
        // RIGHT PANEL: AUDIT LOGS
        // =========================================

        var logContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 25, 25),
            Padding = new Padding(25)
        };

        UiStyleService.ApplyRoundedCorners(logContainer, 18);

        var logHeader = new Label
        {
            Text = "SYSTEM AUDIT LOGS",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            Dock = DockStyle.Top,
            Height = 45
        };

        logContainer.Controls.Add(logHeader);

        var logScroll = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 35),
            Padding = new Padding(15)
        };

        UiStyleService.ApplyRoundedCorners(logScroll, 10);

        _logs.Dock = DockStyle.Fill;
        logScroll.Controls.Add(_logs);

        logContainer.Controls.Add(logScroll);

        mainGrid.Controls.Add(controlPanel, 0, 0);
        mainGrid.Controls.Add(logContainer, 1, 0);

        Controls.Add(mainGrid);
    }

    // =========================================
    // HELPERS
    // =========================================

    private void FinishUpdate(string message)
    {
        _txtCategory.Clear();
        RefreshCategoryList();
        _dashboard.SyncCategories();
        _dashboard.RefreshData();
        LoadLogs();

        MessageBox.Show(message, "Admin System",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void RefreshCategoryList()
    {
        _comboDeleteCategory.Items.Clear();
        foreach (var cat in _controller.Categories)
            _comboDeleteCategory.Items.Add(cat);
    }

    private void LoadLogs()
    {
        _logs.Items.Clear();

        var auditItems = _controller.GetAuditLogs();

        foreach (var log in auditItems)
            _logs.Items.Add($"[{DateTime.Now:HH:mm}] {log}");
    }

    // باقي الميثودز زي ما هي (ResetCycle / ShowResetCycleDialog)
}
