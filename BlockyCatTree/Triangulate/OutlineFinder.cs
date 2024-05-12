using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// Knows how to find multiple continuous regions and draw paths around them,
/// including regions with holes and even nested regions. 
/// </summary>
public static class OutlineFinder
{
    public static List<Path2d> FindAllPaths(IReadOnlyBooleanSlice slice, bool includeInteriors = true)
    {
        var outlines = FindOutlines(slice, includeInteriors);
        return outlines.SelectMany(o => o.ToPaths()).ToList();
    }

    public static IEnumerable<Outline> FindOutlines(IReadOnlyBooleanSlice slice, bool includeInteriors = true)
    {
        return FindNestedOutlines(slice, includeInteriors).SelectMany(no => no.Flatten());
    }

    private static List<NestedOutline> FindNestedOutlines(IReadOnlyBooleanSlice slice, bool includeInteriors = true, RotationDirection rotationDirection = RotationDirection.CounterClockwise)
    {
        var outlines = new List<NestedOutline>();
        var remainingSlice = slice;
        while (true)
        {
            var path = FindOutline(remainingSlice, rotationDirection);
            if (path == null)
            {
                break;
            }
            var interiorTester = new PathInteriorTester(path);
            var sliceViewWithoutInterior = new SliceViewWithoutInterior(remainingSlice, interiorTester);
            if (!includeInteriors)
            {
                outlines.Add(new NestedOutline(path, []));
            }
            else
            {
                var sliceViewWithOnlyInvertedInterior = new SliceViewWithOnlyInvertedInterior(remainingSlice, interiorTester);
                outlines.Add(new NestedOutline(path, FindNestedOutlines(sliceViewWithOnlyInvertedInterior, includeInteriors, rotationDirection.Opposite())));
            }
            remainingSlice = sliceViewWithoutInterior;
        }
        return outlines;
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
        return slice.FindStartingPoint();
    }
}
