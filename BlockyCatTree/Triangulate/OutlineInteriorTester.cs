using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

public class OutlineInteriorTester
{
    private readonly PathInteriorTester _exteriorTester;
    private readonly List<PathInteriorTester> _interiorTesters;

    public OutlineInteriorTester(Outline outline)
    {
        _exteriorTester = new PathInteriorTester(outline.Exterior);
        _interiorTesters = outline.Interiors.Select(i => new PathInteriorTester(i)).ToList();
    }

    public bool Inside(Point2d point2d) =>
        _exteriorTester.Inside(point2d) &&
        !_interiorTesters.Any(interiorTester => interiorTester.Inside(point2d));

    public IEnumerable<Point2d> IterateRowMajor()
    {
        return _exteriorTester.IterateRowMajor()
            .Where(p => !_interiorTesters.Any(interiorTester => interiorTester.Inside(p)));
    }
}
