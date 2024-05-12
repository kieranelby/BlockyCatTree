using BlockyCatTree.Pixel;

namespace BlockyCatTree.Triangulate;

public readonly record struct NestedOutline(Path2d Exterior, List<NestedOutline> Interiors)
{
    public IEnumerable<Outline> Flatten()
    {
        yield return new Outline(Exterior, Interiors.Select(o => o.Exterior).ToList());
        foreach (var flattenedNestedExterior in
                 Interiors
                     .SelectMany(interior =>
                         interior
                             .Interiors
                             .SelectMany(
                                 nestedExterior => nestedExterior.Flatten())))
        {
            yield return flattenedNestedExterior;
        }
    }
}
