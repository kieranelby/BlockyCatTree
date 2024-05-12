using BlockyCatTree.Triangulate;
using BlockyCatTree.Voxel;

namespace BlockyCatTree.Generation;

public static class UpSampler
{
    private readonly record struct Rule(Point3d A, Point3d B)
    {
        public Rule RotateAroundZed()
        {
            return new Rule(
                A.RotateAroundZed(RotationDirection.Clockwise),
                B.RotateAroundZed(RotationDirection.Clockwise));
        }
        public Rule FlipAroundY()
        {
            return new Rule(
                A.FlipAroundY(),
                B.FlipAroundY());
        }
    }

    private static List<Rule> _evenRules = MakeRules([
        new Rule(new Point3d(0, 0, 0), new Point3d(0, 0, 0)), // simple upscale
        new Rule(new Point3d(1, 0, 0), new Point3d(0, 1, 0)), // diagonal same level
        new Rule(new Point3d(0,0,-1), new Point3d(0,1,0)),
        new Rule(new Point3d(0, 0, -1), new Point3d(1, 1, 0)),
        new Rule(new Point3d(1, 0, -1), new Point3d(0, 1, 0)),
    ]).ToList();

    private static List<Rule> _oddRules = MakeRules([
        new Rule(new Point3d(0, 0, 0), new Point3d(0, 0, 0)), // simple upscale
        new Rule(new Point3d(1, 0, 0), new Point3d(0, 1, 0)), // diagonal same level
        new Rule(new Point3d(0,0,+1), new Point3d(1,0,0)),
        new Rule(new Point3d(1, 0, +1), new Point3d(0, 1, 0)),
    ]).ToList();

    private static IEnumerable<Rule> MakeRules(List<Rule> baseRules)
    {
        foreach (var baseRule in baseRules)
        {
            var rule = baseRule;
            var flip = rule.FlipAroundY();
            for (var i = 0; i < 4; i++)
            {
                yield return rule;
                yield return flip;
                rule = rule.RotateAroundZed();
                flip = flip.RotateAroundZed();
            }
        }
    }

    public static Voxels<T> UpSample<T>(Voxels<T> source) where T : struct
    {
        var target = new Voxels<T>();
        var targetBounds = source.GetInclusiveBounds().Expand(1,1,0).Scale(2);
        foreach (var targetPoint3d in targetBounds.Iterate())
        {
            var rules = (targetPoint3d.Z % 2 == 0) ? _evenRules : _oddRules;
            if (rules.Exists(r => Evaluate(targetPoint3d, r)))
            {
                target.Set(targetPoint3d, default);
            }
        }
        return target;

        bool Evaluate(Point3d targetPoint3d, Rule rule)
        {
            return TargetExistsInSource(targetPoint3d.Plus(rule.A)) &&
                   TargetExistsInSource(targetPoint3d.Plus(rule.B));
        }
        bool TargetExistsInSource(Point3d targetPoint3d)
        {
            return source.Exists(targetPoint3d.ScaleDown(2));
        }
    }
}
