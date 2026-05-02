using Masroofy.App.Controllers;
using Masroofy.App.Assets;
using Masroofy.App.Services;
using Masroofy.App.Views.Components;

namespace Masroofy.App.Views.Forms;

public sealed class MainForm : Form
{

    private readonly AppController _controller;
    private readonly ThemeManager _themeService;
    private readonly List<Button> _navButtons = [];
    private readonly Panel _panelContent = new() { Dock = DockStyle.Fill, Name = "PanelContent" };

    // تعريف الصفحات (Views)
    private readonly DashboardView _dashboardView;
    private readonly HistoryView _historyView;
    private readonly DebtTrackerView _debtView;
    private readonly AnalyticsView _analyticsView;
    private readonly TutorialView _tutorialView;
    private readonly AdminPanelView _adminView;
    private readonly SettingsView _settingsView;

    private bool _managerModeEnabled;
    private Button? _btnDashboard, _btnHistory, _btnDebt, _btnAnalytics, _btnTutorial, _btnAdmin, _btnSettings;

    public MainForm(AppController controller, ThemeManager themeService)
    {


        _controller = controller;
        _themeService = themeService;

        // تهيئة الصفحات مع تمرير المراجع اللازمة
        // 1. أنشئ الـ Dashboard أولاً
        _dashboardView = new DashboardView(_controller, _themeService, this);
        // 2. مرر الـ dashboard لباقي الصفحات
        _historyView = new HistoryView(_controller, _dashboardView);
        _debtView = new DebtTrackerView(_controller, _dashboardView);
        _adminView = new AdminPanelView(_controller, _dashboardView);
        _analyticsView = new AnalyticsView(controller, _dashboardView);
        _tutorialView = new TutorialView(controller, _dashboardView);
        // حذفنا _dashboardView من النص لأن الكنترولر بيقوم بالواجب
        _settingsView = new SettingsView(controller, themeService, this, enabled =>
        {
            _managerModeEnabled = enabled;
            _historyView.SetManagerMode(enabled); // تفعيل التعديل/الحذف في الهيستوري تلقائياً
        });

        // إعدادات النافذة الرئيسية
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        KeyPreview = true; // ضروري لتفعيل اختصارات الكيبورد
        Text = $"Masroofy Omni-Edition - {controller.CurrentUserName}";
        KeyDown += MainForm_KeyDown;

        var sidebar = BuildSidebar();
        Controls.Add(_panelContent);
        Controls.Add(sidebar);

        // تشغيل الصفحة الرئيسية افتراضياً
        ShowView(_dashboardView);
        if (_btnDashboard != null) SetActiveNav(_btnDashboard);

        _themeService.ApplyTheme(this);
    }

    private Control BuildSidebar()
    {
        var sidebar = new Panel { Dock = DockStyle.Left, Width = 260, BackColor = ColorPalette.DarkSurface };

        var lblBrand = new Label
        {
            Text = "Masroofy", // اسم البراند الخاص بك
            Dock = DockStyle.Top,
            Height = 80,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen
        };

        // إنشاء الأزرار
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

        // ربط الأحداث (Events)
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

    private Button CreateNavBtn(string text, int index)
    {
        var btn = new Button
        {
            Text = $"  {text}", // مسافة بسيطة لترك مجال للأيقونات مستقبلاً
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

    private void ShowView(Control view)
    {
        if (!_panelContent.Controls.Contains(view))
        {
            _panelContent.Controls.Clear();
            view.Dock = DockStyle.Fill; // هذا السطر هو الأهم لاستغلال كامل المساحة
            _panelContent.Controls.Add(view);
        }

        // تحديث البيانات في الصفحة المختارة فور ظهورها
        if (view is DashboardView dash) dash.RefreshCategories();
        else if (view is HistoryView history) history.Reload();
        else if (view is AnalyticsView analytics) analytics.Reload();
        else if (view is TutorialView tutorial) tutorial.Reload();
        else if (view is DebtTrackerView debt) debt.LoadDebts();

        _themeService.ApplyTheme(this);
    }

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

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        // التعامل مع الاختصارات Ctrl + Number
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