using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// Knows how to find multiple continuous regions and draw paths around them,
/// including regions with holes and even nested regions. 
/// </summary>
public static class OutlineFinder
{
    public static List<Path2d> FindOutlines(IReadOnlyBooleanSlice slice)
    {
        var accumulatedPaths = new List<Path2d>();
        FindOutlinesInner(accumulatedPaths, slice, RotationDirection.CounterClockwise);
        return accumulatedPaths;
    }

    private static void FindOutlinesInner(ICollection<Path2d> accumulatedPaths, IReadOnlyBooleanSlice slice, RotationDirection rotationDirection)
    {
        var exterior = FindOutline(slice, rotationDirection);
        if (exterior == null)
        {
            return;
        }
        accumulatedPaths.Add(exterior);
        var interiorTester = new InteriorTester(exterior);
        var sliceViewWithoutInterior = new SliceViewWithoutInterior(slice, interiorTester);
        var sliceViewWithOnlyInvertedInterior = new SliceViewWithOnlyInvertedInterior(slice, interiorTester);
        FindOutlinesInner(accumulatedPaths, sliceViewWithOnlyInvertedInterior, rotationDirection.Opposite());
        // ReSharper disable once TailRecursiveCall
        FindOutlinesInner(accumulatedPaths, sliceViewWithoutInterior, rotationDirection);
    }

    public static Path2d? FindOutline(IReadOnlyBooleanSlice slice, RotationDirection rotationDirection)
    {
        var startingPoint = FindStartingPoint(slice);
        if (startingPoint == null)
        {
            return null;
        }
        // Careful - this assumes the start-point was found by starting in the
        // bottom-left and working right then up - that is we assume the bottom
        // side of the point is empty
        startingPoint = startingPoint.Value.Plus(rotationDirection == RotationDirection.CounterClockwise
            ? new Point2d(0, 0)
            : new Point2d(1, 0));
        var direction = rotationDirection == RotationDirection.CounterClockwise
            ? new Point2d(1, 0)
            : new Point2d(-1, 0);
        var path2d = new Path2d(rotationDirection);
        var position = startingPoint.Value;
        path2d.Add(position);
        while (true)
        {
            position = position.Plus(direction);
            path2d.Add(position);
            if (position == startingPoint)
            {
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
        return path2d;
    }

    private static Point2d ChooseVectorToCheck(Point2d direction, RotationDirection rotationDirection)
    {
        switch (rotationDirection)
        {
            case RotationDirection.CounterClockwise:
                // this is a bit mysterious, i arrived at this by drawing on grid paper!
                // read the first one as:
                // if we are going right counter-clockwise, for our path to be the exterior
                // we need there to be a point above us, which is a vector of 0, 0 because
                // of the way our co-ordinates work
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
                    // read the first one as:
                    // if we are going right clockwise, for our path to be the exterior
                    // we need there to be a point below us, which is a vector of 0, -1
                    ( 1, 0) => new Point2d( 0, -1),
                    ( 0, 1) => new Point2d( 0,  0),
                    (-1, 0) => new Point2d(-1,  0),
                    ( 0,-1) => new Point2d(-1, -1),
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(rotationDirection), rotationDirection, null);
        }
    }

    private static Point2d? FindStartingPoint(IReadOnlyBooleanSlice slice)
    {
        // Careful - elsewhere we rely on starting in the bottom-left and working right
        // then same for the next row up - we want the bottom side of the point to be empty
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
