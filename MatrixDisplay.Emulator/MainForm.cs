namespace MatrixDisplay.Emulator;

using System;

public sealed class MainForm : Form, IDisplayController
{
    private readonly MatrixPanel _matrixPanel;

    public MainForm(int width, int height)
    {
        _matrixPanel = new MatrixPanel(width, height) { Dock = DockStyle.Fill, };

        Controls.Add(_matrixPanel);

        Width = 800;
        Height = 600;
    }

    public void Update(ReadOnlySpan<Color> buffer)
    {
        _matrixPanel.Update(buffer);
    }
}
