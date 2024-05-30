using System.Numerics;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;
using BlockyCatTree.Triangulate;

namespace BlockyCatTree.Generation.IO;

public class SnapshotWriter
{
    private readonly string _targetDirectory;
    private readonly Dictionary<string, Model> _externalBuildItemCache;

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
        var alreadyAddedExternalSolids = new Dictionary<string, ObjectId>();  
        foreach (var externalBuildItem in snapshot.ExternalBuildItemList)
        {
            var externalModel = LoadExternalModel(externalBuildItem.SourceFilename);
            if (!alreadyAddedExternalSolids.TryGetValue(externalBuildItem.SourceFilename, out var solidObjectId))
            {
                solidObjectId = nextObjectId;
                nextObjectId = nextObjectId.Next;
                var solid = externalModel.Solids[0] with { ObjectId = solidObjectId};
                solids.Add(solid);
                alreadyAddedExternalSolids[externalBuildItem.SourceFilename] = solidObjectId;
            }
            var buildItem = new BuildItem(solidObjectId, externalModel.BuildItems[0].Transform * externalBuildItem.Transform * basePlateCenterTransform);
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
        var outputFilePath = $"{_targetDirectory}/tree-{snapshot.TotalStepNumber:D5}-{snapshot.StageName}-{snapshot.StageStepNumber:D4}.3mf";
        BasicThreeEmEffWriter.Write(outputFilePath, model);
    }

    private Model LoadExternalModel(string sourceFilename)
    {
        if (!_externalBuildItemCache.TryGetValue(sourceFilename, out var model))
        {
            model = BasicThreeEmEffReader.Read(sourceFilename);
            _externalBuildItemCache[sourceFilename] = model;
        }
        return model;
    }
}
