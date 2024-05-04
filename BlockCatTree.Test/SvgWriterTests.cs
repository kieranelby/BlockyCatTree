using System.Xml.Linq;
using BlockyCatTree;

namespace BlockCatTree.Test;

[TestFixture]
public class SvgWriterTests
{
    private const string ExpectedSvgNs = "http://www.w3.org/2000/svg";
    
    private double _scale;
    private MemoryStream _stream;
    private (Point2d Min, Point2d Max) _bounds;
    private SvgWriter _uut;

    [SetUp]
    public void Setup()
    {
        _scale = 10.0;
        _bounds = (new Point2d(5, 5), new Point2d(14, 24));
        _stream = new MemoryStream();
        _uut = new SvgWriter(_stream, _bounds, _scale);
    }

    [TearDown]
    public void Teardown()
    {
        _uut.Dispose();
        _stream.Dispose();
    }

    private void Close()
    {
        _uut.Close();
        _stream.SetLength(_stream.Position);
        _stream.Seek(0L, SeekOrigin.Begin);
    }

    private XDocument CloseAndReadBack()
    {
        Close();
        return XDocument.Load(_stream);
    }

    [Test]
    public void TestEmptyCase()
    {
        var xdoc = CloseAndReadBack();
        var root = xdoc.Root;
        Assert.That(root, Is.Not.Null);
        Assert.That(root.Name, Is.EqualTo(XName.Get("svg", ExpectedSvgNs)));
        // the world has X ranging 5..14, that is 10 cells (not 9!),
        // we want one cell of padding each side, so 12, and we
        // asked for a scale of 10, so 120 screen units
        // NB - XML namespaces are very confusing, are the attributes in the namespace or not? 
        var widthAttribute = root.Attribute(XName.Get("width"));
        Assert.That(widthAttribute, Is.Not.Null);
        Assert.That(double.Parse(widthAttribute.Value), Is.EqualTo(120.0));
        // similarly for Y, but we have 20 cells not 10
        var heightAttribute = root.Attribute(XName.Get("height"));
        Assert.That(heightAttribute, Is.Not.Null);
        Assert.That(double.Parse(heightAttribute.Value), Is.EqualTo(220.0));
        var rootChildren = root.Elements().ToList();
        Assert.That(rootChildren, Has.Count.EqualTo(1), "expected single 'g' element to setup transform");
        var rootChild = rootChildren[0];
        Assert.That(rootChild.Name, Is.EqualTo(XName.Get("g", ExpectedSvgNs)));
        var transformAttribute = rootChild.Attribute(XName.Get("transform"));
        Assert.That(transformAttribute, Is.Not.Null);
        // Somewhat surprisingly, the transform are applied right-to-left
        // (perhaps it makes more sense if you think of them generating nested elements?)
        Assert.That(transformAttribute.Value, Is.EqualTo("scale(10 -10) translate(-4 -26)"));
    }

    [Test]
    public void TestSlice()
    {
        var slice = new Slice<int>();
        slice.Set(new Point2d(5, 5), 1);
        slice.Set(new Point2d(8, 6), 0);
        slice.Set(new Point2d(10, 20), 1);
        _uut.AddSlice(slice, i => i switch
        {
            null => "red",
            0 => "blue",
            1 => "green",
            _ => throw new Exception($"unexpected payload '{i}'")
        });
        var xdoc = CloseAndReadBack();
        // Because all the hard work is done by the SVG transform the mapping is fairly obvious
        var rectangles = xdoc.Descendants().Where(x => x.Name.Equals(XName.Get("rect", ExpectedSvgNs))).ToList();
        // Only expect rectangles for the area covered by the slice bounds (6x16 = 96),
        // even if the ones passed to the writer are bigger
        Assert.That(rectangles, Has.Count.EqualTo(96));
        Assert.That(rectangles.Where(r => r.Attribute(XName.Get("fill"))?.Value == "green").ToList(), Has.Count.EqualTo(2));
        Assert.That(rectangles.Where(r => r.Attribute(XName.Get("fill"))?.Value == "blue").ToList(), Has.Count.EqualTo(1));
        Assert.That(
            rectangles
                .Where(r => 
                    r.Attribute(XName.Get("fill"))?.Value == "blue" &&
                    r.Attribute(XName.Get("x"))?.Value == "8" &&
                    r.Attribute(XName.Get("y"))?.Value == "6")
                .ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    [Explicit]
    public void ExplicitTestExampleToFile()
    {
        var input = new []
        {
            "     #  #",
            "     # o",
            "     #",
            // remember we set our bounds to start at (5,5), hence all the padding
            "",
            "",
            "",
            "",
            ""
        };
        var slice = ArrayToSlice.Make<char,int>(input, c => c switch
        {
            '#' => 1,
            'o' => 0,
            ' ' => null,
            _ => throw new Exception($"unexpected char '{c}'")
        });
        _uut.AddSlice(slice, i => i switch
        {
            null => "red",
            0 => "blue",
            1 => "green",
            _ => throw new Exception($"unexpected payload '{i}'")
        });
        Close();
        using var outStream = File.OpenWrite("E:/example01.svg");
        outStream.SetLength(0);
        _stream.CopyTo(outStream);
        outStream.Close();
    }
}