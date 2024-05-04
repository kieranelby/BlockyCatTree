using System.Text;

namespace BlockyCatTree;

/// <summary>
/// Helps print the contents of a slice to a terminal for debugging,
/// or to make unit tests more readable.
/// </summary>
public static class SliceToText
{
    public static IEnumerable<string> Make<TPayload>(
        Slice<TPayload> slice) where TPayload : struct
    {
        return Make(slice, Convert);
        char Convert(TPayload? payload) => payload.HasValue ? '#' : '.';
    }

    public static IEnumerable<string> Make<TPayload>(
        Slice<TPayload> slice,
        Func<TPayload?, char> converter) where TPayload : struct
    {
        var bounds = slice.GetInclusiveBounds();
        for (var y = bounds.Max.Y; y >= bounds.Min.Y; y--)
        {
            var sb = new StringBuilder();
            for (var x = bounds.Min.X; x <= bounds.Max.X; x++)
            {
                var maybePayload = slice.Get(new Point2d(x, y));
                var c = converter(maybePayload);
                sb.Append(c);
            }
            yield return sb.ToString();
        }
    }
}
