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

        var timer = new DeltaTimer(TimeSpan.FromSeconds(0.05D));
        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            if (timer.Tick())
            {
                var data = buffer.Data;
                data.Fill(Color.White);

                for (var index = 0; index < buffer.Width / 4; index++)
                {
                    data[Random.Shared.Next(data.Length)] = new Color(0xCC, 0xCC, 0xCC);
                }

                buffer.MarkDirty();
                buffer.Commit();
            }

            spinWait.SpinOnce();
        }
    }
}
