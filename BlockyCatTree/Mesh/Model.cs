namespace BlockyCatTree.Mesh;

/// <summary>
/// Similar to the root "model" used in 3MF, one or more items to build,
/// each of which consist of a solid and some transformation. 
/// </summary>
public record Model(List<Solid> Solids, List<BuildItem> BuildItems);
