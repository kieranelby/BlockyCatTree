using System.Numerics;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;
using BlockyCatTree.Triangulate;

namespace BlockyCatTree.Generation.IO;

public class SnapshotWriter
{
    private readonly string _targetDirectory;
    private readonly Dictionary<string, ObjectId> _externalBuildItemCache;

    public SnapshotWriter(string targetDirectory)
    {
        _targetDirectory = targetDirectory;
        _externalBuildItemCache = new();
    }

    public void CleanTarget()
    {
        foreach (var filepath in Directory.EnumerateFiles(_targetDirectory, "*.3mf"))
        {
            File.Delete(filepath);
        }
    }
    
    public void Write(GeneratorSnapshot snapshot)
    {
        var nextObjectId = ObjectId.FirstId;
        var solids = new List<Solid>();
        var buildItems = new List<BuildItem>();
        var basePlateCenterTransform = Matrix4x4.CreateTranslation(128.0f, 128.0f, 0.0f);
        foreach (var namedVoxels in snapshot.NamedVoxelsList)
        {
            var objectId = nextObjectId;
            nextObjectId = nextObjectId.Next;
            var solid = VoxelsToSolid.Triangulate(objectId, namedVoxels.Voxels);
            var buildItem = new BuildItem(objectId, basePlateCenterTransform);
            solids.Add(solid);
            buildItems.Add(buildItem);
        }
        foreach (var externalBuildItem in snapshot.ExternalBuildItemList)
        {
            if (!_externalBuildItemCache.TryGetValue(externalBuildItem.SourceFilename, out var objectId))
            {
                objectId = nextObjectId;
                nextObjectId = nextObjectId.Next;
                var solid = LoadExternalSolid(externalBuildItem.SourceFilename);
                solids.Add(solid);
                _externalBuildItemCache[externalBuildItem.SourceFilename] = objectId;
            }
            var buildItem = new BuildItem(objectId, externalBuildItem.Transform * basePlateCenterTransform);
            buildItems.Add(buildItem);
        }
        var model = new Model(solids, buildItems, new Dictionary<string, string>
        {
            {"Title","BlockyCatTree"},
            {"Application","BlockyCatTree"},
            {"BlockyCatTree:TotalStepNumber",snapshot.TotalStepNumber.ToString()},
            {"BlockyCatTree:StageStepNumber",snapshot.StageStepNumber.ToString()},
            {"BlockyCatTree:Stage",snapshot.StageName},
        });
        var outputFilePath = $"{_targetDirectory}/tree-{snapshot.TotalStepNumber}-{snapshot.StageName}-{snapshot.StageStepNumber}.3mf";
        BasicThreeEmEffWriter.Write(outputFilePath, model);
    }

    private Solid LoadExternalSolid(string sourceFilename)
    {
        throw new NotImplementedException();
    }
}
