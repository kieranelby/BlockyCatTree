using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

public static class OutlineFinder
{
    public static List<Outline> FindOutlines(IReadOnlyBooleanSlice slice)
    {
        var outlines = new List<Outline>();
        var startingPoint = FindStartingPoint(slice);
        if (startingPoint == null)
        {
            return outlines;
        }
        var rotationDirection = RotationDirection.CounterClockwise;
        startingPoint = startingPoint.Value.Plus(rotationDirection == RotationDirection.CounterClockwise
            ? new Point2d(0, 0)
            : new Point2d(1, 0));
        var direction = rotationDirection == RotationDirection.CounterClockwise
            ? new Point2d(1, 0)
            : new Point2d(-1, 0);
        var path2d = new Path2d();
        Path2d exterior;
        var position = startingPoint.Value;
        path2d.Add(position);
        while (true)
        {
            position = position.Plus(direction);
            path2d.Add(position);
            if (position == startingPoint)
            {
                exterior = path2d;
                break;
            }
            var candidateDirection = direction.Rotate(rotationDirection);
            for (var i = 1; i <= 4; i++)
            {
                if (i == 4)
                {
                    throw new Exception($"failed to find which way to go at {position} after {direction}");
                }
                var vectorToCheck = ChooseVectorToCheck(candidateDirection, rotationDirection);
                if (slice.Exists(position.Plus(vectorToCheck)))
                {
                    break;
                } 
                candidateDirection = candidateDirection.Rotate(rotationDirection.Opposite());
            }
            direction = candidateDirection;
        }
        var interior = new Path2d(); // TODO
        outlines.Add(new Outline(exterior, interior));
        return outlines;
    }

    private static Point2d ChooseVectorToCheck(Point2d direction, RotationDirection rotationDirection)
    {
        switch (rotationDirection)
        {
            case RotationDirection.CounterClockwise:
                return direction switch
                {
                    ( 1, 0) => new Point2d( 0,  0),
                    ( 0, 1) => new Point2d(-1,  0),
                    (-1, 0) => new Point2d(-1, -1),
                    ( 0,-1) => new Point2d( 0, -1),
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
            case RotationDirection.Clockwise:
                return direction switch
                {
                    // TODO
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rotationDirection), rotationDirection, null);
        }
    }

    private static Point2d? FindStartingPoint(IReadOnlyBooleanSlice slice)
    {
        foreach (var point2d in slice.GetInclusiveBounds().IterateRowMajor())
        {
            if (slice.Exists(point2d))
            {
                return point2d;
            }
        }
        return null;
    }
}
