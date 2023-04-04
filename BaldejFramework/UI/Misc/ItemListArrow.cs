using BaldejFramework.Render;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BaldejFramework.UI.Misc;

public class ItemListArrow
{
    public Vector2 Position { get => _position; set { _position = value; } }
    public Vector2 Size { get => _size; set { _size = value; Update(); } }
    public int Layer { get => _layer; set { _layer = value; Update(); } }
    public Color4 Color { get => _color; set { _color = value; Update(); } }
    public float Rotation { get; set; }
    public float BaseRotation;
    public bool IsVisible = true;
    
    Vector2 _size;
    Vector2 _position;
    Color4 _color;
    int _layer;
    int _vbo = GL.GenBuffer();
    int _vao = GL.GenVertexArray();
    Shader colorShader = new Shader(@"Shaders\UI\panelColorVertShader.shader", @"Shaders\UI\panelColorFragShader.shader");
    private float[] _verts = new float[9];
    
    public ItemListArrow(Vector2 size, Vector2 position, float rotation, int layer, Color4 color)
    {
        _size = size;
        _position = position;
        _layer = layer;
        _color = color;
        BaseRotation = rotation;
        
        Update();
    }
    
    public void Render()
    {
        if (IsVisible)
        {
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        
            Matrix4 modelMatrix = Matrix4.Identity;
            modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
            modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
            colorShader.SetMatrix4("model", modelMatrix);
            colorShader.Use();
            GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);
        }
    }

    private void Update()
    {
        float aspectRatio = (float)BaldejFramework.Render.Render.window.Size.X / (float)BaldejFramework.Render.Render.window.Size.Y;
        
        _verts = new float[]
        {
            _size.X / 2 / aspectRatio, -_size.Y / 2, _layer + 0.05f,
            -_size.X / aspectRatio / 2, -_size.Y / 2, _layer + 0.05f,
            0,_size.Y / 2, _layer + 0.05f
        };
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts, BufferUsageHint.DynamicDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        colorShader.SetFloat("r", Color.R);
        colorShader.SetFloat("g", Color.G);
        colorShader.SetFloat("b", Color.B);
        colorShader.SetFloat("a", Color.A);
        colorShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
        colorShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
    }
}