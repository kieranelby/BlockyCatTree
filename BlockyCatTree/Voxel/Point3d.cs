using System.Numerics;
using BlockyCatTree.Pixel;
using BlockyCatTree.Triangulate;

namespace BlockyCatTree.Voxel;

/// <remarks>
/// Note that we follow a more mathematical convention as
/// opposed to some graphics conventions:
///  - increasing X moves right
///  - increasing Y moves away from the viewer
///  - increasing Z moves higher and higher
/// We treat Z as a bit special - see <see cref="Zed"/>.
/// Since our co-ordinates are integer, it really refers to the corner
/// with smallest X, Y, Z of the cube (not center!).
/// </remarks>
public readonly record struct Point3d(int X, int Y, int Z)
{
    public static readonly Point3d Origin = new Point3d(0, 0, 0);
    
    public Point2d Point2d => new Point2d(X, Y);
    public Zed Zed => new Zed(Z);

    public Point3d Plus(Point3d other)
    {
        return new Point3d(X + other.X, Y + other.Y, Z + other.Z);
    }

    public Point3d Minus(Point3d other)
    {
        return new Point3d(X - other.X, Y - other.Y, Z - other.Z);
    }

    public Point3d RotateAroundZed(RotationDirection rotationDirection)
    {
        var rotatedPoint2d = Point2d.Rotate(rotationDirection);
        return new Point3d(rotatedPoint2d.X, rotatedPoint2d.Y, Z);
    }

    public float Length => AsVector3.Length();

    public Vector3 AsVector3 => new Vector3((float)X, (float)Y, (float)Z);

    public Point3d ScaleUp(int multiplier) => new(X * multiplier, Y * multiplier, Z * multiplier);
    public Point3d ScaleDown(int divisor) => new(X / divisor, Y / divisor, Z / divisor);

    public Point3d FlipAroundY()
    {
        return this with { X = -X };
    }
}

