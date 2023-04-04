using BaldejFramework.Render;
using BaldejFramework.Assets;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using BEPUphysics.Entities.Prefabs;
using Quaternion = BEPUutilities.Quaternion;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using System.Collections.ObjectModel;

namespace BaldejFramework.UI
{
    public class Panel : UiElement, IDisposable
    {
        #region Public variables
        public Vector2 Size
        {
            get => _size;
        }

        public Vector2 Position
        {
            get => _position;
        }
        

        public float Rotation
        {
            get => _rotation;
        }

        public AdaptationFlag adaptationMode
        {
            get => _adapt;
            set
            {
                _adapt = value;
                Update();
            }
        }

        public Vector2 BaseSize
        {
            get => _baseSize;
            set
            {
                _baseSize = value;
                if (_sizeAnimationEnd == null)
                    Update();
            }
        }

        public int Layer
        {
            get => _layer;
            set
            {
                _layer = value;
                Update();
            }
        }

        public float ItemSizeY
        {
            get => _itemSizeY;
            set { _itemSizeY = value; Update(); }
        }

        public Vector2 BasePosition
        {
            get => _basePosition;
            set => _basePosition = value;
        }

        public ObservableCollection<string> Items = new ObservableCollection<string>();

        public float BaseRotation { get; set; }

        public TextureAsset? Texture;
        public Color4 Color;
        public int BodyID { get => (int)_body.CollisionInformation.Tag; }
        #endregion

        #region Private variables
        private Vector2 _size;
        private Vector2 _position;
        private float _rotation;
        private float _itemSizeY;

        private AdaptationFlag _adapt;
        private Vector2 _baseSize;
        private Vector2 _basePosition;
        public Action? OnClick { get; set; }
        private int _layer;

        private int _vbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private UiElement? Parent;

        private Shader textureShader = new Shader(@"Shaders\UI\panelTextureVertShader.shader",
            @"Shaders\UI\panelTextureFragShader.shader");

        private Shader colorShader = new Shader(@"Shaders\UI\panelColorVertShader.shader",
            @"Shaders\UI\panelColorFragShader.shader");

        private float[] _verts = new float[30];
        private Box _body;
        private Vector2? _positionAnimationEnd;
        private float _positionAnimationSpeedDivider = 8;
        private Vector2? _sizeAnimationEnd;
        private float _sizeAnimationSpeedDivider = 8;
        private bool _disposed;
        #endregion

        public Panel(Vector2 size, Vector2 position, Color4 color, float rotation = 0, UiElement? parent = null,
            int layer = 0, TextureAsset? texture = null, AdaptationFlag AdaptationMode = AdaptationFlag.AdaptByX)
        {
            #region Setting parameters
            _baseSize = size;
            _basePosition = position;
            _adapt = AdaptationMode;
            Parent = parent;
            _layer = layer;
            BaseRotation = rotation;
            if (texture == null)
                Color = color;
            else
                Texture = texture;
            #endregion

            #region Updating
            Update();
            #endregion

            #region Adding to main elements list
            UiManager.elements.Add(this);
            #endregion
        }

        public void Render()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Setting Size, Position and Rotation depending on parent's parameters

                if (Parent != null)
                {
                    _size = Parent.Size * BaseSize;
                    _position = Parent.Position + BasePosition;
                    _rotation = Parent.Rotation + BaseRotation;
                }
                else
                {
                    _size = BaseSize;
                    _position = BasePosition;
                    _rotation = BaseRotation;
                }

                #endregion

                #region Rigidbody transform setup

