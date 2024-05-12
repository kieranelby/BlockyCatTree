namespace BlockyCatTree.Pixel;

/// <summary>
/// You can think of it as a bitmap, or as a horizontal plane from one layer of voxels. 
/// </summary>
public class Slice<TPayload> : IReadOnlyBooleanSlice where TPayload : struct
{
    private readonly Dictionary<Point2d, TPayload> _pointToPayload = new();

    public bool IsEmpty => _pointToPayload.Count == 0;

    public TPayload? Get(Point2d point2d)
    {
        return _pointToPayload.TryGetValue(point2d, out var payload)
            ? payload
            : null;
    }

    public bool Exists(Point2d point2d) => Get(point2d).HasValue;

    public void Set(Point2d point2d, TPayload payload)
    {
        _pointToPayload[point2d] = payload;
    }

    public void Remove(Point2d point2d)
    {
        _pointToPayload.Remove(point2d);
    }

    public Bounds2d GetInclusiveBounds() =>
        IsEmpty
            ? new Bounds2d(Point2d.Origin, Point2d.Origin)
            // TODO - might want to cache for performance? 
            : new Bounds2d(new Point2d(
                    _pointToPayload.Keys.Min(p => p.X),
                    _pointToPayload.Keys.Min(p => p.Y)
                ), new Point2d(
                    _pointToPayload.Keys.Max(p => p.X),
                    _pointToPayload.Keys.Max(p => p.Y)
                )
            );

    public void CopyPointsTo(List<Point2d> destination)
    {
        destination.AddRange(_pointToPayload.Keys);
    }

    public Slice<TNewPayload> Clone<TNewPayload>(Func<TPayload, TNewPayload?> mapFunction) where TNewPayload : struct
    {
        var copy = new Slice<TNewPayload>();
        foreach (var entry in _pointToPayload)
        {
            var newValue = mapFunction(entry.Value);
            if (newValue.HasValue)
            {
                copy._pointToPayload.Add(entry.Key, newValue.Value);
            }
        }
        return copy;
    }
}
