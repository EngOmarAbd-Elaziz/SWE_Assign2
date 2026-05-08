using Masroofy.App.Controllers;
using Masroofy.App.Services;
using Masroofy.App.Assets;

namespace Masroofy.App.Views.Components;

/// <summary>
/// A UserControl that renders the application settings panel, providing controls for
/// updating the user PIN, changing the admin password, and unlocking manager mode
/// via PIN verification.
/// </summary>
public sealed class SettingsView : UserControl
{
    private readonly AppController _controller;
    private readonly ThemeManager _themeService;
    private readonly Form _hostForm;
    private readonly Action<bool> _onManagerModeChanged;

    /// <summary>
    /// Initializes a new instance of <see cref="SettingsView"/>, wires up all dependencies,
    /// and builds the responsive settings layout.
    /// </summary>
    /// <param name="controller">The application controller used to execute PIN and password changes.</param>
    /// <param name="themeService">The theme manager, retained for potential theme-related settings.</param>
    /// <param name="hostForm">The host form, retained for dialog positioning and theme operations.</param>
    /// <param name="managerModeChanged">
    /// A callback invoked with true when manager mode is successfully unlocked, or false when
    /// PIN verification fails.
    /// </param>
    public SettingsView(AppController controller, ThemeManager themeService, Form hostForm, Action<bool> managerModeChanged)
    {
        _controller = controller;
        _themeService = themeService;
        _hostForm = hostForm;
        _onManagerModeChanged = managerModeChanged;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true;

        InitializeResponsiveLayout();
    }

    /// <summary>
    /// Clears and rebuilds the full settings layout, including the page header and three
    /// settings cards: Security and Privacy for PIN changes, Admin Credentials for admin
    /// password updates, and Admin Privileges for manager mode unlock.
    /// </summary>
    private void InitializeResponsiveLayout()
    {
        this.Controls.Clear();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            RowCount = 4,
            AutoSize = true,
            Padding = new Padding(50),
            BackColor = Color.Transparent
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        // 1. Page header
        var lblHeader = new Label
        {
            Text = "APPLICATION SETTINGS",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 40)
        };

        // 2. PIN change card
        var txtNewPin = CreateLargeTextBox("Enter New 4-Digit PIN");
        var btnChangePin = CreateLargeButton("UPDATE SECURITY PIN", Color.FromArgb(0, 122, 204));
        btnChangePin.Click += (_, _) => HandlePinChange(txtNewPin);
        var pinCard = CreateSettingsGroup("SECURITY & PRIVACY", "Change your primary access code for the application.", txtNewPin, btnChangePin);

        // 3. Admin password change card
        var txtAdminPass = CreateLargeTextBox("Enter New Admin Password");
        var btnAdminPass = CreateLargeButton("UPDATE ADMIN PASSWORD", ColorPalette.WarningOrange);
        btnAdminPass.Click += (_, _) => HandleAdminPasswordChange(txtAdminPass);
        var adminPassCard = CreateSettingsGroup("ADMIN CREDENTIALS", "Update the administrator password for manager panel access.", txtAdminPass, btnAdminPass);

        // 4. Manager mode unlock card
        var txtManagerPin = CreateLargeTextBox("Enter Admin PIN to Unlock");
        var btnEnableManager = CreateLargeButton("UNLOCK MANAGER MODE", ColorPalette.AccentGreen);
        btnEnableManager.Click += (_, _) => HandleManagerUnlock(txtManagerPin);
        var managerCard = CreateSettingsGroup("ADMIN PRIVILEGES", "Unlock advanced editing features and system administration.", txtManagerPin, btnEnableManager);

        mainLayout.Controls.Add(lblHeader);
        mainLayout.Controls.Add(pinCard);
        mainLayout.Controls.Add(adminPassCard);
        mainLayout.Controls.Add(managerCard);

        this.Controls.Add(mainLayout);
    }

    /// <summary>
    /// Creates a styled settings card panel containing a bold title, a descriptive subtitle,
    /// and a horizontal row with an optional input field followed by an action button.
    /// </summary>
    /// <param name="title">The bold heading displayed at the top of the card.</param>
    /// <param name="description">The descriptive subtitle shown below the heading.</param>
    /// <param name="input">An optional password text box placed before the action button; may be null.</param>
    /// <param name="action">The action button placed in the card's control row.</param>
    /// <returns>A styled <see cref="Panel"/> ready to be added to the settings layout.</returns>
    private Panel CreateSettingsGroup(string title, string description, TextBox? input, Button action)
    {
        var groupPanel = new Panel
        {
            Width = 1000,
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

    /// <summary>
    /// Creates a styled password text box with a dark background, white text, and the
    /// specified placeholder text.
    /// </summary>
    /// <param name="placeholder">The placeholder text displayed when the field is empty.</param>
    /// <returns>A <see cref="TextBox"/> configured for secure password input.</returns>
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

    /// <summary>
    /// Creates a styled action button with rounded corners, flat appearance, white text,
    /// hand cursor, and the specified label and background color.
    /// </summary>
    /// <param name="text">The button label text.</param>
    /// <param name="color">The background color of the button.</param>
    /// <returns>A fully styled <see cref="Button"/> ready to be placed in a settings card.</returns>
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

    /// <summary>
    /// Validates that the entered PIN is at least 4 characters, submits the change via the
    /// controller, clears the input field, and shows a confirmation message. Displays a
    /// validation warning and aborts if the PIN is too short.
    /// </summary>
    /// <param name="txt">The text box containing the new PIN entered by the user.</param>
    private void HandlePinChange(TextBox txt)
    {
        if (txt.Text.Length < 4) { MessageBox.Show("PIN must be at least 4 digits."); return; }
        _controller.ChangePin(txt.Text);
        txt.Clear();
        MessageBox.Show("Security PIN Updated!");
    }

    /// <summary>
    /// Validates that the entered admin password is non-empty and at least 6 characters,
    /// submits the change via the controller, clears the input field, and shows a success
    /// message. Displays a validation warning and aborts if either check fails.
    /// </summary>
    /// <param name="txt">The text box containing the new admin password entered by the user.</param>
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

    /// <summary>
    /// Verifies the entered PIN against the current user's credentials via the controller,
    /// then invokes the manager mode callback with the result. Clears the input field and
    /// shows either an access granted or access denied message depending on the outcome.
    /// </summary>
    /// <param name="txt">The text box containing the PIN entered by the user for verification.</param>
    private void HandleManagerUnlock(TextBox txt)
    {
        var isAuthorized = _controller.VerifyCurrentPin(txt.Text);
        _onManagerModeChanged(isAuthorized);
        txt.Clear();
        if (isAuthorized) MessageBox.Show("Manager Mode Active!");
        else MessageBox.Show("Access Denied: Wrong PIN.");
    }
}
