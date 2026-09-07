namespace ExcellentTaste.WinForms.Controls;

internal sealed class ToolbarGridPage : UserControl
{
    private readonly TableLayoutPanel _layout;

    public ToolbarGridPage()
    {
        Dock = DockStyle.Fill;

        _layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 2
        };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        _layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        Toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight
        };

        _layout.Controls.Add(Toolbar, 0, 0);
        Controls.Add(_layout);
    }

    public FlowLayoutPanel Toolbar { get; }

    public void SetContent(Control content)
    {
        content.Dock = DockStyle.Fill;
        _layout.Controls.Add(content, 0, 1);
    }
}
