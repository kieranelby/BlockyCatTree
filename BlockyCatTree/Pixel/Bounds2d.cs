namespace BlockyCatTree.Pixel;

public readonly record struct Bounds2d(Point2d Min, Point2d Max)
{
    public IEnumerable<Point2d> IterateRowMajor()
    {
        for (var y = Min.Y; y <= Max.Y; y++)
        {
            for (var x = Min.X; x <= Max.X; x++)
            {
                yield return new Point2d(x, y);
            }
        }
    }

    public Bounds2d Combine(Bounds2d other) =>
        new(new Point2d(
            Math.Min(Min.X, other.Min.X),
            Math.Min(Min.Y, other.Min.Y)
        ), new Point2d(
            Math.Max(Max.X, other.Max.X),
            Math.Max(Max.Y, other.Max.Y)
        ));
}
