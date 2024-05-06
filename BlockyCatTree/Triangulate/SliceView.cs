using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// A view onto a slice where, for the path represented by the <see cref="InteriorTester"/>:
///  - nothing outside the path exists
///  - everything inside the path is the opposite of the underlying slice (exists if it doesn't and vice versa)
/// Useful for finding holes in regions (and nested regions).
/// </summary>
public class SliceViewWithOnlyInvertedInterior(IReadOnlyBooleanSlice underlying, InteriorTester interiorTester)
    : SliceView(underlying, interiorTester, Combine)
{
    private static bool Combine(bool underlyingExists, bool isInside)
    {
        return isInside && !underlyingExists;
    }
}

/// <summary>
/// A view onto a slice where, for the path represented by the <see cref="InteriorTester"/>:
///  - nothing inside the path exists
///  - everything outside the path is the same as normal
/// Useful for finding multiple regions.
/// </summary>
public class SliceViewWithoutInterior(IReadOnlyBooleanSlice underlying, InteriorTester interiorTester)
    : SliceView(underlying, interiorTester, Combine)
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
public class SliceView : IReadOnlyBooleanSlice
{
    private readonly IReadOnlyBooleanSlice _underlying;
    private readonly InteriorTester _interiorTester;
    private readonly Func<bool, bool, bool> _combineFunction;

    public SliceView(IReadOnlyBooleanSlice underlying, InteriorTester interiorTester, Func<bool,bool,bool> combineFunction)
    {
        _underlying = underlying;
        _interiorTester = interiorTester;
        _combineFunction = combineFunction;
    }
    
    public bool Exists(Point2d point2d)
    {
        var underlyingExists = _underlying.Exists(point2d);
        var isInside = _interiorTester.Inside(point2d);
        return _combineFunction(underlyingExists, isInside);
    }

    // TODO - we could be more efficient for some cases by reducing the bounds to match the path
    public Bounds2d GetInclusiveBounds() => _underlying.GetInclusiveBounds();
}
