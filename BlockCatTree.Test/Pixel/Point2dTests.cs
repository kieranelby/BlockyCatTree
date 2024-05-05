using BlockyCatTree.Pixel;
using BlockyCatTree.Triangulate;

namespace BlockCatTree.Test.Pixel;

[TestFixture]
public class Point2dTests
{
    [Test]
    public void TestRotationClockwise()
    {
        var point2d = new Point2d(3,2).Rotate(RotationDirection.Clockwise);
        Assert.That(point2d, Is.EqualTo(new Point2d(-2, 3)));
    }

    [Test]
    public void TestRotationCounterClockwise()
    {
        var point2d = new Point2d(-2,3).Rotate(RotationDirection.CounterClockwise);
        Assert.That(point2d, Is.EqualTo(new Point2d(3, 2)));
    }
}