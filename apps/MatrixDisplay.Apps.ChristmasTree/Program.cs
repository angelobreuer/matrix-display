using MatrixDisplay;
using MatrixDisplay.Apps.ChristmasTree;

var buffer = PixelBuffer.Instance;
buffer.AutoCommit = false;

void RunEffect<T>() where T : IEffect, new()
{
    using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var effect = new T();

    buffer!.Data.Clear();

    try
    {
        effect.Run(buffer!, cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
    }
}

void SelfTest(Color color)
{
    var tickCount = Environment.TickCount64;

    for (var index = 0; index < buffer!.Data.Length; index++)
    {
        if (index > 0)
        {
            buffer.Data[index - 1] = default;
        }

        buffer.Data[index] = color;
        buffer.Commit();

        SpinWait.SpinUntil(() => Environment.TickCount64 - tickCount > 5);
        tickCount += 5;
    }

    buffer.Data.Clear();
    buffer.Commit();
}

SelfTest(Color.Red);
SelfTest(Color.Green);
SelfTest(Color.Blue);
SelfTest(Color.White);

while (true)
{
    RunEffect<SimpleShiftingPatternEffect>();
    RunEffect<WhiteFlickerEffect>();
    RunEffect<HueShiftEffect>();
    RunEffect<ChristmasLightEffect>();
    RunEffect<StarlightEffect>();
    RunEffect<BlazeEffect>();
}
