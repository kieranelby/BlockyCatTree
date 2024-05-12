namespace BlockyCatTree.Pixel;

public interface IReadOnlyBooleanSlice
{
    bool Exists(Point2d point2d);
    Bounds2d GetInclusiveBounds();
    Point2d? FindStartingPoint();
}