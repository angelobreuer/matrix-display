using MatrixDisplay;

var buffer = PixelBuffer.Instance;

buffer.AutoCommit = false;

var activeFireworks = new List<Firework>();

for (var frame = 0; ; frame++)
{
    buffer.Clear();

    if (frame % 7 is 0)
    {
        var particleColor = new Color(
            R: (byte)Random.Shared.Next(0, 256),
            G: (byte)Random.Shared.Next(0, 256),
            B: (byte)Random.Shared.Next(0, 256));

        activeFireworks.Add(new Firework(
            StartX: Random.Shared.Next(0, buffer.Width),
            FrameStep: frame,
            EndY: Random.Shared.Next(buffer.Height / 2, buffer.Height),
            ParticleColor: particleColor));
    }

    foreach (var firework in activeFireworks)
    {
        var progress = (double)(frame - firework.FrameStep);
        progress *= Math.Exp(progress / 8D) / 16D;

        if (progress > firework.EndY + 8)
        {
            continue;
        }

        var y = buffer.Height;
        var x = firework.StartX;

        for (var index = 0; index < firework.EndY && progress > 0; index++)
        {
            buffer[x, y--] = firework.ParticleColor;
            progress--;
        }

        if (progress > 0)
        {
            buffer[x, y] = firework.ParticleColor;
            progress--;
        }

        if (progress > 0)
        {
            buffer[x - 1, y - 1] = firework.ParticleColor;
            buffer[x + 1, y + 1] = firework.ParticleColor;
            buffer[x - 1, y + 1] = firework.ParticleColor;
            buffer[x + 1, y - 1] = firework.ParticleColor;
            progress--;
        }

        if (progress > 0)
        {
            buffer[x - 2, y - 2] = firework.ParticleColor;
            buffer[x + 2, y + 2] = firework.ParticleColor;
            buffer[x - 2, y + 2] = firework.ParticleColor;
            buffer[x + 2, y - 2] = firework.ParticleColor;
            progress--;
        }
    }

    buffer.Commit();
    Thread.Sleep(16);
}
