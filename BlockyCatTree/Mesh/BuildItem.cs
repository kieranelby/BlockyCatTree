using System.Numerics;

namespace BlockyCatTree.Mesh;

/// <summary>
/// Similar to build&gt;item as used in 3MF, a solid shape to build.
/// The <see cref="ObjectId"/> refers to a <see cref="Solid"/>.
/// </summary>
public record BuildItem(ObjectId ObjectId, Matrix4x4 Transform);
