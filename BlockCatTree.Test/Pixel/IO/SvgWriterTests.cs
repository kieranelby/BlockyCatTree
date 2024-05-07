using System.Xml.Linq;
using BlockyCatTree.Pixel;
using BlockyCatTree.Pixel.IO;
using BlockyCatTree.Triangulate;

namespace BlockCatTree.Test.Pixel.IO;

[TestFixture]
public class SvgWriterTests
{
    private const string ExpectedSvgNs = "http://www.w3.org/2000/svg";
    
    private double _scale;
    private MemoryStream _stream;
    private SvgWriter? _uut;

    [SetUp]
    public void Setup()
    {
        _scale = 10.0;
        _stream = new MemoryStream();
    }

    [TearDown]
    public void Teardown()
    {
        _uut?.Dispose();
        _stream.Dispose();
    }

    private void Close()
    {
        _uut?.Close();
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
        var bounds = new Bounds2d(new Point2d(5, 5), new Point2d(14, 24));
        _uut = new SvgWriter(_stream, bounds, _scale);
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
        // Check that it uses the bounds we specify, not the slice's
        var bounds = new Bounds2d(new Point2d(5, 5), new Point2d(14, 24));
        _uut = new SvgWriter(_stream, bounds, _scale);
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
    public void TestPath()
    {
        var bounds = new Bounds2d(new Point2d(1, 1), new Point2d(4, 4));
        _uut = new SvgWriter(_stream, bounds, _scale);
        var path2d = new Path2d(
        [
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
            new(2, 1)
        ], RotationDirection.CounterClockwise);
        _uut.AddPath(path2d);
        var xdoc = CloseAndReadBack();
        // Because all the hard work is done by the SVG transform the mapping is fairly obvious
        var lines = xdoc.Descendants().Where(x => x.Name.Equals(XName.Get("line", ExpectedSvgNs))).ToList();
        Assert.That(lines, Has.Count.EqualTo(12));
        Assert.That(
            lines
                .Where(r => 
                    r.Attribute(XName.Get("stroke"))?.Value == "black" &&
                    r.Attribute(XName.Get("x1"))?.Value == "3" &&
                    r.Attribute(XName.Get("y1"))?.Value == "1" &&
                    r.Attribute(XName.Get("x2"))?.Value == "3" &&
                    r.Attribute(XName.Get("y2"))?.Value == "2")
                .ToList(), Has.Count.EqualTo(1));
    }

    [Test]
    [Explicit]
    public void ExplicitTestSliceExampleToFile()
    {
        var input = new []
        {
            "#  #",
            "# o",
            "#",
        };
        var slice = ArrayToSlice.Make<char,int>(input, c => c switch
        {
            '#' => 1,
            'o' => 0,
            ' ' => null,
            _ => throw new Exception($"unexpected char '{c}'")
        });
        var bounds = slice.GetInclusiveBounds();
        _uut = new SvgWriter(_stream, bounds, _scale);
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

    [Test]
    [Explicit]
    public void ExplicitTestSliceAndTrickySingleOutlineExampleToFile()
    {
        var input = new []
        {
            //123456789
            "#   # #   ", // 4
            " ## ##### ", // 3
            " #### ##  ", // 2
            "  ##  ### ", // 1
            " ####  #  ", // 0
            //123456789
        };
        var slice = ArrayToSlice.Make<char,int>(input, c => c switch
        {
            '#' => 1,
            ' ' => null,
            _ => throw new Exception($"unexpected char '{c}'")
        });
        var bounds = slice.GetInclusiveBounds();
        _uut = new SvgWriter(_stream, bounds, _scale);
        _uut.AddSlice(slice, i => i switch
        {
            null => "red",
            1 => "green",
            _ => throw new Exception($"unexpected payload '{i}'")
        });
        var outline = OutlineFinder.FindOutline(slice, RotationDirection.Clockwise);
        Assume.That(outline, Is.Not.Null);
        _uut.AddPath(outline!);
        Close();
        using var outStream = File.OpenWrite("E:/example02.svg");
        outStream.SetLength(0);
        _stream.CopyTo(outStream);
        outStream.Close();
    }
    
    [Test]
    [Explicit]
    public void ExplicitTestSliceAndComplexMultipleOutlinesExampleToFile()
    {
        var input = new []
        {
            //12345678901
            "##   X X X  ",  // 3
            "#  XXXXXXXX ",  // 2
            "  XX      XX",  // 1
            " XX   WW  XX",  // 0
            " XX  WWWW XX",  // 9
            " XX       XX",  // 8
            "XXX  MMMM  XX", // 7
            "XXX  M   M X ", // 6
            "XXX  M 8 M X ", // 5
            "XXX  M   M X ", // 4
            "XXX  MMMM  XX", // 3
            "XXX  MM   XX ", // 2
            " XX     XXX",   // 1
            "  XXXXXXXX ",   // 0
            //12345678901
        };
        var slice = ArrayToSlice.Make<char,char>(input, c => c == ' ' ? null : c);
        var bounds = slice.GetInclusiveBounds();
        _uut = new SvgWriter(_stream, bounds, _scale);
        _uut.AddSlice(slice, c => c switch
        {
            null => "gray",
            '#' => "green",
            'X' => "red",
            'M' => "blue",
            '8' => "yellow",
            'W' => "cyan",
            _ => throw new Exception($"unexpected payload '{c}'")
        });
        var outlines = OutlineFinder.FindOutlines(slice);
        foreach (var outline in outlines)
        {
            _uut.AddPath(outline);
        }
        Close();
        using var outStream = File.OpenWrite("E:/example03.svg");
        outStream.SetLength(0);
        _stream.CopyTo(outStream);
        outStream.Close();
    }
}