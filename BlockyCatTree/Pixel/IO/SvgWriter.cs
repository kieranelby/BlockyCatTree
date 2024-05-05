using System.Text;
using System.Xml;

namespace BlockyCatTree.Pixel.IO;

/// <summary>
/// Helps turn the contents of 2d elements like slices and paths to an SVG file,
/// mostly to help debug problems graphically.
/// </summary>
public class SvgWriter : IDisposable
{
    private const string SvgNs = "http://www.w3.org/2000/svg";

    private readonly XmlWriter _xmlWriter;
    private bool _closed;

    public SvgWriter(string outputFilename, (Point2d Min, Point2d Max) worldBounds, double scale)
        : this(s => XmlWriter.Create(outputFilename, s), worldBounds, scale) {}

    public SvgWriter(Stream outputStream, (Point2d Min, Point2d Max) worldBounds, double scale)
        : this(s => XmlWriter.Create(outputStream, s), worldBounds, scale) {}

    private SvgWriter(Func<XmlWriterSettings,XmlWriter> xmlWriterFactory, (Point2d Min, Point2d Max) worldBounds, double scale)
    {
        var settings = new XmlWriterSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            OmitXmlDeclaration = true,
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Encoding = Encoding.UTF8,
            Indent = true
        };
        _xmlWriter = xmlWriterFactory(settings);
        //_xmlWriter.WriteStartDocument();
        _xmlWriter.WriteStartElement("svg", SvgNs);
        // e.g. if X ranges from 5..14, 14 - 5 is 9, but a cell has
        // width 1, so really 10 - and we want one more either side
        var unitCell = new Point2d(1, 1);
        var worldPadding = new Point2d(2, 2);
        var worldSize =
            worldBounds.Max
                .Minus(worldBounds.Min)
                .Plus(unitCell)
                .Plus(worldPadding);
        _xmlWriter.WriteAttributeString("width", $"{worldSize.X * scale}");
        _xmlWriter.WriteAttributeString("height", $"{worldSize.Y * scale}");
        _xmlWriter.WriteStartElement("g");
        var xOff = -(worldBounds.Min.X - 1);
        var yOff = -(worldBounds.Max.Y + 2); // needs an extra one due to, umm, the origin being at top not bottom?
        // Somewhat surprisingly, the transform are applied right-to-left
        // (perhaps it makes more sense if you think of them generating nested elements?)
        _xmlWriter.WriteAttributeString("transform", $"scale({scale} {-scale}) translate({xOff} {yOff})");
    }

    public void AddSlice<TPayload>(Slice<TPayload> slice, Func<TPayload?,string> fillFunc) where TPayload : struct
    {
        var bounds = slice.GetInclusiveBounds();
        for (var y = bounds.Min.Y; y <= bounds.Max.Y; y++)
        {
            for (var x = bounds.Min.X; x <= bounds.Max.X; x++)
            {
                var maybePayload = slice.Get(new Point2d(x, y));
                var fill = fillFunc(maybePayload);
                _xmlWriter.WriteStartElement("rect");
                _xmlWriter.WriteAttributeString("x",$"{x}");
                _xmlWriter.WriteAttributeString("y",$"{y}");
                _xmlWriter.WriteAttributeString("width", "1");
                _xmlWriter.WriteAttributeString("height", "1");
                _xmlWriter.WriteAttributeString("fill", fill);
                _xmlWriter.WriteEndElement(); // rect 
            }
        }
    }

    public void Close()
    {
        if (_closed)
        {
            return;
        }
        _xmlWriter.WriteEndElement(); // g
        _xmlWriter.WriteEndElement(); // svg
        _xmlWriter.Close();
        _xmlWriter.Dispose();
        _closed = true;
    }

    public void Dispose()
    {
        Close();
    }
}
