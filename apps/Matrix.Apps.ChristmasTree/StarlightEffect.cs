namespace MatrixDisplay.Apps.ChristmasTree;

using MatrixSdk;

internal class StarlightEffect : IEffect
{
    public void Run(ImageBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var fadeTimer = new DeltaTimer(TimeSpan.FromSeconds(0.001D));
        var spawnTimer = new DeltaTimer(TimeSpan.FromSeconds(0.05D));
        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (fadeTimer.Tick())
            {
                buffer.Fade(1);
            }

            if (spawnTimer.Tick())
            {
                var x = Random.Shared.Next(buffer.Width);

                var r = (byte)Random.Shared.Next(0, byte.MaxValue + 1);
                var g = (byte)Random.Shared.Next(0, byte.MaxValue + 1);
                var b = (byte)Random.Shared.Next(0, byte.MaxValue + 1);

                buffer[x, 0] = new Color(r, g, b);
            }

            spinWait.SpinOnce();
            buffer.Commit();
        }
    }
}