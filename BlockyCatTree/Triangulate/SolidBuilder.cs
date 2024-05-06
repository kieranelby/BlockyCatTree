using BlockyCatTree.Mesh;
using BlockyCatTree.Pixel;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// A little help with building a solid, avoids duplicate vertices,
/// does quad to triangle, and the int to double conversion at the end. 
/// </summary>
public class SolidBuilder
{
    private readonly ObjectId _objectId;
    private readonly Dictionary<Point3d, int> _pointToVertexIndex = new();
    private readonly List<Point3d> _blockVertices = [];
    private readonly List<Triangle> _triangles = [];

    public SolidBuilder(ObjectId objectId)
    {
        _objectId = objectId;
    }

    public void AddTriangle(Point3d v1, Point3d v2, Point3d v3)
    {
        _triangles.Add(new Triangle(
            GetOrCreateVertexIndex(v1),
            GetOrCreateVertexIndex(v2),
            GetOrCreateVertexIndex(v3)));
    }

    private int GetOrCreateVertexIndex(Point3d v)
    {
        if (_pointToVertexIndex.TryGetValue(v, out var index))
        {
            return index;
        }
        index = _blockVertices.Count;
        _blockVertices.Add(v);
        _pointToVertexIndex[v] = index;
        return index;
    }

    public void AddTopFace(Zed zed, Point2d point2d)
    {
        var vA = new Point3d(point2d.X + 0, point2d.Y + 0, zed.Value);
        var vB = new Point3d(point2d.X + 1, point2d.Y + 0, zed.Value);
        var vC = new Point3d(point2d.X + 1, point2d.Y + 1, zed.Value);
        var vD = new Point3d(point2d.X + 0, point2d.Y + 1, zed.Value);
        AddTriangle(vA, vB, vC);
        AddTriangle(vA, vC, vD);
    }

    public void AddBottomFace(Zed zed, Point2d point2d)
    {
        var vA = new Point3d(point2d.X + 0, point2d.Y + 0, zed.Value);
        var vB = new Point3d(point2d.X + 1, point2d.Y + 0, zed.Value);
        var vC = new Point3d(point2d.X + 1, point2d.Y + 1, zed.Value);
        var vD = new Point3d(point2d.X + 0, point2d.Y + 1, zed.Value);
        AddTriangle(vA, vD, vC);
        AddTriangle(vA, vC, vB);
    }
    
    public Solid Build()
    {
        var meshVertices = _blockVertices.Select(v => new Vertex(v.X, v.Y, v.Z)).ToList();
        return new Solid(_objectId, meshVertices, _triangles);
    }

    public void AddExteriorSideFace(Zed zed, Point2d point1, Point2d point2)
    {
        var vA = new Point3d(point1.X, point1.Y, zed.Value);
        var vB = new Point3d(point2.X, point2.Y, zed.Value);
        var vC = new Point3d(point2.X, point2.Y, zed.Value + 1);
        var vD = new Point3d(point1.X, point1.Y, zed.Value + 1);
        // We don't need another version with the opposite normal, since our paths go
        // counter-clockwise for exterior outline and clockwise for interior outline.
        AddTriangle(vA, vB, vC);
        AddTriangle(vA, vC, vD);
    }
}
