using BlockyCatTree.Mesh;
using BlockyCatTree.Pixel;
using BlockyCatTree.Triangulate;
using BlockyCatTree.Voxel;

namespace BlockCatTree.Test.Triangulate;

public class SolidBuilderTests
{
    private ObjectId _objectId;
    private SolidBuilder _uut;
    private Zed _zed;

    [SetUp]
    public void Setup()
    {
        _objectId = ObjectId.FirstId;
        _uut = new SolidBuilder(_objectId);
        _zed = new Zed(10);
    }

    [Test]
    public void TestEmptyCase()
    {
        var solid = _uut.Build();
        Assert.That(solid.ObjectId, Is.EqualTo(_objectId));
        Assert.That(solid.Vertices, Is.Empty);
        Assert.That(solid.Triangles, Is.Empty);
    }

    [Test]
    public void TestAddTriangles()
    {
        var a = new Point3d(2, 5, _zed.Value);
        var b = new Point3d(3, 5, _zed.Value);
        var c = new Point3d(2, 6, _zed.Value);
        var d = new Point3d(2, 5, _zed.Value + 1);
        _uut.AddTriangle(a, b, c);
        _uut.AddTriangle(a, b, d);
        var solid = _uut.Build();
        Assert.That(solid.Vertices, Is.EqualTo(new List<Vertex>
        {
            new(a.X, a.Y, a.Z),
            new(b.X, b.Y, b.Z),
            new(c.X, c.Y, c.Z),
            new(d.X, d.Y, d.Z),
        }));
        Assert.That(solid.Triangles, Is.EqualTo(new List<Triangle>
        {
            new(0, 1, 2),
            new(0, 1, 3),
        }));
    }

    [Test]
    public void TestAddBottomFace()
    {
        _uut.AddBottomFace(_zed, new Point2d(2, 5));
        var solid = _uut.Build();
        Assert.That(solid.Vertices, Is.EqualTo(new List<Vertex>
        {
            new(2, 5, _zed.Value),
            new(2, 6, _zed.Value),
            new(3, 6, _zed.Value),
            new(3, 5, _zed.Value),
        }));
        Assert.That(solid.Triangles, Is.EqualTo(new List<Triangle>
        {
            new(0, 1, 2),
            new(0, 2, 3),
        }));
    }

    [Test]
    public void TestAddTopFace()
    {
        _uut.AddTopFace(_zed, new Point2d(2, 5));
        var solid = _uut.Build();
        Assert.That(solid.Vertices, Is.EqualTo(new List<Vertex>
        {
            new(2, 5, _zed.Value),
            new(3, 5, _zed.Value),
            new(3, 6, _zed.Value),
            new(2, 6, _zed.Value),
        }));
        Assert.That(solid.Triangles, Is.EqualTo(new List<Triangle>
        {
            new(0, 1, 2),
            new(0, 2, 3),
        }));
    }

    [Test]
    public void TestAddExteriorSideFace()
    {
        _uut.AddExteriorSideFace(_zed, new Point2d(2, 5), new Point2d(3, 5));
        var solid = _uut.Build();
        Assert.That(solid.Vertices, Is.EqualTo(new List<Vertex>
        {
            new(2, 5, _zed.Value),
            new(3, 5, _zed.Value),
            new(3, 5, _zed.Value + 1),
            new(2, 5, _zed.Value + 1),
        }));
        Assert.That(solid.Triangles, Is.EqualTo(new List<Triangle>
        {
            new(0, 1, 2),
            new(0, 2, 3),
        }));
    }
}
