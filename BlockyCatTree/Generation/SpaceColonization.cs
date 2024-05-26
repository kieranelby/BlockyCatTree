using System.Numerics;
using BlockyCatTree.Pixel;
using BlockyCatTree.Triangulate;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Generation;

public class SpaceColonization : IGenerator
{
    public enum Stage
    {
        CreateAttractors,
        GrowLower,
        GrowUpper,
        Strengthen,
        Clean,
        Upsample,
        Done
    }

    private readonly Settings _settings = new();
    private readonly Random _rng;
    public int StageStartStepNumber { get; private set; }
    public int TotalStepNumber { get; private set; }
    public int StageStepNumber => TotalStepNumber - StageStartStepNumber;
    public Stage CurrentStage { get; private set; } = Stage.CreateAttractors;
    public bool IsDone => CurrentStage == Stage.Done;
    public string StageName => CurrentStage.ToString();
    public Voxels<byte> Voxels { get; private set; } = new();
    public Voxels<byte> InternalVoxels { get; } = new();

    public void DoNextStep()
    {
        TotalStepNumber++;
        if (TotalStepNumber > 5000)
        {
            Console.WriteLine("Warning - reached maximum number of steps");
            StageStartStepNumber = TotalStepNumber;
            CurrentStage = Stage.Done;
        }
        switch (CurrentStage)
        {
            case Stage.CreateAttractors:
                CreateAttractors();
                NextStage();
                break;
            case Stage.GrowLower:
                if (!GrowLower())
                {
                    NextStage();
                }
                break;
            case Stage.GrowUpper:
                if (!GrowUpper())
                {
                    NextStage();
                }
                break;
            case Stage.Strengthen:
                if (!Strengthen())
                {
                    NextStage();
                }
                break;
            case Stage.Clean:
                if (!Clean())
                {
                    NextStage();
                }
                break;
            case Stage.Upsample:
                if (!Upsample())
                {
                    NextStage();
                }
                break;
            case Stage.Done:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void NextStage()
    {
        StageStartStepNumber = TotalStepNumber;
        CurrentStage++;
    }

    public GeneratorSnapshot TakeSnapshot()
    {
        var namedVoxelsList = new List<NamedVoxels>
        {
            new NamedVoxels("Main", Voxels.Clone<byte>(x => x))
        };
        return new GeneratorSnapshot(TotalStepNumber, StageName, StageStepNumber, IsDone, namedVoxelsList, []);
    }

    public record Settings(
        int NumUpperAttractors = 200,
        int UpperRadius = 24,
        int UpperInfluenceRadius = 18,
        int KillRadius = 2,
        int TrunkLength = 2,
        int TrunkSpacing = 3,
        int StartHeight = 6,
        int RootStemLength = 2,
        int NumLowerAttractors = 50,
        int LowerRadius = 15,
        int LowerInfluenceRadius = 20,
        bool Upsample = true,
        bool Strengthen = true,
        float UnitVoxelStaticWeight = 2.0f,
        float UnitVoxelDynamicWeight = 3.0f,
        float WeightLimit = 100.0f
        );

    private class TreeNode(Point3d point3d)
    {
        public Point3d Point3d { get; set; } = point3d;
        public bool Active { get; set; } = true;
        public List<Attractor> Attractors { get; } = [];
    }

    private class Attractor(Point3d point3d)
    {
        public Point3d Point3d { get; set; } = point3d;
        public bool Active { get; set; } = true;
        public bool EverReachable { get; set; }
    }

    private record NodesAndAttractors(List<TreeNode> TreeNodes, List<Attractor> Attractors)
    {
        public NodesAndAttractors() : this([], [])
        {
        }
    }

    private readonly NodesAndAttractors _lowerNodesAndAttractors = new();
    private readonly NodesAndAttractors _upperNodesAndAttractors = new();

    public SpaceColonization(Random rng)
    {
        _rng = rng;
    }

    public void CreateAttractors()
    {
        
        _upperNodesAndAttractors.TreeNodes.Add(new TreeNode(new Point3d(0, 0, _settings.StartHeight)));
        _lowerNodesAndAttractors.TreeNodes.Add(new TreeNode(new Point3d(0, 0, _settings.StartHeight)));

        var upperCenterHeight = _settings.StartHeight + _settings.TrunkLength + _settings.UpperRadius;
        
        // Ellipsoid of upper attractors (TODO - this isn't really a proper ellipsoid)
        var upperCenter = new Point3d(0, 0, upperCenterHeight);
        while (_upperNodesAndAttractors.Attractors.Count < _settings.NumUpperAttractors)
        {
            var randomCuboidPoint = new Point3d(
                _rng.Next(-_settings.UpperRadius, _settings.UpperRadius),
                _rng.Next(-_settings.UpperRadius, _settings.UpperRadius),
                _rng.Next(upperCenterHeight - _settings.UpperRadius, upperCenterHeight + _settings.UpperRadius)
            );
            if (randomCuboidPoint.Minus(upperCenter).Length > _settings.UpperRadius)
            {
                continue;
            }
            AddUpperAttractor(randomCuboidPoint);
        }

        // Slightly cheating trail of upper attractors going up the trunk
        for (var z = _settings.StartHeight + 1; z < upperCenterHeight; z += _settings.TrunkSpacing)
        {
            AddUpperAttractor(new Point3d(0, 0, z));
        }

        var lowerCenterHeight = _settings.StartHeight - - _settings.RootStemLength - _settings.LowerRadius;

        // Ellipsoid of lower attractors (TODO - this isn't really a proper ellipsoid)
        var lowerCenter = new Point3d(0, 0, lowerCenterHeight);
        while (_lowerNodesAndAttractors.Attractors.Count < _settings.NumLowerAttractors)
        {
            var randomCuboidPoint = new Point3d(
                _rng.Next(-_settings.LowerRadius, _settings.LowerRadius),
                _rng.Next(-_settings.LowerRadius, _settings.LowerRadius),
                _rng.Next(lowerCenterHeight - _settings.LowerRadius, lowerCenterHeight + _settings.LowerRadius)
            );
            var distance = randomCuboidPoint.Minus(lowerCenter).Length;
            if (distance > _settings.LowerRadius)
            {
                continue;
            }
            AddLowerAttractor(randomCuboidPoint);
        }
        for (var i = 0; i < _settings.NumLowerAttractors; i++)
        {
            AddLowerAttractor(new Point3d(
                _rng.Next(-_settings.LowerRadius, _settings.LowerRadius),
                _rng.Next(-_settings.LowerRadius, _settings.LowerRadius),
                _rng.Next(lowerCenterHeight - _settings.LowerRadius, lowerCenterHeight + _settings.LowerRadius)
            ));
        }

        // Slightly cheating trail of lower attractors down the root stem
        for (var z = _settings.StartHeight - 1; z > lowerCenterHeight; z -= _settings.TrunkSpacing)
        {
            AddLowerAttractor(new Point3d(0, 0, z));
        }

        return;

        void AddUpperAttractor(Point3d point3d)
        {
            var attractor = new Attractor(point3d);
            _upperNodesAndAttractors.Attractors.Add(attractor);
            InternalVoxels.Set(attractor.Point3d, default);
        }
        void AddLowerAttractor(Point3d point3d)
        {
            var attractor = new Attractor(point3d);
            _lowerNodesAndAttractors.Attractors.Add(attractor);
            InternalVoxels.Set(attractor.Point3d, default);
        }
    }

    public bool GrowUpper()
    {
        var attractors = _upperNodesAndAttractors.Attractors; 
        var treeNodes = _upperNodesAndAttractors.TreeNodes; 
        foreach (var attractor in attractors)
        {
            attractor.EverReachable = false;
            TreeNode? closestTreeNode = null;
            var closestTreeNodeDistance = double.MaxValue;
            foreach (var treeNode in treeNodes)
            {
                if (IsTooClose(treeNode, attractor, _settings.KillRadius))
                {
                    attractor.Active = false;
                    continue;
                }
                if (!IsEverReachableUpper(treeNode, attractor))
                {
                    continue;
                }
                attractor.EverReachable = true;
                if (!IsInsideInfluenceRadius(treeNode, attractor, _settings.UpperInfluenceRadius))
                {
                    continue;
                }
                var distance = MeasureDistance(treeNode, attractor);
                if (distance < closestTreeNodeDistance)
                {
                    closestTreeNodeDistance = distance;
                    closestTreeNode = treeNode;
                }
            }
            if (!attractor.EverReachable)
            {
                attractor.Active = false;
                continue;
            }
            if (closestTreeNode != null)
            {
                closestTreeNode.Attractors.Add(attractor);
            }
        }
        var newTreeNodes = new List<TreeNode>();
        foreach (var treeNode in treeNodes)
        {
            if (treeNode.Attractors.Count == 0)
            {
                // TODO - can we do something to keep them around a bit longer?
                treeNode.Active = false;
                continue;
            }
            var growthDirection = ChooseGrowthDirection(treeNode);
            var growthStep = ChooseBestStepTowards(growthDirection);
            FillUpperNode(treeNode.Point3d);
            newTreeNodes.Add(new TreeNode(treeNode.Point3d.Plus(growthStep)));
            treeNode.Attractors.Clear();
        }
        treeNodes.RemoveAll(t => !t.Active);
        treeNodes.AddRange(newTreeNodes);
        attractors.RemoveAll(a => !a.Active);
        var maxSteps = 3 * (_settings.TrunkLength + _settings.UpperRadius * 2);
        return treeNodes.Count > 0 && attractors.Count > 0 && (TotalStepNumber - StageStartStepNumber) < maxSteps;
    }

    private bool GrowLower()
    {
        var attractors = _lowerNodesAndAttractors.Attractors; 
        var treeNodes = _lowerNodesAndAttractors.TreeNodes; 
        foreach (var attractor in attractors)
        {
            attractor.EverReachable = false;
            TreeNode? closestTreeNode = null;
            var closestTreeNodeDistance = double.MaxValue;
            foreach (var treeNode in treeNodes)
            {
                if (IsTooClose(treeNode, attractor, _settings.KillRadius))
                {
                    attractor.Active = false;
                    continue;
                }
                if (!IsEverReachableLower(treeNode, attractor))
                {
                    continue;
                }
                attractor.EverReachable = true;
                if (!IsInsideInfluenceRadius(treeNode, attractor, _settings.LowerInfluenceRadius))
                {
                    continue;
                }
                var distance = MeasureDistance(treeNode, attractor);
                if (distance < closestTreeNodeDistance)
                {
                    closestTreeNodeDistance = distance;
                    closestTreeNode = treeNode;
                }
            }
            if (!attractor.EverReachable)
            {
                attractor.Active = false;
                continue;
            }
            if (closestTreeNode != null)
            {
                closestTreeNode.Attractors.Add(attractor);
            }
        }
        var newTreeNodes = new List<TreeNode>();
        foreach (var treeNode in treeNodes)
        {
            if (treeNode.Attractors.Count == 0)
            {
                // TODO - can we do something to keep them around a bit longer?
                treeNode.Active = false;
                continue;
            }
            var growthDirection = ChooseGrowthDirection(treeNode);
            var growthStep = ChooseBestStepTowards(growthDirection);
            FillLowerNode(treeNode.Point3d);
            newTreeNodes.Add(new TreeNode(treeNode.Point3d.Plus(growthStep)));
            treeNode.Attractors.Clear();
        }
        treeNodes.RemoveAll(t => !t.Active);
        treeNodes.AddRange(newTreeNodes);
        attractors.RemoveAll(a => !a.Active);
        var maxSteps = 3 * (_settings.StartHeight + _settings.LowerRadius * 2);
        return treeNodes.Count > 0 && attractors.Count > 0 && (TotalStepNumber - StageStartStepNumber) < maxSteps;
    }

    private void FillUpperNode(Point3d point3d)
    {
        Voxels.Set(point3d, default);
    }

    private void FillLowerNode(Point3d point3d)
    {
        for (var z = point3d.Z; z >= 0; z--)
        {
            Voxels.Set(point3d with { Z = z }, default);
        }
    }

    private Voxels<float> ComputeWeights()
    {
        var weightVoxels = new Voxels<float>();
        // Work from top to ground (no need to go underground)
        foreach (var zed in Voxels.GetInclusiveZedBounds().Reverse())
        {
            if (zed.Value < 0)
            {
                break;
            }
            var currentWeightSlice = weightVoxels.GetOrCreateSlice(zed);
            var currentSlice = Voxels.GetOrCreateSlice(zed);
            var weightSliceBelow = weightVoxels.GetOrCreateSlice(zed.Minus(1));
            var sliceBelow = Voxels.GetOrCreateSlice(zed.Minus(1));
            // For each region in the current slice ...
            var outlines = OutlineFinder.FindOutlines(currentSlice).ToList();
            foreach (var outline in outlines)
            {
                var outlineTester = new OutlineInteriorTester(outline);
                // Work out the average weight of the region, including any we added from above in the previous iteration
                var regionCount = outlineTester.IterateRowMajor().Count();
                var regionTotalWeight = outlineTester.IterateRowMajor().Select(p => currentWeightSlice.Get(p) ?? 0.0f).Sum();
                // Don't forget to add in the weight of each voxel - to stop the weight
                // near the bottom becoming far too high, and to sort of model wind loading,
                // we say that voxels in regions with a high area : perimeter ratio weigh less
                // For a 1x1, the perimeter is 4 and the area is 1, so PoA = 4.
                // For a 10x10, the perimeter is 40 and the area is 100, so PoA = 0.4.
                var perimeterOverArea = (float) (outline.Exterior.Points.Count - 1) / regionCount;
                var regionAverageWeight = regionTotalWeight / regionCount +
                                          _settings.UnitVoxelStaticWeight +
                                          _settings.UnitVoxelDynamicWeight * perimeterOverArea;
                foreach (var point2d in outlineTester.IterateRowMajor())
                {
                    // Distribute the weight evenly in the region
                    currentWeightSlice.Set(point2d, regionAverageWeight);
                    if (zed.Value > 0)
                    {
                        // And add it to the layer below, picking one supporting point for each point
                        // (hopefully it will be redistributed reasonably on the next iteration)
                        // There should always be a supporting point - something is wrong with our
                        // generation if not!
                        var supportingPoint =
                            GetAtThenAround(point2d, 1)
                                .First(p => sliceBelow.Exists(p));
                        // isn't the slice below empty? no, we could map multiple points above
                        // to the same one below
                        weightSliceBelow.Set(supportingPoint, regionAverageWeight +
                            (weightSliceBelow.Get(supportingPoint) ?? 0.0f));
                    }
                }
            }
        }
        return weightVoxels;
    }

    private bool Strengthen()
    {
        if (!_settings.Strengthen)
        {
            return false;
        }
        var weightLimit = _settings.WeightLimit;
        var weightVoxels = ComputeWeights();
        var weightLimitExceeded = false;
        var unfixablyHeavy = false;
        foreach (var zed in weightVoxels.GetInclusiveZedBounds())
        {
            if (zed.Value < 0)
            {
                continue;
            }
            var currentWeightSlice = weightVoxels.GetOrCreateSlice(zed);
            var currentSlice = Voxels.GetOrCreateSlice(zed);
            var sliceBelow = Voxels.GetOrCreateSlice(zed.Minus(1));
            var points = new List<Point2d>();
            currentSlice.CopyPointsTo(points);
            foreach (var point2d in points)
            {
                var weight = currentWeightSlice.Get(point2d)!.Value;
                if (weight > weightLimit)
                {
                    weightLimitExceeded = true;
                    var possibleNewPoints =
                        GetAround(point2d, 1)
                            .Where(p =>
                                !currentSlice.Exists(p) &&
                                zed.Value == 0 ||
                                GetAtThenAround(p, 1).Any(q => sliceBelow.Exists(q)))
                            .ToArray();
                    if (possibleNewPoints.Length > 0)
                    {
                        var possibleNewPoint = possibleNewPoints[_rng.Next(possibleNewPoints.Length)];
                        currentSlice.Set(possibleNewPoint, default);
                    }
                    else
                    {
                        unfixablyHeavy = true;
                    }
                }
            }
        }
        var maxSteps = 500;
        return weightLimitExceeded && (TotalStepNumber - StageStartStepNumber) < maxSteps;
    }
    
    private bool Clean()
    {
        foreach (var zed in Voxels.GetInclusiveZedBounds())
        {
            if (zed.Value >= 0)
            {
                break;
            }
            Voxels.SetSlice(zed, new Slice<byte>());
        }
        if (!Voxels.TryGetSlice(new Zed(0), out var bottomSlice))
        {
            throw new Exception("bottom slice missing");
        }
        var outlines = OutlineFinder.FindAllPaths(bottomSlice!, false);
        foreach (var outline in outlines)
        {
            var interiorTester = new PathInteriorTester(outline);
            foreach (var point2d in bottomSlice.GetInclusiveBounds().IterateRowMajor())
            {
                if (interiorTester.Inside(point2d))
                {
                    bottomSlice.Set(point2d, default);
                }
            }
        }
        return false;
    }

    private bool Upsample()
    {
        if (!_settings.Upsample)
        {
            return false;
        }
        Voxels = UpSampler.UpSample(Voxels);
        return false;
    }

    private bool IsEverReachableUpper(TreeNode treeNode, Attractor attractor)
    {
        var vector = attractor.Point3d.Minus(treeNode.Point3d);
        var canReachWithoutOverhang =
            vector.Z > 0 && Math.Abs(vector.X) < vector.Z && Math.Abs(vector.Y) < vector.Z;
        return canReachWithoutOverhang;
    }

    private bool IsEverReachableLower(TreeNode treeNode, Attractor attractor)
    {
        var vector = attractor.Point3d.Minus(treeNode.Point3d);
        return treeNode.Point3d.Z >= 0 && vector.Z < 0-(Math.Abs(vector.X) + Math.Abs(vector.Y))/4;
    }

    private bool IsInsideInfluenceRadius(TreeNode treeNode, Attractor attractor, int influenceDistance) =>
        MeasureDistance(treeNode, attractor) < influenceDistance + 1e-10;

    private bool IsTooClose(TreeNode treeNode, Attractor attractor, int killRadius) =>
        MeasureDistance(treeNode, attractor) < killRadius + 1e-10;

    private float MeasureDistance(TreeNode treeNode, Attractor attractor) =>
        attractor.Point3d.Minus(treeNode.Point3d).Length;

    private Vector3 ChooseGrowthDirection(TreeNode treeNode)
    {
        if (treeNode.Attractors.Count == 0)
        {
            throw new Exception("no attractors");
        }

        var growthVector = treeNode.Attractors
            .Select(a => a.Point3d.Minus(treeNode.Point3d).AsVector3)
            .Select(Vector3.Normalize)
            .Aggregate((va, vb) => va + vb);
        return Vector3.Normalize(growthVector);
    }

    private IEnumerable<Point2d> GetAtThenAround(Point2d point2d, int expandBy)
    {
        yield return point2d;
        for (var y = -expandBy; y <= +expandBy; y++)
        {
            for (var x = -expandBy; x <= +expandBy; x++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }
                yield return point2d.Plus(new Point2d(x, y));
            }
        }
    }

    private IEnumerable<Point2d> GetAround(Point2d point2d, int expandBy)
    {
        for (var y = -expandBy; y <= +expandBy; y++)
        {
            for (var x = -expandBy; x <= +expandBy; x++)
            {
                if (y == 0 && x == 0)
                {
                    continue;
                }
                yield return point2d.Plus(new Point2d(x, y));
            }
        }
    }

    private Point3d ChooseBestStepTowards(Vector3 vector3)
    {
        var bestStep = Point3d.Origin;
        var bestDistance = float.MaxValue;
        for (var z = -1; z <= +1; z++)
        {
            for (var y = -1; y <= +1; y++)
            {
                for (var x = -1; x <= +1; x++)
                {
                    var step = new Point3d(x, y, z);
                    var distance = (vector3 - step.AsVector3).Length();
                    if (distance >= bestDistance)
                    {
                        continue;
                    }
                    bestDistance = distance;
                    bestStep = step;
                }
            }
        }
        return bestStep;
    }
}
