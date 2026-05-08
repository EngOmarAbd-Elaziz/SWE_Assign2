using Masroofy.App.Controllers;
using Masroofy.App.Assets;
using Masroofy.App.Services;
using Masroofy.App.Views.Components;

namespace Masroofy.App.Views.Forms;

/// <summary>
/// The application's main window, rendered as a borderless maximized form. Hosts a fixed
/// left sidebar for navigation and a content panel that swaps between all major views:
/// Dashboard, History, Debt Tracker, Analytics, Tutorial, Manager Panel, and Settings.
/// Supports keyboard shortcuts for navigation and theme toggling, and enforces manager
/// mode access control on the Admin Panel.
/// </summary>
public sealed class MainForm : Form
{
    private readonly AppController _controller;
    private readonly ThemeManager _themeService;
    private readonly List<Button> _navButtons = [];
    private readonly Panel _panelContent = new() { Dock = DockStyle.Fill, Name = "PanelContent" };

    private readonly DashboardView _dashboardView;
    private readonly HistoryView _historyView;
    private readonly DebtTrackerView _debtView;
    private readonly AnalyticsView _analyticsView;
    private readonly TutorialView _tutorialView;
    private readonly AdminPanelView _adminView;
    private readonly SettingsView _settingsView;

    private bool _managerModeEnabled;
    private Button? _btnDashboard, _btnHistory, _btnDebt, _btnAnalytics, _btnTutorial, _btnAdmin, _btnSettings;

    /// <summary>
    /// Initializes a new instance of <see cref="MainForm"/>, constructs all views in
    /// dependency order, builds the sidebar, wires keyboard shortcuts, shows the dashboard
    /// as the default view, and applies the current theme.
    /// </summary>
    /// <param name="controller">The application controller shared across all views.</param>
    /// <param name="themeService">The theme manager used to apply and toggle the visual theme.</param>
    public MainForm(AppController controller, ThemeManager themeService)
    {
        _controller = controller;
        _themeService = themeService;

        _dashboardView = new DashboardView(_controller, _themeService, this);
        _historyView = new HistoryView(_controller, _dashboardView);
        _debtView = new DebtTrackerView(_controller, _dashboardView);
        _adminView = new AdminPanelView(_controller, _dashboardView);
        _analyticsView = new AnalyticsView(controller, _dashboardView);
        _tutorialView = new TutorialView(controller, _dashboardView);
        _settingsView = new SettingsView(controller, themeService, this, enabled =>
        {
            _managerModeEnabled = enabled;
            _historyView.SetManagerMode(enabled);
        });

        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        Text = $"Masroofy Omni-Edition - {controller.CurrentUserName}";
        KeyDown += MainForm_KeyDown;

        var sidebar = BuildSidebar();
        Controls.Add(_panelContent);
        Controls.Add(sidebar);

        ShowView(_dashboardView);
        if (_btnDashboard != null) SetActiveNav(_btnDashboard);

        _themeService.ApplyTheme(this);
    }

