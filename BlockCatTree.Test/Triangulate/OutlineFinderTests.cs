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
    public void TestBasicOutlinesExample()
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
        var exterior = outlines[0];
        Assert.That(exterior.Points, Is.EqualTo(
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
    
    [TestCase(RotationDirection.CounterClockwise)]
    [TestCase(RotationDirection.Clockwise)]
    public void TestBasicOutlineBidirectionalExample(RotationDirection rotationDirection)
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
        var outline = OutlineFinder.FindOutline(slice, rotationDirection);
        var expectedCounterClockwisePoints = new List<Point2d>
        {
            new(2, 1),
            new(3, 1),
            new(3, 2),
            new(4, 2),
            new(4, 3),
            new(3, 3),
            new(3, 4),
            new(2, 4),
            new(1, 4),
            new(1, 3),
            new(1, 2),
            new(2, 2),
            new(2, 1),
        };
        var expectedClockwisePoints = new List<Point2d>
        {
            new(3, 1),
            new(2, 1),
            new(2, 2),
            new(1, 2),
            new(1, 3),
            new(1, 4),
            new(2, 4),
            new(3, 4),
            new(3, 3),
            new(4, 3),
            new(4, 2),
            new(3, 2),
            new(3, 1),
        };
        var expectedPoints = rotationDirection == RotationDirection.CounterClockwise
            ? expectedCounterClockwisePoints
            : expectedClockwisePoints;
        Assert.That(outline, Is.Not.Null);
        Assert.That(outline.Points, Is.EqualTo(expectedPoints));
    }

}