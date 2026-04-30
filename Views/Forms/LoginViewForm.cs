using Masroofy.App.Assets;
using Masroofy.App.Models;
using Masroofy.App.Services;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Masroofy.App.Views.Forms;

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
        TextAlign = ContentAlignment.MiddleCenter
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
    public User? AuthenticatedUser { get; private set; }

    public LoginViewForm(IAuthService auth, IReadOnlyList<string> users)
    {
        _auth = auth;

        // إعدادات النافذة الرئيسية (ATM Style)
        Text = "Masroofy - Secure Access";
        BackColor = ColorPalette.DarkBackground;
        ForeColor = ColorPalette.DarkText;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;

        // زيادة الحجم لضمان ظهور كل شيء بوضوح
        int formWidth = 450;
        int formHeight = 720;
        ClientSize = new Size(formWidth, formHeight);

        // حواف دائرية للفورم نفسه
        Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, formWidth, formHeight, 40, 40));

        foreach (var user in users) _cmbUsers.Items.Add(user);
        if (_cmbUsers.Items.Count > 0) _cmbUsers.SelectedIndex = 0;
        _cmbUsers.Font = new Font("Segoe UI", 12);

        BuildPinDots();
        var keypad = BuildKeypad();

        var btnLogin = new Button
        {
            Text = "UNLOCK SYSTEM",
            Width = 360,
            Height = 55,
            BackColor = ColorPalette.AccentGreen,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI Semibold", 13),
            Cursor = Cursors.Hand,
            Margin = new Padding(0, 20, 0, 0)
        };
        btnLogin.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btnLogin, 15);

        btnLogin.Click += (_, _) => AttemptLogin();

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(45, 40, 45, 40),
            WrapContents = false
        };

        var lblBrand = new Label
        {
            Text = "LUX GLOW",
            Font = new Font("Segoe UI", 26, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(70, 0, 0, 30)
        };

        _cmbUsers.Width = 360;
        layout.Controls.AddRange([lblBrand, _cmbUsers, _pinDots, keypad, btnLogin, _lblStatus]);
        Controls.Add(layout);
    }

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

            // جعل الأزرار دائرية تماماً
            UiStyleService.ApplyRoundedCorners(btn, 42);

            btn.Click += (_, _) => OnKey(key);
            grid.Controls.Add(btn, i % 3, i / 3);
        }

        return grid;
    }

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
            ClearPin();
        }
    }

    private void ClearPin()
    {
        for (var i = 0; i < _pinDigits.Length; i++) _pinDigits[i] = "";
        _pinPos = 0;
        RenderDots();
    }
}

internal static class NativeMethods
{
    [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
    public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
}