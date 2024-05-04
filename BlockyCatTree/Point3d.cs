namespace BlockyCatTree;

/// <remarks>
/// Note that we follow a more mathematical convention as
/// opposed to some graphics conventions:
///  - increasing X moves right
///  - increasing Y moves away from the viewer
///  - increasing Z moves higher and higher
/// We treat Z as a bit special - see <see cref="Zed"/>.
/// </remarks>
public readonly record struct Point3d(int X, int Y, int Z)
{
    public static readonly Point3d Origin = new Point3d(0, 0, 0);
    
    public Point2d Point2d => new Point2d(X, Y);
    public Zed Zed => new Zed(Z);
}