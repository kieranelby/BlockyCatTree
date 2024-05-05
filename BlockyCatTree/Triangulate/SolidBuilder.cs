using BlockyCatTree.Mesh;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Triangulate;

/// <summary>
/// A little help with building a solid, just helps avoid duplicate
/// vertices and does the int to double conversion at the end. 
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

    public Solid Build()
    {
        var meshVertices = _blockVertices.Select(v => new Vertex(v.X, v.Y, v.Z)).ToList();
        return new Solid(_objectId, meshVertices, _triangles);
    }
}
