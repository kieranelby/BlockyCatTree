using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

public readonly record struct Outline(Path2d Exterior, List<Path2d> Interiors)
{
    public IEnumerable<Path2d> ToPaths()
    {
        yield return Exterior;
        foreach (var interior in Interiors)
        {
            yield return interior;
        }
    }
}
