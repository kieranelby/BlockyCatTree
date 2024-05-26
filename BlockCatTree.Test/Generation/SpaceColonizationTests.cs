using BlockyCatTree.Generation;
using BlockyCatTree.Generation.IO;

namespace BlockCatTree.Test.Generation;

[TestFixture]
public class SpaceColonizationTests
{
    private SpaceColonization _uut;
    private Runner _runner;

    [SetUp]
    public void Setup()
    {
        _uut = new SpaceColonization(new Random(12345));
        _runner = new Runner(_uut, new SnapshotWriter("E:/tree"), true);
    }

    [Test]
    [Explicit]
    public void ExplicitTestExampleToFile()
    {
        _runner.Run();
    }
}
