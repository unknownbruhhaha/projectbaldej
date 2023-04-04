using BaldejFramework.Assets;
using BaldejFramework.Render;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace BaldejFramework.Components
{
    public class Mesh : Component, IDisposable
    {
        #region Public Variables
        public string componentID => "Mesh";
        public GameObject? owner { get; set; }

        public ObjMeshAsset Asset
        {
            get => _asset;
            set
            {
                _asset = value;
                Update();
            }
        }
        public int CurrentAnimationFrame
        {
            get => _currentAnimationFrame;
            set
            {
                _currentAnimationFrame = value;
                Update();
            }
        }

        public int AnimationEndFrame;
        public int AnimationStartFrame;

        public bool LoopAnimation = false;
        public Shader shader { get; set; }
        public TextureAsset? Texture;
        public Action? OnAnimationEnd;
        public int DisableDistance = -1;
        #endregion

        #region Private Variables
        private ObjMeshAsset _asset;
        private int _currentAnimationFrame = 0;
        private int _vbo = GL.GenBuffer();
        private int _uvbo = GL.GenBuffer();
        private int _nbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private bool _shouldDraw = false;

        private float[] _verts { get => _asset.Frames[CurrentAnimationFrame].Vertices; }
        private float[] _uvs { get => _asset.Frames[CurrentAnimationFrame].UVs; }
        private float[] _normals { get => _asset.Frames[CurrentAnimationFrame].Normals; }
        #endregion

        public Mesh(ObjMeshAsset meshAsset, TextureAsset? texture, string vertShaderPath = @"shaders\vert.shader", string fragShaderPath = @"shaders\frag.shader")
        {
            shader = new Shader(vertShaderPath, fragShaderPath);
            _asset = meshAsset;
            Texture = texture;

            Update();
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_nbo);
            GL.DeleteBuffer(_uvbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteShader(shader.Handle);
            //Console.WriteLine("mesh disposed!");
        }

        public void OnRender()
        {
            if (_shouldDraw)
            {
                GL.BindVertexArray(_vao);

                if (Texture != null)
                {
                    Texture?.Use();
                    shader.SetInt("texture0", 2);
                }

                shader.SetVector3("lightColor", Render.Render.SunColor);
                shader.SetVector3("lightDirection", Render.Render.SunDirection);
                shader.Use();

                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);
            }
        }

        public void OnUpdate()
        {
            if (CurrentAnimationFrame + 1 <= AnimationEndFrame)
                CurrentAnimationFrame++;
            else if (CurrentAnimationFrame + 1 > AnimationEndFrame && LoopAnimation)
                CurrentAnimationFrame = AnimationStartFrame;
            else if (CurrentAnimationFrame + 1 > AnimationEndFrame && !LoopAnimation)
                OnAnimationEnd?.Invoke();

            Transform transformComponent = (Transform)owner.GetComponent("Transform");

            if (DisableDistance == -1)
            {
                _shouldDraw = true;
                SetupMatrix(transformComponent);
                return;
            }
            
            if (Math.Abs(transformComponent.Position.X - Render.Render.camera.Position.X)
                + Math.Abs(transformComponent.Position.Y - Render.Render.camera.Position.Y)
                + Math.Abs(transformComponent.Position.Z - Render.Render.camera.Position.Z) < DisableDistance)
            {
                _shouldDraw = true;
                SetupMatrix(transformComponent);
            }
            else
            {
                _shouldDraw = false;
            }
        }

        private void Update()
        {
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _uvbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _uvs.Length * sizeof(float), _uvs, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _nbo);
            GL.BufferData(BufferTarget.ArrayBuffer, _normals.Length * sizeof(float), _normals, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(2);
        }

        private void SetupMatrix(Transform transformComponent)
        {
            Matrix4 transform = Matrix4.Identity;
            transform = transform * Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.Z));
            transform = transform * Matrix4.CreateRotationY(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.Y));
            transform = transform * Matrix4.CreateRotationX(Convert.ToSingle(Math.PI / 180 * transformComponent.Rotation.X));
            transform = transform * Matrix4.CreateScale(transformComponent.Scale.X, transformComponent.Scale.Y, transformComponent.Scale.Z);
            transform = transform * Matrix4.CreateTranslation(transformComponent.Position.X, transformComponent.Position.Y, transformComponent.Position.Z);
            
            shader.SetMatrix4("model", transform);
            shader.SetMatrix4("view", Render.Render.camera.GetViewMatrix());
            shader.SetMatrix4("projection", Render.Render.camera.GetProjectionMatrix());
        }
    }
}
