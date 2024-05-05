using System.Numerics;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;
using BlockyCatTree.Triangulate;
using BlockyCatTree.Voxel.IO;

namespace BlockCatTree.Test.Triangulate;

[TestFixture]
public class VoxelsToSolidTests
{
    [Test]
    public void TestMinimalExample()
    {
        var input = new []
        {
            new [] {
                "#",
            },
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        var voxelsToSolid = new VoxelsToSolid();
        var objectId = ObjectId.FirstId;
        var solid = voxelsToSolid.Triangulate(objectId, voxels);
        Assert.That(solid.ObjectId, Is.EqualTo(objectId));
        Assert.That(solid.Vertices, Has.Count.EqualTo(8));
        Assert.That(solid.Triangles, Has.Count.EqualTo(12));
    }

    [Test]
    [Explicit]
    public void ExplicitTestMinimalExampleToFile()
    {
        var input = new []
        {
            new [] {
                "#",
            },
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        var voxelsToSolid = new VoxelsToSolid();
        var objectId = ObjectId.FirstId;
        var solid = voxelsToSolid.Triangulate(objectId, voxels);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        var model = new Model([solid], [buildItem]);
        BasicThreeEmEffWriter.Write("E:/example02.3mf", model);
    }

    private static int? Convert(char c) => c switch
    {
        '#' => 1,
        ' ' => null,
        _ => throw new Exception($"unexpected char '{c}'")
    };
}
