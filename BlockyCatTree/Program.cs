// See https://aka.ms/new-console-template for more information

using BlockyCatTree.Generation;
using BlockyCatTree.Generation.IO;

Console.WriteLine("BlockyCatTree starting");
var generator = new SpaceColonization(new Random(12345));
var snapshotWriter = new SnapshotWriter("E:/tree");
snapshotWriter.CleanTarget();
var runner = new Runner(generator, snapshotWriter, false);
runner.Run();
Console.WriteLine("BlockyCatTree finished");
