using Masroofy.App.Services;

namespace Masroofy.App.Views.Forms;

public sealed class FirstRunWizardForm : Form
{
    private readonly IAuthService _auth;
    private readonly TextBox _txtName = new() { PlaceholderText = "Admin username" };
    private readonly TextBox _txtPin = new() { PlaceholderText = "Admin PIN", PasswordChar = '*' };
    private readonly TextBox _txtMaster = new() { PlaceholderText = "Secret master key", PasswordChar = '*' };
    private readonly Label _lblStatus = new() { AutoSize = true };

    public FirstRunWizardForm(IAuthService auth)
    {
        _auth = auth;
        Text = "Masroofy First Run Setup";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(360, 240);

        var btnCreate = new Button { Text = "Create Admin", Width = 300, Height = 34 };
        btnCreate.Click += (_, _) =>
        {
            var ok = _auth.Register(_txtName.Text, _txtPin.Text, "Admin", _txtMaster.Text, out var message);
            _lblStatus.Text = message;
            if (ok)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(24),
            WrapContents = false
        };
        _txtName.Width = _txtPin.Width = _txtMaster.Width = 300;
        layout.Controls.AddRange([_txtName, _txtPin, _txtMaster, btnCreate, _lblStatus]);
        Controls.Add(layout);
    }
}
