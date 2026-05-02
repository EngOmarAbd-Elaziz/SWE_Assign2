using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Services;
using System.Drawing;
using System.Windows.Forms;

namespace Masroofy.App.Views.Components;

public sealed class TutorialView : UserControl
{
    private readonly AppController _controller;
    private readonly TabControl _tabControl = new()
    {
        Dock = DockStyle.Fill,
        Appearance = TabAppearance.FlatButtons,
        ItemSize = new Size(0, 1)
    };

    public TutorialView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        this.DoubleBuffered = true;
        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;

        // زدنا عدد الصفحات لـ 8 لتغطية كل التفاصيل
        for (int i = 0; i < 8; i++)
            _tabControl.TabPages.Add(new TabPage { BackColor = ColorPalette.DarkBackground, AutoScroll = true });

        SetupNavigation();

        this.Load += (s, e) => Reload();
        this.SizeChanged += (s, e) => Reload();
    }

    private void SetupNavigation()
    {
        var navPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 85,
            Padding = new Padding(40, 25, 0, 0),
            BackColor = Color.FromArgb(10, 10, 10),
            AutoScroll = true,
            WrapContents = false
        };

        string[] tabs = { "Vision", "Interface", "Safe Engine", "Transaction Mastery", "Debt Law", "Emergency", "Admin Center", "Strategy" };
        for (int i = 0; i < tabs.Length; i++) navPanel.Controls.Add(CreateNavButton(tabs[i], i));

        this.Controls.Add(_tabControl);
        this.Controls.Add(navPanel);
    }

    private Button CreateNavButton(string text, int index)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            Width = 140,
            Height = 40,
            ForeColor = index == 0 ? ColorPalette.AccentGreen : Color.Silver,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        btn.Click += (s, e) =>
        {
            _tabControl.SelectedIndex = index;
            if (btn.Parent != null)
            {
                foreach (Control c in btn.Parent.Controls) c.ForeColor = Color.Silver;
            }
            btn.ForeColor = ColorPalette.AccentGreen;
        };
        return btn;
    }

    public void Reload()
    {
        if (this.Width <= 100 || _tabControl.TabPages.Count == 0) return;

        this.SuspendLayout();
        foreach (TabPage page in _tabControl.TabPages) page.Controls.Clear();

        // محتوى ضخم جداً وشامل لكل جوانب النظام
        string[][] data = new string[][] {
            // 1. الرؤية العامة
            new[] {
                "The Masroofy Financial Operating System",
                "🌐 Beyond Budget Tracking: A Wealth Velocity Engine",
                "🏦 The Cash-Safe Limit Relationship|Masroofy's core innovation lies in the mathematical relationship between your actual cash position and the calculated Safe Daily Limit. The system doesn't just track spending—it creates a dynamic buffer that protects your financial future.|📊 Mathematical Foundation|The Safe Daily Limit is calculated as: (Remaining Balance + Debt Adjustments) ÷ Remaining Days. This creates a 'Liquidity Velocity Vector' that accelerates wealth accumulation by preventing premature spending depletion.|🛡️ Protection Against Financial Entropy|Without this relationship, traditional budgeting suffers from 'Surprise Depletion' where users spend freely early in the cycle, leaving insufficient funds for later obligations. Masroofy eliminates this through predictive mathematics.|💡 Cognitive Economics|The system leverages behavioral economics principles, using immediate feedback loops to encourage disciplined spending while rewarding surplus generation through compound growth effects."
            },
            // 2. شرح الواجهة
            new[] {
                "The Interface Architecture",
                "🖥️ Your Financial Command Center: A Dark-Themed Cockpit for Decision-Making",
                "🎯 Dashboard as Decision Engine|The Dashboard serves as your primary cockpit, displaying critical metrics with color-coded alerts: Safe Daily Limit, remaining cycle days, and budget health indicators.|📈 Analytics Deep-Dive|Interactive LiveCharts provide spending velocity analysis, category breakdowns, and predictive forecasting to identify spending patterns and optimize future behavior.|📋 Transaction Ledger|Complete expense history with advanced filtering, search capabilities, and CSV export functionality essential for tax preparation and financial forensics.|⚖️ Debt Management|Advanced liability tracking with strict separation between borrowing (increases RemainingBalance but flagged non-spendable) and lending (reduces available liquidity).|🔧 System Controls|Settings for user management, PIN security, and configuration options alongside Admin Panel for database integrity and comprehensive audit logging.|🎨 Visual Psychology|Strategic color usage: Green signals growth/safety, Red indicates danger zones, Silver denotes administrative functions—minimizing cognitive load while maximizing actionable insights."
            },
            // 3. المحرك المالي
            new[] {
                "The Safe Limit Engine",
                "🧠 Dynamic Budgetary Mathematics: The Algorithmic Heart of Financial Control",
                "🏦 Cash Position Integration|The engine begins with your actual cash position: Remaining Balance = Total Allowance - Cumulative Expenses. This represents your true liquid assets.|📅 Time-Based Distribution|Remaining Days calculation ensures equitable distribution: (End Date - Current Date) + 1, preventing front-loading of expenses.|💰 Debt Impact Adjustment|Virtual Deduction Architecture adds borrowing amounts but isolates them as non-spendable capital, while subtracting lending amounts to reflect true liquidity.|🔄 Daily Rollover Mechanism|Unspent daily allocations compound forward, creating wealth acceleration through consistent surplus generation.|🛡️ Overspend Protection|When daily spending exceeds calculated limits, the engine implements proportional reduction across remaining days to prevent total depletion.|📊 Self-Regulating System|Continuous recalculation based on real-time data creates a feedback loop that transforms static budgeting into dynamic wealth optimization."
            },
            // 4. إتقان المعاملات
            new[] {
                "Transaction Mastery",
                "💰 Precision Expense Tracking: Every Transaction as a Wealth Event",
                "⚡ Real-Time Impact|Each expense immediately reduces Remaining Balance and triggers Safe Daily Limit recalculation, maintaining mathematical accuracy throughout the cycle.|🏷️ Category Intelligence|Semantic classification enables pattern recognition: Food, Transport, Bills, Entertainment—each category contributes to behavioral analysis.|⏰ Temporal Precision|Millisecond timestamping ensures accurate daily aggregation and prevents timing-based calculation discrepancies.|📝 Audit Trail|Immutable log entries create forensic records for financial accountability and dispute resolution.|🔧 Administrative Controls|Edit/delete capabilities for data correction while preserving audit integrity through compensatory logging.|📊 Export Functionality|CSV output for external analysis, tax preparation, and integration with financial planning software.|🔒 Transaction Atomicity|Each operation is indivisible—either completes fully or rolls back entirely, preventing partial state corruption."
            },
            // 5. قانون الديون
            new[] {
                "Debt Law & Liability Management",
                "⚖️ Virtual Deduction Architecture: Invisible Liabilities Without Inflated Spending Power",
                "🚫 Psychological Protection|When you borrow money, Masroofy prevents the 'Rich Illusion'—the dangerous feeling that borrowed funds represent true wealth.|💰 Borrowing Logic|Incoming loans increase Remaining Balance for calculation purposes but receive a 'Liability Flag' that prevents expenditure on discretionary items.|📉 Lending Logic|Money given to others reduces Remaining Balance and creates receivables requiring settlement to restore full liquidity.|🔄 Settlement Mechanism|The 'Settle' function removes debt records and adjusts balances, 'unlocking' previously isolated funds back into operational liquidity.|📊 Net Worth Accuracy|Debts factor into overall financial position while maintaining strict separation from day-to-day spending power.|🛡️ Risk Mitigation|Borrowed capital cannot accidentally be spent on non-essential purchases, establishing a 'Debt Firewall' that protects wealth accumulation.|👁️ Transparency|Comprehensive audit trails enable precise liability tracking and settlement verification without compromising spending discipline."
            },
            // 6. بروتوكولات الطوارئ
            new[] {
                "Emergency Protocols",
                "🚨 Crisis Management Algorithms: Soft Landings for Financial Storms",
                "🚫 Zero Balance Alert|When Remaining Balance reaches absolute zero, all non-essential spending is blocked through interface-level restrictions and visual warnings.|📉 Overspend Recovery|Daily overspending triggers proportional limit reduction across remaining cycle days to prevent total depletion while allowing essential expenditures.|🚑 Emergency Handling|Unexpected bills are categorized as 'Emergency' transactions, activating a 'Repair Algorithm' that distributes impact evenly across remaining days.|🔄 Hard Reset|Complete system reconstruction based on current actual cash position, clearing historical data while preserving immutable audit logs.|💸 Debt Emergency|Critical borrowing can be marked as 'Emergency Borrowing' with accelerated settlement requirements and enhanced tracking.|🔮 Forecast Alerts|Predictive warnings when current spending trajectories will lead to deficit before cycle completion.|🛡️ Soft Landings|Gradual adjustment rather than catastrophic financial crashes through algorithmic safeguards that protect behavioral tendencies."
            },
            // 7. مركز الإدارة
            new[] {
                "Admin Center & System Integrity",
                "🛠️ Database Command & Control: Data Integrity First Architecture",
                "📂 Category Management|Dynamic addition, modification, and removal of expense categories with immediate system-wide propagation for consistent classification.|🔍 Transaction Audit|Complete database visualization with granular edit/delete capabilities for data correction while maintaining audit trails.|📅 Cycle Management|Force-close cycles, system resets, and historical data manipulation with mathematical precision.|👤 User Administration|PIN security management, role assignments, and access controls through encrypted storage mechanisms.|💾 Database Operations|Automated backup procedures, integrity verification, and performance diagnostics.|📊 Audit Analysis|Comprehensive activity tracking for forensic financial investigation.|⚡ System Health|Real-time performance metrics and error diagnostics.|🔒 Integrity Preservation|Administrative actions never compromise Safe Limit Engine mathematics, maintaining consistent calculations during maintenance."
            },
            // 8. استراتيجية النجاح
            new[] {
                "Success Strategy & Wealth Acceleration",
                "📈 Financial Mastery Roadmap: From Budgeting to Wealth Creation",
                "🌅 Morning Ritual|Begin each day by checking your Safe Daily Limit before any spending decisions to align expenditures with calculated boundaries.|📊 Wealth Velocity|This represents the speed at which your financial position improves through disciplined spending and surplus generation.|🔟 10% Buffer Rule|Aim to end each day with at least 10% of your daily limit unspent, triggering compound growth through the Daily Rollover mechanism.|🏷️ Category Discipline|Honest expense classification prevents data pollution and maintains algorithmic accuracy for reliable forecasting.|💰 Debt Strategy|Restrict borrowing to wealth-building opportunities rather than consumption, leveraging Virtual Deduction Architecture for risk management.|📅 Cycle Optimization|Align budget periods with income patterns and expense rhythms for maximum effectiveness.|📈 Analytics Utilization|Regular review of spending velocity metrics and category analysis enables continuous behavioral adjustment.|💎 Long-term Vision|Use Masroofy to build 'Wealth Capital' through consistent surplus accumulation and strategic financial decisions.|🛡️ Emergency Preparedness|Maintain 20% of monthly allowance as 'Emergency Reserve' outside system tracking for unexpected circumstances.|🎯 Transformation|These principles convert reactive survival-mode budgeting into proactive systematic wealth creation, where every financial decision becomes a step toward independence."
            }
        };

        for (int i = 0; i < 8; i++) BuildPage(_tabControl.TabPages[i], data[i]);
        this.ResumeLayout();
    }

    private void BuildPage(TabPage page, string[] content)
    {
        page.AutoScroll = true;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 1,
            AutoSize = true,
            Padding = new Padding(60, 40, 60, 100),
            BackColor = Color.Transparent
        };
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        // Calculate dynamic width for text wrapping
        int textWidth = Math.Max(this.Width - 120, 600); // 60px margin on each side, minimum 600px

        // 1. العنوان الضخم
        mainLayout.Controls.Add(new Label
        {
            Text = content[0],
            Font = new Font("Segoe UI", 36, FontStyle.Bold),
            ForeColor = Color.White,
            AutoSize = true,
            MaximumSize = new Size(textWidth, 0),
            Margin = new Padding(0, 0, 0, 30)
        });

        // 2. فاصل
        mainLayout.Controls.Add(new Label
        {
            Text = "─".PadRight(50, '─'),
            Font = new Font("Segoe UI", 12),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20)
        });

        // 3. العنوان الفرعي مع الأيقونة
        mainLayout.Controls.Add(new Label
        {
            Text = content[1],
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            MaximumSize = new Size(textWidth, 0),
            Margin = new Padding(0, 0, 0, 40)
        });

        // 4. الشرح التفصيلي - استخدام BuildJournalPage للتقسيم
        BuildJournalPage(mainLayout, content[2], textWidth);

        page.Controls.Add(mainLayout);
    }

    private void BuildJournalPage(TableLayoutPanel layout, string content, int textWidth)
    {
        // تقسيم المحتوى باستخدام | كفاصل
        var paragraphs = content.Split('|');

        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph)) continue;

            // تحديد نوع الفقرة من خلال الأيقونة في البداية
            var isHeader = paragraph.Trim().StartsWith("🏦") || paragraph.Trim().StartsWith("📊") ||
                          paragraph.Trim().StartsWith("🛡️") || paragraph.Trim().StartsWith("💡") ||
                          paragraph.Trim().StartsWith("🎯") || paragraph.Trim().StartsWith("📈") ||
                          paragraph.Trim().StartsWith("📋") || paragraph.Trim().StartsWith("⚖️") ||
                          paragraph.Trim().StartsWith("🔧") || paragraph.Trim().StartsWith("🎨") ||
                          paragraph.Trim().StartsWith("🏷️") || paragraph.Trim().StartsWith("⏰") ||
                          paragraph.Trim().StartsWith("📝") || paragraph.Trim().StartsWith("🔧") ||
                          paragraph.Trim().StartsWith("📊") || paragraph.Trim().StartsWith("🔒") ||
                          paragraph.Trim().StartsWith("🚫") || paragraph.Trim().StartsWith("💰") ||
                          paragraph.Trim().StartsWith("📉") || paragraph.Trim().StartsWith("🔄") ||
                          paragraph.Trim().StartsWith("📊") || paragraph.Trim().StartsWith("👁️") ||
                          paragraph.Trim().StartsWith("🚫") || paragraph.Trim().StartsWith("📉") ||
                          paragraph.Trim().StartsWith("🚑") || paragraph.Trim().StartsWith("🔄") ||
                          paragraph.Trim().StartsWith("💸") || paragraph.Trim().StartsWith("🔮") ||
                          paragraph.Trim().StartsWith("🛡️") || paragraph.Trim().StartsWith("📂") ||
                          paragraph.Trim().StartsWith("🔍") || paragraph.Trim().StartsWith("📅") ||
                          paragraph.Trim().StartsWith("👤") || paragraph.Trim().StartsWith("💾") ||
                          paragraph.Trim().StartsWith("📊") || paragraph.Trim().StartsWith("⚡") ||
                          paragraph.Trim().StartsWith("🔒") || paragraph.Trim().StartsWith("🌅") ||
                          paragraph.Trim().StartsWith("📊") || paragraph.Trim().StartsWith("🔟") ||
                          paragraph.Trim().StartsWith("🏷️") || paragraph.Trim().StartsWith("💰") ||
                          paragraph.Trim().StartsWith("📅") || paragraph.Trim().StartsWith("📈") ||
                          paragraph.Trim().StartsWith("💎") || paragraph.Trim().StartsWith("🛡️") ||
                          paragraph.Trim().StartsWith("🎯");

            var label = new Label
            {
                Text = paragraph.Trim(),
                ForeColor = isHeader ? ColorPalette.AccentGreen : Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", isHeader ? 18 : 16, isHeader ? FontStyle.Bold : FontStyle.Regular),
                AutoSize = true,
                MaximumSize = new Size(textWidth, 0),
                Margin = new Padding(0, 0, 0, 25), // 25px minimum spacing between paragraphs
                Padding = new Padding(0)
            };

            layout.Controls.Add(label);
        }
    }
}