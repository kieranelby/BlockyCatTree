namespace BlockyCatTree.Pixel;

/// <summary>
/// Just an always empty slice. No need to construct, just use <see cref="Instance"/>.
/// </summary>
/// <remarks>
/// Even an empty struct uses a byte, so might as well use a byte.
/// </remarks>
public class EmptyReadOnlyBooleanSlice : Slice<byte>
{
    public static readonly IReadOnlyBooleanSlice Instance = new EmptyReadOnlyBooleanSlice();
    private EmptyReadOnlyBooleanSlice()
    {
    }
}
