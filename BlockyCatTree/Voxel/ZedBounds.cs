using System.Collections;

namespace BlockyCatTree.Voxel;

public readonly record struct ZedBounds(Zed Min, Zed Max) : IEnumerable<Zed>
{
    public IEnumerator<Zed> GetEnumerator() =>
        Enumerable.Range(Min.Value, 1 + Max.Value - Min.Value).Select(i => new Zed(i)).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public ZedBounds Expand(int amount) =>
        new ZedBounds(new Zed(Min.Value - amount), new Zed(Max.Value + amount));
}