                _body.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Layer);
                _body.Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward,
                    Convert.ToSingle(Math.PI / 180) * Rotation);

                #endregion

                #region Binding VAO and VBO

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

                #endregion

                #region Animating

                Animate();

                #endregion

                #region Setting up model matrix and shaders

                if (Texture != null)
                {
                    Texture.Use();
                    textureShader.SetInt("texture0", 1);
                    Matrix4 modelMatrix = Matrix4.Identity;
                    modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                    modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                    modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                    textureShader.SetMatrix4("model", modelMatrix);
                    textureShader.Use();
                }
                else
                {
                    Matrix4 modelMatrix = Matrix4.Identity;
                    modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                    modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                    modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                    colorShader.SetMatrix4("model", modelMatrix);
                    colorShader.Use();
                }

                #endregion

                #region Finally drawing our element

                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);

                #endregion
            }
        }

        public void Resize(Vector2i ScreenSize) // on resize 
        {
            Update();
        }

        public void Clicked(Vector2 raycastPosition) // on click 
        {
            OnClick?.Invoke();
        }

        void Update()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Removing old rigidbody
                if (_body != null)
                    Physics.UISpace.Remove(_body);
                #endregion

                #region Setting Size, Position and Rotation depending on parent's parameters
                if (Parent != null)
                {
                    _size = Parent.Size * BaseSize;
                    _position = Parent.Position + BasePosition;
                    _rotation = Parent.Rotation + BaseRotation;
                }
                else
                {
                    _size = BaseSize;
                    _position = BasePosition;
                    _rotation = BaseRotation;
                }
                #endregion

                #region Working with vertices and Rigidbody
                float aspectRatio = (float)BaldejFramework.Render.Render.window.Size.X /
                                    (float)BaldejFramework.Render.Render.window.Size.Y;

                if (adaptationMode == AdaptationFlag.AdaptByY)
                {
                    _verts = new float[30]
                    {
                        0.5f, 0.5f / aspectRatio, Layer * (float)0.001, 1, 1,
                        0.5f, -0.5f / aspectRatio, Layer * (float)0.001, 1, 0,
                        -0.5f, -0.5f / aspectRatio, Layer * (float)0.001, 0, 0,
                        -0.5f, 0.5f / aspectRatio, Layer * (float)0.001, 0, 1,
                        -0.5f, -0.5f / aspectRatio, Layer * (float)0.001, 0, 0,
                        0.5f, 0.5f / aspectRatio, Layer * (float)0.001, 1, 1
                    };
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X, Size.Y / aspectRatio, 0.001f);
                }
                else if (adaptationMode == AdaptationFlag.AdaptByX)
                {
                    _verts = new float[30]
                    {
                        (float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001, 1, 1,
                        (float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001, 1, 0,
                        -(float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001, 0, 0,
                        -(float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001, 0, 1,
                        -(float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001, 0, 0,
                        (float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001, 1, 1
                    };
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X / aspectRatio, Size.Y, 0.001f);
                }
                else if (adaptationMode == AdaptationFlag.NoAdaptation)
                {
                    _verts = new float[30]
                    {
                        0.5f, 0.5f, Layer * (float)0.001, 1, 1,
                        0.5f, -0.5f, Layer * (float)0.001, 1, 0,
                        -0.5f, -0.5f, Layer * (float)0.001, 0, 0,
                        -0.5f, 0.5f, Layer * (float)0.001, 0, 1,
                        -0.5f, -0.5f, Layer * (float)0.001, 0, 0,
                        0.5f, 0.5f, Layer * (float)0.001, 1, 1
                    };
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X, Size.Y, 0.001f);
                }
                #endregion

                #region Binding VAO and VBO and sending data
                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts, BufferUsageHint.DynamicDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                #endregion

                #region Setting shaders' parameters
                colorShader.SetFloat("r", Color.R);
                colorShader.SetFloat("g", Color.G);
                colorShader.SetFloat("b", Color.B);
                colorShader.SetFloat("a", Color.A);
                colorShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
                colorShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
                textureShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
                textureShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
                #endregion

                #region Setting body parameters
                _body.CollisionInformation.Tag = Physics.GenID();
                Physics.UISpace.Add(_body);
                #endregion
            }
        }

        public void Dispose() //on dispose 
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_vao);
            if (_body != null)
                Physics.UISpace.Remove(_body);
            _disposed = true;
        }

        public void RunAnimation(Vector2 positionAnimationEnd, Vector2 sizeAnimationEnd, float positionAnimationSpeedDivider = 8, float sizeAnimationSpeedDivider = 8)
        {
            _positionAnimationEnd = positionAnimationEnd;
            _positionAnimationSpeedDivider = positionAnimationSpeedDivider;
            _sizeAnimationEnd = sizeAnimationEnd;
            _sizeAnimationSpeedDivider = sizeAnimationSpeedDivider;
        }

        void Animate()
        {
            if (_positionAnimationEnd != null)
            {
                Vector2 animationEnd = (Vector2)_positionAnimationEnd;
                float positionX = animationEnd.X;
                float positionY = animationEnd.Y;

                if (BasePosition.X < animationEnd.X)
                {
                    Console.WriteLine("a: " + positionX);
                    positionX = Math.Clamp(BasePosition.X + Math.Abs(BasePosition.X - animationEnd.X) / _positionAnimationSpeedDivider, 0 - Size.X, animationEnd.X);
                }
                else if (BasePosition.X > animationEnd.X)
                {
                    positionX = Math.Clamp(BasePosition.X - Math.Abs(BasePosition.X - animationEnd.X) / _positionAnimationSpeedDivider, animationEnd.X, 1 + Size.Y);
                }

                if (BasePosition.Y < animationEnd.Y)
                {
                    positionY = Math.Clamp(BasePosition.Y + Math.Abs(BasePosition.Y - animationEnd.Y) / _positionAnimationSpeedDivider, 0 - Size.Y, animationEnd.Y);
                }
                else if (BasePosition.Y > animationEnd.Y)
                {
                    positionY = Math.Clamp(BasePosition.Y - Math.Abs(BasePosition.Y - animationEnd.Y) / _positionAnimationSpeedDivider, animationEnd.Y, 1 + Size.Y);
                }

                if (Math.Abs(positionY - animationEnd.Y) <= 0.001f)
                    positionY = animationEnd.Y;
                if (Math.Abs(positionX - animationEnd.X) <= 0.001f)
                    positionX = animationEnd.X;
                if (positionY == animationEnd.Y && positionX == animationEnd.X)
                    _positionAnimationEnd = null;

                BasePosition = new Vector2(positionX, positionY);
            }

            if (_sizeAnimationEnd != null)
            {
                Vector2 animationEnd = (Vector2)_sizeAnimationEnd;
                float sizeX = animationEnd.X;
                float sizeY = animationEnd.Y;

                if (BaseSize.X < animationEnd.X)
                {
                    Console.WriteLine("a: " + sizeX);
                    sizeX = Math.Clamp(BaseSize.X + Math.Abs(BaseSize.X - animationEnd.X) / _sizeAnimationSpeedDivider, 0, animationEnd.X);
                }
                else if (BaseSize.X > animationEnd.X)
                {
                    sizeX = Math.Clamp(BaseSize.X - Math.Abs(BaseSize.X - animationEnd.X) / _sizeAnimationSpeedDivider, 0, animationEnd.X);
                }

                if (BaseSize.Y < animationEnd.Y)
                {
                    sizeY = Math.Clamp(BaseSize.Y + Math.Abs(BaseSize.Y - animationEnd.Y) / _sizeAnimationSpeedDivider, 0, animationEnd.X);
                }
                else if (BaseSize.Y > animationEnd.Y)
                {
                    sizeY = Math.Clamp(BaseSize.Y - Math.Abs(BaseSize.Y - animationEnd.Y) / _sizeAnimationSpeedDivider, 0, animationEnd.X);
                }

                if (Math.Abs(sizeY - animationEnd.Y) <= 0.001f)
                    sizeY = animationEnd.Y;
                if (Math.Abs(sizeX - animationEnd.X) <= 0.001f)
                    sizeX = animationEnd.X;
                if (sizeY == animationEnd.Y && sizeX == animationEnd.X)
                {
                    _sizeAnimationEnd = null;
                }

                BaseSize = new Vector2(sizeX, sizeY);
            }
        }
    }
}