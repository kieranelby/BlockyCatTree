// See https://aka.ms/new-console-template for more information

using System.Numerics;
using BlockyCatTree.Generation;
using BlockyCatTree.Mesh;
using BlockyCatTree.Mesh.IO;
using BlockyCatTree.Triangulate;

Console.WriteLine("Hello, World!");

var generator = new SpaceColonization(new Random(12345));
generator.RunToEnd();
var objectId = ObjectId.FirstId;
var solid = VoxelsToSolid.Triangulate(objectId, generator.Voxels);
var transform = Matrix4x4.CreateTranslation(128.0f, 128.0f, 0.0f);
var buildItem = new BuildItem(objectId, transform);
var model = new Model([solid], [buildItem]);
BasicThreeEmEffWriter.Write("E:/example06.3mf", model);
