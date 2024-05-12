using BlockyCatTree.Pixel;

namespace BlockyCatTree.Voxel;

public interface IReadOnlyBooleanVoxels
{
    bool IsEmpty { get; }
    bool Exists(Point3d point3d);
    IReadOnlyBooleanSlice GetReadOnlyBooleanSlice(Zed zed);
    ZedBounds GetInclusiveZedBounds();
    Bounds3d GetInclusiveBounds();
}
