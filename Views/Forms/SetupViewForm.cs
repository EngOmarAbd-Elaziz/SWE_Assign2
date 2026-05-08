using Masroofy.App.Assets;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Forms;

/// <summary>
/// A fixed-dialog form displayed on first run after admin registration, allowing the user
/// to configure their name, PIN, starting balance, and budget duration to initialize their
/// first budget cycle. Closes with <see cref="DialogResult.OK"/> on successful setup.
/// </summary>
public sealed class SetupViewForm : Form
{
    private readonly InitialSetupService _setupService;
    private readonly TextBox _txtUser = new() { Width = 320, Height = 35, PlaceholderText = "Your Name (e.g., Omar)", Font = new Font("Segoe UI", 11) };
    private readonly TextBox _txtPin = new() { Width = 320, Height = 35, PlaceholderText = "Set 4-Digit PIN", PasswordChar = '*', Font = new Font("Segoe UI", 11) };
    private readonly NumericUpDown _numBalance = new() { DecimalPlaces = 2, Maximum = 1_000_000, Width = 320, Font = new Font("Segoe UI", 11) };
    private readonly NumericUpDown _numDays = new() { Minimum = 1, Maximum = 365, Value = 30, Width = 320, Font = new Font("Segoe UI", 11) };
    private readonly Label _lblStatus = new() { AutoSize = true, ForeColor = ColorPalette.OverspentRed, Margin = new Padding(0, 10, 0, 0) };

    /// <summary>
    /// Initializes a new instance of <see cref="SetupViewForm"/>, builds the setup layout
    /// with labeled input groups for name, PIN, starting balance, and budget duration,
    /// and wires the Create My Budget button to validate inputs and invoke the setup service.
    /// </summary>
    /// <param name="setupService">The setup service used to register the user and create the initial budget cycle.</param>
    public SetupViewForm(InitialSetupService setupService)
    {
        _setupService = setupService;
        Text = "Masroofy - Initial Configuration";
        BackColor = ColorPalette.DarkBackground;
        ForeColor = ColorPalette.DarkText;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(380, 520);

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

        layout.Controls.Add(new Label
        {
            Text = "First-Time Setup",
            AutoSize = true,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 255, 127),
            Margin = new Padding(0, 0, 0, 25)
        });

        AddInputGroup(layout, "Full Name", _txtUser);
        AddInputGroup(layout, "Security PIN", _txtPin);
        AddInputGroup(layout, "Starting Balance (EGP)", _numBalance);
        AddInputGroup(layout, "Budget Duration (Days)", _numDays);

        layout.Controls.Add(btnSetup);
        layout.Controls.Add(_lblStatus);
        Controls.Add(layout);
    }

    /// <summary>
    /// Adds a labeled input group to the given flow layout, consisting of a small bold
    /// label above the provided input control, with consistent top margin spacing.
    /// </summary>
    /// <param name="parent">The flow layout panel to add the label and input control to.</param>
    /// <param name="labelText">The descriptive label text displayed above the input control.</param>
    /// <param name="input">The input control to place beneath the label.</param>
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
