using BlockyCatTree.Voxel;

namespace BlockCatTree.Test.Voxel;

[TestFixture]
public class ZedBoundsTests
{
    [Test]
    public void TestBasics()
    {
        var bounds = new ZedBounds(new Zed(2), new Zed(4));
        Assert.That(bounds.ToList(), Is.EqualTo(new List<Zed>
        {
            new(2), new(3), new(4),
        }));
        var biggerBounds = bounds.Expand(2);
        Assert.That(biggerBounds.ToList(), Is.EqualTo(new List<Zed>
        {
            new(0), new(1), 
            new(2), new(3), new(4),
            new(5), new(6),
        }));
    }
}
