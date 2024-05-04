using BlockyCatTree;

namespace BlockCatTree.Test;

public class SliceTests
{
    private Point2d _pointA;
    private Point2d _pointB;
    private Point2d _pointC;
    private Point2d _pointD;
    private Slice<int> _uut;

    [SetUp]
    public void Setup()
    {
        _pointA = new Point2d(1, 20);
        _pointB = new Point2d(4, 7);
        _pointC = new Point2d(5, 4);
        _pointD = new Point2d(6, 6);
        _uut = new Slice<int>();
    }

    [Test]
    public void TestBasicOperations()
    {
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.GetInclusiveBounds(), Is.EqualTo((Point2d.Origin, Point2d.Origin)));
        Assert.That(_uut.Get(_pointA), Is.EqualTo(null));
        _uut.Remove(_pointA);
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.Get(_pointA), Is.EqualTo(null));
        _uut.Set(_pointA, 0);
        _uut.Set(_pointB, 10);
        _uut.Set(_pointC, 100);
        Assert.That(_uut.GetInclusiveBounds(), Is.EqualTo(
            (new Point2d(1, 4), new Point2d(5, 20))
        ));
        Assert.That(_uut.Get(_pointA), Is.EqualTo(0));
        Assert.That(_uut.Get(_pointB), Is.EqualTo(10));
        Assert.That(_uut.Get(_pointC), Is.EqualTo(100));
        Assert.That(_uut.Get(_pointD), Is.EqualTo(null));
        Assert.That(_uut.IsEmpty, Is.False);
        _uut.Remove(_pointA);
        Assert.That(_uut.IsEmpty, Is.False);
        Assert.That(_uut.GetInclusiveBounds(), Is.EqualTo(
            (new Point2d(4, 4), new Point2d(5, 7))
        ));
        Assert.That(_uut.Get(_pointA), Is.EqualTo(null));
        _uut.Remove(_pointB);
        _uut.Remove(_pointC);
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.GetInclusiveBounds(), Is.EqualTo((Point2d.Origin, Point2d.Origin)));
    }
}