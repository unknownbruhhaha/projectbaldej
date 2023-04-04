using BaldejFramework.Render;
using BaldejFramework.Assets;
using BEPUphysics.Entities.Prefabs;
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
    public class Button : UiElement, IDisposable
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
            set => _rotation = value;
        }

        public AdaptationFlag adaptationMode
        {
            get => _adaptMode;
            set
            {
                _adaptMode = value;
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

        public Vector2 BasePosition
        {
            get => _basePos;
            set => _basePos = value;
        }

        public float BaseRotation { get; set; }

        public bool YCentred
        {
            get => _yCentred;
            set
            {
                _yCentred = value;
                Update();
            }
        }

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                Update();
            }
        }

        public int FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                Update();
            }
        }

        public Color4 BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                Update();
            }
        }

        public Color4 TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Update();
            }
        }

        public string FontPath
        {
            get => _fontPath;
            set
            {
                _fontPath = value;
                Update();
            }
        }

        public Action? OnClick { get; set; }

        public int BodyID
        {
            get => (int)_body.CollisionInformation.Tag;
        }

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
        private int _needFontSize;
        private Color4 _backgroundColor;
        private Color4 _textColor;
        private string _fontPath;

        private int _vbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private bool _isSelected = false;
        private UiElement? Parent;

        private Shader textureShader = new Shader(@"Shaders\UI\panelTextureVertShader.shader",
            @"Shaders\UI\panelTextureFragShader.shader");

        private float[] _verts = new float[30];
        private FontCollection collection;
        private FontFamily family;
        private Font font;
        private Texture texture;
        private Vector2 needTextureSize;

        private Box _body;
        private Vector2? _positionAnimationEnd;
        private float _positionAnimationSpeedDivider = 8;
        private Vector2? _sizeAnimationEnd;
        private float _sizeAnimationSpeedDivider = 8;

        private bool _disposed;

        #endregion

        public Button(string text, Vector2 size, Vector2 position, Color4 backgroundColor, Color4 textColor,
            float rotation = 0, UiElement? parent = null, int layer = 0, int fontSize = 72,
            string fontPath = @"Fonts\StandardFont.ttf", AdaptationFlag AdaptationMode = AdaptationFlag.AdaptByX)
        {
            #region Setting parameters

            _baseSize = size;
            _basePos = position;
            _adaptMode = AdaptationMode;
            Parent = parent;
            _layer = layer;
            _backgroundColor = backgroundColor;
            _textColor = textColor;
            _fontSize = fontSize;
            _fontPath = fontPath;
            _text = text;
            BaseRotation = rotation;

            #endregion

            #region Setting size and position depending on parent's transform

            if (Parent != null)
            {
                _size = Parent.Size * BaseSize;
                _position = Parent.Position + BasePosition;
            }
            else
            {
                _size = BaseSize;
                _position = BasePosition;
            }

            #endregion

            #region Setting shaders' matrices to make everything draw correctly

            textureShader.SetMatrix4("model", Matrix4.Identity);
            textureShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
            textureShader.SetMatrix4("projection", UI.UiManager.UiMatrix);

            #endregion

            #region Adding to main elements list

            UiManager.elements.Add(this);

            #endregion
        }

        public void Render()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
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

                #region Applying animation

                Animate();

                #endregion

                #region Rigidbody transform setup

                _body.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Layer);
                _body.Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward,
                    Convert.ToSingle(Math.PI / 180) * Rotation);

                #endregion

                Matrix4 modelMatrix = Matrix4.Identity;
                modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);

                #region Drawing

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                texture.Use();
                textureShader.SetMatrix4("model", modelMatrix);
                textureShader.SetInt("texture0", 1);
                textureShader.Use();
                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);

                #endregion
            }
        }

        public void Clicked(Vector2 raycastPosition)
        {
            OnClick?.Invoke();
        }

        public void Resize(Vector2i ScreenSize)
        {
            Update();
        }

        void Update()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Removing old rigidbody

                if (_body != null)
                    Physics.UISpace.Remove(_body);

                #endregion

                #region Working with vertices

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
                    needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
                    _needFontSize = Convert.ToInt32(_fontSize / aspectRatio);
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
                    _needFontSize = Convert.ToInt32(_fontSize / aspectRatio);
                    needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
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
                    _needFontSize = _fontSize;
                    needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X, Size.Y, 0.001f);
                }

                #endregion

                #region Working with texture

                if (texture != null) texture.DestroyGLTexture();

                FontCollection collection = new();
                family = collection.Add(AssetManager.GetAssetFullPath(FontPath));
                font = family.CreateFont(_fontSize, FontStyle.Regular);

                texture = new Texture(Convert.ToInt32(needTextureSize.X), Convert.ToInt32(needTextureSize.Y));

                float threshold = 0.01F;
                Color sourceColor = SixLabors.ImageSharp.Color.Transparent;
                Color targetColor = new Color(new System.Numerics.Vector4(BackgroundColor.R, BackgroundColor.G,
                    BackgroundColor.B, BackgroundColor.A));
                var brush = new RecolorBrush(sourceColor, targetColor, threshold);
                texture.img.Mutate(x => x.Fill(brush));

                if (!YCentred)
                    texture.img.Mutate(x => x.DrawText(_text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(TextColor.R, TextColor.G,
                            TextColor.B,
                            TextColor.A)), new PointF(0, 0)));
                else
                    texture.img.Mutate(x => x.DrawText(_text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(TextColor.R, TextColor.G,
                            TextColor.B,
                            TextColor.A)), new PointF(0, texture.img.Height / 4)));
                texture.ToGLTexture();

                #endregion

                #region Binding VAO and VBO

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts,
                    BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);
                int texCoordLocation = textureShader.GetAttribLocation("vertTexCoord");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float),
                    3 * sizeof(float));

                #endregion

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

        public void RunAnimation(Vector2 positionAnimationEnd, Vector2 sizeAnimationEnd, float positionAnimationSpeedDivider = 8, float sizeAnimationSpeedDivider = 8)
        {
            _positionAnimationEnd = positionAnimationEnd;
            _positionAnimationSpeedDivider = positionAnimationSpeedDivider;
            _sizeAnimationEnd = sizeAnimationEnd;
            _sizeAnimationSpeedDivider = sizeAnimationSpeedDivider;
        }
    }
}