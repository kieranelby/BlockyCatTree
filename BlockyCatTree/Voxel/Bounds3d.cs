using BlockyCatTree.Pixel;

namespace BlockyCatTree.Voxel;

public readonly record struct Bounds3d(Point3d Min, Point3d Max)
{
    public Bounds3d Scale(int multiplier) => new(Min.ScaleUp(multiplier), Max.ScaleUp(multiplier));

    public IEnumerable<Point3d> Iterate()
    {
        for (var z = Min.Z; z <= Max.Z; z++)
        {
            for (var y = Min.Y; y <= Max.Y; y++)
            {
                for (var x = Min.X; x <= Max.X; x++)
                {
                    yield return new Point3d(x, y, z);
                }
            }
        }
    }
    
    public static Bounds3d Combine(Bounds2d bounds2d, ZedBounds zedBounds) =>
        new(new Point3d(
                bounds2d.Min.X,
                bounds2d.Min.Y,
                zedBounds.Min.Value), 
            new Point3d(
                bounds2d.Max.X,
                bounds2d.Max.Y,
                zedBounds.Max.Value));

    public Bounds3d Expand(int dx, int dy, int dz) => new(Min.Plus(new Point3d(-dx, -dy, -dz)), Max.Plus(new Point3d(dx, dy, dz)));
}
