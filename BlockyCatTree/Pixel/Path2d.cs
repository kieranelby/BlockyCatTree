namespace BlockyCatTree.Pixel;

public record Path2d
{
    public List<Point2d> Points { get; init; }

    public Path2d(List<Point2d>? points = null)
    {
        Points = points ?? [];
    }
}
