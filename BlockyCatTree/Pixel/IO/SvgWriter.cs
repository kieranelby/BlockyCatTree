using System.Text;
using System.Xml;
using BlockyCatTree.Triangulate;

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

    private readonly record struct FractionalPoint(double X, double Y)
    {
        public FractionalPoint Plus(FractionalPoint other) => new(X + other.X, Y + other.Y);
        public static FractionalPoint Make(Point2d point2d) => new(point2d.X, point2d.Y);
        public static FractionalPoint Make(Point2d point2d, double vectorScale) =>
            new(point2d.X * vectorScale, point2d.Y * vectorScale);
    }

    public SvgWriter(string outputFilename, Bounds2d worldBounds, double scale)
        : this(s => XmlWriter.Create(outputFilename, s), worldBounds, scale) {}

    public SvgWriter(Stream outputStream, Bounds2d worldBounds, double scale)
        : this(s => XmlWriter.Create(outputStream, s), worldBounds, scale) {}

    private SvgWriter(Func<XmlWriterSettings,XmlWriter> xmlWriterFactory, Bounds2d worldBounds, double scale)
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
        foreach (var point2d in bounds.IterateRowMajor())
        {
            var maybePayload = slice.Get(point2d);
            var fill = fillFunc(maybePayload);
            _xmlWriter.WriteStartElement("rect");
            _xmlWriter.WriteAttributeString("x",$"{point2d.X}");
            _xmlWriter.WriteAttributeString("y",$"{point2d.Y}");
            _xmlWriter.WriteAttributeString("width", "1");
            _xmlWriter.WriteAttributeString("height", "1");
            _xmlWriter.WriteAttributeString("fill", fill);
            _xmlWriter.WriteEndElement(); // rect 
        }
    }

    public void AddPath(Path2d path2d)
    {
        var points = path2d.Points;
        var rotationDirection = RotationDirection.Clockwise;
        if (points.Count < 2)
        {
            return;
        }
        var previousPoint = points.First();
        foreach (var point in points.Skip(1))
        {
            _xmlWriter.WriteStartElement("line");
            _xmlWriter.WriteAttributeString("x1",$"{previousPoint.X}");
            _xmlWriter.WriteAttributeString("y1",$"{previousPoint.Y}");
            _xmlWriter.WriteAttributeString("x2",$"{point.X}");
            _xmlWriter.WriteAttributeString("y2",$"{point.Y}");
            _xmlWriter.WriteAttributeString("stroke", "black");
            _xmlWriter.WriteAttributeString("stroke-width", "0.2");
            _xmlWriter.WriteEndElement(); // line

            var vector = point.Minus(previousPoint);
            var polygonPoints = new []
                {
                    FractionalPoint.Make(previousPoint)
                        .Plus(FractionalPoint.Make(vector, 0.3)),
                    FractionalPoint.Make(previousPoint)
                        .Plus(FractionalPoint.Make(vector, 0.3))
                        .Plus(FractionalPoint.Make(vector.Rotate(rotationDirection.Opposite()), 0.3)),
                    FractionalPoint.Make(previousPoint)
                        .Plus(FractionalPoint.Make(vector, 0.8)),
                };
            var polygonPointsStr = string.Join(' ', polygonPoints.Select(p => $"{p.X},{p.Y}"));

            _xmlWriter.WriteStartElement("polygon");
            _xmlWriter.WriteAttributeString("points",polygonPointsStr);
            _xmlWriter.WriteAttributeString("fill", "black");
            _xmlWriter.WriteEndElement(); // polygon

            previousPoint = point;
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
