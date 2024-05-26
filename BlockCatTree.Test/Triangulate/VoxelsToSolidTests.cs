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
        //  - do exterior walls counter-clockwise
        //  - there aren't any interior ones here, but would be clockwise if so
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
        // as well as the assumptions above, we're assuming that when making square:
        //   D C
        //   A B
        // we do triangle ABC then ACD
        Assert.That(solid.Triangles, Is.EqualTo(new List<Triangle>
        {
            new (0,1,2), new (0,2,3),
            new (0,3,4), new (0,4,5),
            new (3,2,6), new (3,6,4),
            new (2,1,7), new (2,7,6),
            new (1,0,5), new (1,5,7),
            new (5,4,6), new (5,6,7),
        }));
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
        var model = new Model([solid], [buildItem], []);
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
        var model = new Model([solid], [buildItem], []);
        BasicThreeEmEffWriter.Write("E:/example03.3mf", model);
    }

    [Test]
    [Explicit]
    public void ExplicitTestDubiousExampleToFile()
    {
        // this has some unsupported cubes that connect only via one corner
        var input = new []
        {
            new []
            {
                //123456789
                "         #", // 4
                "#         ", // 3
                "          ", // 2
                "          ", // 1
                "#         ", // 0
                //123456789
            },
            new []
            {
            //123456789
            "#   # #   ", // 4
            " ## ##### ", // 3
            " #### ##  ", // 2
            "  ##  ### ", // 1
            " ####  #  ", // 0
            //123456789
            }
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        var model = new Model([solid], [buildItem], []);
        BasicThreeEmEffWriter.Write("E:/example04.3mf", model);
    }

    [Test]
    [Explicit]
    public void ExplicitTestComplexExampleToFile()
    {
        var input = new []
        {
            // as well as multiple outer regions, this has multiple nested regions
            new []
            {
                //123456789012345
                "                ",// 6
                " ##   X  X X    ",// 5
                " #  XXXXXXXXX   ",// 4
                "   XX       XX  ",// 3
                "  XX   WWW  XX  ",// 2
                "  XX  WWWWW XX  ",// 1
                "  XX        XX  ",// 0
                " XXX  MMMMM  XX ",// 9
                " XXX  M   MM X  ",// 8
                " XXX  M    M X  ",// 7
                " XXX  M  8 M X  ",// 6
                " XXX  M    M X  ",// 5
                " XXX  MMMMMM XX ",// 4
                " XXX  MMM    X  ",// 3
                "  XX      XXXX  ",// 2
                "   XXXXXXXXX    ",// 1
                "                ",// 0
                //123456789012345
            },
            new []
            {
                //12345678901234
                "##############  ",// 6
                "############### ",// 5
                "################",// 4
                "################",// 3
                "################",// 2
                "################",// 1
                "################",// 0
                "################",// 9
                "################",// 8
                "################",// 7
                "################",// 6
                "################",// 5
                "################",// 4
                "################",// 3
                "############### ",// 2
                " #############  ",// 1
                "  ###########   ",// 0
                //12345678901234
            }
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        var buildItem = new BuildItem(objectId, Matrix4x4.Identity);
        var model = new Model([solid], [buildItem], []);
        BasicThreeEmEffWriter.Write("E:/example05.3mf", model);
    }

    private static int? Convert(char c) => c switch
    {
        ' ' => null,
        _ => 1
    };
}
