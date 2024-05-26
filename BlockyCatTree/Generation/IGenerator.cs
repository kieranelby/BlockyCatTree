using System.Numerics;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Generation;

public interface IGenerator
{
    int TotalStepNumber { get; }
    string StageName { get; }
    int StageStepNumber { get; }
    bool IsDone { get; }
    void DoNextStep();
    GeneratorSnapshot TakeSnapshot();
}

public record NamedVoxels(string Name, IReadOnlyBooleanVoxels Voxels)
{
}

public record ExternalBuildItem(string SourceFilename, Matrix4x4 Transform)
{
}

public record GeneratorSnapshot(
    int TotalStepNumber,
    string StageName,
    int StageStepNumber,
    bool IsDone,
    List<NamedVoxels> NamedVoxelsList,
    List<ExternalBuildItem> ExternalBuildItemList)
{
}
