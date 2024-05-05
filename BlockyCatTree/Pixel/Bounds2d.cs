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
};
