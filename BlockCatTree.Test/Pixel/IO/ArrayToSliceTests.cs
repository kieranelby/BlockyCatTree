using BlockyCatTree.Pixel;
using BlockyCatTree.Pixel.IO;

namespace BlockCatTree.Test.Pixel.IO;

public class ArrayToSliceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestBasicExample()
    {
        // don't forget 'y' goes up, not down
        var input = new []
        {
            "#  #",
            "# o",
            "#",
        };
        var slice = ArrayToSlice.Make(input, Convert);
        Assert.That(slice.GetInclusiveBounds(), Is.EqualTo(new Bounds2d(new Point2d(0, 0), new Point2d(3, 2))));
        Assert.That(slice.Get(new Point2d(3, 2)), Is.EqualTo(1));
        Assert.That(slice.Get(new Point2d(2, 1)), Is.EqualTo(0));
        Assert.That(slice.Get(new Point2d(1, 2)), Is.Null);
        Assert.That(slice.Get(new Point2d(3, 3)), Is.Null);
        return;
        int? Convert(char c) => c switch
            {
                '#' => 1,
                'o' => 0,
                ' ' => null,
                _ => throw new Exception($"unexpected char '{c}'")
            };
    }
}