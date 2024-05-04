namespace BlockyCatTree;

/// <remarks>
/// Note that we follow mathematical convention - best to think
/// of increasing Y as going away or up, not down like in some
/// graphics applications. 
/// </remarks>
public readonly record struct Point2d(int X, int Y)
{
    public static readonly Point2d Origin = new Point2d(0, 0);
    public Point2d Minus(Point2d other) => new Point2d(X - other.X, Y - other.Y);
    public Point2d Plus(Point2d other) => new Point2d(X + other.X, Y + other.Y);
}
