sealed record class Shape
{
    public static readonly Shape[] All = {
        new Shape(new[] {
            new Point(0, -2),
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
            new Point(0, 2),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(0, 1),
            new Point(1, 1),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
        }),

        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
        }),
        new Shape(new[] {
            new Point(0, -1),
            new Point(0, 0),
            new Point(1, 0),
            new Point(1, 1),
        }),
    };

    public readonly Point[] Points;

    public Shape(Point[] points)
    {

        Points = points;
    }
}
