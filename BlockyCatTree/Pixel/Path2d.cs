using BlockyCatTree.Triangulate;

namespace BlockyCatTree.Pixel;

public record Path2d(List<Point2d> Points, RotationDirection RotationDirection)
{
    public Path2d(RotationDirection rotationDirection) : this([], rotationDirection)
    {
    }

    public bool IsEmpty => Points.Count == 0;

    public void Add(Point2d position)
    {
        Points.Add(position);
    }
}
