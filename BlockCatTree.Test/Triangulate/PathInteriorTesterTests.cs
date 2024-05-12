using BlockyCatTree.Pixel;
using BlockyCatTree.Pixel.IO;
using BlockyCatTree.Triangulate;

namespace BlockCatTree.Test.Triangulate;

public class PathInteriorTesterTests
{
    [SetUp]
    public void Setup()
    {
    }

    private static int? Convert(char c) => c switch
    {
        '#' => 1,
        ' ' => null,
        _ => throw new Exception($"unexpected char '{c}'")
    };

    [Test]
    public void TestBasicOutlineExample()
    {
        var input = new []
        {
        //   01234
            "     ", // 4
            " ##  ", // 3
            " ### ", // 2
            "  #  ", // 1
            "     ", // 0
        //   01234
        };
        var slice = ArrayToSlice.Make(input, Convert);
        var outlines = OutlineFinder.FindAllPaths(slice);
        Assert.That(outlines, Has.Count.EqualTo(1));
        var exterior = outlines[0];
        var uut = new PathInteriorTester(exterior);
        foreach (var point2d in slice.GetInclusiveBounds().IterateRowMajor())
        {
            Assert.That(uut.Inside(point2d), Is.EqualTo(slice.Exists(point2d)), $"Slice and InteriorTester do not agree at {point2d}");
        }
    }
}