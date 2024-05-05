using BlockyCatTree.Voxel;

namespace BlockCatTree.Test.Voxel;

public class VoxelTests
{
    private Point3d _pointA1;
    private Point3d _pointB1;
    private Point3d _pointB2;
    private Point3d _pointB3;
    private Voxels<int> _uut;

    [SetUp]
    public void Setup()
    {
        _pointA1 = new Point3d(1, 20, 3);
        _pointB1 = new Point3d(4, 5, 6);
        _pointB2 = new Point3d(5, 4, 6);
        _pointB3 = new Point3d(6, 6, 6);
        _uut = new Voxels<int>();
    }

    [Test]
    public void TestBasicOperations()
    {
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.GetZedInclusiveBounds(), Is.EqualTo((Zed.Origin, Zed.Origin)));
        Assert.That(_uut.Get(_pointA1), Is.EqualTo(null));
        _uut.Remove(_pointA1);
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.Get(_pointA1), Is.EqualTo(null));
        _uut.Set(_pointA1, 0);
        _uut.Set(_pointB1, 10);
        _uut.Set(_pointB2, 100);
        Assert.That(_uut.Get(_pointA1), Is.EqualTo(0));
        Assert.That(_uut.Get(_pointB1), Is.EqualTo(10));
        Assert.That(_uut.Get(_pointB2), Is.EqualTo(100));
        Assert.That(_uut.Get(_pointB3), Is.EqualTo(null));
        Assert.That(_uut.IsEmpty, Is.False);
        Assert.That(_uut.GetZedInclusiveBounds(), Is.EqualTo((_pointA1.Zed, _pointB2.Zed)));
        _uut.Remove(_pointA1);
        Assert.That(_uut.Get(_pointA1), Is.EqualTo(null));
        Assert.That(_uut.IsEmpty, Is.False);
        Assert.That(_uut.GetZedInclusiveBounds(), Is.EqualTo((_pointB1.Zed, _pointB2.Zed)));
        _uut.Remove(_pointB1);
        _uut.Remove(_pointB2);
        Assert.That(_uut.IsEmpty, Is.True);
        Assert.That(_uut.GetZedInclusiveBounds(), Is.EqualTo((Zed.Origin, Zed.Origin)));
    }

    [Test]
    public void TestSlicing()
    {
        Assume.That(_uut.IsEmpty, Is.True);
        _uut.Set(_pointA1, 1);
        _uut.Set(_pointB1, 2);
        var bounds = _uut.GetZedInclusiveBounds();
        Assume.That(bounds, Is.EqualTo((new Zed(3), new Zed(6))));
        foreach (var zed in Zed.InclusiveRange(bounds))
        {
            var hasSlice = _uut.TryGetSlice(zed, out var maybeSlice);
            if (zed == _pointA1.Zed || zed == _pointB1.Zed)
            {
                Assert.That(hasSlice, Is.True);
                Assert.That(maybeSlice, Is.Not.Null);
                if (zed == _pointA1.Zed)
                {
                    Assert.That(maybeSlice.Get(_pointA1.Point2d), Is.EqualTo(1));
                    Assert.That(maybeSlice.Get(_pointB1.Point2d), Is.Null);
                }
                else if (zed == _pointB1.Zed)
                {
                    Assert.That(maybeSlice.Get(_pointB1.Point2d), Is.EqualTo(2));
                    Assert.That(maybeSlice.Get(_pointA1.Point2d), Is.Null);
                } 
            }
            else
            {
                Assert.That(hasSlice, Is.False);
                Assert.That(maybeSlice, Is.Null);
            }
        }
    }
}