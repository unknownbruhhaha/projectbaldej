using OpenTK.Mathematics;

namespace BaldejFramework.UI
{
    public interface UiElement
    {
        public Vector2 Size { get; }
        public Vector2 Position { get; } 
        public float Rotation { get; }

        public int Layer { get; set; }

        public AdaptationFlag adaptationMode { get; set; }

        public Vector2 BaseSize { get; set; }
        public Vector2 BasePosition { get; set; }
        public float BaseRotation { get; set; }

        public Action? OnClick { get; set; }
        public int BodyID { get; }
        
        public void Render();

        public void Resize(Vector2i screenSize);

        public void Clicked(Vector2 raycastPosition);
    }
}
