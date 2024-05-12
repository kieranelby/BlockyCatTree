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
            //1234
            "     ", // 4
            " ##  ", // 3
            " ### ", // 2
            "  #  ", // 1
            "     ", // 0
            //1234
        };
        var slice = ArrayToSlice.Make(input, Convert);
        var outlines = OutlineFinder.FindAllPaths(slice);
        Assert.That(outlines, Has.Count.EqualTo(1));
        var exterior = outlines[0];
        Assert.That(exterior.RotationDirection, Is.EqualTo(RotationDirection.CounterClockwise));
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
            //1234
            "     ", // 4
            " ##  ", // 3
            " ### ", // 2
            "  #  ", // 1
            "     ", // 0
            //1234
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
        Assert.That(outline.RotationDirection, Is.EqualTo(rotationDirection));
    }

    [Test]
    public void TestMultipleOutlinesExample()
    {
        var input = new []
        {
            //1234
            "     ", // 4
            " #   ", // 3
            "     ", // 2
            " # # ", // 1
            "     ", // 0
            //1234
        };
        var slice = ArrayToSlice.Make(input, Convert);
        var outlines = OutlineFinder.FindAllPaths(slice);
        Assert.That(outlines, Has.Count.EqualTo(3));
        // We assume we find the lower region first,
        // or if tied, the left-most
        var exteriorA = outlines[0];
        Assert.That(exteriorA.RotationDirection, Is.EqualTo(RotationDirection.CounterClockwise));
        Assert.That(exteriorA.Points, Is.EqualTo(
            new List<Point2d>
            {
                new (1,1),
                new (2,1),
                new (2,2), 
                new (1,2),
                new (1,1), 
            }));
        var exteriorB = outlines[1];
        Assert.That(exteriorB.RotationDirection, Is.EqualTo(RotationDirection.CounterClockwise));
        Assert.That(exteriorB.Points, Is.EqualTo(
            new List<Point2d>
            {
                new (3,1),
                new (4,1),
                new (4,2), 
                new (3,2),
                new (3,1), 
            }));
        var exteriorC = outlines[2];
        Assert.That(exteriorC.RotationDirection, Is.EqualTo(RotationDirection.CounterClockwise));
        Assert.That(exteriorC.Points, Is.EqualTo(
            new List<Point2d>
            {
                new (1,3),
                new (2,3),
                new (2,4), 
                new (1,4),
                new (1,3), 
            }));
    }
    
    [Test]
    public void TestHollowExample()
    {
        var input = new []
        {
            //1234
            "     ", // 4
            " ### ", // 3
            " # # ", // 2
            " ### ", // 1
            "     ", // 0
            //1234
        };
        var slice = ArrayToSlice.Make(input, Convert);
        var outlines = OutlineFinder.FindAllPaths(slice);
        // We assume it returns the outside path first, then the inside path,
        // and that inside paths run in the opposite direction
        Assert.That(outlines, Has.Count.EqualTo(2));
        var exteriorA = outlines[0];
        Assert.That(exteriorA.RotationDirection, Is.EqualTo(RotationDirection.CounterClockwise));
        Assert.That(exteriorA.Points, Has.Count.EqualTo(13));
        var exteriorB = outlines[1];
        Assert.That(exteriorB.RotationDirection, Is.EqualTo(RotationDirection.Clockwise));
        // We assume for clockwise paths the starting position is one to the right
        Assert.That(exteriorB.Points, Is.EqualTo(
            new List<Point2d>
            {
                new (3,2),
                new (2,2), 
                new (2,3),
                new (3,3), 
                new (3,2),
            }));
    }    
}
