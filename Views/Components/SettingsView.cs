using Masroofy.App.Controllers;
using Masroofy.App.Services;
using Masroofy.App.Assets;

namespace Masroofy.App.Views.Components;

public sealed class SettingsView : UserControl
{
    private readonly AppController _controller;
    private readonly ThemeManager _themeService;
    private readonly Form _hostForm;
    private readonly Action<bool> _onManagerModeChanged;

    public SettingsView(AppController controller, ThemeManager themeService, Form hostForm, Action<bool> managerModeChanged)
    {
        _controller = controller;
        _themeService = themeService;
        _hostForm = hostForm;
        _onManagerModeChanged = managerModeChanged;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true; // لضمان ظهور العناصر لو الشاشة صغرت

        InitializeResponsiveLayout();
    }

    private void InitializeResponsiveLayout()
    {
        this.Controls.Clear();

        // الحاوية الرئيسية مرنة جداً
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top, // تلتصق بالأعلى وتتمدد
            ColumnCount = 1,
            RowCount = 4,
            AutoSize = true,
            Padding = new Padding(50),
            BackColor = Color.Transparent
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // 1. العنوان الرئيسي
        var lblHeader = new Label
        {
            Text = "APPLICATION SETTINGS",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 40)
        };

        // 2. كارت تغيير الـ PIN
        var txtNewPin = CreateLargeTextBox("Enter New 4-Digit PIN");
        var btnChangePin = CreateLargeButton("UPDATE SECURITY PIN", Color.FromArgb(0, 122, 204));
        btnChangePin.Click += (_, _) => HandlePinChange(txtNewPin);

        var pinCard = CreateSettingsGroup("SECURITY & PRIVACY", "Change your primary access code for the application.", txtNewPin, btnChangePin);

        // 3. كارت تحديث كلمة مرور Admin
        var txtAdminPass = CreateLargeTextBox("Enter New Admin Password");
        var btnAdminPass = CreateLargeButton("UPDATE ADMIN PASSWORD", ColorPalette.WarningOrange);
        btnAdminPass.Click += (_, _) => HandleAdminPasswordChange(txtAdminPass);

        var adminPassCard = CreateSettingsGroup("ADMIN CREDENTIALS", "Update the administrator password for manager panel access.", txtAdminPass, btnAdminPass);

        // 4. كارت الـ Manager Mode
        var txtManagerPin = CreateLargeTextBox("Enter Admin PIN to Unlock");
        var btnEnableManager = CreateLargeButton("UNLOCK MANAGER MODE", ColorPalette.AccentGreen);
        btnEnableManager.Click += (_, _) => HandleManagerUnlock(txtManagerPin);

        var managerCard = CreateSettingsGroup("ADMIN PRIVILEGES", "Unlock advanced editing features and system administration.", txtManagerPin, btnEnableManager);

        // إضافة الكل للحاوية
        mainLayout.Controls.Add(lblHeader);
        mainLayout.Controls.Add(pinCard);
        mainLayout.Controls.Add(adminPassCard);
        mainLayout.Controls.Add(managerCard);

        this.Controls.Add(mainLayout);
    }

    private Panel CreateSettingsGroup(string title, string description, TextBox? input, Button action)
    {
        var groupPanel = new Panel
        {
            Width = 1000, // عرض كبير ليناسب الشاشات
            Height = 180,
            Margin = new Padding(0, 0, 0, 30),
            BackColor = Color.FromArgb(30, 30, 30)
        };
        UiStyleService.ApplyRoundedCorners(groupPanel, 20);

        var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 16, FontStyle.Bold), ForeColor = Color.White, Location = new Point(30, 20), AutoSize = true };
        var lblDesc = new Label { Text = description, Font = new Font("Segoe UI", 11), ForeColor = Color.Gray, Location = new Point(30, 55), AutoSize = true };

        var controlsLayout = new FlowLayoutPanel
        {
            Location = new Point(30, 95),
            Size = new Size(940, 70),
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent
        };

        if (input != null) controlsLayout.Controls.Add(input);
        controlsLayout.Controls.Add(action);

        groupPanel.Controls.AddRange(new Control[] { lblTitle, lblDesc, controlsLayout });
        return groupPanel;
    }

    private TextBox CreateLargeTextBox(string placeholder) => new TextBox
    {
        Width = 400,
        Height = 50,
        PasswordChar = '*',
        PlaceholderText = placeholder,
        Font = new Font("Segoe UI", 14F),
        BackColor = Color.FromArgb(20, 20, 20),
        ForeColor = Color.White,
        BorderStyle = BorderStyle.FixedSingle,
    };

    private Button CreateLargeButton(string text, Color color)
    {
        var btn = new Button
        {
            Text = text,
            Width = 300,
            Height = 50,
            BackColor = color,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Margin = new Padding(15, 0, 0, 0)
        };
        btn.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btn, 12);
        return btn;
    }

    private void HandlePinChange(TextBox txt)
    {
        if (txt.Text.Length < 4) { MessageBox.Show("PIN must be at least 4 digits."); return; }
        _controller.ChangePin(txt.Text);
        txt.Clear();
        MessageBox.Show("Security PIN Updated!");
    }

    private void HandleAdminPasswordChange(TextBox txt)
    {
        if (string.IsNullOrWhiteSpace(txt.Text))
        {
            MessageBox.Show("Admin password cannot be empty.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (txt.Text.Length < 6)
        {
            MessageBox.Show("Admin password must be at least 6 characters.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _controller.ChangeAdminPassword(txt.Text);
        txt.Clear();
        MessageBox.Show("Admin password updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void HandleManagerUnlock(TextBox txt)
    {
        var isAuthorized = _controller.VerifyCurrentPin(txt.Text);
        _onManagerModeChanged(isAuthorized);
        txt.Clear();
        if (isAuthorized) MessageBox.Show("Manager Mode Active!");
        else MessageBox.Show("Access Denied: Wrong PIN.");
    }
}