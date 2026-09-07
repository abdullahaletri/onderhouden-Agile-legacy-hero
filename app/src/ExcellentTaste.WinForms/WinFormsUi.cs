namespace ExcellentTaste.WinForms;

internal static class WinFormsUi
{
    internal static Button CreateButton(string text, int minWidth = 96)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(minWidth, 0),
            Padding = new Padding(8, 2, 8, 2),
            UseCompatibleTextRendering = true
        };
    }

    internal static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            Dock = DockStyle.Fill,
            MultiSelect = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
    }

    internal static void HideColumn(DataGridView grid, string columnName)
    {
        if (grid.Columns.Contains(columnName))
        {
            grid.Columns[columnName]!.Visible = false;
        }
    }

    internal static TableLayoutPanel CreateFormLayout()
    {
        TableLayoutPanel layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            Padding = new Padding(12),
            AutoScroll = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        return layout;
    }

    internal static void AddRow(TableLayoutPanel layout, string label, Control control)
    {
        int row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, row);
        control.Dock = DockStyle.Fill;
        layout.Controls.Add(control, 1, row);
    }

    internal static void AddButtons(TableLayoutPanel layout)
    {
        FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        Button save = CreateButton("Bewaren");
        save.DialogResult = DialogResult.OK;
        Button cancel = CreateButton("Annuleren");
        cancel.DialogResult = DialogResult.Cancel;
        buttons.Controls.AddRange(new Control[] { save, cancel });

        int row = layout.RowCount++;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(buttons, 0, row);
        layout.SetColumnSpan(buttons, 2);

        Form? form = layout.FindForm();
        if (form != null)
        {
            form.AcceptButton = save;
            form.CancelButton = cancel;
        }
    }

    internal static void SelectComboValue<T>(ComboBox combo, T value)
    {
        foreach (ComboOption<T> item in combo.Items)
        {
            if (EqualityComparer<T>.Default.Equals(item.Value, value))
            {
                combo.SelectedItem = item;
                return;
            }
        }

        if (combo.Items.Count > 0)
        {
            combo.SelectedIndex = 0;
        }
    }
}
