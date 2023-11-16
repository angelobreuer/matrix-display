using MatrixDisplay.Apps.ChristmasTree;
using MatrixSdk;

var buffer = ImageBuffer.Create();

void RunEffect<T>() where T : IEffect, new()
{
    using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
    var effect = new T();

    buffer.Clear();

    try
    {
        effect.Run(buffer, cancellationTokenSource.Token);
    }
    catch (OperationCanceledException)
    {
    }
}

void SelfTest(MatrixSdk.Color color)
{
    var tickCount = Environment.TickCount64;

    for (var index = 0; index < buffer.Colors.Length; index++)
    {
        if (index > 0)
        {
            buffer.Colors[index - 1] = default;
        }

        buffer.Colors[index] = color;
        buffer.Commit();

        SpinWait.SpinUntil(() => Environment.TickCount64 - tickCount > 5);
        tickCount += 5;
    }

    buffer.Clear();
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
