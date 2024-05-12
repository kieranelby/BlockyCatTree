using BlockyCatTree.Triangulate;

namespace BlockyCatTree.Pixel;

/// <remarks>
/// Note that:
/// 1) in terms of axes we follow vaguely mathematical convention - best to think
/// of increasing Y as going away from us, not down like in some graphics contexts.
/// 2) our co-ordinates are integers, so our points are not infinitely small,
/// but rather 1x1 in size - so does (X,Y) describe the center of the 1x1 square?
/// no, we are less mathematical here - we use (X,Y) to describe the corner that
/// is left-most and closest to us, the really-a-square-point extends from (X,Y)
/// to just before (X+1,Y+1) starts, as opposed to using +/- 0.5. 
/// </remarks>
public readonly record struct Point2d(int X, int Y)
{
    public static readonly Point2d Origin = new Point2d(0, 0);
    public Point2d Minus(Point2d other) => new Point2d(X - other.X, Y - other.Y);
    public Point2d Plus(Point2d other) => new Point2d(X + other.X, Y + other.Y);

    public Point2d Rotate(RotationDirection rotationDirection)
    {
        switch (rotationDirection)
        {
            case RotationDirection.Clockwise:
                return new Point2d(-Y, X);
            case RotationDirection.CounterClockwise:
                return new Point2d(Y, -X);
            default:
                throw new ArgumentOutOfRangeException(nameof(rotationDirection), rotationDirection, null);
        }
    }

    public Point2d Scale(int multiplier) => new(X * multiplier, Y * multiplier);
}
