using OpenTK.Mathematics;

namespace BaldejFramework.Components
{
    public interface Transform : Component
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public string componentID { get { return  "Transform"; } }

        public GameObject? owner { get; set; }

        public abstract void OnUpdate();

        public abstract void OnRender();
    }
}
