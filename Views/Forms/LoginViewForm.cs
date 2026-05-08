using Masroofy.App.Assets;
using Masroofy.App.Models;
using Masroofy.App.Services;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Masroofy.App.Views.Forms;

/// <summary>
/// A borderless, rounded login form that presents a user selector and a 4-digit PIN keypad.
/// Authenticates the selected user via the auth service and exposes the result through
/// <see cref="AuthenticatedUser"/>. Plays a shake animation on failed login attempts.
/// Closes with <see cref="DialogResult.OK"/> on successful authentication.
/// </summary>
public sealed class LoginViewForm : Form
{
    private readonly IAuthService _auth;
    private readonly ComboBox _cmbUsers = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Height = 45,
        BackColor = Color.FromArgb(45, 45, 48),
        ForeColor = Color.White,
        FlatStyle = FlatStyle.Flat
    };

    private readonly Label _lblStatus = new()
    {
        AutoSize = true,
        ForeColor = Color.Tomato,
        Margin = new Padding(0, 10, 0, 0),
        Font = new Font("Segoe UI", 10, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleCenter,
        Text = "Enter your 4-digit PIN"
    };

    private readonly FlowLayoutPanel _pinDots = new()
    {
        Width = 360,
        Height = 40,
        FlowDirection = FlowDirection.LeftToRight,
        Padding = new Padding(85, 5, 0, 0),
        Margin = new Padding(0, 15, 0, 10)
    };

    private readonly string[] _pinDigits = ["", "", "", ""];
    private int _pinPos;
    private readonly System.Windows.Forms.Timer _shakeTimer = new() { Interval = 16 };
    private int _shakeStep;
    private int _originalLeft;

    /// <summary>
    /// Gets the authenticated <see cref="User"/> after a successful login, or null if
    /// authentication has not yet succeeded.
    /// </summary>
    public User? AuthenticatedUser { get; private set; }

    /// <summary>
    /// Initializes a new instance of <see cref="LoginViewForm"/>, builds the borderless
    /// rounded layout with a brand label, user selector, PIN dot indicators, and numeric
    /// keypad, then populates the user dropdown from the provided list.
    /// </summary>
    /// <param name="auth">The authentication service used to verify username and PIN combinations.</param>
    /// <param name="users">The list of registered usernames to populate the user selector.</param>
    public LoginViewForm(IAuthService auth, IReadOnlyList<string> users)
    {
        _auth = auth;

        Text = "Masroofy - Secure Access";
        BackColor = ColorPalette.DarkBackground;
        ForeColor = ColorPalette.DarkText;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;

        int formWidth = 450;
        int formHeight = 720;
        ClientSize = new Size(formWidth, formHeight);

        Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, formWidth, formHeight, 40, 40));

        foreach (var user in users) _cmbUsers.Items.Add(user);
        if (_cmbUsers.Items.Count > 0) _cmbUsers.SelectedIndex = 0;
        _cmbUsers.Font = new Font("Segoe UI", 12);

        BuildPinDots();
        var keypad = BuildKeypad();

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(45, 40, 45, 40),
            WrapContents = false
        };

        var lblBrand = new Label
        {
            Text = "Masroofy",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(70, 0, 0, 30)
        };

        _cmbUsers.Width = 360;
        layout.Controls.AddRange([lblBrand, _cmbUsers, _pinDots, keypad, _lblStatus]);
        Controls.Add(layout);

        _shakeTimer.Tick += ShakeTimer_Tick;
    }

    /// <summary>
    /// Clears and rebuilds the four PIN indicator dots, rendering each as an unfilled
    /// rounded circle in the default surface color.
    /// </summary>
    private void BuildPinDots()
    {
        _pinDots.Controls.Clear();
        for (var i = 0; i < 4; i++)
        {
            var dot = new Panel
            {
                Width = 24,
                Height = 24,
                BackColor = Color.FromArgb(60, 60, 60),
                Margin = new Padding(12, 0, 12, 0)
            };
            UiStyleService.ApplyRoundedCorners(dot, 12);
            _pinDots.Controls.Add(dot);
        }
    }

    /// <summary>
    /// Builds the 3x4 numeric keypad grid containing digits 0–9, a clear key, and a
    /// backspace key. Each key is styled as a rounded dark button and wired to <see cref="OnKey"/>.
    /// </summary>
    /// <returns>A <see cref="TableLayoutPanel"/> containing all keypad buttons.</returns>
    private Control BuildKeypad()
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = 3,
            RowCount = 4,
            Width = 360,
            Height = 380,
            Margin = new Padding(0, 10, 0, 10)
        };

        for (var i = 0; i < 3; i++) grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        for (var i = 0; i < 4; i++) grid.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));

        var nums = new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "⌫" };
        for (var i = 0; i < nums.Length; i++)
        {
            var key = nums[i];
            var btn = new Button
            {
                Text = key,
                Width = 85,
                Height = 85,
                Margin = new Padding(12, 5, 12, 5),
                Font = new Font("Segoe UI Semibold", 18),
                BackColor = Color.FromArgb(35, 35, 38),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 55, 60);

            UiStyleService.ApplyRoundedCorners(btn, 42);

            btn.Click += (_, _) => OnKey(key);
            grid.Controls.Add(btn, i % 3, i / 3);
        }

        return grid;
    }

    /// <summary>
    /// Handles a keypad key press. C clears all PIN digits, ⌫ removes the last entered digit,
    /// and digit keys append to the PIN buffer. Triggers <see cref="AttemptLogin"/> automatically
    /// when the fourth digit is entered.
    /// </summary>
    /// <param name="key">The key label pressed on the keypad.</param>
    private void OnKey(string key)
    {
        if (key == "C") { ClearPin(); return; }
        if (key == "⌫")
        {
            if (_pinPos > 0) _pinDigits[--_pinPos] = "";
            RenderDots();
            return;
        }
        if (_pinPos < 4)
        {
            _pinDigits[_pinPos++] = key;
            RenderDots();
            if (_pinPos == 4) AttemptLogin();
        }
    }

    /// <summary>
    /// Updates each PIN dot's background color to accent green if its corresponding digit
    /// has been entered, or back to the default surface color if it is empty.
    /// </summary>
    private void RenderDots()
    {
        for (var i = 0; i < _pinDots.Controls.Count; i++)
        {
            if (_pinDots.Controls[i] is Panel dot)
            {
                bool isFilled = !string.IsNullOrWhiteSpace(_pinDigits[i]);
                dot.BackColor = isFilled ? ColorPalette.AccentGreen : ColorPalette.DarkSurface;
                dot.BorderStyle = BorderStyle.None;
            }
        }
    }

    /// <summary>
    /// Concatenates the four PIN digits and attempts authentication via the auth service.
    /// On success, sets <see cref="AuthenticatedUser"/> and closes the form with
    /// <see cref="DialogResult.OK"/>. On failure, updates the status label, triggers the
    /// shake animation, and clears the PIN buffer.
    /// </summary>
    private void AttemptLogin()
    {
        if (_cmbUsers.SelectedItem is not string userName) return;
        var pin = string.Concat(_pinDigits);
        var user = _auth.Authenticate(userName, pin);

        if (user != null)
        {
            AuthenticatedUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            _lblStatus.Text = "INVALID PIN - ACCESS DENIED";
            _lblStatus.ForeColor = Color.Tomato;
            ShakeWindow();
            ClearPin();
        }
    }

    /// <summary>
    /// Clears all four PIN digit entries, resets the position counter to zero, and
    /// re-renders the PIN dots to their empty state.
    /// </summary>
    private void ClearPin()
    {
        for (var i = 0; i < _pinDigits.Length; i++) _pinDigits[i] = "";
        _pinPos = 0;
        RenderDots();
    }

    /// <summary>
    /// Starts the shake animation by recording the form's current left position and
    /// resetting the step counter. Has no effect if the animation is already running.
    /// </summary>
    private void ShakeWindow()
    {
        if (_shakeTimer.Enabled) return;
        _originalLeft = this.Left;
        _shakeStep = 0;
        _shakeTimer.Start();
    }

    /// <summary>
    /// Advances the shake animation by applying a progressively smaller horizontal offset
    /// to the form's left position on each timer tick. Stops the timer and restores the
    /// original position after six steps.
    /// </summary>
    private void ShakeTimer_Tick(object? sender, EventArgs e)
    {
        _shakeStep++;
        var offset = _shakeStep switch
        {
            1 => -12,
            2 => 12,
            3 => -8,
            4 => 8,
            5 => -4,
            6 => 4,
            _ => 0
        };

        this.Left = _originalLeft + offset;

        if (_shakeStep >= 6)
        {
            _shakeTimer.Stop();
            this.Left = _originalLeft;
        }
    }
}

/// <summary>
/// Provides P/Invoke declarations for native Win32 GDI functions used to apply
/// rounded region clipping to borderless forms.
/// </summary>
internal static class NativeMethods
{
    /// <summary>
    /// Creates a rounded rectangular region that can be assigned to a form's
    /// <see cref="Control.Region"/> property to produce rounded corners on borderless windows.
    /// </summary>
    /// <param name="nLeftRect">The x-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="nTopRect">The y-coordinate of the upper-left corner of the rectangle.</param>
    /// <param name="nRightRect">The x-coordinate of the lower-right corner of the rectangle.</param>
    /// <param name="nBottomRect">The y-coordinate of the lower-right corner of the rectangle.</param>
    /// <param name="nWidthEllipse">The width of the ellipse used to round the corners.</param>
    /// <param name="nHeightEllipse">The height of the ellipse used to round the corners.</param>
    /// <returns>A handle to the rounded rectangular region.</returns>
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
}
