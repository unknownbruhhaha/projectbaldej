using BaldejFramework.Render;
using BaldejFramework.Assets;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Quaternion = BEPUutilities.Quaternion;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework.UI
{
    public class TextLabel : UiElement, IDisposable
    {
        #region Public variables
        public Vector2 Size { get => _size; }
        public Vector2 Position { get => _position; }
        public float Rotation { get => _rotation; set => _rotation = value; }
        public AdaptationFlag adaptationMode { get => _adaptMode; set { _adaptMode = value; UpdateTexture(); } }
        public Vector2 BaseSize
        {
            get => _baseSize;
            set
            {
                _baseSize = value;
                if (_sizeAnimationEnd == null)
                    UpdateTexture();
            }
        }
        public int Layer { get => _layer; set { _layer = value; UpdateTexture(); } }
        public Vector2 BasePosition { get => _basePos; set { _basePos = value; } }
        public float BaseRotation { get; set; }
        public bool YCentred { get => _yCentred; set { _yCentred = value; UpdateTexture(); } }
        public string Text { get => _text; set { _text = value; UpdateTexture(); } }
        public int TextSize { get => _fontSize; set { _fontSize = value; UpdateTexture(); } }
        public Color4 Color { get => _color; set { _color = value; UpdateTexture(); } }
        public string FontPath { get => _fontPath; set { _fontPath = value; UpdateTexture(); } }
        public Action? OnClick { get; set; }
        public int BodyID { get => (int)_body.CollisionInformation.Tag; }
        public Box Body { get => _body; }
        #endregion

        #region Private variables
        private Vector2 _size;
        private Vector2 _position;
        private float _rotation;
        
        private AdaptationFlag _adaptMode;
        private Vector2 _baseSize;
        private Vector2 _basePos;
        private int _layer;
        private bool _yCentred;
        private string _text;
        private int _fontSize;
        private Color4 _color;
        private string _fontPath;

        private int _vbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private UiElement? Parent;
        private Shader shader = new Shader(@"Shaders\UI\panelTextureVertShader.shader", @"Shaders\UI\panelTextureFragShader.shader");
        private FontCollection collection;
        private FontFamily family;
        private Font font;
        private Texture texture;
        private float[] _verts = new float[30];
        private Box _body;
        private Vector2 _needTextureSize;
        private int _needFontSize;
        private Vector2? _positionAnimationEnd;
        private float _positionAnimationSpeedDivider = 8;
        private Vector2? _sizeAnimationEnd;
        private float _sizeAnimationSpeedDivider = 8;

        private bool _disposed;
        #endregion

        public TextLabel(string text, Vector2 size, Vector2 position, float rotation, Color4 color, int layer, int textSize = 256, bool yCentred = false, string Font = @"Fonts\StandardFont.ttf", UiElement? parent = null, AdaptationFlag AdaptationMode = AdaptationFlag.AdaptByX)
        {
            _text = text;
            _baseSize = size;
            _basePos = position;
            _adaptMode = AdaptationMode;
            Parent = parent;
            _color = color;
            _fontPath = Font;
            _fontSize = textSize;
            _layer = layer;
            _yCentred = yCentred;
            BaseRotation = rotation;
            texture = null;

            UpdateTexture();

            UiManager.elements.Add(this);
        }

        public void Render()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Setting parameters depending on parent

                float aspectRatio = (float)BaldejFramework.Render.Render.window.Size.X /
                                    (float)BaldejFramework.Render.Render.window.Size.Y;
                if (Parent != null)
                {
                    Console.WriteLine("SZ: " + Parent.Size.X / aspectRatio);
                    Console.WriteLine("SZ1: " + Size.X);
                    if (Parent.adaptationMode == AdaptationFlag.AdaptByX)
                        _size = new Vector2(Parent.Size.X / aspectRatio, Parent.Size.Y) * BaseSize;
                    else if (Parent.adaptationMode == AdaptationFlag.AdaptByY)
                        _size = new Vector2(Parent.Size.X, Parent.Size.Y / aspectRatio) * BaseSize;
                    else
                        _size = new Vector2(Parent.Size.X, Parent.Size.Y) * BaseSize;
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

                #region Binding VAO, VBO, setting texture

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts,
                    BufferUsageHint.DynamicDraw);

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                int texCoordLocation = shader.GetAttribLocation("vertTexCoord");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                    3 * sizeof(float));

                shader.SetInt("texture0", 1);

                shader.Use();
                texture.Use();

                #endregion

                #region Rigidbody transform setup

                _body.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Layer);
                _body.Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward,
                    Convert.ToSingle(Math.PI / 180) * Rotation);

                #endregion

                #region Setting model matrix

                Matrix4 modelMatrix = Matrix4.Identity;
                modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                shader.SetMatrix4("model", modelMatrix);

                #endregion

                #region Animate Position and Size

                Animate();

                #endregion

                #region Finally drawing

                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);

                #endregion
            }
        }

        public void Resize(Vector2i ScreenSize)
        {
            UpdateTexture();
        }

        public void Clicked(Vector2 raycastPosition)
        {
            OnClick?.Invoke();
        }

        void UpdateTexture()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Removing old rigidbody

                if (_body != null)
                    Physics.UISpace.Remove(_body);

                #endregion

                float aspectRatio = (float)BaldejFramework.Render.Render.window.Size.X /
                                    (float)BaldejFramework.Render.Render.window.Size.Y;
                if (Parent != null)
                {
                    if (Parent.adaptationMode == AdaptationFlag.AdaptByX)
                        _size = new Vector2(Parent.Size.X / aspectRatio, Parent.Size.Y) * BaseSize;
                    else if (Parent.adaptationMode == AdaptationFlag.AdaptByY)
                        _size = new Vector2(Parent.Size.X, Parent.Size.Y / aspectRatio) * BaseSize;
                    else
                        _size = new Vector2(Parent.Size.X, Parent.Size.Y) * BaseSize;
                    _position = Parent.Position + BasePosition;
                    _rotation = Parent.Rotation + BaseRotation;
                }
                else
                {
                    _size = BaseSize;
                    _position = BasePosition;
                    _rotation = BaseRotation;
                }

                if (texture != null) texture.DestroyGLTexture();

                FontCollection collection = new();
                family = collection.Add(AssetManager.GetAssetFullPath(FontPath));
                font = family.CreateFont(TextSize, FontStyle.Regular);

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
                    _needTextureSize = new Vector2(Size.X * 1000, Size.Y * 1000);
                }
                else if (adaptationMode == AdaptationFlag.AdaptByX)
                {
                    _verts = new float[30]
                    {
                        0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 1, 1,
                        0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 1, 0,
                        -0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 0, 0,
                        -0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 0, 1,
                        -0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 0, 0,
                        0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 1, 1
                    };
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X / aspectRatio, Size.Y, 0.001f);
                    _needTextureSize = new Vector2(Size.X * 1000, Size.Y * 1000);
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
                    _needTextureSize = new Vector2(Size.X * 1000, Size.Y * 1000);
                }

                texture = new Texture(Convert.ToInt32(_needTextureSize.X), Convert.ToInt32(_needTextureSize.Y));
                if (!YCentred)
                    texture.img.Mutate(x => x.DrawText(Text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(Color.R, Color.G, Color.B, Color.A)),
                        new PointF(0, 0)));
                else
                    texture.img.Mutate(x => x.DrawText(Text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(Color.R, Color.G, Color.B, Color.A)),
                        new PointF(0, texture.img.Height / 4)));
                texture.ToGLTexture();
                shader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
                shader.SetMatrix4("projection", UI.UiManager.UiMatrix);

                #region Setting body parameters

                _body.CollisionInformation.Tag = Physics.GenID();
                Physics.UISpace.Add(_body);

                #endregion
            }
        }

        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_vao);
            texture.DestroyGLTexture();
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