using System.Numerics;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;

namespace BlockCatTree.Test.Mesh.IO;

[TestFixture]
public class BasicThreeEmEffRoundTripTests
{
    private string _tempFilePath;

    [SetUp]
    public void Setup()
    {
        var baseTempPath = Path.GetTempPath();
        _tempFilePath = Path.Join(baseTempPath, Path.GetRandomFileName());
    }

    [TearDown]
    public void Teardown()
    {
        File.Delete(_tempFilePath);
    }

    [Test]
    public void TestWriteModelAndReadBack()
    {
        var originalModel = MakeSingleCuboidModel();
        BasicThreeEmEffWriter.Write(_tempFilePath, originalModel);
        var readBackModel = BasicThreeEmEffReader.Read(_tempFilePath);
        Assume.That(originalModel.Solids.Count, Is.EqualTo(1));
        Assume.That(originalModel.BuildItems.Count, Is.EqualTo(1));
        Assert.That(readBackModel.Solids.Count, Is.EqualTo(originalModel.Solids.Count));
        Assert.That(readBackModel.BuildItems.Count, Is.EqualTo(originalModel.BuildItems.Count));
        Assert.That(readBackModel.Solids[0].ObjectId, Is.EqualTo(originalModel.Solids[0].ObjectId));
        Assert.That(readBackModel.Solids[0].Vertices, Is.EqualTo(originalModel.Solids[0].Vertices));
        Assert.That(readBackModel.Solids[0].Triangles, Is.EqualTo(originalModel.Solids[0].Triangles));
        Assert.That(readBackModel.BuildItems[0].ObjectId, Is.EqualTo(originalModel.BuildItems[0].ObjectId));
        Assert.That(readBackModel.BuildItems[0].Transform, Is.EqualTo(originalModel.BuildItems[0].Transform));
    }

    [Test]
    [Explicit]
    public void ExplicitTestWriteModelToFile()
    {
        var originalModel = MakeSingleCuboidModel();
        BasicThreeEmEffWriter.Write("E:/example01.3mf", originalModel);
    }

    private static Model MakeSingleCuboidModel()
    {
        // example taken from https://en.wikipedia.org/wiki/3D_Manufacturing_Format#Sample_file
        var vertices = new List<Vertex>
        {
            new (0,0,0),
            new (1,0,0),
            new (1,2,0),
            new (0,2,0),
            new (0,0,3),
            new (1,0,3),
            new (1,2,3),
            new (0,2,3),
        };
        var triangles = new List<Triangle>
        {
            new (3,2,1),
            new (1,0,3),
            new (4,5,6),
            new (6,7,4),
            new (0,1,5),
            new (5,4,0),
            new (1,2,6),
            new (6,5,1),
            new (2,3,7),
            new (7,6,2),
            new (3,0,4),
            new (4,7,3),
        };
        var objectId = new ObjectId(1);
        var solid = new Solid(objectId, vertices, triangles);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        return new Model([solid], [buildItem]);
    }
}