    /// <summary>
    /// Builds the left navigation sidebar containing the brand label, all navigation buttons,
    /// and an Exit button docked to the bottom. Wires each button to its corresponding view,
    /// with the Manager Panel button guarded by a manager mode check.
    /// </summary>
    /// <returns>A <see cref="Panel"/> representing the fully constructed sidebar.</returns>
    private Control BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = ColorPalette.DarkSurface };

        var lblBrand = new Label
        {
            Text = "Masroofy",
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen
        };

        _btnDashboard = CreateNavBtn("Dashboard", 1);
        _btnHistory = CreateNavBtn("History", 2);
        _btnDebt = CreateNavBtn("Debt Tracker", 3);
        _btnAnalytics = CreateNavBtn("Analytics", 4);
        _btnTutorial = CreateNavBtn("Tutorial", 5);
        _btnAdmin = CreateNavBtn("Manager Panel", 6);
        _btnSettings = CreateNavBtn("Settings", 7);

        var btnExit = new Button { Text = "Exit Application", Dock = DockStyle.Bottom, Height = 60, FlatStyle = FlatStyle.Flat, BackColor = ColorPalette.DarkSurface, ForeColor = Color.LightGray };
        UiStyleService.StyleNavButton(btnExit);
        btnExit.Click += (_, _) => Application.Exit();

        _btnDashboard.Click += (_, _) => { SetActiveNav(_btnDashboard); ShowView(_dashboardView); };
        _btnHistory.Click += (_, _) => { SetActiveNav(_btnHistory); ShowView(_historyView); };
        _btnDebt.Click += (_, _) => { SetActiveNav(_btnDebt); ShowView(_debtView); };
        _btnAnalytics.Click += (_, _) => { SetActiveNav(_btnAnalytics); ShowView(_analyticsView); };
        _btnTutorial.Click += (_, _) => { SetActiveNav(_btnTutorial); ShowView(_tutorialView); };
        _btnSettings.Click += (_, _) => { SetActiveNav(_btnSettings); ShowView(_settingsView); };
        _btnAdmin.Click += (_, _) =>
        {
            if (!_managerModeEnabled)
            {
                MessageBox.Show("Please unlock Manager Mode from Settings first.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SetActiveNav(_btnAdmin);
            ShowView(_adminView);
        };

        _navButtons.AddRange([_btnDashboard, _btnHistory, _btnDebt, _btnAnalytics, _btnTutorial, _btnAdmin, _btnSettings]);

        sidebar.Controls.AddRange([btnExit, _btnSettings, _btnAdmin, _btnTutorial, _btnAnalytics, _btnDebt, _btnHistory, _btnDashboard, lblBrand]);
        return sidebar;
    }

    /// <summary>
    /// Creates a styled navigation button with left-aligned text, hover background transitions,
    /// and a hand cursor. The button is docked to the top of the sidebar.
    /// </summary>
    /// <param name="text">The display label for the navigation button.</param>
    /// <param name="index">The position index of the button, reserved for future icon mapping.</param>
    /// <returns>A fully styled navigation <see cref="Button"/>.</returns>
    private Button CreateNavBtn(string text, int index)
    {
        var btn = new Button
        {
            Text = $"  {text}",
            Dock = DockStyle.Top,
            Height = 55,
            TextAlign = ContentAlignment.MiddleLeft,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        UiStyleService.StyleNavButton(btn);

        btn.MouseEnter += (_, _) =>
        {
            if (btn.Tag?.ToString() != "active") btn.BackColor = Color.FromArgb(40, 40, 43);
        };
        btn.MouseLeave += (_, _) =>
        {
            if (btn.Tag?.ToString() != "active") btn.BackColor = Color.Transparent;
        };

        return btn;
    }

    /// <summary>
    /// Displays the given view in the content panel, replacing any previously shown control.
    /// Triggers a data refresh appropriate to the view type immediately after it is shown,
    /// then reapplies the current theme to the entire form.
    /// </summary>
    /// <param name="view">The view control to display in the content panel.</param>
    private void ShowView(Control view)
    {
        if (!_panelContent.Controls.Contains(view))
        {
            _panelContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            _panelContent.Controls.Add(view);
        }

        if (view is DashboardView dash) dash.RefreshCategories();
        else if (view is HistoryView history) history.Reload();
        else if (view is AnalyticsView analytics) analytics.Reload();
        else if (view is TutorialView tutorial) tutorial.Reload();
        else if (view is DebtTrackerView debt) debt.LoadDebts();

        _themeService.ApplyTheme(this);
    }

    /// <summary>
    /// Marks the given button as the active navigation item by setting its tag, background,
    /// and foreground to the active accent style, and resets all other navigation buttons
    /// to their default inactive appearance.
    /// </summary>
    /// <param name="active">The navigation button to mark as active.</param>
    private void SetActiveNav(Button active)
    {
        foreach (var btn in _navButtons)
        {
            btn.Tag = null;
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Color.DarkGray;
        }

        active.Tag = "active";
        active.BackColor = ColorPalette.AccentGreen;
        active.ForeColor = Color.White;
    }

    /// <summary>
    /// Handles keyboard shortcuts for the main form. Ctrl+1 through Ctrl+6 simulate clicks
    /// on the corresponding navigation buttons. Ctrl+T toggles the application theme.
    /// Escape prompts the user to confirm before closing the application.
    /// </summary>
    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.D1: _btnDashboard?.PerformClick(); break;
                case Keys.D2: _btnHistory?.PerformClick(); break;
                case Keys.D3: _btnDebt?.PerformClick(); break;
                case Keys.D4: _btnAnalytics?.PerformClick(); break;
                case Keys.D5: _btnAdmin?.PerformClick(); break;
                case Keys.D6: _btnSettings?.PerformClick(); break;
                case Keys.T: _themeService.ToggleTheme(this); break;
            }
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.Escape)
        {
            if (MessageBox.Show("Exit Masroofy?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes) Close();
        }
    }
}
