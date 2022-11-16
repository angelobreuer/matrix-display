﻿namespace MatrixDisplay.Apps.ChristmasTree;

using System;
using System.Threading;
using MatrixDisplay;

internal sealed class HueShiftEffect : IEffect
{
    public void Run(PixelBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var timer = new DeltaTimer(TimeSpan.FromSeconds(60D / 1000D)); // 60 FPS
        var spinWait = new SpinWait();
        var offset = 0.0D;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (timer.Tick())
            {
                var shiftPerStep = 360.0D / buffer.Width;

                for (var x = 0; x < buffer.Width; x++)
                {
                    buffer.Data[buffer.Width - 1 - x] = Color.FromHsv((shiftPerStep * x) + offset, 1D, 1D);
                }

                buffer.MarkDirty();
                buffer.Commit();

                offset += shiftPerStep * 2D;
            }

            spinWait.SpinOnce();
        }
    }
}
