using System.IO.Compression;
using System.Numerics;
using System.Xml;

namespace BlockyCatTree.Mesh.IO;

/// <summary>
/// Knows how to read very simple 3MF files containing a single model object and build item.
/// </summary>
public static class BasicThreeEmEffReader
{
    public static Model Read(string inputFilepath)
    {
        using var zipFile = ZipFile.OpenRead(inputFilepath);
        var entry = zipFile.GetEntry("3D/3dmodel.model");
        if (entry == null)
        {
            throw new Exception($"could not find model entry");
        }
        using var stream = entry.Open();
        
        var doc = new XmlDocument();
        doc.Load(stream);
        var docNamespaceManager = new XmlNamespaceManager(doc.NameTable);        
        docNamespaceManager.AddNamespace("ns", """http://schemas.microsoft.com/3dmanufacturing/core/2015/02""");
        var buildItemNode = doc.DocumentElement!.SelectSingleNode("/ns:model/ns:build/ns:item", docNamespaceManager);
        if (buildItemNode == null)
        {
            throw new Exception($"could not find single build item");
        }
        var objectIdStr = buildItemNode.Attributes!["objectid"]!.Value;
        var objectId = new ObjectId(objectIdStr);
        var transformAttribute = buildItemNode.Attributes?["transform"];
        var transform = transformAttribute != null
            ? ParseTransformString(transformAttribute.Value)
            : Matrix4x4.Identity;
        var objectNode = doc.DocumentElement!.SelectSingleNode($"/ns:model/ns:resources/ns:object[@id={objectIdStr}]", docNamespaceManager);
        if (objectNode == null)
        {
            throw new Exception($"Could not find object with id {objectIdStr}");
        }
        var vertexNodes = objectNode.SelectNodes("ns:mesh/ns:vertices/ns:vertex", docNamespaceManager);
        var triangleNodes = objectNode.SelectNodes("ns:mesh/ns:triangles/ns:triangle", docNamespaceManager);
        var vertices = vertexNodes!.Cast<XmlNode>().Select(vn => new Vertex(
            double.Parse(vn!.Attributes!["x"]!.Value),
            double.Parse(vn!.Attributes!["y"]!.Value),
            double.Parse(vn!.Attributes!["z"]!.Value))).ToList();
        // don't bother with the p properties (material/colour)
        var triangles = triangleNodes!.Cast<XmlNode>().Select(vn => new Triangle(
            int.Parse(vn!.Attributes!["v1"]!.Value),
            int.Parse(vn!.Attributes!["v2"]!.Value),
            int.Parse(vn!.Attributes!["v3"]!.Value))).ToList();
        var solid = new Solid(objectId, vertices, triangles);
        var buildItem = new BuildItem(objectId, transform);
        return new Model(new List<Solid> { solid }, new List<BuildItem>{ buildItem });
    }

    private static Matrix4x4 ParseTransformString(string transform)
    {
        var tps = transform.Split(' ').Select(float.Parse).ToArray();
        return new Matrix4x4(
            tps[0], tps[1], tps[2], 0.0f,
            tps[3], tps[4], tps[5], 0.0f,
            tps[6], tps[7], tps[8], 0.0f,
            tps[9],tps[10],tps[11], 1.0f
        );
    }
}