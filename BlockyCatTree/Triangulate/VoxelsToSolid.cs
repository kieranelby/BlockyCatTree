using BlockyCatTree.Mesh;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Triangulate;

public class VoxelsToSolid
{
    public Solid Triangulate(ObjectId objectId, IReadOnlyBooleanVoxels voxels)
    {
        var sb = new SolidBuilder(objectId);
        return sb.Build();
    }
}