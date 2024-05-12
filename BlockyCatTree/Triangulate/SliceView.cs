using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// A view onto a slice where, for the path represented by the <see cref="PathInteriorTester"/>:
///  - nothing outside the path exists
///  - everything inside the path is the opposite of the underlying slice (exists if it doesn't and vice versa)
/// Useful for finding holes in regions (and nested regions).
/// </summary>
public sealed class SliceViewWithOnlyInvertedInterior(IReadOnlyBooleanSlice underlying, PathInteriorTester pathInteriorTester)
    : SliceView(underlying, pathInteriorTester, Combine)
{
    private static bool Combine(bool underlyingExists, bool isInside)
    {
        return isInside && !underlyingExists;
    }
    
    public override Bounds2d GetInclusiveBounds() => PathInteriorTester.GetInclusiveBounds();
}

/// <summary>
/// A view onto a slice where, for the path represented by the <see cref="PathInteriorTester"/>:
///  - nothing inside the path exists
///  - everything outside the path is the same as normal
/// Useful for finding multiple regions.
/// </summary>
public sealed class SliceViewWithoutInterior(IReadOnlyBooleanSlice underlying, PathInteriorTester pathInteriorTester)
    : SliceView(underlying, pathInteriorTester, Combine)
{
    private static bool Combine(bool underlyingExists, bool isInside)
    {
        return !isInside && underlyingExists;
    }
}

/// <summary>
/// A view onto a slice which transforms it differently based on whether
/// points are inside a path or not.
/// </summary>
public abstract class SliceView : IReadOnlyBooleanSlice
{
    private readonly IReadOnlyBooleanSlice _underlying;
    protected readonly PathInteriorTester PathInteriorTester;
    private readonly Func<bool, bool, bool> _combineFunction;

    protected SliceView(IReadOnlyBooleanSlice underlying, PathInteriorTester pathInteriorTester, Func<bool,bool,bool> combineFunction)
    {
        _underlying = underlying;
        PathInteriorTester = pathInteriorTester;
        _combineFunction = combineFunction;
    }
    
    public bool Exists(Point2d point2d)
    {
        var underlyingExists = _underlying.Exists(point2d);
        var isInside = PathInteriorTester.Inside(point2d);
        return _combineFunction(underlyingExists, isInside);
    }

    public virtual Bounds2d GetInclusiveBounds() => _underlying.GetInclusiveBounds();

    public Point2d? FindStartingPoint()
    {
        return GetInclusiveBounds().IterateRowMajor().Select(p => (Point2d?) p).FirstOrDefault(p => Exists(p.Value), null);
    }
}
