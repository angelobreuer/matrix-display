namespace MatrixSdk.Emulator;

public sealed class MainForm : Form
{
    private readonly MatrixPanel _matrixPanel;

    public MainForm(int width, int height)
    {
        _matrixPanel = new MatrixPanel(new ImageBounds(width, height)) { Dock = DockStyle.Fill, };

        Controls.Add(_matrixPanel);

        Width = 800;
        Height = 600;
        Text = "Matrix SDK Emulator";
    }
}