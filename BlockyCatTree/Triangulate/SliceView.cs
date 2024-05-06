using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

public class SliceViewWithOnlyInvertedInterior(IReadOnlyBooleanSlice underlying, InteriorTester interiorTester)
    : SliceView(underlying, interiorTester, Combine)
{
    private static bool Combine(bool underlyingExists, bool isInside)
    {
        return isInside && !underlyingExists;
    }
}

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
