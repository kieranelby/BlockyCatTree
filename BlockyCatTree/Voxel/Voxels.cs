using BlockyCatTree.Pixel;

namespace BlockyCatTree.Voxel;

/// <summary>
/// A 3d world made up of little 1x1x1 blocks, organised by height.
/// </summary>
public class Voxels<TPayload> : IReadOnlyBooleanVoxels where TPayload : struct
{
    private readonly Dictionary<Zed, Slice<TPayload>> _zedToSlice = new();

    public bool IsEmpty => _zedToSlice.Count == 0;
    public bool Exists(Point3d point3d) => Get(point3d).HasValue;

    public IReadOnlyBooleanSlice GetReadOnlyBooleanSlice(Zed zed)
    {
        return TryGetSlice(zed, out var maybeSlice) ? maybeSlice! : EmptyReadOnlyBooleanSlice.Instance;
    }

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

    public void SetSlice(Zed zed, Slice<TPayload> slice)
    {
        _zedToSlice[zed] = slice;
    }

    public bool TryGetSlice(Zed zed, out Slice<TPayload>? maybeSlice) =>
        _zedToSlice.TryGetValue(zed, out maybeSlice);

    public Slice<TPayload> GetOrCreateSlice(Zed zed)
    {
        if (!TryGetSlice(zed, out var slice))
        {
            slice = new Slice<TPayload>();
            SetSlice(zed, slice);
        }
        return slice!;
    }

    public ZedBounds GetInclusiveZedBounds() =>
        IsEmpty
            ? new ZedBounds(Zed.Origin, Zed.Origin)
            : new ZedBounds(_zedToSlice.Keys.Min(), _zedToSlice.Keys.Max());

    public Bounds3d GetInclusiveBounds()
    {
        if (IsEmpty)
        {
            return new Bounds3d();
        }
        var bounds2d = 
            _zedToSlice
                .Values
                .Aggregate<Slice<TPayload>?, Bounds2d?>(null, (current, slice) => current?.Combine(slice!.GetInclusiveBounds()) ?? slice!.GetInclusiveBounds());
        return Bounds3d.Combine(bounds2d!.Value, GetInclusiveZedBounds());
    }
    
    public Voxels<TNewPayload> Clone<TNewPayload>(Func<TPayload,TNewPayload?> mapFunction) where TNewPayload : struct
    {
        var newVoxels = new Voxels<TNewPayload>();
        foreach (var entry in _zedToSlice)
        {
            newVoxels.SetSlice(entry.Key, entry.Value.Clone(mapFunction));
        }
        return newVoxels;
    }
}
