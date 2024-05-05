using BlockyCatTree.Pixel;

namespace BlockyCatTree.Voxel;

/// <summary>
/// A 3d world made up of little 1x1x1 blocks, organised by height.
/// </summary>
public class Voxels<TPayload> where TPayload : struct
{
    private readonly Dictionary<Zed, Slice<TPayload>> _zedToSlice = new();

    public bool IsEmpty => _zedToSlice.Count == 0;
    
    public TPayload? Get(Point3d point3d)
    {
        return !_zedToSlice.TryGetValue(point3d.Zed, out var slice) ? null : slice.Get(point3d.Point2d);
    }

    public void Set(Point3d point3d, TPayload payload)
    {
        if (!_zedToSlice.TryGetValue(point3d.Zed, out var slice))
        {
            slice = new Slice<TPayload>();
            _zedToSlice[point3d.Zed] = slice;
        }
        slice.Set(point3d.Point2d, payload);
    }

    public void Remove(Point3d point3d)
    {
        if (!_zedToSlice.TryGetValue(point3d.Zed, out var slice))
        {
            return;
        }
        slice.Remove(point3d.Point2d);
        if (slice.IsEmpty)
        {
            _zedToSlice.Remove(point3d.Zed);
        }
    }

    public bool TryGetSlice(Zed zed, out Slice<TPayload>? maybeSlice) =>
        _zedToSlice.TryGetValue(zed, out maybeSlice);

    public (Zed Min, Zed Max) GetZedInclusiveBounds() =>
        IsEmpty
            ? (Zed.Origin, Zed.Origin)
            : (_zedToSlice.Keys.Min(), _zedToSlice.Keys.Max());
}
