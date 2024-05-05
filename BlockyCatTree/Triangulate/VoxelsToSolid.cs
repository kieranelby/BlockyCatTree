using BlockyCatTree.Mesh;
using BlockyCatTree.Pixel;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Triangulate;

public static class VoxelsToSolid
{
    public static Solid Triangulate(ObjectId objectId, IReadOnlyBooleanVoxels voxels)
    {
        var sb = new SolidBuilder(objectId);
        var sliceBelow = EmptyReadOnlyBooleanSlice.Instance;
        foreach (var zed in voxels.GetInclusiveZedBounds().Expand(1))
        {
            var slice = voxels.GetReadOnlyBooleanSlice(zed);
            AddTopAndBottomFaces(sb, zed, slice, sliceBelow);
            AddWalls(sb, zed, slice);
            sliceBelow = slice;
        }
        return sb.Build();
    }

    private static void AddTopAndBottomFaces(
        SolidBuilder sb, Zed zed, IReadOnlyBooleanSlice slice, IReadOnlyBooleanSlice sliceBelow)
    {
        // Do the horizontal triangles first (they're easier ... I think?)
        var bounds = slice.GetInclusiveBounds().Combine(sliceBelow.GetInclusiveBounds());
        foreach (var point2d in bounds.IterateRowMajor())
        {
            var exists = slice.Exists(point2d);
            var existsBelow = sliceBelow.Exists(point2d);
            if (exists == existsBelow)
            {
                continue;
            }
            if (exists)
            {
                sb.AddBottomFace(zed, point2d);
            }
            else
            {
                sb.AddTopFace(zed, point2d);
            }
        }
    }

    private static void AddWalls(SolidBuilder sb, Zed zed, IReadOnlyBooleanSlice slice)
    {
        var outlines = OutlineFinder.FindOutlines(slice);
        foreach (var outline in outlines)
        {
            AddWalls(sb, zed, outline.Exterior);
            // TODO - interior
        }
    }
    
    private static void AddWalls(SolidBuilder sb, Zed zed, Path2d path2d)
    {
        if (path2d.Points.Count < 2)
        {
            return;
        }
        var previousPoint = path2d.Points.First();
        foreach (var point in path2d.Points.Skip(1))
        {
            sb.AddExteriorSideFace(zed, previousPoint, point);
            previousPoint = point;
        }
    }
}
