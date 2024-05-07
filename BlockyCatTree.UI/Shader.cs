using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace BlockyCatTree.UI;

public sealed class Shader : IDisposable
{
    private bool _disposed;
    private readonly int _handle;
    private readonly Dictionary<string, int> _uniformLocations;

    public Shader(string vertexPath, string fragmentPath)
    {
        var vertexShaderSource = File.ReadAllText(vertexPath);
        var fragmentShaderSource = File.ReadAllText(fragmentPath);
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);
        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(vertexShader);
        // ReSharper disable once InlineOutVariableDeclaration
        int success;
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(vertexShader);
            throw new Exception($"failed to compile vertex shader from {vertexPath}: {infoLog}");
        }
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out success);
        if (success == 0)
        {
            var infoLog = GL.GetShaderInfoLog(fragmentShader);
            throw new Exception($"failed to compile fragment shader from {fragmentPath}: {infoLog}");
        }
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
        GL.LinkProgram(_handle);
        GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out success);
        if (success == 0)
        {
            var infoLog = GL.GetProgramInfoLog(_handle);
            throw new Exception($"failed to link shaders from {vertexPath} and {fragmentPath}: {infoLog}");
        }
        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);

        GL.GetProgram(_handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);
        _uniformLocations = new Dictionary<string, int>();
        for (var i = 0; i < numberOfUniforms; i++)
        {
            var key = GL.GetActiveUniform(_handle, i, out _, out _);
            var location = GL.GetUniformLocation(_handle, key);
            _uniformLocations.Add(key, location);
        }        
    }

    public void Use()
    {
        GL.UseProgram(_handle);
    }

    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(_handle, attribName);
    }
    
    public void SetMatrix4(string name, ref Matrix4 matrix)
    {
        Use();
        var location = _uniformLocations[name];
        GL.UniformMatrix4(location, true, ref matrix);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        GL.DeleteProgram(_handle);
    }
}
