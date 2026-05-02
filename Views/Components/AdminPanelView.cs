using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

public sealed class AdminPanelView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;
    private readonly Form? _hostForm;

    // عناصر الإدخال
    private readonly ListBox _logs = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(20, 20, 20), ForeColor = Color.LightGray, Font = new Font("Consolas", 10F), BorderStyle = BorderStyle.None };
    private readonly TextBox _txtCategory = new() { Width = 300, Height = 45, PlaceholderText = "New Category Name...", Font = new Font("Segoe UI", 12F) };
    private readonly ComboBox _comboDeleteCategory = new() { Width = 300, Height = 45, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 12F) };
    private readonly NumericUpDown _numSettle = new() { Width = 200, Minimum = 0, Maximum = 1000000, Font = new Font("Segoe UI", 14F, FontStyle.Bold) };

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

    private void InitializeAdminLayout()
    {
        var mainGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(20, 20, 20, 20)
        };
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
        mainGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));

        // --- LEFT PANEL: ADMIN CONTROLS ---
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

        // 1. CATEGORY MANAGEMENT CARD
        var catCard = new Panel
        {
            Width = 550,
            Height = 260,
            Margin = new Padding(0, 0, 0, 25),
            BackColor = Color.FromArgb(30, 30, 30),
            AutoSize = false
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

        catCard.Controls.Add(new Label
        {
            Text = "Add New Category:",
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(20, 75)
        });

        _txtCategory.Width = 300;
        _txtCategory.Height = 38;
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
            Location = new Point(330, 98),
            Cursor = Cursors.Hand
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnAdd, 8);
        btnAdd.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtCategory.Text)) return;
            _controller.AddNewCategory(_txtCategory.Text.Trim());
            FinishUpdate("Category Added");
        };
        catCard.Controls.Add(btnAdd);

        catCard.Controls.Add(new Label
        {
            Text = "Delete Existing Category:",
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(20, 155)
        });

        _comboDeleteCategory.Width = 300;
        _comboDeleteCategory.Height = 38;
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
            Location = new Point(330, 178),
            Cursor = Cursors.Hand
        };
        btnDel.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnDel, 8);
        btnDel.Click += (_, _) =>
        {
            var selected = _comboDeleteCategory.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selected)) return;
            _controller.DeleteCategory(selected);
            FinishUpdate("Category Removed");
        };
        catCard.Controls.Add(btnDel);
        controlPanel.Controls.Add(catCard);

        // 2. FINANCIAL SETTLEMENT CARD
        var financeCard = new Panel
        {
            Width = 550,
            Height = 160,
            Margin = new Padding(0, 0, 0, 25),
            BackColor = Color.FromArgb(30, 30, 30),
            AutoSize = false
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

        financeCard.Controls.Add(new Label
        {
            Text = "Process debt settlements",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray,
            Location = new Point(20, 45),
            AutoSize = true
        });

        financeCard.Controls.Add(new Label
        {
            Text = "Settlement Amount (EGP):",
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(20, 75)
        });

        _numSettle.Width = 200;
        _numSettle.Height = 38;
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
            Location = new Point(240, 98),
            Cursor = Cursors.Hand
        };
        btnSettle.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnSettle, 8);
        btnSettle.Click += (_, _) =>
        {
            if (_numSettle.Value <= 0) return;
            _controller.ProcessLendSettlement(_numSettle.Value);
            _numSettle.Value = 0;
            FinishUpdate("Debt Settled");
        };
        financeCard.Controls.Add(btnSettle);
        controlPanel.Controls.Add(financeCard);

        // 3. BACKUP DATABASE BUTTON
        var btnBackup = new Button
        {
            Text = "BACKUP DATABASE",
            BackColor = Color.FromArgb(63, 63, 70),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Width = 550,
            Height = 45,
            Margin = new Padding(0, 0, 0, 25),
            Cursor = Cursors.Hand
        };
        btnBackup.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnBackup, 8);
        btnBackup.Click += (_, _) =>
        {
            using var save = new SaveFileDialog { Filter = "DB files (*.db)|*.db", FileName = $"backup_{DateTime.Now:yyyyMMdd}.db" };
            if (save.ShowDialog() == DialogResult.OK) _controller.BackupDatabase(save.FileName);
        };
        controlPanel.Controls.Add(btnBackup);

        // 4. RESET CYCLE CARD
        var resetCard = new Panel
        {
            Width = 550,
            Height = 320,
            Margin = new Padding(0, 0, 0, 0),
            BackColor = Color.FromArgb(30, 30, 30),
            AutoSize = false
        };
        UiStyleService.ApplyRoundedCorners(resetCard, 15);

        resetCard.Controls.Add(new Label
        {
            Text = "FRESH START: NEW FINANCIAL CYCLE",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 15),
            AutoSize = true
        });

        resetCard.Controls.Add(new Label
        {
            Text = "Reset all transactions and start fresh",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.DimGray,
            Location = new Point(20, 45),
            AutoSize = true
        });

        var btnReset = new Button
        {
            Text = "⚡ START NEW CYCLE",
            BackColor = ColorPalette.WarningOrange,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Width = 510,
            Height = 50,
            Location = new Point(20, 75),
            Cursor = Cursors.Hand
        };
        btnReset.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnReset, 8);
        btnReset.Click += (_, _) => ShowResetCycleDialog();
        resetCard.Controls.Add(btnReset);

        var resetDesc = new Label
        {
            Text = "⚠️ WARNING: This will PERMANENTLY DELETE all transactions.\nYou will set: Initial Balance & Cycle Start Date.",
            ForeColor = Color.OrangeRed,
            Font = new Font("Segoe UI", 9),
            Location = new Point(20, 140),
            AutoSize = true,
            MaximumSize = new Size(510, 150)
        };
        resetCard.Controls.Add(resetDesc);
        controlPanel.Controls.Add(resetCard);

        // --- RIGHT PANEL: AUDIT LOGS ---
        var logContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(25, 25, 25),
            Padding = new Padding(25),
            AutoSize = false
        };
        UiStyleService.ApplyRoundedCorners(logContainer, 18);

        var logHeader = new Label
        {
            Text = "📋 SYSTEM AUDIT LOGS",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            Dock = DockStyle.Top,
            Height = 45,
            Margin = new Padding(0, 0, 0, 20)
        };
        logContainer.Controls.Add(logHeader);

        var logScrollContainer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(35, 35, 35),
            Padding = new Padding(15),
            AutoSize = false
        };
        UiStyleService.ApplyRoundedCorners(logScrollContainer, 10);

        _logs.Dock = DockStyle.Fill;
        _logs.Font = new Font("Consolas", 11F);
        _logs.BackColor = Color.FromArgb(20, 20, 20);
        _logs.ForeColor = Color.LightGray;
        logScrollContainer.Controls.Add(_logs);
        logContainer.Controls.Add(logScrollContainer);

        mainGrid.Controls.Add(controlPanel, 0, 0);
        mainGrid.Controls.Add(logContainer, 1, 0);

        this.Controls.Add(mainGrid);
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

    private void ShowResetCycleDialog()
    {
        var form = new Form
        {
            Text = "Start New Financial Cycle",
            BackColor = ColorPalette.DarkBackground,
            ForeColor = ColorPalette.DarkText,
            Width = 600,
            Height = 480,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var lblWarning = new Label
        {
            Text = "⚠️ This action will DELETE all transactions permanently.",
            ForeColor = Color.OrangeRed,
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 20),
            MaximumSize = new Size(550, 100)
        };
        form.Controls.Add(lblWarning);

        var lblInfo = new Label
        {
            Text = "Configure your new financial cycle settings below:",
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Location = new Point(25, 70)
        };
        form.Controls.Add(lblInfo);

        var lblBalance = new Label
        {
            Text = "Initial Balance (EGP):",
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 110)
        };
        form.Controls.Add(lblBalance);

        var numBalance = new NumericUpDown
        {
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 1000000,
            Width = 280,
            Height = 45,
            Font = new Font("Segoe UI", 12),
            Location = new Point(25, 140)
        };
        form.Controls.Add(numBalance);

        var lblDate = new Label
        {
            Text = "Cycle Start Date:",
            ForeColor = Color.LightGray,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 200)
        };
        form.Controls.Add(lblDate);

        var dtStart = new DateTimePicker
        {
            Value = DateTime.Today,
            Format = DateTimePickerFormat.Short,
            Width = 280,
            Height = 45,
            Font = new Font("Segoe UI", 12),
            Location = new Point(25, 230)
        };
        form.Controls.Add(dtStart);

        var btnConfirm = new Button
        {
            Text = "RESET & START NEW CYCLE",
            BackColor = ColorPalette.WarningOrange,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Width = 550,
            Height = 50,
            Location = new Point(25, 300)
        };
        btnConfirm.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnConfirm, 10);
        btnConfirm.Click += (_, _) =>
        {
            var result = MessageBox.Show(
                "Are you absolutely sure? This will DELETE all transaction data permanently.",
                "Confirm Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2
            );

            if (result == DialogResult.Yes)
            {
                ResetCycle((decimal)numBalance.Value, dtStart.Value);
                form.Close();
                FinishUpdate("Financial cycle reset successfully!");
            }
        };
        form.Controls.Add(btnConfirm);

        form.ShowDialog(_hostForm);
    }

    private void ResetCycle(decimal initialBalance, DateTime cycleStartDate)
    {
        // 1. Clear all transactions (TRUNCATE equivalent)
        _controller.ClearAllExpenses();

        // 2. Reset the budget cycle with new values
        _controller.CreateNewBudgetCycle(initialBalance, cycleStartDate);

        // 3. Update audit log
        _controller.LogAction($"System Reset: New cycle started with initial balance {initialBalance:C2} from {cycleStartDate:yyyy-MM-dd}");

        // 4. Refresh dashboard
        _dashboard.RefreshData();
    }
}