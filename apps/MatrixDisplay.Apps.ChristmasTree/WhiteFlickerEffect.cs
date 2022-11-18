namespace MatrixDisplay.Apps.ChristmasTree;

using System;
using System.Threading;
using MatrixDisplay;

internal sealed class WhiteFlickerEffect : IEffect
{
    public void Run(PixelBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var spawnTimer = new DeltaTimer(TimeSpan.FromSeconds(0.2D));
        var fadeTimer = new DeltaTimer(TimeSpan.FromSeconds(0.01D));
        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (fadeTimer.Tick())
            {
                buffer.Fade(4);
            }

            if (spawnTimer.Tick())
            {
                var data = buffer.Data;

                for (var index = 0; index < buffer.Width / 4; index++)
                {
                    data[Random.Shared.Next(data.Length)] = Color.White;
                }

                buffer.MarkDirty();
                buffer.Commit();
            }

            spinWait.SpinOnce();
        }
    }
}
