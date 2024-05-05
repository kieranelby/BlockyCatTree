namespace BlockyCatTree.Pixel;

public interface IReadOnlyBooleanSlice
{
    bool Exists(Point2d point2d);
    (Point2d Min, Point2d Max) GetInclusiveBounds();
}