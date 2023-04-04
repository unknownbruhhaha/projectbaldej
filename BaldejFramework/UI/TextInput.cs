using BaldejFramework.Render;
using BaldejFramework.Assets;
using BEPUphysics.Entities.Prefabs;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Quaternion = BEPUutilities.Quaternion;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace BaldejFramework.UI
{
    public class TextInput : UiElement, IDisposable
    {
        #region Public variables
        public Vector2 Size { get => _size; }
        public Vector2 Position { get => _position; }
        public float Rotation { get => _rotation; set => _rotation = value; }
        
        public AdaptationFlag adaptationMode { get => _adaptMode; set { _adaptMode = value; Update(); } }
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
        public int Layer { get => _layer; set { _layer = value; Update(); } }
        public Vector2 BasePosition { get => _basePos; set { _basePos = value; } }
        public float BaseRotation { get; set; }
        public bool YCentred { get => _yCentred; set { _yCentred = value; Update(); } }
        public string Text { get => _text; set { _text = value; Update(); } }
        public int FontSize { get => _fontSize; set { _fontSize = value; Update(); } }
        public Color4 BackgroundColor { get => _backgroundColor; set { _backgroundColor = value; Update(); } }
        public Color4 TextColor { get => _textColor; set { _textColor = value; Update(); } }
        public string FontPath { get => _fontPath; set { _fontPath = value; Update(); } }
        public Action? OnClick { get; set; }
        public Action? OnApply { get; set; }
        public Action? OnTextChanged { get; set; }
        public Box Body { get => _body; }
        public int BodyID { get => (int)_body.CollisionInformation.Tag; }
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
        private string _text = "";
        private int _fontSize;
        private int _needFontSize;
        private Color4 _backgroundColor;
        private Color4 _textColor;
        private string _fontPath;
        private float _zCoord;

        private int _vbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private int _vbo01 = GL.GenBuffer();
        private int _vao01 = GL.GenVertexArray();
        private bool _isSelected = false;
        private UiElement? Parent;
        private Shader textureShader = new Shader(@"Shaders\UI\panelTextureVertShader.shader", @"Shaders\UI\panelTextureFragShader.shader");
        private Shader colorShader = new Shader(@"Shaders\UI\panelColorVertShader.shader", @"Shaders\UI\panelColorFragShader.shader");
        private float[] _verts = new float[30];
        private float[] _verts01 = new float[30];
        private FontCollection collection;
        private FontFamily family;
        private Font font;
        private Texture texture;
        private Vector2 _needTextureSize;
        
        private Box _body;
        private Vector2? _positionAnimationEnd;
        private float _positionAnimationSpeedDivider = 8;
        private Vector2? _sizeAnimationEnd;
        private float _sizeAnimationSpeedDivider = 8;

        private bool _disposed;
        #endregion

        public TextInput(Vector2 size, Vector2 position, Color4 backgroundColor, Color4 textColor, float rotation, UiElement? parent = null, int layer = 0, int fontSize = 72, string fontPath = @"Fonts\StandardFont.ttf", AdaptationFlag AdaptationMode = AdaptationFlag.AdaptByX)
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
            colorShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
            colorShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
            textureShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
            textureShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
            #endregion
            
            #region Update
            Update();
            #endregion
            
            #region Adding to main elements list
            UiManager.elements.Add(this);
            #endregion

            #region Setting events(inputs)
            BaldejFramework.Render.Render.window.TextInput += OnTextInput;
            BaldejFramework.Render.Render.window.KeyDown += OnKeyDown;
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

                #region Rigidbody transform setup

                _body.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Layer);
                _body.Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward,
                    Convert.ToSingle(Math.PI / 180) * Rotation);

                #endregion

                #region Animate

                Animate();

                #endregion

                #region Drawing panel

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

                Matrix4 modelMatrix = Matrix4.Identity;
                modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                colorShader.SetMatrix4("model", modelMatrix);
                colorShader.Use();
                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);

                #endregion

                #region Drawing text

                GL.BindVertexArray(_vao01);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo01);
                texture.Use();
                textureShader.SetInt("texture0", 1);
                textureShader.SetMatrix4("model", modelMatrix);
                textureShader.Use();
                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);

                #endregion

                #region Check if selected

                if (_isSelected == false && UiManager.SelectedElement == this)
                {
                    _isSelected = true;
                    Update();
                }
                else if (_isSelected == true && UiManager.SelectedElement != this)
                {
                    _isSelected = false;
                    Update();
                }

                #endregion
            }
        }

        // if special keys like backspace or enter pressed
        private void OnKeyDown(KeyboardKeyEventArgs obj)
        {
            if (obj.Key == Keys.Backspace)
            {
                if (obj.Command)
                {
                    Text = "";
                    if (OnTextChanged != null)
                    {
                        OnTextChanged.Invoke();    
                    }
                    return;
                }

                if (Text.Length > 0)
                {
                    Text = Text.Remove(Text.Length - 1);
                    if (OnTextChanged != null)
                    {
                        OnTextChanged.Invoke();
                    }
                }
            }

            if (obj.Key == Keys.Enter)
            {
                if (OnApply != null)
                {
                    OnApply.Invoke();    
                }
            }
        }
        
        // if user is entering text
        private void OnTextInput(TextInputEventArgs obj)
        {
            if (UiManager.SelectedElement == this)
            {
                Text += Char.ConvertFromUtf32(obj.Unicode);
                if (OnTextChanged != null)
                {
                    OnTextChanged.Invoke();    
                }
            }
        }
        
        public void Clicked(Vector2 raycastPosition)
        {
            OnClick?.Invoke();
            Update();
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
                    _verts01 = new float[30]
                    {
                        0.5f, 0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 1, 1,
                        0.5f, -0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 1, 0,
                        -0.5f, -0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 0, 0,
                        -0.5f, 0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 0, 1,
                        -0.5f, -0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 0, 0,
                        0.5f, 0.5f / aspectRatio, Layer * (float)0.001 + 0.0001f, 1, 1
                    };
                    _needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
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
                    _verts01 = new float[30]
                    {
                        (float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001 + 0.0001f, 1, 1,
                        (float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001 + 0.0001f, 1, 0,
                        -(float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001 + 0.0001f, 0, 0,
                        -(float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001 + 0.0001f, 0, 1,
                        -(float)0.5f / aspectRatio, -(float)0.5f, Layer * (float)0.001 + 0.0001f, 0, 0,
                        (float)0.5f / aspectRatio, (float)0.5f, Layer * (float)0.001 + 0.0001f, 1, 1
                    };
                    _needFontSize = Convert.ToInt32(_fontSize / aspectRatio);
                    _needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
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
                    _verts01 = new float[30]
                    {
                        0.5f, 0.5f, Layer * (float)0.001 + 0.0001f, 1, 1,
                        0.5f, -0.5f, Layer * (float)0.001 + 0.0001f, 1, 0,
                        -0.5f, -0.5f, Layer * (float)0.001 + 0.0001f, 0, 0,
                        -0.5f, 0.5f, Layer * (float)0.001 + 0.0001f, 0, 1,
                        -0.5f, -0.5f, Layer * (float)0.001 + 0.0001f, 0, 0,
                        0.5f, 0.5f, Layer * (float)0.001 + 0.0001f, 1, 1
                    };
                    _needFontSize = _fontSize;
                    _needTextureSize = new Vector2(Size.X * 1500, Size.Y * 1500);
                    _body = new Box(BEPUutilities.Vector3.Zero, Size.X, Size.Y, 0.001f);
                }

                #endregion

                #region Binding VAO and VBO for panel

                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                GL.BufferData(BufferTarget.ArrayBuffer, _verts.Length * sizeof(float), _verts,
                    BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
                GL.EnableVertexAttribArray(0);

                #endregion

                #region Setting shader for panel

                colorShader.SetFloat("r", BackgroundColor.R);
                colorShader.SetFloat("g", BackgroundColor.G);
                colorShader.SetFloat("b", BackgroundColor.B);
                colorShader.SetFloat("a", BackgroundColor.A);

                #endregion

                #region Working with texture

                if (texture != null) texture.DestroyGLTexture();
                FontCollection collection = new();
                family = collection.Add(AssetManager.GetAssetFullPath(FontPath));
                font = family.CreateFont(_fontSize, FontStyle.Regular);

                texture = new Texture(Convert.ToInt32(_needTextureSize.X), Convert.ToInt32(_needTextureSize.Y));
                string text = GetNeedTextWithOffset();
                if (!YCentred)
                    texture.img.Mutate(x => x.DrawText(text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(TextColor.R, TextColor.G,
                            TextColor.B, TextColor.A)), new PointF(0, 0)));
                else
                    texture.img.Mutate(x => x.DrawText(text, font,
                        new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(TextColor.R, TextColor.G,
                            TextColor.B, TextColor.A)), new PointF(0, texture.img.Height / 4)));
                if (_isSelected)
                    texture.img.Mutate(x =>
                        x.DrawLines(
                            new SixLabors.ImageSharp.Color(new System.Numerics.Vector4(TextColor.R, TextColor.G,
                                TextColor.B, TextColor.A)), 15,
                            new PointF[]
                            {
                                new PointF(TextMeasurer.Measure(text, new TextOptions(font)).Width + 10, 15),
                                new PointF(TextMeasurer.Measure(text, new TextOptions(font)).Width + 10,
                                    Size.Y * 1500 - 15)
                            }));
                texture.ToGLTexture();

                #endregion

                #region Binding VAO and VBO for text

                GL.BindVertexArray(_vao01);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo01);
                GL.BufferData(BufferTarget.ArrayBuffer, _verts01.Length * sizeof(float), _verts01,
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

        string GetNeedTextWithOffset()
        {
            TextOptions rendererOptions = new TextOptions(font);
            string txt = _text;
            if (TextMeasurer.Measure(txt, rendererOptions).Width > _size.X * 1500)
            {
                for (int i = 0; i < _text.Length; i++)
                {
                    txt = txt.Remove(0,1);
                    FontRectangle fr = TextMeasurer.Measure(txt, rendererOptions);
                    if (fr.Width <= _size.X * 1500)
                    {
                        break;
                    }
                }       
            }

            return txt;
        }
        
        public void Dispose()
        {
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_vao);
            texture.DestroyGLTexture();
            if (_body != null)
                Physics.UISpace.Remove(_body);
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
