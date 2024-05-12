using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// Knows how to test (reasonably efficiently) if a point is inside a
/// closed path or not.
/// </summary>
public class PathInteriorTester
{
    private readonly Dictionary<int, List<int>> _yToXTransitions = new();
    
    public PathInteriorTester(Path2d path2d)
    {
        if (path2d.Points.Count < 3)
        {
            throw new Exception("must have at least three points");
        }
        var prevPosition = path2d.Points[0];
        foreach (var position in path2d.Points.Skip(1))
        {
            var direction = position.Minus(prevPosition);
            if (direction.X == 0)
            {
                var y = (direction.Y > 0) ? prevPosition.Y : position.Y;
                if (!_yToXTransitions.TryGetValue(y, out var transitions))
                {
                    transitions = new List<int>();
                    _yToXTransitions[y] = transitions;
                }
                transitions.Add(position.X);
            }
            prevPosition = position;
        }
        foreach (var transitions in _yToXTransitions.Values)
        {
            transitions.Sort();
            if (transitions.Count == 0)
            {
                throw new Exception("did not expect transitions to be empty");
            }
            if ((transitions.Count % 2) != 0)
            {
                throw new Exception("did not expect transitions to have an odd number of items - is the path closed?");
            }
        }
    }

    public bool Inside(Point2d point2d)
    {
        if (!_yToXTransitions.TryGetValue(point2d.Y, out var transitions))
        {
            return false;
        }
        var inside = false;
        foreach (var transition in transitions)
        {
            if (point2d.X < transition)
            {
                return inside;
            }
            inside = !inside;
        }
        return inside;
    }

    public IEnumerable<Point2d> IterateRowMajor()
    {
        var minY = _yToXTransitions.Keys.Min();
        var maxY = _yToXTransitions.Keys.Max();
        for (var y = minY; y <= maxY; y++)
        {
            var transitions = _yToXTransitions[y];
            for (var i = 0; i < transitions.Count; i += 2)
            {
                var inX = transitions[i + 0];
                var outX = transitions[i + 1];
                for (var x = inX; x < outX; x++)
                {
                    yield return new Point2d(x, y);
                }
            }
        }
    }
}
