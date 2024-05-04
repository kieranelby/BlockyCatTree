namespace BlockyCatTree;

public static class ArrayToSlice
{
    public static Slice<TPayload> Make<TIn, TPayload>(
        IEnumerable<IEnumerable<TIn>> rows,
        Func<TIn,TPayload?> converter) where TPayload : struct
    {
        var slice = new Slice<TPayload>();
        foreach (var (row, y) in rows.Reverse().Select((r,idx) => (r,idx)))
        {
            foreach (var (cell, x) in row.Select((c,idx) => (c,idx)))
            {
                var payload = converter(cell);
                if (payload.HasValue)
                {
                    slice.Set(new Point2d(x, y), payload.Value);
                }
            }
        }
        return slice;
    }
}
