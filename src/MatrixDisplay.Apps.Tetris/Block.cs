using MatrixDisplay;

sealed record class Block(Shape Shape, Color Color)
{
    private Point[]? _points;

    public int X { get; set; }
    public int Y { get; set; }
    public Point[] Points => _points ?? Shape.Points;

    public void DestroyLine(int absoluteY)
    {
        _points ??= Shape.Points;

        var result = new List<Point>();
        var newY = 0;

        foreach (var points in _points.GroupBy(x => x.Y).OrderBy(x => x.Key))
        {
            if (points.Key + Y != absoluteY)
            {
                foreach (var (x, _) in points)
                {
                    result.Add(new Point(x, newY));
                }

                newY++;
            }
        }

        _points = result.ToArray();
    }

    public bool IsDestroyed
    {
        get
        {
            return _points.Length == 0;
        }
    }

    public void Rotate(int rotation)
    {
        var points = new Point[Shape.Points.Length];
        var angle = rotation * Math.PI / 180;

        for (var i = 0; i < Shape.Points.Length; i++)
        {
            points[i] = new Point(Points[i].X, Points[i].Y)
            {
                X = (int)Math.Round((Points[i].X * Math.Cos(angle)) - (Points[i].Y * Math.Sin(angle))),
                Y = (int)Math.Round((Points[i].X * Math.Sin(angle)) + (Points[i].Y * Math.Cos(angle))),
            };
        }

        _points = points;
    }

    public void Render(PixelBuffer buffer)
    {
        foreach (var point in Points)
        {
            buffer[X + point.X, Y + point.Y] = Color;
        }
    }

    public bool IsColliding(Block otherBlock)
    {
        foreach (var point1 in Points)
        {
            foreach (var point2 in otherBlock.Points)
            {
                if (point1.X + X == point2.X + otherBlock.X && point1.Y + Y == point2.Y + otherBlock.Y)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
