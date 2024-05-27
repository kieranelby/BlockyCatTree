using System.Globalization;
using System.IO.Compression;
using System.Numerics;
using System.Text;
using System.Xml;

namespace BlockyCatTree.Mesh.IO;

/// <summary>
/// Knows how to write simple 3MF files that can be read by:
///  - ourselves
///  - our own UI
///  - Bambu Studio
///  - online 3d viewer thingy
///  - Blender plugin
/// There are some slightly unusual things we do:
///  - vendor metadata describing the generation state (for progress snapshots)
///  - [planned] use non-built objects to store voxels
/// </summary>
public static class BasicThreeEmEffWriter
{
    public static void Write(string outputFilepath, Model model)
    {
        var baseTempPath = Path.GetTempPath();
        var tempPath = Path.Join(baseTempPath, Path.GetRandomFileName());
        var tempModelsPath = Path.Join(tempPath, "3D");
        Directory.CreateDirectory(tempModelsPath);
        var xmlFilePath = Path.Join(tempModelsPath, "3dmodel.model");
        var settings = new XmlWriterSettings
        {
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Encoding = Encoding.UTF8,
            Indent = true
        };
        using (var writer = XmlWriter.Create(xmlFilePath, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("model", "http://schemas.microsoft.com/3dmanufacturing/core/2015/02");
            writer.WriteAttributeString("unit", "millimeter");
            writer.WriteAttributeString("xml", "lang", "http://www.w3.org/XML/1998/namespace", "en-US");
            var prefixesAdded = new HashSet<string>();
            foreach (var name in model.Metadata.Keys)
            {
                if (!name.Contains(':'))
                {
                    continue;
                }
                var namespacePrefix = name.Split(':', 2)[0];
                if (prefixesAdded.Contains(namespacePrefix))
                {
                    continue;
                }
                writer.WriteAttributeString("xmlns", namespacePrefix, null, $"http://example.com/{namespacePrefix}");
                prefixesAdded.Add(namespacePrefix);
            }
            foreach (var (name, value) in model.Metadata)
            {
                writer.WriteStartElement("metadata");
                writer.WriteAttributeString("name", name);
                writer.WriteString(value);
                writer.WriteEndElement(); // metadata
            }
            writer.WriteStartElement("resources");
            foreach (var solid in model.Solids)
            {
                WriteSolid(writer, solid);
            }
            writer.WriteEndElement(); // resources
            writer.WriteStartElement("build");
            foreach (var buildItem in model.BuildItems)
            {
                writer.WriteStartElement("item");
                writer.WriteAttributeString("objectid", buildItem.ObjectId.AsString);
                writer.WriteAttributeString("transform", MakeTransformString(buildItem.Transform));
                writer.WriteEndElement(); // item
            }
            writer.WriteEndElement(); // build
            writer.WriteEndElement(); // model
            writer.WriteEndDocument();
        }
        var contentTypesPath = Path.Join(tempPath, "[Content_Types].xml");
        var contentTypesText =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\" /><Default Extension=\"model\" ContentType=\"application/vnd.ms-package.3dmanufacturing-3dmodel+xml\" /></Types>";
        File.WriteAllText(contentTypesPath, contentTypesText, Encoding.UTF8);
        var relsPath = Path.Join(tempPath, "_rels");
        Directory.CreateDirectory(relsPath);
        var relsRelsPath = Path.Join(relsPath, ".rels");
        var relsRelsText =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Target=\"/3D/3dmodel.model\" Id=\"rel0\" Type=\"http://schemas.microsoft.com/3dmanufacturing/2013/01/3dmodel\" /></Relationships>";
        File.WriteAllText(relsRelsPath, relsRelsText, Encoding.UTF8);
        File.Delete("tree.3mf");
        File.Delete(outputFilepath);
        ZipFile.CreateFromDirectory(tempPath, outputFilepath);
    }

    private static void WriteSolid(XmlWriter writer, Solid solid)
    {
        writer.WriteStartElement("object");
        writer.WriteAttributeString("id", solid.ObjectId.AsString);
        writer.WriteAttributeString("type", "model");
        writer.WriteStartElement("mesh");
        writer.WriteStartElement("vertices");
        foreach (var vertex in solid.Vertices)
        {
            writer.WriteStartElement("vertex");
            writer.WriteAttributeString("x", vertex.X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", vertex.Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", vertex.Z.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // vertex
        }
        writer.WriteEndElement(); // vertices
        writer.WriteStartElement("triangles");
        foreach (var triangle in solid.Triangles)
        {
            writer.WriteStartElement("triangle");
            writer.WriteAttributeString("v1", triangle.V1.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("v2", triangle.V2.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("v3", triangle.V3.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement(); // vertex
        }
        writer.WriteEndElement(); // triangles
        writer.WriteEndElement(); // mesh
        writer.WriteEndElement(); // object
    }

    private static string MakeTransformString(Matrix4x4 transform)
    {
        var components = new []
        {
            transform.M11,
            transform.M12,
            transform.M13,
            // assumed to be 0.0
            transform.M21,
            transform.M22,
            transform.M23,
            // assumed to be 0.0
            transform.M31,
            transform.M32,
            transform.M33,
            // assumed to be 0.0
            transform.M41,
            transform.M42,
            transform.M43,
            // assumed to be 1.0
        };
        return string.Join(' ', components.Select(c => c.ToString(CultureInfo.InvariantCulture)));
    }
}
