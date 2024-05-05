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
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        Assert.That(solid.ObjectId, Is.EqualTo(objectId));
        Assert.That(solid.Vertices, Has.Count.EqualTo(8));
        // ok, we're assuming quite a lot here, namely that we:
        //  - work left to right
        //  - work front to back
        //  - work bottom to top
        //  - for each level, do ceiling of last and floor of new at the same time
        //  - do the walls after the floor for each level
        Assert.That(solid.Vertices, Is.EqualTo(new List<Vertex>
        {
            new (0,0,0),
            new (0,1,0),
            new (1,1,0),
            new (1,0,0),
            new (1,0,1),
            new (0,0,1),
            new (1,1,1),
            new (0,1,1),
        }));
        Assert.That(solid.Triangles, Has.Count.EqualTo(12));
        // as well as the assumptions 
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
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        var model = new Model([solid], [buildItem]);
        BasicThreeEmEffWriter.Write("E:/example02.3mf", model);
    }

    [Test]
    [Explicit]
    public void ExplicitTestSimpleExampleToFile()
    {
        var input = new []
        {
            new [] {
                "#  ",
                "#  "
            },
            new [] {
                "#  ",
                "###",
            },
            new [] {
                "###",
                "###"
            },
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        var model = new Model([solid], [buildItem]);
        BasicThreeEmEffWriter.Write("E:/example03.3mf", model);
    }

    private static int? Convert(char c) => c switch
    {
        '#' => 1,
        ' ' => null,
        _ => throw new Exception($"unexpected char '{c}'")
    };
}
