using System.Numerics;
using BlockyCatTree.Generation;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;
using BlockyCatTree.Triangulate;

namespace BlockCatTree.Test.Generation;

[TestFixture]
public class SpaceColonizationTests
{
    private SpaceColonization _uut;

    [SetUp]
    public void Setup()
    {
        _uut = new SpaceColonization(new Random(12345));
    }

    [Test]
    [Explicit]
    public void ExplicitTestExampleToFile()
    {
        _uut.RunToEnd();
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, _uut.Voxels);
        var objectId2 = objectId.Next;
        var solid2 = VoxelsToSolid.Triangulate(objectId2, _uut.InternalVoxels);
        var transform = Matrix4x4.CreateTranslation(128.0f, 128.0f, 0.0f);
        var buildItem = new BuildItem(objectId, transform);
        var buildItem2 = new BuildItem(objectId2, transform);
        //var model = new Model([solid, solid2], [buildItem, buildItem2]);
        var model = new Model([solid], [buildItem]);
        BasicThreeEmEffWriter.Write("E:/example06.3mf", model);
    }
}