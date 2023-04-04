using BaldejFramework.Render;
using System.Collections.ObjectModel;
using BEPUphysics.Entities.Prefabs;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using BaldejFramework.Assets;
using SixLabors.ImageSharp;
using OpenTK.Windowing.Common;
using Quaternion = BEPUutilities.Quaternion;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;
using BEPUphysics;
using BEPUutilities;

namespace BaldejFramework.UI
{
    public class ItemList : UiElement, IDisposable
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

        public float ItemSizeY
        {
            get => _itemSizeY;
            set { _itemSizeY = value; Update(); }
        }

        public ObservableCollection<string> Items = new ObservableCollection<string>();

        public int TextSize { get => _textSize; set { _textSize = value; Update(); } }
        public Color4 TextColor { get => _textcolor; set { _textcolor = value; Update(); } }
        public string FontPath { get => _fontPath; set { _fontPath = value; Update(); } }

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

        public int BorderThickness
        {
            get => _borderThickness;
            set
            {
                _borderThickness = value;
                Update();
            }
        }

        public int XOffset
        {
            get => _XOffset;
            set
            {
                _XOffset = value;
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
            get => _basePosition;
            set => _basePosition = value;
        }

        public int ElementBorder
        {
            get => _elementBorder;
            set
            {
                _elementBorder = value;
                Update();
            }
        }

        public float BaseRotation { get; set; }

        public Color4 Color;
        public int BodyID { get => (int)_body.CollisionInformation.Tag; }
        public Action? OnClick { get; set; }
        public Action<int>? OnItemClick { get; set; }
        #endregion

        #region Private variables
        private Vector2 _size;
        private Vector2 _position;
        private float _rotation;
        private float _itemSizeY;
        private int _borderThickness;
        private int _XOffset;
        private int _elementBorder;
        private Texture? _texture;
        private int _textureHeight;
        private int _textureAllElementsHeight;
        private AdaptationFlag _adapt;
        private Vector2 _baseSize;
        private Vector2 _basePosition;

        private int _layer;

        private int _vbo = GL.GenBuffer();
        private int _vao = GL.GenVertexArray();
        private UiElement? Parent;

        private Shader textureShader = new Shader(@"Shaders\UI\panelTextureVertShader.shader",
            @"Shaders\UI\panelTextureFragShader.shader");

        private float[] _verts = new float[30];
        private Box _body;
        private string _fontPath;
        private Color4 _textcolor;
        private int _textSize;
        private FontFamily family;
        private Font font;
        private float _oldTextureScrollOffset;
        private float _scrollOffset;
        private float _textureScrollOffset;

        private Space _itemlistSpace = new Space();
        private List<Box> _itemsColliders = new List<Box>();

        private bool _disposed = false;
        private double _currentTime;

        private Vector2? _positionAnimationEnd;
        private Vector2? _sizeAnimationEnd;
        private float _positionAnimationSpeedDivider = 8;
        private float _sizeAnimationSpeedDivider = 8;
        #endregion

        public ItemList(Vector2 size, float itemYSize, Vector2 position, ObservableCollection<string> items, Color4 backgroundColor, Color4 textColor, int borderThickness = 0, int XOffset = 10, int textSize = 56, int elementBorder = 2, string fontPath = @"Fonts\StandardFont.ttf", float rotation = 0, UiElement? parent = null,
            int layer = 0, AdaptationFlag AdaptationMode = AdaptationFlag.AdaptByX)
        {
            #region Setting parameters
            _baseSize = size;
            _basePosition = position;
            _adapt = AdaptationMode;
            Parent = parent;
            _layer = layer;
            Color = backgroundColor;
            BaseRotation = rotation;
            _itemSizeY = itemYSize;
            _textcolor = textColor;
            _fontPath = fontPath;
            _textSize = textSize;
            _borderThickness = borderThickness;
            _XOffset = XOffset;
            _elementBorder = elementBorder;
            Items = items;
            #endregion

            #region Setting events
            Items.CollectionChanged += (object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
            {
                Update();
            };
            BaldejFramework.Render.Render.window.MouseWheel += (MouseWheelEventArgs args) =>
            {
                _textureScrollOffset = Math.Clamp(_textureScrollOffset + args.OffsetY * _itemSizeY / 4 * Size.Y * 750, 0, _textureAllElementsHeight - _textureHeight + borderThickness + elementBorder / 2);
                _scrollOffset = _textureScrollOffset / 750;
            };
            #endregion

            #region Updating
            _texture = new Texture(750, 750);
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
                _body.Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward, Convert.ToSingle(Math.PI / 180) * Rotation);
                _body.OrientationMatrix = Matrix3x3.CreateScale(Size.X, Size.Y, 1);
                _body.Position = new BEPUutilities.Vector3(Position.X, Position.Y, Layer);

                #endregion

                #region Binding VAO and VBO
                GL.BindVertexArray(_vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
                #endregion

                #region Applying animation
                Animate();
                #endregion

                #region Setting up model matrix and shaders
                if (_texture != null)
                {
                    _texture.Use();
                    textureShader.SetInt("texture0", 1);
                    Matrix4 modelMatrix = Matrix4.Identity;
                    modelMatrix *= Matrix4.CreateScale(Size.X, Size.Y, 1);
                    modelMatrix *= Matrix4.CreateRotationZ(Convert.ToSingle(Math.PI / 180) * Rotation);
                    modelMatrix *= Matrix4.CreateTranslation(Position.X, Position.Y, 0);
                    textureShader.SetMatrix4("model", modelMatrix);
                    int texCoordLocation = textureShader.GetAttribLocation("vertTexCoord");
                    GL.EnableVertexAttribArray(texCoordLocation);
                    GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
                    textureShader.Use();
                }
                #endregion

                #region Finally drawing our element
                GL.DrawArrays(PrimitiveType.Triangles, 0, _verts.Length / 3);
                #endregion

                #region Update texture if scrolled
                if (_oldTextureScrollOffset != _textureScrollOffset)
                {
                    UpdateTexture();
                    _oldTextureScrollOffset = _textureScrollOffset;
                }
                #endregion
            }
        }

        public void Resize(Vector2i ScreenSize) // on resize 
        {
            if (!_disposed)
                Update();
        }

        public void Clicked(Vector2 raycastPosition) // on click 
        {
            if (!_disposed)
            {
                if (Items.Count > 0)
                {
                    _itemsColliders[0].Position = new BEPUutilities.Vector3(_position.X, _position.Y - _size.Y / 2 + (_size.Y * _itemSizeY) / 2 - _scrollOffset, Layer);
                    _itemsColliders[0].Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward, Convert.ToSingle(Math.PI / 180) * Rotation);
                    for (int i = 1; i < _itemsColliders.Count; i++)
                    {
                        _itemsColliders[i].Position = new BEPUutilities.Vector3(_position.X, _itemsColliders[i - 1].Position.Y + (_size.Y * _itemSizeY), Layer);
                        _itemsColliders[i].Orientation = Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Backward, Convert.ToSingle(Math.PI / 180) * Rotation);
                    }
                    _itemlistSpace.Update();
                    BEPUutilities.Vector3 rayFrom = new BEPUutilities.Vector3(raycastPosition.X, raycastPosition.Y, Layer + 1);
                    BEPUutilities.Vector3 rayTo = new BEPUutilities.Vector3(0, 0, Layer - 1);
                    _itemlistSpace.RayCast(new Ray(rayFrom, rayTo), out var result);
                    Console.WriteLine("raycast pos = " + raycastPosition);
                    if (result.HitObject != null)
                    {
                        Console.WriteLine(result.HitObject.Tag);
                        OnItemClick?.Invoke(Convert.ToInt32(result.HitObject.Tag));
                    }
                    else
                        Console.WriteLine("no hit");
                }
                OnClick?.Invoke();
            }
        }

        void Update()
        {
            if (!_disposed && !BaldejFramework.Render.Render.WindowMinimized)
            {
                #region Removing old rigidbody
                for (int i = 0; i < _itemsColliders.Count; i++)
                    _itemlistSpace.Remove(_itemsColliders[i]);
                _itemsColliders = new List<Box>();
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
                    0.5f, -0.5f/ aspectRatio, Layer * (float)0.001, 1, 0,
                    -0.5f, -0.5f/ aspectRatio, Layer * (float)0.001, 0, 0,
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
                    0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 1, 1,
                    0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 1, 0,
                    -0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 0, 0,
                    -0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 0, 1,
                    -0.5f / aspectRatio, -0.5f, Layer * (float)0.001, 0, 0,
                    0.5f / aspectRatio, 0.5f, Layer * (float)0.001, 1, 1
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

                #region Setting shader's parameters
                textureShader.SetMatrix4("view", Matrix4.LookAt(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0)));
                textureShader.SetMatrix4("projection", UI.UiManager.UiMatrix);
                #endregion

                #region Creating bodies
                for (int i = 0; i < Items.Count; i++)
                {
                    _itemsColliders.Add(new Box(BEPUutilities.Vector3.Zero, Size.X, Size.Y * (ItemSizeY / 2) * 2, 0.1f));
                    _itemlistSpace.Add(_itemsColliders[i]);
                    _itemsColliders[i].CollisionInformation.Tag = i;
                }
                #endregion

                #region Creating texture
                UpdateTexture();
                #endregion

                #region Setting body parameters
                _body.CollisionInformation.Tag = Physics.GenID();
                Physics.UISpace.Add(_body);
                #endregion
            }
        }
        
        void UpdateTexture() 
        {
            if (!_disposed)
            {
                if (_texture != null)
                {
                    #region Creating image in texture
                    _texture.img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(Convert.ToInt32(Size.X * 750), Convert.ToInt32(Size.Y * 750));
                    #endregion

                    #region Setting _textureHeight and _textureAllElementsHeight
                    _textureHeight = Convert.ToInt32(750 * Size.Y);
                    _textureAllElementsHeight = Convert.ToInt32(750 * Size.Y * ItemSizeY * Items.Count);
                    #endregion

                    #region Working on fonts
                    FontCollection collection = new();
                    family = collection.Add(AssetManager.GetAssetFullPath(FontPath));
                    font = family.CreateFont(_textSize, FontStyle.Regular);
                    #endregion

                    #region Background color
                    float threshold = 0.01F;
                    Color sourceColor = SixLabors.ImageSharp.Color.Transparent;
                    Color targetColor = new Color(new System.Numerics.Vector4(Color.R, Color.G, Color.B, Color.A));
                    var brush = new RecolorBrush(sourceColor, targetColor, threshold);
                    _texture.img.Mutate(x => x.Fill(brush));
                    #endregion

                    #region Drawing text
                    for (int i = 0; i < Items.Count; i++)
                    {
                        _texture.img.Mutate(x => x.DrawText(Items[i], font, new Color(new System.Numerics.Vector4(TextColor.R, TextColor.G, TextColor.B, TextColor.A)), new PointF(_XOffset, 750 * Size.Y * ItemSizeY * i + BorderThickness - _textureScrollOffset)));

                        if (BorderThickness > 0)
                        {
                            Pen borderPen = new Pen(new Color(new System.Numerics.Vector4(TextColor.R, TextColor.G, TextColor.B, TextColor.A)), BorderThickness);
                            _texture.img.Mutate(x => x.DrawLines(borderPen, new PointF[2] { new PointF(_XOffset, 750 * Size.Y * ItemSizeY * i + TextSize + BorderThickness / 2 - _textureScrollOffset), new PointF(750 * Size.X - _XOffset, 750 * Size.Y * ItemSizeY * i + TextSize + BorderThickness / 2 - _textureScrollOffset) }));
                        }
                    }
                    #endregion

                    #region Drawing element's border
                    if (ElementBorder > 0)
                    {
                        Pen borderPen = new Pen(new Color(new System.Numerics.Vector4(TextColor.R, TextColor.G, TextColor.B, TextColor.A)), ElementBorder);
                        _texture.img.Mutate(x => x.DrawLines(borderPen, new PointF[]
                        { new PointF(ElementBorder / 2, 750 * Size.Y - ElementBorder / 2), new PointF(750 * Size.X, 750 * Size.Y - ElementBorder / 2) }));
                        _texture.img.Mutate(x => x.DrawLines(borderPen, new PointF[]
                        { new PointF(ElementBorder / 2, ElementBorder / 2), new PointF(ElementBorder / 2, 750 * Size.Y - ElementBorder / 2) }));

                        _texture.img.Mutate(x => x.DrawLines(borderPen, new PointF[]
                        { new PointF(750 * Size.X - ElementBorder, ElementBorder / 2), new PointF(0 - ElementBorder, ElementBorder / 2) }));
                        _texture.img.Mutate(x => x.DrawLines(borderPen, new PointF[]
                        { new PointF(750 * Size.X - ElementBorder, ElementBorder / 2), new PointF(750 * Size.X - ElementBorder, 750 * Size.Y - ElementBorder / 2) }));
                    }
                    #endregion

                    #region Updating OpenGL Texture
                    _texture.UpdateTexture();
                    #endregion
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose");
            Console.Beep();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);
            GL.DeleteBuffer(_vbo);
            GL.DeleteVertexArray(_vao);
            GL.DeleteProgram(textureShader.Handle);
            _texture.DestroyGLTexture();
            if (_body != null)
            {
                Physics.UISpace.Remove(_body);
                for (int i = 0; i < _itemsColliders.Count; i++)
                    _itemlistSpace.Remove(_itemsColliders[i]);
                _itemsColliders.Clear();
            }
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
