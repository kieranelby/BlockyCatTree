using BlockyCatTree.Pixel;
using BlockyCatTree.Pixel.IO;
using BlockyCatTree.Triangulate;

namespace BlockCatTree.Test.Triangulate;

public class OutlineFinderTests
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
    public void TestBasicExample()
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
        var outlines = OutlineFinder.FindOutlines(slice);
        Assert.That(outlines, Has.Count.EqualTo(1));
        var outline = outlines[0];
        Assert.That(outline.Interior.IsEmpty, Is.True);
        Assert.That(outline.Exterior.Points, Is.EqualTo(
            new List<Point2d>
            {
                new (2,1),
                new (3,1), 
                new (3,2),
                new (4,2), 
                new (4,3),
                new (3,3), 
                new (3,4), 
                new (2,4),
                new (1,4), 
                new (1,3), 
                new (1,2),
                new (2,2), 
                new (2,1),
            }));
    }
}