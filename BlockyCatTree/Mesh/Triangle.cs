namespace BlockyCatTree.Mesh;

/// <summary>
/// The vertices should be listed in counter-clock-wise order from outside ("right-hand rule").
/// The properties are indices into <see cref="Solid.Vertices"/>. 
/// </summary>
public readonly record struct Triangle(int V1, int V2, int V3);
