namespace BlockyCatTree.Mesh;

/// <summary>
/// Similar to "object type=model" used in 3MF, a mesh describing a solid item with an identifier. 
/// </summary>
public record Solid(ObjectId ObjectId, List<Vertex> Vertices, List<Triangle> Triangles);
