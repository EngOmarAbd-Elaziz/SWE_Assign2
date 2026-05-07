using Masroofy.App.Assets;
using Masroofy.App.Controllers;
using Masroofy.App.Models;
using Masroofy.App.Services;

namespace Masroofy.App.Views.Components;

/// <summary>
/// Debt management module responsible for tracking
/// borrowing and lending records, including CRUD operations
/// and balance impact updates.
/// </summary>
public sealed class DebtTrackerView : UserControl
{
    private readonly AppController _controller;
    private readonly DashboardView _dashboard;

    private readonly DataGridView _grid = new()
    {
        Dock = DockStyle.Fill,
        AllowUserToAddRows = false,
        ReadOnly = true,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        BackgroundColor = ColorPalette.DarkBackground,
        BorderStyle = BorderStyle.None,
        GridColor = Color.FromArgb(60, 60, 60),
        EnableHeadersVisualStyles = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        ColumnHeadersHeight = 48,
        RowHeadersVisible = false,
        DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorPalette.DarkSurface,
            ForeColor = Color.WhiteSmoke,
            SelectionBackColor = Color.FromArgb(45, 45, 45),
            SelectionForeColor = Color.White,
            Font = new Font("Segoe UI", 12F)
        },
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = ColorPalette.DarkSurface,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12F, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft
        },
        Margin = new Padding(0, 10, 0, 0)
    };

    private readonly ComboBox _cmbType = new()
    {
        DropDownStyle = ComboBoxStyle.DropDownList,
        Width = 170,
        Height = 38
    };

    private readonly NumericUpDown _numAmount = new()
    {
        DecimalPlaces = 2,
        Maximum = 1000000,
        Width = 170,
        Height = 38
    };

    private readonly TextBox _txtNote = new()
    {
        Width = 320,
        Height = 38,
        PlaceholderText = "Note (e.g. vendor / purpose)",
        Font = new Font("Segoe UI", 10F)
    };

    private readonly CheckBox _chkApply = new()
    {
        Text = "Apply to Balance",
        AutoSize = true,
        ForeColor = Color.LightGray
    };

    private DebtRecord? _selected;

    public DebtTrackerView(AppController controller, DashboardView dashboard)
    {
        _controller = controller;
        _dashboard = dashboard;

        Dock = DockStyle.Fill;
        BackColor = ColorPalette.DarkBackground;
        AutoScroll = true;

        var card = UiStyleService.CreateCard(new Size(0, 0));
        card.BackColor = ColorPalette.DarkSurface;
        card.Dock = DockStyle.Fill;

        var title = new Label
        {
            Text = "Debt Tracker",
            Font = new Font("Segoe UI", 32, FontStyle.Bold),
            ForeColor = ColorPalette.AccentGreen,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 18)
        };

        _cmbType.Items.AddRange(new[] { "Borrowing", "Lending" });
        _cmbType.SelectedIndex = 0;

        SetupGrid();

        var btnAdd = CreateActionButton("Add", ColorPalette.AccentGreen);
        var btnEdit = CreateActionButton("Edit", Color.FromArgb(120, 120, 120));
        var btnDelete = CreateActionButton("Delete", ColorPalette.OverspentRed);

        btnAdd.Click += (_, _) => AddDebt();
        btnEdit.Click += (_, _) => EditDebt();
        btnDelete.Click += (_, _) => DeleteDebt();

        var inputPanel = BuildInputPanel(btnAdd, btnEdit, btnDelete);
        var contentLayout = BuildContentLayout();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(40),
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100F)
            }
        };

        mainLayout.Controls.Add(title, 0, 0);
        mainLayout.Controls.Add(inputPanel, 0, 1);
        mainLayout.Controls.Add(contentLayout, 0, 2);

        card.Controls.Add(mainLayout);
        Controls.Add(card);

        LoadDebts();
    }

    /// <summary>
    /// Creates a styled action button used in CRUD operations.
    /// </summary>
    private Button CreateActionButton(string text, Color backColor)
    {
        var btn = new Button
        {
            Text = text,
            FlatStyle = FlatStyle.Flat,
            BackColor = backColor,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            Width = 180,
            Height = 60
        };

        btn.FlatAppearance.BorderSize = 0;
        UiStyleService.ApplyRoundedCorners(btn, 10);

        return btn;
    }

    /// <summary>
    /// Configures DataGridView structure for debt records.
    /// </summary>
    private void SetupGrid()
    {
        _grid.Columns.Clear();

        _grid.Columns.Add("Id", "Id");
        _grid.Columns.Add("Type", "Type");
        _grid.Columns.Add("Amount", "Amount");
        _grid.Columns.Add("Note", "Note");
        _grid.Columns.Add("Date", "Date");

        _grid.Columns[0].Visible = false;

        _grid.Columns[1].Width = 130;
        _grid.Columns[2].Width = 140;
        _grid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        _grid.Columns[4].Width = 150;

        _grid.SelectionChanged += (_, _) => BindSelected();
    }

    /// <summary>
    /// Loads all debt records from controller into the grid.
    /// </summary>
    public void LoadDebts()
    {
        _grid.Rows.Clear();

        foreach (var debt in _controller.GetDebts())
        {
            _grid.Rows.Add(
                debt.Id,
                debt.Type,
                debt.Amount.ToString("N2"),
                debt.Note,
                debt.Date.ToString("yyyy-MM-dd"));
        }
    }

    /// <summary>
    /// Binds selected grid row to input fields.
    /// </summary>
    private void BindSelected()
    {
        if (_grid.CurrentRow == null) return;

        var id = Convert.ToInt32(_grid.CurrentRow.Cells[0].Value);
        _selected = _controller.GetDebts().FirstOrDefault(x => x.Id == id);

        if (_selected == null) return;

        _cmbType.Text = _selected.Type;
        _numAmount.Value = _selected.Amount;
        _txtNote.Text = _selected.Note;
    }

    /// <summary>
    /// Adds a new debt record.
    /// </summary>
    private void AddDebt()
    {
        if (_numAmount.Value <= 0) return;

        _controller.AddDebt(
            _numAmount.Value,
            _cmbType.Text,
            _txtNote.Text.Trim(),
            _chkApply.Checked);

        _dashboard.RefreshData();
        ResetInputs();
        LoadDebts();
    }

    /// <summary>
    /// Updates selected debt record.
    /// </summary>
    private void EditDebt()
    {
        if (_selected == null || _numAmount.Value <= 0) return;

        _controller.UpdateDebt(
            _selected,
            _numAmount.Value,
            _cmbType.Text,
            _txtNote.Text.Trim());

        _dashboard.RefreshData();
        LoadDebts();
    }

    /// <summary>
    /// Deletes selected debt record.
    /// </summary>
    private void DeleteDebt()
    {
        if (_selected == null) return;

        if (MessageBox.Show(
            "Are you sure you want to delete this record?",
            "Confirm Deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _controller.DeleteDebt(_selected);

            _dashboard.RefreshData();
            ResetInputs();
            LoadDebts();
        }
    }

    /// <summary>
    /// Resets input fields to default state.
    /// </summary>
    private void ResetInputs()
    {
        _txtNote.Clear();
        _numAmount.Value = 0;
        _chkApply.Checked = false;
        _selected = null;
    }
}
