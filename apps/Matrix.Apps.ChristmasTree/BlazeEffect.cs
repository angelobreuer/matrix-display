namespace MatrixDisplay.Apps.ChristmasTree;

using System;
using System.Threading;
using MatrixSdk;

internal sealed class BlazeEffect : IEffect
{
    public void Run(ImageBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var blazes = new List<Blaze>();

        var spawnTimer = new DeltaTimer(TimeSpan.FromSeconds(0.2D));
        var moveTimer = new DeltaTimer(TimeSpan.FromSeconds(0.01D));

        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            var triggerRender = false;

            if (spawnTimer.Tick())
            {
                var position = (float)Random.Shared.Next(-5, buffer.Width + 5);
                var vectorSign = Random.Shared.Next(0, 2) is 0 ? -1 : 1; // 0 or 1
                var vector = (float)(vectorSign * (Random.Shared.NextDouble() + 2D));

                var hue = Random.Shared.NextDouble() * 30D;
                var innerColor = Color.FromHsv(hue, 1D, 1D);
                var outerColor = Color.FromHsv(hue, 1D, .3D);

                blazes.Add(new Blaze(vector, innerColor, outerColor) { Position = position, });
                triggerRender = true;
            }

            if (moveTimer.Tick())
            {
                foreach (var blaze in blazes)
                {
                    blaze.Position += blaze.Vector;
                }

                triggerRender = true;
            }

            if (triggerRender)
            {
                buffer.Clear();

                foreach (var blaze in blazes)
                {
                    var positionInScene = (int)Math.Round(blaze.Position);

                    buffer[positionInScene - 1, 0] = blaze.OuterColor;
                    buffer[positionInScene + 1, 0] = blaze.OuterColor;
                    buffer[positionInScene, 0] = blaze.InnerColor;
                }

                buffer.Commit();
            }

            spinWait.SpinOnce();
        }
    }

    private sealed record class Blaze(float Vector /* Direction / Speed */, Color InnerColor, Color OuterColor)
    {
        public float Position { get; set; }
    }
}
