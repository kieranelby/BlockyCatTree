namespace BlockyCatTree.Pixel;

public record Path2d(List<Point2d> Points)
{
    public Path2d() : this([])
    {
    }

    public bool IsEmpty => Points.Count == 0;

    public void Add(Point2d position)
    {
        Points.Add(position);
    }
}
