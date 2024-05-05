using BlockyCatTree.Pixel;
using BlockyCatTree.Voxel;
using BlockyCatTree.Voxel.IO;

namespace BlockCatTree.Test.Voxel.IO;

public class ArrayToVoxelTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestBasicExample()
    {
        // slices here go from top to bottom
        // don't forget 'y' goes up, not down and finishes at zero
        var input = new []
        {
            new [] {
                "#o#",
                "#oo",
                "#",
            },
            new [] {
                " ##",
                "##*",
            }
        };
        var voxels = ArrayToVoxels.Make(input, Convert);
        Assert.That(voxels.GetZedInclusiveBounds(), Is.EqualTo((new Zed(0), new Zed(1))));
        Assert.That(voxels.TryGetSlice(new Zed(1), out var upperSlice), Is.True);
        Assert.That(voxels.TryGetSlice(new Zed(0), out var lowerSlice), Is.True);
        Assert.That(upperSlice, Is.Not.Null);
        Assert.That(upperSlice.GetInclusiveBounds(), Is.EqualTo(new Bounds2d(new Point2d(0, 0), new Point2d(2, 2))));
        Assert.That(lowerSlice, Is.Not.Null);
        Assert.That(lowerSlice.GetInclusiveBounds(), Is.EqualTo(new Bounds2d(new Point2d(0, 0), new Point2d(2, 1))));
        Assert.That(voxels.Get(new Point3d(2, 0, 0)), Is.EqualTo(2));
        Assert.That(voxels.Get(new Point3d(1, 1, 1)), Is.EqualTo(0));
        return;
        int? Convert(char c) => c switch
            {
                '*' => 2,
                '#' => 1,
                'o' => 0,
                ' ' => null,
                _ => throw new Exception($"unexpected char '{c}'")
            };
    }
}