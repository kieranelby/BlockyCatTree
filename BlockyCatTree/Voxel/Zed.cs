namespace BlockyCatTree.Voxel;

/// <summary>
/// This is just one of the 3d axes, but height is a bit special due to gravity.
/// </summary>
public readonly record struct Zed(int Value) : IComparable<Zed>
{
    public static readonly Zed Origin = new(0);

    public int CompareTo(Zed other)
    {
        return Value.CompareTo(other.Value);
    }
}
