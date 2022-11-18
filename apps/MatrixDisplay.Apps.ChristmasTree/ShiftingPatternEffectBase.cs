namespace MatrixDisplay.Apps.ChristmasTree;

using System;
using System.Threading;
using MatrixDisplay;

internal abstract class ShiftingPatternEffectBase : IEffect
{
    public void Run(PixelBuffer buffer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(buffer);

        var pattern = BuildPattern(buffer.Width);
        var deltaTimer = new DeltaTimer(TimeSpan.FromSeconds(0.02D));
        var spinWait = new SpinWait();
        var shift = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            if (deltaTimer.Tick())
            {
                for (var index = 0; index < buffer.Width; index++)
                {
                    var sourcePosition = (shift + index) % buffer.Width;
                    var destinationPosition = buffer.Width - 1 - index;

                    buffer.Data[destinationPosition] = pattern[sourcePosition];
                }

                buffer.MarkDirty();
                buffer.Commit();

                shift++;
            }

            spinWait.SpinOnce();
        }
    }

    protected abstract IEnumerable<PatternSegment> GetPattern();

    private ReadOnlySpan<Color> BuildPattern(int count)
    {
        const int DesiredPatternCount = 3;

        var pattern = GetPattern()
            .SelectMany(segment => Enumerable
                .Repeat(segment.Color, segment.Count))
            .ToArray();

        var totalSegmentCount = pattern.Length;
        var patternDesiredLeds = DesiredPatternCount * totalSegmentCount;
        var stretchFactor = (double)patternDesiredLeds / count;

        var result = GC.AllocateUninitializedArray<Color>(count);

        for (var index = 0; index < count; index++)
        {
            var mappedPosition = (int)Math.Floor(index * stretchFactor);
            result[index] = pattern[mappedPosition % pattern.Length];
        }

        return result;
    }

    protected readonly record struct PatternSegment(Color Color, int Count);
}
