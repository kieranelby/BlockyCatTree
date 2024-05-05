namespace BlockyCatTree.Triangulate;

public enum RotationDirection
{
    Clockwise = -1,
    CounterClockwise = 1
}

public static class RotationDirectionExtension
{
    public static RotationDirection Opposite(this RotationDirection rotationDirection)
    {
        return (RotationDirection)(-(int)rotationDirection);
    }
}
