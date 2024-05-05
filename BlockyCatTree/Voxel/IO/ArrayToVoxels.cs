using BlockyCatTree.Pixel.IO;

namespace BlockyCatTree.Voxel.IO;

public static class ArrayToVoxels
{
    public static Voxels<TPayload> Make<TIn, TPayload>(
        IEnumerable<IEnumerable<IEnumerable<TIn>>> sliceArrays,
        Func<TIn,TPayload?> converter) where TPayload : struct
    {
        var voxels = new Voxels<TPayload>();
        foreach (var (sliceArray, z) in sliceArrays.Reverse().Select((sa,idx) => (sa,idx)))
        {
            var slice = ArrayToSlice.Make(sliceArray, converter);
            voxels.SetSlice(new Zed(z), slice);
        }
        return voxels;
    }
}