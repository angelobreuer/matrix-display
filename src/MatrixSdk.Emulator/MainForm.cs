namespace MatrixSdk.Emulator;

public sealed class MainForm : Form
{
    private readonly MatrixPanel _matrixPanel;

    public MainForm()
    {
        _matrixPanel = new MatrixPanel { Dock = DockStyle.Fill, };

        Controls.Add(_matrixPanel);

        Width = 800;
        Height = 600;
        Text = "Matrix SDK Emulator";
    }
}