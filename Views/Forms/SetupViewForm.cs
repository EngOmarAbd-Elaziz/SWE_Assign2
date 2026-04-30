using Masroofy.App.Assets;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Forms;

public sealed class SetupViewForm : Form
{
    private readonly InitialSetupService _setupService;
    private readonly TextBox _txtUser = new() { Width = 320, Height = 35, PlaceholderText = "Your Name (e.g., Omar)", Font = new Font("Segoe UI", 11) };
    private readonly TextBox _txtPin = new() { Width = 320, Height = 35, PlaceholderText = "Set 4-Digit PIN", PasswordChar = '*', Font = new Font("Segoe UI", 11) };
    private readonly NumericUpDown _numBalance = new() { DecimalPlaces = 2, Maximum = 1_000_000, Width = 320, Font = new Font("Segoe UI", 11) };
    private readonly NumericUpDown _numDays = new() { Minimum = 1, Maximum = 365, Value = 30, Width = 320, Font = new Font("Segoe UI", 11) };
    private readonly Label _lblStatus = new() { AutoSize = true, ForeColor = ColorPalette.OverspentRed, Margin = new Padding(0, 10, 0, 0) };

    public SetupViewForm(InitialSetupService setupService)
    {
        _setupService = setupService;
        Text = "Masroofy - Initial Configuration";
        BackColor = ColorPalette.DarkBackground; // خلفية داكنة متناسقة
        ForeColor = ColorPalette.DarkText;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(380, 520); // زيادة الارتفاع لراحة التنسيق

        var btnSetup = new Button
        {
            Text = "Create My Budget",
            Width = 320,
            Height = 45,
            BackColor = Color.FromArgb(0, 122, 204),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 11),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 20, 0, 0)
        };
        UiStyleService.ApplyRoundedCorners(btnSetup, 10);

        btnSetup.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_txtUser.Text) || string.IsNullOrWhiteSpace(_txtPin.Text))
            {
                _lblStatus.Text = "Please fill all identity fields.";
                return;
            }

            var ok = _setupService.Setup(_txtUser.Text.Trim(), _txtPin.Text, _numBalance.Value, (int)_numDays.Value, out var msg);
            _lblStatus.Text = msg;

            if (ok)
            {
                _lblStatus.ForeColor = Color.SpringGreen;
                MessageBox.Show("Welcome to Masroofy! Your budget has been initialized.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(30),
            WrapContents = false
        };

        // الهيدر
        layout.Controls.Add(new Label
        {
            Text = "First-Time Setup",
            AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 255, 127), // Glow Color
            Margin = new Padding(0, 0, 0, 25)
        });

        // حقول الإدخال مع التسميات
        AddInputGroup(layout, "Full Name", _txtUser);
        AddInputGroup(layout, "Security PIN", _txtPin);
        AddInputGroup(layout, "Starting Balance (EGP)", _numBalance);
        AddInputGroup(layout, "Budget Duration (Days)", _numDays);

        layout.Controls.Add(btnSetup);
        layout.Controls.Add(_lblStatus);

        Controls.Add(layout);
    }

    private void AddInputGroup(FlowLayoutPanel parent, string labelText, Control input)
    {
        parent.Controls.Add(new Label
        {
            Text = labelText,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 9),
            ForeColor = Color.DarkGray,
            Margin = new Padding(0, 10, 0, 3)
        });
        parent.Controls.Add(input);
    }
}