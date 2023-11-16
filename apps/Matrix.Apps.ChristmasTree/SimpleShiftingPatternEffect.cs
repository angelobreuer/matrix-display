namespace MatrixDisplay.Apps.ChristmasTree;

using System.Collections.Generic;
using MatrixSdk;

internal sealed class SimpleShiftingPatternEffect : ShiftingPatternEffectBase
{
    protected override IEnumerable<PatternSegment> GetPattern()
    {
        yield return new PatternSegment(Color.Red, 3);
        yield return new PatternSegment(Color.Green, 2);
    }
}
