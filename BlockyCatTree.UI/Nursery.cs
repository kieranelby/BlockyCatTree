using BlockyCatTree.Mesh;
using BlockyCatTree.Triangulate;
using BlockyCatTree.Voxel.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BlockyCatTree.UI;

public class Nursery : GameWindow
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public Nursery() :
        base(
            GameWindowSettings.Default,
            new NativeWindowSettings
                { ClientSize = (800, 600), Title = "Blocky Cat Tree Nursery" }
        ) { }

    private Shader? _shader;

    private float[] _vertices;

    private uint[] _indices;    
    
    private int _vertexBufferObject;
    private int _vertexArrayObject;
    private int _elementBufferObject;
    private double _time;

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape))
        {
            Close();
        }
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        GL.Enable(EnableCap.DepthTest);
        
        var input = new []
        {
            // as well as multiple outer regions, this has multiple nested regions
            new []
            {
                //123456789012345
                "                ",// 6
                " ##   X  X X    ",// 5
                " #  XXXXXXXXX   ",// 4
                "   XX       XX  ",// 3
                "  XX   WWW  XX  ",// 2
                "  XX  WWWWW XX  ",// 1
                "  XX        XX  ",// 0
                " XXX  MMMMM  XX ",// 9
                " XXX  M   MM X  ",// 8
                " XXX  M    M X  ",// 7
                " XXX  M  8 M X  ",// 6
                " XXX  M    M X  ",// 5
                " XXX  MMMMMM XX ",// 4
                " XXX  MMM    X  ",// 3
                "  XX      XXXX  ",// 2
                "   XXXXXXXXX    ",// 1
                "                ",// 0
                //123456789012345
            },
            new []
            {
                //12345678901234
                "##############  ",// 6
                "############### ",// 5
                "################",// 4
                "################",// 3
                "################",// 2
                "################",// 1
                "################",// 0
                "################",// 9
                "################",// 8
                "################",// 7
                "################",// 6
                "################",// 5
                "################",// 4
                "################",// 3
                "############### ",// 2
                " #############  ",// 1
                "  ###########   ",// 0
                //12345678901234
            }
        };
        var voxels = ArrayToVoxels.Make(input, c => c == ' ' ? (char?) null : c);
        var objectId = ObjectId.FirstId;
        var solid = VoxelsToSolid.Triangulate(objectId, voxels);
        _vertices = new float[solid.Vertices.Count * 3];
        var vertexDataIndex = 0;
        foreach (var vertex in solid.Vertices)
        {
            _vertices[vertexDataIndex++] = (float)vertex.X;
            _vertices[vertexDataIndex++] = -(float)vertex.Z;
            _vertices[vertexDataIndex++] = (float)vertex.Y;
        }
        _indices = new uint[solid.Triangles.Count * 3];
        var indicesDataIndex = 0;
        foreach (var triangle in solid.Triangles)
        {
            _indices[indicesDataIndex++] = (uint)triangle.V1;
            _indices[indicesDataIndex++] = (uint)triangle.V2;
            _indices[indicesDataIndex++] = (uint)triangle.V3;
        }

        _vertexArrayObject = GL.GenVertexArray();
        GL.BindVertexArray(_vertexArrayObject);

        _vertexBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

        _elementBufferObject = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);

        _shader = new Shader("shader.vert", "shader.frag");
        _shader.Use();

        var vertexLocation = _shader.GetAttribLocation("aPosition");
        GL.EnableVertexAttribArray(vertexLocation);
        GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

    }

    protected override void OnUnload()
    {
        base.OnUnload();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        GL.DeleteBuffer(_elementBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vertexBufferObject);
        _shader?.Dispose();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        
        _time += e.Time;

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.BindVertexArray(_vertexArrayObject);

        var model =
            Matrix4.CreateTranslation(-8, -8, -1) *
            Matrix4.CreateScale(0.05f) *
            Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(20.0 * _time)) *
            Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(10.0 * _time));
        // Note that we're translating the scene in the reverse direction of where we want to move.
        var view = Matrix4.CreateTranslation(0.0f, 0.0f, -3.0f);
        // Not 100% sure if this is correct - e.g. does title bar mess it up?
        var aspectRatio = (float)ClientSize.X / ClientSize.Y;
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), aspectRatio, 0.1f, 100.0f);

        _shader!.Use();
        _shader!.SetMatrix4("model", ref model);
        _shader!.SetMatrix4("view", ref view);
        _shader!.SetMatrix4("projection", ref projection);

        GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedInt, 0);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }    
}
