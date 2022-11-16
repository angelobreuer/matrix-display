namespace MatrixDisplay.Apps.ChristmasTree;

using MatrixDisplay;

internal class ChristmasLightEffect : IEffect
{
    public void Run(PixelBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var swapTimer = new DeltaTimer(TimeSpan.FromSeconds(.5D));

        var preset1 = new Color[buffer.Width];
        var preset2 = new Color[buffer.Width];

        for (var index = 0; index < buffer.Width; index++)
        {
            if (index % 2 is 0)
            {
                preset1[index] = Color.Red;
                preset2[index] = Color.Green;
            }
            else
            {
                preset1[index] = Color.Green;
                preset2[index] = Color.Red;
            }
        }

        var flag = false;
        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (swapTimer.Tick())
            {
                flag = !flag;

                var preset = flag ? preset1 : preset2;
                preset.CopyTo(buffer.Data);
                buffer.MarkDirty();
                buffer.Commit();
            }

            spinWait.SpinOnce();
        }
    }
}
